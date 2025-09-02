Imports System.ComponentModel
Imports System.Text
Imports System.Runtime.Serialization
Imports System.IO

Namespace MeasurementSettings

    ''' <summary>
    ''' Enum that specifies the logarithmic conversion algorithm used to go from 16 bit values to 8.
    ''' The actual byte values used are 1 higher then the register values used, so you can convert this
    ''' enum to the register value required by doing EnumValue - 1. Take care to keep it that way!
    ''' </summary>
    Public Enum LogConversion As UInt16
        Invalid              =  0
        OriginalLog          =  1 ' reg= 0 FJs original log function, misses 48 bytes, default value for electronics
        BEGIN                =  OriginalLog 'First valid entry
        Decade16_3bLog       =  2 ' reg= 1 New logarithmic conversion, only 16 bytes unused
        Decade16Old          =  3 ' reg= 2 Different, worste conversion, do NOT use.
        RESERVED_3           =  4 ' reg= 3
        RESERVED_4           =  5 ' reg= 4
        RESERVED_5           =  6 ' reg= 5
        RESERVED_6           =  7 ' reg= 6
        Linear8Bit_low       =  8 ' reg= 7 8 Bit linear, lowest 8 bits,  step= 0.125, max=  32 millivolt (minus a few).
        Linear8Bit_shifted_1 =  9 ' reg= 8 8 Bit linear, shifted 1 bit,  step= 0.250, max=  64 millivolt (minus a few).
        Linear8Bit_shifted_2 = 10 ' reg= 9 8 Bit linear, shifted 2 bit,  step= 0.500, max= 128 millivolt (minus a few).
        Linear8Bit_shifted_3 = 11 ' reg=10 8 Bit linear, shifted 3 bit,  step= 1.000, max= 256 millivolt (minus a few).
        Linear8Bit_shifted_4 = 12 ' reg=11 8 Bit linear, shifted 4 bit,  step= 2.000, max= 512 millivolt (minus a few).
        Linear8Bit_shifted_5 = 13 ' reg=12 8 Bit linear, shifted 5 bit,  step= 4.000, max=1024 millivolt (minus a few).
        Linear8Bit_shifted_6 = 14 ' reg=13 8 Bit linear, shifted 6 bit,  step= 8.000, max=2048 millivolt (minus a few).
        Linear8Bit_shifted_7 = 15 ' reg=14 8 Bit linear, shifted 7 bit,  step=16.000, max=4096 millivolt (minus a few).
        Linear8Bit_high      = 16 ' reg=15 8 Bit linear, highest 8 bits, step=32.000, max=8192 millivolt (minus a few).
        THE_END              = 17 ' First invalid entry.
    End Enum



    <Serializable()> Public Structure IifSetSelectionInfo
        Public SetId          As Integer
        Public WantImages     As Boolean
        Public NumberOfImages As Integer ' 0 means unlimited.
    End Structure


    <Serializable()> Public Class Measurement
        Implements ICloneable

        Private _cytosettings As CytoSense.CytoSettings.CytoSenseSetting
        Public release As New Serializing.VersionTrackableClass(New Date(2013, 4, 23))
        <NonSerialized()> Public Event RefreshNeeded()

        Public Sub New(name As String, folderPath As String, cytosettings As CytoSense.CytoSettings.CytoSenseSetting)
            _cytosettings = cytosettings
            SetDefaultMeasurement(name, folderPath)
        End Sub

        ''' <summary>
        ''' Copy Constructor.  Creates a deep copy of the measurements object.
        ''' MemberWise clone is unfortunately unusable as it creates a shallow copy. This does
        ''' not include arrays, or other objects.
        ''' </summary>
        ''' <param name="other">The measurement objects we are copying the data from.</param>
        ''' <remarks>NOTE: Not everything is copied deep, the _cytosettins reference is just copied
        ''' as a reference.  There should only be one of these per machine.</remarks>
        Public Sub New( other As Measurement )
            Assign(other)
        End Sub

        ''' <summary>
        ''' Assigns the other measurement to this, it COPIES all values, a deep copy is made,
        ''' so nested structures are cloned and then assigned.  This allows you to copy
        ''' the settings from one measurement to a new one.
        ''' </summary>
        ''' <param name="other">The other measurement to copy all the values from.</param>
        ''' <remarks>YOU MUST add members here if you add members to the measurement settings.</remarks>
        Public Sub Assign(other As Measurement)
            _cytosettings    = other._cytosettings  'NOTE: SHALLOW, reference copied only
            _status          = other._status
            _progress        = other._progress
            _repeatID        = other._repeatID
            _cytoTrayID      = other._cytoTrayID
            _Enabled         = other._Enabled
            _TabName         = other._TabName
#Disable Warning BC40008 ' Type or member is obsolete
            _SamplePompSpeed = other._SamplePompSpeed
            _configuredSamplePumpSpeed = other._configuredSamplePumpSpeed
#Enable Warning BC40008 ' Type or member is obsolete

            _actualSamplePumpSpeedSetting            = other._actualSamplePumpSpeedSetting
            _configuredSamplePumpSpeedSetting        = other._configuredSamplePumpSpeedSetting
            _configuredMinimalSamplePumpSpeedSetting = other._configuredMinimalSamplePumpSpeedSetting
            _limitParticleRate                       = other._limitParticleRate
            _maxParticleRate        = other._maxParticleRate
            _enableMinimumAutoSpeed = other._enableMinimumAutoSpeed
#Disable Warning BC40008 ' Type or member is obsolete
            _minimumAutoSpeed = other._minimumAutoSpeed
#Enable Warning BC40008 ' Type or member is obsolete

            If other._TriggerLevelsFJ IsNot Nothing Then
                _TriggerLevelsFJ = CType(other._TriggerLevelsFJ.Clone(), Integer())  'DEEP
            End If

            If other._channelDataConversion IsNot Nothing Then
                Redim _channelDataConversion(other._channelDataConversion.Length-1)
                For i = 0 To _channelDataConversion.Length-1
                    _channelDataConversion(i) = other._channelDataConversion(i)
                Next
            End If

            _TriggerLevel1e        = other._TriggerLevel1e
            _TriggerLevel2e        = other._TriggerLevel2e
            _StopafterTimertext    = other._StopafterTimertext
            _MaxNumberParticleText = other._MaxNumberParticleText
            _StopAtParticles       = other._StopAtParticles
            _MaxAnalysedVolume     = other._MaxAnalysedVolume
            _StopAtAnalysedVolume  = other._StopAtAnalysedVolume
            _MaxPumpedVolume       = other._MaxPumpedVolume
            _StopAtPumpedVolume    = other._StopAtPumpedVolume
            _MaxNumberFotoText     = other._MaxNumberFotoText
            _StopAtFotos           = other._StopAtFotos
            _StopAfterTime         = other._StopAfterTime
            _SaveTextbox           = other._SaveTextbox
            _FlushCheck            = other._FlushCheck
            _TellCheck             = other._TellCheck
            _IIFCheck              = other._IIFCheck
            _IIFFileLocation       = other._IIFFileLocation
#Disable Warning BC40008
            _GVCheck               = other._GVCheck
#Enable Warning BC40008
            _BSTCheck              = other._BSTCheck
            _repeat                = other._repeat
            _blockSize             = other._blockSize

            IF other._PMTlevels IsNot Nothing Then
                _PMTlevels = CType(other._PMTlevels.Clone(),Byte())   'DEEP
            End If

            _subFolder               = other._subFolder
            _userRemarks             = other._userRemarks
            _autoBlockSize           = other._autoBlockSize
            _maxTimeOut              = other._maxTimeOut
            _adaptiveMaxTimeOut      = other._adaptiveMaxTimeOut
            _seperateConcentration   = other._seperateConcentration
            _adaptiveSamplePumpSpeed = other._adaptiveSamplePumpSpeed
            _measureNoiseLevels      = other._measureNoiseLevels
            _calibrateCamera         = other._calibrateCamera

            If other._TriggerChannelArray IsNot Nothing Then
                _TriggerChannelArray = CType(other._TriggerChannelArray.Clone(), Boolean()) 'DEEP
            End If

            If other._SelectableHighLowChannelsOnHighArray IsNot Nothing Then
                _SelectableHighLowChannelsOnHighArray = CType(other._SelectableHighLowChannelsOnHighArray.Clone(),Boolean())   'DEEP
            End If

            If other._LowCheck IsNot Nothing Then
                _LowCheck = CType(other._LowCheck.Clone(),Boolean())   'DEEP
            End If

            _IIFParameters = New IIFParameters(other._IIFParameters)
            _IIFuseTargetAll             = other._IIFuseTargetAll
            _IIFuseSmartGrid             = other._IIFuseSmartGrid
            _IIFuseTargetRange           = other._IIFuseTargetRange
            _IIFFwsRatioRangeCalibration = other._IIFFwsRatioRangeCalibration
            _IIFPhotographLargeParticles = other._IIFPhotographLargeParticles
            _IIFFwsRatioMax              = other._IIFFwsRatioMax
            _IIFFwsRatioMin              = other._IIFFwsRatioMin
            _IIFRestrictFwsRange         = other._IIFRestrictFwsRange
            If other._iifSmartgridChannelIds IsNot Nothing Then
                _iifSmartgridChannelIds = New List(Of Integer)(other._iifSmartgridChannelIds)
            End If

            _IIFuseFreeFormSelection  = other._IIFuseFreeFormSelection
            _IIFFreeFormSelection     = New IIFFreeFormSelection(other._IIFFreeFormSelection)
            _IIFRoiName               = other._IIFRoiName
            _iifUseSetDefinitionSelector = other._iifUseSetDefinitionSelector
            _iifSetDefinitionFilename    = other._iifSetDefinitionFilename
            _iifSetDefinitionXml         = other._iifSetDefinitionXml
            If other._iifSetSelectionInfo Is Nothing Then
                _iifSetSelectionInfo     = New List(Of IifSetSelectionInfo)()
            Else
                _iifSetSelectionInfo     = New List(Of IifSetSelectionInfo)(other._iifSetSelectionInfo)
            End If

            _MultiSamplerValve       = other._MultiSamplerValve
            _multiSamplerPrime       = other._multiSamplerPrime
            _multiSamplerClean       = other._multiSamplerClean

            _cytoSelector            = other._cytoSelector

            If other._cytoSelectorEnabledTraysIDs IsNot Nothing Then
                _cytoSelectorEnabledTraysIDs = New List(Of Integer)(other._cytoSelectorEnabledTraysIDs)
            End If

            _useStainingModule             = other._useStainingModule
            _includeUnstainedMeasurement   = other._includeUnstainedMeasurement
            _useExternalStaining           = other._useExternalStaining
            _cleanStainingModuleAfterwards = other._cleanStainingModuleAfterwards
            _cleanStainingModuleAfterwardsIncludeSamplePump = other._cleanStainingModuleAfterwardsIncludeSamplePump
            _stainIncubateSeconds          = other._stainIncubateSeconds

            _injectDye1 = other._injectDye1 
            _dye1Amount = other._dye1Amount 
            _injectDye2 = other._injectDye2 
            _dye2Amount = other._dye2Amount 

            _segmentedDatafile           = other._segmentedDatafile
            EnableExport                 = other.EnableExport

            If other._exportSettings IsNot Nothing Then
                _exportSettings = New ExportSettings(other._exportSettings)
            End If
            'Array of settings, need to create a deep copy.
            If other._preSiftSettings IsNot Nothing Then
                Redim _preSiftSettings(other._preSiftSettings.Length - 1)
                For i = 0 To _preSiftSettings.Length - 1
                    _preSiftSettings(i) = New Data.PreSiftSetting(other._preSiftSettings(i))
                Next
            End If

            'Array of settings, need to create a deep copy.
            If other._smartTriggerSettings IsNot Nothing Then
                Redim _smartTriggerSettings( other._smartTriggerSettings.Length-1)
                For i = 0 To _smartTriggerSettings.Length - 1
                    _smartTriggerSettings(i) = New SmartTriggerSettings(other._smartTriggerSettings(i))
                Next
            End If

            _smartTriggerCombinationType = other._smartTriggerCombinationType

            If other._remoteSettings IsNot Nothing Then
                _remoteSettings = New Remote.RemoteSettings( other._remoteSettings )
            End If

            _sendEmail       = other._sendEmail
            _emailAddress    = other._emailAddress
            _ccWorkspaceFile = other._ccWorkspaceFile

            If other._functionGeneratorSettings IsNot Nothing Then
                _functionGeneratorSettings = New FunctionGeneratorSettings(other._functionGeneratorSettings)
            End If

            _useDcRestore       = other._useDcRestore
            _usbSpeed           = other._usbSpeed
            _isBeadsMeasurement = other._isBeadsMeasurement

            _useDps       = other._useDps
            _depth        = other._depth
            _depthProfile = New List(Of Single)(other._depthProfile)
        End Sub

        Private Sub SetDefaultMeasurement(name As String, folderPath As String)
            Me.Enabled = True

            'A speed of 1ul/s is recommended for novice users. Below an ugly but easy way to find the recommended samplespeedbyte for all possible pump configurations. 
            SamplePumpSpeedSetting = 15 ' Will be 1.05 with the default calibration (DC sample pump). So if below fails?
            For i = 0 To 255
                Dim tmpspeed As Double = _cytosettings.getFlowSpeed(i)
                If tmpspeed > 1 And tmpspeed < 1.1 Then
                    SamplePumpSpeedSetting = i
                    Exit For
                End If
            Next
            ConfiguredSamplePumpSpeedSetting = SamplePumpSpeedSetting
            MinimumAutoSpeedSetting = 1 ' Minimum speed setting.

            If _cytosettings.hasFJDataElectronics Then
                Dim trigValue As Integer = CInt(30 / _cytosettings.triggerlevelConstant)
                For i = 0 To TriggerLevelsFJ.Length - 1 
                    TriggerLevelsFJ(i) = trigValue
                Next
            Else
                Me.TriggerLevelByte1e = 30
                Me.TriggerLevelByte2e = 30

            End If
            Me.StopafterTimertext = 180
            Me.MaxNumberParticleText = 1000
            Me.StopAtParticles = False
            Me.MaxNumberFotoText = 150
            Me.StopAtFotos = False
            ReDim Me.LowCheck(7)
            For i As Int16 = 0 To 7
                Me.LowCheck(i) = False
            Next
            Me.SaveTextbox = folderPath + name + ".cyz"
            Me.FlushCheck = False
            Me.TellCheck = False
            Me.IIFCheck = False
#Disable Warning BC40008
            Me.GVCheck = False
#Enable Warning BC40008
            ReDim Me.TriggerChannelArray(_cytosettings.channels.Length - 1)

            'findout the id of recommended channel for triggering
            Dim triggerchannelID As Integer = 0
            If Not _cytosettings.hasFJDataElectronics Then
                triggerchannelID += 1 'in this case the zeroth channel is a triggerchannel
            End If
            If _cytosettings.hasCurvature Then
                triggerchannelID += 2  'in this case the first two channels are FWS L/R, and are not recommend for triggering
            Else
                triggerchannelID += 1
            End If
            Me.TriggerChannelArray(triggerchannelID) = True


            Me.TabName = name
            Me.repeat = 1
            Me.BlockSize = BlockSizes.b4k
            If _cytosettings.hasaPIC AndAlso _cytosettings.PIC.I2CPMTLevelControl Then
                _PMTlevels = CType(_cytosettings.CytoUSBSettings.PMTLevelPresets(1).Clone(),Byte())
            End If
            _segmentedDatafile = True
            _MaxNumberFotoText = 250

            _IIFPhotographLargeParticles = _cytosettings.iif.AllwaysTakeLargerParticlePictures

            Dim conv = If(_cytosettings.hasFJDataElectronics, LogConversion.Decade16_3bLog, LogConversion.OriginalLog)
            ReDim _channelDataConversion(_cytosettings.channels.Length - 1)
            For i = 0 To _cytosettings.channels.Length - 1
                _channelDataConversion(i) = conv 
            Next
        End Sub

        ''' <summary>
        ''' Set some defaults for some properties, in case they were not serialized. If they
        ''' were serialized, these defaults will be overridden on loading.
        ''' </summary>
        ''' <param name="context"></param>
        <OnDeserializing()> _
        Private Sub OnDeserializing(ByVal context As StreamingContext)
            _limitParticleRate      = False
            _maxParticleRate        = 6000
            _enableMinimumAutoSpeed = False
#Disable Warning BC40008 ' Type or member is obsolete
            _minimumAutoSpeed = 255
#Enable Warning BC40008 ' Type or member is obsolete
            _actualSamplePumpSpeedSetting            = Integer.MinValue
            _configuredSamplePumpSpeedSetting        = Integer.MinValue
            _configuredMinimalSamplePumpSpeedSetting = Integer.MinValue
        End Sub

        <OnDeserialized()> _
        Private Sub OnDeserialized(ByVal context As StreamingContext)
            If _actualSamplePumpSpeedSetting = Integer.MinValue Then ' Need to convert old settings.
#Disable Warning BC40008 ' Type or member is obsolete
                If _configuredSamplePumpSpeed = 0 Then  ' If no configured value is set, then use the current speed value.
                    _configuredSamplePumpSpeed = CByte(_SamplePompSpeed)
                End If
                If _cytosettings IsNot Nothing Then
                   _actualSamplePumpSpeedSetting            = 256 - _SamplePompSpeed
                   _configuredSamplePumpSpeedSetting        = 256 - _configuredSamplePumpSpeed
                   _configuredMinimalSamplePumpSpeedSetting = 256 - _minimumAutoSpeed
                End If ' Else no cytoSettings, so we cannot do this check. For channel data conversion we do it on first access,
                       ' we do this when setting the CytoSettings property. Lets do this for us as well.

                       ' There is a setCytoSettings member but that is not called anymore.
#Enable Warning BC40008 ' Type or member is obsolete
            End If
            If _channelDataConversion Is Nothing Then
                ' We have some (very) old files where there is an incorrect value stored form _cytosettings.channels.length.
                ' This causes crashes on loading the files. In this case it does not hurt to simply always create the
                ' maximum number of channelDataConversions.  A few bytes extra in memory is not a problem.
                ' Maximum is 12 channels + 2 trigger + 1 DSP. Makes 15
                If _cytosettings IsNot Nothing Then
                    ReDim _channelDataConversion(15 - 1)
                    For i = 0 To 15 - 1
                        _channelDataConversion(i) = LogConversion.OriginalLog  ' This is the default for the hardware, so keep it the default here as well and for old hardware.
                    Next
                End If ' Else Some old files do not have this setting on loading, we need to do it later when accessing the _channelDataConversion
            End If
            If _depthProfile Is Nothing Then 'Loaded an old measurement setting without a depth profile list. Initialize it to an empty list.
                _depthProfile = New List(Of Single)()
            End If

            If _iifSetDefinitionXml Is Nothing Then 
                _iifSetDefinitionXml = ""
            End If

           If _subfolder Is Nothing Then
                _subfolder = ""
           End If
        End Sub

        <NonSerialized()> Private _status As String = ""
        <NonSerialized()> Private _progress As Double = 0
        Private _repeatID As Integer = 1
        <NonSerialized()> Public Event newStatus(ByVal message As String)

        Public Sub SetStatus(ByVal message As String, ByVal progress As Double)
            _status = message
            _progress = progress
            RaiseEvent newStatus(_status)
        End Sub

        <ComponentModel.Browsable(False)>
        Public ReadOnly Property Status As String
            Get
                Return _status
            End Get
        End Property
        <ComponentModel.Browsable(False)> _
        Public ReadOnly Property Progress As Double
            Get
                Return _progress
            End Get
        End Property

        ''' <summary>
        ''' Keeps track on the number of measurement this is. (Only  important if repeat > 1)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Measurement results"),
            DisplayName("Repeat #"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property RepeatID As Integer
            Get
                Return _repeatID
            End Get

            Set(ByVal value As Integer)
                _repeatID = value
                RaiseEvent RefreshNeeded()
            End Set
        End Property

        Private _cytoTrayID As Integer

        <Category("Measurement instrument settings"), DisplayName("Cytoselector tray"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public Property CytoSelectorTrayID As Integer
            Get
                Return _cytoTrayID
            End Get

            Set(ByVal value As Integer)
                _cytoTrayID = value
                RaiseEvent RefreshNeeded()
            End Set
        End Property

        Private _Enabled As Boolean = True

        <Category("Miscellaneous"),
            DisplayName("Enabled"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Check to enable the measurement"),
            ComponentModel.Browsable(False)>
        Public Property Enabled() As Boolean
            Get
                Return _Enabled
            End Get

            Set(ByVal value As Boolean)
                _Enabled = value
            End Set
        End Property

        Private _TabName As String

        <Category("Measurement settings"),
            DisplayName("Measurement name"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specify the name of the measurement"),
            ComponentModel.Browsable(True)>
        Public Property TabName() As String
            Get
                Return _TabName
            End Get

            Set(ByVal value As String)
                _TabName = value
            End Set
        End Property
        <Obsolete>
        Private _SamplePompSpeed As Int32 = 166

        <Category("Measurement instrument settings"),
            DisplayName("Sample pump speed setting"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specify the sample pump speed setting"),
            ComponentModel.Browsable(False)>
        Public Property SamplePumpSpeedSetting() As Integer
            Get
                Return _actualSamplePumpSpeedSetting
            End Get
            Set(ByVal value As Integer)
                _actualSamplePumpSpeedSetting = value
            End Set
        End Property


        <Category("Measurement instrument settings"),
            DisplayName("Sample pump speed"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specifies the sample pump speed during the measurement in [μL/s]"),
            CytoSense.Data.DataBase.Attributes.Format("#0.0# μL/s"),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property SamplePompSpeed() As Double
            Get
                Return Math.Round(getFlowrate(_cytosettings), 2)
            End Get
        End Property

        <Obsolete>
        Private _configuredSamplePumpSpeed As Byte = 0

        <Category("Measurement instrument settings"),
            DisplayName("Configured sample pump speed setting"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("The configured sample pump speed setting value"),
            Browsable(False)>
        Public Property ConfiguredSamplePumpSpeedSetting As Integer
            Get
                Return _configuredSamplePumpSpeedSetting
            End Get

            Set(value As Integer)
                _configuredSamplePumpSpeedSetting = value
            End Set
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("Configured Sample pump speed"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("The Configured sample pump speed [μL/s]"),
            CytoSense.Data.DataBase.Attributes.Format("#0.0# μL/s"),
            Browsable(True)>
        Public ReadOnly Property ConfiguredSamplePompSpeed() As Double
            Get
                Return Math.Round(getFlowrate(ConfiguredSamplePumpSpeedSetting), 2)
            End Get
        End Property
        '
        ' New sample pump speed settings for the changes made for the XR. We store and control things
        ' a little different. The actual previous properties are now deprecated, but left in there
        ' so we can still load older files.
        Private _actualSamplePumpSpeedSetting As Integer            = Integer.MinValue
        Private _configuredSamplePumpSpeedSetting As Integer        = Integer.MinValue
        Private _configuredMinimalSamplePumpSpeedSetting As Integer = Integer.MinValue
        Private _limitParticleRate As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Limit particle rate"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Limit the maximum particle rate"),
            Browsable(True)>
        Public Property LimitParticleRate() As Boolean
            Get
                Return _limitParticleRate
            End Get

            Set(value As Boolean)
                _limitParticleRate = value
            End Set
        End Property

        Private _maxParticleRate As Integer = 5000

        <Category("Measurement instrument settings"),
            DisplayName("Maximum particle rate"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("The maximum particle rate we want during a measurement"),
            Browsable(True)>
        Public Property  MaxParticleRate As Integer 
            Get
                Return _maxParticleRate
            End Get

            Set(value As Integer)
                _maxParticleRate = value
            End Set
        End Property
        
        Private _enableMinimumAutoSpeed As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Enable minimum auto speed"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specify a minimum samplepump speed the auto adjustment cannot go below."),
            Browsable(True)>
        Public Property EnableMinimumAutoSpeed As Boolean
            Get 
                Return _enableMinimumAutoSpeed
            End Get

            Set(value As Boolean)
                _enableMinimumAutoSpeed = value
            End Set
        End Property

        <Obsolete>
        Private _minimumAutoSpeed As Byte = 255 'Aboluut minimum as default

        <Category("Measurement instrument settings"),
            DisplayName("Minimum auto speed byte"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("The minimum samplepump speed the auto adjustment cannot go below."),
            Browsable(False)>
        Public Property MinimumAutoSpeedSetting As Integer
            Get 
                Return _configuredMinimalSamplePumpSpeedSetting
            End Get

            Set(value As Integer)
                _configuredMinimalSamplePumpSpeedSetting = value
            End Set
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("Minimum auto speed"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("The minimum samplepump speed the auto adjustment cannot go below."),
            CytoSense.Data.DataBase.Attributes.Format("#0.0# μL/s"),
            Browsable(True)>
        Public ReadOnly Property MinimumAutoSpeed As Double
            Get 
                Return Math.Round(getFlowrate(_configuredMinimalSamplePumpSpeedSetting), 2)
            End Get
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("Trigger level"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specifies the trigger level during the measurement in [mV]. Note that the triggerlevel can have an offset of  up to 5 mV different for each channel."),
            ComponentModel.Browsable(True),
            CytoSense.Data.DataBase.Attributes.Format("#0.### \mV")>
        Public ReadOnly Property TriggerLevel1e() As Double
            Get
                Dim trig = getTriggerLevel1mV(_cytosettings)

                If Not CytoSettings.hasFJDataElectronics Then
                    trig = Math.Round(getTriggerLevel1mV(_cytosettings))
                End If

                Return trig
            End Get
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("Trigger channel"),
            ComponentModel.ReadOnly(True),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property TriggerChannel() As String
            Get
                Dim res As String = ""
                For i As Integer = 0 To _cytosettings.channels.Length - 1
                    If _TriggerChannelArray(i) Then
                        If res = "" Then
                            res = _cytosettings.channels(i).name
                        Else
                            res = res & ", " & _cytosettings.channels(i).name
                        End If
                    End If
                Next
                Return res
            End Get
        End Property

        Public Sub setTriggerLevelFJ(c As CytoSense.CytoSettings.channel, value As Integer)
            TriggerLevelsFJ(_cytosettings.getChannelIndex(c.name)) = value
        End Sub

        Private _TriggerLevelsFJ() As Integer

        <Category("Measurement instrument settings"),
            DisplayName("Trigger levels"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the trigger byte value level during the measurement for FJ electronics. [ 0-5000 mv]"),
            ComponentModel.Browsable(False)>
        Public Property TriggerLevelsFJ() As Integer()
            Get
                If Object.Equals(Nothing, _TriggerLevelsFJ) Then
                    ReDim _TriggerLevelsFJ(_cytosettings.channels.Length - 1)
                End If
                If _TriggerLevelsFJ.Count <> _cytosettings.channels.Length Then
                    ReDim Preserve _TriggerLevelsFJ(_cytosettings.channels.Length - 1)
                End If
                Return _TriggerLevelsFJ
            End Get

            Set(ByVal value As Integer())
                _TriggerLevelsFJ = value
            End Set
        End Property

        Private _channelDataConversion() As LogConversion

        <ComponentModel.Browsable(False)> _
        Public Property ChannelDataConversion() As LogConversion()
            Get
                Return _channelDataConversion
            End Get

            Set(value As LogConversion())
                If value.Length <> _cytosettings.channels.Length Then
                    Throw New ArgumentException(String.Format("Invalid number of dataconversion settings, numChannel={0}, numConversions={1}", _cytosettings.channels.Length, value.Length))
                End If
                _channelDataConversion = value
            End Set
        End Property

        Private _TriggerLevel1e As Int32 = 50

        <Category("Measurement instrument settings"),
            DisplayName("Trigger level 1st grabber"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the trigger byte value level during the measurement for the bottom grabber."),
            ComponentModel.Browsable(False)>
        Public Property TriggerLevelByte1e() As Int32
            Get
                Return _TriggerLevel1e
            End Get

            Set(ByVal value As Int32)
                _TriggerLevel1e = value
            End Set
        End Property

        Private _TriggerLevel2e As Int32 = 50

        <Category("Measurement instrument settings"),
            DisplayName("Trigger level 2nd grabber"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the trigger byte value level during the measurement for the top grabber."),
            ComponentModel.Browsable(False)>
        Public Property TriggerLevelByte2e() As Int32
            Get
                Return _TriggerLevel2e
            End Get

            Set(ByVal value As Int32)
                _TriggerLevel2e = value
            End Set
        End Property

        Private _StopafterTimertext As Int32 = 180

        <Category("Measurement settings"),
            DisplayName("Stop after seconds"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the maximum time after which a measurement will be stopped."),
            CytoSense.Data.DataBase.Attributes.Format("#0 \s"),
            ComponentModel.Browsable(True)>
        Public Property StopafterTimertext() As Int32
            Get
                Return _StopafterTimertext
            End Get

            Set(ByVal value As Int32)
                _StopafterTimertext = value
            End Set
        End Property

        Private _MaxNumberParticleText As Int32 = 0

        <Category("Measurement settings"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the maximum number of downloaded particles after which a measurement will be stopped. Zero means no limit"),
            ComponentModel.Browsable(False)>
        Public Property MaxNumberParticleText() As Int32
            Get
                Return _MaxNumberParticleText
            End Get

            Set(ByVal value As Int32)
                _MaxNumberParticleText = value
            End Set
        End Property

        Private _StopAtParticles As Boolean = False

        <Category("Measurement settings"),
            DisplayName("Enable max particles"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Enables stopping at a certain number of particles"),
            ComponentModel.Browsable(False)>
        Public Property StopAtParticles() As Boolean
            Get
                Return _StopAtParticles
            End Get

            Set(ByVal value As Boolean)
                _StopAtParticles = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Stop at # of particles"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specifies the maximum number of downloaded particles after which a measurement will be stopped."),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property StopAtParticlesString() As String
            Get
                If _StopAtParticles Then
                    Return _MaxNumberParticleText.ToString()
                Else
                    Return "-"
                End If
            End Get
        End Property

        Private _MaxAnalysedVolume As Double = 0

        <Category("Measurement settings"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the maximum amount of analysed volume after which a measurement will be stopped."),
            CytoSense.Data.DataBase.Attributes.Format("#0 μL"),
            ComponentModel.Browsable(False)>
        Public Property MaxAnalysedVolume() As Double
            Get
                Return _MaxAnalysedVolume
            End Get

            Set(ByVal value As Double)
                _MaxAnalysedVolume = value
            End Set
        End Property

        Private _StopAtAnalysedVolume As Boolean = False

        <Category("Measurement settings"),
            DisplayName("Enable max analysed volume"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Enables stopping at a amount of analysed volume after which a measurement will be stopped"),
            ComponentModel.Browsable(False)>
        Public Property StopAtAnalysedVolume() As Boolean
            Get
                Return _StopAtAnalysedVolume
            End Get

            Set(ByVal value As Boolean)
                _StopAtAnalysedVolume = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Stop at analysed volume"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specifies the maximum amount of analysed volume after which a measurement will be stopped."),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property StopAtAnalysedVolumeString() As String
            Get
                If _StopAtAnalysedVolume Then
                    Return _MaxAnalysedVolume.ToString("#0 μL")
                Else
                    Return "-"
                End If
            End Get
        End Property

        Private _MaxPumpedVolume As Double = 0

        <Category("Measurement settings"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Maximum pumped volume"),
            ComponentModel.Browsable(False)>
        Public Property MaxPumpedVolume() As Double
            Get
                Return _MaxPumpedVolume
            End Get

            Set(ByVal value As Double)
                _MaxPumpedVolume = value
            End Set
        End Property

        Private _StopAtPumpedVolume As Boolean = False

        <Category("Measurement settings"),
            DisplayName("Enable max pumped volume"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Enables stopping at a certain pumped volume"),
            ComponentModel.Browsable(False)>
        Public Property StopAtPumpedVolume() As Boolean
            Get
                Return _StopAtPumpedVolume
            End Get

            Set(ByVal value As Boolean)
                _StopAtPumpedVolume = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Stop at pumped volume"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the maximum amount of pumped volume after which a measurement will be stopped."),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property StopAtPumpedVolumeString() As String
            Get
                If _StopAtPumpedVolume Then
                    Return _MaxPumpedVolume.ToString("#0 μL")
                Else
                    Return "-"
                End If
            End Get
        End Property

        Private _MaxNumberFotoText As Int32 = 250

        <Category("Measurement settings"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the maximum number of downloaded images after which a measurement will be stopped."),
            ComponentModel.Browsable(False)>
        Public Property MaxNumberFotoText() As Int32
            Get
                Return _MaxNumberFotoText
            End Get

            Set(ByVal value As Int32)
                If value < 1 And IIFCheck Then
                    value = 1
                End If
                _MaxNumberFotoText = value
            End Set
        End Property

        Private _StopAtFotos As Boolean = True

        <Category("Measurement settings"),
            DisplayName("Enable max images"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Enables stopping at a certain number of images"),
            ComponentModel.Browsable(False)>
        Public Property StopAtFotos() As Boolean
            Get
                Return _StopAtFotos
            End Get

            Set(ByVal value As Boolean)
                _StopAtFotos = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Stop at # images"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Specifies the maximum number of downloaded images after which a measurement will be stopped."),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property StopAtFotosString() As String
            Get
                If _StopAtFotos Then
                    Return _MaxNumberFotoText.ToString()
                Else
                    Return "-"
                End If
            End Get
        End Property

        Private _StopAfterTime As Boolean = True 'Depreciated

        ''' <summary>
        ''' Deprecated
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Measurement settings"),
            DisplayName("Stop after time."),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute("Enables stopping after a certain amount of time. Depreciated!"),
            ComponentModel.Browsable(False)>
        Public ReadOnly Property StopAfterTime() As Boolean
            Get
                Return True
            End Get
        End Property

        Private _SaveTextbox As String

        <Category("Misc"),
            DisplayName("Filename"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies filename to which the measurement data will be saved."),
            ComponentModel.Browsable(False)>
        Public Property SaveTextbox() As String 'Not browsable for the DB-function, do not change! (Or change the display name too)
            Get
                Return _SaveTextbox
            End Get

            Set(ByVal value As String)
                _SaveTextbox = value
            End Set
        End Property

        Private _FlushCheck As Boolean = False

        <Category("Measurement settings"),
            DisplayName("Force Flush"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Check only if a separate flush should always be done for this measurement."),
            ComponentModel.Browsable(True)>
        Public Property FlushCheck() As Boolean
            Get
                Return _FlushCheck
            End Get

            Set(ByVal value As Boolean)
                _FlushCheck = value
            End Set
        End Property

        Private _TellCheck As Boolean = True
        <Category("Misc"),
            DisplayName("Speak when ready"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Check if a computerized voice should say when the measurement is done."),
            ComponentModel.Browsable(False)>
        Public Property TellCheck() As Boolean
            Get
                Return _StopAfterTime
            End Get

            Set(ByVal value As Boolean)
                _StopAfterTime = value
            End Set
        End Property

        Private _IIFCheck As Boolean = False

        <Category("Image in Flow"),
            DisplayName("Enabled IIF"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Check if the Image in Flow module should be enabled for this measurement."),
            ComponentModel.Browsable(True)>
        Public Property IIFCheck() As Boolean
            Get
                Return _IIFCheck
            End Get

            Set(ByVal value As Boolean)
                _IIFCheck = value
            End Set
        End Property

        Private _IIFFileLocation As String 'moet vervangen worden door IIFPARAMETERS !

        <Category("Image in Flow"),
            DisplayName("IIF parameter file"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Select an Image in Flow parameter file created from CytoClus. Parameters from this file will be sent to the IIF module, instructing it which particles should be photographed"),
            ComponentModel.Browsable(True)>
        Public Property IIFFileLocation() As String
            Get
                Return _IIFFileLocation
            End Get

            Set(ByVal value As String)
                _IIFFileLocation = value
            End Set
        End Property

        <Obsolete()>
        Private _GVCheck As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Enabled GV module"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Check if the GV module should be enabled for this measurement."),
            ComponentModel.Browsable(False)>
        Public ReadOnly Property GVCheck() As Boolean
            Get
#Disable Warning BC40008
                Return _GVCheck
#Enable Warning BC40008
            End Get
        End Property

        Private _BSTCheck As Boolean = False

        ''' <summary>
        ''' Property is readonly now, it remains for backwards compatibility.  The BST system was redesigned,
        ''' so this setting is no longer used.  It is replaced with configuration that is independent of the
        ''' measurement. (For biocide), and with a new option that does a beads measurement only.
        ''' </summary>
        ''' <remarks></remarks>
        <Category("Measurement instrument settings"),
              DisplayName("Enabled BST module"),
              ComponentModel.ReadOnly(True),
              DescriptionAttribute("Check if the BST module should be enabled for this measurement."),
              ComponentModel.Browsable(True)>
        Public ReadOnly Property BSTCheck() As Boolean
            Get
                Return _BSTCheck
            End Get
        End Property

        Private _repeat As Int32 = 0

        <Category("Misc"),
            DisplayName("# of Cycles"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the number of times this measurement will be run."),
            ComponentModel.Browsable(False)>
        Public Property repeat() As Int32
            Get
                Return _repeat
            End Get

            Set(ByVal value As Int32)
                _repeat = value
            End Set
        End Property

        Private _blockSize As BlockSizes = BlockSizes.b4k

        <Category("Measurement instrument settings"),
            DisplayName("Block size enum"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the size of the buffer. Smal buffer means higher user interface refresh rate. Large buffer size is faster"),
            ComponentModel.Browsable(False)>
        Public Property BlockSize As BlockSizes
            Get
                Return _blockSize
            End Get

            Set(ByVal value As BlockSizes)
                _blockSize = value
            End Set
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("Block size"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Specifies the size of the buffer. Smal buffer means higher user interface refresh rate. Large buffer size is faster"),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property BlockSize_str As String 'special formatted output for database
            Get
                If _cytosettings.hasFJDataElectronics Then
                    Return "-"
                ElseIf _autoBlockSize Then
                    Return "Auto"
                Else
                    Return _blockSize.ToString.Trim(CChar("b"))
                End If
            End Get
        End Property


        Private _PMTlevels As Byte()

        <Category("Measurement instrument settings"),
            DisplayName("Raw PMT bytes"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property PMTlevels As Byte()
            Get
                Return _PMTlevels
            End Get

            Set(value As Byte())
                _PMTlevels = value
            End Set
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("PMT levels (short)"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public ReadOnly Property PMTlevelsString As String
            Get
                Dim sb As New StringBuilder
                sb.Append("[")
                Dim first As Boolean = True
                For i = 0 To _cytosettings.channels.Length - 1
                    If _cytosettings.channels(i).hasI2CPMTLevel Then
                        If Not first Then
                            sb.Append(",")
                        Else
                            first = False
                        End If
                        Dim pmtGain = _cytosettings.channels(i).ByteLevel2PmtGain(PMTlevels(_cytosettings.channels(i).PMTLevel_id), _cytosettings.HighVoltageType)
                        sb.AppendFormat("{0} ({1}x)", PMTlevels(_cytosettings.channels(i).PMTLevel_id), CInt(pmtGain))
                    End If
                Next
                sb.Append("]")
                Return sb.ToString()
            End Get
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("PMT levels"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property PMTlevels_str As String
            Get
                Dim res As String = ""
                For i = 0 To _cytosettings.channels.Length - 1
                    If _cytosettings.channels(i).hasI2CPMTLevel Then
                        Dim pmtGain = _cytosettings.channels(i).ByteLevel2PmtGain(PMTlevels(_cytosettings.channels(i).PMTLevel_id), _cytosettings.HighVoltageType)
                        Dim chanStr = String.Format("{0}: {1} ({2}x)", _cytosettings.channels(i).name, PMTlevels(_cytosettings.channels(i).PMTLevel_id), CInt(pmtGain))
                        res &= chanStr & ", "
                    End If
                Next

                If res = "" Then
                    res = "-"
                Else
                    res = res.Substring(0, res.Length - 2) 'remove comma
                End If
                Return res
            End Get
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("PMT preset"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property PMTLevelPreset As String
            Get
                If _PMTlevels Is Nothing Then
                    Return "-"
                End If
                Dim res(2) As Boolean
                res(0) = True
                res(1) = True
                res(2) = True

                For i = 0 To PMTlevels.Length - 1
                    If PMTlevels(i) <> _cytosettings.CytoUSBSettings.PMTLevelPresets(0)(i) Then
                        res(0) = False
                    End If
                    If PMTlevels(i) <> _cytosettings.CytoUSBSettings.PMTLevelPresets(1)(i) Then
                        res(1) = False
                    End If
                    If PMTlevels(i) <> _cytosettings.CytoUSBSettings.PMTLevelPresets(2)(i) Then
                        res(2) = False
                    End If
                Next

                If res(0) = True Then
                    Return "Low sensitivity"
                End If
                If res(1) = True Then
                    Return "Medium sensitivity"
                End If
                If res(2) = True Then
                    Return "High sensitivity"
                End If

                Return "User sensitivity"
            End Get
        End Property

		Private _subfolder As String = ""

        <Category("File"),
            DisplayName("subfolder"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property Subfolder As String
            Get
                Return _subfolder
            End Get

            Set(ByVal value As String)
                _subfolder = value
            End Set
        End Property

        Private _userRemarks As String

        <Category("File"),
            DisplayName("User remarks"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property UserRemarks As String
            Get
                Return _userRemarks
            End Get

            Set(ByVal value As String)
                _userRemarks = value
            End Set
        End Property

        Private _autoBlockSize As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Automatic block size"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property AutoBlockSize As Boolean
            Get
                Return _autoBlockSize
            End Get
            Set(ByVal value As Boolean)
                _autoBlockSize = value
            End Set
        End Property

        Private _maxTimeOut As Double = 10

        <Category("Measurement settings"),
            DisplayName("Max time out time"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property MaxTimeOut As Double
            Get
                Return _maxTimeOut
            End Get

            Set(ByVal value As Double)
                _maxTimeOut = value
            End Set
        End Property

        Private _adaptiveMaxTimeOut As Boolean = True

        <Category("Measurement settings"),
            DisplayName("Adaptive max time out"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property AdaptiveMaxTimeOut As Boolean
            Get
                Return _adaptiveMaxTimeOut
            End Get

            Set(ByVal value As Boolean)
                _adaptiveMaxTimeOut = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Max time out"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property MaxTimeOut_str As String
            Get
                If _adaptiveMaxTimeOut Then
                    Return "Auto"
                End If
                Return _maxTimeOut & " s"
            End Get
        End Property

        Private _seperateConcentration As Boolean = True

        <Category("Measurement settings"),
            DisplayName("Pre-concentration"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property SeperateConcentration As Boolean
            Get
                Return _seperateConcentration
            End Get

            Set(ByVal value As Boolean)
                _seperateConcentration = value
            End Set
        End Property

        Private _adaptiveSamplePumpSpeed As Boolean = False

        <Category("Measurement settings"),
            DisplayName("Adaptive sample pump speed"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property AdaptiveSamplePumpSpeed As Boolean
            Get
                Return _adaptiveSamplePumpSpeed
            End Get

            Set(ByVal value As Boolean)
                _adaptiveSamplePumpSpeed = value
            End Set
        End Property

        Private _measureNoiseLevels As Boolean = True

        <Category("Measurement settings"),
            DisplayName("Measure noise levels"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property MeasureNoiseLevels As Boolean
            Get
                Return _measureNoiseLevels
            End Get

            Set(ByVal value As Boolean)
                _measureNoiseLevels = value
            End Set
        End Property

        Private _calibrateCamera As Boolean = True

        <Category("Measurement settings"),
            DisplayName("Calibrate camera"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True), Obsolete()>
        Public Property CalibrateCamera As Boolean
            Get
                Return _calibrateCamera
            End Get

            Set(ByVal value As Boolean)
                _calibrateCamera = value
            End Set
        End Property

        Private _TriggerChannelArray() As Boolean

        <Category("Measurement instrument settings"),
            DisplayName("Trigger channels"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("List of trigger channels"),
            ComponentModel.Browsable(False)>
        Public Property TriggerChannelArray() As Boolean()
            Get
                Return _TriggerChannelArray
            End Get

            Set(ByVal value As Boolean())
                _TriggerChannelArray = value
            End Set
        End Property

        Private _SelectableHighLowChannelsOnHighArray() As Boolean

        <Category("Measurement instrument settings"),
            DisplayName("Sensitivity"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("High/Low selection"),
            ComponentModel.Browsable(False)>
        Public Property SelectableHighLowChannelsOnHighArray() As Boolean()
            Get
                Return _SelectableHighLowChannelsOnHighArray
            End Get

            Set(ByVal value As Boolean())
                _SelectableHighLowChannelsOnHighArray = value
            End Set
        End Property

        Private _LowCheck() As Boolean

        <Category("Measurement instrument settings"),
            DisplayName("Low Checks"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Low checks"),
            ComponentModel.Browsable(False)>
        Public Property LowCheck() As Boolean()
            Get
                Return _LowCheck
            End Get

            Set(ByVal value As Boolean())
                _LowCheck = value
            End Set
        End Property

        Private _IIFParameters As IIFParameters

        <Category("Image in Flow"),
            DisplayName("IIF parameters"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFParameters() As IIFParameters
            Get
                Return _IIFParameters
            End Get

            Set(ByVal value As IIFParameters)
                _IIFParameters = value
            End Set
        End Property

        <Category("Image in Flow"),
            DisplayName("Selected IIF mode"),
            ComponentModel.ReadOnly(True),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public readonly Property SelectedIifMode As String
            Get
                Dim bool = Nothing
                If IIFuseTargetAll Then
                    return "Target All IIF"
                ElseIf IIFuseSmartGrid Then
                    Return "Smart Grid IIF"
                ElseIf IIFuseTargetRange Then
                    Return "Target Range IIF"
                ElseIf IIFUseSetDefintionSelector Then
                    Return "Set Definition IIF"
                Else 
                    Return "Unknown IIF setting"
                End If
                    
            End Get
        End Property

        Private _IIFuseTargetAll As Boolean = False

        ''' <summary>
        ''' Set to true when the target "Target All" is enabled in New electronics. (PIC IIF)
        ''' In machines with old electronics(DSP) this flag is ignored.  That one only does
        ''' target range, or smartgrid, never both.
        ''' </summary>
        <Category("Image in Flow"),
            DisplayName("Target All IIF"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFuseTargetAll As Boolean
            Get
                Return _IIFuseTargetAll
            End Get

            Set(ByVal value As Boolean)
                _IIFuseTargetAll = value
            End Set
        End Property


        Private _IIFuseTargetRange As Boolean = False

        ''' <summary>
        ''' Set to true when the target range for IIF is enabled in New electronics. (PIC IIF)
        ''' In machines with old electronics(DSP) this flag is ignored.  That one only does
        ''' target range, or smartgrid, never both.
        ''' </summary>
        <Category("Image in Flow"),
            DisplayName("Target Range IIF"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFuseTargetRange As Boolean
            Get
                Return _IIFuseTargetRange
            End Get

            Set(ByVal value As Boolean)
                _IIFuseTargetRange = value
            End Set
        End Property

        Private _IIFuseSmartGrid As Boolean = True

        <Category("Image in Flow"),
            DisplayName("Smart grid IIF"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFuseSmartGrid As Boolean
            Get
                Return _IIFuseSmartGrid
            End Get

            Set(ByVal value As Boolean)
                _IIFuseSmartGrid = value
            End Set
        End Property

        <Category("Image in Flow"),
            DisplayName("IIF smart grid"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property SmartGrid_str As String
            Get
                If Not _IIFuseSmartGrid OrElse Not _IIFCheck Then ' If no image in flow configured, then also do not display any smartgrid settings.
                    Return ""
                End If
                If IifSmartGridChannelIds Is Nothing Then
                    Return "FL-Red"
                Else
                    Dim s As String = ""
                    For i = 0 To IifSmartGridChannelIds.Count - 1
                        ' There are files from the CS-2015-68 machine, that has only 5 channels, but somehow the smartgrid channel
                        ' ID is set to 6.  And we have no channel 6, so if we find these files then we correct it. to
                        ' the number of the FlRed Channel. 
                        Dim idx = IifSmartGridChannelIds(i)
                        If idx > _cytosettings.channels.Length - 1 AndAlso CytoSettings.SerialNumber = "CS-2015-68" Then
                            idx = _cytosettings.channels.Length - 1
                        End If
                        s &= _cytosettings.channels(idx).name & ", "
                    Next
                    If s.Length > 0 Then
                        s = s.Substring(0, s.Length - 2)
                    End If
                    Return s
                End If
            End Get
        End Property

        '
        ' The channels to use for smartgrid, must be one of the lower 6 channels 
        ' available in the CytoSense. In the old versions this must allways be 
        ' number 6 (Fl-Red in the CytoSense in Eijsden) I think. The new options is supported 
        ' by DSP firmware version since ...
        ' 
        Private _iifSmartgridChannelIds As New List(Of Integer)()

        <Category("Image in Flow"),
            DisplayName("Smart grid channels"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IifSmartGridChannelIds As List(Of Integer)
            Get
                Return _iifSmartgridChannelIds
            End Get
            Set(value As List(Of Integer))
                _iifSmartgridChannelIds = value
            End Set
        End Property

        Private _IIFuseFreeFormSelection As Boolean = False

        <Category("Image in Flow"),
            DisplayName("IIF freeform"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFuseFreeFormSelection As Boolean
            Get
                Return _IIFuseFreeFormSelection
            End Get

            Set(ByVal value As Boolean)
                _IIFuseFreeFormSelection = value
            End Set
        End Property

        Private _IIFFreeFormSelection As IIFFreeFormSelection

        <Category("Image in Flow"),
            DisplayName("IIF free form selection"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property IIFFreeFormSelection As IIFFreeFormSelection
            Get
                Return _IIFFreeFormSelection
            End Get

            Set(ByVal value As IIFFreeFormSelection)
                _IIFFreeFormSelection = value
            End Set
        End Property

        Private _IIFRestrictFwsRange As Boolean = False

        <Category("Image in Flow"),
            DisplayName("IIF Restrict Fws Range"),
            Description("Only take pictures in a restricted range of FWS ratios"),
            ComponentModel.Browsable(True)>
        Public Property IIFRestrictFwsRange As Boolean
            Get
                Return _IIFRestrictFwsRange
            End Get

            Set(ByVal value As Boolean)
                _IIFRestrictFwsRange = value
            End Set
        End Property

        Private _IIFFwsRatioMin As Double = 0.1

        <Category("Image in Flow"),
            DisplayName("IIF FWS Ratio Min"),
            Description("Minimum value for FWS Ratio"),
            ComponentModel.Browsable(True)>
        Public Property IIFFwsRatioMin As Double
            Get
                Return _IIFFwsRatioMin
            End Get

            Set(ByVal value As Double)
                _IIFFwsRatioMin = value
            End Set
        End Property

        Private _IIFFwsRatioMax As Double = 10.0

        <Category("Image in Flow"),
            DisplayName("IIF FWS Ratio Max"),
            Description("Maximum value for FWS Ratio"),
            ComponentModel.Browsable(True)>
        Public Property IIFFwsRatioMax As Double
            Get
                Return _IIFFwsRatioMax
            End Get

            Set(ByVal value As Double)
                _IIFFwsRatioMax = value
            End Set
        End Property

        Private _IIFFwsRatioRangeCalibration As Boolean = False

        <Obsolete()>
        <Category("Image in Flow"),
            DisplayName("FwsRatio Calibration"),
            Description("A special measurement to determin the Ratio to use for taking pictures."),
            ComponentModel.Browsable(True)>
        Public Property IIFFwsRatioRangeCalibration As Boolean
            Get
                Return _IIFFwsRatioRangeCalibration
            End Get

            Set(ByVal value As Boolean)
                _IIFFwsRatioRangeCalibration = value
            End Set
        End Property

        Private _IIFPhotographLargeParticles As Boolean

        <Category("Image in Flow"),
            DisplayName("Photograph Large Particles"),
            Description("Always take pictures of large particles (to large to analyse)"),
            ComponentModel.Browsable(True)>
        Public Property IIFPhotographLargeParticles As Boolean
            Get
                Return _IIFPhotographLargeParticles
            End Get

            Set(value As Boolean)
                _IIFPhotographLargeParticles = value
            End Set
        End Property


        Private _iifUseSetDefinitionSelector As Boolean = False
        <Category("Image in Flow"),
            DisplayName("Use Set Definition"),
            Description("Use a set defintition to decide which particles to image."),
            ComponentModel.Browsable(false)>
        Public Property IIFUseSetDefintionSelector As Boolean
            Get
                Return _iifUseSetDefinitionSelector
            End Get
            Set(value As Boolean)
                _iifUseSetDefinitionSelector = value
            End Set
        End Property


        Private _iifSetDefinitionXml As String = ""
        <Category("Image in Flow"),
            DisplayName("Set Definition XML"),
            Description("XML containing the set definitions."),
            ComponentModel.Browsable(false)>
        Public Property IIFSetDefintionXml As String
            Get
                Return _iifSetDefinitionXml
            End Get
            Set(value As String)
                _iifSetDefinitionXml = value
            End Set
        End Property


        ' Original file name (full path)
        Private _iifSetDefinitionFilename As String
        <Category("Image in Flow"),
            DisplayName("Set Definition filename"),
            Description("Filename of the set definition."),
            ComponentModel.Browsable(false)>
        Public Property IIFSetDefintionFileName As String
            Get
                Return _iifSetDefinitionFilename
            End Get
            Set(value As String)
                _iifSetDefinitionFilename = value
            End Set
        End Property



        Private _iifSetSelectionInfo As List(Of IifSetSelectionInfo) = Nothing
        <Category("Image in Flow"),
            DisplayName("SelectionInfo"),
            Description("Info on selected sets."),
            ComponentModel.Browsable(False)>
        Public Property IIFSetSelectionInfo As List(Of IifSetSelectionInfo)
            Get
                Return _iifSetSelectionInfo
            End Get
            Set(value As List(Of IifSetSelectionInfo))
                _iifSetSelectionInfo = value
            End Set
        End Property

        Private _IIFRoiName As String
        <Category("Measurement settings"),
            DisplayName("ROI Name"),
            Description("The name of the region of interrest used for making mages."),
            ComponentModel.Browsable(True)>
        Public Property IIFRoiName As String
            Get
                Return _IIFRoiName
            End Get
            Set(value As String)
                _IIFRoiName = value
            End Set
        End Property


        <Obsolete>
        Private _multiSampler As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Multisampler enabled"),
            Description(""),
            Browsable(True)>
        Public ReadOnly Property MultiSampler As Boolean
            Get
                Return _cytosettings.EnableMultiSampler
            End Get
        End Property

        Private _MultiSamplerValve As Integer

        <Category("Measurement instrument settings"),
            DisplayName("Multisampler valve"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property MultiSamplerValve As Integer
            Get
                Return _MultiSamplerValve
            End Get

            Set(value As Integer)
                _MultiSamplerValve = value
            End Set
        End Property

        Private _multiSamplerPrime As Boolean
        Private _multiSamplerClean As Boolean

        <Category("Measurement instrument settings"),
            DisplayName("MultiSampler prime"),
            Description(""),
            Browsable(True)>
        Public Property MultiSamplerPrime As Boolean
            Get
                Return _multiSamplerPrime
            End Get

            Set(value As Boolean)
                _multiSamplerPrime = value
            End Set
        End Property

        <Category("Measurement instrument settings"),
            DisplayName("MultiSampler clean"),
            Description(""),
            Browsable(True)>
        Public Property MultiSamplerClean As Boolean
            Get
                Return _multiSamplerClean
            End Get

            Set(value As Boolean)
                _multiSamplerClean = value
            End Set
        End Property

        Private _cytoSelector As Boolean = False

        <Category("Measurement instrument settings"),
            DisplayName("Cytoselector enabled"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property CytoSelector As Boolean
            Get
                Return _cytoSelector
            End Get

            Set(value As Boolean)
                _cytoSelector = value
            End Set
        End Property

        Private _cytoSelectorEnabledTraysIDs As New List(Of Integer)

        <Category("Measurement instrument settings"),
            DisplayName("Cytoselector trays"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public ReadOnly Property CytoSelectorEnabledTraysIDs As List(Of Integer)
            Get
                Return _cytoSelectorEnabledTraysIDs
            End Get
        End Property

        Private _injectorFocus As Double

        <Category("Measurement instrument settings"),
            DisplayName("Injector focus"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property InjectorFocus As Double
            Get
                Return _injectorFocus
            End Get

            Set(value As Double)
                _injectorFocus = value
            End Set
        End Property

        Private _injectorPosition As Double

        <Category("Measurement instrument settings"),
            DisplayName("Injector position"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property InjectorPosition As Double
            Get
                Return _injectorPosition
            End Get

            Set(value As Double)
                _injectorPosition = value
            End Set
        End Property

#Region "Staining Module Settings"
        Private _useStainingModule As Boolean = False
        <Category("Staining Module"), DisplayName("Inject Stain"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public Property UseStainingModule As Boolean
            Get
                Return _useStainingModule
            End Get
            Set(value As Boolean)
                _useStainingModule = value
            End Set
        End Property

        Private _cleanStainingModuleAfterwards As Boolean
        <Category("Staining Module"), DisplayName("Clean Staining Module"), DescriptionAttribute("Clean the staining module after the measurement is done."), ComponentModel.Browsable(True)>
        Public Property CleanStainingModuleAfterwards As Boolean
        Get
                Return _cleanStainingModuleAfterwards
        End Get
            Set(value As Boolean)
                _cleanStainingModuleAfterwards = value
            End Set
        End Property

        Private _cleanStainingModuleAfterwardsIncludeSamplePump As Boolean
        <Category("Staining Module"), DisplayName("Include Samplepump In Clean"), DescriptionAttribute("After clean the stianing module file the sample pump with cleaning fluid as well."), ComponentModel.Browsable(True)>
        Public Property CleanStainingModuleAfterwardsIncludeSamplePump As Boolean
        Get
                Return _cleanStainingModuleAfterwardsIncludeSamplePump
        End Get
            Set(value As Boolean)
                _cleanStainingModuleAfterwardsIncludeSamplePump = value
            End Set
        End Property



        Private _includeUnstainedMeasurement As Boolean = False
        <Category("Staining Module"), DisplayName("Include unstained measurement"), DescriptionAttribute("Run an unstained measurement while the stained sample is incubating,"), ComponentModel.Browsable(True)>
        Public Property IncludeUnstainedMeasurement As Boolean
            Get
                Return _includeUnstainedMeasurement
            End Get
            Set(value As Boolean)
                _includeUnstainedMeasurement = value
            End Set
        End Property

        Private _useExternalStaining As Boolean = False
        <Obsolete()> Public Property UseExternalStaining As Boolean
            Get
                Return _useExternalStaining
            End Get
            Set(value As Boolean)
                _useExternalStaining = value
            End Set
        End Property

        ' We cannot store per measurement override in the CytoSettings.StainingModule object, as the CytoSettings object
        ' is/can be shared between measurements.  So we really need to store the values here.
        ' For now this is dye amount and staining time.  We initialize them to negative values, so on first loading
        ' the defaults from the settings object will be used, but after that the values stored here will be
        ' used.
        Private _stainIncubateSeconds As Integer = -1
        <Category("Staining Module"), DisplayName("Stain Incubate Time"), Description("The time the stain should incubate before doing the measurement"), ComponentModel.Browsable(True)>
        Public Property StainIncubateSeconds As Integer
            Get
                Return _stainIncubateSeconds
            End Get
            Set(value As Integer)
                _stainIncubateSeconds = value
            End Set
        End Property
        
        Private _stainFlushWhileIncubate As Boolean = False
        <Category("Staining Module"), DisplayName("Flush Samplepump During Incubation"), Description("Flush the CytoSEnse while the stain is incubating"), ComponentModel.Browsable(True)>
        Public Property StainFlushWhileIncubate As Boolean
            Get
                Return _stainFlushWhileIncubate
            End Get
            Set(value As Boolean)
                _stainFlushWhileIncubate = value
            End Set
        End Property

        Private _stainDyeInjectAmount As Integer = -1
        <Obsolete> Public Property StainDyeInjectAmount As Integer
            Get
                Return _stainDyeInjectAmount
            End Get
            Set(value As Integer)
                _stainDyeInjectAmount = value
            End Set
        End Property

        Private _injectDye1 As Boolean = False
        <Category("Staining Module"), DisplayName("Inject Dye 1"), Description("Inject dye from unit 1."), ComponentModel.Browsable(True)>
        Public Property InjectDye1 As Boolean
            Get
                Return _injectDye1
            End Get
            Set(value As Boolean)
                _injectDye1 = value
            End Set
        End Property

        Private _dye1Amount As Integer = -1
        <Category("Staining Module"), DisplayName("Dye 1 Inject Amount"), Description("The amount of dye (microliters) from uni1 1 to inject in the sample."), ComponentModel.Browsable(True)>
        Public Property Dye1Amount As Integer
            Get
                Return _dye1Amount
            End Get
            Set(value As Integer)
                _dye1Amount = value
            End Set
        End Property

        Private _injectDye2 As Boolean = False
        <Category("Staining Module"), DisplayName("Inject Dye 2"), Description("Inject dye from unit 2."), ComponentModel.Browsable(True)>
        Public Property InjectDye2 As Boolean
            Get
                Return _injectDye2
            End Get
            Set(value As Boolean)
                _injectDye2 = value
            End Set
        End Property

        Private _dye2Amount As Integer = -1
        <Category("Staining Module"), DisplayName("Dye 2 Inject Amount"), Description("The amount of dye (microliters) from uni1 2 to inject in the sample."), ComponentModel.Browsable(True)>
        Public Property Dye2Amount As Integer
            Get
                Return _dye2Amount
            End Get
            Set(value As Integer)
                _dye2Amount = value
            End Set
        End Property

#End Region '"Staining Module Settings"


#Region "DPS Settings"
        Private _useDps As Boolean
        Private _depth  As Single
        Private _depthProfile As List(Of Single) = New List(Of Single)()
        <Category("Depth Profiling"), DisplayName("Use Depth Profiler"), DescriptionAttribute("Use the depth profiler for this measurement."), ComponentModel.Browsable(True)>
        Public Property UseDps As Boolean
            Get
                Return _useDps
            End Get
            Set(value As Boolean)
                _useDps = value
            End Set
        End Property

        <Category("Depth Profiling"), DisplayName("Depth"), DescriptionAttribute("The depth this measurement was taken at."), ComponentModel.Browsable(True)>
        Public Property Depth As Single
            Get
                Return _depth
            End Get
            Set(value As Single)
                _depth = value
            End Set
        End Property

        <ComponentModel.Browsable(False)>
        Public Property DepthProfile As List(Of Single)
            Get
                Return _depthProfile
            End Get
            Set(value As List(Of Single))
                _depthProfile = value
            End Set
        End Property

        <Category("Depth Profiling"), DisplayName("Depth Profile"), DescriptionAttribute("THe complete depth profile this measurement was scheduled for."), ComponentModel.Browsable(True)>
        Public ReadOnly Property DepthProfileString As String
        Get
            Return String.Join(",",  _depthProfile.Select( Function(depth) depth.ToString("0.00")))
        End Get
        End Property

#End Region '"DPS Settings"

        Private _segmentedDatafile As Boolean

        <Category("Measurement settings"),
            DisplayName("Segmented datafile format"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public Property SegmentedDatafile As Boolean
            Get
                Return _segmentedDatafile
            End Get

            Set(value As Boolean)
                _segmentedDatafile = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Enable export"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property EnableExport As Boolean

        Private _exportSettings As ExportSettings

        <Category("Measurement settings"),
            DisplayName("Export settings"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(False)>
        Public Property ExportSettings As ExportSettings
            Get
                If _exportSettings Is Nothing Then
                    _exportSettings = New ExportSettings()
                End If
                Return _exportSettings
            End Get

            Set(value As ExportSettings)
                _exportSettings = value
            End Set
        End Property

        ''' <summary>
        ''' Some old files do not have this setting on deserialization, and therefor we cannot set the
        ''' LogConversion Type array for these files.  The value gets set immediately after deserialization,
        ''' so we set the value when this property is set.
        ''' 
        ''' And for some we also need to update the actual number of entries in the Trigger Channel array.
        ''' I found this for some old NIOZ files in our test data set.
        ''' </summary>
        ''' <returns></returns>
        <Category("Other"),
            DisplayName("CytoSettings"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("CytoSettings"),
            ComponentModel.Browsable(False)>
        Public Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _cytosettings
            End Get
            Set(ByVal value As CytoSense.CytoSettings.CytoSenseSetting)
                _cytosettings = value
                If _channelDataConversion Is Nothing Then
                    ReDim _channelDataConversion(_cytosettings.channels.Length - 1)
                    For i = 0 To _cytosettings.channels.Length - 1
                        _channelDataConversion(i) = LogConversion.OriginalLog  ' This is the default for the hardware, so keep it the default here as well and for old hardware.
                    Next
                End If
                ' This happens when loading very old files, where this is called immediately after loading.
                If _actualSamplePumpSpeedSetting = Integer.MinValue Then ' Need to convert old settings.
    #Disable Warning BC40008 ' Type or member is obsolete
                    If _configuredSamplePumpSpeed = 0 Then  ' If no configured value is set, then use the current speed value.
                        _configuredSamplePumpSpeed = CByte(_SamplePompSpeed)
                    End If
                    If _cytosettings.hasaDCSamplePump Then
                        _actualSamplePumpSpeedSetting            = 256 - _SamplePompSpeed
                        _configuredSamplePumpSpeedSetting        = 256 - _configuredSamplePumpSpeed
                        _configuredMinimalSamplePumpSpeedSetting = 256 - _minimumAutoSpeed
                    Else ' Very old sample pump stepper, pre DC
                        _actualSamplePumpSpeedSetting            = _SamplePompSpeed
                        _configuredSamplePumpSpeedSetting        = _configuredSamplePumpSpeed
                        _configuredMinimalSamplePumpSpeedSetting = _minimumAutoSpeed
                    End If 
    #Enable Warning BC40008 ' Type or member is obsolete
                End If
                If _TriggerChannelArray.Length <> _cytosettings.channels.Length Then
                    ReDim Preserve _TriggerChannelArray(_cytosettings.channels.Length-1)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Returns buffer size for one channel
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Multiply by numberofchannels to get total memorysize for one block</remarks>
        Public Function getMemorySize() As Integer
            If _blockSize = CytoSense.MeasurementSettings.BlockSizes.b4k Then
                Return 4096
            ElseIf _blockSize = CytoSense.MeasurementSettings.BlockSizes.b16k Then
                Return 4 * 4096
            ElseIf _blockSize = CytoSense.MeasurementSettings.BlockSizes.b64k Then
                Return 16 * 4096
            ElseIf _blockSize = CytoSense.MeasurementSettings.BlockSizes.b256k Then
                Return 64 * 4096
            End If
            Return -1
        End Function

        ''' <summary>
        ''' Returns the index of the first trigger channel found.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>ONLY SUPPORT A SINGLE Trigger channel for now</remarks>
        Private Function TriggerChannelIdx(ByVal cytosettings As CytoSettings.CytoSenseSetting) As Integer
            For i As Integer = 0 To cytosettings.channels.Length - 1
                If _TriggerChannelArray(i) Then
                    Return i
                End If
            Next
            Return -1
        End Function

        ''' <summary>
        ''' Depreciated. Use getTriggerLevel1mV() instead
        ''' </summary>
        ''' <param name="cytosettings"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getTriggerLevel1mV(ByVal cytosettings As CytoSettings.CytoSenseSetting) As Double
            If cytosettings.hasFJDataElectronics Then
                Dim idx = TriggerChannelIdx(cytosettings)
                Return cytosettings.getTriggerLevel_mV(CUShort(TriggerLevelsFJ(idx)))
            Else
                Return cytosettings.getTriggerLevel_mV(CUShort(TriggerLevelByte1e))
            End If
        End Function

        Public Function getTriggerLevel1mV() As Double
            If Cytosettings.hasFJDataElectronics Then
                Dim idx = TriggerChannelIdx(cytosettings)
                Return Cytosettings.getTriggerLevel_mV(CUShort(TriggerLevelsFJ(idx)))
            Else
                Return Cytosettings.getTriggerLevel_mV(CUShort(TriggerLevelByte1e))
            End If
        End Function

        Public Function getFlowrate(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting) As Double
            Return cytosettings.getFlowSpeed(SamplePumpSpeedSetting)
        End Function

        Private Function getFlowrate(speedB As Integer) As Double
            Return _cytosettings.getFlowSpeed(speedB)
        End Function

        ''' <summary>
        ''' CytoSettings need to be set in order for this to work!
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getFlowrate() As Double
            Return _cytosettings.getFlowSpeed(SamplePumpSpeedSetting)
        End Function

        Public Function getSelectableHighLowChannelsonHigh(ByVal cytosettings As CytoSettings.CytoSenseSetting) As Boolean()
            Dim SelectableHighLowChannelsonHigh(cytosettings.channels.Length - 1) As Boolean

            For i As Int16 = 0 To CShort(cytosettings.channels.Length - 1)
                If cytosettings.channels(i).hasLowCheck And Not LowCheck(cytosettings.channels(i).lowcheckID) Then
                    SelectableHighLowChannelsonHigh(i) = cytosettings.channels(i).hasLowCheck And Not LowCheck(cytosettings.channels(i).lowcheckID)
                End If
            Next

            Return SelectableHighLowChannelsonHigh
        End Function

        Dim _preSiftSettings() As CytoSense.Data.PreSiftSetting

        <ComponentModel.Browsable(False)>
        Public Property PreSiftSettings() As CytoSense.Data.PreSiftSetting()
            Get
                Return _preSiftSettings
            End Get

            Set(ByVal value As CytoSense.Data.PreSiftSetting())
                _preSiftSettings = value
            End Set
        End Property

        Dim _smartTriggerCombinationType As SmartTriggerCombinationT

        <Browsable(False)>
        Public Property SmartTriggerCombinationType() As SmartTriggerCombinationT 
            Get
                Return _smartTriggerCombinationType
            End Get

            Set(ByVal value As SmartTriggerCombinationT)
                _smartTriggerCombinationType = value
            End Set
        End Property

        Dim _smartTriggerSettings() As SmartTriggerSettings

        <Browsable(False)>
        Public Property SmartTriggerSettings() As SmartTriggerSettings()
            Get
                Return _smartTriggerSettings
            End Get

            Set(ByVal value As SmartTriggerSettings())
                _smartTriggerSettings = value
            End Set
        End Property

        <Category("Measurement settings"),
            DisplayName("Smart Trigger"),
            DescriptionAttribute(""),
            ComponentModel.Browsable(True)>
        Public ReadOnly Property SmartTriggerSettingDescription() As String
            Get
                If SmartTriggeringEnabled And _smartTriggerSettings IsNot Nothing Then
                    If _smartTriggerSettings.Length = 1 Then
                        Return _smartTriggerSettings(0).ToString()
                    Else
                        Dim opStr = If (_smartTriggerCombinationType=SmartTriggerCombinationT.SmartTriggerAnd, "And", "Or")
                        Dim descr As New StringBuilder()
                        descr.AppendFormat("({0})", _smartTriggerSettings(0))
                        For i = 1 To _smartTriggerSettings.Length - 1
                            descr.AppendFormat(" {0} ({1})", opStr, _smartTriggerSettings(i))
                        Next
                        Return descr.ToString()
                    End If
                ElseIf SmartTriggeringEnabled And _preSiftSettings IsNot Nothing Then
                    ' legacy
                    Dim descr As New StringBuilder()
                     Dim opStr = If (_smartTriggerCombinationType=SmartTriggerCombinationT.SmartTriggerAnd, "And", "Or")
                    descr.AppendFormat("({0})", _preSiftSettings(0))
                    For i = 1 To _preSiftSettings.Length - 1
                        descr.AppendFormat(" {0} ({1})", opStr, _preSiftSettings(i))
                    Next
                    Return descr.ToString()
                Else
                    Return ""
                End If
            End Get
        End Property

        <ComponentModel.Browsable(False)>
        Public ReadOnly Property SmartTriggeringEnabled As Boolean
            Get
                Dim legacySmartTriggeringEnabled As Boolean = Not Object.Equals(Nothing, _preSiftSettings) AndAlso _preSiftSettings.Length > 0

                Return Not Object.Equals(Nothing, _smartTriggerSettings) AndAlso _smartTriggerSettings.Length > 0 Or legacySmartTriggeringEnabled
            End Get
        End Property

        Private _remoteSettings As New Remote.RemoteSettings(Remote.Mode.None)

        <ComponentModel.Browsable(False)>
        Public Property RemoteSettings As Remote.RemoteSettings
            Get
                If Object.Equals(Nothing, _remoteSettings) Then
                    _remoteSettings = New Remote.RemoteSettings
                End If
                Return _remoteSettings
            End Get

            Set(value As Remote.RemoteSettings)
                _remoteSettings = value
            End Set
        End Property

        Private _sendEmail As Boolean

        <ComponentModel.Browsable(False)>
        Public Property SendMail As Boolean
            Get
                Return _sendEmail
            End Get

            Set(value As Boolean)
                _sendEmail = value
            End Set
        End Property

        Private _emailAddress As String

        <ComponentModel.Browsable(False)>
        Public Property EmailAddress As String
            Get
                Return _emailAddress
            End Get

            Set(value As String)
                _emailAddress = value
            End Set
        End Property

        Private _ccWorkspaceFile As String

        <ComponentModel.Browsable(False)>
        Public Property CCWorkspace As String
            Get
                Return _ccWorkspaceFile
            End Get

            Set(value As String)
                _ccWorkspaceFile = value
            End Set
        End Property

        'for use in CytoUSB
        <NonSerialized()> Public Event NewProgress(ByVal p As Double)
        Public Sub RaiseProgressEvent(ByVal p As Double)
            If p > 100 Then
                p = 100
            End If
            If p < 0 Then
                p = 0
            End If
            RaiseEvent NewProgress(p)
        End Sub

        Public Sub RaiseRefreshEvent()
            RaiseEvent RefreshNeeded()
        End Sub

        ''' <summary>
        ''' Gets a list of the selected trigger levels.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getTriggerChannels() As List(Of CytoSense.CytoSettings.channel)
            Dim chns As New List(Of CytoSense.CytoSettings.channel)

            For i As Integer = 0 To CytoSettings.channels.Length - 1
                If TriggerChannelArray(i) Then
                    chns.Add(CytoSettings.channels(i))
                End If
            Next

            Return chns
        End Function

        Private _functionGeneratorSettings As FunctionGeneratorSettings

        <ComponentModel.Browsable(False)>
        Public Property FunctionGeneratorSettings As FunctionGeneratorSettings
            Get
                If _functionGeneratorSettings Is Nothing Then
                    _functionGeneratorSettings = New FunctionGeneratorSettings
                End If
                Return _functionGeneratorSettings
            End Get

            Set(value As FunctionGeneratorSettings)
                _functionGeneratorSettings = value
            End Set
        End Property

        ' New V10 electronics allows enable/disable of dc restore settings.
        ' Default is enabled of course.
        Private _useDcRestore As Boolean = True

        <ComponentModel.Browsable(False)>
        Public Property UseDcRestore As Boolean
            Get
                Return _useDcRestore
            End Get

            Set(value As Boolean)
                _useDcRestore = value
            End Set
        End Property

        ' New V10 electronics allows setting of USB Speed, for older versions this is not possible/relevant.
        ' Speed is in Megabits, default is 8

        Private _usbSpeed As Integer = 8

        <ComponentModel.Browsable(False)>
        Public Property UsbSpeed As Integer
            Get
                Return _usbSpeed
            End Get

            Set(value As Integer)
                _usbSpeed = value
            End Set
        End Property

        Private _isBeadsMeasurement As Boolean = False
        <Category("Measurement instrument settings"),
            DisplayName("Beads Measurement"),
            ComponentModel.ReadOnly(False),
            DescriptionAttribute("Is set when this was an automatic beads measurement."),
            ComponentModel.Browsable(True)>
        Public Property IsBeadsMeasurement() As Boolean
            Get
                Return _isBeadsMeasurement
            End Get

            Set(value As Boolean)
                _isBeadsMeasurement = value
            End Set
        End Property

        Function Clone() As Object Implements System.ICloneable.Clone
            Return New Measurement(Me)
        End Function

        Public Overrides Function ToString() As String
            Return TabName
        End Function
    End Class

    Public Enum StartedBy
        User
        ExternalTrigger
        Schedule
    End Enum

    Public Enum BlockSizes
        b4k = 0
        b16k = 2
        b64k = 4
        b256k = 6
    End Enum
End Namespace
