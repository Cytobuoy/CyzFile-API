Imports CytoSense.Data.Analysis

Namespace PlotSettings


    ''' <summary>
    ''' For now only the dotplot definition is here, probably more should be added later on. 
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class HistPlotDefinition
        Private _name As String        
        Private _nBins As Integer = 128
        Private _logScale As Boolean = False
        Private _threeSigmaRange As Boolean = False 'If true, range is three sigma to either side of mu, otherwise just min and max datapoints
        Private _cumulativeMode As Boolean = False
        Private _overlayNormalDist As Boolean = False

        <NonSerialized> Public Event settingsChanged()

        ''' <summary>
        ''' General constructor
        ''' </summary>
        Public Sub New(ByVal name As String, ByVal ax As Axis)
            _name = name
            Axis = ax
        End Sub

        ''' <summary>
        ''' Constructor without name, the name is constructed from the axis titles.
        ''' </summary>
        Public Sub New(ByVal ax As Axis)
             _axis = ax
            _name = String.Format("{0}", _axis.Name)
        End Sub

        ''' <summary>
        ''' Copy Constructor
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New( other As HistPlotDefinition )
            _name              = other.Name
            _nBins             = other._nBins
            _logScale          = other._logScale
            _threeSigmaRange   = other._threeSigmaRange
            _cumulativeMode    = other._cumulativeMode
            _overlayNormalDist = other._overlayNormalDist
            Axis               = other.Axis
        End Sub

        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        Public Property Axis As Axis


        Public ReadOnly Property CytoSettings() As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _Axis.CytoSettings
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _name
        End Function


        Public Property LogScale As Boolean
            Get
                Return _logScale
            End Get
            Set(ByVal value As Boolean)
                _logScale = value
                RaiseEvent settingsChanged()
            End Set
        End Property

        ''' <summary>
        ''' If true, the range of the histogram extends three standard deviations to either side. Otherwise, the full data range is used.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ThreeSigmaRange As Boolean
            Get
                Return _threeSigmaRange
            End Get
            Set(ByVal value As Boolean)
                _threeSigmaRange = value
                RaiseEvent settingsChanged()
            End Set
        End Property

        Public Property OverlayNormalDistribution() As Boolean
            Get
                Return _overlayNormalDist
            End Get
            Set(ByVal value As Boolean)
                _overlayNormalDist = value
                RaiseEvent settingsChanged()
            End Set
        End Property

        Public Property NumberOfBins As Integer
            Get
                Return _nBins
            End Get
            Set(ByVal value As Integer)
                _nBins = Math.Abs(Math.Min(value, 10000))
                RaiseEvent settingsChanged()
            End Set
        End Property

        Property CumulativeMode() As Boolean
            Get
                Return _cumulativeMode
            End Get
            Set(ByVal value As Boolean)
                _cumulativeMode = value
                RaiseEvent settingsChanged()
            End Set
        End Property
    End Class

    ''' <summary>
    ''' For now only the dotplot definition is here, probably more should be added later on. 
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class DotPlotDefinition
        Private _name As String
        Private _xAxis As Axis
        Private _yAxis As Axis

        ''' <summary>
        ''' General constructor
        ''' </summary>
        Public Sub New(ByVal name As String, ByVal xAxis As Axis, ByVal yAxis As Axis)
            _name = name
            _xAxis = xAxis
            _yAxis = yAxis
        End Sub

        ''' <summary>
        ''' Constructor without name, the name is constructed from the axis titles.
        ''' </summary>
        Public Sub New(ByVal xAxis As Axis, ByVal yAxis As Axis)
            _xAxis = xAxis
            _yAxis = yAxis
            _name = String.Format("{0} - {1}", xAxis.Name, yAxis.Name)

        End Sub

        Public Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        Public ReadOnly Property xAxis() As Axis
            Get
                Return _xAxis
            End Get
        End Property

        Public ReadOnly Property yAxis() As Axis
            Get
                Return _yAxis
            End Get
        End Property

        Public ReadOnly Property CytoSettings() As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _xAxis.CytoSettings
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _name
        End Function
    End Class


    ''' <summary>
    ''' Stores the definition of a 3D plot.
    ''' Since all Plot3D stuff is in a seperate app, and used by the CytoUsb as well,
    ''' we cannot use this 3D definition class in the plot itsselfd.  So
    ''' we need to use some trickery
    ''' </summary>
    ''' <remarks>Basicly the same as a 2D plot, except that it conains 1 extra axis.
    ''' Object is obsolete, we no longer support 3D plots, but let it here so we can
    ''' actually load old definitions without crashing.</remarks>
    <Obsolete>
    <Serializable()> _
    Public Class DotPlot3DDefinition
        Inherits DotPlotDefinition
        Private _zAxis As Axis

        ''' <summary>
        ''' General constructor
        ''' </summary>
        Public Sub New(name As String, xAxis As Axis, yAxis As Axis, zAxis As Axis, guid As Integer)
            MyBase.New(name, xAxis, yAxis)
            _guid = guid
            _zAxis = zAxis
        End Sub

        ''' <summary>
        ''' Constructor without name, the name is constructed from the axis titles.
        ''' </summary>
        Public Sub New(xAxis As Axis, yAxis As Axis, zAxis As Axis, guid As Integer)
            MyBase.New(String.Format("{0} - {1} - {2}", xAxis.Name, yAxis.Name, zAxis), xAxis, yAxis)
            _guid = guid
            _zAxis = zAxis
        End Sub

        Public ReadOnly Property GUID As Integer
        Public ReadOnly Property zAxis() As Axis
            Get
                Return _zAxis
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Custom comparer for the DotplotDefinition class
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DotplotDefComparer
        Implements IEqualityComparer(Of DotPlotDefinition)

        Public Function Equals1(
            ByVal def1 As DotPlotDefinition,
            ByVal def2 As DotPlotDefinition
            ) As Boolean Implements IEqualityComparer(Of DotPlotDefinition).Equals

            ' Check whether the compared objects reference the same data.
            If def1 Is def2 Then Return True

            'Check whether any of the compared objects is null.
            If def1 Is Nothing OrElse def2 Is Nothing Then Return False

            ' Check whether the properties are equal.
            Return (def1.Name = def2.Name)
        End Function

        Public Function GetHashCode1(
            ByVal obj As DotPlotDefinition
            ) As Integer Implements IEqualityComparer(Of DotPlotDefinition).GetHashCode

            ' Check whether the object is null.
            If obj Is Nothing Then Return 0

            ' Get hash code for the Name field if it is not null.
            Return obj.Name.GetHashCode
        End Function
    End Class

    ''' <summary>
    ''' For now only the dotplot definition is here, probably more should be added later on. 
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public Class CollectionOfProfilesDefinition

        ''' <summary>
        ''' General constructor
        ''' </summary>
        Public Sub New(id As Integer)

        End Sub

        ''' <summary>
        ''' Copy Constructor.
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New(other As CollectionOfProfilesDefinition)
            ID = other.ID
        End Sub


        Public ReadOnly Property ID As Integer

    End Class


End Namespace ' PlotSettings