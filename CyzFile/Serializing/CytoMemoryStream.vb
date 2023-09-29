Imports System.IO
Imports System.Runtime.Serialization

Namespace Serializing



    ''' <summary>
    ''' A SubClass of the memory stream, it will not really add any functionality, but it will allow us
    ''' to add serialization that will be compatible with the original serialization in the .Net framework, and
    ''' we can still use it internally in all the classes that expect a memory stream. We need to change the type
    ''' but all the other things stay the same. (We probably need to add some forwarding constructors). 
    ''' This will allow us to read and write memory stream objects in a way that is compatible to the original
    ''' .Net framework implementation, but will run on .Net core as well.
    ''' 
    ''' Besides this memory stream class we will also need a custom serialization binder object that will insert
    ''' this class whenever it sees a normal memory stream, and inserts the name of the normal memory stream whenever
    ''' it is serializing this class.
    ''' </summary>
    <Serializable>
    Public Class CytoMemoryStream
        Inherits MemoryStream
        Implements ISerializable

        Public Sub New(byteArr As Byte())
            MyBase.New(byteArr)
        End Sub

		Public Sub New()
            MyBase.New()
        End Sub


        ''' <summary>
        ''' Construct a new memory stream object from the serialization info.  We are only interested in the actual
        ''' data, not in the other state, we just construct a default MemoryStream object, with the data retrieved
        ''' from the stream.
        ''' </summary>
        ''' <param name="info"></param>
        ''' <param name="context"></param>
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New( ResizeArray(DirectCast(info.GetValue("_buffer",GetType(Byte())), Byte()),info.GetInt32("_length")))
        End Sub


        ''' <summary>
        ''' Return the object data need to be stored.  We are mapping this class to an actual MemoryStream. The MemoryStream
        ''' class does not support serializing anymore in .net 7, but to remain compatible I will store the datafile
        ''' the way a memory stream was serialized originally.  I copied all the properties that were present in deserialization
        ''' that I found in the file, not sure all are needed on deserialization.
        ''' 
        ''' _buffer of type System.Byte[]: System.Byte[]
        ''' _origin of type System.Int32: 0
        ''' _position of type System.Int32: 704935
        ''' _length of type System.Int32: 704935
        ''' _capacity of type System.Int32: 1048576
        ''' _expandable of type System.Boolean: True
        ''' _writable of type System.Boolean: True
        ''' _exposable of type System.Boolean: True
        ''' _isOpen of type System.Boolean: True
        ''' MarshalByRefObject+__identity of type System.Object: 
        '''
        ''' I am not sure about the __identity thing, apparently in my test files it is always Nothing, so I will add it like that.
        ''' 
        ''' </summary>
        ''' <param name="info"></param>
        ''' <param name="context"></param>
        Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
                If info Is Nothing Then
                throw New System.ArgumentNullException("info")
                End If

            ' NOTE we do not care about the exact state we are saving, only about the buffer, data, so we can actually
            ' just save the array, and set size, position, etc. to a reset buffer. Note
            ' GetBuffer sometimes fails, so ToArray is safer, and we cannot use the internal members, because
            ' in .net 7 they may not even exist, even if we could access them.

            Dim data As Byte() = ToArray()

            info.AddValue("_buffer",     data) 
            info.AddValue("_origin",     0)
            info.AddValue("_position",   0)
            info.AddValue("_length",     data.Length)
            info.AddValue("_capacity",   data.Length)
            info.AddValue("_expandable", False) '  Original was true but we will never change the stream(I think)
            info.AddValue("_writable",   False)  '  Original was true but we will never change the stream(I think)
            info.AddValue("_exposable",  False)
            info.AddValue("_isOpen",     True)
            info.AddValue("MarshalByRefObject+__identity", CType(Nothing, System.Object))
        End Sub

        ''' <summary>
        ''' Deserialized stream is often much larger then the actual data, so resizing saves us a lot
        ''' of memory. (Especially if you have a 1000 images or so.)
        ''' </summary>
        ''' <param name="arr"></param>
        ''' <param name="len"></param>
        ''' <returns></returns>
        Private Shared Function ResizeArray( arr As Byte(), len As Int32) As Byte()
            If arr.Length > len Then
                ReDim Preserve arr(len-1)
            End If
            Return arr
        End Function


    End Class

End Namespace
