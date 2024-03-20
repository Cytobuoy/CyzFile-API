Imports System.Drawing
Imports CytoSense.Data.ParticleHandling

Namespace Data.Analysis

    ''' <summary>
    ''' An index based set contains little more than a list of particles to include. Mostly useful for importing exported results from other programs.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class indexBasedSet
        Inherits CytoSet

        Private _particles As ParticleHandling.Particle()

        Public Class indexesDoNotFitDatafileException
            Inherits Exception
            Public Sub New(ByRef thisSet As indexBasedSet)
                MyBase.New(String.Format("The indexes defined in set {0} do not fit this datafile.", thisSet.Name))
            End Sub
        End Class

        Public Sub New(ByVal name As String, ByVal color As System.Drawing.Color, ByVal IDs As Integer(), ByVal datafile As CytoSense.Data.DataFileWrapper)
            MyBase.New(name, cytoSetType.indexBased, color, datafile)
            Throw New NotImplementedException()
        End Sub

        Public Overrides Property datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get

            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Function Clone() As CytoSet
            Dim myclone As New indexBasedSet(Me.Name, Me.colorOfSet, Me.ParticleIDs, Me.datafile)
            Return myclone
        End Function

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Sub RecalculateParticleIndices()
            Throw New NotImplementedException()
        End Sub
    End Class

    <Serializable()> Public Class DefaultSet
        ' This one always exists and contains all particles
        Inherits CytoSet

        Private _curMachine As String

        Public Const defaultSetName As String = "Default (all)"

        ''' <summary>
        ''' Needed by XML de-serializer code
        ''' </summary>
        Public Sub New()
            MyBase.New(defaultSetName, cytoSetType.DefaultAll, Color.Black)
        End Sub

        Public Sub New(color As Color)
            MyBase.New(defaultSetName, cytoSetType.DefaultAll, color)
            ListID = 0
        End Sub

        Public Sub New(ByVal datafile As CytoSense.Data.DataFileWrapper, color As Color, listId As Integer, vis As Boolean)
            MyBase.New(defaultSetName, cytoSetType.DefaultAll, color, datafile, listId, vis)
            listId = 0
            If datafile IsNot Nothing Then
                'RecalculateIDs()
                RecalculateParticleIndices()
            End If
        End Sub

        Public Sub New(other As DefaultSet)
            MyBase.New(other.Name, other.type, other.colorOfSet, other.datafile, other.ListID, other.Visible)
            ListID = 0
            If datafile IsNot Nothing Then
                RecalculateParticleIndices()
            End If
        End Sub

        Public Overrides Sub RecalculateParticleIndices()
            If Not Invalid OrElse _datafile Is Nothing Then
                Return
            End If

            Debug.WriteLine("DefaultSet.RecalculateParticleIndices")

            ReDim _ParticleIndices(_datafile.SplittedParticles.Length - 1)

            For i As Integer = 0 To _datafile.SplittedParticles.Length - 1
                _ParticleIndices(i) = i
            Next
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Return True
        End Function

        Public ReadOnly Overrides Property UseInIIF As Boolean
            Get
                Return False
            End Get
        End Property
        Public Overrides Property datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get

            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value
                RecalculateParticleIndices()
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Function Clone() As CytoSet
            Return New DefaultSet(Me)
        End Function
    End Class
End Namespace