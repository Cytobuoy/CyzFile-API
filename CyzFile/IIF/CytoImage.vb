Imports System.Drawing

Namespace Data.ParticleHandling

    <Serializable()> Public Class CytoImage
        Private Shared _log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        'Settings for output
        <Obsolete()> Private _ScaleBar As Boolean = True

        'Properties
        Public imageCropped As Boolean = False
        <Obsolete()> Public userDefinedCrop As Boolean = False 'not yet implemented

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
        Private _imageMat As OpenCvSharp.Mat
        Private _imageStream As System.IO.MemoryStream
        Private _croppedRect As OpenCvSharp.Rect
        Private _cytoSettings As CytoSettings.CytoSenseSetting 'is used only for background image currently

        'Particle width data
        <Obsolete()> Private _pwst As System.Drawing.Point
        <Obsolete()> Private _pwend As System.Drawing.Point


        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, im As IO.MemoryStream, Optional crpRct As OpenCvSharp.Rect = Nothing)
            _cytoSettings = cytosettings
            _imageStream = im
            _croppedRect = crpRct
            If _imageStream.Capacity > _imageStream.Length Then
                _imageStream.Capacity = Cint(_imageStream.Length)
            End If
        End Sub

        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, im As OpenCvSharp.Mat, ims As IO.MemoryStream, crpRct As OpenCvSharp.Rect)
            _cytoSettings = cytosettings
            _imageMat = im
            _imageStream = ims
            _croppedRect = crpRct
            If im.IsSubmatrix Then
                imageCropped = true
            End If
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

        Public Function GetCroppedImage(marginBase As Integer, marginFactor As Double, bgThreshold As Integer, erosionDilation As Integer) As OpenCvSharp.Mat
            Return GetCroppedImage(marginBase,marginFactor,bgThreshold,erosionDilation,True, True)
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
                SyncLock _imageStream
                    Return AutoCropOpenCV(_imageStream, _cytoSettings.iif.OpenCvBackground, _cytoSettings.iif.OpenCvBackgroundMean, marginBase, marginFactor, bgThreshold, erosionDilation, brightFieldCorrection, extendObjectDetection, _croppedRect)
                End SyncLock
            End IF
        End Function

		Public Function GetImageAsMat() As OpenCvSharp.Mat
            Return ImageUtil.LoadOpenCvImage(_imageStream)
        End Function

        Public Property ImageMat As OpenCvSharp.Mat
            Get
                Return _imageMat
            End Get
            Set(value As OpenCvSharp.Mat)
                _imageMat = value
            End Set
        End Property

        Public Property ImageStream As System.IO.MemoryStream
            Get
                Return _imageStream
            End Get
            Set(ByVal value As System.IO.MemoryStream)
                _imageStream = value
            End Set
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

        Public ReadOnly Property isProcessed As Boolean
            Get
                Return CropResult <> CropResultEnum.AwaitingCrop
            End Get
        End Property


        <NonSerialized()> Private _cropRectangle As Rectangle

        ' Settings used for the previous cropping, if the settings change we need to crop again, else we can
        ' reuse the previous crop rectangle.
        <NonSerialized()> Private _cropMarginBase      As Integer
        <NonSerialized()> Private _cropMarginFactor    As Double
        <NonSerialized()> Private _cropBgThreshold     As Integer
        <NonSerialized()> Private _cropErosionDilation As Integer

        ' Temporary, check how much it improves performance when caching the image itself, NOTE:
        ' extended object detection flags, and bright field correction flag need to be taken into account yet.
        'They are not currently.  Could that be the performance issue>
        <NonSerialized()> Private _croppedImage As OpenCvSharp.Mat

        Public Const IMG_STEP_SIZE As Integer = 80
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
        Public Function AutoCropOpenCV(ByVal sourceStream As System.IO.MemoryStream, ByVal bgImg As OpenCvSharp.Mat, bgMean As Double, marginBase As Integer, marginFactor As Double, bgThreshold As Integer, erosionDilation As Integer, brightFieldCorrection As Boolean, extendObjectDetection As Boolean, optional ByVal croppedRect As OpenCvSharp.Rect = Nothing) As OpenCvSharp.Mat
            If CropResult = CropResultEnum.AwaitingCrop OrElse (marginBase <> _cropMarginBase OrElse marginFactor <> _cropMarginFactor OrElse bgThreshold <> _cropBgThreshold OrElse  erosionDilation <> _cropErosionDilation) Then ' Do the calculations.
                If bgImg Is Nothing Then 'No background, cannot crop.
                    CropResult = CropResultEnum.BackgroundNeededButNotFound
                Else
                    SyncLock _imageStream
                        Dim img = ImageUtil.LoadOpenCvImage(sourceStream)
                        If Not IsNothing(croppedRect) AndAlso Not croppedRect = OpenCvSharp.Rect.Empty Then
                            bgImg = New OpenCvSharp.Mat(bgImg, croppedRect)
                        End If
                        Dim unsortedCountours = ImageUtil.DetectObjects(img, bgImg, bgThreshold, erosionDilation)
                        Dim objContours = unsortedCountours.OrderByDescending(Function(cnt As OpenCvSharp.Point()) OpenCvSharp.Cv2.ContourArea(cnt)).ToList()

                        If objContours IsNot Nothing AndAlso objContours.Count > 0 Then
                            Dim objCntr = objContours(0)
                            Dim rect = OpenCvSharp.Cv2.BoundingRect(objCntr)
                            Dim largeRect = ImageUtil.CalculateLargeBoundingBox(rect, marginBase, marginFactor, img.Cols,img.Rows, IMG_STEP_SIZE, IMG_STEP_SIZE)

                            If extendObjectDetection Then
                                Dim extObjBB As OpenCVSharp.Rect
                                Dim extLargeBB As OpenCVSharp.Rect
                                Dim extObjContours As List(Of OpenCvSharp.Point()) = Nothing
                                ImageUtil.ExtendDetectedObject(objContours, rect, largerect, marginBase, marginFactor, img.Cols,img.Rows, IMG_STEP_SIZE, IMG_STEP_SIZE, extObjBB, extLargeBB, extObjContours) 
                                _cropRectangle = New Rectangle(extLargeBB.X, extLargeBB.Y, extLargeBB.Width, extLargeBB.Height)
                            Else
                                _cropRectangle = New Rectangle(largeRect.X, largeRect.Y, largeRect.Width, largeRect.Height)
                            End If

                            CropResult           = CropResultEnum.CropOK
                            _cropMarginBase      = marginBase
                            _cropMarginFactor    = marginFactor
                            _cropBgThreshold     = bgThreshold
                            _cropErosionDilation = erosionDilation
                            Dim newImage = ImageUtil.LoadOpenCvImage(sourceStream)
                            Dim cropRect = New OpenCvSharp.Rect(_cropRectangle.X,_cropRectangle.Y,_cropRectangle.Width, _cropRectangle.Height)
                            _croppedImage = ImageUtil.CropEnhanceImage(newImage, cropRect, brightFieldCorrection, bgImg, bgMean)
                        Else
                            CropResult = CropResultEnum.NoBlobFound
                        End If
                    End SyncLock
                End If
            End If
            If CropResult = CropResultEnum.CropOK Then ' Return detected rect.
                Return _croppedImage
            Else If CropResult = CropResultEnum.NoBlobFound OrElse CropResult = CropResultEnum.BackgroundNeededButNotFound OrElse CropResult = CropResultEnum.BackgroundWrong Then 'Return complete image
                Return ImageUtil.LoadOpenCvImage(sourceStream)
            Else
                Throw New Exception(String.Format("Error cropping image: '{0}'", CropResult))
            End If
        End Function
    End Class
End Namespace