Imports System.Drawing
Imports System.Runtime.Serialization

Namespace CytoSettings

    ''' <summary>
    ''' Description of the camera used in the system.  This data is a direct match to the CameraInformation
    ''' from the PixelInk SDK.  I created a matching structure here because the pixelInk sdk one is not
    ''' marked as serializable, and to avoid a dependency from the basic settings dll on the PixelInk sdk
    ''' dll. 
    ''' </summary>
    <Serializable()> _
    Public Class CameraDescriptors
        Public BootLoadVersion As String
        Public CameraName As String
        Public Description As String
        Public FirmwareVersion As String
        Public FpgaVersion As String
        Public LensDescription As String
        Public ModelName As String
        Public SerialNumber As String
        Public VendorName As String
        Public XmlVersion As String
    End Class

    ' NOTE: These are values for unrotated images.
    <Serializable()> _
    Public Structure RoiInfo
        Public LeftMin As Integer
        Public LeftMax As Integer
        Public TopMin As Integer
        Public TopMax As Integer

        Public WidthMin As Integer
        Public WidthMax As Integer
        Public HeightMin As Integer
        Public HeightMax As Integer
    End Structure


    ''' <summary>
    ''' Several camera features, we load these from the camera, so in theory we should be a little
    ''' more camera independent in the future
    ''' </summary>
    <Serializable()> _
    Public Class CameraFeatures
        Public SensorWidth As Integer   ' Width of the sensor in pixels
        Public SensorHeight As Integer  ' Height of the sensor in pixels.
        Public BrightnessSupported As Boolean  'Does the camera support a brightness setting, and if so what are min/max value
        Public BrightnessMin As Single
        Public BrightnessMax As Single
        Public AvailableGainFactors As List(Of Single)
        ' ROI ranges/limits, not sure yet how to do this.
        ' Min Value is also used as step size
        Public RoiRanges As RoiInfo
        Public PixelPitch As Single 
        Public FlashDelay_mus As Integer  'Extra delay to add to the flash in micro seconds.  Some of the newer cameras require this to get the flash at the correct time.
    End Class


    <Serializable()> Public Structure IIFSettings
        Public release As Serializing.VersionTrackableClass

        Public Shared ReadOnly FlashDurationStepSize As Double = 7.8125 'Number of nanosecond per flash duration step.

        ' GainList() As Double = {0, 1.7, 3.2, 4.6, 6.7, 8.4, 10.1, 11.7, 13.8, 15.1, 16.6, 16.9, 17.3, 17.6, 17.7}
        ' For other camera's this may be different.
        ' For new camera PL-D722 : {0, 1.14, 2.48, 4.08, 6.02, 7.2, 8.53, 10.1, 12.04, 14.53, 18.06}
        ' NOTE: IIFGain is interpreted incorrectly as index into gain list, but camera uses it directly as dBValue.
        ' This CTor is deprecated, use the other one for new machines.
        ' <Obsolete("Handling of IIFGain is incorrect, please use the other constructor instead.")> _
        Public Sub New(ByVal IIFGain As Int16, ByVal IIFBrightness As Int16, ByVal IIFROITop As Int16, ByVal IIFROILeft As Int16, ByVal IIFROIWidth As Int16, ByVal IIFROIHeight As Int16, ByVal IIFHorizontalFlip As Boolean, ByVal IIFVerticalFlip As Boolean, ByVal IIFRotate As Int16, ByVal IIFCompression As Boolean, ByVal IIFTriggerPositive As Boolean, ByVal IIFDSPComparisonChannel As channel, ByVal IIFEnableRS232 As Boolean, ByVal IIFCameraDelay As UInteger, ByVal IIFImageScaleMuPerPixel As Single, ByVal IIFBaud As Integer)
            release = New Serializing.VersionTrackableClass(New Date(2012, 7, 30))
            EnableRS232 = IIFEnableRS232
            RS232Baud = IIFBaud
            Gain = IIFGain
            Brightness = IIFBrightness
            ROITop = IIFROITop
            ROILeft = IIFROILeft
            ROIWidth = IIFROIWidth
            ROIHeight = IIFROIHeight
            HorizontalFlip = IIFHorizontalFlip
            VerticalFlip = IIFVerticalFlip
            Rotate = IIFRotate
            Compression = IIFCompression
            TriggerPositive = IIFTriggerPositive
            DSPComparisonChannel = IIFDSPComparisonChannel
            ImageScaleMuPerPixel = IIFImageScaleMuPerPixel ' = CameraPixelSizeInMu / Magnification
            CameraDelay = IIFCameraDelay
            AutoCameraDelay = True
            CameraDelayFactor = 1
            RS232Handshaking = IO.Ports.Handshake.XOnXOff
            ExposureTime = 0 'unused due to buggy pxl software
            Me.framerate = 50
            BigParticleThreshold = 1000
            Camera = Nothing
            CameraFeatures = Nothing
            dBGain = Single.NaN
            opticalMagnification = Single.NaN
            DefaultTakeLargeParticlePictures = True
        End Sub

        ' NOTE: For new electronics, the DSP RS232 settings are no longer used.
        ' Add some optional parameters, now use named parameters, makes reading easier, NOTE:
        ' take care NOT TO CHANGE THE DEFAULTS, or check all use of the CTor to make sure you
        ' are not accidentally changing stuff.
        Public Sub New(Optional chan As channel = Nothing, Optional gain As Single=0.0, Optional bright As Int16=0,  Optional magnification As Single = 16, Optional delay As UInteger = &H29A00, Optional flash As Integer = -1)
            release = New Serializing.VersionTrackableClass(New Date(2012, 7, 30))
            EnableRS232 = True
            RS232Baud = 460800
            Brightness = bright
            'Set to max sensor size on loading
            ROITop = -1 
            ROILeft = -1
            ROIWidth = -1
            ROIHeight = -1
            HorizontalFlip = True
            VerticalFlip = False
            Rotate = 0
            Compression = True
            TriggerPositive = True
            DSPComparisonChannel = chan
            ImageScaleMuPerPixel = Single.NaN
            CameraDelay = delay
            AutoCameraDelay = True
            CameraDelayFactor = 1
            RS232Handshaking = IO.Ports.Handshake.XOnXOff
            ExposureTime = 0 'unused due to buggy pxl software
            Me.framerate = 50
            BigParticleThreshold = 1000
            Camera = Nothing
            CameraFeatures = Nothing
            dBGain = gain
            opticalMagnification = magnification
            FlashDurationCount = flash
            DefaultTakeLargeParticlePictures = True
        End Sub



        <OnDeserializing()> _
        Private Sub SetValuesOnDeserializing(ByVal context As StreamingContext)
            dBGain = Single.NaN ' Default value is 0, but that is a legal value, so we need this to signal it was not actually deserialized.
                                ' If a value was actually serialized, then that should overwrite this values.
            FlashDurationCount = -1 ' And actually serialized value will override this value.
            opticalMagnification = Single.NaN 'If  a real value is present that will overwrite, this allows us to detect unserialized values that need to calculated.
        End Sub

        Dim EnableRS232 As Boolean
        Dim minimumRequiredDSPVersion As String
        Dim RS232Baud As Integer
        Dim RS232Handshaking As System.IO.Ports.Handshake
        Dim Gain As Int16 'OLD DB VALUE, INCORRECT, WAS USED AS Index by Our software, but camera tried to use it as DB value (Round to nearest supported value)
        Dim Brightness As Int16
        Dim ROITop As Int16
        Dim ROILeft As Int16
        Dim ROIWidth As Int16
        Dim ROIHeight As Int16
        Dim HorizontalFlip As Boolean
        Dim VerticalFlip As Boolean
        Dim Rotate As Int16
        Dim Compression As Boolean
        Dim TriggerPositive As Boolean
        Dim DSPComparisonChannel As channel
        Dim CameraDelay As UInteger
        <Obsolete> Dim CameraDelayOffset_us As Integer 'Left in for backwards compatibility of serialized settings.
        Dim AutoCameraDelay As Boolean
        Dim CameraDelayFactor As Double 'for bigger flow cell
        private ImageScaleMuPerPixel As Single ' = CameraPixelSizeInMu / Magnification
        <Obsolete("Use the OpenCvBackground property")> ' Do not use directly, use the OpenCvBackground property
        Dim Background As Imaging.CyzFileBitmap ' Was System.Drawing.Image Image 
        Dim _backgroundStream As Serializing.CytoMemoryStream

        Private _ImageScaleMuPerPixel As Single
        Public Property ImageScaleMuPerPixelP As Single
            Get
                If _enableOverrideMuPerPixel Then
                    Return _overrideMuPerPixel
                Else
                    Return ImageScaleMuPerPixel
                End If
            End Get
            Set(value As Single)
                ImageScaleMuPerPixel = value
            End Set
        End Property
        <NonSerialized> Private _overrideMuPerPixel As Single
        <NonSerialized> Private _enableOverrideMuPerPixel As Boolean
        <ComponentModel.Browsable(False)>
        Public WriteOnly Property OverrideMuPerPixel As Single
            Set(value As Single)
                _overrideMuPerPixel = value
            End Set
        End Property
        <ComponentModel.Browsable(False)>
        Public Property EnableOverrideMuPerPixel As Boolean
            Get
                Return _enableOverrideMuPerPixel
            End Get
            Set(value As Boolean)
                _enableOverrideMuPerPixel = value
            End Set
        End Property
        'Public ReadOnly Property BackgroundStream As IO.MemoryStream
        '    Get
        '        If _backgroundStream Is Nothing Then
        '            _backgroundStream = New IO.MemoryStream()
        '            Background.Save(_backgroundStream, Imaging.ImageFormat.Bmp)
        '        End If
        '        Return _backgroundStream
        '    End Get
        'End Property
        ''' <summary>
        ''' Not pretty, caching a converted OpenCV background image here to reuse for processing all the images. 
        ''' It really does not belong here, but for now i am copying the
        ''' previous implementation so I do not have to rewrite it all.
        ''' To Convert from stored image to OpenCV, we save it to a memory stream, and load form there.
        ''' NOTE: If no background is present then OpenCvbacgrkound returned is also Nothing!
        ''' </summary>
        <NonSerialized> Private _openCvBackground As OpenCvSharp.Mat 
        Public ReadOnly Property OpenCvBackground As OpenCvSharp.Mat
            Get
#Disable Warning BC40000 ' Type or member is obsolete
                If _openCvBackground Is Nothing AndAlso Background IsNot Nothing Then
                    SyncLock Background
                        Using imgStr As New System.IO.MemoryStream(Background.Data)
                            _openCvBackground = ImageUtil.LoadOpenCvImage(imgStr)
                        End Using
                        _openCvBackgroundMean = OpenCvSharp.Cv2.Mean(_openCvBackground).Val0
                    End SyncLock
                End If
#Enable Warning BC40000 ' Type or member is obsolete
                Return _openCvBackground
            End Get
        End Property

        <NonSerialized> Private _openCvBackgroundMean As Double
        Public ReadOnly Property OpenCvBackgroundMean As Double
        Get
#Disable Warning BC40000 ' Type or member is obsolete
                If _openCvBackground Is Nothing AndAlso Background IsNot Nothing Then
                    SyncLock Background
                        Using imgStr As New System.IO.MemoryStream(Background.Data)
                            _openCvBackground = ImageUtil.LoadOpenCvImage(imgStr)
                        End Using
                        _openCvBackgroundMean = OpenCvSharp.Cv2.Mean(_openCvBackground).Val0
                    End SyncLock
                End If
#Enable Warning BC40000 ' Type or member is obsolete
                Return _openCvBackgroundMean
        End Get
        End Property

        Public Property EnableAutoCrop As Boolean
        Public Property CropBGThreshold As Integer
        Public Property CropErodeDilateSteps As Integer
        Public Property CropMarginBase As Integer
        Public Property CropMarginFactor As Double
         

        Dim ExposureTime As Single
        Dim framerate As Single 'percentage , actual speed dependent on ROI
        Dim DSPCompileVersion As String
        Dim CamPowerThroughUSB As Boolean
        <Obsolete()> ' This is NOT supported any more, do not use this.  
        Dim BitDepth As BitDepth

        Dim BigParticleThreshold As UInt16 'only for PICIIF

        Dim Camera As CameraDescriptors 'Description of the camera, loaded form the camera itself. NOTE: Only valid in IIF measurements!
        Dim CameraFeatures As CameraFeatures
        Dim dBGain As Single ' The gain to use in DB
        Dim opticalMagnification As Single ' Combine with camera pixel pitch to get ImageScaleMuPerPixel
        Dim FlashDurationCount As Integer ' Duration of flash for new electronics, in steps of 7.8125 nanoseconds, it is a byte value in the FPGA< max can be read from FPGA, -1 is used to indicate NOT set!
        Private DefaultTakeLargeParticlePictures? As Boolean ' The default value for new measurements.
        Public TargetPosition As Point ' Expected position of the particle in the image.  Use to set max delays and in the future as an aide
                                       ' to auto cropping.  Coordinates are for the FULL image, not relative to the ROI.
        Public FactoryTargetPosition As Point ' Expected position of the particle in the image.  Use to set max delays and in the future as an aide
                                              ' to auto cropping.  Coordinates are for the FULL image, not relative to the ROI.

        Public Property AllwaysTakeLargerParticlePictures As Boolean
            Get
                If Not DefaultTakeLargeParticlePictures.HasValue Then
                    DefaultTakeLargeParticlePictures = True
                End If
                Return DefaultTakeLargeParticlePictures.Value
            End Get
            Set(value As Boolean)
                DefaultTakeLargeParticlePictures = value
            End Set
        End Property

        Public ReadOnly Property DspCompileDate As DateTime
            Get
                If String.IsNullOrEmpty(DSPCompileVersion) Then
                    ' We don't have a compile date yet, so assume the oldest version available.
                    Return New DateTime ' Some date before the smart grid was implemented.
                ElseIf DSPCompileVersion.Length <= 10 Then    ' Old version
                    Dim dateParts = DSPCompileVersion.Split("-"c)
                    Return New DateTime(Int32.Parse(dateParts(2)), Int32.Parse(dateParts(1)), Int32.Parse(dateParts(0)))
                Else    'New version
                    Return DateTime.Parse(DSPCompileVersion)
                End If
            End Get
        End Property

    End Structure
    <Obsolete()> ' Not supported anymore.
    Public Enum BitDepth
        Mono8
        Mono16
    End Enum
End Namespace