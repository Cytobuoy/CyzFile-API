
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data.ParticleHandling.Channel

''' <summary>
''' Ádding some simple tests before modifying the GetParameter function.
''' (Actually, I modified the GetParameter function first, then decided to add the
''' tests.)  I just selected a particle from an Eijsden DataFile, and selected a few
''' channels. I first got the test to run with the old implementation. The next
''' step is that the new one is (approximately) the same.
''' </summary>
<TestClass()> Public Class TestChannelData

    Private Shared _cytoSettings As CytoSense.CytoSettings.CytoSenseSetting = TestConfig.TestInsturment1()
    Private Shared _chanInfo As CytoSense.CytoSettings.channel = _cytoSettings.Channels(3)

    Private Shared _part1SwsData As Byte() = {29,33,36,37,38,39,41,48,61,77,94,110,125,139,153,164,175,183,191,195,201,203,204,203,200,195,190,181,173,161,149,136,123,110,97,86,75,65,55,46,41,38,38,38,42,52,66,83,100,116,131,145,159,169,177,182,184,182,178,171,162,151,139,127,115,103,90,75,63,50,39,30,23,19,15,10}
    Private Shared _part1FlRedHsData As Byte() = {9,10,11,15,18,20,20,18,18,21,27,32,35,37,37,38,40,44,48,51,53,55,55,54,53,53,51,50,48,46,44,43,41,37,30,22,13,6,1,0,0,0,0,0,1,3,6,11,16,21,24,25,27,28,26,23,18,15,12,9,8,9,10,10,7,3,0,0,1,6,13,20,25,28,26,21}

    Private Shared _chanData()() As Byte 
    
'     = { 
       '{29,33,36,37,38,39,41,48,61,77,94,110,125,139,153,164,175,183,191,195,201,203,204,203,200,195,190,181,173,161,149,136,123,110,97,86,75,65,55,46,41,38,38,38,42,52,66,83,100,116,131,145,159,169,177,182,184,182,178,171,162,151,139,127,115,103,90,75,63,50,39,30,23,19,15,10},
'        {9,10,11,15,18,20,20,18,18,21,27,32,35,37,37,38,40,44,48,51,53,55,55,54,53,53,51,50,48,46,44,43,41,37,30,22,13,6,1,0,0,0,0,0,1,3,6,11,16,21,24,25,27,28,26,23,18,15,12,9,8,9,10,10,7,3,0,0,1,6,13,20,25,28,26,21} }

    Shared Sub New()
        ReDim _chanData(1)
        _chanData(SWSDATA) = _part1SwsData
        _chanData(REDDATA) = _part1FlRedHsData
    End Sub


    Private Const SWSDATA As Integer = 0
    Private Const REDDATA As Integer = 1


    <DataTestMethod>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Length,           5.1213)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Total,        11118.5976)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Maximum,        778.3907)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Average,        148.2378)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Inertia,          0.5401)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.CentreOfGravity, 31.3817)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.FillFactor, 0.3181)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Asymmetry,        0.1632)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.NumberOfCells, 2.6108)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.SampleLength,    41.2500)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.TimeOfArrival,    1.2340)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.First,            1.4990)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Last,             0.7648)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.Minimum,          0.7648)>
    <DataRow(SWSDATA, SWSDATA, ChannelData.ParameterSelector.SWSCOV,          51.9899)>
    _
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Length,          11.3688)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Total,          106.7794)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Maximum,          3.6128)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Average,          1.4086)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Inertia,          0.7200)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.CentreOfGravity, 31.1565)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.FillFactor, 0.6964)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Asymmetry,        0.1692)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.NumberOfCells, 1.9507)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.SampleLength,    41.2500)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.TimeOfArrival,    1.2340)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.First,            0.7005)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Last,             1.1380)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.Minimum,          0.5000)>
    <DataRow(REDDATA, SWSDATA, ChannelData.ParameterSelector.SWSCOV,           0.2457)>
    Public Sub TestGetParameters( dataIdx As Integer, swsIdx As Integer, par As ChannelData.ParameterSelector, expected As Double)
        Dim chan = New ChannelData_Hardware(CytoSense.MeasurementSettings.LogConversion.OriginalLog, _chanData(dataIdx), _chanInfo, _cytoSettings, 1.234)
        If dataIdx = swsIdx Then
            chan.sws = chan
        Else
            chan.sws = New ChannelData_Hardware(CytoSense.MeasurementSettings.LogConversion.OriginalLog, _chanData(swsIdx), _chanInfo, _cytoSettings, 1.234)
        End If
        Dim actual = chan.Parameter(par)
        Assert.AreEqual(expected, actual, 0.001)
    End Sub

End Class