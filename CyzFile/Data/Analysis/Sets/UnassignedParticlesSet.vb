Imports System.Xml
Imports CytoSense.Data.ParticleHandling

Namespace Data.Analysis

    ''' <summary>
    ''' A set that contains only particles that are not yet used in another class
    ''' The using classes are responsible for keeping the allSets property up-to-date. If this is the case the recalculateIDs will do the work.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class UnassignedParticlesSet
        Inherits CytoSet

        Private _allsets As SetsList
        <NonSerialized()> Private _particles As ParticleHandling.Particle()

        ''' <summary>
        ''' Needed by XML (de)serializer code, DO NOT USE 
        ''' </summary>
        Public Sub New()
            MyBase.New("", cytoSetType.unassignedParticles, Drawing.Color.Gold)
        End Sub

        Public Sub New(setsList As SetsList, myColor As Drawing.Color)
            MyBase.New("Unassigned Particles", cytoSetType.unassignedParticles, myColor)
            _allsets = setsList
        End Sub

        Public Sub New(setsList As SetsList, myColor As Drawing.Color, datafile As DataFileWrapper)
            MyBase.New("Unassigned Particles", cytoSetType.unassignedParticles, myColor, datafile)
            _allsets = setsList
        End Sub

        Public Sub New(setsList As SetsList, myColor As Drawing.Color, datafile As DataFileWrapper, listId As Integer, vis As Boolean)
            MyBase.New("Unassigned Particles", cytoSetType.unassignedParticles, myColor, datafile, listId, vis)
            _allsets = setsList
        End Sub

        Public Sub New(other As UnassignedParticlesSet)
            MyBase.New(other.Name, other.Type, other.ColorOfSet, other.Datafile, other.ListID, other.Visible)
            _allsets = other._allsets
        End Sub

        Public Overrides Sub RecalculateParticleIndices()
            Dim usedParticleIndices As New List(Of Integer)
            Dim allParticleIndices As Integer() = {}

            'Remark: if the particle ids happen to be sorted (I'm not sure), this process can be heavily optimized!
            For i = 0 To _allsets.Count - 1

                If Not TypeOf _allsets(i) Is DefaultSet AndAlso
                    Not TypeOf _allsets(i) Is UnassignedParticlesSet AndAlso
                    Not TypeOf _allsets(i) Is AllImagesSet Then
                    usedParticleIndices = usedParticleIndices.Union(_allsets(i).ParticleIndices).ToList
                ElseIf TypeOf _allsets(i) Is DefaultSet Then
                    'Use the defaultset to get all indices:
                    allParticleIndices = _allsets(i).ParticleIndices
                End If
            Next
            _ParticleIndices = (allParticleIndices.Except(usedParticleIndices).ToArray)
            _particles = {}

            _Invalid = True ' always because comparing to the previous indices is very expensive
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            For i As Integer = 0 To _allsets.Count - 1
                If Not TypeOf _allsets(i) Is DefaultSet AndAlso Not TypeOf _allsets(i) Is UnassignedParticlesSet AndAlso Not TypeOf _allsets(i) Is AllImagesSet Then
                    If _allsets(i).TestSingleParticle(p) Then
                        Return False
                    End If
                End If
            Next
            Return True
        End Function

        Public WriteOnly Property AllSets() As SetsList
            Set(value As SetsList)
                _allsets = value
            End Set
        End Property

        Public Overrides Function Clone() As CytoSet
            Return New UnassignedParticlesSet(Me)
        End Function

        Public Overrides Property Datafile As DataFileWrapper
            Get
                Return _datafile
            End Get
            Set(value As DataFileWrapper)
                _datafile = value
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)
        End Sub

    End Class
End Namespace
