Imports System.Runtime.Serialization

<Serializable()> Public Class SensorHealthLimits

    ''' <summary>
    ''' Default constructor, only needed for the serialization/deserialization
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
    End Sub

    Public Sub New(corespeed As Double)
        DetectorNoiseLevel = New SensorLimit(0, 10) 'mV
        PressureAbs = New SensorLimit(-100, 500 * (2.2 / corespeed)) 'mB
        PressureDiff = New SensorLimit(-500 * (2.2 / corespeed), 100) 'mB
        SheathTemp = New SensorLimit(3, 35) 'degC
        SystemTemp = New SensorLimit(3, 40) 'degC
        PMTTemp = New SensorLimit(3, 40) 'degC
        BuoyTemp = New SensorLimit(1, 50) 'degC
        LaserTemp = New SensorLimit(3, 40) 'degC
        PressureExt = New SensorLimit(-1, 20) 'Bar
        ExtSupplyPowerVoltage = New SensorLimit(12.5, 15) 'V
        IntVoltage = New SensorLimit(11, 15) 'V
        IntRecharge = New SensorLimit(-1, 700) 'A
        ExtBatteryVoltage = New SensorLimit(12, 16.8) 'Volt.  NOTE: battery has more levels, 1 is disconnected, I think we can put hte limit at 5, below is disconnected, battery can never go below that.

        LaserDiodeTemperature = New SensorLimit(23, 27) 'degC
        LaserDiodeCurrent     = New SensorLimit(150, 220) 'degC
        LaserTecLoad          = New SensorLimit(0,80) ' percentage
        LaserInputVoltage     = New SensorLimit(4.5,5.3) 'Volt

        CameraSensorTemperature = New SensorLimit(0, 50)
        CameraBodyTemperature   = New SensorLimit(0, 50)
    End Sub

    ''' <summary>
    ''' Default initialize the new sensor limits before deserializing to make sure they
    ''' always exist even when loading older datafiles.
    ''' </summary>
    ''' <param name="sc"></param>
    <OnDeserializing>
    Private Sub OnDeserializing(sc As StreamingContext)
        _laserDiodeTemperature = New SensorLimit(23, 27) 'degC
        _laserDiodeCurrent     = New SensorLimit(0, 220) 'degC
        _laserTecLoad          = New SensorLimit(0,80) ' percentage
        _laserInputVoltage     = New SensorLimit(4.4,5.3) 'Volt
    End Sub

    ''' <summary>
    ''' This is not good enough, detector levels should be per channel
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DetectorNoiseLevel As SensorLimit

    ''' <summary>
    ''' degC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SheathTemp As SensorLimit

    ''' <summary>
    ''' degC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SystemTemp As SensorLimit

    ''' <summary>
    ''' degC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PMTTemp As SensorLimit


    ''' <summary>
    ''' degC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property BuoyTemp As SensorLimit

    ''' <summary>
    ''' degC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LaserTemp As SensorLimit


    ''' <summary>
    ''' mB
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PressureAbs As SensorLimit

    ''' <summary>
    ''' mB
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PressureDiff As SensorLimit

    ''' <summary>
    ''' Bar
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PressureExt As SensorLimit

    ''' <summary>
    ''' V
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ExtSupplyPowerVoltage As SensorLimit

    ''' <summary>
    ''' V
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IntVoltage As SensorLimit

    ''' <summary>
    ''' V
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IntRecharge As SensorLimit

    Public Property ExtBatteryVoltage As SensorLimit

    Public Property LaserDiodeTemperature As SensorLimit
    Public Property LaserDiodeCurrent     As SensorLimit
    Public Property LaserTecLoad          As SensorLimit
    Public Property LaserInputVoltage     As SensorLimit

    Public Property CameraSensorTemperature As SensorLimit
    Public Property CameraBodyTemperature   As SensorLimit

    <Serializable()> Public Structure SensorLimit
        Public Sub New(minValue As Double, maxValue As Double)
            Me.minValue = minValue
            Me.maxValue = maxValue
        End Sub

        Dim maxValue As Double
        Dim minValue As Double

        Public Function Check(value As Double) As Boolean
            If Double.IsNaN(value) Then 'ftdi communication problems... ignore
                Return True
            End If
            Return maxValue > value AndAlso minValue <= value
        End Function
    End Structure
End Class

