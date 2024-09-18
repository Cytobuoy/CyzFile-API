
Imports System.Runtime.Serialization
Imports CytoSense.Remote

Namespace CytoUSBSettings
    Public Enum LowerTabpages
        Schedule
        RealTimeData
        Advanced_settings
        Image_in_Flow
        IIF_Terminal
    End Enum

    ''' <summary>
    ''' THe action to take for a fatal exception, or an aborted measurement.  We consider both these
    ''' options fatal. 
    ''' </summary>
    Public Enum FatalErrorAction
        Invalid = 0
        DoNothing
        RestartCytoUsb
        RebootComputer
    End Enum


    <Serializable()> Public Class CytoUSBSettings

        Public release As New Serializing.VersionTrackableClass(New Date(2012, 10, 8))
        Public CytoUSBVersion As String 'added on 2011 8 3
        Public DebugMode As Boolean 'not used anymore
        Public DisableBlower As Boolean
        Public DisableExternalBlower As Boolean
        Public DisableSheathPump As Boolean
        Public DisableSamplePump As Boolean
        Public DisableLaser As Boolean
        Public DisableLaser2 As Boolean
        Public DisableHighVoltage As Boolean
        Public DisablePreAmps As Boolean
        Public DisableCirculationPump As Boolean
        Public DisableValvePower As Boolean
        Public DisableDSP As Boolean
        Public DisableBypass As Boolean
        Public DisableExternalPinchValve As Boolean
        Public DisableDeflatePinchValve As Boolean = False
        Public DisableBeadsPinchValve As Boolean = False
        Public ChannelVisualisationMode As CytoSense.CytoSettings.ChannelAccessMode
        Public DisableCharger As Boolean
        Public DisableValvedSamplingChamber As Boolean
        Public DisableExternalPump As Boolean

        'pay attention here, when adding fields, to also update AtLeastOneDeviceDisabled is necessary!

        Public DisableLaserTempSensor As Boolean
        Public DisablePMTTempSensor As Boolean
        Public DisableSystemTempSensor As Boolean
        Public DisableSheathTempSensor As Boolean
        Public DisableAbsPressureSensor As Boolean ' NOTE: For older pressure sensors this is channel 0, for newer flow through this is channel 1
        Public DisableDiffPressureSensor As Boolean ' NOTE: For older pressure sensors this is channel 1, for newer flow through this is channel 0
        Public DisableExtPressureSensor As Boolean
        Public DisableFTDISensorBoard As Boolean

        'New sensors for the power board
        Public DisableMainPowerSensor As Boolean       = False
        Public DisableChargerPowerSensor As Boolean    = False
        Public DisableBatteryPowerSensor As Boolean    = False
        Public DisableExtBatteryPowerSensor As Boolean = False
        Public DisableEmbeddedPcPowerSensor As Boolean = False
        Public DisableLaser1PowerSensor As Boolean     = False
        Public DisableLaser2PowerSensor As Boolean     = False
        Public DisableSparePowerSensor As Boolean      = False
        Public DisableAnalogExtBatteryPowerSensor As Boolean = False
        Public DisableWaterSensor As Boolean                 = False
        Public DisableLaser1Sensor As Boolean     = False
        Public DisableLaser2Sensor As Boolean     = False

        'pay attention here, when adding fields, to also update AtLeastOneSensorDisabled is necessary!

        Public DisableSheathFlowSensor As Boolean            = False
        Public DisableSheathSpeedSensor As Boolean           = False
        Public DisableParticleRateSensor As Boolean          = False

        Public EnableChargerWhenLaser As Boolean
        Public EnableStabilitizer As Boolean
        Public EnableAutoStart As Boolean

        Public ErrorAction As FatalErrorAction = FatalErrorAction.DoNothing

        Public Function AtLeastOneActuatorDisabled() As Boolean
            Return DisableBlower Or DisableExternalBlower Or DisableSheathPump Or DisableSheathPump Or DisableSamplePump Or DisableLaser Or DisableHighVoltage Or DisablePreAmps Or DisableCirculationPump Or DisableValvePower Or DisableDSP Or DisableBypass Or DisableCharger
        End Function
        Public Function AtLeastOneSensorDisabled() As Boolean
            Return DisableLaserTempSensor Or DisablePMTTempSensor Or DisableSystemTempSensor Or DisableSheathTempSensor Or DisableAbsPressureSensor Or DisableDiffPressureSensor Or DisableExtPressureSensor Or DisableFTDISensorBoard Or DisableLaser1Sensor Or DisableLaser2Sensor 

        End Function

        Public LowerTabPage As LowerTabpages

        Public GraphSettings As New List(Of CytoSense.PlotSettings.GraphPlotSettings)
        Public ScatterSettings As New List(Of CytoSense.PlotSettings.ScatterPlotSettings)
        Public Sub New()
            ScatterSettings.Add(New CytoSense.PlotSettings.ScatterPlotSettings)
            GraphSettings.Add(New CytoSense.PlotSettings.GraphPlotSettings)
        End Sub


        Public Sub setPMTLevelPresets(pmtL As Byte(), pmtM As Byte(), pmtH As Byte())
            ReDim PMTLevelPresets(2)

            PMTLevelPresets(0) = pmtL
            PMTLevelPresets(1) = pmtM
            PMTLevelPresets(2) = pmtH

        End Sub
        Public PMTLevelPresets()() As Byte

        Public RemoteSettings As New RemoteSettings()

        <OnDeserialized> _
        Private Sub OnDeserialized(sc As StreamingContext)
            If ErrorAction = FatalErrorAction.Invalid Then
                ErrorAction = FatalErrorAction.DoNothing
            End If
        End Sub

    End Class

    ''' <summary>
    ''' We currently support only a single custom server specification, in the future we should allow a list of
    ''' these remote settings and allow the user to choose from them, for now we do not.
    ''' </summary>
    <Serializable()> Public Class RemoteSettings
        Dim _CustomServerAddress As String
        Dim _Bandwidth As Integer = 1000
        Dim _iifFileSync As iifFileSyncMode
        Dim _iifSyncLocation As String
        Private _transferProtocol  As TransferProtocol
        Private _windowsShareName  As String
        ' NOTE: Username and password are stored using the windows credential manager, NOT in here.
        Private _destinationFolder As String
        Private _limitBandwidth    As Boolean
        Private _port              As Integer
        ' NOTE: Delay shutdown was incorrectly added by me as an integer, a copy/paste error probably.  It should be a boolean,
        ' unfortunately at that time we did have the Strict compiler option set in VB, so VB happily did all the conversions without telling
        ' us.  Now I feel hesitant to change the type because it could result in problems loading older datafiles.  So I will do the conversions
        ' myself, 0 ==> False, all else is True.
        Private _delayShutdown     As Integer 
        Private _maxDelayTime      As TimeSpan = TimeSpan.FromMinutes(10)

        Private Const FALSE_VALUE As Integer = 0
        Private Const TRUE_VALUE As Integer  = 1 ' NOTE: Never use true for testing, we set it explicitely, but we only test for the FALSE value.
                                                 ' Depending on which automatic conversion function was used, true could be 1, but also -1 (The VB style).
                                                 ' SO the only value we can depend on is false, which is always 0.

        <OnDeserialized>
        Private Sub OnDeserialized(sc As StreamingContext)
            If _delayShutdown = FALSE_VALUE AndAlso _maxDelayTime = TimeSpan.Zero Then
                _maxDelayTime = TimeSpan.FromMinutes(10) '  When loading existing CytoSettings, set the default timeout 10 minutes. 0 makes no sense anyway.
            End If
        End Sub

        Public Property Protocol As TransferProtocol
            Get
                Return _transferProtocol
            End Get
            Set(value As TransferProtocol)
                _transferProtocol = value
            End Set
        End Property

        Public Property WindowsShare As String
            Get
                Return _windowsShareName
            End Get
            Set(value As String)
                _windowsShareName = value
            End Set
        End Property

        Public Property DestinationFolder As String
            Get
                Return _destinationFolder
            End Get
            Set(value As String)
                _destinationFolder = value
            End Set
        End Property

        <Obsolete()> Public Property LimitBandwidth As Boolean
            Get
                Return _limitBandwidth
            End Get
            Set(value As Boolean)
                _limitBandwidth = value
            End Set
        End Property

        <Obsolete()>
        Public Property IifSyncLocation As String
            Get
                Return _iifSyncLocation
            End Get
            Set(value As String)
                _iifSyncLocation = value
            End Set
        End Property

        <Obsolete()>
        Public Property IifFileSync As iifFileSyncMode
            Get
                Return _iifFileSync
            End Get
            Set(value As iifFileSyncMode)
                _iifFileSync = value
            End Set
        End Property

        <Obsolete()> Public Property BandWidth As Integer
            Get
                Return _Bandwidth
            End Get
            Set(value As Integer)
                _Bandwidth = value
            End Set
        End Property

        Public Property CustomServerAddress As String
            Get
                Return _CustomServerAddress
            End Get
            Set(value As String)
                _CustomServerAddress = value
            End Set
        End Property

        Public Property Port As Integer
            Get
                Return _port
            End Get
            Set(value As Integer)
                _port = value
            End Set
        End Property

        Public Property DelayShutdown As Boolean
            Get
                Return If(_delayShutdown=FALSE_VALUE,False,True)
            End Get
            Set(value As Boolean)
                _delayShutdown = If(value, TRUE_VALUE, FALSE_VALUE)
            End Set
        End Property

        Public property MaxDelayTime As TimeSpan
            Get
                Return _maxDelayTime
            End Get
            Set(value As TimeSpan)
                _maxDelayTime = value
            End Set
        End Property

    End Class

    <Serializable()> Public Enum iifFileSyncMode
        Disabled = 0
        CytoBuoyServer = 1
        CustomServer = 2
    End Enum

End Namespace
