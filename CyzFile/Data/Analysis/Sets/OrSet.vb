Imports System.Xml
Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Serializing

Namespace Data.Analysis

    ''' <summary>
    ''' This set is an OR combination of a list of sets.
    ''' The combined sets can only combine 2 sets, but allows you to choose
    ''' between Union/Intersect/Except.
    ''' This or set is used to allow you to put lists on both sides of the
    ''' combined set operation.  This set ONLY allows the Union operation, not all
    ''' the other, as such it is somewhat limited.
    ''' 
    ''' We use a special set for this instead of adding list support to the combinedSet,
    ''' to make it easier to support this consistently in the GUI.  To keep the interface
    ''' the way it is, allow expanding collapsing/selection, etc.  We need some kind
    ''' of class to represent this or set. So I created one.  The big difference with
    ''' this set is that the user can not create this set himself, Also, he cannot
    ''' select it.
    ''' 
    ''' It will a be a bit of a strange set, since the user is limited, he cannot
    ''' name it, cannot select it, etc.  It is sort of a limited use set.  But
    ''' we (probably) need it to handle some GUI stuff, that will be very tricky
    ''' to do when do it the other way.
    ''' 
    ''' In the combine set dialog, when the user selects a single set, just that
    ''' set is added, when he selects multiple, then this set is wrapped around the list.
    ''' It is basically a copy of the CombinedSet, but uses a list instead of 2 sets,
    ''' and it only has one operation.
    ''' We may end up with more trouble then the combination 2 lists, but lets try.
    ''' Maybe it would be nice to create a combined set base class and then
    ''' subclass combined set and or set from that (See composite pattern)
    ''' but that would create backwards compatibility issues.
    ''' </summary>
    <Serializable()> Public Class OrSet
        Inherits CompositeSet

        ''' <summary>
        ''' Needed by XML (de)serializer code, DO NOT USE 
        ''' </summary>
        Public Sub New()
            MyBase.New("", cytoSetType.OrSet, Drawing.Color.Gold)
        End Sub

        Public Sub New(sets As List(Of CytoSet), autoSet As Boolean)
            Me.New(sets, Nothing, autoSet)
        End Sub

        Public Sub New(name As String, myColor As Drawing.Color, sets As List(Of CytoSet), dfw As CytoSense.Data.DataFileWrapper, autoSet As Boolean)
            MyBase.New(name, cytoSetType.OrSet, myColor, dfw) ' Use fully transparent color ?!? Unfortunately there is no INVALID color

            _setList = New List(Of CytoSet)(sets)
            _autoSet = autoSet

            If _datafile IsNot Nothing Then
                RecalculateParticleIndices()
            End If
        End Sub

        Public Sub New(name As String, myColor As Drawing.Color, sets As List(Of CytoSet), dfw As CytoSense.Data.DataFileWrapper, autoSet As Boolean, listId As Integer, vis As Boolean)
            MyBase.New(name, cytoSetType.OrSet, myColor, dfw, listId, vis) ' Use fully transparent color ?!? Unfortunately there is no INVALID color

            _setList = New List(Of CytoSet)(sets)
            _autoSet = autoSet

            If _datafile IsNot Nothing Then
                RecalculateParticleIndices()
            End If
        End Sub

        ''' <summary>
        ''' Create a deep copy, cloning all subsets as well.
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New(other As OrSet)
            MyBase.New(other.Name, cytoSetType.OrSet, other.ColorOfSet, other.Datafile, other.ListID, other.Visible) ' Use fully transparent color ?!? Unfortunately there is no INVALID color

            _setList = other.SetList.Select(Function(s) s.Clone()).ToList()
            _autoSet = other._autoSet

            If _datafile IsNot Nothing Then
                RecalculateParticleIndices()
            End If
        End Sub

        Public Sub New(name As String, myColor As Drawing.Color, sets As List(Of CytoSet), autoSet As Boolean)
            Me.New(name, myColor, sets, Nothing, autoSet)
        End Sub

        Public Sub New(sets As List(Of CytoSet), dfw As CytoSense.Data.DataFileWrapper, autoSet As Boolean)
            Me.New(GenerateName(sets), Drawing.Color.FromArgb(0, 0, 0, 0), sets, dfw, autoSet) ' Use fully transparent color ?!? Unfortunately there is no INVALID color
        End Sub

        Public Overrides Property Datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get

            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value
                _setList.ForEach(Sub(s) s.Datafile = value)
                RecalculateParticleIndices()
            End Set
        End Property

        Public Sub UpdateSets(sets As List(Of CytoSet))
            _setList = sets
        End Sub

        ' Automatically added sets are not displayed at the top level.
        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return _autoSet
            End Get
        End Property

        Public Overrides Property Invalid As Boolean
            Get
                For Each setEntry In _setList
                    If setEntry.Invalid Then
                        Return True
                    End If
                Next

                Return False
            End Get

            Set(value As Boolean)
                _Invalid = value ' not used
            End Set
        End Property


        Public Overrides Sub RecalculateParticleIndices()
            Dim indices As IEnumerable(Of Integer) = New List(Of Integer)()

            For Each s In _setList
                indices = indices.Union(s.ParticleIndices)
            Next

            _ParticleIndices = indices.Distinct().ToArray()

            Array.Sort(_ParticleIndices)
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Dim res As Boolean = False

            For Each s In _setList
                res = res Or s.TestSingleParticle(p)
            Next

            Return res
        End Function

        Public Sub subSetIndexesChanged()
            RecalculateParticleIndices()
        End Sub

        Public Overrides Function Clone() As CytoSet
            Return New OrSet(Me)
        End Function

        Public Shared Function GenerateName(sets As List(Of CytoSet)) As String
            Return "(" + String.Join(" | ", sets.OrderBy(Function(s) s.ListID).Select(Function(s) s.Name)) + ")"
        End Function

        Public ReadOnly Property SetList As List(Of CytoSet)
            Get
                Return _setList
            End Get
        End Property

        Public Overrides ReadOnly Property ChildSets As List(Of CytoSet)
            Get
                Return _setList
            End Get
        End Property

        Public ReadOnly Property AutoSet As Boolean
            Get
                Return _autoSet
            End Get
        End Property

        Private _setList As List(Of CytoSet)
        Private _autoSet As Boolean

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            document.AppendChildElement(parentNode, "AutoSet", _autoSet)

            Dim setListNode As XmlElement = document.AppendChildElement(parentNode, "SetList")

            For Each cytoSet In _setList
                document.AppendChildElement(setListNode, "ListID", cytoSet.ListID)
            Next
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)

            _setList = New List(Of CytoSet)
            _autoSet = parentNode.ReadChildElementAsBoolean("AutoSet")

            Dim setListNode As XmlElement = parentNode.Item("SetList")

            For Each childNode As XmlElement In setListNode.SelectNodes("ListID")
                _setList.Add(New CytoSetDummy(CInt(childNode.InnerText)))
            Next
        End Sub
    End Class
End Namespace
