Imports System.Globalization
Imports System.Runtime.Serialization

Namespace CytoSettings

    <Obsolete()> <Flags()> _
    Public Enum StainingModuleTemperatureMode
        Off = 0
        Dye = 1
        IcubateLoop = 2
        DyeAndLoop = 3
    End Enum


    <Serializable()> _
    Public Structure LinearConversion_t
        Public Sub New(off As Single, slp As Single)
            Offset = off
            Slope = slp
        End Sub

        Public Function Convert(val As Byte) As Single
            Return Offset + val * Slope
        End Function

        Public Function Inverse(val As Single) As Byte
            val -= Offset
            val /= Slope
            Return CByte(Math.Round(val, 0, MidpointRounding.AwayFromZero))
        End Function

        Public Function AsStorageString() As String
            Return String.Format(CultureInfo.InvariantCulture, "{0},{1}", Offset, Slope)
        End Function

        Public Shared Function FromStorageString(storage As String) As LinearConversion_t
            Dim fields = storage.Split(","c)
            Dim conv As New LinearConversion_t()
            conv.Offset = Single.Parse(fields(0), CultureInfo.InvariantCulture.NumberFormat)
            conv.Slope = Single.Parse(fields(1), CultureInfo.InvariantCulture.NumberFormat)
            Return conv
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Return Equals(DirectCast(obj,LinearConversion_t))
        End Function
        Public Overloads Function Equals(other As LinearConversion_t) As Boolean
            Return Offset = other.Offset AndAlso
                   Slope  = other.Slope
        End Function
        Public Shared Operator=(lhs As LinearConversion_t, rhs As LinearConversion_t) As Boolean
            Return lhs.Equals(rhs)
        End Operator
        Public Shared Operator<>(lhs As LinearConversion_t, rhs As LinearConversion_t) As Boolean
            Return Not lhs.Equals(rhs)
        End Operator

        Public Offset As Single
        Public Slope As Single
    End Structure


    <Serializable()>
    Public Enum StainingModuleModel_t
        Invalid = 0
        BsmV1
        BsmV2
    End Enum


    ''' <summary>
    ''' Configuration of and settings for the bacterial staining module!
    ''' Both in one class, not really pretty I think, but for now should work.
    ''' And it is the way it currently works in CytoUsb.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> _
    Public Class StainingModuleSettings
        <Obsolete()> Public StainName As String = "" ' Name of the stain used, only for display purposes.
        <Obsolete()> Public AvailableDyeAmount As Short = 0 'MicroLiters of Dye left in the Staining Module. (Tracked by the module itself!)

        ' Which temperatures do we want to control inside the BSM, and what is there target temperature.
        <Obsolete()> Public TemperatureControlMode As StainingModuleTemperatureMode = StainingModuleTemperatureMode.Off
        <Obsolete()> Public DyeTargetTemperature As Double = 4.0
        Public LoopTargetTemperature As Double = 21.0
        'NOTE: Below is NOT used for priming anymore, only for inject, priming is done at the highest possible speed,
        'sp we do not need an actual variable to store that setting.
        <Obsolete>
        Public PrimeSamplePumpSpeedByte As Byte = 130 ' Wanted sample pump speed, use a value that can actually be set.
        Public LoopVolume As Integer = 1300 ' Volume of incubate loop in microliters
        Public PrimeStainingModuleSeconds As Integer = 30
        <Obsolete()> Public DyeInjectSeconds As Integer = 80
        <Obsolete()> Public DyeSpeedDac As Byte = 83 ' Byte speed setting for injecting the dye.
        <Obsolete()> Public DyeInjectAmount As Integer = 13 ' microliters of dye to insert, still unused.
        <Obsolete()> Public DyeToLoopSeconds As Integer = 10 ' Seconds to pump the last of the dye into the loop. 
        <Obsolete()> Public IncubateSeconds As Integer = 900 'Time to incubate the dye.
        Public PrimeCytoSenseSeconds As Integer = 45 ' Seconds to pump stained sample to the CytoSense

        <Obsolete>
        Public NoLoopCleanSeconds As Integer = 5
        Public LoopCleanSeconds As Integer = 20

        Public FirmwareVersion As String ' Used to store the firmware version in a measurement file.
        Public SerialNumber As String ' Serial number of the staining module.

        <Obsolete()> Public SamplePumpSpeedConversion As LinearConversion_t ' Convert speed byte to microliters per second.

        Public ControlLoopTemperature As Boolean  = False 'Set to true if we want to control the loop temperature.
        'Dye Unit 1
        Public DyeUnit1Present As Boolean            = False 'Indicate if dye unit 1 is present
        Public DyeUnit1Stain As String               = "" 'Name of stain in Dye1, only 30 bytes available for storing UTF8 string
        Public DyeUnit1IncubateSeconds As Integer    = 900 'Time to incubate the dye.
        Public DyeUnit1StainLeft As UInteger         = 0 ' Amount of Dye left
        Public DyeUnit1InjectAmount As UShort        = 0 ' Default amount to inject.
        Public DyeUnit1StainLeftWarning As UShort    = 0 ' If dye left gets below this level, a warning is displayed (somewhere)
        Public DyeUnit1ControlTemperature As Boolean = False
        Public DyeUnit1TargetTemperature As Double   = 4.0
        Public DyeUnit1PrimeCircuit As TimeSpan      = TimeSpan.FromMinutes(10) ' Prime time for the DyeCircuit 
        Public DyeUnit1PrimeDispense As TimeSpan     = TimeSpan.FromMinutes(5)  ' Prime time for the dispense part of the DyeCircuit 

        'Dye Unit 2
        Public DyeUnit2Present As Boolean            = False  'Indicate if dye unit 1 is present
        Public DyeUnit2Stain As String               = ""  'Name of stain in Dye1, only 30 bytes available for storing UTF8 string
        Public DyeUnit2IncubateSeconds As Integer    = 900 'Time to incubate the dye.
        Public DyeUnit2StainLeft As UInteger         = 0 ' Amount of Dye left
        Public DyeUnit2InjectAmount As UShort        = 0 ' Default amount to inject.
        Public DyeUnit2StainLeftWarning As UShort    = 0 ' If dye left gets below this level, a warning is displayed (somewhere)
        Public DyeUnit2ControlTemperature As Boolean = False
        Public DyeUnit2TargetTemperature As Double   = 4.0
        Public DyeUnit2PrimeCircuit As TimeSpan      = TimeSpan.FromMinutes(10) ' Prime time for the DyeCircuit 
        Public DyeUnit2PrimeDispense As TimeSpan     = TimeSpan.FromMinutes(5) ' Prime time for the dispense part of the DyeCircuit 


        Public Model As StainingModuleModel_t

        ''' <summary>
        ''' Initialize the Model to an invalid value before deserializing so we can be
        ''' detect if it was loaded afterwards.
        ''' </summary>
        ''' <param name="context"></param>
        <OnDeserializing()>
        Private Sub OnDeserializing(context As StreamingContext)
            Model = StainingModuleModel_t.Invalid
        End Sub

        ''' <summary>
        ''' After deserialization check the value of the model, and if it is set
        ''' to Invalid then it must be an older version, so we set it to
        ''' BsmV1
        ''' </summary>
        ''' <param name="context"></param>
        <OnDeserialized()>
        Private Sub OnDeserializied(context As StreamingContext)
            If Model = StainingModuleModel_t.Invalid Then
                Model = StainingModuleModel_t.BsmV1
            End If
        End Sub

    End Class

End Namespace
