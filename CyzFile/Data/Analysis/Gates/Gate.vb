Imports System.Xml
Imports CytoSense.CytoSettings
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public Enum GateType
        Invalid = 0
        Range
        Rectangle
        Polygon
        FreeForm
    End Enum

    ''' <summary>
    ''' Interface definition for a gate.  Not sure this will add much, so I may decide to remove it 
    ''' later on.  For now it helps me focus.
    ''' </summary>
    ''' <remarks></remarks>
    Public Interface IGate
        ReadOnly Property Type() As GateType
        ReadOnly Property ParticleIndices As Integer()

        ''' <summary>
        ''' A list of the outline coordinates (in data coordinates), forming a path that is used for plotting.
        ''' </summary>
        ReadOnly Property Outline() As List(Of Drawing.PointF)

        ''' <summary>
        ''' A list of the outline coordinates (in data coordinates), forming a path that is used for plotting.
        ''' The xAxis and yAxis parameter specify the order of the x and Y that will be used for 
        ''' drawing, if needed the x and y values will be swapped before returning to the caller.
        ''' For a 1D gate, one of the gates is a don't care, and the other must be the one that is actually
        ''' used.
        ''' </summary>
        Function GetOutline(Axis1 As Axis, yAxis2 As Axis) As List(Of Drawing.PointF)

        Sub RecalculateParticleIndices()

        ''' <summary>
        ''' Tests a single particle against the gate
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function TestSingleParticle(p As CytoSense.Data.ParticleHandling.Particle) As Boolean

        ''' <summary>
        ''' Returns whether the gate has BOTH of these axes, either in x and y or y and x. For  a 1D gate,
        ''' the functions returns true if one of the axis matches, the other one is a don't care.
        ''' This makes it easier to test if a gate should be drawn inside a specific plot.
        ''' </summary>
        Function HasAxis(Axis1 As Axis, yAxis2 As Axis) As Boolean

        ''' <summary>
        ''' Returns whether the gate has this axis (and possibly other ones as well)
        ''' </summary>
        Function HasAxis(thisAxis As Axis) As Boolean

        ''' <summary>
        ''' Returns true if the gate is on the same axis as this one. Used to look for
        ''' gates to replace when moving gate definitions.
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns></returns>
        Function HasAxis(other As IGate) As Boolean

        ''' <summary>
        ''' Create a DEEP copy of the gate object. Note the indexes property is not copied, this will need to
        ''' be recalculated again.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Function Clone() As IGate

        ''' <summary>
        ''' Create a DEEP copy of the gate object and also set the datafile/cytosettings of this copy
        ''' </summary>
        ''' <param name="datafile"></param>
        ''' <returns></returns>
        Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate

        ''' <summary>
        ''' Call this after loading the gates from XML, it will update the CytoSettings object in all the Axis
        ''' that are used by this gate.
        ''' </summary>
        ''' <param name="settings"></param>
        Sub UpdateCytoSettings(settings As CytoSenseSetting)


        ''' <summary>
        ''' If a gate is new or has changes its Invalid property should return True so the owner of the
        ''' gate knows it needs to call recalculateIDs.
        ''' This property should ONLY be set to false inside workfile.RecalculateSetIDs
        ''' </summary>
        ''' <returns></returns>
        Property Invalid As Boolean

        ReadOnly Property Axes As Axis()
    End Interface

    <Serializable()>
    Public MustInherit Class Gate
        Implements IXmlDocumentIO
        Implements IGate

        <NonSerialized>
        Protected _datafile As DataFileWrapper = Nothing

        Public Property DataFile As DataFileWrapper
            Get
                Return _datafile
            End Get
            Set(value As DataFileWrapper)
                _datafile = value
                LoadAxisValues()
            End Set
        End Property

        Protected Sub New()
            _Invalid = True
        End Sub

#Region "IGate Implementation"
        Public Overridable ReadOnly Property Type() As GateType Implements IGate.Type
            Get
                Return GateType.Invalid
            End Get
        End Property

        Public MustOverride Sub LoadAxisValues()

        Public MustOverride Sub RecalculateParticleIndices() Implements IGate.RecalculateParticleIndices

        Public MustOverride Function TestSingleParticle(p As CytoSense.Data.ParticleHandling.Particle) As Boolean Implements IGate.TestSingleParticle

        Public ReadOnly Property ParticleIndices As Integer() Implements IGate.ParticleIndices
            Get
                Return _ParticleIndices
            End Get
        End Property

        Public MustOverride Function HasAxis(Axis1 As Axis, Axis2 As Axis) As Boolean Implements IGate.HasAxis

        Public MustOverride Function HasAxis(thisAxis As Axis) As Boolean Implements IGate.HasAxis

        Public MustOverride Function HasAxis(other As IGate) As Boolean Implements IGate.HasAxis

        Public MustOverride Function Clone() As IGate Implements IGate.Clone

        Public MustOverride Sub     UpdateCytoSettings(settings As CytoSenseSetting) Implements IGate.UpdateCytoSettings


        ''' <summary>
        ''' RVDH This SHOULD have been a constructor, but this is made impossible because a gateList only contains IGate references
        ''' Changing the gateList entries to descendants of Gate will probably break workspace serialization, so just to be safe
        ''' we use this method.
        ''' </summary>
        ''' <param name="datafile">should not be null</param>
        ''' <returns></returns>
        Public MustOverride Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate Implements IGate.CreateWorkfileCopy

        Public MustOverride ReadOnly Property Outline As List(Of Drawing.PointF) Implements IGate.Outline

        Public Overridable Property Invalid As Boolean Implements IGate.Invalid
            Get
                Return _Invalid
            End Get
            Set(value As Boolean)
                _Invalid = value
            End Set
        End Property

        Public MustOverride ReadOnly Property Axes As Axis() Implements IGate.Axes

        Public MustOverride Function GetOutline(Axis1 As Axis, yAxis2 As Axis) As List(Of Drawing.PointF) Implements IGate.GetOutline
#End Region

        Public Overridable Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
        End Sub

        Public Overridable Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
        End Sub

#Region "Private Data"

        <NonSerialized()> Protected _Invalid As Boolean = True
        <NonSerialized()> Protected _ParticleIndices As Integer()
#End Region
    End Class
End Namespace