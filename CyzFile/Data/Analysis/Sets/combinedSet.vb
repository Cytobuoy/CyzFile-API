
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Xml
Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()> Public Class combinedSet
        Inherits CompositeSet
        Implements ISerializable

        ''' <summary>
        ''' Needed by XML (de)serializer code, DO NOT USE
        ''' </summary>
        Public Sub New()
            MyBase.New("", cytoSetType.combined, Drawing.Color.Gold)

            _set1 = Nothing
            _set2 = Nothing
            _myGateCombi = SetsCombinationType.set1AND2
        End Sub

        Public Sub New(ByVal name As String, ByVal myColor As Drawing.Color, ByRef set1 As CytoSet, ByRef set2 As CytoSet, ByVal combiType As SetsCombinationType)
            MyBase.New(name, cytoSetType.combined, myColor)

            _set1 = set1
            _set2 = set2
            _myGateCombi = combiType
        End Sub

        Public Sub New(ByVal name As String, ByVal myColor As Drawing.Color, ByRef set1 As CytoSet, ByRef set2 As CytoSet, ByVal combiType As SetsCombinationType, ByVal datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean)
            MyBase.New(name, cytoSetType.combined, myColor, datafile, listId, vis)

            _set1 = set1
            _set2 = set2
            _myGateCombi = combiType
        End Sub

        Public Overrides Property Datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get
            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _set1.Datafile = value
                _set2.Datafile = value
                _datafile = value

                RecalculateParticleIndices()
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Private Shared EMPTY_IDS() As Integer = New Integer() {}

        Public Overrides Property Invalid As Boolean
            Get
                If _set1 IsNot Nothing AndAlso _set2 IsNot Nothing Then
                    _Invalid = _set1.Invalid Or _set2.Invalid
                Else
                    _Invalid = False
                End If

                Return _Invalid
            End Get

            Set(value As Boolean)
                _Invalid = value ' meaningless of course....
            End Set
        End Property

        Public Overrides Sub RecalculateParticleIndices()
            ' Always recalculate

            Dim set1Indices = If(_set1 IsNot Nothing, _set1.ParticleIndices, EMPTY_IDS)
            Dim set2Indices = If(_set2 IsNot Nothing, _set2.ParticleIndices, EMPTY_IDS)

            Select Case _myGateCombi
                Case SetsCombinationType.set1AND2
                    _ParticleIndices = set1Indices.Intersect(set2Indices).ToArray
                Case SetsCombinationType.set1OR2
                    _ParticleIndices = set1Indices.Union(set2Indices).Distinct.ToArray
                Case SetsCombinationType.set1NOT2
                    _ParticleIndices = set1Indices.Except(set2Indices).ToArray
                Case Else
                    'Shouldn't happen but evades compiler warning
                    Throw New NotImplementedException("This sets combination type does not exist")
            End Select

            Array.Sort(_ParticleIndices)
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Select Case _myGateCombi
                Case SetsCombinationType.set1AND2
                    Return _set1.TestSingleParticle(p) And _set2.TestSingleParticle(p)
                Case SetsCombinationType.set1OR2
                    Return _set1.TestSingleParticle(p) Or _set2.TestSingleParticle(p)
                Case SetsCombinationType.set1NOT2
                    Return _set1.TestSingleParticle(p) And Not _set2.TestSingleParticle(p)
                Case Else
                    'Shouldn't happen but evades compiler warning
                    Throw New NotImplementedException("This sets combination type does not exist")
            End Select
        End Function

        Public Overrides Function Clone() As CytoSet
            If _datafile Is Nothing Then
                Return New combinedSet(Name, ColorOfSet, _set1, _set2, _myGateCombi)
            Else
                Return New combinedSet(Name, ColorOfSet, _set1, _set2, _myGateCombi, _datafile, ListID, Visible)
            End If

        End Function

#Region "Subset info"

        Public Overrides ReadOnly Property ChildSets As List(Of CytoSet)
            Get
                Dim l As New List(Of CytoSet)()
                l.Add(_set1)
                l.Add(_set2)
                Return l
            End Get
        End Property

        Private _myGateCombi As SetsCombinationType

        Public ReadOnly Property GateCombination() As SetsCombinationType
            Get
                Return _myGateCombi
            End Get
        End Property

        Private _set1 As CytoSet
        Private _set2 As CytoSet

        Public ReadOnly Property Set1 As CytoSet
            Get
                Return _set1
            End Get
        End Property

        Public ReadOnly Property Set2 As CytoSet
            Get
                Return _set2
            End Get
        End Property

        Public Sub UpdateSubSets(set1 As CytoSet, set2 As CytoSet)
            _set1 = set1
            _set2 = set2
        End Sub

#End Region

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            document.AppendChildElement(parentNode, "Set1ListID", _set1.ListID.ToString())
            document.AppendChildElement(parentNode, "Set2ListID", _set2.ListID.ToString())
            document.AppendChildElement(parentNode, "CombinationType", _myGateCombi.ToString())
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)

            _set1 = New CytoSetDummy(parentNode.ReadChildElementAsInteger("Set1ListID"))
            _set2 = New CytoSetDummy(parentNode.ReadChildElementAsInteger("Set2ListID"))
            _myGateCombi = parentNode.ReadChildElementAsEnum(Of SetsCombinationType)("CombinationType")
        End Sub


        ''' <summary>
        ''' ISerializable Constructor
        ''' 
        ''' We had to implement ISerializable because the Binary Serializer could not
        ''' de-serialize pre v 4.7 workspaces with combinedSets. 
        ''' 
        ''' The pre v4.7 versions use _set members that were specified with WithEvents. 
        ''' The newer versions do not specify WithEvents for the _set members. 
        ''' </summary>
        ''' <param name="info"></param>
        ''' <param name="context"></param>
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New("dummy", cytoSetType.combined, Drawing.Color.Azure)

            ' Default serialization adds name of baseclass(es) to the variable names..

            _name = info.GetString("CytoSet+_name")
            _myType = DirectCast(info.GetValue("CytoSet+_myType", GetType(cytoSetType)), cytoSetType)
            _listID = info.GetInt32("CytoSet+_listID")
            _myGateCombi = CType(info.GetValue("_myGateCombi", GetType(SetsCombinationType)),SetsCombinationType)
            _visible = info.GetBoolean("CytoSet+_visible")
            _myColor = CType(info.GetValue("CytoSet+_myColor", GetType(System.Drawing.Color)),System.Drawing.Color)

            ' Old workspaces have an extra underscore added to _set (because of WithEvents?)...

            Dim memberNames As String() = CType(info.GetType().GetProperty("MemberNames", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static).GetValue(info), String())

            If memberNames.Contains("__set1") Then
                _set1 = DirectCast(info.GetValue("__set1", GetType(CytoSet)), CytoSet)
                _set2 = DirectCast(info.GetValue("__set2", GetType(CytoSet)), CytoSet)
            Else
                _set1 = DirectCast(info.GetValue("_set1", GetType(CytoSet)), CytoSet)
                _set2 = DirectCast(info.GetValue("_set2", GetType(CytoSet)), CytoSet)
            End If
        End Sub

        Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
            info.AddValue("CytoSet+_name", _name)
            info.AddValue("CytoSet+_myType", _myType, GetType(cytoSetType))
            info.AddValue("CytoSet+_listID", _listID)
            info.AddValue("_myGateCombi", _myGateCombi, GetType(SetsCombinationType))
            info.AddValue("CytoSet+_visible", _visible)
            info.AddValue("CytoSet+_myColor", _myColor, GetType(System.Drawing.Color))

            info.AddValue("_set1", _set1, GetType(CytoSet))
            info.AddValue("_set2", _set2, GetType(CytoSet))
        End Sub
    End Class

    <Serializable()> Public Enum SetsCombinationType
        Invalid = 0
        ''' <summary>
        ''' All particles that are in set 1 OR set 2. Basically the sum of the two.
        ''' </summary>
        set1OR2
        ''' <summary>
        ''' All particles that are in set 1 AND set 2. Basically only the particles that are in both sets simultaneously.
        ''' </summary>
        set1AND2
        ''' <summary>
        ''' All particles that are in set 1 and NOT in set 2. Basically set1 - set2.
        ''' </summary>
        set1NOT2
    End Enum
End Namespace
