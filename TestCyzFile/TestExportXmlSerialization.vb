Option Strict On
Imports CytoSense.MeasurementSettings

''' <summary>
''' Some very simple test for serializing the export settings to XML.  I am about to rewrite the
''' serialization and I want the end result to be compatible. So I am adding a few tests to
''' see if it stays the same.  It is not an in depth test of all possibilities, justsomething
''' quick and dirty.
''' </summary>

<TestClass()> 
Public Class TestExportXmlSerialization


    ''' <summary>
    ''' Just try and load an see check it does not crash.
    ''' </summary>
    <TestMethod()>
    Public Sub BasicLoadTest()
        Using file As New System.IO.FileStream("Misc/TestExportXml.c4e", IO.FileMode.Open)
            Dim settings = ExportSettings.XmlDeserialize(file)
        End Using
    End Sub

  <TestMethod()>
    Public Sub LoadTest()
        Using file As New System.IO.FileStream("Misc/TestExportXml.c4e", IO.FileMode.Open)
            Dim settings = ExportSettings.XmlDeserialize(file)
            Assert.AreEqual( False, settings.Summary)' As Boolean
            Assert.AreEqual( True,settings.RawPulseShapes_csv)' As Boolean
            Assert.AreEqual( False,settings.RawPulseShapes_mat)' As Boolean
            Assert.AreEqual( True, settings.ListMode_csv)' As Boolean
            Assert.AreEqual( False, settings.ListMode_mat)' As Boolean
            Assert.AreEqual( True, settings.ListMode_fcs)' As Boolean
            Assert.AreEqual( False, settings.AdditionalListMode_csv)' As Boolean 'EAWAG parameters
            Assert.AreEqual( True, settings.SensorLogs)' As Boolean
            Assert.AreEqual( True, settings.IIF)' As Boolean
            Assert.AreEqual( True, settings.MeansPerSetPerFile)' As Boolean
            Assert.AreEqual( "means_per_set_per_file.csv",settings.MeansPerSetPerFile_outputfile)' As String
            Assert.AreEqual( True,settings.CYZFile)' As Boolean

            'Properties for generating image overviews in batch export.
            Assert.AreEqual( True,settings.OverViewImageGenerate)' As Boolean
            Assert.AreEqual( True,settings.OverViewImageShowScalebar)' As Boolean
            Assert.AreEqual( True,settings.OverViewImageShowId)' As Boolean
            Assert.AreEqual( Nothing, settings.OverViewImageOutputFolder)' As String
            Assert.AreEqual( Nothing, settings.OverviewImageFilenamePrefix)' As String
            Assert.AreEqual( 16384, settings.OverViewImageMaxWidth)  ' Default values
            Assert.AreEqual( 16384, settings.OverViewImageMaxHeight) ' Default values
            Assert.AreEqual( CytoSense.ImageUtil.JPEG_GUID, settings.OverViewImageExportType)' As Guid = ImageFormat.Jpeg.Guid
            Assert.AreEqual( False, settings.GpsLog)' As Boolean
            Assert.AreEqual( True, settings.ListmodeExport)' As Boolean
            Dim lmChannels = New String(){"FWS L","FWS R","SWS","Fl Yellow","Fl Orange","Fl Red"}
            CollectionAssert.AreEqual( lmChannels, settings.SelectedListmodeChannels)' As String()
            Dim lmParams = New String(){"Length","Total","Maximum","Average","Fill factor","Number of cells"}
            CollectionAssert.AreEqual( lmParams ,settings.SelectedListmodeParameters)'As String()
            Dim expSets = New String(){"Default (all)","New Set","New Set 1","New Set 2"}
            CollectionAssert.AreEqual( expSets, settings.SelectedSets)' As String()
            Assert.AreEqual( "F:\My CytoSense\datafiles\", settings.OutputDirectory)' As String
            Assert.AreEqual( CytoSense.ImageUtil.PNG_GUID, settings.IIFImageFormatType)' As Guid = ImageFormat.Jpeg.Guid
            Assert.AreEqual( True, settings.IIFZipfile)' As Boolean
            Assert.AreEqual( False,settings.IIFCropped)' As Boolean
            Assert.AreEqual( False,settings.IIFCroppedWithScalebar)' As Boolean
            Assert.AreEqual( True, settings.IIFUncropped)' As Boolean
            Assert.AreEqual( False,settings.IIFUncroppedWithScalebar)' As Boolean
            Assert.AreEqual( True, settings.IIFBackgroundImage)' As Boolean

            Dim statChan = New String(){"SWS","Fl Yellow","Fl Orange","Fl Red"}
            CollectionAssert.AreEqual( statChan,settings.SelectedStatisticsChannels)' As String()
            Dim statParam= New String(){"Total","Maximum","Average","Number of cells"}
            CollectionAssert.AreEqual( statParam, settings.SelectedStatisticsParameters)' As String()
            Assert.AreEqual( True, settings.Statistics_csv)' As Boolean
            Assert.AreEqual( False,settings.Statistics_mat)' As Boolean
            Assert.AreEqual( True,settings.Statistics_min)' As Boolean
            Assert.AreEqual( True,settings.Statistics_max)' As Boolean
            Assert.AreEqual( True,settings.Statistics_mean)' As Boolean
            Assert.AreEqual( True,settings.Statistics_SD)' As Boolean
            Assert.AreEqual( True,settings.Statistics_Total)' As Boolean

            'maybe there should be an additional data settings struct. Also for channel mode, ArrivalTimeExportMode, and which kind of images etc
            Assert.AreEqual( ExportSettings.ChannelExportMode.smoothed, settings.Smoothing)' As ChannelExportMode = ChannelExportMode.smoothed
            Assert.AreEqual( ";" , settings.delimiter)' As String = ";"
            Assert.AreEqual( ArrivalTimeExportMode.Relative, settings.TimeExportMode)' As ArrivalTimeExportMode = ArrivalTimeExportMode.Relative
            Assert.AreEqual( "CS-2016-04", settings.SerialNumber)' As String    ' Machine serial number
            Assert.AreEqual( New DateTime(2018,4,24), settings.ConfigurationeDate)' As DateTime  ' Machine hardware configuration date
            Assert.AreEqual( "," ,settings.DecimalSeparator)' As String
            Assert.AreEqual( False, settings.AppendStatisticsFile)' As Boolean ' Append to the statistics file if it allready exists instead of overwriting it.
        End Using
    End Sub

    <TestMethod()>
    Public Sub LoadTest_2()
        Using file As New System.IO.FileStream("Misc/TestExportXml_New.c4e", IO.FileMode.Open)
            Dim settings = ExportSettings.XmlDeserialize(file)
            Assert.AreEqual( False, settings.Summary)' As Boolean
            Assert.AreEqual( True,settings.RawPulseShapes_csv)' As Boolean
            Assert.AreEqual( False,settings.RawPulseShapes_mat)' As Boolean
            Assert.AreEqual( True, settings.ListMode_csv)' As Boolean
            Assert.AreEqual( False, settings.ListMode_mat)' As Boolean
            Assert.AreEqual( True, settings.ListMode_fcs)' As Boolean
            Assert.AreEqual( False, settings.AdditionalListMode_csv)' As Boolean 'EAWAG parameters
            Assert.AreEqual( True, settings.SensorLogs)' As Boolean
            Assert.AreEqual( True, settings.IIF)' As Boolean
            Assert.AreEqual( True, settings.MeansPerSetPerFile)' As Boolean
            Assert.AreEqual( "means_per_set_per_file.csv",settings.MeansPerSetPerFile_outputfile)' As String
            Assert.AreEqual( True,settings.CYZFile)' As Boolean

            'Properties for generating image overviews in batch export.
            Assert.AreEqual( True,settings.OverViewImageGenerate)' As Boolean
            Assert.AreEqual( True,settings.OverViewImageShowScalebar)' As Boolean
            Assert.AreEqual( True,settings.OverViewImageShowId)' As Boolean
            Assert.AreEqual( Nothing, settings.OverViewImageOutputFolder)' As String
            Assert.AreEqual( Nothing, settings.OverviewImageFilenamePrefix)' As String
            Assert.AreEqual( 1920, settings.OverViewImageMaxWidth)  ' Default values
            Assert.AreEqual( 1080, settings.OverViewImageMaxHeight) ' Default values
            Assert.AreEqual( CytoSense.ImageUtil.JPEG_GUID, settings.OverViewImageExportType)' As Guid = ImageFormat.Jpeg.Guid
            Assert.AreEqual( False, settings.GpsLog)' As Boolean
            Assert.AreEqual( True, settings.ListmodeExport)' As Boolean
            Dim lmChannels = New String(){"FWS L","FWS R","SWS","Fl Yellow","Fl Orange","Fl Red"}
            CollectionAssert.AreEqual( lmChannels, settings.SelectedListmodeChannels)' As String()
            Dim lmParams = New String(){"Length","Total","Maximum","Average","Fill factor","Number of cells"}
            CollectionAssert.AreEqual( lmParams ,settings.SelectedListmodeParameters)'As String()
            Dim expSets = New String(){"Default (all)","New Set","New Set 1","New Set 2"}
            CollectionAssert.AreEqual( expSets, settings.SelectedSets)' As String()
            Assert.AreEqual( "F:\My CytoSense\datafiles\", settings.OutputDirectory)' As String
            Assert.AreEqual( CytoSense.ImageUtil.PNG_GUID, settings.IIFImageFormatType)' As Guid = ImageFormat.Jpeg.Guid
            Assert.AreEqual( True, settings.IIFZipfile)' As Boolean
            Assert.AreEqual( False,settings.IIFCropped)' As Boolean
            Assert.AreEqual( False,settings.IIFCroppedWithScalebar)' As Boolean
            Assert.AreEqual( True, settings.IIFUncropped)' As Boolean
            Assert.AreEqual( False,settings.IIFUncroppedWithScalebar)' As Boolean
            Assert.AreEqual( True, settings.IIFBackgroundImage)' As Boolean

            Dim statChan = New String(){"SWS","Fl Yellow","Fl Orange","Fl Red"}
            CollectionAssert.AreEqual( statChan,settings.SelectedStatisticsChannels)' As String()
            Dim statParam= New String(){"Total","Maximum","Average","Number of cells"}
            CollectionAssert.AreEqual( statParam, settings.SelectedStatisticsParameters)' As String()
            Assert.AreEqual( True, settings.Statistics_csv)' As Boolean
            Assert.AreEqual( False,settings.Statistics_mat)' As Boolean
            Assert.AreEqual( True,settings.Statistics_min)' As Boolean
            Assert.AreEqual( True,settings.Statistics_max)' As Boolean
            Assert.AreEqual( True,settings.Statistics_mean)' As Boolean
            Assert.AreEqual( True,settings.Statistics_SD)' As Boolean
            Assert.AreEqual( True,settings.Statistics_Total)' As Boolean

            'maybe there should be an additional data settings struct. Also for channel mode, ArrivalTimeExportMode, and which kind of images etc
            Assert.AreEqual( ExportSettings.ChannelExportMode.smoothed, settings.Smoothing)' As ChannelExportMode = ChannelExportMode.smoothed
            Assert.AreEqual( ";" , settings.delimiter)' As String = ";"
            Assert.AreEqual( ArrivalTimeExportMode.Relative, settings.TimeExportMode)' As ArrivalTimeExportMode = ArrivalTimeExportMode.Relative
            Assert.AreEqual( "CS-2016-04", settings.SerialNumber)' As String    ' Machine serial number
            Assert.AreEqual( New DateTime(2018,4,24), settings.ConfigurationeDate)' As DateTime  ' Machine hardware configuration date
            Assert.AreEqual( "," ,settings.DecimalSeparator)' As String
            Assert.AreEqual( False, settings.AppendStatisticsFile)' As Boolean ' Append to the statistics file if it allready exists instead of overwriting it.
        End Using
    End Sub

    ''' <summary>
    ''' Simple test storing a file, and then loading it again.
    ''' As a sample we use the demo file that we load first. Store it
    ''' to a memory stream, then load it again. (We tested the laoding, so this should
    ''' result in the same in memory file if everything is OK.
    ''' </summary>
    <TestMethod()>
    Public Sub StoreLoadTest()
        Dim initialSettings As ExportSettings
        Using file As New System.IO.FileStream("Misc/TestExportXml.c4e", IO.FileMode.Open)
            initialSettings = ExportSettings.XmlDeserialize(file)
        End Using

        Dim settingsStream As New IO.MemoryStream()
        ExportSettings.XmlSerialize(settingsStream, initialSettings)
        settingsStream.Seek(0, IO.SeekOrigin.Begin)

        Dim newSettings = ExportSettings.XmlDeserialize(settingsStream)
        Assert.AreEqual( initialSettings.Summary, newSettings.Summary)' As Boolean
        Assert.AreEqual( initialSettings.RawPulseShapes_csv,newSettings.RawPulseShapes_csv)' As Boolean
        Assert.AreEqual( initialSettings.RawPulseShapes_mat,newSettings.RawPulseShapes_mat)' As Boolean
        Assert.AreEqual( initialSettings.ListMode_csv, newSettings.ListMode_csv)' As Boolean
        Assert.AreEqual( initialSettings.ListMode_mat, newSettings.ListMode_mat)' As Boolean
        Assert.AreEqual( initialSettings.ListMode_fcs, newSettings.ListMode_fcs)' As Boolean
        Assert.AreEqual( initialSettings.AdditionalListMode_csv, newSettings.AdditionalListMode_csv)' As Boolean 'EAWAG parameters
        Assert.AreEqual( initialSettings.SensorLogs, newSettings.SensorLogs)' As Boolean
        Assert.AreEqual( initialSettings.IIF, newSettings.IIF)' As Boolean
        Assert.AreEqual( initialSettings.MeansPerSetPerFile, newSettings.MeansPerSetPerFile)' As Boolean
        Assert.AreEqual( initialSettings.MeansPerSetPerFile_outputfile,newSettings.MeansPerSetPerFile_outputfile)' As String
        Assert.AreEqual( initialSettings.CYZFile,newSettings.CYZFile)' As Boolean

        'Properties for generating image overviews in batch export.
        Assert.AreEqual( initialSettings.OverViewImageGenerate,newSettings.OverViewImageGenerate)' As Boolean
        Assert.AreEqual( initialSettings.OverViewImageShowScalebar,newSettings.OverViewImageShowScalebar)' As Boolean
        Assert.AreEqual( initialSettings.OverViewImageShowId,newSettings.OverViewImageShowId)' As Boolean
        Assert.AreEqual( initialSettings.OverViewImageOutputFolder, newSettings.OverViewImageOutputFolder)' As String
        Assert.AreEqual( initialSettings.OverviewImageFilenamePrefix, newSettings.OverviewImageFilenamePrefix)' As String
        Assert.AreEqual( initialSettings.OverViewImageMaxWidth,  newSettings.OverViewImageMaxWidth)
        Assert.AreEqual( initialSettings.OverViewImageMaxHeight, newSettings.OverViewImageMaxHeight)
        Assert.AreEqual( initialSettings.OverViewImageExportType, newSettings.OverViewImageExportType)' As Guid = ImageFormat.Jpeg.Guid
        Assert.AreEqual( initialSettings.GpsLog, newSettings.GpsLog)' As Boolean
        Assert.AreEqual( initialSettings.ListmodeExport, newSettings.ListmodeExport)' As Boolean
        CollectionAssert.AreEqual( initialSettings.SelectedListmodeChannels, newSettings.SelectedListmodeChannels)' As String()
        CollectionAssert.AreEqual( initialSettings.SelectedListmodeParameters,newSettings.SelectedListmodeParameters)'As String()
        CollectionAssert.AreEqual( initialSettings.SelectedSets, newSettings.SelectedSets)' As String()
        Assert.AreEqual( initialSettings.OutputDirectory, newSettings.OutputDirectory)' As String
        Assert.AreEqual( initialSettings.IIFImageFormatType, newSettings.IIFImageFormatType)' As Guid = ImageFormat.Jpeg.Guid
        Assert.AreEqual( initialSettings.IIFZipfile, newSettings.IIFZipfile)' As Boolean
        Assert.AreEqual( initialSettings.IIFCropped,newSettings.IIFCropped)' As Boolean
        Assert.AreEqual( initialSettings.IIFCroppedWithScalebar,newSettings.IIFCroppedWithScalebar)' As Boolean
        Assert.AreEqual( initialSettings.IIFUncropped, newSettings.IIFUncropped)' As Boolean
        Assert.AreEqual( initialSettings.IIFUncroppedWithScalebar,newSettings.IIFUncroppedWithScalebar)' As Boolean
        Assert.AreEqual( initialSettings.IIFBackgroundImage, newSettings.IIFBackgroundImage)' As Boolean

        CollectionAssert.AreEqual( initialSettings.SelectedStatisticsChannels,newSettings.SelectedStatisticsChannels)' As String()
        CollectionAssert.AreEqual( initialSettings.SelectedStatisticsParameters, newSettings.SelectedStatisticsParameters)' As String()
        Assert.AreEqual( initialSettings.Statistics_csv, newSettings.Statistics_csv)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_mat,newSettings.Statistics_mat)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_min,newSettings.Statistics_min)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_max,newSettings.Statistics_max)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_mean,newSettings.Statistics_mean)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_SD,newSettings.Statistics_SD)' As Boolean
        Assert.AreEqual( initialSettings.Statistics_Total,newSettings.Statistics_Total)' As Boolean

        'maybe there should be an additional data newSettings struct. Also for channel mode, ArrivalTimeExportMode, and which kind of images etc
        Assert.AreEqual( initialSettings.Smoothing, newSettings.Smoothing)' As ChannelExportMode = ChannelExportMode.smoothed
        Assert.AreEqual( initialSettings.delimiter, newSettings.delimiter)' As String = ";"
        Assert.AreEqual( initialSettings.TimeExportMode,newSettings.TimeExportMode)' As ArrivalTimeExportMode = ArrivalTimeExportMode.Relative
        Assert.AreEqual( initialSettings.SerialNumber, newSettings.SerialNumber)' As String    ' Machine serial number
        Assert.AreEqual( initialSettings.ConfigurationeDate, newSettings.ConfigurationeDate)' As DateTime  ' Machine hardware configuration date
        Assert.AreEqual( initialSettings.DecimalSeparator ,newSettings.DecimalSeparator)' As String
        Assert.AreEqual( initialSettings.AppendStatisticsFile, newSettings.AppendStatisticsFile)' As Boolean ' Append to the statistics file if it allready exists instead of overwriting it.
    End Sub


    ''' <summary>
    ''' Check if the generated XML is still the same.  Even if the properties red, the XML 
    ''' itself could have changed. This may not be en error, but needs to be checked.  If it is not an error
    ''' the XML stored witht he test needs to be updated.
    ''' </summary>
    <TestMethod()>
    Public Sub StoreLoadLowLevelTest()

        Dim initialStream As New IO.MemoryStream()
        Dim copyInitialStream As New IO.MemoryStream()
        Using  sourceStream As New IO.FileStream("Misc/TestExportXml_New.c4e", IO.FileMode.Open)
            sourceStream.CopyTo(initialStream)
        End Using

        initialStream.Seek(0,IO.SeekOrigin.Begin)
        initialStream.CopyTo(copyInitialStream)
        initialStream.Seek(0,IO.SeekOrigin.Begin)
        copyInitialStream.Seek(0,IO.SeekOrigin.Begin)
        Dim settings = ExportSettings.XmlDeserialize(initialStream)

        Dim destStream = New IO.MemoryStream()
        ExportSettings.XmlSerialize(destStream, settings)
        destStream.Seek(0,IO.SeekOrigin.Begin)

        ' uncommnet to dump to local file when investigating a test failure.
        ' Using tmpStream As New IO.FileStream("f:\test.c4e", IO.FileMode.Create)
        '   ExportSettings.XmlSerialize(tmpStream, settings)
        '' End Using

        Assert.AreEqual(copyInitialStream.Length, destStream.Length)
        
        Dim expReader = New IO.StreamReader(copyInitialStream)
        Dim actReader = New IO.StreamReader(destStream)
        Dim lineCtr As Integer = 0
        While Not expReader.EndOfStream
            Dim expected = expReader.ReadLine()
            Dim actual = actReader.ReadLine()
            If lineCtr <> 1 Then
                Assert.AreEqual(expected,actual, String.Format("Line {0}", lineCtr) )
            End If ' Line 1 contains the date/time the file was generate, is regenerated each time. so will differ.
            lineCtr += 1
        End While
        Assert.IsTrue(actReader.EndOfStream)
    End Sub

End Class
