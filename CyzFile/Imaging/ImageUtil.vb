Imports System.IO
Imports OpenCvSharp


''' <summary>
''' The function in this ImageUtil class are COPIED from an internal image DLL.  I did not want to create a dependency on that
''' DLL for this module since I am in the process of making it publicly available.  Given it's purpose, I could have also
''' removed the functionality that used this imaging, but that would have required a substantial rewrite of parts of the code.
''' So for now I choose this non optimal solution, other better solutions were not feasible given the available time frame.
''' 
''' The whole image processing/automatic cropping does not really belong in a DLL for loading data from a file, but it
''' has historically grown, and now it is here.  And removing is to much work at the moment.
''' </summary>
Public Class ImageUtil

        ''' <summary>
        ''' Load an OpenCV image array from the specified memory stream. 
        ''' </summary>
        ''' <param name="imgStream">MemoryStream containing the image. Unfortunately given the current implementation
        ''' I cannot support any stream, it has to be a memory buffer. But that is what we have at the moment
        ''' so it does not really matter.</param>
        ''' <returns>The loaded image.</returns>
        ''' <remarks>The function access the internal buffer of the imgStream, and then wraps an OpenCV Mat class
        ''' around it, and passes that to openCV function for decoding. The openCV load function also handles the conversion
        ''' to gray scale, if the image was stored as something else.
        ''' 
        ''' NOTE: It looks like it depends on how the stream was created if we can access the internal buffer or not.
        ''' And for some older files it is not possible to actually access the internal data buffer.  This results in
        ''' an unauthorized access violation.  In that case there is a special ToArray function that will do
        ''' the trick.  So we use the TryGetBuffer and see if it works
        ''' </remarks>
        Public Shared Function LoadOpenCvImage(imgStream As MemoryStream ) As Mat 
            Dim buffer As Byte()
            Dim tmpBuf As ArraySegment(Of Byte) = Nothing

            If  imgStream.TryGetBuffer(tmpBuf) Then
                buffer = tmpBuf.Array
            Else ' Old datafile, cannot access array, this creates a copy unfortunately.
                buffer = imgStream.ToArray()
            End If
            return Cv2.ImDecode(buffer, ImreadModes.Grayscale)
        End Function





        ''' <summary>
        ''' A similar function to FindObjects, but instead of returning the bounding box of the largest object
        ''' we return the list of all detected contours.  To get the object for a simple detection, just search for the
        ''' one with the biggest area.  If you want to display more, just sort the whole list, and 
        ''' then you have the biggest for a bounding box, or contour, and all the other contours as well.
        ''' </summary>
        ''' <param name="img"></param>
        ''' <param name="bgImg"></param>
        ''' <param name="diffThreshold"></param>
        ''' <param name="erosionDilation"></param>
        ''' <returns>A list of all detected contours, or an empty list if none were found.</returns>
        Public Shared Function DetectObjects(img As Mat, bgImg As Mat,diffThreshold As Integer, erosionDilation As Integer) As Point()() 

            Dim diffImg = New Mat(img.Size(), img.Type())
            Cv2.Absdiff(img, bgImg, diffImg)
            Dim threshImg = New Mat(img.Size(), img.Type())
            Cv2.Threshold(diffImg, threshImg, diffThreshold, 255, ThresholdTypes.Binary)
            Dim countourImg As Mat = threshImg
            Dim erodeImg As Mat = Nothing
            Dim dilateImg As Mat = Nothing

            If erosionDilation > 0 Then
                erodeImg = New Mat(img.Size(), img.Type())
                Cv2.Erode(threshImg, erodeImg, Nothing, Nothing, erosionDilation)
                dilateImg = New Mat(img.Size(), img.Type())
                Cv2.Dilate(erodeImg, dilateImg, Nothing, Nothing, erosionDilation)
                countourImg = dilateImg
            End If ' else no erosion dilation just use the output from the threshold image.

            Dim cntrs = Cv2.FindContoursAsArray(countourImg, RetrievalModes.External, ContourApproximationModes.ApproxSimple)
            Dispose(diffImg)
            Dispose(threshImg)
            Dispose(erodeImg)
            Dispose(dilateImg)
            Return cntrs
        End Function


        ''' <summary>
        ''' Extend the bounding box bb, with a margin base number of pixels and a factor.  This way we get something of the surrounding of the particle
        ''' as well.  Max x and Y limit the size so we do not go outside the picture, and the stepX (and Y) indicate we only want sizes that are 
        ''' a multiple of these values.  This will make handling the images e.g. for generating an overview easier to handle as we have fewer different
        ''' sizes.
        ''' </summary>
        ''' <param name="bb"></param>
        ''' <param name="marginBase"></param>
        ''' <param name="marginFactor"></param>
        ''' <param name="maxX"></param>
        ''' <param name="maxY"></param>
        ''' <param name="stepX"></param>
        ''' <param name="stepY"></param>
        ''' <returns></returns>
        Public Shared Function CalculateLargeBoundingBox(bb As Rect, marginBase As Integer, marginFactor As Double, maxX As Integer, maxY As Integer, stepX As Integer, stepY As Integer) As Rect 
            Debug.Assert(bb.X >= 0 AndAlso bb.X < maxX AndAlso bb.Width > 0   AndAlso bb.X + bb.Width <= maxX)
            Debug.Assert(bb.Y >= 0 AndAlso bb.Y < maxY AndAlso bb.Height > 0  AndAlso bb.Y + bb.Height <= maxY)
            Dim xMargin As Integer = marginBase + CInt(bb.Width * marginFactor)
            Dim yMargin As Integer = marginBase + CInt(bb.Height * marginFactor)

            Dim top As Integer    = Math.Max(0, bb.Y - yMargin)
            Dim left As Integer   = Math.Max(0, bb.X - xMargin)
            Dim width As Integer  = Math.Min(bb.Width + 2 * xMargin, maxX - left)
            Dim height As Integer = Math.Min(bb.Height + 2 * yMargin, maxY - top)

            Dim newWidth As Integer = (CInt(width / stepX) + 1) * stepX
            Dim newHeight As Integer = (CInt(height / stepY) + 1) * stepY
            If newWidth < stepX Then
                newWidth = stepX
            End If
            If newHeight < stepY Then
                newHeight = stepY
            End If
            If newWidth > maxX Then
                newWidth = maxX
            End If
            If newHeight > maxY Then
                newHeight = maxY
            End If

            Dim halfWDiff As Integer = CInt((newWidth - width) / 2)
            Dim halfHDiff As Integer = CInt((newHeight - height) / 2)
            left = Math.Max(0, left - halfWDiff)
            top  = Math.Max(0, top - halfHDiff)
            newWidth  = Math.Min(newWidth, maxX - left)
            newHeight = Math.Min(newHeight, maxY - top)
            Dim r As Rect = new Rect(left, top, newWidth, newHeight)

            Debug.Assert(r.X >= 0 AndAlso r.X < maxX AndAlso r.Width > 0 AndAlso r.X + r.Width <= maxX)
            Debug.Assert(r.Y >= 0 AndAlso r.Y < maxY AndAlso r.Height > 0 AndAlso r.Y + r.Height <= maxY)

            return r
        End Function




        ''' <summary>
        ''' Basic object detection is done by looking for a single object (at the time of writing this
        ''' comment this simply means selecting the largest object). This function extends that detected object by
        ''' looking for other possible objects that are in the objects bounding box. (The large one is the one where
        ''' we added some margins).  THe marginBase/factor is also passed in because if we add another object we need
        ''' to extend the bounding box and the margin.)
        ''' In the end we return the extended bounding boxes, and a list of contours for all contours that
        ''' are in the newly extended bounding box.
        ''' </summary>
        ''' <param name="objectContours"></param>
        ''' <param name="objBB"></param>
        ''' <param name="largeBB"></param>
        ''' <param name="marginBase"></param>
        ''' <param name="marginFactor"></param>
        ''' <param name="extObjBB"></param>
        ''' <param name="extLargeBB"></param>
        ''' <param name="detectedObjects"></param>
        ''' <remarks>NOTE: Will also contain the original object in detected objects.  If the possible objects
        ''' is sorted on size, then the original object will also be the first in the detected objects.
        ''' </remarks>
        Public Shared Sub ExtendDetectedObject(
            objectContours As List(Of Point()),
            objBB As Rect,
            largeBB As Rect,
            marginBase As Integer,
            marginFactor As Double,
            maxX As Integer,
            maxY As Integer,
            stepX As Integer,
            stepY As Integer,
            ByRef extObjBB As Rect,
            ByRef extLargeBB As Rect,
            ByRef detectedContours As List(Of Point()))

        
            extObjBB          = new Rect(objBB.Location, objBB.Size)
            extLargeBB       = new Rect(largeBB.Location, largeBB.Size)
            detectedContours = new List( Of Point())

            Dim haveContour(objectContours.Count-1) As Boolean
            Dim keepLooking As Boolean = true
            While keepLooking
                keepLooking = False  '  Set to true whenever we add a new object to to our list, so we have them all.

                For i As Integer = 0 To objectContours.Count -1
                    If haveContour(i) Then
                        Continue For
                    End If

                    Dim bb = Cv2.BoundingRect(objectContours(i))
                    If extObjBB.Contains(bb) Then
                        ' // No need to extend bb, but we need to add it to the detected list.
                        detectedContours.Add(objectContours(i))
                        haveContour(i) = true
                    Else If extLargeBB.IntersectsWith(bb) Then
                        '  Object is inside our detection range, add it to the list and extend the bounding box. And we need to go through the list again.
                        keepLooking    = True
                        haveContour(i) = True
                        extObjBB = extObjBB.Union(bb)
                        extLargeBB = CalculateLargeBoundingBox(extObjBB, marginBase, marginFactor, maxX, maxY, stepX, stepY)
                        detectedContours.Add(objectContours(i))
                    End If ' Else no overlap, so just continue checking the next contour
                Next
            
            End While
            
        End Sub


        ''' <summary>
        ''' The function crops the image to the specified size and runs any image enhancement algorithms that are specified.
        ''' Currently the only enhancement supported is doing a BrightFieldCorrection using the background image and the
        ''' the mean background level:
        '''                           srcImage
        '''     result  =  bgMean * ------------
        '''                           bgImage
        ''' This corrects for any irregularities in lighting or pixel sensitivity.
        ''' In the future I expect there will be more options, but until then we can keep this relatively simple interface.
        ''' 
        ''' I combined the cropping and the processing because it allows me to process only the region that we need to
        ''' return.  I am not sure this is the best option, but it currently seems like a good idea, and we always need
        ''' these 2 together.
        ''' </summary>
        ''' <param name="src">The original complete unprocessed image.</param>
        ''' <param name="cropRegion">The Region of interest, i</param>
        ''' <param name="applyBrightFieldCorrection">Set to true if you want bright field correction, false if not.</param>
        ''' <param name="bgMat">The complete (unprocessed) background image (if bright field correction is false, then this
        ''' parameter can be Nothing.</param>
        ''' <param name="bgMean">The average value of the background.  If brightFieldCorrection is false, then the value
        ''' of this parameter is not important.</param>
        ''' <returns>A cropped image with all the image processing applied.</returns>
        ''' <remarks>Maybe remove cropping, if you crop, caller will have to do that for both fore and background
        ''' and we simply only do the bright field correction.  Probably Nicer.</remarks>
        Public Shared Function CropEnhanceImage( src As Mat, cropRegion As rect, applyBrightFieldCorrection As Boolean, bgMat As Mat, bgMean As Double) As Mat
            if Not applyBrightFieldCorrection Then
                Return New Mat(src, cropRegion)
            End If

            Dim cropSrc As Mat = Nothing
            Dim cropBg As Mat = Nothing
            Try
                cropSrc =  New Mat(src, cropRegion)
                cropBg  = New Mat(bgMat, cropRegion)
                return ApplyFlatFieldCorrection(cropSrc, cropBg, bgMean)
            Finally ' Needed to avoid early release and cleanup of the two images while the OpenCV library is still processing.
                GC.KeepAlive(cropSrc)
                GC.KeepAlive(cropBg)
            End Try
        End Function


        Private Shared Function ApplyFlatFieldCorrection(img As Mat, bgImg As Mat, bgMean As Double) As Mat
            return ((img * bgMean) / bgImg).ToMat()
        End Function


        ''' <summary>
        ''' Explicitly dispose OpenCV image (Mat objects) as soon as we can, do not wait for the
        ''' garbage collector, as they can occupy a lot of memory.
        ''' </summary>
        ''' <param name="m"></param>
        Private Shared Sub Dispose( m As Mat)
            if m IsNot Nothing Then
                m.Dispose()
            End If
        End Sub

        ' Several image format definitions, these were copied from the windows (WPF) library, so we can use them
        ' on other platforms as well.  Not the prettiest solution, but at the moment other solutions are way more
        ' complicated then just copying these definitions.
        Public Shared ReadOnly JPEG_GUID As Guid = New Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly EMF_GUID  As Guid = New Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly BMP_GUID  As Guid = New Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly WMF_GUID  As Guid = New Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly GIF_GUID  As Guid = New Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly PNG_GUID  As Guid = New Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}")
        Public Shared ReadOnly TIFF_GUID As Guid = New Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}") 


        ''' <summary>
        ''' Prevent instantiation, it is an abstract class with only shared members, but I do not now how to define that in VB, and a module
        ''' is slightly different I think.
        ''' </summary>
        Private Sub New()

        End Sub

End Class
