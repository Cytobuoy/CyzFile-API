Imports System.Runtime.Serialization

Namespace Data.SegmentedData

    ''' <summary>
    ''' A intermediate wrapper for particles, stripped down to the bear minimum of instanced fields to prevent memory 
    ''' problems, while remaining efficient in terms of CPU usage when converting back to a real particle.
    ''' This class is to be used to store particles efficiently in the segmented data format and the BigDataProcessor.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class RawParticle
        Private _chdata()() As Byte
        Private _clusterInfo As Cluster
        Private _ID As Integer
        Private _timeOfArrival As DateTime

        <NonSerialized> Public SegmentIndex As Integer
        <NonSerialized> Public SegmentParticleIndex As Integer

        Public Sub New(p As CytoSense.Data.ParticleHandling.Particle, ID As Integer, clusterInfo As Cluster, TimeOfArrival As DateTime)
            ReDim _chdata(p.ChannelData_Hardware.Length - 1)
            For i = 0 To _chdata.Length - 1
                _chdata(i) = p.ChannelData_Hardware(i).Data_Raw
            Next
            _clusterInfo = clusterInfo
            _ID = ID
            _timeOfArrival = TimeOfArrival
        End Sub

        ''' <summary>
        ''' Contains ID of the particle in the measurement
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ID As Integer
            Get
                Return _ID
            End Get
        End Property

        ''' <summary>
        ''' Necessary for finding DSP imaged particles (which don't have a proper ID yet after a measurement is finished)
        ''' </summary>
        ''' <param name="ID"></param>
        ''' <remarks></remarks>
        Public Sub reSetID(ID As Integer)
            _ID = ID
        End Sub
        Public ReadOnly Property ClusterInfo As Cluster
            Get
                Return _clusterInfo
            End Get
        End Property

        Public ReadOnly Property ChannelData As Byte()()
            Get
                Return _chdata
            End Get
        End Property
        Public ReadOnly Property nBytes As Integer
            Get
                Return _chdata.Length * _chdata(0).Length
            End Get
        End Property
        Public ReadOnly Property TimeOfArrival As DateTime
            Get
                Return _timeOfArrival
            End Get
        End Property

        ''' <summary>
        ''' Deprecated function for backwards compatibility for CC3
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getMultiplexedBytes() As List(Of Byte)
            Dim b As New List(Of Byte)
            For i = 0 To _chdata.Length - 1
                For j = 0 To _chdata(i).Length - 1
                    b.Add(_chdata(i)(j))
                Next
            Next
            Return b
        End Function

        Public Overrides Function ToString() As String
            Return "FJID: " & _ID
        End Function



        Public Function TimeOfArrivalRelative(start As DateTime) As Single
            Return CSng((_timeOfArrival - start).TotalMilliseconds / 1000.0)
        End Function


    End Class

    ''' <summary>
    ''' The same as above, but now with an image attached. TODO: extend with processImagedStream
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class RawIIFParticle
        Inherits RawParticle
        Implements IDeserializationCallback

        Public Enum ImageMatchResult
            Success
            NoMatch
            MultipleMatches
        End Enum

        <NonSerialized> Public MatchResult As ImageMatchResult
        <NonSerialized> Public SplittedParticleIndex As Integer = -1

        ' RVDH - save memory by setting capacity to length....
        Public Sub OnDeserialization(sender As Object) Implements IDeserializationCallback.OnDeserialization
            If _ImageStream IsNot Nothing Then
                _ImageStream.Capacity = CInt(_ImageStream.Length)
            End If
        End Sub


        Public Property ImageStream As Serializing.CytoMemoryStream

        Public Sub New(p As CytoSense.Data.ParticleHandling.ImagedParticle, clusterInfo As Cluster, TimeOfArrival As DateTime)
            MyBase.New(p, p.ID, clusterInfo, TimeOfArrival)
            _ImageStream = p.ImageHandling.ImageStream
        End Sub

    End Class

End Namespace
