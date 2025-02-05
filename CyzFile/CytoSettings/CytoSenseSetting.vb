Imports System.ComponentModel
Imports System.Runtime.Serialization
Imports System.Threading

Namespace CytoSettings



    ''' <summary>
    ''' Possible states for the external filters used in the BST system
    ''' </summary>
    ''' <remarks>NOTE: Maximum of 16 states (Invalid + 15 others, as they will be stored in 4 bits in the EEPROM)</remarks>
    Public Enum ExternalFilterState As Byte
        Invalid = 0
        InUse = 1     ' Filter is OK, and currently in use
        Ready = 2     ' Filter is OK, ready to be used, but not currently used.
        Full = 3      ' Filter is Full, and should be replaced. (Could we have full AND in use?) Maybe we need that one as well
        Removed = 4   ' Filter is not Present 
        Off = 5       ' Filter taken out of active operation, still present.
        FullInUse = 6 ' Not sure we need this, but I think we do, if both filter are full we need to do something.
    End Enum


    Public Enum ExternalFilterWastePosition As Byte
        Invalid = 0
        Internal = 1
        External = 2
    End Enum

    Public Enum IoExpanderType As Byte
        UsbPrint    =  0
        IOE_Begin   = UsbPrint
        PCF8574     =  1
        PicPin      =  2
        DAC_5578    =  3
        PCA_9534    =  4
        MCP_3428    =  5
        EBST_V2     =  6  '// Connected to the EBST V2. Note: Only with Scpi controller.
        Reserved_7  =  7
        Reserved_8  =  8
        Reserved_9  =  9
        Reserved_10 = 10
        Reserved_11 = 11
        Reserved_12 = 12
        Reserved_13 = 13
        Reserved_14 = 14
        Disabled    = 15
        IOE_End     = 16  ' 1 after the last one.
    End Enum

    <Serializable()>
    Public Structure IoPinCfg
        Public Sub New( addr As Byte, pin As Byte, exp As IoExpanderType)
            I2cAddres = addr
            IoPin = pin
            Expander = exp
        End Sub

        Public I2cAddres As Byte ' 7 bit
        Public IoPin As Byte
        Public Expander As IoExpanderType
        Public Function AsStorageString() As String
            Return String.Format("{0},{1},{2}", I2cAddres, IoPin, CByte(Expander))
        End Function

        Public Shared Function FromStorageString(storage As String) As IoPinCfg
            Dim fields = storage.Split(","c)
            Dim cfg As New IoPinCfg()
            cfg.I2cAddres = Byte.Parse(fields(0))
            cfg.IoPin = Byte.Parse(fields(1))
            cfg.Expander = CType(Byte.Parse(fields(2)), IoExpanderType)
            Return cfg
        End Function
        Public Shared Function IoExpanderLabel(exp As IoExpanderType) As String
            Select exp 
                Case IoExpanderType.UsbPrint
                    Return "USBPrint"
                Case IoExpanderType.PCF8574
                    Return "PCF8574"
                Case IoExpanderType.PicPin
                    Return "PIC Pin"
                Case IoExpanderType.DAC_5578
                    Return "DAC 5578"
                Case IoExpanderType.PCA_9534
                    Return "PCA 9534"
                Case IoExpanderType.MCP_3428
                    Return "ADC MCP 3428"
                Case IoExpanderType.EBST_V2
                    Return "EBST V2"
                Case IoExpanderType.Reserved_7
                    Return "RESERVED 7"
                Case IoExpanderType.Reserved_8
                    Return "RESERVED 8"
                Case IoExpanderType.Reserved_9
                    Return "RESERVED 9"
                Case IoExpanderType.Reserved_10
                    Return "RESERVED 10"
                Case IoExpanderType.Reserved_11
                    Return "RESERVED 11"
                Case IoExpanderType.Reserved_12
                    Return "RESERVED 12"
                Case IoExpanderType.Reserved_13
                    Return "RESERVED 13"
                Case IoExpanderType.Reserved_14
                    Return "RESERVED 14"
                Case IoExpanderType.Disabled
                    Return "Disabled" 
                Case Else
                    Throw New Exception(String.Format("Unknown IO Expander Type: '{0}'", exp))
            End Select
        End Function
        Public Overrides Function Equals(obj As Object) As Boolean
            Return Equals(DirectCast(obj,IoPinCfg))
        End Function
        Public Overloads Function Equals(other As IoPinCfg) As Boolean
            Return I2cAddres = other.I2cAddres AndAlso
                   IoPin     = other.IoPin     AndAlso
                   Expander  = other.Expander
        End Function
        Public Shared Operator=(lhs As IoPinCfg, rhs As IoPinCfg) As Boolean
            Return lhs.Equals(rhs)
        End Operator
        Public Shared Operator<>(lhs As IoPinCfg, rhs As IoPinCfg) As Boolean
            Return Not lhs.Equals(rhs)
        End Operator
    End Structure


    <Serializable()>
    Public Structure PressureSensoreCfg
        Public I2cAddres As Byte      ' 7 bit
        Public Channel As Byte  ' 0 or 1
    End Structure

    ''' <summary>
    ''' The ID of the rear PMT high voltage control depends on the type of the high voltage control print,
    ''' on the older prints, it is ID 6, on the newer it is Id 8. Te need this ID because the control for
    ''' the rear PMT is different, so the same byte value results in a different gain.
    ''' </summary>
    Public Enum HighVoltagePrintType
        Invalid = 0
        Ruud
        FransJan_v1_0
    End Enum

    ''' <summary>
    ''' Specify if the automatic backflush time or a manual override should be used.
    ''' </summary>
    Public Enum BackflushTimeModeType
        Invalid = 0
        Automatic
        Manual
    End Enum



    Public Enum GpsSourceType
        Invalid = 0
        ComPort          'This will be the default on loading when it is Invalid.
        UdpPort
    End Enum

    Public Enum LaserKind_t
        None     =   0
        Matchbox =   1
        Obis     =   2
    End Enum

    Public Enum AutoStart_t
        Invalid =  0
        [On]    =  1
        Off     =  2
    End Enum

    Public enum LaserMode_t
        Invalid =  0
        OFF     =  1
        APC     =  2
        ACC     =  3
    End Enum

    ' Info retrieved from the device, there is some overlap with that in the General
    ' LaserInfoT, that is configured manually. The device info is only available if
    ' there is a connection to the device.
    <Serializable()>
    Public Class LaserDeviceInfo
        Public Sub New()
        End Sub

        Public Sub New(fwVer As String, serNum As String, mdl As String, onTime As Single, onCount As Integer)
            FirmwareVersion = fwVer
            SerialNumber    = serNum
            Model           = mdl
            DiodeOnTime     = onTime
            DiodeOnCount    = onCount
        End Sub

        Public FirmwareVersion As String = ""
        Public SerialNumber    As String = ""
        Public Model           As String = ""
        Public DiodeOnTime     As Single  = 0.0
        Public DiodeOnCount    As Integer = 0
    End Class

    <Serializable()>
    Public Class LaserDeviceSettings
        Public Sub New()
        End Sub

        Public Sub New(dTmp As Single, dCurr As Single, dac As Integer, optPow As Single, currLim As Single, autoStrt As AutoStart_t, al As Integer)
            DiodeTemperature   = dTmp
            DiodeCurrent       = dCurr
            FeedbackDac        = dac
            OpticalOutputPower = optPow
            DiodeCurrentLimit  = currLim
            AutoStart          = autoStrt
            AccessLevel        = al
        End Sub

        Public DiodeTemperature   As Single
        Public DiodeCurrent       As Single
        Public FeedbackDac        As Integer
        Public OpticalOutputPower As Single
        Public DiodeCurrentLimit  As Single
        Public AutoStart          As AutoStart_t
        Public AccessLevel        As Integer
    End Class


    ''' <summary>
    ''' Simple class to record some information about the lasers in the instrument.  For older instruments we do not have this.
    ''' For newer we start adding it.  In the future we should fill this information dynamicly with info we load from the
    ''' laser via USB. Then more info will be added, currently we imit it to the basic information that is static.
    ''' </summary>
    <Serializable()>
    Public Class LaserInfoT
        ''' <summary>
        ''' Parameterless constructor needed for serialization somewhere, I think XML in CC4, not exactly sure how/what/why.
        ''' </summary>
        Public Sub New() 
            Me.New("Unknown","Unknown",0,0,0,0,0,LaserKind_t.None)
        End Sub
        Public Sub New(desc As String, serial As String, wavelen As Integer, maxPwr As Integer, cfgPwr As Integer, beam As Double) 
            Me.New(desc,serial,wavelen,maxPwr,cfgPwr,beam,0,LaserKind_t.None)
        End Sub

        Public Sub New(desc As String, serial As String, wavelen As Integer, maxPwr As Integer, cfgPwr As Integer, beam As Double, urt As Integer, lsrKnd As LaserKind_t) 
            Description     = desc
            SerialNumber    = serial
            Wavelength      = wavelen
            MaxPower        = maxPwr
            ConfiguredPower = cfgPwr
            BeamWidth       = beam
            Uart            = urt
            Kind            = lsrKnd
        End Sub
        Public Description As String      ' Simple string such as Coherent 488LX60, or something like that.
        Public SerialNumber As String     ' The serial number of the laser
        Public Wavelength As Integer      ' Wavelength in nano meter
        Public MaxPower As Integer        ' Max power in milliwatt
        Public ConfiguredPower As Integer ' COnfigured Power in Milliwat.
        Public BeamWidth As Double        ' BeamWidth in nanometer

        Public Uart As Integer     = 0                 ' The number of the STM Uart this laser is connected to.  For now only supported for Matchbox lasers in the XR.
        Public Kind As LaserKind_t = LaserKind_t.None  ' The kind of laser connected to the uart, it determines the protocol used, for now only matchbox is supported.
        ' Information loaded from the laser itself if the option is supported, AND the laser is powered.
        Public DeviceInfo     As LaserDeviceInfo      = Nothing
        Public DeviceSettings As LaserDeviceSettings  = Nothing
        Public Function FormatDisplayString() As String
            Return String.Format("{0} ({1}) {2}mW", Description, SerialNumber, ConfiguredPower)
        End Function

        Public Overrides Function ToString() As String
            Return FormatDisplayString()
        End Function

        ' Initialize members that were added after the first files were already written. This way they will have safe default values
        ' When loading older datafiles.
        <OnDeserializing()>
        Private Sub OnDesrializing(context As StreamingContext)
            Uart = 0 ' 0 Means not connected.
            Kind = LaserKind_t.None  
            DeviceInfo     = Nothing
            DeviceSettings = Nothing
        End Sub

    End Class



    public enum SheathPumpControllerType_t
                        ''' <summary>Invalid value</summary>
        Invalid = 0
                        ''' <summary>The classic I2C controlled DC pump </summary>
        I2C     = 1  
                        ''' <summary>The New stepper motor controlled via an SPI stepper driver. </summary>
        Stepper = 2
    End enum



    ''' <summary>
    ''' This class encompasses all settings a machine can have
    ''' </summary>
    ''' <remarks>The settings of a machine are based around a set of defaults, so a normal machine does have the options for e.g. a sub, although they are meaningless </remarks>
    <Serializable()> Public Class CytoSenseSetting

        Public name As String


        ''' <summary>
        ''' In MHz
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property SampleFrequency As Single
            Get
                Return 4.0
            End Get
        End Property

        ''' <summary>
        ''' Denotes the distance a particle travels through the laser in one sample
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property Sample_to_um_ConversionFactor As Single
            Get
                Return SampleCorespeed / SampleFrequency
            End Get
        End Property


        Public thresholdPCT As Double = Double.NaN

        Public hasSubmode As Boolean   'only for deep subs! 
        Public isSorter As Boolean
        Public isShallowSub As Boolean

        Public shallowIntPressureMin As Double 'denotes the abs pressure on which the sample pump is turned off

        Public HasExternalTrigger As Boolean 'ExternalTriggerVisible
        Public ExternalTriggerIsPulse As Boolean
        Public ExternalTriggerFeedbackLed As Int32 '			6

        Private SerNr As String
        <Category("Instrument info"), DisplayName("Serial number"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public Property SerialNumber As String
            Get
                If SerNr IsNot Nothing Then
                    Return SerNr.Trim 'due to a typo some machines serials have an extra space 
                Else
                    Return "" 'pre usb cytosenses do not have this
                End If
            End Get
            Set(value As String)
                SerNr = value
            End Set
        End Property
        Public HardwareNr As String 
        <Category("Instrument info"), DisplayName("Hardware number"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property HardwareNumber As String
            Get
                Return HardwareNr
            End Get
        End Property


        <Category("Laser Data"), DisplayName("Laser Model"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Laser1Model As String
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceInfo IsNot Nothing Then
                    Return _laserInfo(0).DeviceInfo.Model
                Else
                    Return ""
                End If
            End Get
        End Property
        <Category("Laser Data"), DisplayName("Laser Firmware"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Laser1Firmware As String
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceInfo IsNot Nothing Then
                    Return _laserInfo(0).DeviceInfo.FirmwareVersion
                Else
                    Return ""
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Laser Serial Number"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Laser1SerialNumber As String
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceInfo IsNot Nothing Then
                    Return _laserInfo(0).DeviceInfo.SerialNumber
                Else
                    Return ""
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Laser On Time"), DescriptionAttribute(""), ComponentModel.Browsable(True), Data.DataBase.Attributes.Format("#0.0 hour")>
        Public ReadOnly Property Laser1OnTime As Single
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceInfo IsNot Nothing Then
                    Return _laserInfo(0).DeviceInfo.DiodeOnTime
                Else
                    Return Single.NaN
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Laser On Count"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Laser1OnCount As Integer
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceInfo IsNot Nothing Then
                    Return _laserInfo(0).DeviceInfo.DiodeOnCount
                Else
                    Return 0
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Set Diode Temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True),CytoSense.Data.DataBase.Attributes.Format("#0.00 \°C")>
        Public ReadOnly Property Laser1SetDiodeTemperature As Single
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    Return _laserInfo(0).DeviceSettings.DiodeTemperature
                Else
                    Return Single.NaN
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Set Diode Current"), DescriptionAttribute(""), ComponentModel.Browsable(True),CytoSense.Data.DataBase.Attributes.Format("#0.00 mA")>
        Public ReadOnly Property Laser1SetDiodeCurrent As Single
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    Return _laserInfo(0).DeviceSettings.DiodeCurrent
                Else
                    Return 0
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Feedback DAC"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Laser1FeedbackDAC As Integer
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    Return _laserInfo(0).DeviceSettings.FeedbackDac
                Else
                    Return 0
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Output Power"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.00 mW")>
        Public ReadOnly Property Laser1OpticalOutputPower As Single
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    Return _laserInfo(0).DeviceSettings.OpticalOutputPower
                Else
                    Return 0.0
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Diode Current Limit"), DescriptionAttribute(""), ComponentModel.Browsable(True),CytoSense.Data.DataBase.Attributes.Format("#0.00 mA")>
        Public ReadOnly Property Laser1SetDiodeCurrentLimit As Single
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    Return _laserInfo(0).DeviceSettings.DiodeCurrentLimit
                Else
                    Return 0
                End If
            End Get
        End Property

        <Category("Laser Data"), DisplayName("Auto Start"), DescriptionAttribute(""), ComponentModel.Browsable(True),CytoSense.Data.DataBase.Attributes.Format("#0.00 mA")>
        Public ReadOnly Property Laser1AutoStart As String
            Get
                If _laserInfo IsNot Nothing AndAlso _laserInfo.Length > 0 AndAlso _laserInfo(0).Uart <> 0 AndAlso _laserInfo(0).DeviceSettings IsNot Nothing Then
                    If _laserInfo(0).DeviceSettings.AutoStart = AutoStart_t.On Then
                        Return "On"
                    Else If _laserInfo(0).DeviceSettings.AutoStart = AutoStart_t.Off Then
                        Return "Off"
                    Else 
                        Return "???"
                    End If
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Softwareversion As Int16    'voor Cytoclus? [b] nee niet gebruikt
        Public hasSheetPumpAdjust As Boolean

        Public hasExternalBatterie As Boolean 
        Public hasExternalSupply As Boolean 'wall power sensor
        Public hasCurvature As Boolean
        Public hasWaterDetectionSensor As Boolean
        Public hasPressureSensor As Boolean
        Public hasImageAndFlow As Boolean
        Public hasExternalPump As Boolean
        Public hasGVModule As Boolean
        Private hasPIC As Boolean 'use hasaPic property instead!
        Public hasContinuMode As Boolean 'deprecated
        Public hasDCSamplePump As Boolean
        Public hasBypassPinchValve As Boolean
        Public hasExternalPinchValve As Boolean
        Public hasFJDataElectronics As Boolean

        Public hasDeflatePinchValvePropertyV10 As Boolean = False ' Only for V10, for V8 this is solved differently.
        Public hasBeadsPinchValvePropertyV10 As Boolean   = False ' Only for V10, for V8 this is solved differently.
        Public ExternalPinchValveUlnNumber As Integer = 8 ' For V10 electronics, the external pinch valve can be on different ULN Outputs, for V08 this variable is not used.
        Public BypassPinchValveUlnNumber As Integer   = 7 ' For V10 the bypass pinchvalve ULN number can also be configured.
        Public BeadsPinchValveUlnNumber As Integer    = 6 ' For V10 the beads pinchvalve ULN number can be configured, also configured via PIC EEPROM, so not not sure about this.
        Public DeflatePinchValveUlnNumber As Integer  = 5 ' For V10 the deflate pinchvalve ULN number is now configurable.
        Public ExternalPumpUlnNumber As Integer       = -3 ' For V10 only, 1..16 are ULN, negative numbers are Spare outputs on the PowerBoard, so the default -3 is Spare 3.
#Region "BST Module features"
        ' Features below are configured using the PIC EEPROM, not configured in the Machines.vb file!!
        ' If not present, member will be 0.
        Public BiocideOptions As BiocideModuleOptionsT
        Public BeadsOptions As BeadsModuleOptionsT
        Public ExternalFilterModule As New ExternalFilterModuleOptionsT 'Always present!, Except when de-serializing older settings structures.
        Public CarbonFilterModule As CarbonFilterModuleOptionsT ' If not present, then structure will not be present as well.
        Public hasFlowSensor As Boolean = False
        Public Model As String = ""
        Public SheathPumpController As SheathPumpControllerType_t ' What is the type of the sheath pump.
        <OnDeserializing()>
        Private Sub PrepareForDeserializing(sc As StreamingContext)
            PowerSaveLeaveBlowerOn = True ' Default this new member to True, so it will be true if it was not explicitly stored.
            ExternalPinchValveUlnNumber = 8 'Default to 8, if it was not explicitly stored as something else.
            BypassPinchValveUlnNumber   = 7 'Default to 7, so will be set correctly for V10 electronics that do not have this set in their config yet.
            BeadsPinchValveUlnNumber    = 6 'Default to 6, so it will be set correctly for older configurations that did not have this set.
            DeflatePinchValveUlnNumber  = 5 'Default to 5 so it will be set correctly for configurations that do not have this set.

            _numberOfLasers = 0 ' Initialize to 0, meaning we do not know.
            _laserInfo = Nothing
            hasFlowSensor = False
            Model = ""
        End Sub

        ''' <summary>
        ''' Initialize module options and other fields after loading.
        ''' </summary>
        ''' <remarks>These fields are not present in older versions of the settings class, so after
        ''' loading we check if they are Nothing, or not.  If they are still nothing, this means they were not
        ''' loaded and we simply create a default object for them.
        ''' Also the lock object needs to be initialized, since it is never serialized, we need to do
        ''' it here.
        ''' </remarks>
        <OnDeserialized> _
        Private Sub InitializeModuleOptions(sc As StreamingContext)
            If ExternalFilterModule Is Nothing Then
                ExternalFilterModule = New ExternalFilterModuleOptionsT()
            End If
            _mV_lookupNew_LOCK = New Object()
            If BackFlushTimeMode = BackflushTimeModeType.Invalid Then
                BackFlushTimeMode = BackflushTimeModeType.Manual 'For old files signal the value was never set by the user.
            End If
            If MultiSampler Is Nothing Then
                MultiSampler = New NewMultiSamplerSettings() 'Default initialized, only back flush setting is used for old systems.
            End If
            If GpsSource = GpsSourceType.Invalid Then ' Old file, GpsSource not set also initialize ages
                GpsSource     =  GpsSourceType.ComPort
                GpsUdpPortNumber = 8500
                GpsWarnAge    = TimeSpan.FromMinutes(2)
                GpsErrorAge   = TimeSpan.FromMinutes(5)
            End If
            _channelListLock = New Object()

            ' Check the number of lasers.
            ' Because of a bug there are some configs for V10 electronics, that have a value of 1 where it should be 2,
            ' so on startup always check the DisableLaser2PowerSensor setting. Dunno how many machines there are out there
            ' where this applies.
            If _numberOfLasers = 0 OrElse _numberOfLasers = 1 Then 'Not initialized, for V2 electronics, if not initialized, we can make a guess.
                If hasFJDataElectronics Then
                    If _laserInfo IsNot Nothing Then '' If there is laser info we trust that, else we use the disable power sensor hack.
                        _numberOfLasers = _laserInfo.Length
                    Else
                        If CytoUSBSettings.DisableLaser2PowerSensor Then
                            _numberOfLasers = 1
                        Else
                            _numberOfLasers = 2
                        End If
                    End If
                End If 'Else V08 electronics, no way to know. (most of the time, so we do not even try)
            End If ' Else a number was configured, so leave it alone.
            If StainingModule Is Nothing Then
                ' Older version that did not create the StainingModule settings until a StainingModule was actually used.
                ' Now we always create one, with the InUse flag set to false. Untill we actually
                ' use one.
                StainingModule = New StainingModuleSettings()
            End If
        End Sub


#End Region

        Public DCSamplePump As Calibration.SamplePump.DCSamplePump
        Public PIC As PICSettings


        <Category("Staining Module"), DisplayName("Dye 1 Name"), Description("Name of Dye in unit 1."), ComponentModel.Browsable(True)>
        Public ReadOnly Property Dye1Name As String
            Get
                If Not StainingModule.InUse OrElse Not StainingModule.DyeUnit1Present Then
                    Return ""
                End If
                Return StainingModule.DyeUnit1Stain
            End Get
        End Property

        <Category("Staining Module"), DisplayName("Dye 2 Name"), Description("Name of Dye in unit 2."), ComponentModel.Browsable(True)>
        Public ReadOnly Property Dye2Name As String
            Get
                If Not StainingModule.InUse OrElse Not StainingModule.DyeUnit2Present Then
                    Return ""
                End If
                Return StainingModule.DyeUnit2Stain
            End Get
        End Property

        Public StainingModule As StainingModuleSettings = New StainingModuleSettings()
        Public AutomaticInjector As CAutomaticInjectorSettings = Nothing

        Public DpsModule As DpsModuleSettings = Nothing  ' Nothing if no DPS was ever connected, something, if this CytoSense has seen a DPS before.
        Public MultiSampler As New NewMultiSamplerSettings()

        Public hasVref As Boolean

        'obsolete
        Public sampleSpeedRatio As Double

        'new
        Public samplePumpCalibration As CytoSense.Calibration.SamplePump.SamplePumpCalibrationData


        Public triggerlevelConstant As Double
        Public triggerlevelOffset As Double
        Public CytoUSBSettings As CytoUSBSettings.CytoUSBSettings

        'sub mode
        Public State1SubModeTime As Double  '30 State 1 of 4; Pumping water towards loop...
        Public State2SubModeTime As Double  '60 State 2 of 4; Filling sample loop with water...
        Public State3SubModeTime As Double  '50 State 3 of 4; Pumping water from loop to the flow cuvette...
        Private _subLoopVolume_uL As Double
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property SubLoopVolume_uL As Double
            Get
                If _subLoopVolume_uL = 0 Then
                    _subLoopVolume_uL = 3000
                End If
                Return _subLoopVolume_uL
            End Get
        End Property

        Public FixSamplePumpPosition As Boolean


        ' Some IIF properties, to display in CytoClus, not pretty here, but for now the best place I guess.
        <Category("Image in Flow"),
            DisplayName("Optical Magnification"),
            Description("Magnification factor of the optics."),
            Browsable(True),
            Data.DataBase.Attributes.Format("#0.00 times")
            >
        Public ReadOnly Property OpticalMagnification As Double
            Get
                Return iif.opticalMagnification
            End Get
        End Property

        <Category("Image in Flow"),
            DisplayName("Image Pixel Size"),
            Description("Size of one pixel in the image."),
            Browsable(True),
            Data.DataBase.Attributes.Format("#0.00 μm")
            >
        Public ReadOnly Property ImagePixelSize As Double
            Get
                Return iif.ImageScaleMuPerPixelP
            End Get
        End Property

        ' Some old files may not have the camera info stored, the other parts should be present, so instead of returning the camera pixel size
        ' we try to calculate it, or return 0 it if fails.
        <Category("Image in Flow"),
            DisplayName("Camera Pixel Size"),
            Description("Size of one pixel on the camera sensor."),
            Browsable(True),
            Data.DataBase.Attributes.Format("#0.00 μm")
            >
        Public ReadOnly Property CameraPixelSize As Double
            Get
                Try
                    If iif.CameraFeatures IsNot Nothing Then
                        Return iif.CameraFeatures.PixelPitch
                    Else
                        Return iif.ImageScaleMuPerPixelP * iif.opticalMagnification
                    End If
                Catch ex As Exception ' Assume old data file that does not have the info we want.
                    Return Double.NaN
                End Try
            End Get
        End Property

        <Category("Image in Flow"), DisplayName("AutoCrop Enabled"), Description("Auto Cropping was enabled during this measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property AutoCropEnabled As Boolean
            Get
                #Disable Warning BC40008
                Return enableAutoCrop OrElse iif.EnableAutoCrop ''Check both for now, transition from in this file to the iif struct. Don't want to break old ones
                                                                ''Should be able to completely change this to the iif version later when everyone's settings updated
                #Enable Warning BC40008
            End Get
        End Property

        <Category("Image in Flow"), DisplayName("AutoCrop Background Threshold"), Description("Background threshold used in autocrop (if enabled)."), ComponentModel.Browsable(True)>
        Public ReadOnly Property CropBGThreshold As Integer
            Get
                Return iif.CropBGThreshold
            End Get
        End Property

        <Category("Image in Flow"), DisplayName("AutoCrop erode/dilate steps"), Description("Amount of erosion and dilation steps used in autocrop (if enabled)."), ComponentModel.Browsable(True)>
        Public ReadOnly Property CropErodeDilateSteps As Integer
            Get
                Return iif.CropErodeDilateSteps
            End Get
        End Property

        <Category("Image in Flow"), DisplayName("AutoCrop base margin"), Description("Base margin used in autocrop (if enabled)."), ComponentModel.Browsable(True)>
        Public ReadOnly Property CropMarginBase As Integer
            Get
                Return iif.CropMarginBase
            End Get
        End Property

        <Category("Image in Flow"), DisplayName("AutoCrop margin factor"), Description("factor applied to base margin used in autocrop (if enabled)."), ComponentModel.Browsable(True)>
        Public ReadOnly Property CropMarginFactor As Double
            Get
                Return iif.CropMarginFactor
            End Get
        End Property

        Public iif As IIFSettings
        Public DSPRecognizeString As String
        Public DSPRS232FTDICode As String
        Public DSPUseFTDICodeForRecognition As Boolean

        Public Vref_V As Double 'measured by multimeter
        Public Vref_byte As Double 'measured by AD converter


        Public TabSettingsFilePath As String

        Public MaximumBuoyLifeTime As Int64
        Public BuoyLifeTime As Int64


        'machine cytoUSB settings

        Public EnableLoggin As Boolean
        Public EnableExternalLogging As Boolean
        Public EnableClearLogBeforeMeasurement As Boolean
        Public EnableHardwareConcentration As Boolean 'if enabled use Ruud electronics FTDI concentration instead of PIC

        Public EnableAutoDeviceDetect As Boolean
        Public EnableVoice As Boolean
        Public EnableAutoPowerSave As Boolean
        Public PowerSaveLeaveBlowerOn As Boolean = True
        Public EnableAutoShutdownScheduleMode As Boolean
        Public EnableBackFlushScheduleMode As Boolean
        Public EnableStopPumps As Boolean
        Public EnableCompressIIFImages As Boolean
        Public EnableSaveUnmatchedIIFFoto As Boolean
        Public EnableExternalTrigger As Boolean
        Public EnableGVModule As Boolean
        Public EnableIIFCamera As Boolean
        Public EnableWaterDetectionAlert As Boolean 'deprecated, moved to cytousbsettings. 
        Public EnableSubmode As Boolean
        Public EnableShallowSubMode As Boolean
        Public EnableSeperateUSBProgram As Boolean
        Public EnableWarmingUp As Boolean
        Public EnableDepthTrigger As Boolean
        Public DisableResetDSP As Boolean
        Public EnableExternalPump As Boolean
        Public EnableExternalPinchValve As Boolean ' Use the external pinch valve for sample pump control, either pinch, or pump or neither, never both.
        Public EnableContinuMode As Boolean 'obsolete, because Ruud electronics will never support continu mode 


        Public ExternalSheathMode As Boolean

        <Obsolete()> Public EnableCameraBackgroundCalibration As Boolean
        Public EnableDetectorNoiseLevelDetection As Boolean

        Public EnableMultiSampler As Boolean = False ' Select if you want to use it/is connected or not.


        Public OffsetInternalTemperature As Byte
        Public OffsetInternalVoltage As Byte
        Public OffsetPressure As Byte

        Private _SensorLimits As SensorHealthLimits ' this guy should not be instantiated in s.default, so the property can do a initialization with updated core speed
        <ComponentModel.Browsable(False)>
        Public Property SensorLimits As SensorHealthLimits
            Get
                If _SensorLimits Is Nothing Then
                    _SensorLimits = New SensorHealthLimits(SampleCorespeed)
                End If
                Return _SensorLimits
            End Get
            Set(value As SensorHealthLimits)
                _SensorLimits = value
            End Set
        End Property


        Public HWCIntervalText As Double    'in ms
        Public RealTimeDataViewTimerIntervalText As Double ' in ms
        Public SeperateConcentrationText As Double ' in s

        Public AutoPowerSafeTime As Double 'in seconds
        Public CheckIfEverythingIsOkTimerTextBox As Double 'in seconds
        Public LaserWarmingUpTime As Double  'in s
        Public FlushTime As Double 'in s
        Private BackFlushTime As Double    'in s

        Public Function CalculateAutoBackFlushTime( ftime As Double ) As Double
            Return 0.77 * ftime
        End Function

        ''' <summary>
        ''' Return the correct back flush time, either the automatic one, or the one specified by
        ''' the user in manual override (or unspecified for upgrade path). For compatibility
        ''' with the serialized format we cannot use the term BackflushTime. That variable
        ''' is made private so you cannot access it but the name cannot be changed without a format
        ''' change.
        ''' </summary>
        ''' <returns></returns>
        <Category("Measurement instrument settings"), DisplayName("Backflush Time"), Description(""), ComponentModel.Browsable(True), Data.DataBase.Attributes.Format("#0 \s")>
        Public ReadOnly Property ActualBackFlushTime As Double
            Get
                If BackFlushTimeMode = BackflushTimeModeType.Automatic Then
                    Return CalculateAutoBackFlushTime(FlushTime - 10) 'There is a magic number of 10 seconds added when stored in the file, during this time the sample pump is not flushing, so we do not want that in the calculation for back flush.
                Else
                    Return BackFlushTime
                End If
            End Get
        End Property

        <ComponentModel.Browsable(False)>
        Public Property ManualBackflushTime As Double
            Get
                Return BackFlushTime
            End Get
            Set(value As Double)
                BackFlushTime = Value
            End Set
        End Property

        Public TriggerDepth As Double 'in meters
        Public MaxTimeOut As Double 'in seconds
        Public lowtabs As LowerTabs

        Public FlushATime As Double
        Public FlushBTime As Double
        Public CleaningFromSource1 As Double
        Public CleaningFromSource2 As Double
        Public CleaningFromSource3 As Double
        Public FillingLoopTime As Double
        Public SampleToInjectorTime As Double
        Public PressurizingTime As Double 
        'NOTE: This really is obsolete and should never be used. I assign it in 2 places to make sure the generated data files are 
        '      backwards compatible.
        <Obsolete()> _
        Public hasGPS As Boolean
        Public enableGPS As Boolean
        <Obsolete>
		Public enableAutoCrop As Boolean
        Public baudGPS As Int32
        Public comPortGPS As Int32
        Public GpsSource As GpsSourceType = GpsSourceType.ComPort 'Default to comport
        Public GpsUdpPortNumber As Int32  = 8500' The Udp Port number to use if UDP is used.
        Public GpsWarnAge As TimeSpan     = TimeSpan.FromMinutes(2)' If the last fix is older then this, it is a warning.
        Public GpsErrorAge As TimeSpan    = TimeSpan.FromMinutes(5) 'If the last fix is older then this, it is an error.

        Public MinimumSampleChamberRefreshInterval As TimeSpan = TimeSpan.FromMinutes(2)' Minimum time the sample chamber should be open to allow the content to refresh. 
        Public ExternalPumpTime As Double 'in s
        Public ExternalPumpTime_OverlapWithNormalFlush As Double 'in s

        'Multiple laser options
        ' Two lasers with a small distance between them. Enables FLRed split channels
        Public hasDualLaserDistance As Boolean

        'Two lasers, one modulated. The hardware channels contain signals of 'laser 1' and 'laser 1 + 2', so the software has to subtract this.
        'Note that the channel definition of this machine should then declare which channel to subtract!
        Public hasSoftwareSubstractsLaserChannels As Boolean

        ''' <summary>
        ''' Old BST settings, only present for backwards compatibility with older experiments.
        ''' </summary>
        ''' <remarks>Left enabled because New BST Eijsden is not yet deployed, so we need the old stuff.</remarks>
        Private _BSTSystem As BSTSettings
        <ComponentModel.Browsable(False)>
        Public Property BSTSystem As BSTSettings
            Get
                If Object.Equals(Nothing, _BSTSystem) Then
                    _BSTSystem = BSTSettings.CreateEmptyNotPresent()
                End If
                Return _BSTSystem
            End Get
            Set(value As BSTSettings)
                _BSTSystem = value
            End Set
        End Property


        <ComponentModel.Browsable(False)>
        Public Property SheathType As SheathEnum

        Public digitalOutputs() As digitalOutput
        Public channels As channel()
        Public OldHardwareNrs() As String
        Public dllRelease As New Serializing.VersionTrackableClass(New Date(2012, 8, 12)) 'use this to flag to CytoUsb that the CytoSettings of all machines needs to be reloaded
        Public machineConfigurationRelease As New Serializing.VersionTrackableClass(New Date(2010, 3, 18)) 'use this to flag CytoUsb to update the CytoSettings only for this machine 

#Region "Overridable Properties"
        ''' <summary>
        ''' 'use sampleCorespeed property instead!
        ''' </summary>
        ''' <remarks>staat nu als *1000</remarks>
        Private Corespeed As Double
        <Category("Instrument info"), DisplayName("Samplecore Speed"), DescriptionAttribute("Estimated speed of the sample through the laser"), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.00 \m/s")>
        Public Property SampleCorespeed As Single
            Get
                If _enableOverrideSampleCoreSpeed Then
                    Return _overrideSampleCoreSpeed
                Else
                    Return CSng(Corespeed)
                End If

            End Get
            Set(value As Single)
                Corespeed = value
            End Set
        End Property

        <Category("Instrument info"), DisplayName("Sheath Speed"), DescriptionAttribute("Default sheath speed setting"), ComponentModel.Browsable(True)>
        Public ReadOnly Property DefautlSheathSpeed As Single
            Get
                If hasaPIC AndAlso PIC.I2CSamplePump Then
                    Return CSng(PIC.DefaultSheathSpeed)
                Else
                    Return Single.NaN
                End If
            End Get
        End Property

        <NonSerialized> Private _overrideSampleCoreSpeed As Single
        <NonSerialized> Private _enableOverrideSampleCoreSpeed As Boolean = False
        <ComponentModel.Browsable(False)>
        Public WriteOnly Property OverrideSampleCoreSpeed As Single
            Set(value As Single)
                _overrideSampleCoreSpeed = value
            End Set
        End Property
        <ComponentModel.Browsable(False)>
        Public Property EnableOverrideSampleCoreSpeed As Boolean
            Get
                Return _enableOverrideSampleCoreSpeed
            End Get
            Set(value As Boolean)
                _enableOverrideSampleCoreSpeed = value
            End Set
        End Property

        ''' <summary>
        '''Use LaserBeamWidth property instead!
        ''' </summary>
        ''' <remarks></remarks>
        Private BeamWidth As Double
        <Category("Instrument info"), DisplayName("Laser beam width"), DescriptionAttribute("The minimum distance a particle travels through the laser"), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 μm")>
        Public Property LaserBeamWidth As Single
            Get
                If _enableOverrideLaserBeamWidth Then
                    Return _overrideLaserBeamWidth
                Else
                    Return CSng(BeamWidth)
                End If
            End Get
            Set(value As Single)
                BeamWidth = value
            End Set
        End Property
        <NonSerialized> Private _overrideLaserBeamWidth As Single
        <NonSerialized> Private _enableOverrideLaserBeamWidth As Boolean = False
        <ComponentModel.Browsable(False)>
        Public WriteOnly Property OverrideLaserBeamWidth As Single
            Set(value As Single)
                _overrideLaserBeamWidth = value
            End Set
        End Property
        <ComponentModel.Browsable(False)>
        Public Property EnableOverrideBeamWidth As Boolean
            Get
                Return _enableOverrideLaserBeamWidth
            End Get
            Set(value As Boolean)
                _enableOverrideLaserBeamWidth = value
            End Set
        End Property
#End Region

#Region "Public properties"
        <Category("Instrument info"), DisplayName("Machine"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property machineName As String
            Get
                Return name
            End Get
        End Property

        <Category("Instrument info"), DisplayName("Hardware Channels"), DescriptionAttribute("List of hardware channels the machine has"), ComponentModel.Browsable(False)>
        Public ReadOnly Property HardwareChannelNames() As String()
            Get
                Dim tempArray(channels.Length - 1) As String
                For c = 0 To channels.Length - 1
                    tempArray(c) = channels(c).name
                Next
                Return tempArray
            End Get
        End Property



        <Category("Modules"), DisplayName("CytoSub"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property isSub As Boolean
            Get
                Return hasSubmode
            End Get
        End Property
        <Category("Modules"), DisplayName("Sorter"), DescriptionAttribute(""), ComponentModel.Browsable(False)>
        Public ReadOnly Property Sorter As Boolean
            Get
                Return isSorter
            End Get
        End Property


        <Category("Instrument features"), DisplayName("Externally Triggered"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property ExternalTrigger As Boolean
            Get
                Return HasExternalTrigger
            End Get
        End Property

        <Category("Instrument features"), DisplayName("Curvature"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property Curvature As Boolean
            Get
                Return hasCurvature
            End Get
        End Property

        <Category("Instrument features"), DisplayName("Water detection sensor"), DescriptionAttribute(""), ComponentModel.Browsable(False)>
        Public ReadOnly Property WaterDetectionSensor As Boolean
            Get
                Return hasWaterDetectionSensor
            End Get
        End Property

        Private _waterdetectionEnum As WaterDetectionEnum
        <ComponentModel.Browsable(False)>
        Public Property WaterDetectionConnection As WaterDetectionEnum
            Get
                Return _waterdetectionEnum
            End Get
            Set(value As WaterDetectionEnum)
                _waterdetectionEnum = value
            End Set
        End Property


        <Category("Instrument features"), DisplayName("External pressure sensor"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property PressureSensor As Boolean
            Get
                Return hasPressureSensor
            End Get
        End Property
        <Category("Instrument features"), DisplayName("Two Lasers with dual focus distance"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property dualLaserDistance As Boolean
            Get
                Return hasDualLaserDistance
            End Get
        End Property


        <Category("Modules"), DisplayName("Image in flow"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property ImageAndFlow As Boolean
            Get
                Return hasImageAndFlow
            End Get
        End Property

        <Category("Instrument features"), DisplayName("External pump"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property ExternalPump As Boolean
            Get
                Return hasExternalPump
            End Get
        End Property

        <Category("Modules"), DisplayName("Gas Vacuole module"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property GVModule As Boolean
            Get
                Return hasGVModule
            End Get
        End Property

        <Category("Instrument features"), DisplayName("PIC processor"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property hasaPIC As Boolean
            Get
                Return hasPIC And PIC IsNot Nothing 'some files exist with haspic=1, but no PIC... (e.g. "E:\Data\HD\CytoBuoy\datafiles klanten\China\beads\1.6u TLEMC.CYZ")
            End Get
        End Property
        Public Sub setHasPic(value As Boolean)
            hasPIC = value
        End Sub

        <Category("Instrument features"), DisplayName("Continumode"), DescriptionAttribute(""), ComponentModel.Browsable(False)>
        Public ReadOnly Property ContinuMode As Boolean
            Get
                Return hasContinuMode
            End Get
        End Property

        <Category("Instrument features"), DisplayName("DC samplepump"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property hasaDCSamplePump As Boolean
            Get
                Return hasDCSamplePump
            End Get
        End Property

        <ComponentModel.Browsable(False)>
        Public ReadOnly Property samplepumpCalibrationDate As Date
            Get
                If DCSamplePump IsNot Nothing Then
                    Return DCSamplePump.CalibrationDate
                Else
                    Return Nothing
                End If
            End Get
        End Property

        <Category("Instrument info"), DisplayName("Samplepump calibration date"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property SamplepumpCalibrationDateStr As String
            Get
                If DCSamplePump IsNot Nothing Then
                    If DCSamplePump.IsReallyCalibrated Then
                        Return DCSamplePump.CalibrationDate.ToString("yyyy-MM-dd HH:mm")
                    Else
                        Return "Not Calibrated"
                    End If
                Else
                    Return Nothing
                End If

            End Get
        End Property


        <Category("Instrument features"), DisplayName("Bypass pinchvalves"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property BypassPinchValve As Boolean
            Get
                Return hasBypassPinchValve
            End Get
        End Property

        <Category("Modules"), DisplayName("BST system"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property hasBSTsystem As Boolean
            Get
                Return BSTSystem.IsPresent
            End Get
        End Property

        <Category("Modules"), DisplayName("Biocide"), DescriptionAttribute("An automatic biocide module was connected to the machine"), ComponentModel.Browsable(False)>
        Public ReadOnly Property BiocideModule As Boolean
            Get
                Return BiocideOptions IsNot Nothing
            End Get
        End Property
        ''' <summary>
        ''' True when an automatic beads module is present in the system, false if not.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Modules"), DisplayName("Beads"), DescriptionAttribute("An automatic beads module was connected to the machine"), ComponentModel.Browsable(False)>
        Public ReadOnly Property BeadsModule As Boolean
            Get
                Return BeadsOptions IsNot Nothing
            End Get
        End Property


        <Category("Measurement instrument settings"), DisplayName("Biocide Module Enabled"), DescriptionAttribute("The biocide module was enabed during the measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property BiocideModuleEnabled As Boolean
            Get
                Return BiocideOptions IsNot Nothing AndAlso BiocideOptions.Enabled
            End Get
        End Property


        <Category("Measurement instrument settings"), DisplayName("GPS Enabled"), Description("GPS recording was enabled during this measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property GpsEnabled As Boolean
            Get
                Return enableGPS
            End Get
        End Property




        ''' <summary>
        ''' Estimated Biocide Concentration at the START of the measurement
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The stored concentration is the one estimated after the last inject, so we need to the measurement 
        ''' using the stored concentration, sheath volume and pumped volume.!
        ''' NOTE: Current calculation works per ml, so it ignores changes smaller then a milliliter, and updates only
        ''' after the next ml has been pumped.</remarks>
        <Category("Instrument info"), DisplayName("Biocide Concentration"), DescriptionAttribute("The estimated biocide concentration at the start of the measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property BiocideExtimatedConcentration As UShort
            Get
                If BiocideOptions IsNot Nothing Then ' Make this support function of the BiocideOptions function.
                    Return BiocideModuleOptionsT.EstimateConcentration_ppm(BiocideOptions.EstimatedSheathConcentration, BiocideOptions.SheathVolume, BiocideOptions.PumpedVolume)
                Else
                    Return 0
                End If
            End Get
        End Property

        <Category("Instrument info"), DisplayName("Sample since biocide"), DescriptionAttribute("Pumped volume since the last biocide injection."), ComponentModel.Browsable(True)>
        Public ReadOnly Property BiocidePumpedVolume As UInt32
            Get
                If BiocideOptions IsNot Nothing Then
                    Return BiocideOptions.PumpedVolume
                Else
                    Return 0
                End If
            End Get
        End Property

        ''' <summary>
        ''' Estimated amount of biocide left in the instrument, at the start of the measurement.
        ''' </summary>
        ''' <returns></returns>
        <Category("Instrument info"), DisplayName("Biocide Left (ml)"), DescriptionAttribute("The estimated amount of biocide left in the container at the start of the measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property BiocideLeft_ml As Double
        Get
                If BiocideOptions IsNot Nothing Then
                    Return CDbl(BiocideOptions.ReservoirVolume) / 1000.0
                Else
                    Return 0.0 'No biocide options, so no biocide left
                End If
        End Get
        End Property

        ''' <summary>
        ''' The estimated amount of beads left in syringe at the start of the measurement.
        ''' </summary>
        ''' <returns></returns>
        <Category("Instrument info"), DisplayName("Beads Left (ml)"), DescriptionAttribute("The estimated amount of beads left in syringe at the start of the measurement."), ComponentModel.Browsable(True)>
        Public ReadOnly Property BeadsLeft_ml As Double
        Get
                If BeadsOptions IsNot Nothing Then
                    Return CDbl(BeadsOptions.BeadsVolume) / 1000.0
                Else
                    Return 0.0 'No beads options, so no beads left.
                End If
        End Get
        End Property



        <Category("Version Info"), DisplayName("CytoUSB version"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property CytoUSBVersion As String
            Get
                If CytoUSBSettings IsNot Nothing AndAlso CytoUSBSettings.CytoUSBVersion IsNot Nothing Then
                    If CytoUSBSettings.CytoUSBVersion.StartsWith("Not") Then
                        Return CytoUSBSettings.CytoUSBVersion
                    Else
                        Return CytoUSBSettings.CytoUSBVersion.Split(":"c)(1).Trim 'for some reason, the string "CytoUSB version was put into the version string...
                    End If
                Else
                    Return ""
                End If
            End Get
        End Property

        <Category("Version Info"), DisplayName("DSP version"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property DSPVersion As String
            Get
                If iif.DSPCompileVersion IsNot Nothing Then
                    Return iif.DSPCompileVersion
                Else
                    Return ""
                End If
            End Get
        End Property
        <Category("Version Info"), DisplayName("Date of dll"), DescriptionAttribute("The dllrelease versiontrackable class"), ComponentModel.Browsable(True)>
        Public ReadOnly Property dllVersion As Date
            Get
                If Not Object.Equals(Nothing, dllRelease) Then
                    Return dllRelease.ReleaseDate
                Else
                    Return Nothing
                End If
            End Get
        End Property
        <Category("Version Info"), DisplayName("Date of Hardware"), DescriptionAttribute("The hardware versiontrackable class"), ComponentModel.Browsable(True)>
        Public ReadOnly Property hardwareVersion As Date
            Get
                If Not Object.Equals(Nothing, dllRelease) Then
                    Return machineConfigurationRelease.ReleaseDate
                Else
                    Return Nothing
                End If
            End Get
        End Property

        <Category("Version Info"), DisplayName("PIC Firmware"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property PicVersion As String
            Get
                If hasaPIC AndAlso Not String.IsNullOrEmpty(PIC.FirmwareVersion) Then
                    Return PIC.FirmwareVersion
                Else
                    Return ""
                End If
            End Get
        End Property

#End Region


        Private _highVoltageType As HighVoltagePrintType = HighVoltagePrintType.Invalid
        ''' <summary>
        ''' The default for older machines is Ruud electronics, for newer machines, the FransJan version.
        ''' Unless explicitly specified.
        ''' This may fail for the simulator, so you need set an explicit type for that, if one was not set in
        ''' the original one.  Newer machines have an explicit setting, older electronics machines only
        ''' get an explicit setting when the print is exchanged.
        ''' </summary>
        ''' <returns></returns>
        <ComponentModel.Browsable(False)>
        Public Property HighVoltageType As HighVoltagePrintType
            Get
                If _highVoltageType = HighVoltagePrintType.Invalid Then
                    If SerialNumber.Trim() < "CS-2015-73" Then
                        _highVoltageType = HighVoltagePrintType.Ruud
                    Else
                        _highVoltageType = HighVoltagePrintType.FransJan_v1_0
                    End If
                End If
                Return _highVoltageType
            End Get
            Set (value As HighVoltagePrintType)
                _highVoltageType = value
            End Set
        End Property




        ''' <summary>
        ''' Returns the channel id given a "sync separator" value. Does not work for the trigger channel and the DSP channel!
        ''' </summary>
        ''' <param name="sync"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getChannelIndexFromSync(sync As Byte) As Integer
            If sync >= 130 Then
                sync = CByte(sync - 130)
                sync = CByte(sync / 2)
                Return sync + 1
            Else
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns the index in the (hardware) channel list. Note that this is incompatible with the (volatile) ChannelList property!
        ''' </summary>
        ''' <remarks>
        ''' The use of this method is mildly frowned upon
        ''' </remarks>
        Public Function getChannelIndex(ByVal ChannelName As String) As Int16
            For i As Int16 = 0 To CShort(channels.Length - 1)
                If channels(i).name.ToUpper = ChannelName.ToUpper() Then
                    Return i
                    Exit For
                End If
            Next
            Throw New CytoSenseDoesNotHaveThisChannelException()
        End Function

        Public Function getSheatPumpAdjustDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.SheathPumpAdjust Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getExternalTriggerLedDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.ExternalTriggerLed Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getDSPSetupDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.DSPSetup Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getDSPResetDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.DSPReset Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getGVPinchValveA_BDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.GVPinchValveA_B Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getPinchValveBST() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.PinchValveBST Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function
        Public Function getGVPinchValveC_PumpDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.GVPinchValveC_Pump Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function

        Public Function getBypassPinchValveDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.PinchValveBypass Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function

        Public Function getDeflatePinchValveDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.DeflatePinchValve Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function

        Public Function hasDeflatePinchValve() As Boolean
            If hasFJDataElectronics Then
                Return hasDeflatePinchValvePropertyV10
            Else
                For i As Int16 = 0 To 7
                    If digitalOutputs(i).type = digitalOutput.DI_Type.DeflatePinchValve Then
                        Return True
                    End If
                Next
                Return False
            End If
        End Function

        Public Function getBeadsPinchValveDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.BeadsPinchValve Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function

        Public Function hasBeadsPinchValve() As Boolean
            If hasFJDataElectronics Then
                Return hasBeadsPinchValvePropertyV10
            Else
                For i As Int16 = 0 To 7
                    If digitalOutputs(i).type = digitalOutput.DI_Type.BeadsPinchValve Then
                        Return True
                    End If
                Next
                Return False
            End If
        End Function


        Public Function getExternalPinchValveDIO() As Int16
            For i As Int16 = 0 To 7
                If digitalOutputs(i).type = digitalOutput.DI_Type.ExternalPinchValve Then
                    Return i
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function


        Public Function getFlowSpeed(speed As Integer) As Double
            If hasDCSamplePump AndAlso hasaPIC AndAlso DCSamplePump IsNot Nothing Then
                Return DCSamplePump.getFlowSpeed(speed)
            Else
                'in te voegen voor backwards compatability, of niet gekalibreerde samplepumps:
                If Object.Equals(Nothing, samplePumpCalibration) Then
                    samplePumpCalibration = New CytoSense.Calibration.SamplePump.SamplePumpCalibrationData(sampleSpeedRatio)
                End If

                Return samplePumpCalibration.getFlowSpeed(CByte(speed))
            End If
        End Function

        Public Function getTriggerLevel_mV(i As UInt16) As Double
            If hasFJDataElectronics Then
                Return triggerlevelConstant * i
            Else
                Return (triggerlevelConstant * i) + triggerlevelOffset
            End If
        End Function

        <ComponentModel.Browsable(False)>
        Public ReadOnly Property MaxForwardSpeed As Byte
            Get
                If hasDCSamplePump Then
                    Return 1
                Else
                    Return 13
                End If
            End Get
        End Property

        Public Function getHasLowHighChannelChecks() As Boolean
            If (Not Object.Equals(Nothing, PIC)) AndAlso PIC.I2CPMTLevelControl Then
                Return False
            End If
            If Not Object.Equals(Nothing, digitalOutputs) Then
                For i = 0 To digitalOutputs.Length - 1
                    If digitalOutputs(i).type = digitalOutput.DI_Type.ChannelHighLow OrElse digitalOutputs(i).name.ToUpper.Contains("LOW") Then
                        Return True
                    End If
                Next
            End If
            Return False
        End Function

        <NonSerialized()> Private _mV_lookup() As Integer
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property mV_lookup As Integer()
            Get
                If Object.Equals(Nothing, _mV_lookup) Then
                    If Not hasFJDataElectronics Then
                        _mV_lookup = CytoSense.Data.ParticleHandling.Channel.ChannelData.getmVLookup_RuudElectronics()
                    Else
                        _mV_lookup = CytoSense.Data.ParticleHandling.Channel.ChannelData.getmVLookup_FjElectronics()
                    End If
                End If
                Return _mV_lookup
            End Get
        End Property

        <ComponentModel.Browsable(False)>
        Public ReadOnly Property deLogCorrFactor As Single
            Get
                If Not hasFJDataElectronics Then
                    Return CSng(CytoSense.Data.ParticleHandling.Channel.ChannelData.getmDelogCorrectionFactor_RuudElectronics())
                Else
                    Return CytoSense.Data.ParticleHandling.Channel.ChannelData.getmDelogCorrectionFactor_FjElectronics()
                End If
            End Get
        End Property

        <NonSerialized()> Private _mV_lookupNew_LOCK As Object = New Object() ' Note need to do this in the OnDeserialized As well.
        ''' <summary>
        ''' Millivolt lookup table, the new version is already contains the de-log correction factor, so it is no
        ''' longer needed when using this version.  This can only be done when using the new smoothing, we still need
        ''' the old lookup table when using the old smoothing code.
        ''' </summary>
        ''' <remarks>After stepping through this I noticed that we get here in multiple threads at the same time,
        ''' that is not really an issue, except when generating, it causes some extra work, and it may cause issues
        ''' when replacing _mvLookup, so I rewrote it to lock and work on a temporary variable.
        ''' </remarks>
        <NonSerialized()> Private _mV_lookupNew()() As Single
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property mV_lookupNew As Single()()
            Get
                If _mV_lookupNew Is Nothing Then
                    SyncLock _mV_lookupNew_LOCK
                        If _mV_lookupNew Is Nothing Then
                            Dim tmp_mV_lookupNew(MeasurementSettings.LogConversion.THE_END - 1)() As Single

                            If Not hasFJDataElectronics Then
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.OriginalLog) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_RuudElectronics()
                            Else
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.OriginalLog) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.OriginalLog)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Decade16_3bLog) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Decade16_3bLog)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_low) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_low)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_1) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_1)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_2) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_2)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_3) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_3)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_4) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_4)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_5) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_5)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_6) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_6)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_shifted_7) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_shifted_7)
                                tmp_mV_lookupNew(MeasurementSettings.LogConversion.Linear8Bit_high) = Data.ParticleHandling.Channel.ChannelData.getNewmVLookup_FjElectronics(MeasurementSettings.LogConversion.Linear8Bit_high)
                            End If
                            _mV_lookupNew = tmp_mV_lookupNew
                        End If 'Else other thread allready calculated.
                    End SyncLock
                End If
                Return _mV_lookupNew
            End Get
        End Property

        ' In CytoUsb all simulator stuff should be handled in the detector using different logic so there is no
        ' need for the rest of CytoUsb to check this.
        <Obsolete("Do not use in CytoUsb, still allowed in CytoClus")>
        <Category("Instrument info"), DisplayName("Simulator"), DescriptionAttribute(""), ComponentModel.Browsable(True)>
        Public ReadOnly Property isSimulator As Boolean
            Get
                Return name.EndsWith(" SIM")
            End Get
        End Property


        Private _hasSeperateHighLowPMTs As PMTOptionsEnum

        ''' <summary>
        ''' Legacy function to find out if the instrument has separate high low PMT's. Since previously it was not saved explicitly to the CytoSettings, sometimes it has to be determined through other means...
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <ComponentModel.Browsable(False)>
        Public Property hasSeperateHighLowPMTs As Boolean
            Get
                If _hasSeperateHighLowPMTs = PMTOptionsEnum.unknown Then
                    For i = 0 To channels.Length - 1
                        If channels(i).name.Contains("LS") Then
                            Return True
                        End If
                    Next
                    Return False
                Else
                    Return (_hasSeperateHighLowPMTs = PMTOptionsEnum.AdjustablePMTs_seperateHighLowPMTS) Or (_hasSeperateHighLowPMTs = PMTOptionsEnum.Pico_Plankton_C)
                End If
            End Get
            Set(value As Boolean)
                _hasSeperateHighLowPMTs = If(value, PMTOptionsEnum.AdjustablePMTs_seperateHighLowPMTS, PMTOptionsEnum.unknown)
            End Set
        End Property

        ''' <summary>
        ''' An external battery voltage sensor, connected to same analog input as the original external pressure sensor was.
        ''' Currently this is only for the KAUST II machine, that is the first that has this, and probably the last.
        ''' </summary>
        Public hasExtBatteryVoltageSensor As Boolean = False

        ' There can only be one visualization mode active in the program, so we create a shared property.  And if
        ' the global visualization mode does not match the local one, then it means we need to clear the local data
        ' and update it.  Someone set the visualization mode through another instance of a CytoSettings object.
        <NonSerialized()> Private Shared _GLOBALChannelListMode As ChannelAccessMode = ChannelAccessMode.Normal 'non serialized so default is normal
        <NonSerialized()> Private _channelListMode As ChannelAccessMode = ChannelAccessMode.Normal 'non serialized so default is normal

        ''' <summary>
        ''' This function can be used to switch to a different mode in the way of visualizing channels. 
        ''' If for instance the separate FWS channels need to be visible, set the mode to debugging_optical
        ''' </summary>
        ''' <param name="c"></param>
        ''' <remarks></remarks>
        Public Sub setChannelVisualisationMode(c As ChannelAccessMode)
            If c <> _GLOBALChannelListMode Then
                _GLOBALChannelListMode = c
            End If
            UpdateLocalChannelVisualisationMode()
        End Sub


        <ComponentModel.Browsable(False)>
        Public ReadOnly Property getChannelVisualisationMode() As ChannelAccessMode
            Get
                UpdateLocalChannelVisualisationMode()
                Return _channelListMode
            End Get
        End Property


        ''' <summary>
        ''' We lock the channel list before updating, So we do no mess up the recalculating
        ''' stuff.
        ''' </summary>
        Private Sub UpdateLocalChannelVisualisationMode()
            If _GLOBALChannelListMode <> _channelListMode Then
                SyncLock _channelListLock
                    _channelListMode = _GLOBALChannelListMode
                    _channelList = Nothing
                End SyncLock
            End If
        End Sub


        <NonSerialized()> Private _channelListLock As New Object()
        <NonSerialized()> Private _channelList As List(Of ChannelWrapper)
        ''' <summary>
        ''' Retrieves a list of available channels, e.g. for visualization purposes. This list is used by the particle class in the DataFileWrapper,
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Use setChannelVisualisationMode to set different modes of visualization.
        ''' NOTE: There are some old files from the very first machines that were originally stored in a
        ''' completely different format.  These old files were converted and some errors were introduced,
        ''' they have the visibility of all the channels set to false.  So an empty channel list is generated,
        ''' so I made a special case for these files, and I treat them differently.  The visualization mode is not
        ''' important for these old files, as they do not contain these features.  I detect them by checking the
        ''' number of visible channels, if it is 0, then we have one of these special files, if it is not 
        ''' then not.
        ''' NOTE: THere was no locking and that caused a problem when calculating stuff for particles in parallel.
        ''' Oops.  Resulted in crashes sometimes, and may have resulted in bad data.
        ''' Added a lock works, but I worry about the cost, since the channel list is accessed for every particle.
        ''' So think I will go for a double checked locking pattern. Which also needs to be used 
        ''' by the UpdateLocalChannelVisionationMode function, because all accesses must be locked.
        ''' I think removing the visualization mode stuff would also improve things because all
        ''' the this checking probably takes a lot of time.
        ''' DOuble checked locking is kind of a hot topic on the internet, because compilers and processors are
        ''' allowed to reorder writes if they see no data dependency. And in this case that could in theory mean
        ''' that the _channelList is already written to with the new value before the array is completely initialized
        ''' Chances are small, but it could happen.  So in this case I will first create a temp variable and
        ''' then when that is done, place a memory barrier before assigning to the final field.  That barrier
        ''' makes sure no reordering happens.  And the cost is payed only when the channel list is actually
        ''' recalculated, and then it is not significant.
        ''' 
        ''' Tricky, the update check is for channelList Is Nothing Or count = 0, that is not completely
        ''' atomic. AFAICT Count = 0, should not happen, and the only way it can happen is when
        ''' the update function sets it to 0, and then it will do it again the next time.
        ''' NOTE: THe whole invalidating display mode stuff really messes things up.  Luckily when visualization
        ''' mode is changed, a reload is forced by the software, so that should not happen.
        ''' But if it ever should we could end up returning Nothing, which is BAD
        ''' Basically the code as is is broken, but as long as a visualization mode change forces a reload,
        ''' it should work out OK.
        ''' And we are working on a new file format, that should in the end make things better.
        ''' </remarks>
        <Category("Instrument info"), DisplayName("Channels"), DescriptionAttribute("List of useful channels the machine has"), ComponentModel.Browsable(False)>
        Public ReadOnly Property ChannelList As List(Of ChannelWrapper)
            Get
                UpdateLocalChannelVisualisationMode() ' Will set _channelList to Nothing if visualization mode changed!
                If _channelList Is Nothing Then
                    SyncLock _channelListLock
                        If _channelList Is Nothing Then
                            Dim tmpChannelList = CreateNewChannelList(_channelListMode)
                            Thread.MemoryBarrier() ' Ensure no reordering of memory instructions
                            _channelList = tmpChannelList
                        End If ' Else someone else allready calculated it.
                    End SyncLock
                End If ' Data available, just return it.
                Return _channelList
            End Get
        End Property


        ''' <summary>
        ''' Add all the normal mode channels to the list.  The list should be empty to start with, but
        ''' for symmetry with the other groups we make this an add as well.
        ''' </summary>
        ''' <param name="chnnlLst"></param>
        ''' <remarks>Code for these channels is the same as int he previous version, so the channel list
        ''' for normal mode is the same, and the versions are compatible for normal mode. For all the
        ''' other modes this is not the case, but advanced features were never supported for the other
        ''' modes anyway.</remarks>
        Private Sub AddNormalModeChannels( chnnlLst As List(Of ChannelWrapper) )
            Debug.Assert(chnnlLst.Count = 0) '  Current use requires we start with an empty list.

            If hasCurvature Then ' Combined FWS 
                chnnlLst.Add(New ChannelWrapper(New VirtualFWSCurvatureChannelInfo(1, 2), chnnlLst.Count, LineTypeEnum.line))
            End If
            'get a list of directly visible hardware channels that are not curvature FWs channels, dual focus channels or HF channels:
            For i = 0 To channels.Length - 1
                If channelIsVisibleButNotSpecial( channels(i) ) Then
                    chnnlLst.Add( New ChannelWrapper(i, chnnlLst.Count, channels(i), getChannelLineType(channels(i))))
                End If
            Next

            'add the rest of any available virtuals
            If hasDualLaserDistance Or name = "Izasa Acciona" Then 'Oh the legacy...
                For i = 0 To DualFocussedChannels.Length - 1
                    chnnlLst.Add(New ChannelWrapper(New VirtualDualFocusChannelInfo(channels(DualFocussedChannels(i)).name, channels(DualFocussedChannels(i)).color, VirtualChannelInfo.VirtualChannelType.DualFocusLeft, DualFocussedChannels(i)), chnnlLst.Count, LineTypeEnum.line))
                    chnnlLst.Add(New ChannelWrapper(New VirtualDualFocusChannelInfo(channels(DualFocussedChannels(i)).name, channels(DualFocussedChannels(i)).color, VirtualChannelInfo.VirtualChannelType.DualFocusRight, DualFocussedChannels(i)), chnnlLst.Count, LineTypeEnum.stripe))
                Next
            End If

            If hasSoftwareSubstractsLaserChannels Then
                For i = 0 To channels.Length - 1
                    If channels(i).IsHFplusLFchannel Then
                        Dim vc As New VirtualHFSummedChannelInfo(channels(i).name, channels(i).color, channels(i), channels(channels(i).LF_HardwareChannelIndex))
                        chnnlLst.Add(New ChannelWrapper(vc, chnnlLst.Count, LineTypeEnum.line))
                    End If
                    If channels(i).IsFilteredLFChannel Then
                        Dim vc As New VirtualLFFilteredChannelInfo(channels(i).name, channels(i).color, channels(channels(i).HF_HardwareChannelIndex), channels(i))
                        chnnlLst.Add(New ChannelWrapper(vc, chnnlLst.Count, LineTypeEnum.line))
                    End If
                Next
            End If
            If hasCurvature Then
                chnnlLst.Add(New ChannelWrapper(New VirtualCurvatureChannelInfo(1, 2), chnnlLst.Count, LineTypeEnum.line))
            End If
        End Sub

        ''' <summary>
        ''' Add the optical debugging channels to the end.  The indexes of the other channels will not
        ''' change so this will have no effect on the use of smart trigger or any other functionality
        ''' </summary>
        ''' <param name="chnnlLst"></param>
        Private Sub AddOpticalDebuggingChannels(chnnlLst As List(Of ChannelWrapper) )
            Debug.Assert(chnnlLst.Count > 0) '  Add normal channels first, so cannot be empty

            If Not hasCurvature Then
                Return ' No curvature, so no extra optical debugging channels to add, is easy.
            End If

            For i = 0 To channels.Length - 1
                Dim chan = channels(i)
                If chan.Channel_Type = ChannelTypesEnum.FWSL OrElse chan.Channel_Type = ChannelTypesEnum.FWSR Then
                    chnnlLst.Add( New ChannelWrapper(i, chnnlLst.Count, chan, getChannelLineType(chan)))
                End If
            Next
        End Sub

        ''' <summary>
        ''' For full debugging we add all the remaining hardware channels to the list, including
        ''' the invisible ones for DSP/Trigger/etc. 
        ''' </summary>
        ''' <param name="chnnlLst"></param>
        ''' <remarks>A bit trickier, we need to know which channels were not added before,
        ''' and add them.  Normal mode is all visible, except for FWS L/R, and the laser channels were HF / LF combined channels
        ''' were added.  Then in Optical debugging mode the FWS L and right channels were added.  SO basically we
        ''' now have to add all invisible channels and the HF/LF combined laser channels need to be added here.
        ''' </remarks>
        Private Sub AddFullDebuggingChannels(chnnlLst As List(Of ChannelWrapper) )
            Debug.Assert(chnnlLst.Count > 0) '  Add normal and optical debugging channels first, so cannot be empty

            For i = 0 To channels.Length - 1
                Dim chan = channels(i)
                If Not chan.visible OrElse chan.IsHFplusLFchannel OrElse chan.IsFilteredLFChannel Then
                    chnnlLst.Add( New ChannelWrapper(i, chnnlLst.Count, chan, getChannelLineType(chan)) )
                End If
            Next
        End Sub


        ''' <summary>
        ''' Create a list of channel wrappers for the specified access mode.
        ''' </summary>
        ''' <param name="mode"></param>
        ''' <returns></returns>
        Private Function CreateNewChannelList(mode As ChannelAccessMode) As List(Of ChannelWrapper)
            If channels.Count(Function(c) c.visible = True) = 0 Then
                Return GetChannelsForOldImportedFiles()
            ElseIf mode = ChannelAccessMode.Normal Then
                Dim result = New List(Of ChannelWrapper)()
                AddNormalModeChannels(result)
                Return result
            ElseIf _channelListMode = ChannelAccessMode.Optical_debugging Then
                Dim result = New List(Of ChannelWrapper)()
                AddNormalModeChannels(result)
                AddOpticalDebuggingChannels(result)
                Return result
            ElseIf _channelListMode = ChannelAccessMode.Full_debugging Then
                Dim result = New List(Of ChannelWrapper)()
                AddNormalModeChannels(result)
                AddOpticalDebuggingChannels(result)
                AddFullDebuggingChannels(result)
                Return result
            Else
                Throw New Exception("Unknown visialization mode!")
            End If
        End Function

        ''' <summary>
        ''' Returns all hardware channels that have a dual focus. Due to a legacy situation this function is needed and obligatory to use when looking for these types of channels
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private ReadOnly Property DualFocussedChannels As Integer()
            Get
                Dim res As New List(Of Integer)
                If SerNr = "CS-2011-39" Or SerNr = "CS-2012-42" And dllRelease.ReleaseDate < New DateTime(2013, 4, 21) Then 'Files from Acciona and Tartu machines have legacy files
                    res.Add(getChannelIndex("FL Red"))
                    Return res.ToArray
                End If

                For i = 0 To channels.Length - 1
                    If channels(i).DualFocus Then
                        res.Add(i)
                    End If
                Next

                Return res.ToArray
            End Get
        End Property

        Private Function GetChannelsForOldImportedFiles() As List(Of ChannelWrapper)
            Dim tempChannelList As New List(Of ChannelWrapper)
            Dim id As Integer = 0
            For i = 0 To channels.Length - 1
                If channels(i).Channel_Type <> ChannelTypesEnum.Trigger1 Then
                    tempChannelList.Add(New ChannelWrapper(i, id, channels(i), getChannelLineType(channels(i))))
                    id += 1
                End If
            Next
            Return tempChannelList
        End Function


        ''' <summary>
        ''' Checks if a hardware channel is to be visualized directly, or needs to be either embedded in an virtual channel or excluded completely
        ''' </summary>
        ''' <param name="thisChan"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function channelIsVisibleButNotSpecial(ByVal thisChan As channel) As Boolean
            'if a channel is visible (certain hardware channels used to be directly visualized, these were flagged with visible)
            If thisChan.visible = True Then

                'curvature channels need to be combined, so if this os a FWS L or FWS R channel exclude them. In some files FWS L may exist without a FWS R counter part (or vice versa), so also check the curvature flag
                If (Not (thisChan.Channel_Type = ChannelTypesEnum.FWSL Or thisChan.Channel_Type = ChannelTypesEnum.FWSR)) Or Not hasCurvature Then

                    'HF combined channels need to be combined... so exlude them
                    If (Not (thisChan.IsHFplusLFchannel Or thisChan.IsFilteredLFChannel)) Then
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        Public Function getChannelLineType(ByRef thisChan As channel) As LineTypeEnum
            If thisChan.highsensitivity Or thisChan.Channel_Type = ChannelTypesEnum.FWSR Then
                Return LineTypeEnum.stripe
            Else
                Return LineTypeEnum.line
            End If
        End Function

        Public Enum WaterDetectionEnum
            FTDI = 0
            PICDirect = 1
            PICI2C = 2
        End Enum


        ''' <summary>
        ''' Gets the channel list item with the corresponding name. May return Nothing if it is not found.
        ''' DIRTY HACK: Added a special case for FWS L or FWS R.  Since these can be queried for even if the visualization
        ''' mode is normal.
        ''' </summary>
        Public Function getChannellistItem(ByRef channelname As String) As ChannelWrapper
            Dim identifier As String = channelname 'Yes this is required for the predicate lambda expression
            Dim res =ChannelList.Find(Function(chan As ChannelWrapper)
                                           If chan.ToString = identifier Then
                                               Return True
                                           Else
                                               Return False
                                           End If
                                       End Function)
            If res Is Nothing And _channelListMode = ChannelAccessMode.Normal Then 'Special check for FWS L or FWS R
                Dim t = GetChannellistItemByType(ChannelTypesEnum.FWSL)
                If t.ToString = channelname Then
                    res = t
                Else
                    t = GetChannellistItemByType(ChannelTypesEnum.FWSR)
                    If t.ToString = channelname Then
                        res = t
                    End If
                End If
            End If
            Return res
        End Function

        ''' <summary>
        ''' ONLY use when getChannellistItem cannot be used and HS/LS does not matter. Returns a channel list item by type. May return nothing if the channel is not present or if the visualization mode is not suitable.
        ''' Strange the way that it differentiates between visualization modes.
        ''' </summary>
        Public Function GetChannellistItemByType(ByVal channeltype As ChannelTypesEnum) As ChannelWrapper
            UpdateLocalChannelVisualisationMode()
            If _channelListMode = ChannelAccessMode.Normal And (channeltype = ChannelTypesEnum.FWSL Or channeltype = ChannelTypesEnum.FWSR) Then
                For i = 0 To channels.Length - 1
                    If channeltype = channels(i).Channel_Type Then
                        Dim chn As ChannelWrapper = New ChannelWrapper(channels(i), -1, getChannelLineType(channels(i)))
                        chn.ChannelInfo.visible = False
                        Return chn
                    End If
                Next
                Return Nothing
            End If
            Return Me.ChannelList.Find(Function(chan As ChannelWrapper)
                                           If chan.Channeltype = channeltype Then
                                               Return True
                                           Else
                                               Return False
                                           End If
                                       End Function)
        End Function
        Public Enum ChannelSelectHSLS
            DontCare
            HS
            LS
        End Enum


        ''' <summary>
        ''' Returns a channel list item by type. May return nothing if the channel is not present or if the visualization mode is not suitable.
        ''' </summary>
        ''' <param name="channel">
        ''' !!!Better to use ChannelWrapper overload!!!  Does not discriminate between channels in the case of separate high-sensitivity and low-sensitivity channels.
        ''' </param>
        Public Function getChannellistItem(ByVal channel As ChannelWrapper) As ChannelWrapper
            UpdateLocalChannelVisualisationMode()
            If _channelListMode = ChannelAccessMode.Normal And (channel.Channeltype = ChannelTypesEnum.FWSL Or channel.Channeltype = ChannelTypesEnum.FWSR) Then
                For i = 0 To channels.Length - 1
                    If channel.Channeltype = channels(i).Channel_Type Then
                        Dim chn As ChannelWrapper = New ChannelWrapper(channels(i), -1, getChannelLineType(channels(i)))
                        chn.ChannelInfo.visible = False
                        Return chn
                    End If
                Next
                Return Nothing
            End If
            Return Me.ChannelList.Find(Function(chan As ChannelWrapper)
                                           If chan.Channeltype = channel.Channeltype And chan.ChannelInfo.highsensitivity = channel.ChannelInfo.highsensitivity Then
                                               Return True
                                           Else
                                               Return False
                                           End If
                                       End Function)
        End Function


        Public BackFlushTimeMode As BackflushTimeModeType = BackflushTimeModeType.Automatic 'Default is automagical.

        Public Overrides Function ToString() As String
            Return "Cytosettings: " & name
        End Function

        ''' <summary>
        ''' In CytoUsb, call this function loading/de-serializing from disk.  It will check if there are
        ''' new settings available in the default compiled settings object that are not yet in the 
        ''' one deserialized.  It does not check all, only some specific cases.
        ''' At the moment, only the laser description stuff. More may be added later.
        ''' 
        ''' This way we can add new settings, and make sure they are added to existing definitions without forcing people to reload all the
        ''' CytoSettings, which means loosing calibration and some other settings.
        ''' </summary>
        ''' <remarks>Use serNum when loading default settings, this allows e.g. loading from the original
        ''' settings when the serial number is changed for a simulator.</remarks>
        Public Sub InitializeNewSettings(tmpSettings As CytoSense.CytoSettings.CytoSenseSetting)
            ' Check if laser data is in the current object, and if not, check if it is available now in the software.
            If _laserInfo Is Nothing Then
                
                If tmpSettings._laserInfo IsNot Nothing Then 'We have laser data, so use it to set the one in the current version.
                    _numberOfLasers = tmpSettings._numberOfLasers
                    _laserInfo      = tmpSettings._laserInfo
                End If 'Else no laser data available, so nothing to do.
            End If 'Else have laser info nothing to do.

            ' Check the number of lasers.
            ' For older configs, this is done in de-serialize to fix when loading, but if you create a completely new
            ' settings object from scratch this is also set incorrectly. In theory I should go and edit all V10 configs,
            ' but I fix it here, not as pretty, but should work.
            If _numberOfLasers = 0 Then 'Not initialized, for V2 electronics, if not initialized, we can make a guess.
                If hasFJDataElectronics Then
                    If CytoUSBSettings.DisableLaser2PowerSensor Then
                        _numberOfLasers = 1
                    Else
                        _numberOfLasers = 2
                    End If
                End If 'Else V08 electronics, no way to know. (most of the time, so we do not even try)
            End If ' Else a number was configured, so leave it alone.

        End Sub


        <Category("Instrument info"), DisplayName("Number Of Lasers"), Description("The number of lasers in the instrument"), Browsable(True)>
        Public Property NumberOfLasers As Integer
            Get
                Return _numberOfLasers
            End Get
            Set(value As Integer)
                _numberOfLasers = value
            End Set
        End Property

        <Category("Instrument info"), DisplayName("Lasers"), DescriptionAttribute("List of lasers in the instrument"), Browsable(True)>
        Public Property LaserInfo As LaserInfoT()
            Get
                Return _laserInfo
            End Get
            Set(value As LaserInfoT())
                _laserInfo = value
            End Set
        End Property

        Private _numberOfLasers As Integer
        Private _laserInfo() As LaserInfoT

    End Class

    Public Enum PMTOptionsEnum
        unknown = 0
        Pico_Plankton_A = 1 'Not switchable
        Pico_Plankton_B = 2 'Switchable
        Pico_Plankton_C = 3 'Not switchable
        AdjustablePMTs = 4 'Bytewise switchable
        AdjustablePMTs_seperateHighLowPMTS = 5 'Combination of the above
    End Enum

    Public Enum SheathEnum
        FreshWater
        FilteredSeaWater
    End Enum

    ''' <summary>
    ''' The structure below stores all the configuration for the biocide module, this way it will be included in the
    ''' data files.  So we can always check the data files to see what was configured.
    ''' It will contain user settings, but also system configuration info that is only useful for us. (e.g. I2C address).
    ''' There are also some support functions here that should probably be moved
    ''' somewhere else, e.g. the BiocideModule structure from PIC Control.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class BiocideModuleOptionsT
        Public Enabled As Boolean

        ' Some EEPROM configuration
        Public PumpIo As IoPinCfg
        Public PumpSpeed As UShort
        Public SolenoidPump As Boolean 'Set to True to indicate that the system has a solenoid pump.
        Public MaxInjectPressure As Single 'Maximum pressure that the solenoid pump can handle on inject, or NaN when value can not be loaded.

        ' User Configuration
        Public ReservoirVolume As UInt32
        Public ReservoirConcentration As UInt32
        Public ReservoirWarnLevel As Byte

        Public SheathVolume As UShort
        Public TargetConcentration As UShort
        Public MixTime As Byte
        Public PumpedVolume As UInt32
        Public EstimatedSheathConcentration As UShort

        Public Shared Function EstimateConcentration_ppm(startConcentration_ppm As UShort, sheethVolume_mL As UShort, pumpVolume_muL As UInt32) As UShort
            Dim pumpVolume_ml = pumpVolume_muL \ 1000 ' Get the number of total milliliters, 
            Dim perMlFraction = (sheethVolume_mL - 1) / sheethVolume_mL
            Return CUShort(Math.Pow(perMlFraction, pumpVolume_ml) * startConcentration_ppm)
        End Function

        '  We want targetConcentration,
        '  We have a sheath volume with a current concentration, we want to inject an amount
        '  with the biocide concentration and the new concentration should be the target concentration.
        '
        '                        (sheatvolume-amountToAdd)*currentConcentration + amountToAdd*biocideConcentration
        'new Concentration =   ------------------------------------------------------------------------------------ = targetConcentration
        '                                           sheatvolume         
        '
        'Rewriting/organizing this equation results in:
        '
        '                       Ctarget - Ccurr
        '   Vadd =  Vsheath * -------------------
        '                        Cbio - Ccurr

        Public Shared Function BiocideToAdd_mul(sheatVolume_ml As Integer, concCurr_ppm As Integer, concReservoir_ppm As UInteger, concTarget_ppm As Integer) As UInt32
            If concCurr_ppm >= concTarget_ppm Then
                Return 0
            End If
            Dim VSheath_mul As Double = CDbl(sheatVolume_ml) * 1000
            Dim cCurr_ppm As Double = CDbl(concCurr_ppm)
            Dim cRes_ppm As Double = CDbl(concReservoir_ppm)
            Dim cTarg_ppm As Double = CDbl(concTarget_ppm)
            Return CUInt( VSheath_mul * (cTarg_ppm - cCurr_ppm) / (cRes_ppm - cCurr_ppm) )
        End Function

        '                        (sheatvolume-amountToAdd)*currentConcentration + amountToAdd*biocideConcentration
        'new Concentration =   ------------------------------------------------------------------------------------ = targetConcentration
        '                                           sheatvolume         
        Public Shared Function EstimateConcentrationAfterInjection_ppm(startConcentration_ppm As UShort, sheatVolume_ml As UShort, biocideConcentration_ppm As UInt32, amountToAdd_muL As UInt32) As UShort
            Dim vSheath_mul As Double = CDbl(sheatVolume_ml) * 1000
            Dim cCurr_ppm As Double = CDbl(startConcentration_ppm)
            Dim cBio_ppm As Double = CDbl(biocideConcentration_ppm)
            Dim vAdd_mul As Double = CDbl(amountToAdd_muL)
            Dim cNew As Double = ((vSheath_mul - vAdd_mul) * cCurr_ppm + vAdd_mul * cBio_ppm) / vSheath_mul
            Return CUShort(cNew)
        End Function
    End Class

    ''' <summary>
    ''' All options for the beads module, Most are loaded from the EEPROM configurations, but the new options for CytoSubShallow beads
    ''' configuration (currently only VLIZ) Are not stored in the EEPROM for now. They are set only in the Machines.vb.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class BeadsModuleOptionsT

        ' Some EEPROM configuration
        Public MagnetIO As IoPinCfg
        Public PinchIO As IoPinCfg

        Public MixOn As Byte
        Public MixOff As Byte

        ' User Configuration
        Public Enabled As Boolean
        Public BeadsVolume As UInt32
        Public BeadsConcentration As UShort
        Public BeadsWarnLevel As Byte

        Public MixTime As UShort ' Should byte, but unfortunately there are files out there that have shorts, so for now I changed it back to minutes.
        Public InsertMuL As UShort 'Micro liters to insert for a beads measurement. NOTE: Number of magnets for shallow sub case, 

        'CytoSeub Shallow options:
        'These options are not pretty and could maybe be improved (a lot) but for a one off they should do).
        'For now I try to use magnets so I can ignore exact speed settings and stuff.
        Public ReverseTransportNumMagnets    As Integer ' Num magnets to keep turning revers until all the beads are in the sample pump line.
        Public TransportToInjectorNumMagnets As Integer 'Num milliseconds to run the sample pump at a fixed speed So the beads are close to the injector.
        Public UseDeflatePinchValve As Boolean 'Use a deflate pinchvalve for injection even if it is not a complete shallow sub.
    End Class

    ''' <summary>
    ''' All options for the external filter module
    ''' </summary>
    ''' <remarks>NOTE: I am not sure we really want them all in the experiment, on the other hand,
    ''' a few extra bytes will not hurt, and who knows it may save us sometime.
    ''' NOTE: I put the Present flag in here as well, instead of using the Is Null test,
    ''' that allows us to reduce the interface for the module control class to just
    ''' this object instead of the whole settings object.
    ''' Perhaps we can create a base class with the other options classes, that
    ''' way we can share some code.
    ''' </remarks>
    <Serializable()> _
    Public Class ExternalFilterModuleOptionsT
        'User Configuration
        Public Enabled As Boolean           'If disabled, then the other values are undefined, it could be e.g. that the thing is disconnected!
        Public SwitchingPressure As UShort
        Public Filter1State As ExternalFilterState
        Public Filter2State As ExternalFilterState
        Public Waste As ExternalFilterWastePosition
        'EEPROM Configuration, not sure we really want/need this?
        Public Present As Boolean = False ' If false then all the other values are undetermined!
        Public InputFilterValve_pos1 As IoPinCfg
        Public InputFilterValve_pos2 As IoPinCfg
        Public FilterWasteValve_pos1 As IoPinCfg
        Public FilterWasteValve_pos2 As IoPinCfg
        Public WasteValve_pos1 As IoPinCfg
        Public WasteValve_pos2 As IoPinCfg
        Public Switch1Io As IoPinCfg
        Public Switch2Io As IoPinCfg
        Public PressureSensor As PressureSensoreCfg
    End Class

    ''' <summary>
    ''' All options for the carbon filter module
    ''' </summary>
    ''' <remarks>NOTE: I am not sure we really want them all in the experiment, on the other hand,
    ''' a few extra bytes will not hurt, and who knows it may save us sometime.
    ''' NOTE: I put the Present flag in here as well, instead of using the Is Null test,
    ''' that allows us to reduce the interface for the module control class to just
    ''' this object instead of the whole settings object.
    ''' 'Perhaps we can create a base class with the other options classes, that
    ''' way we can share some code.
    ''' </remarks>
    <Serializable()> _
    Public Class CarbonFilterModuleOptionsT
        'User Configuration
        Public Enabled As Boolean           'If disabled, then the other values are undefined, it could be e.g. that the thing is disconnected!
        Public FilterTime As UShort
        'EEPROM Configuration, not sure we really want/need this?
        Public Present As Boolean = False ' If false then all the other values are undetermined! (For V10, if false, then options structure will be Nothing)
        Public InLoopIO As IoPinCfg
        Public BypassIO As IoPinCfg
        Public ValvePowerIO As IoPinCfg ' Set to Invalid if we have no valve power stuff.
    End Class

    Public Structure OverrideSettings
        Public ReadOnly OverrideBeamWidthEnabled As Boolean
        Public ReadOnly OverrideSampleCoreSpeedEnabled As Boolean
        Public ReadOnly OverrideMuPixelEnabled As Boolean
        Public ReadOnly OverrideBeamWidth As Single
        Public ReadOnly OverrideSampleCoreSpeed As Single
        Public ReadOnly OverrideMuPixel As Single

        Public Sub New(_enableOverrideBeamWidth As Boolean, _overrideBeamWidth As Single,_enableOverrideCoreSpeed As Boolean, _overrideCoreSpeed As Single, _enableOverrideMuPixel As Boolean, _overrideMuPixel As Single)
            OverrideBeamWidthEnabled = _enableOverrideBeamWidth
            OverrideBeamWidth = _overrideBeamWidth
            OverrideSampleCoreSpeedEnabled = _enableOverrideCoreSpeed
            OverrideSampleCoreSpeed = _overrideCoreSpeed
            OverrideMuPixelEnabled = _enableOverrideMuPixel
            OverrideMuPixel = _overrideMuPixel
        End Sub
    End Structure

End Namespace

