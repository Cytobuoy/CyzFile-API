Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.Linq.Expressions
Imports CytoSense.Data
Imports CytoSense.Data.Data
Imports Force.Crc32
Imports log4net

''' <summary>
''' Test functions of the measurment log, added when changing the way it is used because of a new SubDeep protocol.
''' </summary>
''' <remarks></remarks>
<TestClass()> Public Class TestMeasurementLog

    ' We create a series of tests that access the different measurement start/end/duration functions.
    ' Before we start changing the measurement log functions.  These test functions do not only call the actual measurement log
    ' functions, as you would expect, but we also call some higher level functions that are built on top of that.  These should
    ' be handled in there own test code, but unfortunately we do not have that yet.  And because of the close integration with the
    ' functions that we are changing, I add then here.


    ' First test the different methods to access the measurement start time.


    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              2020, 10,  6,  5,  0, 30, 475,  19,  5)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              2020, 10,  7,  4,  0, 29, 975, 814,  1)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              2013,  4,  8, 14, 56, 32, 366, 758,  4)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      2019,  8, 20, 15, 53,  5, 171, 359,  4)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                 2021, 10,  4, 15,  1, 21, 182,  15,  2)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            2017,  2,  8, 13, 50, 59, 956, 653,  8)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      2017,  2,  8, 13, 50, 59, 956, 653,  8)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  2019, 12, 20, 11, 38,  9, 685, 181,  7)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                2019,  1, 22,  8, 52, 32, 787, 773,  1)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          2017,  4, 20, 12,  0, 14,  25, 430,  3)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   2012, 10, 10, 12,  3, 22, 133, 599,  1)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       2017,  4, 20, 12,  0, 14,  25, 430,  3)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                2023,  7, 18, 14, 46, 39, 811, 509,  0)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                   2014, 09, 30, 15, 13, 33, 929,  64,  3)>
    Public Sub Test_MeasurementLog_GetAcquireStart_DataFiles( filename As String, year As Integer, month As Integer, day As Integer, hour As Integer, minute As Integer, seconds As Integer, milliseconds As Integer, microsecond As Integer, diffTick As Integer)

        Dim expected = New DateTime(year, month, day, hour, minute, seconds, milliseconds, microsecond).AddTicks(diffTick)
        
        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.log.getAcquireStart()

        Dim diff = actual-expected


        Assert.AreEqual(expected, actual, String.Format("Actual: {0}-{1}-{2} {3}:{4}:{5} {6} {7} {8}", actual.Year, actual.Month, actual.Day, actual.Hour, actual.Minute, actual.Second, actual.Millisecond, actual.Microsecond, diff.Ticks))


    End Sub



    ' These files do not have a measurement log yet.
    <DataTestMethod()>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    Public Sub Test_MeasurementLog_GetAcquireStart_DataFiles_NOLOG( filename As String )

        Dim dfw = New DataFileWrapper(filename)


        Try
            Dim actual = dfw.MeasurementInfo.log.getAcquireStart()
            Assert.Fail("Statement above should throw an exception.")
        Catch ex As Exception
            If TypeOf ex IsNot System.NullReferenceException  Then
                    throw new AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        "System.NullReferenceException",
                        ex.GetType().FullName,
                        ex.Message))
            End If
        End Try

    End Sub



    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              0)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              0)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              0)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      0)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                 0)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            0)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      0)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  0)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                0)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          0)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   0)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       0)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                0)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                   0)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                 0)>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                           0)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                           0)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                  0)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                                 0)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                   0)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                      0)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                             0)>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                       0)>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                               0)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                    0)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                               0)>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                      0)>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",    0)>
    Public Sub Test_Measurement_startAcquireTime_DataFiles( filename As String, expected As Integer)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.startAcquireTime

        Assert.AreEqual(expected, actual)

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              2020, 10,  6,  5,  0, 30, 475,  19,  5)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              2020, 10,  7,  4,  0, 29, 975, 814,  1)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              2013,  4,  8, 14, 56, 32, 366, 758,  4)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      2019,  8, 20, 15, 53,  5, 171, 359,  4)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                 2021, 10,  4, 15,  1, 21, 182,  15,  2)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            2017,  2,  8, 13, 50, 59, 956, 653,  8)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      2017,  2,  8, 13, 50, 59, 956, 653,  8)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  2019, 12, 20, 11, 38,  9, 685, 181,  7)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                2019,  1, 22,  8, 52, 32, 787, 773,  1)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          2017,  4, 20, 12,  0, 14,  25, 430,  3)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   2012, 10, 10, 12,  3, 22, 133, 599,  1)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       2017,  4, 20, 12,  0, 14,  25, 430,  3)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                2023,  7, 18, 14, 46, 39, 811, 509,  0)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                   2014,  9, 30, 15, 13, 33, 929,  64,  3)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                 2011,  6, 11,  9, 28, 55, 421, 875,  0)> ' Remainder is all before measurement log was added.
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                           2011, 12, 14, 13, 50, 27, 840, 103,  9)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                           2011, 12, 14, 13, 51, 58, 621, 353,  9)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                  2011,  5, 19, 13,  6,  7, 265, 652, -270)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                                 2011,  2, 21, 10, 51, 12, 796, 875,  0)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                   2012,  2, 16, 14, 35, 48, 187, 500,  0)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                      2011,  1,  7, 14, 59, 48, 656, 250,  0)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                             2010, 11, 22, 11, 33,  9, 109, 375,  0)>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                       2010, 12,  8, 15,  0, 15, 984, 375,  0)>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                               2010,  6, 16, 13, 10, 58,  31, 250,  0)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                    2011,  9, 26, 17, 10, 54, 548, 980,  2)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                               2011,  9, 26, 17, 10, 54, 548, 980,  2)>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                      2011,  5, 19, 16, 19, 54, 804, 930,  8)>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",    2010, 12,  3, 15, 17,  1, 328, 125,  0)>
    Public Sub Test_Measurement_ActualAcquireStart_DataFiles( filename As String, year As Integer, month As Integer, day As Integer, hour As Integer, minute As Integer, seconds As Integer, milliseconds As Integer, microsecond As Integer, diffTick As Integer)

        Dim expected = New DateTime(year, month, day, hour, minute, seconds, milliseconds, microsecond).AddTicks(diffTick)
        
        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.ActualAcquireStart

        Dim diff = actual-expected


        Assert.AreEqual(expected, actual, String.Format("Actual: {0}-{1}-{2} {3}:{4}:{5} {6} {7} {8}", actual.Year, actual.Month, actual.Day, actual.Hour, actual.Minute, actual.Second, actual.Millisecond, actual.Microsecond, diff.Ticks))

    End Sub


    ' Then WithEvents test the different measurements to access measurement durations.

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              600)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              600)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              155)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      180)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                  23)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                             60)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                       60)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  212)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                120)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          360)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   600)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       360)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                 20)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                    60)>
    Public Sub Test_MeasurementLog_getAcquireDuration( filename As String, expected As Integer)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.log.getAqcuireDuration()

        Assert.AreEqual(expected, actual)

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    Public Sub Test_MeasurementLog_getAcquireDuration_NOLOG( filename As String)

        Dim dfw = New DataFileWrapper(filename)

        Try
            Dim actual = dfw.MeasurementInfo.log.getAqcuireDuration()
            Assert.Fail("Statement above should throw an exception.")
        Catch ex As Exception
            If TypeOf ex IsNot System.NullReferenceException  Then
                    throw new AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        "System.NullReferenceException",
                        ex.GetType().FullName,
                        ex.Message))
            End If
        End Try

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              600.0F)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              600.0F)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              155.0F)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      180.0F)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                  23.0F)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                             60.0F)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                       60.0F)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  212.0F)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                120.0F)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          360.0F)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   600.0F)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       360.0F)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                 20.0F)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                    60.0F)>
    Public Sub Test_Measurement_ActualMeasureTime( filename As String, expected As Single)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.ActualMeasureTime

        Assert.AreEqual(expected, actual, 0.001)

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                              159.0F)>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                         59.0F)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                         59.0F)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                               141.0F)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                               45.0F)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                 28.0F)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                    47.0F)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                           70.0F)>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                    179.0F)>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                             27.0F)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                  40.0F)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                             40.0F)>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                    78.0F)>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz", 179.0F)>
    Public Sub Test_Measurement_ActualMeasureTime_NOLOG( filename As String, expected As Single)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.ActualMeasureTime

        Assert.AreEqual(expected, actual, 0.001)
    End Sub

    ' And finally the endtime calculations.

    ' log.getAcuireEnd

    ' Measurement.endAcquireTime.

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              2020, 10,  6,  5, 10, 30, 654, 873, 7)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              2020, 10,  7,  4, 10, 30, 608,  69, 1)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              2013,  4,  8, 14, 59,  7, 494, 631, 2)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      2019,  8, 20, 15, 56,  5, 323, 916, 0)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                 2021, 10,  4, 15,  1, 45, 180, 190, 8)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            2017,  2,  8, 13, 52,  0, 157,  97, 0)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      2017,  2,  8, 13, 52,  0, 157,  97, 0)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  2019, 12, 20, 11, 41, 42,   1, 554, 6)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                2019,  1, 22,  8, 54, 32, 970, 384, 1)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          2017,  4, 20, 12,  6, 14, 885, 264, 1)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   2012, 10, 10, 12, 13, 22, 283, 971, 0)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       2017,  4, 20, 12,  6, 14, 885, 264, 1)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                2023,  7, 18, 14, 46, 59, 976, 800, 3)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                   2014,  9, 30, 15, 14, 34, 165, 509, 7)>
    Public Sub Test_MeasurementLog_getAcquireEnd( filename As String, year As Integer, month As Integer, day As Integer, hour As Integer, minute As Integer, seconds As Integer, milliseconds As Integer, microsecond As Integer, diffTick As Integer)


        Dim expected = New DateTime(year, month, day, hour, minute, seconds, milliseconds, microsecond).AddTicks(diffTick)
        
        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.log.getAcquireEnd()

        Dim diff = actual-expected


        Assert.AreEqual(expected, actual, String.Format("Actual: {0}-{1}-{2} {3}:{4}:{5} {6} {7} {8}", actual.Year, actual.Month, actual.Day, actual.Hour, actual.Minute, actual.Second, actual.Millisecond, actual.Microsecond, diff.Ticks))


    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz")>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz")>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz")>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz")>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz")>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz")>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz")>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz")>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz")>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz")>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz")>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz")>
    Public Sub Test_MeasurementLog_getAcquireEnd_NOLOG( filename As String)

        Dim dfw = New DataFileWrapper(filename)

        Try
            Dim actual = dfw.MeasurementInfo.log.getAcquireEnd()
            Assert.Fail("Statement above should throw an exception.")
        Catch ex As Exception
            If TypeOf ex IsNot System.NullReferenceException  Then
                    throw new AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        "System.NullReferenceException",
                        ex.GetType().FullName,
                        ex.Message))
            End If
        End Try

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                              600)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                              600)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              155)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      180)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                  23)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                             60)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                       60)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  212)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                120)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          360)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   600)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       360)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                 20)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                    60)>
    Public Sub Test_Measurement_endAcquireTime( filename As String, expected As Integer)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.endAcquireTime

        Assert.AreEqual(expected, actual)

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                              159)>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                         59)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                         59)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                               141)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                               45)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                 28)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                    47)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                           70)>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                    179)>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                             27)>  ' !!!! Other problem ?!?!
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                  40)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                             40)>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                    78)>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz", 179)>
    Public Sub Test_Measurement_endAcquireTime_NOLOG( filename As String, expected As Integer)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.MeasurementInfo.endAcquireTime

        Assert.AreEqual(expected, actual)
    End Sub


    ' Add some more checks for derived values, just to make sure that I have not missed something. In theory there would be
    ' no need to add this as we have these tests alread. In real life.



    <DataTestMethod()>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                            30.68)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                            30.68)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                               10.85)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                    9.87)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                                  97.65)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                   110.04)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                       43.71)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                              39.20)>
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                       100.24)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",      898.20)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                  70.61)>
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                                15.12)>  
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                             63.00)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                       63.00)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                    399.60)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                               399.60)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                6049.95)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  474.88)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                369.60)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                          734.40)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                  2688.00)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                             5118.00)>
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                             5118.00)>
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                      212.94)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       734.40)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                 20.20)>
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",    751.80)>
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                    63.00)>
    Public Sub Test_PumpedVolume( filename As String, expected As Double)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.pumpedVolume

        Assert.AreEqual(expected, actual, 0.001)

    End Sub


    <DataTestMethod()>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                              9.318)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                              9.624)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                                 0)> ' Analyzed is 0, no downloaded particles.
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                     1.98)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                             Double.NaN)>  ' Huh ?!!?!?
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                    88.789)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                       12.449)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                         Double.NaN)> ' Huh ?!!?!?
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                        3.377)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",     897.653)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",             Double.NaN)> ' Huh ?!!?!? 
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                                0.467)> 
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            23.888)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      23.888)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                   314.396)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                              314.396)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                210.976)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  11.223)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",               305.354)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         469.416)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                 1139.273)> 
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                     174.171)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      469.416)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                0)> ' Huh, 0 is actually correct, no downloaded particles.
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",    Double.PositiveInfinity)> ' Huh, was actually infinite, Recorded concentration is 0, which leads to infinite analyzed volume.  Bad datafile, correct code.
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                    6.354)>
    Public Sub Test_AnalyzedVolume( filename As String, expected As Double)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.analyzedVolume

        If Double.IsNaN(expected) Then ' Cannot compare NaN so we have to do it like this.
            Assert.IsTrue(Double.IsNaN(actual))
        Else
            Assert.AreEqual(expected, actual, 0.001)
        End If
    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz")> ' Mismatch error exception.
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz")> ' Mismatch error exception.
    Public Sub Test_AnalyzedVolume_ConcentrationMismatch( filename As String)

        Dim dfw = New DataFileWrapper(filename)

        Try
            Dim actual = dfw.analyzedVolume
            Assert.Fail("Statement above should throw an exception.")
        Catch ex As Exception
            If TypeOf ex IsNot CytoSense.ConcentrationMisMatchException  Then
                    throw new AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        "CytoSense.ConcentrationMisMatchException",
                        ex.GetType().FullName,
                        ex.Message))
            End If
        End Try




    End Sub




    <DataTestMethod()>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                          3143.412)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                          2726.299)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              675.119)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                 15275.359)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                                Double.NaN)>
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  1359.751)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                      485.003)>
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                            Double.NaN)> 
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                      9882.527)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",       51.170)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                   0.0)> 
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                              7682.153)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           1542.680)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     22.397)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                   22.271)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                              22.271)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                144.088)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz", 461.387)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                20.026)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         173.758)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                    3.104)> 
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                      17.833)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",       34.911)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                 0.050)> 
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",       0.0)>  
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                  5232.862)>
    Public Sub Test_Concentration( filename As String, expected As Double)

        Dim dfw = New DataFileWrapper(filename)

        Dim actual = dfw.Concentration

        If Double.IsNaN(expected) Then ' Cannot compare NaN so we have to do it like this.
            Assert.IsTrue(Double.IsNaN(actual))
        Else
            Assert.AreEqual(expected, actual, 0.001)
        End If
    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz")> ' Mismatch error exception.
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz")> ' Mismatch error exception.
    Public Sub Test_Concentration_ConcentrationMismatch( filename As String)

        Dim dfw = New DataFileWrapper(filename)

        Try
            Dim actual = dfw.Concentration
            Assert.Fail("Statement above should throw an exception.")
        Catch ex As Exception
            If TypeOf ex IsNot CytoSense.ConcentrationMisMatchException  Then
                    throw new AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        "CytoSense.ConcentrationMisMatchException",
                        ex.GetType().FullName,
                        ex.Message))
            End If
        End Try
    End Sub

    ''' <summary>
    ''' We select the preconcentration mode (either for PIC or FTDI) and then do a call to get
    ''' the actual concentration (this is before compensation for smart triggering).
    ''' NOTE: For All instruments after 2017-85, the preconcentration is never used internally,
    ''' and the mode is ignore.  We always return the concentration based on particle counts, NEVER
    ''' the preconcentration, regardless of the mode.
    ''' NOTE: A concentration of -1, means that no preconcentration measurements were done.
    ''' </summary>
    ''' <param name="filename"></param>
    ''' <param name="expected"></param>
    ''' <remarks>
    ''' Maybe we should add some special code for these, to also check the actual preconcentration
    ''' calculations are not messed up.</remarks>

    <DataTestMethod()>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                          2306.754)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                          3430.081)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                               -1.0)> ' No preconcentration measurent
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                17463.617)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                                   0.0)>  ' Preconcentration measurement exists, but is empty. So counts as 0
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                     0.0)>  ' Is actually 0, lots of 0 measurements
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                       -1.0)>  ' No preconcentration
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                               6.664)> 
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                      9892.401)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",       51.170)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                   0.0)> '  Is actually 0
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                                -1.0)> ' No Preconcentration measurement
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                            429.875)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                      429.875)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                     21.551)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                                21.551)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                  -1.0)>  ' No Preconcnetration measurement
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  550.493)>  
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                 17.916)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         1557.981)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   133.998)> 
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                                0.680)> 
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                                0.513)> 
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                       16.578)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      1557.981)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                  0.049)> 
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",      0.0)>   ' Huh, was actually 0, is not actually the preconcentration, but the measurement concentration because of FJ electroncis.
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                  4971.219)>
    Public Sub Test_Preconcentration( filename As String, expected As Double)

        Dim dfw = New DataFileWrapper(filename)

        Dim mode = If(dfw.CytoSettings.hasaPIC, ConcentrationModeEnum.Pre_measurement_PIC, ConcentrationModeEnum.Pre_measurement_FTDI)

        Dim actual = dfw.ActualConcentration(mode)

        Assert.AreEqual(expected, actual, 0.001)

    End Sub

    <DataTestMethod()>
    <DataRow("DataFiles/1 2011-12-14 13u50.cyz",                                          3143.412)>
    <DataRow("DataFiles/1 2011-12-14 13u51.cyz",                                          2726.299)>
    <DataRow("DataFiles/1p6um NF beads 2013-04-08 14u56.cyz",                              675.119)>
    <DataRow("DataFiles/1p6umbeads 2011-05-19 13u06.cyz",                                15275.359)>
    <DataRow("DataFiles/1umFLYbeads 2011-02-21 10u51.cyz",                               Double.NaN)> ' Huh ??
    <DataRow("DataFiles/Algae and beads very fast 2012-02-16 14u35.cyz",                  1359.751)>
    <DataRow("DataFiles/algjes 2011-01-07 14u59.cyz",                                     -1.0)>  ' No centration measurement ?!?!!?
    <DataRow("DataFiles/algjes machteld 2010-11-22 11u33.cyz",                            Double.NaN)> ' Huh ??
    <DataRow("DataFiles/beads 2010-12-08 15u00.cyz",                                      9882.527)>
    <DataRow("DataFiles/beads Measurement - Medium sensitivity 2019-08-20 15h50.cyz",       51.170)>
    <DataRow("DataFiles/beads reservoir measurement 2021-10-04 15h00.cyz",                   0.0)> ' Is actually 0
    <DataRow("DataFiles/beads1.6u.cyz-2010-06-16 13-10.CYZ",                                -1.0)> ' No concentration available ???
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50.cyz",                           1542.680)>
    <DataRow("DataFiles/BeadsCalibration 2017-02-08 13h50_Beads.cyz",                     1542.680)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10.cyz",                                     22.271)>
    <DataRow("DataFiles/BerreIIF 2011-09-26 17u10_orig.cyz",                                22.271)>
    <DataRow("DataFiles/d0_a1_run_3 2011-06-11 09u28.cyz",                                 144.088)>
    <DataRow("DataFiles/dsc sea water  sch haven oud 2019-12-20 11h38_All No Images.cyz",  550.493)>
    <DataRow("DataFiles/FunctionalTest_measurement#1 2019-01-22 08h52.cyz",                 20.026)>
    <DataRow("DataFiles/LW_2017_protocol_1 2017-04-20 12h00.cyz",                         1671.594)>
    <DataRow("DataFiles/Maaswater gevoeliger 256K 2012-10-10 12u03.cyz",                   854.826)> 
    <DataRow("DataFiles/nano_cend16_20 2020-10-06 05u00.cyz",                               19.686)> 
    <DataRow("DataFiles/nano_cend16_20 2020-10-07 04u00.cyz",                               12.400)> 
    <DataRow("DataFiles/pollen 2011-05-19 16u19.cyz",                                       17.833)>
    <DataRow("DataFiles/profiles_LW_2017_protocol_1 2017-04-20 12h00_New Set 2.cyz",      1671.594)>
    <DataRow("DataFiles/ruistest_1 2023-07-18 14h46_2.cyz",                                  0.050)> 
    <DataRow("DataFiles/scenedesmus 230410 gemengd in kraanwater 2010-12-03 15u17.cyz",      0.0)> ' Huh i sactually 0
    <DataRow("DataFiles/Segmented 2014-09-30 15u13.cyz",                                  5232.862)>
    Public Sub Test_MeasurementConcentration( filename As String, expected As Double)

        Dim dfw = New DataFileWrapper(filename)

        Dim mode = If(dfw.CytoSettings.hasaPIC, ConcentrationModeEnum.During_measurement_PIC, ConcentrationModeEnum.During_measurement_FTDI)

        Dim actual = dfw.ActualConcentration(mode)

        If Double.IsNaN(expected) Then ' Cannot compare NaN so we have to do it like this.
            Assert.IsTrue(Double.IsNaN(actual))
        Else
            Assert.AreEqual(expected, actual, 0.001)
        End If
    End Sub

End Class
