Imports CytoSense.Data
Imports Force.Crc32

''' <summary>
''' Test functions for the datafile wrapper and datafilewrapperfunctions class.
''' </summary>
''' <remarks></remarks>
<TestClass()> Public Class TestDataFileWrapper

    Private _testContext As TestContext

    Public Property TestContext As TestContext
        Get
            Return _testContext
        End Get
        Set(ByVal value As TestContext)
            _testContext = value
        End Set
    End Property

    Public Function GetPropertyValue(ByVal obj As Object, ByVal PropName As String) As Object
        Dim objType As Type = obj.GetType()
        Dim pInfo As System.Reflection.PropertyInfo = objType.GetProperty(PropName)
        Dim PropValue As Object = pInfo.GetValue(obj, Reflection.BindingFlags.GetProperty, Nothing, Nothing, Nothing)
        Return PropValue
    End Function

    <DataTestMethod()>
    <DataRow("pumpedVolume", 734.4)>
    Public Sub TestSubSetSmartTriggerProperty(propName As String, expected As Double)
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Dim actual As Double = CDbl(GetPropertyValue(dfw,propName))
        Assert.AreEqual(expected, actual,            0.001)
    End Sub


    ' Different version have slightly diffent. analyzed volume due to the complex calculations
    'and resulting roundoff errors and variations.
    'Cannot easily use property by name because there are 2 properties named analyzedVolume...
    <TestMethod()>
    Public Sub TestSubSetSmartTriggerAnalyzedVolume()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(469.4161, dfw.analyzedVolume,            0.001)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerConcentration()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(34.9114,     dfw.Concentration,             0.001)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerSplittedParticlesLength()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(16388,    dfw.SplittedParticles.Length)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerNumberOfDownloadedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(784673,  dfw.MeasurementInfo.NumberofDownloadedParticles)
    End Sub


    <TestMethod()>
    Public Sub TestSubSetSmartTriggerWasReduced()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(True,  dfw.ReductionInfo.WasReduced)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerOriginalNumber()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(784673,  dfw.ReductionInfo.OriginalNumber)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerNumberSavedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(16388,    dfw.MeasurementInfo.NumberofSavedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestSubSetSmartTriggerNumberOfCountedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")
        Assert.AreEqual(1197382L,  dfw.MeasurementInfo.NumberofCountedParticles )
    End Sub


    ' An old file, where the number of particles smart triggered is set to 0, instead of
    ' the total number of particles.  Resulting in a bad concentration calculation.
    <TestMethod()>
    Public Sub TestConcentration_NumSmartTriggeredZero()
        Dim dfw = New DataFileWrapper("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")
        Assert.AreEqual(144.0875,     dfw.Concentration,             0.001)
    End Sub



    ' Non reduced smart trigger file test
    <TestMethod()>
    Public Sub TestSmartTriggerAnalyzedVolume()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(469.4161, dfw.analyzedVolume,            0.001)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerConcentration()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(173.7584,     dfw.Concentration,             0.001)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerSplittedParticlesLength()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(81564,    dfw.SplittedParticles.Length)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerNumberOfDownloadedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(784673,  dfw.MeasurementInfo.NumberofDownloadedParticles)
    End Sub


    <TestMethod()>
    Public Sub TestSmartTriggerWasReduced()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(False,  dfw.ReductionInfo.WasReduced)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerOriginalNumber()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(0,  dfw.ReductionInfo.OriginalNumber)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerNumberSavedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(81565,    dfw.MeasurementInfo.NumberofSavedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestSmartTriggerNumberOfCountedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")
        Assert.AreEqual(1197382L,  dfw.MeasurementInfo.NumberofCountedParticles )
    End Sub


    ' Reduced No Smart Trigger file test
    <TestMethod()>
    Public Sub TestReducedAnalyzedVolume()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(23.8877, dfw.analyzedVolume,            0.001)
    End Sub

    <TestMethod()>
    Public Sub TestReducedConcentration()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(22.3965,     dfw.Concentration,             0.001)
    End Sub

    <TestMethod()>
    Public Sub TestReducedSplittedParticlesLength()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(535,    dfw.SplittedParticles.Length)
    End Sub

    <TestMethod()>
    Public Sub TestReducedNumberOfDownloadedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(36851,  dfw.MeasurementInfo.NumberofDownloadedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestReducedWasReduced()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(True,  dfw.ReductionInfo.WasReduced)
    End Sub

    <TestMethod()>
    Public Sub TestReducedOriginalNumber()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
Assert.AreEqual(36851,  dfw.ReductionInfo.OriginalNumber)
    End Sub

    <TestMethod()>
    Public Sub TestReducedNumberSavedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(535,    dfw.MeasurementInfo.NumberofSavedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestReducedNumberOfCountedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(92820L,  dfw.MeasurementInfo.NumberofCountedParticles )
    End Sub

    ' Non Reduced, Non Smart Trigger File file test

    <TestMethod()>
    Public Sub TestConcentration()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(1542.6795,     dfw.Concentration,             0.001)
    End Sub

    <TestMethod()>
    Public Sub TestSplittedParticlesLength()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(36850,    dfw.SplittedParticles.Length)
    End Sub

    <TestMethod()>
    Public Sub TestNumberOfDownloadedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(36851,  dfw.MeasurementInfo.NumberofDownloadedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestWasReduced()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(False,  dfw.ReductionInfo.WasReduced)
End Sub

    <TestMethod()>
    Public Sub TestOriginalNumber()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(0,  dfw.ReductionInfo.OriginalNumber)
    End Sub

    <TestMethod()>
    Public Sub TestNumberSavedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")
        Assert.AreEqual(36851,    dfw.MeasurementInfo.NumberofSavedParticles)
    End Sub

    <TestMethod()>
    Public Sub TestNumberOfCountedParticles()
        Dim dfw = New DataFileWrapper("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")
        Assert.AreEqual(92820L,  dfw.MeasurementInfo.NumberofCountedParticles )
    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",          True, 0.6801,  19.6859, 3481, 100753)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",          True, 0.5125,  12.4003, 2623,  63465)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz", False,   -1.0,  22.3965,   -1 , 92820)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",            False,   -1.0, 144.0875,   -1, 868104)>
    Public Sub TestPreconcentrationConflict( filename As String, expConflict As Boolean, expPreconcentration As Double, expConcentration As Double, expPreParticleCount As Int64, expParticleCount As Int64)

        Dim dfw = New DataFileWrapper(filename)
        Dim actPreconcentration As Double = -1
        Dim actConcentration As Double  = -1
        Dim act = dfw.CheckConcentration(actConcentration, actPreconcentration)
        Assert.AreEqual(expConflict,act)
        If act Then
            Assert.AreEqual(expConcentration, actConcentration, 1e-3)
            Assert.AreEqual(expPreConcentration, actPreconcentration, 1e-3)

            dfw.ConcentrationMode = ConcentrationModeEnum.During_measurement_FTDI
            Assert.AreEqual( expConcentration, dfw.Concentration, 1e-3)
            Assert.AreEqual( expParticleCount, dfw.MeasurementInfo.NumberofCountedParticles)

            dfw.ConcentrationMode = ConcentrationModeEnum.Pre_measurement_FTDI
            Assert.AreEqual( expPreConcentration, dfw.Concentration, 1e-3)
            Assert.AreEqual( expPreParticleCount, dfw.MeasurementInfo.NumberofCountedParticles)
        Else
            Assert.AreEqual( expConcentration, dfw.Concentration, 1e-3)
            Assert.AreEqual( expParticleCount, dfw.MeasurementInfo.NumberofCountedParticles)
        End If
    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",          True)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",          True)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz", False)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",            False)>
    Public Sub TestPreconcentrationConflictException( filename As String, expConflict As Boolean)

        Dim dfw = New DataFileWrapper(filename)
        If expConflict Then
            ExceptionAssert.Throws(Of CytoSense.ConcentrationMisMatchException)(
                                    Sub()
                                        Dim d1 = dfw.Concentration()
                                    End Sub,
                                    Nothing)
            Else
                Dim d2 = dfw.Concentration()
            End If

    End Sub



    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           1.05)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  3.93)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     4.99)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     1.05)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", 2.24)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         2.04)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      2.04)>
    Public Sub TestConfiguredSamplePumpSpeed( filename As String, exp As Double )
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.MeasurementSettings.ConfiguredSamplePompSpeed
        Assert.AreEqual(exp, act, 1e-3)
    End Sub

    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            15)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                   36)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      61)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      15)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  32)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          27)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       27)>
    Public Sub TestConfiguredSamplePumpSpeedByte( filename As String, exp As Integer)
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.MeasurementSettings.ConfiguredSamplePumpSpeedSetting
        Assert.AreEqual(exp, act)
    End Sub

    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           1.05)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  3.93)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     4.99)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     1.05)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", 2.24)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         2.04)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      2.04)>
    Public Sub TestActualSamplePumpSpeed( filename As String, exp As Double )
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.MeasurementSettings.SamplePompSpeed
        Assert.AreEqual(exp, act, 1e-3)
    End Sub

    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            15)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                   36)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      61)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      15)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  32)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          27)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       27)>
    Public Sub TestActualSamplePumpSpeedByte( filename As String, exp As Integer)
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.MeasurementSettings.SamplePumpSpeedSetting
        Assert.AreEqual(exp, act)
    End Sub

    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           63)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  110.04)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     898.2)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     63)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", 474.88)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         734.4)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      734.4)>
    Public Sub TestPumpedVolume( filename As String, exp As Double )
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.pumpedVolume
        Assert.AreEqual(exp, act, 1e-3)
    End Sub

    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           23.8877)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  88.7891)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     897.6528)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     23.8877)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", 11.2227)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         469.4162)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      469.4162)>
    Public Sub TestAnalyzedVolume( filename As String, exp As Double )
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.analyzedVolume
        Assert.AreEqual(exp, act, 1e-3)
    End Sub


    <DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           1542.6795)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  1359.7505)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",       51.1701)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                       22.3965)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  461.3874)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          173.7584)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",        34.9115)>
    Public Sub TestParticleConcentration( filename As String, exp As Double )
        Dim dfw = New DataFileWrapper(filename)
        Dim act = dfw.Concentration
        Assert.AreEqual(exp, act, 1e-3)
    End Sub


   public class DoubleComparer
        Implements IComparer
       Public Sub New(delta As Double)
            _delta = delta
       End Sub
       Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim xDbl As Double = CDbl(x)
            Dim yDbl As Double = CDbl(y)

            If (Math.Abs(xDbl-yDbl) < _delta) Then
                Return 0
            Else If xDbl < yDbl Then
                Return -1
            Else
                Return 1
            End If
        End Function
        Private _delta As Double
    End Class

<DataTestMethod>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",  New Double(){
        0.0000, 0.0700, 0.1400, 0.2100, 0.2800, 0.3500, 0.4200, 0.4900, 0.5600, 0.6300, 
        0.7000, 0.7700, 0.8400, 0.9100, 0.9800, 1.0500, 1.1200, 1.1900, 1.2600, 1.3300, 
        1.4000, 1.4700, 1.5400, 1.6100, 1.6800, 1.7500, 1.8200, 1.8900, 1.9600, 2.0300, 
        2.1000, 2.1700, 2.2400, 2.3100, 2.3800, 2.4500, 2.5200, 2.5900, 2.6600, 2.7300, 
        2.8000, 2.8700, 2.9400, 3.0100, 3.0800, 3.1500, 3.2200, 3.2900, 3.3600, 3.4300, 
        3.5000, 3.5700, 3.6400, 3.7100, 3.7800, 3.8500, 3.9200, 3.9900, 4.0600, 4.1300, 
        4.2000, 4.2700, 4.3400, 4.4100, 4.4800, 4.5500, 4.6200, 4.6900, 4.7600, 4.8300, 
        4.9000, 4.9700, 5.0400, 5.1100, 5.1800, 5.2500, 5.3200, 5.3900, 5.4600, 5.5300, 
        5.6000, 5.6700, 5.7400, 5.8100, 5.8800, 5.9500, 6.0200, 6.0900, 6.1600, 6.2300, 
        6.3000, 6.3700, 6.4400, 6.5100, 6.5800, 6.6500, 6.7200, 6.7900, 6.8600, 6.9300, 
        7.0000, 7.0700, 7.1400, 7.2100, 7.2800, 7.3500, 7.4200, 7.4900, 7.5600, 7.6300, 
        7.7000, 7.7700, 7.8400, 7.9100, 7.9800, 8.0500, 8.1200, 8.1900, 8.2600, 8.3300,
        8.4000, 8.4700, 8.5400, 8.6100, 8.6800, 8.7500, 8.8200, 8.8900, 8.9600, 9.0300,
        9.1000, 9.1700, 9.2400, 9.3100, 9.3800, 9.4500, 9.5200, 9.5900, 9.6600, 9.7300,
        9.8000, 9.8700, 9.9400, 10.0100, 10.0800, 10.1500, 10.2200, 10.2900, 10.3600, 10.4300,
        10.5000, 10.5700, 10.6400, 10.7100, 10.7800, 10.8500, 10.9200, 10.9900, 11.0600, 11.1300,
        11.2000, 11.2700, 11.3400, 11.4100, 11.4800, 11.5500, 11.6200, 11.6900, 11.7600, 11.8300,
        11.9000, 11.9700, 12.0400, 12.1100, 12.1800, 12.2500, 12.3200, 12.3900, 12.4600, 12.5300,
        12.6000, 12.6700, 12.7400, 12.8100, 12.8800, 12.9500, 13.0200, 13.0900, 13.1600, 13.2300,
        13.3000, 13.3700, 13.4400, 13.5100, 13.5800, 13.6500, 13.7200, 13.7900, 13.8600, 13.9300,
        14.0000, 14.0700, 14.1400, 14.2100, 14.2800, 14.3500, 14.4200, 14.4900, 14.5600, 14.6300,
        14.7000, 14.7700, 14.8400, 14.9100, 14.9800, 15.0500, 15.1200, 15.1900, 15.2600, 15.3300,
        15.4000, 15.4700, 15.5400, 15.6100, 15.6800, 15.7500, 15.8200, 15.8900, 15.9600, 16.0300,
        16.1000, 16.1700, 16.2400, 16.3100, 16.3800, 16.4500, 16.5200, 16.5900, 16.6600, 16.7300,
        16.8000, 16.8700, 16.9400, 17.0100, 17.0800, 17.1500, 17.2200, 17.2900, 17.3600, 17.4300,
        17.5000, 17.5700, 17.6400, 17.7100, 17.7800, 17.8500         }
        )>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",  New Double(){
        0.0000, 0.1093, 0.2186, 0.3279, 0.4372, 0.5465, 0.6558, 0.7651, 0.8744, 0.9837,
        1.0930, 1.2023, 1.3116, 1.4209, 1.5302, 1.6395, 1.7488, 1.8581, 1.9674, 2.0767,
        2.1860, 2.2953, 2.4046, 2.5139, 2.6232, 2.7325, 2.8418, 2.9511, 3.0604, 3.1697,
        3.2790, 3.3883, 3.4976, 3.6069, 3.7162, 3.8255, 3.9348, 4.0441, 4.1534, 4.2627,
        4.3720, 4.4813, 4.5906, 4.6999, 4.8092, 4.9185, 5.0278, 5.1371, 5.2464, 5.3557,
        5.4650, 5.5743, 5.6836, 5.7929, 5.9022, 6.0115, 6.1208, 6.2301, 6.3394, 6.4487,
        6.5580, 6.6673, 6.7766, 6.8859, 6.9952, 7.1045, 7.2138, 7.3231, 7.4324, 7.5417,
        7.6510, 7.7603, 7.8696, 7.9789, 8.0882, 8.1975, 8.3068, 8.4161, 8.5254, 8.6347,
        8.7440, 8.8533, 8.9626, 9.0719, 9.1812, 9.2905, 9.3998, 9.5091, 9.6184, 9.7277,
        9.8370, 9.9463, 10.0556, 10.1649, 10.2742, 10.3835, 10.4928, 10.6021, 10.7114, 10.8207, 
        10.9300, 11.0393, 11.1486, 11.2579, 11.3672, 11.4765, 11.5858, 11.6951, 11.8044, 11.9137, 
        12.0230, 12.1323, 12.2416, 12.3509, 12.4602, 12.5695, 12.6788, 12.7881, 12.8974, 13.0067, 
        13.1160, 13.2253, 13.3346, 13.4439, 13.5532, 13.6625, 13.7718, 13.8811, 13.9904, 14.0997, 
        14.2090, 14.3183, 14.4276, 14.5369, 14.6462, 14.7555, 14.8648, 14.9741, 15.0834, 15.1927,
        15.3020, 15.4113, 15.5206, 15.6299, 15.7392, 15.8485, 15.9578, 16.0671, 16.1764, 16.2857,
        16.3950, 16.5043, 16.6136, 16.7229, 16.8322, 16.9415, 17.0508, 17.1601, 17.2694, 17.3787,
        17.4880, 17.5973, 17.7066, 17.8159, 17.9252, 18.0345, 18.1438, 18.2531, 18.3624, 18.4717,
        18.5810, 18.6903, 18.7996, 18.9089, 19.0182, 19.1275, 19.2368, 19.3461, 19.4554, 19.5647,
        19.6740, 19.7833, 19.8926, 20.0019, 20.1112, 20.2205, 20.3298, 20.4391, 20.5484, 20.6577,
        20.7670, 20.8763, 20.9856, 21.0949, 21.2042, 21.3135, 21.4228, 21.5321, 21.6414, 21.7508,
        21.8601, 21.9694, 22.0787, 22.1880, 22.2973, 22.4066, 22.5159, 22.6252, 22.7345, 22.8438,
        22.9531, 23.0624, 23.1717, 23.2810, 23.3903, 23.4996, 23.6089, 23.7182, 23.8275, 23.9368,
        24.0461, 24.1554, 24.2647, 24.3740, 24.4833, 24.5926, 24.7019, 24.8112, 24.9205, 25.0298,
        25.1391, 25.2484, 25.3577, 25.4670, 25.5763, 25.6856, 25.7949, 25.9042, 26.0135, 26.1228,
        26.2321, 26.3414, 26.4507, 26.5600, 26.6693, 26.7786, 26.8879, 26.9972, 27.1065, 27.2158,
        27.3251, 27.4344, 27.5437, 27.6530, 27.7623, 27.8716 }
    )>
    Public Sub TestGetFlowSpeed( filename As String, exp As Double())
        Dim dfw = New DataFileWrapper(filename)
        Dim act As Double()
        ReDim act(exp.Length-1)
        For i As Integer = 0 To act.Length-1
            act(i) = dfw.CytoSettings.getFlowSpeed(i)
        Next
        CollectionAssert.AreEqual(exp, act, New DoubleComparer(1e-3) )
    End Sub

    ' The original files stored the background image as windows System.Drawing.Bitmap.  This class is not
    ' available in .Net Core, so I am replacing it with a custom class.  The Bitmap internally stored
    ' the data as a JPEG file.  So in my own class I simply load that JPEG file and keep it in memory.
    ' For my check I took the background image and converted it to JPEG stream and calculated the Crc.
    ' So in the old case it was first loaded from the JPEG and then compressed to JPEG again, in the
    ' new case I simply have the data buffer with the JPEG data.  For older files, recompressing the 
    ' the image does not result in the exact same stream.  I do not know how big the difference is, but
    ' the Crcs differ. (It could only be some encoder version number difference, or it could be really
    ' different because of improvements in the encoder). Anyway the test fails.  If I explicitely take the
    ' data, load it into a windows bitmap, and then compress it to JPEG again to simulate the process
    ' I end up with the same Crc.  So that proves that my data is loaded correctly. 
    ' I then did a run with both methods of Crc calculation and stored the new Crcs in the table.
    ' So that I can do the actual loading verification in .net core as well.

    Private Shared _backgroundImageCrcs As Dictionary(Of String, Long) = New Dictionary(Of String, Long) From { 
        {"DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      &H705A3139L},  
        {"DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     &H78D9E916L},  
        {"DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                &H97A1F8E4L},  ' Old bitmap Crc: &H6E028BC8L
        {"DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", &H042E0A0AL},  
        {"DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         &H705A3139L},  
        {"DataFiles/pollen 2011-05-19 16u19.cyz",                                     &H1DF7546AL},  ' Old bitmap Crc: &H32976BEEL
        {"DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                 &H069EB400L}   ' Old bitmap Crc: &HC3B63BD2L
        }

    <DataTestMethod>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz")>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz")>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    Public Sub TestBackgroundImageCrcs(filename As String)
        Dim dfw = New DataFileWrapper(filename)
        Assert.IsTrue(dfw.CytoSettings.iif.OpenCvBackground IsNot Nothing)

        If Not _backgroundImageCrcs.ContainsKey(filename)  Then ' No Data, Just dump Crcs
#Disable Warning BC40000 ' Type or member is obsolete
            Dim crc As UInteger = Crc32Algorithm.Compute(dfw.CytoSettings.iif.Background.Data,0,dfw.CytoSettings.iif.Background.Data.Length)
#Enable Warning BC40000 ' Type or member is obsolete
            Dim lCrc As Long = CLng(crc)
            Debug.WriteLine("&H{0:X8}L,", lCrc)
            Assert.Fail(String.Format("Add Crc Information for this file: {0} ==> &H{1:X8}L", filename, lCrC))
        Else
            Dim expectedCrc= _backgroundImageCrcs(filename)

#Disable Warning BC40000 ' Type or member is obsolete
            Dim crc As UInteger = Crc32Algorithm.Compute(dfw.CytoSettings.iif.Background.Data,0,dfw.CytoSettings.iif.Background.Data.Length)
#Enable Warning BC40000 ' Type or member is obsolete
            Dim lCrc As Long = CLng(crc)
            Assert.AreEqual(expectedCrc,lCrc)
        End If
        
    End Sub

    '  Maaswater gevoeliger 256K 2012-10-10 12u03.cyz
    ' beads1.6u.cyz-2010-06-16 13-10.CYZ

    ''' <summary>
    ''' Very simple test, just check if we can open the datafiles without any errors occuring.
    ''' </summary>
    <DataTestMethod>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz")>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz")>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ")>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz")>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz")>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    Public Sub TestOpen(filename As String)
        Dim dfw = New DataFileWrapper(filename)
        Dim numParticles = dfw.SplittedParticles.Length ' 
        If numParticles > 0 Then
            Dim p1 = dfw.SplittedParticles(0) ' Force loading of all the particle Data.
        End If
        Assert.IsTrue(True) ' If we get here, all is well.
    End Sub


    ' This is a very old and very rare data format.  It is probably the first attempt to create a custom datafile format.
    ' It consists of a file serialized in multiple parts, a bit like the current segmented file format. But different.
    ' Until today I did not know it existed, and I am not sure it actually exists out there in "the wild".
    ' Of the approx. 40000 files I scanned on our servers, there were 4 in this format, no more
    ' For now I decided not to invest anymore time in this format, just report we do not support it.
    ' If it turns out there are actually significant data files that use this format we will look
    ' at supporting them.  The code can still be found in the repository for the CytoOLD DLL.
   <DataTestMethod>
   <DataRow("DataFiles/Measurement 2012-06-20 14u01.cyz")>
    Public Sub TestOpenSectioned(filename As String)
        Try
            Dim dfw = New DataFileWrapper(filename)
            Dim numParticles = dfw.SplittedParticles.Length ' 
            If numParticles > 0 Then
                Dim p1 = dfw.SplittedParticles(0) ' Force loading of all the particle Data.
            End If
            Assert.Fail("File is a sctioned file that is not supported, should have thrown.") ' If we get here, all is well.
        Catch ex As Exception
            If ex.InnerException IsNot Nothing AndAlso ex.InnerException.Message.StartsWith("Sectioned datafiles are not supported") Then
                Assert.IsTrue(True) ' We expect this because this format is not supported!
            Else    ' Another unexpected exception happend.
                Assert.Fail(String.Format("Expected 'Sectioned datafiles are not supported' error, got: '{0} ({1})'", ex.Message, If(ex.InnerException IsNot Nothing,ex.InnerException.Message,"")))
            End If
        End Try
    End Sub



    ' Some older files did not store JPEG streams, but serialized a Bitmap class instead, which internally
    ' converted it to JPEG, so it was bascially the same.  Anyway I replaced the bitmap class with my own
    ' and now simply keep the JPEG stream in the file, instead of loading it, and then resaving it as JPEG
    ' again.  However, because of some changes to the JPEG library (which is 10 years newer, I actually get
    ' different Crcs for these files.
    ' If I add the process of loading the original image into a Bitmap, and then export it as JPEG again
    ' I do get the original Crcs again.  To make the test run on .net core as well, ran the tests once with
    ' both methods of calculation, and then replaced the original Crcs with the new ones.
    ' I Kept the original Crcs in comments.

    Private Shared _imageCrcs As Dictionary(Of String, Long()) = New Dictionary(Of String, Long()) From {
        {"DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz", { &H00076C8EL, &HC3D34A47L, &HC67A7CAFL, &H6E23AEB3L, &HFF32D77EL, &HE93026BAL, &H750935BCL, &HFE7E9DE1L, &HBD2802CEL, &HC3568907L, &H9BE751FDL} },
        {"DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz", { } },
        {"DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz", { } },
        {"DataFiles/BeadsCalibration 2017-02-08 13h50.cyz", { } },
        {"DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz", { } },
        {"DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz", { &H312248C7L, &HD15E298FL } },
        {"DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", { } },
        {"DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz", { &HCE047B54L, &H00076C8EL, &HC3D34A47L, &HC67A7CAFL, &H6E23AEB3L, &HFF32D77EL, &HE93026BAL, &H750935BCL, &HFE7E9DE1L, &HBD2802CEL, &HEB088B56L, &H00A0E33BL, &HC3568907L, &H9BE751FDL, &HA24DAFB1L, &H1DA2326EL } },
        {"DataFiles/pollen 2011-05-19 16u19.cyz", 
            {  &HDFC948EEL, &HDE76227AL, &H5C6CD0B4L, &H8A33542AL, &H4387C2ECL, &H4FE24B93L, &H02BEF3C3L, &H7BCFF041L, &HD7C28D6AL, &H0B022434L, &HCD0FECA4L, &HDE521095L, &H0666FCAFL, &H4C343FA3L, &H57F2EB4DL, &H8804161BL,
               &H7821B9D5L, &H284C84ECL, &HFC7FDBD0L, &H99D478A1L, &HBD8BF09EL, &H075095C4L, &H68351690L, &H64E9C9FBL, &H3218A10DL, &HD2C29566L, &H955EA294L, &H494D0FEDL, &H3FB3F1F5L, &H4C09C080L, &H3E454487L, &H6AFDB005L,
               &H33B613E6L, &H91D29DC3L, &HC76D738AL, &HE4888B0AL, &H6D3BB742L, &H138E2E7CL, &HFB5D2B0AL, &HAE439CFAL, &H667EBDB3L, &HF216F67BL, &H42756F0FL, &H0C0AB599L, &H0091986AL, &HDC4F0AC8L, &H0EC08D4AL, &H91101328L,
               &HA75B7EEEL, &H0675B3EEL, &HA0C53FCFL, &H189D9956L, &H390DFE3DL, &H476C45FEL, &HE063950FL, &HB0C02C98L, &HEB90F5C5L, &H085FD380L, &HB8816940L, &H233C7A67L, &H7C90E859L, &HA7B2FED4L, &HE31A77D4L, &H0E1EE211L,
               &HC28F9B86L, &H0E45C7C4L, &H8EF7AD11L, &HCF03C623L, &HFA524EF2L, &H99D5877BL, &H4FD77931L, &HD214B073L, &HB725E7F7L, &H9D9E6A82L, &H230D1E1FL, &HCF1A8060L, &HDDDE3EBBL, &H5B311362L, &H2B9D8C41L, &H676A524BL,
               &HFA59A4BAL, &HC5DAC76DL, &H5ABF2712L, &HD6348B2DL, &HF5AC3983L, &HBC8F5C9CL, &H0B7F8D8BL, &H94A7CEEEL, &HA328CFB8L, &HFDD64F7AL, &H7C732B42L, &H2CBB4DE2L, &H8C9159EBL, &H35EA5473L, &HDEB9A44EL, &HD75C3E8FL,
               &HC1578725L, &HD75325F0L, &H17185F35L, &HF058B735L, &H5792472FL, &H094E8483L, &H15D9D24EL, &H5ABE13D6L, &H1AA43BCEL, &H2F426930L, &H934424BDL, &HA592B993L, &HB8D3DB13L, &H44A00DD8L, &H921574B7L, &H388BE4C7L,
               &H4E1DBBCBL, &HDC5A6E00L, &H7F06A833L, &H3692728EL, &H8139CA77L, &H1EC4FB49L, &H471DD631L, &HE58998D5L, &HC57EAD81L, &H0C3E80C7L, &H6089FECBL, &HD0E7C990L, &H705AE2B0L, &HE32E4E5DL, &HC3F27473L, &H4338E701L,
               &H012D0BA5L, &H8DC8DA33L, &H822F609AL, &H125C8513L, &HEE46DB39L, &HB48364FBL, &H1F69B487L, &HE24D60F5L, &H8C6951CAL, &H0D5EC14CL, &H3D85F4F0L, &H3219B392L, &HA115BC23L, &H5D406DDCL, &HEE55F865L, &H0135A99AL,
               &H7F77E8B3L, &H2010C1F2L, &H25370499L, &HFAC32084L, &HAB0176E7L, &HEB57C8E7L, &HCBE8D6C1L, &H4C128036L, &H222E52BFL, &HED9777B1L, &HAC73D29EL, &H0965D571L, &H6853A70DL, &HDDF7A912L, &H6B76D35DL, &H17E58004L,
               &H61F66E99L, &H53492762L, &H500AF536L, &HCE588C96L, &H11C5C417L, &H3931F21AL, &HAEE8693DL, &H83610CA3L, &H1A6216F9L, &HC40E588CL, &H526A092EL, &HA687657EL, &H3086F10AL, &HD0793DBFL, &HCCF51191L, &H63BA2950L,
               &HE6F2BCDDL, &H85815442L, &H052F8039L, &HA13D97C4L, &H44EBD90BL, &HB366B560L, &HC4EF6555L, &HE3E3EA85L, &H9FE24937L, &HF43D2B12L, &H78B66D19L, &H2892FEC3L, &H7EA6D77FL, &H3573A850L, &HA2D0EA67L, &H5B94440AL,
               &HE14F3987L, &HCE36A19EL, &H9ACABF3BL, &H3700A604L, &H8CD04124L, &H7BDF9A38L, &H9D35206FL, &HC65A4B56L, &H0CA14A4DL, &HD39DA04EL, &H8A52A469L, &H2DD95234L, &HBA458BA2L, &HBAEF71D1L
               } },
        {"DataFiles/1p6umbeads 2011-05-19 13u06.cyz", 
            { &H53673CCBL, &HE535D7DDL, &H135104C3L, &H4F50C0A9L, &H98AE6068L, &H53B4185EL, &HEF595209L, &H97FAB4D3L, &H2F36D66EL, &H7731EF54L, &H6421E4F3L, &H282CB69EL, &H3D14932CL, &HF0436B3EL, &HE72AB351L, &H5F3DA177L,
              &HBB4B615BL, &HAE529297L, &HC6C1B1FEL, &H3CA943FDL, &H101CBD81L, &H46FC8CFDL, &HD4BB32EDL, &H8202BE3CL, &H1D28968CL, &H97FFB1A2L, &H7FE4AA15L, &H6CF3E23EL, &HEF600832L, &H67C73505L, &HEE8D2E40L, &H665DE208L,
              &H0812D57AL, &H62F2AAECL, &H529B15C2L, &H333DF244L, &H45A91BD4L, &HD7259954L, &H42F96B3AL, &H8737A45FL, &H3F25E6A1L, &H2D6825D3L, &H28E77EAAL, &H83CE5193L, &HCB352099L, &H7063A501L, &HFFB21294L, &HC706102AL,
              &HA3D980DEL, &H02C4E146L, &H9552A0FFL, &HA58B5460L, &H229E7A99L, &HAD030F34L, &H57803257L, &H9B1E7162L, &HFA396E61L, &HBD798457L, &H3C3E3ACCL, &H63FB630DL, &H00160573L, &HCB03B591L, &HE3A0D809L, &H3033512AL,
              &H471DEFC4L, &H019EBE26L, &HA6A87674L, &H637A2CF5L, &H2BBA3D16L, &H6A8D4577L, &HC94F1A26L, &H57C6266DL, &HCBFFDB73L, &HC20BE35FL, &H36FDEE93L, &H536C8F23L, &HA9B0D5A3L, &HBCC7F137L, &H0D50BE57L, &H86EE0FB5L,
              &H9CC59E35L, &H1E1B8C6BL, &H27E78DF9L, &HAE8061A3L, &H8648DB2BL, &H8B8C40DFL, &H4DD18971L, &H93D695D6L, &HB0EFA667L, &HA7A3C06AL, &HDCF093AEL, &H9C227843L, &HD65A4462L, &HEB528AFDL, &H18804E65L, &H11CF0058L,
              &HE43692C4L, &H571EDFF1L, &HDA687E46L, &HD9AEB5EFL 
             } }
       }

            ' Original checksums
            '{"DataFiles/pollen 2011-05-19 16u19.cyz", 
            '{ &H32A63D9BL, &HA0EA6D78L, &H7B4E9880L, &H5DD073C0L, &HD921E3DEL, &H6D93AE4AL, &HD8273CC6L, &HC5C757D6L, &HB99392ADL, &HCF937A8FL,
            '  &H25E07397L, &HB3453684L, &HD6B9B254L, &H96B7F82CL, &HF8382574L, &HD9A4B11BL, &H7A47DCC5L, &H95C4C72EL, &H8A791C3BL, &H5F2E18FDL,
            '  &H36EF1DF5L, &H632E537DL, &HCE505FEDL, &H9737829CL, &H49E4812BL, &H5D47544DL, &HC0CB4B7EL, &H2ACBB851L, &H534EB3CCL, &H87C06714L,
            '  &H469ECBE2L, &HF44AC393L, &H7CAB42B5L, &HB0CE5B3AL, &H442EF763L, &H2ACEE976L, &H8512C42DL, &H4E6E3B40L, &H70317C66L, &H775CDCA5L,
            '  &HA434A2B8L, &H301A016CL, &H5041F9EBL, &H1A837216L, &HFA7121E9L, &H6F43DD30L, &H291B6058L, &H966DA435L, &H7FE2BB18L, &H8D4113BAL,
            '  &H54FC7E74L, &H7E5F222BL, &HF5B07703L, &HEA8DA2C8L, &HACC6BB3EL, &H8AF5BF7BL, &H4FE4C099L, &H4A084BF0L, &H20B82BBEL, &H3E21F3D9L,
            '  &HA1F131ABL, &H7B5AD28BL, &H7F30A656L, &HE2D0D207L, &HBED043B7L, &HF7C17481L, &H4E4801C4L, &H983C1FA9L, &H8137AB34L, &H4F5A42D2L,
            '  &H1AD4E047L, &H17A357F6L, &H4329E23BL, &H3D330670L, &HCAAE1FD5L, &H05BDE632L, &HEAB97FCAL, &HD61691AFL, &H410050BDL, &H7AD3532CL,
            '  &H4D25F016L, &H7BA97AC6L, &HC8B2D4B5L, &HE04429EAL, &H6FABFDBDL, &H0FB59917L, &H979A4143L, &HBB736B29L, &HE4E7A6E9L, &H7AF7BB8BL,
            '  &HED07BA48L, &H674F9339L, &H2C3225D7L, &H1193FE4FL, &HBB8A951BL, &HC46A8259L, &HCF0EFC93L, &H875382F9L, &H546666F0L, &H97ECF5F1L,
            '  &H4BD36440L, &H15930BEEL, &H7E0A032EL, &HEFE11721L, &HC34F4C27L, &H23A3F47BL, &H9FDB340CL, &H8FE56878L, &HB82B6CB4L, &H3993E277L,
            '  &HB913D8ACL, &HE358D388L, &H03D152F9L, &H56E1D1B8L, &HF08ABE37L, &HF617B649L, &H202C6459L, &HDA3E9813L, &H99B4E8E0L, &H7D780F98L,
            '  &HF061E6D2L, &H1458C09AL, &HEFD55899L, &H07AF9DC4L, &H894B8F62L, &HFBC2865CL, &H24DEBBE8L, &HF12D96D6L, &H4C22D85EL, &HD13BC7D1L,
            '  &H52F882D7L, &H8E1323F3L, &H5C04D59FL, &H515E07C0L, &H08C962F1L, &H99BABE15L, &H5A664A44L, &H3575083CL, &HAFE72A9FL, &H4A93B505L,
            '  &H315AAB08L, &H43F9BEA0L, &H1770D619L, &H500E16F5L, &HA3DF2541L, &HA32DC0A1L, &H2B3AE92EL, &HD7C176C6L, &H9707B211L, &H80EBF355L,
            '  &HB8EECB59L, &HE625E1A4L, &H61E19099L, &HD487F484L, &H9AAA7BB8L, &H86042E03L, &H4F015F57L, &HD0B2AEF9L, &H2811E0E7L, &HD614AB50L,
            '  &H77C6128AL, &HA1A09542L, &HFFF6040BL, &H636B2EE7L, &H216718C6L, &H082CCA14L, &H5B32DF57L, &H6984185CL, &HD18CA3F9L, &H3BFDBB4FL,
            '  &H05CB3D64L, &H2890F49FL, &H29B382D9L, &HA00BCC30L, &H44DFBC1DL, &H505F6168L, &H90B33438L, &H0A9A74F1L, &HA8B211A1L, &HF338B772L,
            '  &H55C03D92L, &H92F593CEL, &HBF48A229L, &H622EC8D0L, &HF09A69A3L, &H41C07249L, &H711E597DL, &HC61047D0L, &HA0AD32A6L, &H17FAFAA9L,
            '  &H21071125L, &H75515CD2L, &H819A3E51L, &H048E0108L, &H367BDE63L, &H29E78AC8L, &H36B34A3DL, &HB745FA83L, &H17C94AE3L, &H1B13DE64L,
            '  &H5C2288AFL, &HB7ADB2C9L, &HE2BC6F07L, &HD4C0874BL, &H463BCF5FL, &HA0B8D834L } },
        '{"DataFiles/1p6umbeads 2011-05-19 13u06.cyz", 
        '    { &HB34C2135L, &H80F3CF86L, &H47986EECL, &HB6A60D1FL, &HA5E993D9L, &H413542ABL, &HFDAE4711L, &HA5FA4045L, &H3466F93DL, &HA7CF17ABL,
        '      &HCEF8DD8FL, &HB83DB67AL, &H2F939503L, &HCEA47962L, &H6879D550L, &H4CAEC31DL, &H0D808EF1L, &HD01C2875L, &H3D830CF4L, &HD4FC0BEDL,
        '      &H34829DB8L, &H59835BCBL, &HFBEE2C05L, &HD7333A0CL, &HACDC46C6L, &H3692C93EL, &HF6425142L, &H492CF202L, &H8499585DL, &H51A32F83L,
        '      &HB6AF372FL, &H8C6985BFL, &HFFB03FE5L, &H9A0057B7L, &H9EADF975L, &H0107B703L, &H2070CAE2L, &H9DBC07F9L, &HBF61E67EL, &H6CC0EF2AL,
        '      &H03B29504L, &H859F94B5L, &H6286C8FBL, &HA0DD38E3L, &H41D90CAFL, &H9BB4B399L, &H80F97067L, &HC3E58CF0L, &HB3EFBA62L, &HAB593EEBL,
        '      &HBE5FAB4EL, &H6E488C7EL, &H3E827DBDL, &H4E72DDC0L, &H00FD6048L, &HF865AC11L, &HAA27030FL, &H36F80074L, &H5AA2883DL, &H2FD5A6DEL,
        '      &H5A7B8B2BL, &HFF8F2F32L, &H6B2C8135L, &H061A192EL, &H91F2B2C7L, &H94FC45A0L, &H8A00382AL, &HAD8C6925L, &H91BF9617L, &H6C0AF6B8L,
        '      &H2AFFEF76L, &H94514648L, &HD8879A5CL, &H7517EE06L, &HF0DDEF9FL, &H474A7A14L, &H7654F33EL, &HCAD99FC4L, &H53308A4FL, &HAED1C614L,
        '      &H5CC8CA28L, &HA1F9DE08L, &H46D8944EL, &H4E28727BL, &H332C22B2L, &H16E34DAEL, &HD780FD7AL, &HD5EA6EDEL, &H5CB0C547L, &H62F1BDACL,
        '      &H93EEDBE4L, &H129EF4B7L, &HA8F44F07L, &H954F76F6L, &H8B8541E6L, &HF8309B7CL, &HDCB26A84L, &HC61BC50EL, &H69001151L, &H17D1C068L
        '     } }
        ' {"DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz", { &H335AAF69L, &H04C88D9DL } },



    ''' <summary>
    ''' Test method, that loads image files, and compares them to known CRCs, I need to do this because I am changing serialization
    ''' of the images slightly for .Net 7. It does not handle storing the MemoryStreams, so we need to override that part to
    ''' create compatible data files, and to be able to handle old data files.
    ''' Since I do not know how to pass variable sized arrays as parameter, I create a dictionary above that will hold the CRCs.
    ''' If no dictionary is present, I will simply dump out the CRCs if there are any.
    ''' </summary>
    <DataTestMethod>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz")>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    Public Sub TestImageCrcs(filename As String)
        Dim dfw = New DataFileWrapper(filename)
        If Not _imageCrcs.ContainsKey(filename)  Then ' No Data, Just dump Crcs
            Dim imgParts = dfw.SplittedParticlesWithImages
            For i = 0 To imgParts.Length-1
                Dim ip = imgParts(i)
                Dim imgStr = ip.ImageHandling.ImageStream
                imgStr.Position = 0 ' Reset reading to the beginning

                Dim crc As UInteger = Crc32Algorithm.Compute(imgStr.ToArray(),0,imgStr.Length)
                Dim lCrc As Long = CLng(crc)
                Debug.WriteLine("&H{0:X8}L,", lCrc)
            Next
            Assert.Fail("Add Crc Information for this file!")
        Else
            Dim expectedCrcs= _imageCrcs(filename)
            Dim crcs As List(Of Long) = New List(Of Long)()
            Dim imgParts = dfw.SplittedParticlesWithImages
            For i = 0 To imgParts.Length-1
                Dim ip = imgParts(i)
                Dim imgStr = ip.ImageHandling.ImageStream
                imgStr.Position = 0 ' Reset reading to the beginning
                Dim crc As UInteger = Crc32Algorithm.Compute(imgStr.ToArray(),0,imgStr.Length)
                Dim lCrc As Long = CLng(crc)
                crcs.Add(lCrc)

                ' Debug.WriteLine("&H{0:X8}L,", lCrc)
            Next
            Dim actCrcs As Long() = crcs.ToArray()
            CollectionAssert.AreEqual(expectedCrcs, actCrcs)
        End If
        
    End Sub

    ''' <summary>
    ''' Test accessing the database properties for all the files. Sometimes when adding new properties we 
    ''' mess up handling old data files that do not have them. This should hopefully pick that up.
    ''' We will not know if we get a sane default, but we at least know if it crashes or not.
    ''' 
    ''' The code for this is basically copied from CytoUtils.CyzDb.GetAllFileColumns. Except we do not store the
    ''' data in columns, we just access the properties.
    ''' 
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <remarks>
    ''' Files below are not included in this test because they have other problems.
    '''     DataFiles/Measurement 2012-06-20 14u01.cyz ==> Sectioned datafiles are not supported
    '''     DataFiles/nano_cend16_20 2020-10-06 05u00.cyz ==> Concentration mismatch
    '''     DataFiles/nano_cend16_20 2020-10-07 04u00.cyz ==> Concentration mismatch
    ''' </remarks>

    <DataTestMethod>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz")>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz")>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz")>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz")>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz")>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz")>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz")>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz")>
    Public Sub TestDbProperties(filename As String)
        Dim dfw = New DataFileWrapper(filename)

        Dim pdc = System.ComponentModel.TypeDescriptor.GetProperties(GetType(DataFileWrapper))
        For Each prop As System.ComponentModel.PropertyDescriptor In pdc
            If prop.IsBrowsable Then
                Dim p = prop.GetValue(dfw)
            End If
        Next

        Dim pdc1 As System.ComponentModel.PropertyDescriptorCollection = System.ComponentModel.TypeDescriptor.GetProperties(GetType(MeasurementInfo))
        For Each prop As System.ComponentModel.PropertyDescriptor In pdc1
            If prop.IsBrowsable Then
                Dim p = prop.GetValue(dfw.MeasurementInfo)
            End If
        Next

        Dim pdc2 = System.ComponentModel.TypeDescriptor.GetProperties(GetType(CytoSense.MeasurementSettings.Measurement))
        For Each prop As System.ComponentModel.PropertyDescriptor In pdc2
            If prop.IsBrowsable Then
                Dim p = prop.GetValue(dfw.MeasurementSettings)
            End If
        Next

        Dim pdc3 = System.ComponentModel.TypeDescriptor.GetProperties(GetType(CytoSense.CytoSettings.CytoSenseSetting))
        For Each prop As System.ComponentModel.PropertyDescriptor In pdc3
            If prop.IsBrowsable Then
                Dim p = prop.GetValue(dfw.CytoSettings)
            End If
        Next
    End Sub

    Private Sub TouchAllBrowsableProperties( pdc As System.ComponentModel.PropertyDescriptorCollection, o As Object )
        For Each prop As System.ComponentModel.PropertyDescriptor In pdc
            If prop.IsBrowsable Then
                Dim p = prop.GetValue(o)
            End If
        Next
    End Sub


End Class
