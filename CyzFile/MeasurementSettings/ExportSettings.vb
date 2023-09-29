Imports System.IO
Imports System.Xml
Imports System.Drawing.Imaging
Imports CytoSense.Serializing

Namespace MeasurementSettings

    Public Enum ArrivalTimeExportMode
        Invalid = 0
        None
        Relative
        TimeOfDay
        DateTime
    End Enum

    <Serializable> Public Class ExportSettings
                Implements IXmlDocumentIO
        Public Property Summary As Boolean
        Public Property RawPulseShapes_csv As Boolean
        Public Property RawPulseShapes_mat As Boolean
        Public Property ListMode_csv As Boolean
        Public Property ListMode_mat As Boolean
        Public Property ListMode_fcs As Boolean
        Public Property AdditionalListMode_csv As Boolean 'EAWAG parameters
        Public Property SensorLogs As Boolean
        Public Property IIF As Boolean
        Public Property MeansPerSetPerFile As Boolean
        Public Property MeansPerSetPerFile_outputfile As String
        Public Property CYZFile As Boolean

        Public Property NotifySetting As Integer '0 is none, 1 is windows sounds, 2 is speech
        'Properties for generating image overviews in batch export.
        Public Property OverViewImageGenerate As Boolean
        Public Property OverViewImageShowScalebar As Boolean
        Public Property OverViewImageShowId As Boolean
        Public Property OverViewImageOutputFolder As String
        Public Property OverviewImageFilenamePrefix As String
        Public Property OverViewImageMaxWidth As Integer = 16384
        Public Property OverViewImageMaxHeight As Integer = 16384
        Public Property OverViewImageExportType As Guid = ImageUtil.JPEG_GUID
        Public Property GpsLog As Boolean
        Public Property ListmodeExport As Boolean
        Public Property SelectedListmodeChannels As String()
        Public Property SelectedListmodeParameters As String()
        Public Property SelectedSets As String()
        Public Property SetsXML As String
        Public Property SetsName As String
        Public Property OutputDirectory As String
        Public Property IIFImageFormatType As Guid = ImageUtil.JPEG_GUID
        Public Property IIFZipfile As Boolean
        Public Property IIFCropped As Boolean
        Public Property IIFCroppedWithScalebar As Boolean
        Public Property IIFUncropped As Boolean
        Public Property IIFUncroppedWithScalebar As Boolean
        Public Property IIFBackgroundImage As Boolean

        Public Property SelectedStatisticsChannels As String()
        Public Property SelectedStatisticsParameters As String()
        Public Property Statistics_csv As Boolean
        Public Property Statistics_mat As Boolean
        Public Property Statistics_min As Boolean
        Public Property Statistics_max As Boolean
        Public Property Statistics_mean As Boolean
        Public Property Statistics_SD As Boolean
        Public Property Statistics_Total As Boolean

        'maybe there should be an additional data settings struct. Also for channel mode, ArrivalTimeExportMode, and which kind of images etc
        Public Property Smoothing As ChannelExportMode = ChannelExportMode.smoothed
        Public delimiter As String = ";"
        Public TimeExportMode As ArrivalTimeExportMode = ArrivalTimeExportMode.Relative
        Public SerialNumber As String    ' Machine serial number
        Public ConfigurationeDate As DateTime  ' Machine hardware configuration date
        Public DecimalSeparator As String
        Public AppendStatisticsFile As Boolean ' Append to the statistics file if it allready exists instead of overwriting it.

        Public Enum ChannelExportMode
            smoothed = 0 'default
            rawBytes = 1
            unsmoothed = 2
        End Enum

        Public Sub New()
            SelectedSets = New String() {}
            SelectedListmodeChannels = New String() {}
            SelectedListmodeParameters = New String() {}
            SelectedStatisticsChannels = New String() {}
            SelectedStatisticsParameters = New String() {}
        End Sub

        Public Sub New(other As ExportSettings)
            Summary = other.Summary
            RawPulseShapes_csv = other.RawPulseShapes_csv
            RawPulseShapes_mat = other.RawPulseShapes_mat
            ListMode_csv = other.ListMode_csv
            ListMode_mat = other.ListMode_mat
            ListMode_fcs = other.ListMode_fcs
            AdditionalListMode_csv = other.AdditionalListMode_csv
            SensorLogs = other.SensorLogs
            IIF = other.IIF
            MeansPerSetPerFile = other.MeansPerSetPerFile
            MeansPerSetPerFile_outputfile = other.MeansPerSetPerFile_outputfile
            CYZFile = other.CYZFile
            NotifySetting = other.NotifySetting

            Smoothing = other.Smoothing
            delimiter = other.delimiter
            DecimalSeparator = other.DecimalSeparator

            OverViewImageGenerate     = other.OverViewImageGenerate
            OverViewImageShowScalebar = other.OverViewImageShowScalebar
            OverViewImageShowId       = other.OverViewImageShowId
            OverViewImageOutputFolder = other.OverViewImageOutputFolder
            OverViewImageMaxWidth     = other.OverViewImageMaxWidth
            OverViewImageMaxHeight    = other.OverViewImageMaxHeight
            OverViewImageExportType   = other.OverViewImageExportType

            GpsLog = other.GpsLog
            
            SetsName = other.SetsName
            SetsXML = other.SetsXML

            If other.SelectedSets IsNot Nothing Then
                SelectedSets = other.SelectedSets
            Else
                SelectedSets = New String() {}
            End If

            If other.SelectedListmodeChannels IsNot Nothing Then
                SelectedListmodeChannels = other.SelectedListmodeChannels
            Else
                SelectedListmodeChannels = New String() {}
            End If

            If other.SelectedListmodeParameters IsNot Nothing Then
                SelectedListmodeParameters = other.SelectedListmodeParameters
            Else
                SelectedListmodeParameters = New String() {}
            End If

            ListmodeExport = other.ListmodeExport
            TimeExportMode = other.TimeExportMode

            Smoothing = other.Smoothing
            OutputDirectory = other.OutputDirectory

            SerialNumber = other.SerialNumber
            ConfigurationeDate = other.ConfigurationeDate
            AppendStatisticsFile = other.AppendStatisticsFile

            IIFImageFormatType = other.IIFImageFormatType
            IIFZipfile = other.IIFZipfile
            IIFCropped = other.IIFCropped
            IIFCroppedWithScalebar = other.IIFCroppedWithScalebar
            IIFUncropped = other.IIFUncropped
            IIFUncroppedWithScalebar = other.IIFUncroppedWithScalebar
            IIFBackgroundImage = other.IIFBackgroundImage

            SelectedStatisticsChannels = other.SelectedStatisticsChannels
            SelectedStatisticsParameters = other.SelectedStatisticsParameters
            Statistics_csv = other.Statistics_csv
            Statistics_mat = other.Statistics_mat
            Statistics_min = other.Statistics_min
            Statistics_max = other.Statistics_max
            Statistics_mean = other.Statistics_mean
            Statistics_SD = other.Statistics_SD
            Statistics_Total = other.Statistics_Total
        End Sub

        ''' <summary>
        ''' Number of tasks that are enabled. (for progress observability)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        Public ReadOnly Property nTasks As Integer
            Get
                Dim taskCount As Integer = 0

                ' exports that involve particle(set)s

                If ListmodeExport Then
                    If ListMode_csv Then taskCount += SelectedSets.Length
                    If ListMode_mat Then taskCount += SelectedSets.Length
                    If ListMode_fcs Then taskCount += SelectedSets.Length
                End If
                If AdditionalListMode_csv Then taskCount += SelectedSets.Length
                If RawPulseShapes_csv Then taskCount += SelectedSets.Length
                If RawPulseShapes_mat Then taskCount += SelectedSets.Length
                If CYZFile Then taskCount += SelectedSets.Length
                If MeansPerSetPerFile Then taskCount += SelectedSets.Length
                If OverViewImageGenerate Then taskCount += SelectedSets.Length
                If IIF Then taskCount += SelectedSets.Length

                ' exports that do not involve particle(set)s

                If Summary Then taskCount += 1
                If SensorLogs Then taskCount += 1
                If GpsLog Then taskCount += 1

                Return taskCount
            End Get
        End Property


        Private Sub LoadListModeXmlSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                ListmodeExport = False
                Return
            End If
            If Not node.TryGetAttribute("export", ListmodeExport) Then
                ListmodeExport = False
            End If
            If Not node.TryGetAttribute("arrival_time_mode", TimeExportMode) Then
                TimeExportMode = ArrivalTimeExportMode.Relative
            End If

            Dim formatStr As String = "" 'Default options.
            If node.TryGetAttribute("output", formatStr) Then
                Dim formats = formatStr.Split(","c)
                For Each frmt In formats
                    If frmt.ToLower() = "csv" Then
                        ListMode_csv = True
                    ElseIf frmt.ToLower() = "fcs" Then
                        ListMode_fcs = True
                    ElseIf frmt.ToLower() = "mat" Then
                        ListMode_mat = True
                    Else
                        Throw New Exception(String.Format("Unsupported listmode output format: '{0}'", frmt))
                    End If
                Next
            Else 'Not format specification. If export is true then add csv as default, else leave it as it is.
                If ListmodeExport Then
                    ListMode_csv = True
                End If
            End If 'Else empty format string, could mean no formats at all.

            Dim channelString As String = ""
            If node.TryReadChildElement("channels", channelString) Then
                SelectedListmodeChannels = channelString.Split(","c)
            End If
            Dim parameterString As String = ""
            If node.TryReadChildElement("parameters", parameterString) Then
                SelectedListmodeParameters = parameterString.Split(","c)
            End If
        End Sub


        Private Sub StoreOverviewImgSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export",       OverViewImageGenerate)
            doc.AddAttribute(node, "scalebar",     OverViewImageShowScalebar)
            doc.AddAttribute(node, "particle_id",  OverViewImageShowId)
            doc.AddAttribute(node, "max_width",    OverViewImageMaxWidth)
            doc.AddAttribute(node, "max_height",   OverViewImageMaxHeight)
            doc.AddAttribute(node, "img",          LabelForImgType(OverViewImageExportType)) ' Convert to short string like jpg/png/tif/etc, instead of GUID.
        End Sub


        Private Sub LoadOverviewImageSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                OverViewImageGenerate = False
                Return
            End If

            node.TryGetAttribute("export",OverViewImageGenerate, False)
            node.TryGetAttribute("scalebar",OverViewImageShowScalebar, False)
            node.TryGetAttribute("particle_id",OverViewImageShowId, False)
            node.TryGetAttribute("max_width", OverViewImageMaxWidth, 16384)
            node.TryGetAttribute("max_height", OverViewImageMaxHeight, 16384)

            Dim imgExportStr As String = ""
            node.TryGetAttribute("img",imgExportStr,"jpg")
            OverViewImageExportType = FileTypeGuidFromLabel(imgExportStr)
        End Sub

        Private Sub StoreCsvSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "delimiter", delimiter)
            doc.AddAttribute(node, "decimal_separator", DecimalSeparator)
        End Sub

        Private Sub StoreSummarySettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node,"export", Summary)
        End Sub

        Private Sub StoreGpsSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export", GpsLog)
        End Sub

        Private Sub StoreSensorLogsSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export", SensorLogs)
        End Sub

        Private Sub StoreListModeXml(doc As XmlDocument, node As XmlElement)

            Dim formats = New List(Of String)()
            If ListMode_csv Then formats.Add("csv")
            If ListMode_fcs Then formats.Add("fcs")
            If ListMode_mat Then formats.Add("mat")
            doc.AddAttribute(node, "export", ListmodeExport)

            doc.AddAttribute(node, "arrival_time_mode", TimeExportMode.ToString())
            doc.AddAttribute(node, "output", String.Join(","c, formats))
            doc.AppendChildElement(node, "channels",   String.Join(","c, SelectedListmodeChannels))
            doc.AppendChildElement(node, "parameters", String.Join(","c, SelectedListmodeParameters))
        End Sub

        Private Sub StoreSetStatisticsModeSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export", MeansPerSetPerFile)

            Dim formats = New List(Of String)()
            If Statistics_csv Then
                formats.Add("csv")
            End If
            If Statistics_mat Then
                formats.Add("mat")
            End If

            doc.AddAttribute(node, "output", String.Join(","c, formats))
            doc.AddAttribute(node, "append", AppendStatisticsFile)
            doc.AppendChildElement(node, "filename", MeansPerSetPerFile_outputfile)
            doc.AppendChildElement(node, "channels", String.Join(","c, SelectedStatisticsChannels))
            doc.AppendChildElement(node, "parameters", String.Join(","c, SelectedStatisticsParameters))

            Dim s As New List(Of String)
            If Statistics_min  Then s.Add("min")
            If Statistics_max  Then s.Add("max")
            If Statistics_mean Then s.Add("mean")
            If Statistics_SD   Then s.Add("sd")
            If Statistics_Total Then s.Add("total")
            doc.AppendChildElement(node, "statistics", String.Join(","c, s))
        End Sub

        Private Sub StoreCyzFileSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export", CYZFile)
        End Sub

        Private Sub StorePulseModeSettings(doc As XmlDocument, node As XmlElement)
            Dim export = (RawPulseShapes_csv OrElse RawPulseShapes_mat)
            Dim formats = New List(Of String)()

            If RawPulseShapes_csv Then formats.Add("csv")
            If RawPulseShapes_mat Then formats.Add("mat")

            doc.AddAttribute(node, "export", export)
            doc.AddAttribute(node, "output", String.Join(","c, formats))
            doc.AddAttribute(node, "data", Smoothing.ToString())
        End Sub

        Private Sub StoreImageSettings(doc As XmlDocument, node As XmlElement)
            doc.AddAttribute(node, "export",             IIF)
            doc.AddAttribute(node, "img",                LabelForImgType(IIFImageFormatType))
            doc.AddAttribute(node, "output",             If(IIFZipfile, "zip", "folder"))
            doc.AddAttribute(node, "cropped",            IIFCropped)
            doc.AddAttribute(node, "cropped_scalebar",   IIFCroppedWithScalebar)
            doc.AddAttribute(node, "uncropped",          IIFUncropped)
            doc.AddAttribute(node, "uncropped_scalebar", IIFUncroppedWithScalebar)
            doc.AddAttribute(node, "background",         IIFBackgroundImage)
        End Sub

        Public Sub XmlDocumentWrite(doc As XmlDocument, node As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            doc.AddAttribute(node, "serial",             SerialNumber)
            doc.AddAttribute(node, "configuration_date", ConfigurationeDate.ToString("yyyy-MM-dd"))
            doc.AddAttribute(node, "creation_date",      DateTime.Now.ToString("u"))

            StoreCsvSettings(              doc, doc.AppendChildElement(node, "csv_settings"))
            doc.AppendChildElement(node, "output_folder", OutputDirectory)
            StoreSummarySettings(          doc, doc.AppendChildElement(node, "summary"))
            StoreGpsSettings(              doc, doc.AppendChildElement(node, "gps_log"))
            StoreSensorLogsSettings(       doc, doc.AppendChildElement(node, "sensor_logs"))
            doc.AppendChildElement(node, "selected_sets", String.Join(","c, SelectedSets))
            StoreListModeXml(              doc, doc.AppendChildElement(node, "listmode"))
            StoreSetStatisticsModeSettings(doc, doc.AppendChildElement(node, "set_statistics"))
            StoreCyzFileSettings(          doc, doc.AppendChildElement(node, "cyzfile"))
            StorePulseModeSettings(        doc, doc.AppendChildElement(node, "pulses"))
            StoreImageSettings(            doc, doc.AppendChildElement(node, "images"))
            StoreOverviewImgSettings(      doc, doc.AppendChildElement(node, "overview_img"))
        End Sub


        Private Sub LoadCsvSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                Return ' element was nto present, just ignore and use default.
            End If

            node.TryGetAttribute("delimiter",         delimiter, ",")
            node.TryGetAttribute("decimal_separator", DecimalSeparator, ".")
        End Sub

        Private Sub LoadSummarySettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                Return
            End If
            node.TryGetAttribute("export", Summary, False)
        End Sub

        Private Sub LoadGpsSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                Return
            End If
            node.TryGetAttribute("export", GpsLog, False)
        End Sub

        Private Sub LoadSensorLogsSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                Return
            End If
            node.TryGetAttribute("export", SensorLogs, False)
        End Sub

        Private Sub LoadSelectedSets(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                Return
            End If
            SelectedSets = node.InnerText.Split(","c)
        End Sub

        Private Sub LoadSetStatisticsModeSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                MeansPerSetPerFile = False
                Return
            End If

            node.TryGetAttribute("export", MeansPerSetPerFile, False)
            node.TryGetAttribute("append", AppendStatisticsFile, False)

            Dim formatString As String = ""
            node.TryGetAttribute("output", formatString) 
            Dim formats = formatString.Split(","c)
            For Each frmt In formats
                If Not String.IsNullOrEmpty(frmt) Then
                    If frmt.ToLower() = "csv" Then
                        Statistics_csv = True
                    Else If frmt.ToLower = "mat" Then
                        Statistics_mat = True
                    Else
                        Throw New Exception(String.Format("Unsupported statistics output format: '{0}'", frmt))
                    End If
                End If 'Ignore completely empty entries
            Next

            node.TryReadChildElement("filename", MeansPerSetPerFile_outputfile, "") 

            Dim chanStr As String = ""
            node.TryReadChildElement("channels", chanStr) 
            SelectedStatisticsChannels = chanStr.Split(","c)

            Dim parStr As String = ""
            node.TryReadChildElement("parameters", parStr) 
            SelectedStatisticsParameters = parStr.Split(","c)

            Dim statStr As String = ""
            node.TryReadChildElement("statistics", statStr) 
            Dim stats = statStr.Split(","c)
            For Each s In stats
                Select s.ToLower()
                    Case "min"
                        Statistics_min = True
                    Case "max"
                        Statistics_max = True
                    Case "mean"
                        Statistics_mean = True
                    Case "sd"
                        Statistics_SD = True
                    Case "total"
                        Statistics_Total = True
                    Case ""
                        'IGNORE empty entries.
                    Case Else
                        Throw New Exception(String.Format("Unsupported statistic: '{0}'", s))
                End Select
            Next
        End Sub

        Private Sub LoadCyzFileSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                CYZFile = False
                Return
            End If
            node.TryGetAttribute("export",CYZFile,False)
        End Sub


        Private Sub LoadPulseModeSettings(doc As XmlDocument, node As XmlElement)
            RawPulseShapes_csv = False
            RawPulseShapes_mat = False
            If node Is Nothing Then
                Return
            End If
            Dim export As Boolean = False
            node.TryGetAttribute("export", export, False)
            If export Then 'No export, then no formats are defined, this behavior may change in the future...
                Dim outputStr As String = ""
                node.TryGetAttribute("output", outputStr)
                If Not String.IsNullOrEmpty(outputStr) Then
                    Dim formats = outputStr.Split(","c)
                    For Each frmt In formats
                        If frmt.ToLower() = "csv" Then
                            RawPulseShapes_csv = True
                        ElseIf frmt.ToLower() = "mat" Then
                            RawPulseShapes_mat = True
                        Else
                            Throw New Exception(String.Format("Unsupported statistics output format: '{0}'", frmt))
                        End If
                    Next
                End If
            End If
            node.TryGetAttribute("data", Smoothing,ChannelExportMode.smoothed)
        End Sub

        Private Sub LoadImageSettings(doc As XmlDocument, node As XmlElement)
            If node Is Nothing Then
                IIF = False
                Return
            End If
            node.TryGetAttribute("export", IIF, False)
            Dim imgFormatStr As String = ""
            node.TryGetAttribute("img", imgFormatStr, "jpg")
            IIFImageFormatType = FileTypeGuidFromLabel(imgFormatStr)
            Dim outputStr As String = ""
            node.TryGetAttribute("output", outputStr)
            IIFZipfile = (outputStr = "zip")
            node.TryGetAttribute("cropped",            IIFCropped, False)
            node.TryGetAttribute("cropped_scalebar",   IIFCroppedWithScalebar, False)
            node.TryGetAttribute("uncropped",          IIFUncropped, False)
            node.TryGetAttribute("uncropped_scalebar", IIFUncroppedWithScalebar, False)
            node.TryGetAttribute("background",         IIFBackgroundImage, False)
        End Sub




        Public Sub XmlDocumentRead(doc As XmlDocument, node As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            node.TryGetAttribute("serial", SerialNumber)
            
            Dim configDateStr As String = ""
            If node.TryGetAttribute("configuration_date",configDateStr) Then
                ConfigurationeDate = DateTime.Parse(configDateStr)
            End If 'Else config date not present, just ignore.

            LoadCsvSettings(               doc, DirectCast(node.Item("csv_settings"),   XmlElement))
            node.TryReadChildElement("output_folder", OutputDirectory)
            LoadSummarySettings(           doc, DirectCast(node.Item("summary"),        XmlElement))
            LoadGpsSettings(               doc, DirectCast(node.Item("gps_log"),        XmlElement))
            LoadSensorLogsSettings(        doc, DirectCast(node.Item("sensor_logs"),    XmlElement))
            LoadSelectedSets(              doc, DirectCast(node.Item("selected_sets"),  XmlElement))
            LoadListModeXmlSettings(       doc, DirectCast(node.Item("listmode"),       XmlElement))
            LoadSetStatisticsModeSettings( doc, DirectCast(node.Item("set_statistics"), XmlElement))
            LoadCyzFileSettings(           doc, DirectCast(node.Item("cyzfile"),        XmlElement))
            LoadPulseModeSettings(         doc, DirectCast(node.Item("pulses"),         XmlElement))
            LoadImageSettings(             doc, DirectCast(node.Item("images"),         XmlElement))
            LoadOverviewImageSettings(     doc, DirectCast(node.Item("overview_img"),   XmlElement))
        End Sub


        ''' <summary>
        ''' Version that uses the XmlDocument interface for serializing, same as used for the sets list.
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="expSet"></param>
        Public Shared Sub XmlSerialize(str As Stream, expSet As ExportSettings)
            Dim xmlDocument As New XmlDocument()
            Dim xmlDecl = xmlDocument.CreateXmlDeclaration("1.0", "utf-8", Nothing)
            xmlDocument.AppendChild(xmlDecl)

            Dim rootElement As XmlElement = xmlDocument.CreateElement("exportsettings")
            xmlDocument.AppendChild(rootElement)

            expSet.XmlDocumentWrite(xmlDocument, rootElement)

            xmlDocument.Save(str)
        End Sub


        Public Shared Function XmlDeSerialize(str As Stream) As ExportSettings
            Dim xmlDocument As New XmlDocument()
            Dim newSet = New ExportSettings()

            xmlDocument.Load(str)
            newSet.XmlDocumentRead(xmlDocument, xmlDocument.Item("exportsettings"))
            Return newSet
        End Function



        ''' <summary>
        ''' Returns the label to use for the filetype in the xml.  This has a large overlap with the file extension
        ''' and it should probably be moved to the UImage class.
        ''' </summary>
        ''' <param name="fileTypeGuid">The GUI of the image format that we want to know the extension for.</param>
        ''' <returns>The extension, without the "." to use for a file.  E.g. for a JPEG file, it will return "jpg"</returns>
        ''' <remarks></remarks>
        Private Shared Function LabelForImgType(fileTypeGuid As Guid) As String
            If fileTypeGuid = ImageUtil.JPEG_GUID Then
                Return "jpg"
            ElseIf fileTypeGuid = ImageUtil.EMF_GUID Then
                Return "emf"
            ElseIf fileTypeGuid = ImageUtil.BMP_GUID Then
                Return "bmp"
            ElseIf fileTypeGuid = ImageUtil.WMF_GUID Then
                Return "wmf"
            ElseIf fileTypeGuid = ImageUtil.GIF_GUID Then
                Return "gif"
            ElseIf fileTypeGuid = ImageUtil.PNG_GUID Then
                Return "png"
            ElseIf fileTypeGuid = ImageUtil.TIFF_GUID Then
                Return "tif"
            Else
                Throw New Exception(String.Format("Unsupported filetype: '{0}'", fileTypeGuid))
            End If
        End Function

        ''' <summary>
        ''' Returns a filetype GUID for a specific label.  The opposite of the function LabelForImageType.
        ''' </summary>
        ''' <param name="lbl"></param>
        ''' <returns></returns>
        Private Shared Function FileTypeGuidFromLabel(lbl As String) As Guid
            If lbl = "jpg" Then
                Return ImageUtil.JPEG_GUID
            ElseIf lbl = "emf" Then
                Return ImageUtil.EMF_GUID
            ElseIf lbl = "bmp" Then
                Return ImageUtil.BMP_GUID
            ElseIf lbl = "wmf" Then
                Return ImageUtil.WMF_GUID
            ElseIf lbl = "gif" Then
                Return ImageUtil.GIF_GUID
            ElseIf lbl = "png" Then
                Return ImageUtil.PNG_GUID
            ElseIf (lbl = "tif" OrElse lbl = "tiff") Then
                Return ImageUtil.TIFF_GUID
            Else
                Throw New Exception(String.Format("Unsupported filetype: '{0}'", lbl))
            End If
        End Function



    End Class
End Namespace
