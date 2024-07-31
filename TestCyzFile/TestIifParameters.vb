Imports CytoSense.Data.Analysis

''' <summary>
''' Test the creation of an IIF parameter class from the gatebased sets that are supported in CytoClus 4.
''' There are some issues with a ranged based set so I am adding tests for that one.  Maybe for the other sets 
''' as well.
''' And some rextangle sets, and also test combinations of these gates in a single set, this should be supported
''' as long as the gates used do not overlap.
''' </summary>
<TestClass()> 
Public Class TestIifParameters



    ''' <summary>
    ''' Range gates are created used rangegate builders, and the gates are created using:
    ''' 
    '''     Public Overrides Function getGate(xAxis As Axis, yAxis As Axis) As Gate
    '''        Return New RangeGate(xAxis, Math.Min(_pathPoints(0).X, _pathPoints(1).X), Math.Max(_pathPoints(0).X, _pathPoints(1).X))
    '''    End Function
    ''' And the Y axis range is created the same way, but then selecting min/max points from the Y axis.  
    ''' So a range gate allways has the first as min and the last as max.  Code seems OK, so we focus on everthing
    ''' from the constructor.
    ''' Unfortunately we need CytoSettings for a lot of things, so we just select one.
    ''' </summary>
    <TestMethod()>    
    Public Sub TestRangeBasedSet_1()
        Dim cytoSettings = TestConfig.TestInstrument3()
        Dim swsMaxAxis = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Total, cytoSettings)
        Dim gbSet As New GateBasedSet("TestSet1", System.Drawing.Color.Red)
        Dim rangeGate As Gate = New RangeGate(swsMaxAxis, 15.0,   5471.12) 
        gbSet.addGate(rangeGate)

        Dim iifParams As New CytoSense.MeasurementSettings.IIFParameters(cytoSettings, gbSet)

        Assert.AreEqual(9, iifParams.Channels.Length)
        Dim chan = iifParams.Channels(1)
        Assert.AreEqual("SWS HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)

        Assert.AreEqual(true,     chan.Parameters(1).enable)
        Assert.AreEqual("Total",  chan.Parameters(1).name)
        Assert.AreEqual(15.0F,    chan.Parameters(1).min)
        Assert.AreEqual(5471.12F, chan.Parameters(1).max)

        Assert.AreEqual(false,    chan.Parameters(2).enable)
        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)
    End Sub


    ''' <summary>
    ''' EMpty range, should not happen, but code accepts it OK.
    ''' </summary>
    <TestMethod()>    
    Public Sub TestRangeBasedSet_2()
        Dim cytoSettings = TestConfig.TestInstrument3()
        Dim swsMaxAxis = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Total, cytoSettings)
        Dim gbSet As New GateBasedSet("TestSet1", System.Drawing.Color.Red)
        Dim rangeGate As Gate = New RangeGate(swsMaxAxis, 16.0F,   15.0F) 
        gbSet.addGate(rangeGate)

        Dim iifParams As New CytoSense.MeasurementSettings.IIFParameters(cytoSettings, gbSet)

        Assert.AreEqual(9, iifParams.Channels.Length)
        Dim chan = iifParams.Channels(1)
        Assert.AreEqual("SWS HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)

        Assert.AreEqual(true,     chan.Parameters(1).enable)
        Assert.AreEqual("Total",  chan.Parameters(1).name)
        Assert.AreEqual(16.0F,    chan.Parameters(1).min)
        Assert.AreEqual(15.0F,    chan.Parameters(1).max)

        Assert.AreEqual(false,    chan.Parameters(2).enable)
        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)
    End Sub


    ''' <summary>
    ''' Multi Range, on different axis. (Multi range on the same axis should not exist).
    ''' </summary>
    <TestMethod()>    
    Public Sub TestRangeBasedSet_3()
        Dim cytoSettings      = TestConfig.TestInstrument3()
        Dim swsTotalAxis      = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Total, cytoSettings)
        Dim flYellowHsMaxAxis = New SingleAxis(cytoSettings.ChannelList(3),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Maximum, cytoSettings)
        Dim gbSet As New GateBasedSet("TestSet1", System.Drawing.Color.Red)
        Dim swsRangeGate As Gate    = New RangeGate(swsTotalAxis, 15.0,   5471.12) 
        Dim yellowRangeGate As Gate = New RangeGate(flYellowHsMaxAxis, 50.0F,   100.0F) 
        gbSet.addGate(swsRangeGate)
        gbSet.addGate(yellowRangeGate)

        Dim iifParams As New CytoSense.MeasurementSettings.IIFParameters(cytoSettings, gbSet)

        Assert.AreEqual(9, iifParams.Channels.Length)
        Dim chan = iifParams.Channels(1)
        Assert.AreEqual("SWS HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)

        Assert.AreEqual(true,     chan.Parameters(1).enable)
        Assert.AreEqual("Total",  chan.Parameters(1).name)
        Assert.AreEqual(15.0F,    chan.Parameters(1).min)
        Assert.AreEqual(5471.12F, chan.Parameters(1).max)

        Assert.AreEqual(false,    chan.Parameters(2).enable)

        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)

        chan = iifParams.Channels(3)
        Assert.AreEqual("FL Yellow HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)
        Assert.AreEqual(false,    chan.Parameters(1).enable)

        Assert.AreEqual(true,      chan.Parameters(2).enable)
        Assert.AreEqual("Maximum", chan.Parameters(2).name)
        Assert.AreEqual(50.0F,     chan.Parameters(2).min)
        Assert.AreEqual(100.0F,    chan.Parameters(2).max)

        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)
    End Sub

    ''' <summary>
    ''' Simple test method to verify construction from a rectangle based set.
    ''' We do not check all boundary conditions, just a few quick tests.
    ''' </summary>
    <TestMethod()>    
    Public Sub TestRectangleBasedSet_1()
        Dim cytoSettings = TestConfig.TestInstrument3()
        Dim swsMaxAxis       As Axis  = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Total, cytoSettings)
        Dim flYellowHsMaxAxis As Axis = New SingleAxis(cytoSettings.ChannelList(3),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Maximum, cytoSettings)

        Dim gbSet As New GateBasedSet("TestSet1", System.Drawing.Color.Red)
        Dim rectGate As Gate = New RectangleGate(swsMaxAxis, flYellowHsMaxAxis, 15.0F,   5471.12F, 50.0F,   100.0F)
        gbSet.addGate(rectGate)
        Dim iifParams As New CytoSense.MeasurementSettings.IIFParameters(cytoSettings, gbSet)

        Assert.AreEqual(9, iifParams.Channels.Length)
        Dim chan = iifParams.Channels(1)
        Assert.AreEqual("SWS HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)

        Assert.AreEqual(true,     chan.Parameters(1).enable)
        Assert.AreEqual("Total",  chan.Parameters(1).name)
        Assert.AreEqual(15.0F,    chan.Parameters(1).min)
        Assert.AreEqual(5471.12F, chan.Parameters(1).max)

        Assert.AreEqual(false,    chan.Parameters(2).enable)

        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)

        chan = iifParams.Channels(3)
        Assert.AreEqual("FL Yellow HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)
        Assert.AreEqual(false,    chan.Parameters(1).enable)

        Assert.AreEqual(true,      chan.Parameters(2).enable)
        Assert.AreEqual("Maximum", chan.Parameters(2).name)
        Assert.AreEqual(50.0F,     chan.Parameters(2).min)
        Assert.AreEqual(100.0F,    chan.Parameters(2).max)

        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)
    End Sub

    <TestMethod()>    
    Public Sub TestRectangleRangeBasedSet_1()
        Dim cytoSettings = TestConfig.TestInstrument3()
        Dim swsTotalAxis          As Axis  = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Total,  cytoSettings)
        Dim flYellowHsMaxAxis     As Axis = New SingleAxis(cytoSettings.ChannelList(3),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Maximum, cytoSettings)
        Dim swsLengthxAxis        As Axis = New SingleAxis(cytoSettings.ChannelList(1),CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector.Length,  cytoSettings)

        Dim gbSet As New GateBasedSet("TestSet1", System.Drawing.Color.Red)
        Dim rectGate As Gate = New RectangleGate(swsTotalAxis, flYellowHsMaxAxis, 15.0F,   5471.12F, 50.0F,   100.0F)
        gbSet.addGate(rectGate)
        Dim rangeGate As Gate = New RangeGate(swsLengthxAxis, 10.0F, 50.0F)
        gbSet.addGate(rangeGate)

        Dim iifParams As New CytoSense.MeasurementSettings.IIFParameters(cytoSettings, gbSet)

        Assert.AreEqual(9, iifParams.Channels.Length)
        Dim chan = iifParams.Channels(1)
        Assert.AreEqual("SWS HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(true,     chan.Parameters(0).enable)
        Assert.AreEqual("Length", chan.Parameters(0).name)
        Assert.AreEqual(10.0F,    chan.Parameters(0).min)
        Assert.AreEqual(50.0F,    chan.Parameters(0).max)

        Assert.AreEqual(true,     chan.Parameters(1).enable)
        Assert.AreEqual("Total",  chan.Parameters(1).name)
        Assert.AreEqual(15.0F,    chan.Parameters(1).min)
        Assert.AreEqual(5471.12F, chan.Parameters(1).max)

        Assert.AreEqual(false,    chan.Parameters(2).enable)

        Assert.AreEqual(false,    chan.Parameters(3).enable)
        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)

        chan = iifParams.Channels(3)
        Assert.AreEqual("FL Yellow HS", chan.name)
        Assert.AreEqual(10,       chan.Parameters.Length)
        Assert.AreEqual(false,    chan.Parameters(0).enable)
        Assert.AreEqual(false,    chan.Parameters(1).enable)

        Assert.AreEqual(true,      chan.Parameters(2).enable)
        Assert.AreEqual("Maximum", chan.Parameters(2).name)
        Assert.AreEqual(50.0F,     chan.Parameters(2).min)
        Assert.AreEqual(100.0F,    chan.Parameters(2).max)

        Assert.AreEqual(false,    chan.Parameters(4).enable)
        Assert.AreEqual(false,    chan.Parameters(5).enable)
        Assert.AreEqual(false,    chan.Parameters(6).enable)
        Assert.AreEqual(false,    chan.Parameters(7).enable)
        Assert.AreEqual(false,    chan.Parameters(8).enable)
        Assert.AreEqual(false,    chan.Parameters(9).enable)
    End Sub



End Class
