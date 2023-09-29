Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data.ParticleHandling.Channel

''' <summary>
''' Test the parameter calculations in the CytoSettings DLL.
''' </summary>
<TestClass()>
Public Class TestParameterCalculations
    Public ReadOnly LONG_SWS_DATA As Single() = New Single() {
        2.594074, 2.746666, 3.204444, 3.662221, 4.272592, 5.493332, 7.78222, 11.90222, 18.31111, 26.85629,
        36.9274, 46.99851, 56.61184, 65.91998, 75.38072, 83.77332, 90.94516, 97.65923, 104.9837, 113.9866,
        126.0415, 143.2844, 169.0726, 200.354, 233.314, 266.7318, 293.8932, 312.8147, 327.311, 330.2103,
        324.1066, 324.1066, 336.314, 361.9496, 400.0977, 450.4532, 506.1495, 552.9954, 588.7021, 615.5583,
        632.3436, 643.9406, 649.7391, 638.1421, 615.5583, 610.0651, 626.8502, 643.9406, 661.6413, 699.4843,
        744.6517, 771.3553, 785.3939, 792.4131, 785.3939, 764.4887, 731.2235, 686.6665, 644.2458, 610.3702,
        578.1732, 552.3851, 537.5836, 537.5836, 542.4666, 542.4666, 542.4666, 528.2755, 496.231, 457.7776,
        408.3377, 350.8103, 302.7436, 268.8681, 241.4014, 218.3599, 202.7955, 193.7925, 186.9259, 180.3644,
        172.4296, 161.9007, 147.0992, 125.7363, 101.6266, 79.80591, 61.64739, 46.38814, 34.02814, 25.63555,
        20.29481, 16.78518, 14.64889, 12.97037, 11.59703, 10.52889
    }

    Public ReadOnly LONG_SWS_LENGTH As Single = 19.95365

    Public ReadOnly LONG_SWS_VARLENGTH_25 As Single = 29.46394

    Public ReadOnly LONG_SWS_VARLENGTH_75 As Single = 11.67604

    Public ReadOnly FUNNY_SWS_DATA As Single() = New Single() {
        19.68444, 19.37926, 19.53185, 20.4474, 21.21037, 21.97333, 23.65185, 25.63555, 27.61925, 29.29777,
        30.36592, 30.6711, 30.36592, 29.14518, 26.5511, 24.10962, 21.66814, 18.92148, 16.48, 14.49629,
        12.51259, 10.52889, 8.850368, 7.934813
    }

    Public ReadOnly FUNNY_SWS_LENGTH As Single = 10.1776

    Public ReadOnly FUNNY_SWS_VARLENGTH_25 As Single = 12.61947

    Public ReadOnly FUNNY_SWS_VARLENGTH_75 As Single = 5.26033

    'Reverse particle so we have a test case where the end is misbehaving.
    Public ReadOnly FUNNY_SWS_REVERSE_DATA As Single() = New Single() {
        7.934813, 8.850368, 10.52889, 12.51259, 14.49629, 16.48, 18.92148, 21.66814, 24.10962, 26.5511, 29.14518, 30.36592,
        30.6711, 30.36592, 29.29777, 27.61925, 25.63555, 23.65185, 21.97333, 21.21037, 20.4474, 19.53185, 19.37926, 19.68444
    }

    Public ReadOnly FUNNY_SWS_REVERSE_LENGTH As Single = 10.1776

    'Reverse particle so we have a test case where the end is misbehaving.
    Public ReadOnly ALL_ABOVE_SWS_DATA As Single() = New Single() {
        0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.6103702, 0.6103702, 0.6103702,
        0.6103702, 0.6103702, 0.6103702, 0.6103702, 0.6103702, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679,
        0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679,
        0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679, 0.457777679
    }

    Public ReadOnly ALL_ABOVE_SWS_LENGTH As Single = 21.4329071

    Public ReadOnly ALL_ABOVE_SWS_VARLENGTH_75 As Single = 4.73578

    Public ReadOnly ALL_ZERO_DATA As Single() = New Single() {
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0
    }

    Public ReadOnly ALL_HUNDRED_DATA As Single() = New Single() {
        100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0,
        100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0,
        100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0,
        100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0, 100.0
    }

    Public ReadOnly SLOOT_13u25_PARTICLEID_16_FWS As Single() = New Single() {
        145.5,
        156.25,
        172.40625,
        190.65625,
        208.71875,
        222.46875,
        234.375,
        244.46875,
        250.46875,
        250.375,
        244.25,
        234.125,
        220.03125,
        204,
        186,
        166,
        153.333328
    }

    Public ReadOnly ALL_ZERO_LENGTH As Single = 21.4329071

    ''' <summary>
    ''' Create a Channel data hardware object containing the passed smoothed mvValues as
    ''' pulse shape.  The smoothed mv values to use for parameter calculation.
    ''' 
    ''' We create the object with dummy settings for CytoSettings, etc.  We make this an SWS 
    ''' channel regardless of the channel it was.  WE only use this for testing some parameter calculations
    ''' so not that important what channel we make. (Untill we start testing SWS Covariance then it
    ''' becomes more complex and this funciton cannot be used.)
    ''' </summary>
    ''' <param name="mvValues"></param>
    ''' <returns>A channel data object witht he specified pulse shape.</returns>
    Private Function CreateChannelDataHardware(mvValues As Single()) As ChannelData_Hardware
        Dim cs = TestConfig.Defaults(TestConfig.ElectronicType.Ruud)
        Return New ChannelData_Hardware(CytoSense.MeasurementSettings.LogConversion.OriginalLog, mvValues, cs.channels(2), cs, 0.0)
    End Function

    <TestMethod()> Public Sub TestLengthLongSws()
        Dim chan = CreateChannelDataHardware(LONG_SWS_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.Length)
        Assert.AreEqual(LONG_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyLengthSws()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.Length)
        Assert.AreEqual(FUNNY_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyLengthReverseSws()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_REVERSE_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.Length)
        Assert.AreEqual(FUNNY_SWS_REVERSE_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestAllAboveThreshold()
        Dim chan = CreateChannelDataHardware(ALL_ABOVE_SWS_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.Length)
        Assert.AreEqual(ALL_ABOVE_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestAllZeroThreshold()
        Dim chan = CreateChannelDataHardware(ALL_ZERO_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.Length)
        Assert.AreEqual(ALL_ZERO_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarLengthLongSws_25()
        Dim chan = CreateChannelDataHardware(LONG_SWS_DATA)
        ChannelData.VarLength = 25
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(LONG_SWS_VARLENGTH_25, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarLengthLongSws_75()
        Dim chan = CreateChannelDataHardware(LONG_SWS_DATA)
        ChannelData.VarLength = 75
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(LONG_SWS_VARLENGTH_75, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyVarLengthSws()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_DATA)
        ChannelData.VarLength = 50
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(FUNNY_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyVarLengthSws_25()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_DATA)
        ChannelData.VarLength = 25
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(FUNNY_SWS_VARLENGTH_25, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyVarLengthSws_75()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_DATA)
        ChannelData.VarLength = 75
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(FUNNY_SWS_VARLENGTH_75, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestFunnyVarLengthReverseSws()
        Dim chan = CreateChannelDataHardware(FUNNY_SWS_REVERSE_DATA)
        ChannelData.VarLength = 50
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(FUNNY_SWS_REVERSE_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarAllAboveThreshold()
        Dim chan = CreateChannelDataHardware(ALL_ABOVE_SWS_DATA)
        ChannelData.VarLength = 50
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(ALL_ABOVE_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarAllAboveThreshold_25()
        Dim chan = CreateChannelDataHardware(ALL_ABOVE_SWS_DATA)
        ChannelData.VarLength = 25
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(ALL_ABOVE_SWS_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarAllAboveThreshold_75()
        Dim chan = CreateChannelDataHardware(ALL_ABOVE_SWS_DATA)
        ChannelData.VarLength = 75
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(ALL_ABOVE_SWS_VARLENGTH_75, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestVarAllZeroThreshold()
        Dim chan = CreateChannelDataHardware(ALL_ZERO_DATA)
        ChannelData.VarLength = 50
        Dim act = chan.Parameter(ChannelData.ParameterSelector.VariableLength)
        Assert.AreEqual(ALL_ZERO_LENGTH, act, 0.0001)
    End Sub

    <TestMethod()> Public Sub TestAllZeroNumberOfCells()
        Dim chan = CreateChannelDataHardware(ALL_ZERO_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.NumberOfCells)
        Assert.AreEqual(0, act, 0)
    End Sub

    <TestMethod()> Public Sub TestAllHundredNumberOfCells()
        Dim chan = CreateChannelDataHardware(ALL_HUNDRED_DATA)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.NumberOfCells)
        Assert.AreEqual(0, act, 0)
    End Sub

    <TestMethod()> Public Sub TestNumberOfCells()
        Dim chan = CreateChannelDataHardware(SLOOT_13u25_PARTICLEID_16_FWS)
        Dim act = chan.Parameter(ChannelData.ParameterSelector.NumberOfCells)
        Assert.AreEqual(1.0148, act, 0.0001)
    End Sub
End Class
