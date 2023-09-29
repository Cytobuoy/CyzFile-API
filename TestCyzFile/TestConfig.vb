Imports CytoSense
Imports CytoSense.CytoSettings
Imports System.Drawing

''' <summary>
''' For some tests we need actual machine configurations.  This class contains the definition of several such instruments used for testing.
''' These configurations are loosely based on actual instruments, but there is no guarantee that they will actually match the current
''' configuration of any actual instruments.  They are only meant to be used for testing.
''' </summary>
Public Class TestConfig
        Private Shared _log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Public Const TestInstrument1_SerNr As String = "CS-1111-11"
        Public Const TestInstrument1_HardwareNr As String = "FT1111111"
        Public Shared Function TestInsturment1() As CytoSenseSetting
            Dim s As CytoSenseSetting = Defaults(ElectronicType.Ruud)
            s.name         = "TestInstrument1"
            s.SerialNumber = TestInstrument1_SerNr
            s.HardwareNr   = TestInstrument1_HardwareNr

            s.triggerlevelOffset   = -4.623715084
            s.triggerlevelConstant =  1.071641472

            s.SampleCorespeed = 2.2
            s.Channels = { _
                    New channel("Trigger1",       0, Color.Gray,        False, False), _
                    New channel("FWS L",        130, Color.Black,       True,  False), _
                    New channel("FWS R",        132, Color.Black,       True,  False), _
                    New channel("SWS HS",       134, Color.Blue,        True,  3, 0, 110, True ), _
                    New channel("FL Yellow HS", 136, Color.YellowGreen, True,  4, 0, 110, True ), _
                    New channel("FL Orange HS", 138, Color.Orange,      True,  5, 0, 110, True ), _
                    New channel("FL Red HS",    140, Color.DarkRed,     True,  6, 0, 120, True ), _
                    New channel("DSP",            0, Color.Gray,        False, False), _
                    New channel("Trigger2",       0, Color.Gray,        False, False), _
                    New channel("SWS LS",       146, Color.DarkBlue,    True,  8, 0, 110, False), _
                    New channel("FL Yellow LS", 148, Color.Yellow,      True,  1, 0, 110, False), _
                    New channel("FL Red LS",    150, Color.Red,         True,  2, 0, 110, False) _
                }
            s.hasSeperateHighLowPMTs = True
            s.hasCurvature = True
           'PMT ID   1    2    3    4    5    6    7    8 (Low, medium and High)
            s.CytoUSBSettings.setPMTLevelPresets( _
                {   60,  60,  50,  60,  60,  75,   0,  50}, _
                {   66,  74,  55,  81,  80, 100,   0,  39}, _
                {   90,  90,  66,  90,  90, 100,   0,  66} _
            )

            s.hasVref = True
            s.LaserWarmingUpTime = 80

            s.hasBypassPinchValve   = True
            s.hasExternalPinchValve = False

            s.digitalOutputs(0).Configure( "Bypass PinchValve",   False, digitalOutput.DI_Type.PinchValveBypass   )
            s.digitalOutputs(5).Configure( "DSP reset",           False, digitalOutput.DI_Type.DSPReset           )
            s.digitalOutputs(6).Configure( "DSP setup",           False, digitalOutput.DI_Type.DSPSetup           )

            s.iif = New IIFSettings( chan:=s.channels(3), gain:=6.02, magnification:=16)

            s.iif.minimumRequiredDSPVersion = "20110223"
            s.DSPRecognizeString            = "USB Serial Port (COM" 
            s.DSPRS232FTDICode              = "FTDSP111"
            s.DSPUseFTDICodeForRecognition  = True
            s.iif.RS232Handshaking          = IO.Ports.Handshake.RequestToSend
            s.iif.CamPowerThroughUSB        = True
            s.hasImageAndFlow               = True

            s.PIC = New PICSettings()
            s.setHasPic(True)
            s.PIC.ConcentrationCounter    = True
            s.EnableHardwareConcentration = False 

            s.PIC.I2C = True
            s.PIC.I2CTemp_Sheath     = True
            s.PIC.I2CTemp_PMT        = True
            s.PIC.I2CTemp_Laser      = True
            s.PIC.I2CAbsPressure     = True
            s.PIC.I2CDiffPressure    = True
            s.PIC.I2CPMTLevelControl = True
            s.PIC.I2CFTDIPowerSwitch = True
            s.PIC.HallSensor         = True
            s.PIC.I2CSheathPump      = True
            s.PIC.DefaultSheathSpeed = 66.3 

            s.hasDCSamplePump = True
            s.DCSamplePump = Calibration.SamplePump.DCSamplePump.DefaultCalibration()

            s.machineConfigurationRelease = New CytoSense.Serializing.VersionTrackableClass( New Date(2019, 3, 11) )
            Return s
        End Function



        Public Enum ElectronicType
            None
            Ruud
            V10
            V10_PwrV2   ' V10 electronics, with powerboard V2
        End Enum

        Public Shared Function Defaults( fjElectronics As ElectronicType) As CytoSenseSetting 

            _log.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name)

            Dim s As New CytoSenseSetting
            s.name = "Defaults"
            s.LaserBeamWidth = 5
            s.SampleCorespeed = 2.2
            s.thresholdPCT = 0.5
            s.BuoyLifeTime = 0
            s.hasSubmode = False
            s.HasExternalTrigger = False
            s.ExternalTriggerIsPulse = False
            s.ExternalTriggerFeedbackLed = 6

            s.EnableLoggin = True
            s.EnableExternalLogging = False
            s.EnableClearLogBeforeMeasurement = True
            s.EnableHardwareConcentration = True
            s.EnableAutoDeviceDetect = True
            s.EnableVoice = True
            s.EnableAutoPowerSave = True
            s.EnableAutoShutdownScheduleMode = True
            s.EnableBackFlushScheduleMode = False
            s.EnableStopPumps = True
            s.EnableCompressIIFImages = True
            s.EnableSaveUnmatchedIIFFoto = False
            s.EnableExternalTrigger = False
            s.EnableGVModule = False
            s.EnableIIFCamera = True
            's.EnableWaterDetectionAlert = True
            s.EnableSubmode = False
            s.EnableExternalPump = False
            s.EnableContinuMode = False
#Disable Warning BC40008 ' Type or member is obsolete
            s.EnableCameraBackgroundCalibration = True
#Enable Warning BC40008 ' Type or member is obsolete
            s.EnableDetectorNoiseLevelDetection = True

            s.ExternalSheathMode = False

            s.ExternalPumpTime = 120
            s.ExternalPumpTime_OverlapWithNormalFlush = 0

            s.State1SubModeTime = 15
            s.State2SubModeTime = 60
            s.State3SubModeTime = 50

            s.EnableWarmingUp = True
            s.EnableSeperateUSBProgram = False
            s.DisableResetDSP = False
            s.isSorter = False

            s.HWCIntervalText = 6000
            s.RealTimeDataViewTimerIntervalText = 500
            s.SeperateConcentrationText = 20 ' in s
            s.AutoPowerSafeTime = 600
            s.CheckIfEverythingIsOkTimerTextBox = 3
            s.LaserWarmingUpTime = 50
            s.FlushTime           = 50
            s.ManualBackflushTime = 120
            s.BackFlushTimeMode   = BackflushTimeModeType.Automatic

            s.Vref_V = 2.5

            s.CytoUSBSettings = New CytoUSBSettings.CytoUSBSettings



            Dim ch(6) As channel
            ch(0) = New channel("Trigger1", 0, Color.Gray, False, False)
            ch(1) = New channel("FWS", 130, Color.Black, True, False)
            ch(2) = New channel("SWS", 132, Color.Blue, True, False)
            ch(3) = New channel("FL Yellow", 134, Color.Yellow, True, False)
            ch(4) = New channel("FL Orange", 136, Color.Orange, True, False)
            ch(5) = New channel("FL Red", 138, Color.Red, True, False)
            ch(6) = New channel("FL Green", 140, Color.Green, True, False)
            s.channels = ch

            Dim DIO(7) As digitalOutput
            DIO(0) = New digitalOutput("Not used", False, New Point(321, 10), digitalOutput.DI_Type.UnUsed)
            DIO(1) = New digitalOutput("Not used", False, New Point(321, 26), digitalOutput.DI_Type.UnUsed)
            DIO(2) = New digitalOutput("Not used", False, New Point(321, 42), digitalOutput.DI_Type.UnUsed)
            DIO(3) = New digitalOutput("Not used", False, New Point(321, 58), digitalOutput.DI_Type.UnUsed)
            DIO(4) = New digitalOutput("Not used", False, New Point(321, 74), digitalOutput.DI_Type.UnUsed)
            DIO(5) = New digitalOutput("Not used", False, New Point(321, 90), digitalOutput.DI_Type.UnUsed)
            DIO(6) = New digitalOutput("Not used", False, New Point(321, 106), digitalOutput.DI_Type.UnUsed)
            DIO(7) = New digitalOutput("Not used", False, New Point(321, 122), digitalOutput.DI_Type.UnUsed)

            s.digitalOutputs = DIO


            s.SerialNumber = "CS-0000-00"
            s.HardwareNr = "Defaults"
            s.Softwareversion = 2
            s.EnableAutoDeviceDetect = True
            s.hasSheetPumpAdjust = False
            s.hasCurvature = False
            s.hasWaterDetectionSensor = False
            s.hasPressureSensor = False
            s.hasExternalBatterie = False
            s.hasExternalSupply = True
            s.hasExternalPinchValve = False
            s.hasImageAndFlow = False
            s.hasGVModule = False
            s.hasExternalPump = False
            s.hasVref = False
            s.hasBypassPinchValve = False
            s.hasDCSamplePump = False
            s.hasDualLaserDistance = False 
            s.hasSoftwareSubstractsLaserChannels = False

            s.sampleSpeedRatio = 0.00793
            s.triggerlevelConstant = 1.0
            s.triggerlevelOffset = 0

            s.lowtabs = New LowerTabs(0)

            s.MaximumBuoyLifeTime = -1   
            s.TabSettingsFilePath = ""



            s.setHasPic(False)
            s.PIC = New PICSettings()

            s.enableGPS = False
            s.comPortGPS = 6
            s.baudGPS = 9600



            s.FlushATime = 10
            s.FlushBTime = 0.4
            s.CleaningFromSource1 = 30
            s.CleaningFromSource2 = 180
            s.CleaningFromSource3 = 420
            s.FillingLoopTime = 15
            s.SampleToInjectorTime = 45
            s.PressurizingTime = 9 

            s.DSPRecognizeString = "Prolific USB-to-Serial Comm Port (COM"
            s.DSPRS232FTDICode = ""
            s.DSPUseFTDICodeForRecognition = False

            s.OffsetInternalTemperature = 0
            s.OffsetInternalVoltage = 0
            s.OffsetPressure = 0

            s.isShallowSub = False
            s.EnableShallowSubMode = False
            s.shallowIntPressureMin = 50 'mBar

            If fjElectronics = ElectronicType.V10 OrElse fjElectronics = ElectronicType.V10_PwrV2 Then 
                s.OffsetInternalTemperature      = 0
                s.OffsetInternalVoltage          = 0
                s.OffsetPressure                 = 0
                s.SampleCorespeed                = 2.2
                s.hasFJDataElectronics           = True
                s.SensorLimits.IntRecharge       = New SensorHealthLimits.SensorLimit(-1,2500)
                s.triggerlevelOffset             = 0
                s.triggerlevelConstant           = 8192 / 65536 
                s.hasCurvature                   = True
                s.setHasPic(True)
                s.PIC                            = New PICSettings()
                s.PIC.ConcentrationCounter       = True
                s.EnableHardwareConcentration    = False 
                s.PIC.I2CFTDIPowerSwitch         = False 
                s.PIC.I2C                        = True
                s.PIC.I2CFJDataElectronics       = True  
                s.PIC.Powerboard                 = True 
                s.PIC.I2CAbsPressure             = True
                s.PIC.I2CDiffPressure            = True
                s.PIC.HallSensor                 = True
                s.PIC.I2CPMTLevelControl         = True
                s.PIC.I2CSheathPump              = True
                s.PIC.I2CTemp_Sheath             = True
                s.PIC.I2CTemp_PMT                = True
                s.PIC.I2CTemp_Laser              = True
                s.PIC.I2CTemp_System             = True
                s.PIC.I2CSamplePump              = True
                s.PIC.FlowThroughPressureSensors = True
                s.hasDCSamplePump                = True
                s.DCSamplePump                   = Calibration.SamplePump.DCSamplePump.DefaultCalibration()

                s.digitalOutputs(0).Configure( "Bypass PinchValve",   False, digitalOutput.DI_Type.PinchValveBypass   )

                s.PIC.TurnOnTimer = True
                s.PIC.TurnOnTimerPolarity = PICSettings.TurnOnTimerPolarityEnum.Normal
                
                s.hasWaterDetectionSensor  = True
                s.WaterDetectionConnection = CytoSenseSetting.WaterDetectionEnum.PICDirect
                s.PIC.WaterSensor = True
            End If

            If fjElectronics = ElectronicType.V10  Then
                s.hasBypassPinchValve = True
                s.BypassPinchValveUlnNumber = 7
            End If

            
            If fjElectronics = ElectronicType.V10_PwrV2 Then
                s.SensorLimits.ExtSupplyPowerVoltage = New SensorHealthLimits.SensorLimit(11.5, 12.5)
                s.hasBypassPinchValve       = True
                s.BypassPinchValveUlnNumber = 3
            End If

            Return s
        End Function

        Public Const TestInstrument2_SerNr As String      = "CS-2222-22"
        Public Const TestInstrument2_HardwareNr As String = "FT2222222"

        Public Shared Function TestInstrument2() As CytoSenseSetting
            Dim s As CytoSenseSetting = Defaults(ElectronicType.V10_PwrV2)
            s.name         = "TestInstrument2"
            s.SerialNumber = TestInstrument2_SerNr
            s.HardwareNr   = TestInstrument2_HardwareNr

            s.channels = {
                New channel("FWS L",        Color.Black),
                New channel("FWS R",        Color.Black),
                New channel("SWS",          Color.Blue,    2, 0, 110),
                New channel("Fl Yellow",    Color.Yellow,  3, 0, 110),
                New channel("Fl Orange",    Color.Orange,  8, 0, 120),
                New channel("Fl Red",       Color.Red,     1, 0, 110)
            }

            'PMT ID     1    2    3    4    5    6    7,   8,   (Low, medium and High)
            s.CytoUSBSettings.setPMTLevelPresets(
                    {   75,  55,  75,  75,  75,  75,  75,  80 },
                    {   85,  60,  85,  85,  85,  85,  85,  90 },
                    {   95,  65,  95,  95,  95,  95,  95, 100 }
            )

            s.NumberOfLasers = 1
            s.LaserInfo = {
                New LaserInfoT("OBIS 488-60 LS","222222", 488, 60, 50, 5.5)
                }

            s.PIC.DefaultSheathSpeed          = 58.8

            s.hasDeflatePinchValvePropertyV10 = True
            s.DeflatePinchValveUlnNumber      = 6 
            s.hasBeadsPinchValvePropertyV10   = True
            s.BeadsPinchValveUlnNumber        = 5

            s.BeadsOptions = New BeadsModuleOptionsT()
            s.BeadsOptions.InsertMuL                     = 1
            s.BeadsOptions.ReverseTransportNumMagnets    = 4
            s.BeadsOptions.TransportToInjectorNumMagnets = 3

            s.hasSubmode              = False
            s.isShallowSub            = True

            s.PIC.PICIIF         = True
            s.hasImageAndFlow    = True
            s.iif                = New IIFSettings( gain:=6.08,  magnification:=15.7, flash:=9 )
            s.LaserWarmingUpTime = 120


            s.machineConfigurationRelease = New CytoSense.Serializing.VersionTrackableClass(New Date(2021,10,1))
            Return s
        End Function


        Public Const TestInstrument3_SerNr As String      = "CS-3333-33"
        Public Const TestInstrument3_HardwareNr As String = "FT3333333"

        ''' <summary>
        ''' Buoy machine, shallow sub with a waste because it will never go deep, so for software configuration
        ''' it is a normal sense, and not a sub.  But it has a water detector.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Laser 1: LS 488 60mW
        ''' Laser 2: LS 552 60mW
        ''' </remarks>
        Public Shared Function TestInstrument3() As CytoSenseSetting
            Dim s As CytoSenseSetting = Defaults(ElectronicType.V10)
            s.name         = "TestInstrument3"
            s.SerialNumber = TestInstrument3_SerNr
            s.HardwareNr   = TestInstrument3_HardwareNr

            s.channels = {
                New channel("FWS L",        Color.Black),
                New channel("FWS R",        Color.Black),
                New channel("SWS HS",       Color.Blue,    1, 0, 110),
                New channel("SWS LS",       Color.Blue,    4, 0, 110),
                New channel("FL Yellow HS", Color.Yellow,  2, 0, 110),
                New channel("FL Yellow LS", Color.Yellow,  5, 0, 110),
                New channel("Fl Orange",    Color.Orange,  3, 0, 110),
                New channel("FL Red HS",    Color.Red,     8, 0, 120),
                New channel("FL Red LS",    Color.Red,     6, 0, 110)
            }

            'PMT ID     1    2    3    4    5    6    7,   8,   (Low, medium and High)
            s.CytoUSBSettings.setPMTLevelPresets(
                    {   41,  78,  88,  35,  61,  71,  75, 110 },
                    {   51,  88,  98,  45,  71,  81,  85, 120 },
                    {   61,  98, 109,  55,  81,  91,  95, 120 }
            )


            s.PIC.PICIIF              = True
            s.PIC.DefaultSheathSpeed  = 67.1
            s.hasExternalBatterie     = True
            s.hasDeflatePinchValvePropertyV10 = True
            s.DeflatePinchValveUlnNumber      = 8 
            s.hasBeadsPinchValvePropertyV10   = True
            s.BeadsPinchValveUlnNumber        = 6
            s.PIC.ExternalSupplyPower = PICSettings.ExternalSupplyPowerModeEnum.InternalBattery_ExternalSupply
            s.hasImageAndFlow         = True
            s.iif                     = New IIFSettings( gain:=2.48,  magnification:=16, flash:=8 )
            s.LaserWarmingUpTime      = 120

            s.BeadsOptions = New BeadsModuleOptionsT()
            s.BeadsOptions.InsertMuL                     = 1
            s.BeadsOptions.ReverseTransportNumMagnets    = 4
            s.BeadsOptions.TransportToInjectorNumMagnets = 3

            s.CytoUSBSettings.DisableSparePowerSensor  = True

            s.machineConfigurationRelease = New CytoSense.Serializing.VersionTrackableClass(New Date(2020,  3, 10))
            Return s
        End Function



    ''' <summary>
    ''' Private on purposed, prevent instantionation.
    ''' </summary>
    Private Sub New()

    End Sub
End Class
