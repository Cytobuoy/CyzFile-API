Imports OpenCvSharp

Namespace Data.ParticleHandling

    <Serializable()> Public Class CytoImage
        Private Shared _log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        'Settings for output
        <Obsolete()> Private _ScaleBar As Boolean = True

        'Properties
        <Obsolete()> Private imageCropped As Boolean = False
        <Obsolete()> Private userDefinedCrop As Boolean = False 'not yet implemented

        Public CropResult As CropResultEnum = CropResultEnum.AwaitingCrop
        Public Enum CropResultEnum
            AwaitingCrop
            CropOK
            NoImage
            NoBlobFound
            BackgroundNeededButNotFound
            BackgroundWrong
        End Enum


        'Data
        <NonSerialized>
        Private _imageMat As Mat
        Private _imageStream As IO.MemoryStream
        <nonSerialized>
        Private _imageParticleData As particleDataStruct
        Private _croppedRect As Rect
        
        Private _cytoSettings As CytoSettings.CytoSenseSetting 'is used only for background image currently

        'Particle width data
        <Obsolete()> Private _pwst As System.Drawing.Point
        <Obsolete()> Private _pwend As System.Drawing.Point


        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, im As IO.MemoryStream, Optional crpRct As Rect = Nothing)
            _cytoSettings = cytosettings
            _imageStream = im
            _croppedRect = crpRct
            If _imageStream.Capacity > _imageStream.Length Then
                _imageStream.Capacity = Cint(_imageStream.Length)
            End If
        End Sub

        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, im As Mat, ims As IO.MemoryStream, crpRct As OpenCvSharp.Rect)
            _cytoSettings = cytosettings
            _imageMat = im
            _imageStream = ims
            _croppedRect = crpRct
        End Sub

        Public Sub ProcessImage(imgProcSettings As ImageProcessingSettings)
            _processingSettings = imgProcSettings
            GetCroppedImage(imgProcSettings)
            CalculateParticleData()
        End Sub

        ''' <summary>
        ''' Throw away the processed image, forcing it to be regenerated the next time we need it.
        ''' </summary>
        ''' <remarks>We lock the image stream, to make sure we do not do this in the middle of an
        ''' image processing step.</remarks>
        Public Sub ClearProcessedImage()
            SyncLock _imageStream
                CropResult = CropResultEnum.AwaitingCrop
            End SyncLock
        End Sub

        Public Function GetCroppedImage(imgProcSettings As ImageProcessingSettings) As Mat
            Return GetCroppedImage(imgProcSettings.MarginBase, imgProcSettings.MarginFactor, imgProcSettings.Threshold, imgProcSettings.ErosionDilation,
                            imgProcSettings.ApplyBrightFieldCorrection, imgProcSettings.ExtendObjectDetection)
        End Function


        ''' <summary>
        ''' Crop the image using the specified parameters and return the cropped image.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' NOTE: Results are cached, so if you use the same parameters next time, then the
        ''' previous results will be used.
        ''' REMOVE This function and all image processing here...
        ''' </remarks>
        Public Function GetCroppedImage(marginBase As Integer, marginFactor As Double, bgThreshold As Integer, erosionDilation As Integer, brightFieldCorrection As Boolean, extendObjectDetection As Boolean) As OpenCvSharp.Mat
            If _imageStream Is Nothing Then
                CropResult = CropResultEnum.NoImage
                Return Nothing
            Else
                Try
                    Return AutoCropOpenCV(_cytoSettings.iif.OpenCvBackground, _cytoSettings.iif.OpenCvBackgroundMean, marginBase, marginFactor, bgThreshold, erosionDilation, brightFieldCorrection, extendObjectDetection, _croppedRect)
                Catch ex As Exception
                    Throw New Exception(ex.ToString())
                End Try
            End If
        End Function

		Private Function GetImageAsMat() As Mat
            Return ImageUtil.LoadOpenCvImage(_imageStream)
        End Function

        Public ReadOnly Property ImageMat As Mat
            Get
                If _imageMat Is Nothing Then
                    _imageMat = GetImageAsMat()
                End If
                Return _imageMat
            End Get
        End Property

        Public Property ImageStream As IO.MemoryStream
            Get
                Return _imageStream
            End Get
            Set(ByVal value As IO.MemoryStream)
                _imageStream = value
            End Set
        End Property

        Public ReadOnly Property ParticleData As ParticleDataStruct
            Get
                Return _imageParticleData
            End Get
        End Property

        ''' <summary>
        ''' Return the bounding box used in cropping this image (if it was cropped)
        ''' </summary>
        ''' <returns></returns>

        Public ReadOnly Property CropRect As OpenCvSharp.Rect
            Get 
                Return _croppedRect
            End Get
        End Property

        ''' <summary>
        ''' Check whether or not the image has been cropped by comparing its crop rectangle. 
        ''' If the image is not cropped the crop rectangle should have all 0 as values.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsCropped As Boolean
            Get
                Return Not _croppedRect = Rect.FromLTRB(0,0,0,0)
            End Get
        End Property

        Public ReadOnly Property IsProcessed As Boolean
            Get
                Return CropResult <> CropResultEnum.AwaitingCrop
            End Get
        End Property

        ' Settings used for the previous cropping, if the settings change we need to crop again, else we can
        ' reuse the previous crop rectangle.
        <NonSerialized()> Private _processingSettings As ImageProcessingSettings


        ' Temporary, check how much it improves performance when caching the image itself, NOTE:
        ' extended object detection flags, and bright field correction flag need to be taken into account yet.
        'They are not currently.  Could that be the performance issue>
        <NonSerialized()> Private _croppedImage As Mat

        Public Const IMG_STEP_SIZE As Integer = 80

        Public Structure ParticleDataStruct
            Public x As Integer
            Public y As Integer
            Public width As Double
            Public height As Double
            Public area As Double
            Public sharpnessScore As Double

            Public hasData As Boolean
        End Structure

        Public Structure ImageProcessingSettings
            Public MarginBase As Integer
            Public MarginFactor As Double
            Public Threshold As Integer
            Public ErosionDilation As Integer
            Public ApplyBrightFieldCorrection As Boolean
            Public ExtendObjectDetection As Boolean
        End Structure

        ''' <summary>
        ''' The function returns a cropped image if possible, it stores the crop results, so the next time you call it, if the
        ''' previous results are still valid, it will simply return these coordinates and not do the whole cropping again.
        ''' If for some reason it cannot perform the crop, it will set the CropResult function to indicate the reason and
        ''' then it will simply return the entire image uncropped. (This may not be the most pure option,
        ''' returning nothing may be more correct, but it is the one that works best with the
        ''' rest of the program currently).
        ''' </summary>
        ''' <param name="sourceStream">The image to find objects in.</param>
        ''' <param name="bgImg">The background image</param>
        ''' <param name="marginBase"></param>
        ''' <param name="marginFactor"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function AutoCropOpenCV(ByVal bgImg As Mat, bgMean As Double, marginBase As Integer, marginFactor As Double, bgThreshold As Integer, erosionDilation As Integer, brightFieldCorrection As Boolean, extendObjectDetection As Boolean, optional ByVal croppedRect As OpenCvSharp.Rect = Nothing) As OpenCvSharp.Mat
            If CropResult = CropResultEnum.AwaitingCrop OrElse (marginBase <> _processingSettings.MarginBase OrElse marginFactor <> _processingSettings.MarginFactor OrElse bgThreshold <> _processingSettings.Threshold OrElse  erosionDilation <> _processingSettings.ErosionDilation) Then ' Do the calculations.
                If bgImg Is Nothing Then 'No background, cannot crop.
                    CropResult = CropResultEnum.BackgroundNeededButNotFound
                Else
                    SyncLock _imageStream
                        Dim img = ImageMat
                        If Not IsNothing(croppedRect) AndAlso Not croppedRect = Rect.FromLTRB(0,0,0,0) Then
                            bgImg = New Mat(bgImg, croppedRect)
                        End If
                        Dim unsortedCountours = ImageUtil.DetectObjects(img, bgImg, bgThreshold, erosionDilation)
                        Dim objContours = unsortedCountours.OrderByDescending(Function(cnt As Point()) Cv2.ContourArea(cnt)).ToList()

                        If objContours IsNot Nothing AndAlso objContours.Count > 0 Then
                            Dim objCntr = objContours(0)
                            Dim rect = Cv2.BoundingRect(objCntr)
                            Dim largeRect = ImageUtil.CalculateLargeBoundingBox(rect, marginBase, marginFactor, img.Cols,img.Rows, IMG_STEP_SIZE, IMG_STEP_SIZE)

                            Dim cropRectangle As Rect
                            If extendObjectDetection Then
                                Dim extObjBB As Rect
                                Dim extLargeBB As Rect
                                Dim extObjContours As List(Of Point()) = Nothing
                                ImageUtil.ExtendDetectedObject(objContours, rect, largerect, marginBase, marginFactor, img.Cols,img.Rows, IMG_STEP_SIZE, IMG_STEP_SIZE, extObjBB, extLargeBB, extObjContours) 
                                cropRectangle = New Rect(extLargeBB.X, extLargeBB.Y, extLargeBB.Width, extLargeBB.Height)
                            Else
                                cropRectangle = New Rect(largeRect.X, largeRect.Y, largeRect.Width, largeRect.Height)
                            End If

                            CropResult           = CropResultEnum.CropOK
                            _processingSettings.MarginBase      = marginBase
                            _processingSettings.MarginFactor    = marginFactor
                            _processingSettings.Threshold       = bgThreshold
                            _processingSettings.ErosionDilation = erosionDilation
                            _croppedImage = ImageUtil.CropEnhanceImage(img, cropRectangle, brightFieldCorrection, bgImg, bgMean)
                        Else
                            CropResult = CropResultEnum.NoBlobFound
                        End If
                    End SyncLock
                End If
            End If
            If CropResult = CropResultEnum.CropOK Then ' Return detected rect.
                Return _croppedImage
            Else If CropResult = CropResultEnum.NoBlobFound OrElse CropResult = CropResultEnum.BackgroundNeededButNotFound OrElse CropResult = CropResultEnum.BackgroundWrong Then 'Return complete image
                Return ImageMat
            Else
                Throw New Exception(String.Format("Error cropping image: '{0}'", CropResult))
            End If
        End Function

        Public Sub CalculateParticleData()
            Dim bgImg = _cytoSettings.iif.OpenCvBackground
            Dim fittedBg As Mat
            If IsCropped Then
                fittedBg = bgImg(CropRect)
            Else
                fittedBg = bgImg
            End If
            Dim unsortedCountours = ImageUtil.DetectObjects(ImageMat, fittedBg, _processingSettings.Threshold, _processingSettings.ErosionDilation)
            Dim objContours = unsortedCountours.OrderByDescending(Function(cnt As OpenCvSharp.Point()) Cv2.ContourArea(cnt)).ToList()

            If objContours.Count > 0 Then
                Dim contour = objContours(0)
                Dim muPerPixel = _cytoSettings.iif.ImageScaleMuPerPixelP
                Dim data As new ParticleDataStruct With
                {
                    .hasData = true 'We got here, so that means we have a valid contour to get data from
                }
                Dim bbox As Rect = Cv2.BoundingRect(contour)
                Dim isolatedContour As Mat = ImageMat(bbox) 'make a cropped version to calculate the values on. We want the values from just this particle.

                Dim m As Moments = Cv2.Moments(isolatedContour)
			    Dim p As Point = New Point(m.M10 / m.M00, m.M01 / m.M00)
			    data.x = CropRect.X + bbox.X + p.X
			    data.y = CropRect.Y + bbox.Y + p.Y

                data.width = bbox.Width * muPerPixel
                data.height = bbox.Height * muPerPixel
                data.area = Cv2.ContourArea(contour) * (muPerPixel * muPerPixel)
                data.sharpnessScore = CalculateSharpnessSobel(isolatedContour)
			
                _imageParticleData = data
            Else 'No particle or contour to calculate data over
                _imageParticleData = New ParticleDataStruct() 'HasData = false
            End If
            
        End Sub

        Private Function CalculateSharpnessSobel(img As Mat) As Double
            'Aply Sobel
            Dim gx As New Mat()
            Dim gy As New Mat()
            Cv2.Sobel(img, gx, MatType.CV_32F, 1, 0)
            Cv2.Sobel(img, gy, MatType.CV_32F, 0, 1)

            'Normalize results
            Dim normGx As Double = Cv2.Norm(gx)
            Dim normGy As Double = Cv2.Norm(gy)

            Dim sumSq = normGx * normGx + normGy * normGy

            'Calculate average per pixel
            Dim result As Double = sumSq / (img.Width * img.Height)
            return result
        End Function
    End Class
End Namespace