''' <summary>
''' Configuration and settings for the DPS module.  All these values are actually stored and maintained in
''' the DPS module itself.  But we store them here as well so they are stored inside the datafile and can
''' be inspected when analyzing the data. 
''' 
''' Apart from containing the settings, also counters for number of start/stop running hours for
''' the motor, etc. are loaded each time we connect to the DPS.  That way the datafile will also
''' contain usage information on the DPS.
''' </summary>
<Serializable()> _
Public Class DpsModuleSettings
    Public Connected       As Boolean ' Is the DpsModule connected at the moment or not.
    Public SerialNumber    As String  ' The serial number of the DPS module
    Public FirmwareVersion As String  ' The firmware version of the DPS Module.

    Public DisableDps      As Boolean  ' Set this if you do not want to use a connected DPS.

#Region "Usage Counters"
    Public PowerOnCount              As UInteger
    Public PowerOnTime               As TimeSpan
    Public MotorStartCount           As UInteger
    Public MotorOnTime               As TimeSpan
    Public MotorManualStartCount     As UInteger
    Public MotorManualOnTime         As TimeSpan
    Public PumpStartCount            As UInteger
    Public PumpRunTime               As TimeSpan
    Public ManualOverrideCount       As UInteger
    Public EmergencyButtonCount      As UInteger
    Public EmergencyEndStopUpCount   As UInteger
    Public EmergencyEndStopDownCount As UInteger
#End Region ' "Usage Counters"

    Public DepthConversionOffset As Single  ' Offset in meters of the home position, NEGATIVE values are above water.
    Public DepthConversionFactor As Single  'Multiplication factor to go fro meters to counter values. 

    Public TubingFlushTime        As TimeSpan
    Public SampleChamberFlushTime As TimeSpan 'Unused at the moment.


    Public MinimumSamplingDepth As Single 
    Public MaximumSamplingDepth As Single

    Public EnableBacklight As Boolean ' Turn back light on.   
    Public DisablePump As Boolean ' Disable the PUMP, used for testing only, this is a CytoUsb setting, not used directly in the firmware, although it is stored there.

    Public Sub New()
    End Sub

    Public Sub New(other As DpsModuleSettings)
        Connected       = other.Connected
        SerialNumber    = other.SerialNumber
        FirmwareVersion = other.FirmwareVersion

        DisableDps      = other.DisableDps

        PowerOnCount               = other.PowerOnCount
        PowerOnTime                = other.PowerOnTime
        MotorStartCount            = other.MotorStartCount
        MotorOnTime                = other.MotorOnTime
        MotorManualStartCount      = other.MotorManualStartCount
        MotorManualOnTime          = other.MotorManualOnTime
        ManualOverrideCount        = other.ManualOverrideCount
        EmergencyButtonCount       = other.EmergencyButtonCount
        EmergencyEndStopUpCount    = other.EmergencyEndStopUpCount
        EmergencyEndStopDownCount  = other.EmergencyEndStopDownCount

        DepthConversionOffset = other.DepthConversionOffset
        DepthConversionFactor = other.DepthConversionFactor

        TubingFlushTime        = other.TubingFlushTime
        SampleChamberFlushTime = other.SampleChamberFlushTime

        MinimumSamplingDepth = other.MinimumSamplingDepth
        MaximumSamplingDepth = other.MaximumSamplingDepth

        EnableBacklight = other.EnableBacklight
    End Sub
End Class
