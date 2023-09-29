Imports System.IO
Imports System.Runtime.Serialization

Namespace Imaging

''' <summary>
''' Old data files actually stored a System.Drawing.Image class in the datafile. Newer files do not do this
''' but we have to support loading these files.  These classes are unfortunately windows only.  In order to target
''' net7.0 without windows, we need a solution.
''' 
''' So this class was created, when loading the data files using serialization, we replace the System.Drawing.Image
''' class With this DspImage class.  This class then loads the data.
''' 
''' The writing functions will throw an exception, as we do not support generating files with these objects anymore.
''' Only loading the data is supported.
''' 
''' The original windows bitmap (and image) serialization code, basically does a Save to a memory stream and
''' then writes the streams buffer as a byte array.  So it has one serialized property "Data" which is a byte
''' array. And to our convenience, the byte array actually contains a JFIF file.  So we can simply
''' take that byte array, and then store it in our memory stream.
''' 
''' Originally DSP images were stored this way, but that was changed.  The background image however is still
''' stored this way, so we need to support writing this class as well.  That is why you can construct
''' a class passing in the byte data. This should be an array with a JPEG stream.
''' </summary>
<Serializable>
Public Class CyzFileBitmap
Implements ISerializable

        ''' <summary>
        ''' Construct a CyzFileBitmap object, passing in a byte array with JPEG data (JFIF format).
        ''' This is only for use for background images in the camera calibration in
        ''' CytoUsb.
        ''' </summary>
        ''' <param name="jpegData"></param>
        Public Sub New( jpegData As Byte() )
            _data = jpegData
        End Sub


        Public Sub New(info As SerializationInfo, context As StreamingContext)
            _data = CType(info.GetValue("Data", GetType(Byte())),Byte())
        End Sub

        Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
                info.AddValue("Data", _data, GetType(byte())) ' Compatible with the original bitmap serialization. (Assuming the user stored a JPEG stream in the byte array.)
        End Sub

        Public ReadOnly Property Data As Byte()
            Get
                Return _data
            End Get
        End Property

        Private _data As Byte()

    End Class

End Namespace
