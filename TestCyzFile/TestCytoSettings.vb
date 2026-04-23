Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports System
Imports CytoSense.Data
Imports CytoSense.CytoSettings
Imports CytoSense.Serializing

''' <summary>
''' Very very basic testing of a CytoSettings class, the class already exists a long time, and I just created this
''' test class to add some test for a new feature (Humidity sensor related stuff).  All the existing stuff is not
''' touched in these tests, in the future they should be extended. (One can always dream).
''' </summary>
<TestClass()> Public Class TestCytoSettings




    ''' <summary>
    ''' Verify that for a default constructed new settings object, the default settings are present and correct.
    ''' </summary>

    <TestMethod()>
    Public Sub TestDefaultConstruction()

        Dim settings = New CytoSenseSetting()
        Assert.IsNotNull(settings)

        Assert.IsNotNull(settings.System)
        Assert.IsNotNull(settings.System.HumiditySensor)

        Assert.AreEqual(HumiditySensorType_t.None, settings.System.HumiditySensor.Type)
        Assert.AreEqual(5,                         settings.System.HumiditySensor.SubsamplingInterval)

    End Sub




    ''' <summary>
    ''' A simple save and reload, when there is no previous file to overwrite.
    ''' </summary>
    <TestMethod()>
    Public Sub TestSaveLoadHumdityToFile()
        Dim procId = Process.GetCurrentProcess().Id
        Dim testName = String.Format("TestSaveLoadHumidity-{0}.dat", procId)
        Dim tmpPath = Path.GetTempPath()

        Dim testFileName = Path.Combine(tmpPath,testName)
        Dim testTmpFileName = Path.Combine(tmpPath,testName+".tmp")
        Dim testBakFileName = Path.Combine(tmpPath,testName+".backup")

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)


        Dim origSettings = New CytoSenseSetting()
        origSettings.System.HumiditySensor.Type                = HumiditySensorType_t.AHT20
        origSettings.System.HumiditySensor.SubsamplingInterval = 13

        Serializing.SaveToFile(testFileName,origSettings)

        Dim reloadedSettings = CType(Serializing.loadFromFile(testFileName), CytoSenseSetting)

        Assert.AreEqual(HumiditySensorType_t.AHT20, reloadedSettings.System.HumiditySensor.Type )
        Assert.AreEqual(13,                         reloadedSettings.System.HumiditySensor.SubsamplingInterval)


        ' Now store the none and a different subsample value, to see if both variants work correctly.
        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)

        origSettings.System.HumiditySensor.Type                = HumiditySensorType_t.None
        origSettings.System.HumiditySensor.SubsamplingInterval = 17

        Serializing.SaveToFile(testFileName,origSettings)

        Dim reloadedSettings2 = CType(Serializing.loadFromFile(testFileName), CytoSenseSetting)

        Assert.AreEqual(HumiditySensorType_t.None,  reloadedSettings2.System.HumiditySensor.Type )
        Assert.AreEqual(17,                         reloadedSettings2.System.HumiditySensor.SubsamplingInterval)

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)

    End Sub




    ''' <summary>
    ''' Verify that wehn we load an existing datafile without the humidity values, we actually do get correctly
    ''' initialized settings.
    ''' </summary>
    ''' <param name="testfile"></param>
    <DataTestMethod>
    <DataRow("DataFiles\1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles\1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles\1p6um NF beads 2013-04-08 14u56.cyz")>
    <DataRow("DataFiles\1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles\1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles\Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles\algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles\algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles\beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles\beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz")>
    <DataRow("DataFiles\beads reservoir measurement 2021-10-04 15h00.cyz")>
    <DataRow("DataFiles\beads1.6u.cyz-2010-06-16 13-10.cyz")>
    <DataRow("DataFiles\BeadsCalibration 2017-02-08 13h50.cyz")>
    <DataRow("DataFiles\BeadsCalibration 2017-02-08 13h50_Beads.cyz")>
    <DataRow("DataFiles\BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles\BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles\d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles\dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz")>
    <DataRow("DataFiles\FunctionalTest_measurement#1 2019-01-22 08h52.cyz")>
    <DataRow("DataFiles\LW_2017_protocol_1 2017-04-20 12h00.cyz")>
    <DataRow("DataFiles\Maaswater gevoeliger 256K 2012-10-10 12u03.cyz")>
    <DataRow("DataFiles\nano_cend16_20 2020-10-06 05u00.cyz")>
    <DataRow("DataFiles\nano_cend16_20 2020-10-07 04u00.cyz")>
    <DataRow("DataFiles\pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles\profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz")>
    <DataRow("DataFiles\ruistest_1 2023-07-18 14h46_2.cyz")>
    <DataRow("DataFiles\scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    <DataRow("DataFiles\Segmented 2014-09-30 15u13.cyz")>
    <DataRow("DataFiles\ZOO80um_50t60m_diluted5x_8uls_SWST50L60_5min_IIFall_sh high_smrttr SWS SL300 SWS max1000 2025-12-18 13h02.cyz")>
    Public Sub TestHumidtyPropertiesOnStoredSettingsInOlderFiles(testfile As String)
        Dim wrapper As New DataFileWrapper(testFile)
        Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

        Assert.IsNotNull(wrapper.CytoSettings.System,                String.Format("No system settings group found for '{0}'", testfile))
        Assert.IsNotNull(wrapper.CytoSettings.System.HumiditySensor, String.Format("No humidity sensor settings group found for '{0}'", testfile))

        Assert.AreEqual(HumiditySensorType_t.None, wrapper.CytoSettings.System.HumiditySensor.Type)
        Assert.AreEqual(5,                         wrapper.CytoSettings.System.HumiditySensor.SubsamplingInterval)

    End Sub




End Class