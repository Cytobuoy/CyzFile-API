Imports System.Drawing
Imports System.Runtime.Serialization

Namespace DSP

    <Serializable()> Public Class DSPParticle
        Implements IDeserializationCallback
        ' RVDH - save memory by setting capacity to length....
        Public Sub OnDeserialization(sender As Object) Implements IDeserializationCallback.OnDeserialization
            If _imageStream IsNot Nothing Then
                _imageStream.Capacity = CInt(_imageStream.Length)
            End If
        End Sub


        Protected Friend Test As Integer
        Protected Friend _PartID As Integer
        Private _ImageID As Integer
        Private _BlockID As Integer

        Private _ChannelData As Byte()


        Private _Image As Imaging.CyzFileBitmap ' was a system.draming.image, we override with our own type .Image
        Private _imageStream As Serializing.CytoMemoryStream
		Private _cropRect As OpenCvSharp.Rect
        <Runtime.Serialization.OptionalField()> Private _ProcessedImage As Imaging.CyzFileBitmap ' Image ' Provides place for a preprocessed particle image

        Private MatchSucces As Boolean 'moet eigenlijk DataMatchSucces worden
        Private _ImageMatchSucces As Boolean
        Private _matchChannelData As Byte()
        Private _ParResults() As Double


        Private _continousBufferCheckedTo As Integer



        Public Sub New(ByVal ParticleID As Integer, ByVal BlockID As Integer, ByVal ImageID As Integer, ByVal ParResults As Double(), ByVal ChannelData As Byte(), ByVal ImageStream As Serializing.CytoMemoryStream, ByVal cropRect As OpenCvSharp.Rect)
            _PartID = ParticleID
            _ImageID = ImageID
            _ParResults = ParResults
            _ChannelData = ChannelData
            _imageStream = ImageStream
			_cropRect = cropRect
            _BlockID = BlockID
            _ImageMatchSucces = True
            MatchSucces = True
        End Sub


        ''' <summary>The unseperated sample data of this particle</summary>
        ''' <remarks> -ChannelData is ontdaan van syncscheiders
        '''-het eerste sample in _channelData is van kanaal Trigger1, daarna het 130, 132 enz kanaal
        '''-het laatste sample is de sample vóór het Trigger1 kanaal  
        ''' </remarks>
        Public ReadOnly Property ChannelData() As Byte()
            Get
                Return _ChannelData
            End Get
        End Property

        ''' <summary>
        ''' The image matched by this particle
        ''' </summary>
        ''' <remarks>May be empty if corresponding image was not found, because of camera problem!
        ''' In case of sorter, also empty </remarks>
        Public ReadOnly Property ImageStream As CytoSense.Serializing.CytoMemoryStream
            Get
                Return _imageStream
            End Get
        End Property

		Public ReadOnly Property CropRect As OpenCvSharp.Rect
			Get
				Return _cropRect
			End Get
		End Property

        ''' <summary>
        ''' Solves memory issues with Image
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveImages_PlaceStreams()
            If Not Object.Equals(Nothing, _Image) And Object.Equals(Nothing, _imageStream) Then
                ' Dim ms As New CytoSense.Serializing.CytoMemoryStream
                ' _Image.Save(ms, Drawing.Imaging.ImageFormat.Jpeg)
                _imageStream = New CytoSense.Serializing.CytoMemoryStream(_Image.Data)
                _Image = Nothing
            End If
        End Sub


        ''' <summary>
        ''' Flag whether the matching of data and dsp output is OK, and a picture was matched
        ''' </summary>
        ''' <remarks>-Only datamatched particles are saved, but particles without a ImageMatch are also saved
        ''' -This does now  say  if the corresponding image data was found...</remarks>
        Public ReadOnly Property Succes() As Boolean
            Get
                Return _ImageMatchSucces
            End Get
        End Property


        ''' <summary>
        ''' Only used in CytoUSB to check whether the matching of data from the dsp with the buffert was a succes
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Does not saty anything about matched pictures</remarks>
        Public ReadOnly Property DataMatchSucces() As Boolean
            Get
                Return MatchSucces
            End Get
        End Property


        ''' <summary>
        ''' Contains the framenumber, as specified by DSP. The corresponding framenumber from the pixelink camera needs to be found. 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ImageID() As Integer
            Get
                Return _ImageID
            End Get
        End Property


        Public ReadOnly Property BlockID() As Integer
            Get
                Return _BlockID
            End Get
        End Property
        Public ReadOnly Property ParID() As Integer
            Get
                Return _PartID
            End Get
        End Property


    End Class


    Class SyncOutOfRangeException
        Inherits Exception


        Public Sub New()
            MyBase.New("The sync was not in BufferT")
        End Sub 'New

    End Class
End Namespace
