Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Data.ParticleHandling.Channel
Imports System.Runtime.Serialization
Imports System.Xml
Imports CytoSense.Serializing
Imports CytoSense.CytoSettings


Namespace Data.Analysis

    Public Class AxisValuesCache
        Public Class AxisValuesCacheEntry
            Public axisName As String
            Public axisValues As Single()
        End Class

        Private _cacheEntries As List(Of AxisValuesCacheEntry)

        Public Sub New()
            _cacheEntries = New List(Of AxisValuesCacheEntry)
        End Sub

        ''' <summary>
        ''' Gets an AxisValuesCacheEntry
        ''' 
        ''' This function creates a new entry if there is no entry with the axisName
        ''' because the function does not know how to calculate the axisValues, a new entry
        ''' has its axisValues field set to nothing. 
        ''' 
        ''' The caller MUST access the result inside a SyncLock to test if the axisValues
        ''' of the entry are calculated, if not the caller should calculate the entry.axisValues
        ''' inside the SyncLock
        ''' </summary>
        ''' <param name="axisName"></param>
        ''' <returns></returns>
        Public Function GetEntry(axisName As String) As AxisValuesCacheEntry
            Dim entry As AxisValuesCacheEntry

            SyncLock _cacheEntries
                For Each entry In _cacheEntries
                    If entry.axisName = axisName Then
                        Return entry
                    End If
                Next

                entry = New AxisValuesCacheEntry() With {.axisName = axisName, .axisValues = Nothing}
                _cacheEntries.Add(entry)
            End SyncLock

            Return entry
        End Function
    End Class

    ''' <summary>
    ''' An abstract class that wraps a data dimension.
    ''' There are concrete subclasses for implementations, e.g. SingleAxis, RatioAxis
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public MustInherit Class Axis
        Implements IXmlDocumentIO

        Public MustOverride ReadOnly Property Name As String
        Public MustOverride ReadOnly Property ShortName As String


''' <summary>
''' After XML deserialization the CytoSettings reference needs to be set, so call this function to do that before doing anything
''' else with the object.
''' </summary>
''' <param name="settings"></param>
        Public MustOverride Sub UpdateCytoSettings( settings As CytoSenseSetting )


        Public MustOverride Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting

        ''' <summary>
        ''' Can determine for an axis whether it is prudent to plot it on a log scale or not.
        ''' </summary>
        Protected Shared Function IsLogScale(ByVal chantype As CytoSettings.ChannelTypesEnum, ByVal par As ChannelData.ParameterSelector) As Boolean
            Dim log As Boolean = False

            Select Case par
                Case ChannelData.ParameterSelector.Length
                    log = True
                Case ChannelData.ParameterSelector.VariableLength
                    log = True
                Case ChannelData.ParameterSelector.Total
                    log = True
                Case ChannelData.ParameterSelector.Maximum
                    log = True
                Case ChannelData.ParameterSelector.Average
                    log = True
                Case ChannelData.ParameterSelector.Inertia
                    log = True
                Case ChannelData.ParameterSelector.CentreOfGravity
                    log = True
                Case ChannelData.ParameterSelector.FillFactor
                    log = False
                Case ChannelData.ParameterSelector.Asymmetry
                    log = False
                Case ChannelData.ParameterSelector.NumberOfCells
                    log = False
                Case ChannelData.ParameterSelector.SampleLength
                    log = True
                Case ChannelData.ParameterSelector.TimeOfArrival
                    log = False
                Case Else
                    log = True
            End Select

            If chantype = CytoSense.CytoSettings.ChannelTypesEnum.Curvature Then
                'Things with curvature (like total) can be negative
                log = False
            End If

            Return log
        End Function

        Public Overrides Function ToString() As String
            Return Name()
        End Function

        Public Overloads Shared Operator =(ByVal axis1 As Axis, ByVal axis2 As Axis) As Boolean
            'Check whether types are identical
            If Object.ReferenceEquals(axis1.GetType(), axis2.GetType()) AndAlso axis1.Name = axis2.Name Then
                Return True
            Else
                Return False
            End If
        End Operator

        Public Overloads Shared Operator <>(ByVal axis1 As Axis, ByVal axis2 As Axis) As Boolean
            Return Not axis1 = axis2
        End Operator

        ''' <summary>
        ''' Returns a data point on this axis
        ''' </summary>
        Public MustOverride Function GetValue(ByVal particle As Particle) As Single
        'Public MustOverride Function GetValues(particles As Particle(), cache As AxisValuesCache) As Single()
        Public MustOverride Function GetValues(datafile As DataFileWrapper) As Single()

        Private _isLog As Boolean
        ''' <summary>
        ''' Denotes whether this axis is logarithmic
        ''' </summary>
        ''' <remarks>This property might seem superficial at first, but it is necessary to determine the bounds of free forms and polygons</remarks>
        Public Property isLog() As Boolean
            Get
                Return _isLog
            End Get
            Set(value As Boolean)
                _isLog = value
            End Set
        End Property

        Public Overridable Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            parentNode.setAttribute("Name", Name) ' written but not read
            parentNode.setAttribute("IsLog", _isLog.ToString())
        End Sub

        Public Overridable Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            If Not parentNode.TryGetAttribute(Of Boolean)("IsLog", _isLog) Then
				_isLog = parentNode.ReadChildElementAsBoolean("IsLog")
            End If
        End Sub
    End Class

    ''' <summary>
    ''' A data dimension of one channel and parameter combination.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class SingleAxis
        Inherits Axis
        Implements IXmlDocumentIO

        ''' <summary>
        ''' The ChannelWrapper that defines this axis' channel. The wrapper is not allowed to be serialized, so we save it 
        ''' by its name and accept the loss in case of name changes
        ''' </summary>
        <NonSerialized()> Private _channel As CytoSense.CytoSettings.ChannelWrapper
        Private _channelName As String
        Private _channelType As CytoSense.CytoSettings.ChannelTypesEnum 'this does not capture HS/LS difference
        Private _cytoSettings As CytoSettings.CytoSenseSetting
        <NonSerialized()> Public _channelIdx As Integer = -1
        <NonSerialized()> Private _channelIdxInHardware As Boolean = False
        Private _parameter As ChannelData.ParameterSelector

        ''' <summary>
        ''' Needed for XML (de)serialization
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a combination of channel and parameter to generate a data dimension (dotplot axis).
        ''' </summary>
        ''' <param name="cytosettings">
        ''' This parameter is needed for serializing functionality
        '''</param>
        ''' <remarks>Sets the logarithmically (YES) to the default value</remarks>
        Public Sub New(channel As CytoSettings.ChannelWrapper, parameter As ChannelData.ParameterSelector, cytosettings As CytoSettings.CytoSenseSetting)
            _channel = channel
            _channelType = _channel.Channeltype
            _channelName = channel.ChannelInfo.name
            _cytoSettings = cytosettings
            _parameter = parameter
            isLog = IsLogScale(Me.Channel.Channeltype, Me.Parameter)
        End Sub

        <OnDeserialized()>
        Private Sub OnDeserialized(context As StreamingContext)
            _channelIdx = -1 'Always initialize to -1 on load, to force recalculation
        End Sub

        Public ReadOnly Property Channel As CytoSettings.ChannelWrapper
            Get
                If Not Object.Equals(_channel, Nothing) Then
                    'Channel is available, return
                    Return _channel
                Else
                    'Channel is unavailable. If this is due to serializing (we have a name), reinstate. 
                    If _channelType = CytoSense.CytoSettings.ChannelTypesEnum.Unknown Then
                        Return Nothing
                    ElseIf _channelName IsNot Nothing And _channelName <> "" Then 'in old (but beta) CC4 workspaces, only the channel type may be serialized
                        _channel = _cytoSettings.getChannellistItem(_channelName)
                        Return _channel
                    Else
                        _channel = _cytoSettings.GetChannellistItemByType(_channelType)
                        Return _channel
                    End If
                End If
            End Get
        End Property

        Public ReadOnly Property Parameter As ChannelData.ParameterSelector
            Get
                Return _parameter
            End Get
        End Property

        Public Overrides ReadOnly Property Name As String
            Get
                Dim chName As String = ""
                If Not String.IsNullOrEmpty(_channelName) Then
                    chName = _channelName
                ELse
                    chName = Channel.ToString()
                End If
                If Channel.VirtualChannelType = VirtualChannelInfo.VirtualChannelType.ParticleData Then
                    Return _parameter.ToString() & " " & chName
                Else
                    Return ChannelData.ParameterNames(_parameter) & " " & ChannelData.ParameterUnits(_parameter) & " " & chName
                End If
                
            End Get
        End Property

        Public Overrides ReadOnly Property ShortName As String
            Get
                If Channel.VirtualChannelType = VirtualChannelInfo.VirtualChannelType.ParticleData Then
                    Return _parameter.ToString() & " " & Channel.ToString
                Else
                    Return ChannelData.ParameterNames(_parameter) & " " & Channel.ToString
                End If
            End Get
        End Property

        ''' <summary>
        ''' The machine for which this axis was created. Useful when de-serializing an axis in a new environment.
        ''' </summary>
        Public Overrides Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _cytoSettings
            End Get
            Set(value As CytoSense.CytoSettings.CytoSenseSetting)
                _cytoSettings = value
            End Set
        End Property

        Public Overrides Function GetValue(particle As ParticleHandling.Particle) As Single
            If Not Channel.VirtualChannelType = VirtualChannelInfo.VirtualChannelType.ParticleData Then
            ' this function is (also) called from Gate.TestSingleParticle
            ' In batch exports particles are processed one by one, so the _values array is not present

                If _channelIdx < 0 Then
                    Dim res = particle.GetChannelIndexByName(Channel.ChannelInfo.name)
                    _channelIdxInHardware = res.Item1
                    _channelIdx = res.Item2
                End If

                If _channelIdxInHardware Then
                    ' Only for FWS_L, FWS_R channels, these channels are only used by the GUI and not by batch export
                    Return particle.ChannelData_Hardware(_channelIdx).Parameter(Parameter)
                Else
                    Return particle.ChannelData(_channelIdx).Parameter(Parameter)
                End If
            Else
                If particle.hasImage Then
                    Return DirectCast(particle, ImagedParticle).ImageHandling.ParticleData.Parameter(Parameter)
                Else
                    Return Single.NaN
                End If
            End If
        End Function

        Public Overrides Function GetValues(datafile As DataFileWrapper) As Single()
            If Not Channel.VirtualChannelType = VirtualChannelInfo.VirtualChannelType.ParticleData Then
                Dim particles = datafile.SplittedParticles
                Dim valuesCache = datafile._axisValuesCache

                If particles Is Nothing OrElse particles.Length = 0 Then
                    Return Nothing
                End If

		        If CytoSettings Is Nothing
			        CytoSettings = datafile.CytoSettings
		        End If

                Dim cacheEntry = valuesCache.GetEntry(Name)

                SyncLock cacheEntry
                    ' Should the values be calculated?

                    If cacheEntry.axisValues Is Nothing Then
                        If _channelIdx < 0 Then
                            GetValue(particles(0)) ' just to set _channelIdx and _channelIdxInHardware fields
                        End If

                        cacheEntry.axisValues = New Single(particles.Length - 1) {}

                        If _channelIdxInHardware Then
                            Parallel.For(0, particles.Length,
                                    Sub(i As Integer)
                                        cacheEntry.axisValues(i) = particles(i).ChannelData_Hardware(_channelIdx).Parameter(Parameter)
                                    End Sub)
                        Else
                            Parallel.For(0, particles.Length,
                                    Sub(i As Integer)
                                        cacheEntry.axisValues(i) = particles(i).ChannelData(_channelIdx).Parameter(Parameter)
                                    End Sub)
                        End If

                        Debug.WriteLine(String.Format("Calculated AxisValuesCacheEntry {0}", Name))
                    End If
                End SyncLock

                Return cacheEntry.axisValues
            Else 'ParticleData
                Dim data As New List(Of Single)
                For Each particle In datafile.SplittedParticles
                    If particle.hasImage Then
                        data.Add(DirectCast(particle, ImagedParticle).ImageHandling.ParticleData.Parameter(Parameter))
                    Else
                        data.Add(Single.NaN)
                    End If
                Next
                Return data.ToArray()
            End If
        End Function

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            MyBase.XmlDocumentWrite(document, parentNode)

            parentNode.SetAttribute("Type", Me.GetType().Name)
            parentNode.SetAttribute("ChannelType", _channelType.ToString())
            parentNode.SetAttribute("ChannelName", _channelName)
            parentNode.SetAttribute("Parameter", _parameter.ToString())
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            MyBase.XmlDocumentRead(document, parentNode)

            If Not parentNode.TryGetAttribute(Of String)("ChannelName", _channelName) Then
				_channelType = parentNode.ReadChildElementAsEnum(Of CytoSettings.ChannelTypesEnum)("ChannelType")
				_channelName = parentNode.ReadChildElementAsString("ChannelName")
				_parameter = parentNode.ReadChildElementAsEnum(Of ChannelData.ParameterSelector)("Parameter")

			Else
				_channelType = parentNode.GetAttributeAsEnum(Of ChannelTypesEnum)("ChannelType")
				_parameter = parentNode.GetAttributeAsEnum(Of ChannelData.ParameterSelector)("Parameter")
			End If

            _cytoSettings = Nothing
            _channel      = Nothing
            OnDeserialized(Nothing)
        End Sub
        ''' <summary>
        ''' After loading from XML we need to update the CytoSettings reference for the axis.
        ''' </summary>
        ''' <param name="settings"></param>
        Public Overrides Sub UpdateCytoSettings( settings As CytoSenseSetting )
            _cytoSettings = settings
            _channel = _cytoSettings.getChannellistItem(_channelName)
        End Sub

    End Class

    ''' <summary>
    ''' A data dimension of two channels and parameters in a ratio: AxisNum/AxisDen
    ''' </summary>
    <Serializable()> Public Class RatioAxis
        Inherits Axis
        Implements IXmlDocumentIO

        ''' <summary>
        ''' Only for XML (de)serialization
        ''' </summary>
        Public Sub New()
        End Sub

        ''' <summary>
        ''' Initializes a combination of two axes (numerator, denominator) to make one ratio axis for a dotplot.
        ''' </summary>
        ''' <remarks>The default for isLog is num.isLog OR den.isLog</remarks>
        Public Sub New(ByVal channelNum As CytoSettings.ChannelWrapper, ByVal parNum As ChannelData.ParameterSelector, ByVal channelDen As CytoSettings.ChannelWrapper, ByVal parDen As ChannelData.ParameterSelector, ByVal cytosettings As CytoSettings.CytoSenseSetting)
            Me.New(New SingleAxis(channelNum, parNum, cytosettings), New SingleAxis(channelDen, parDen, cytosettings))
            isLog = AxisNumerator.isLog OrElse AxisDenominator.isLog
        End Sub

        ''' <summary>
        ''' Initializes a combination of two axes (numerator, denominator) to make one ratio axis for a dotplot.
        ''' </summary>
        Public Sub New(ByVal axisNum As SingleAxis, ByVal axisDen As SingleAxis)
            _axisNum = axisNum
            _axisDen = axisDen
        End Sub

        Private _axisNum As SingleAxis
        Public ReadOnly Property AxisNumerator() As SingleAxis
            Get
                Return _axisNum
            End Get
        End Property

        Private _axisDen As SingleAxis
        Public ReadOnly Property AxisDenominator() As SingleAxis
            Get
                Return _axisDen
            End Get
        End Property

        Public Overrides ReadOnly Property Name As String
            Get
                Return String.Format("{0} / {1}", AxisNumerator.Name, AxisDenominator.Name)
            End Get
        End Property

        Public Overrides ReadOnly Property ShortName As String
            Get
                Return String.Format("{0} / {1}", AxisNumerator.ShortName, AxisDenominator.ShortName)
            End Get
        End Property

        ''' <summary>
        ''' Returns a data point on this axis
        ''' </summary>
        ''' <remarks>Warning! May return single.inf in case of division by zero</remarks>
        Public Overrides Function GetValue(particle As ParticleHandling.Particle) As Single
            Return _axisNum.GetValue(particle) / _axisDen.GetValue(particle)
        End Function

        Public Overrides Function GetValues(datafile As DataFileWrapper) As Single()
            Dim particles = datafile.SplittedParticles
            Dim valuesCache = datafile._axisValuesCache

            If particles Is Nothing Then
                Return Nothing
            End If

            Dim cacheEntry = valuesCache.GetEntry(Name)

            SyncLock cacheEntry
                ' Should the values be calculated?

                If cacheEntry.axisValues Is Nothing Then
                    cacheEntry.axisValues = New Single(particles.Length - 1) {}

                    Dim numValues = _axisNum.GetValues(datafile)
                    Dim denomValues = _axisDen.GetValues(datafile)

                    For i = 0 To particles.Length - 1
                        cacheEntry.axisValues(i) = numValues(i) / denomValues(i)
                    Next

                    Debug.WriteLine(String.Format("Calculated AxisValuesCacheEntry {0}", Name))
                End If
            End SyncLock

            Return cacheEntry.axisValues
        End Function

        ''' <summary>
        ''' The machine for which this axis was created. Useful when de-serializing an axis in a new environment.
        ''' </summary>
        Public Overrides Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
            Get
                'Assume both ratio axis have the same CytoSettings
                Return _axisDen.CytoSettings
            End Get
            Set(value As CytoSense.CytoSettings.CytoSenseSetting)
                _axisDen.CytoSettings = value
                _axisNum.CytoSettings = value
            End Set

        End Property

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            MyBase.XmlDocumentWrite(document, parentNode)

            parentNode.SetAttribute("Type", Me.GetType().Name)
            _axisNum.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "AxisNumerator"))
            _axisDen.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "AxisDenominator"))
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            MyBase.XmlDocumentRead(document, parentNode)

            _axisNum = New SingleAxis()
            _axisDen = New SingleAxis()

            _axisNum.XmlDocumentRead(document, parentNode.Item("AxisNumerator"))
            _axisDen.XmlDocumentRead(document, parentNode.Item("AxisDenominator"))
        End Sub

        Public Overrides Sub UpdateCytoSettings( settings As CytoSenseSetting )
            _axisNum.UpdateCytoSettings(settings)
            _axisDen.UpdateCytoSettings(settings)
        End Sub

    End Class
End Namespace