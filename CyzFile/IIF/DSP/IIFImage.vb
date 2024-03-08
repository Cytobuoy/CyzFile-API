Imports System.Drawing

Namespace DSP
    ''' <summary>
    ''' Depreciated for use in CytoUSB and soon new datafiles. Only cannot be removed due to old previously serialized datafiles. (Legacy)
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class IIFImage
        Private ImageData As Object
        Public ID As Integer
        Public time As DateTime

        Public Property ImageStreamData As Serializing.CytoMemoryStream
            Get
                Try
                    Return CType(ImageData,Serializing.CytoMemoryStream)
                Catch e As Exception
                    ' This handles "bad" datafiles where the object is not a stream, but a System.Drawing.Bitmap.
                    ' In the process of porting to .net core, the System.Drawing.Bitmap was replaced on loading with a
                    ' CyzFileBitmap.  The code below should handle that.  But I have not been able to find any datafile
                    ' with this problem on our servers. So I have not been able to test the code. In theory it should
                    ' work. :-(
                    Dim tmp As CytoSense.Imaging.CyzFileBitmap  = CType(ImageData,CytoSense.Imaging.CyzFileBitmap)    'due to error when changing to imagestream files with both ImageData as a stream and ImageData as an image may exist. This property takes care of this.
                    Return New Serializing.CytoMemoryStream(tmp.Data, 0, tmp.Data.Length, False, True)
                    
                    ' Original code for the SYstem.Drawing.Bitmap class 
                    ' Dim tmp As Image = CType(ImageData,Image)    'due to error when changing to imagestream files with both ImageData as a stream and ImageData as an image may exist. This property takes care of this.
                    ' Dim ms As New Serializing.CytoMemoryStream
                    ' tmp.Save(ms, Drawing.Imaging.ImageFormat.Jpeg)
                    ' Return ms
                End Try
            End Get
            Set(value As Serializing.CytoMemoryStream)
                ImageData = value
            End Set
        End Property
    End Class

End Namespace