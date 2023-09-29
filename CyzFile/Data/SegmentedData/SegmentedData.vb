
Namespace Data.SegmentedData
    ''' <summary>
    ''' A segmented datafile is build up as follows:
    ''' [Header segment]
    ''' [Datafile structure]
    ''' [Particle Segment 1]
    ''' [Particle Segment 2]
    '''     .
    '''     .
    '''     .
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum DataSegmentType
        Header = 0
        DataFileInfo = 1
        AllParticleData = 2
        SmartTriggerData = 3
        IIFParticleData = 4
    End Enum

    ' Not a pretty place to put the max segment sizes, I would like to make more code independent of
    ' these numbers and remove them completely, but for now it seems to need it in several places
    ' so I put here, better place then the big data processor.
    <Serializable()> Public MustInherit Class DataSegment
        Public Const MAX_PARTICLES_IN_DATAFILE As Integer = 1250000 ' With the small segments we do not have room for more in the header for keeping track of all the segments.
        Public Const MAX_PARTICLE_SEGMENT_SIZE As Integer = 1000    ' Use small segments, they load a lot faster. (only the file size is limited now to about 1.250.000
        Public Const MAX_IMAGE_SEGMENT_SIZE As Integer    = 100     ' Use more smaller segments to reduce memory usage. 

        Private _segmentType As DataSegmentType
        Private _version As New Serializing.VersionTrackableClass(New Date(2013, 4, 7))
        Public Sub New(t As DataSegmentType)
            _segmentType = t
        End Sub

        Public Property SegmentType As DataSegmentType
            Get
                Return _segmentType
            End Get
            Set(value As DataSegmentType)
                _segmentType = value
            End Set
        End Property

        Public ReadOnly Property Version As Serializing.VersionTrackableClass
            Get
                Return _version
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _segmentType.ToString
        End Function

    End Class

    <Serializable()> Public Class ParticleDataSegment
        Inherits DataSegment
        Private _particles As New List(Of RawParticle)

        Public Sub New(t As DataSegmentType)
            MyBase.New(t)
        End Sub
        Public Sub add(p As List(Of RawParticle))
            _particles.AddRange(p)
        End Sub

        Public Property Particles As List(Of RawParticle)
            Get
                Return _particles
            End Get
            Set(value As List(Of RawParticle))
                _particles = value
            End Set
        End Property

        ''' <summary>
        ''' Deprecated function for backwards compatibility for CC3
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getMultiplexedBytes() As List(Of Byte)
            Dim b As New List(Of Byte)
            For i = 0 To _particles.Count - 1
                b.AddRange(_particles(i).getMultiplexedBytes)
            Next
            Return b
        End Function

    End Class

    <Serializable()> Public Class SegmentedDataFileHeader
        Inherits DataSegment
        Dim _df_info As SegmentInfo
        Dim _segments As List(Of SegmentInfo)

        Public Sub New()
            MyBase.New(CytoSense.Data.SegmentedData.DataSegmentType.Header)
            _df_info = New SegmentInfo(0,0, DataSegmentType.DataFileInfo)
            _segments = New List(Of SegmentInfo)()
        End Sub

        Public Sub New(df_info As SegmentInfo, Segments As List(Of SegmentInfo))
            MyBase.New(CytoSense.Data.SegmentedData.DataSegmentType.Header)
            _df_info = df_info
            _segments = Segments
        End Sub

        ''' <summary>
        ''' A special segment for the datafile info, which should always be there
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property df_info As SegmentInfo
            Get
                Return _df_info
            End Get
            Set(value As SegmentInfo)
                _df_info = value
            End Set
        End Property

        ''' <summary>
        ''' Count the number of segments of a specific type.
        ''' </summary>
        ''' <param name="tp">The type of the segment we want to count.</param>
        ''' <returns>The number of segments of the requested type.</returns>
        Public Function NumberOfSegmentsOfType( tp As DataSegmentType) As Integer
            Return Enumerable.Count(_segments, Function(segInfo) segInfo.Type = tp)
        End Function


        ''' <summary>
        ''' Return information for the n'th segment of the requested type.  The function returns
        ''' Nothing if we run out of segments.
        ''' </summary>
        ''' <param name="tp">The type of segments we are interested in.</param>
        ''' <param name="idx">The number of the segment we want.</param>
        ''' <returns></returns>
        Public Function GetSegmentInfoOfType(tp As DataSegmentType, idx As Integer) As SegmentInfo
            For Each si In _segments
                If si.Type = tp Then
                    If idx = 0 Then 'Found it
                        Return si
                    Else 'Not the one we want yet, decrement counter
                        idx -= 1
                    End If
                End If
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Contains a list of the normal data segments
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Segments As List(Of SegmentInfo)
            Get
                Return _segments
            End Get
        End Property

        ''' <summary>
        ''' Checks all segments if 
        ''' </summary>
        ''' <param name="t"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function hasType(t As SegmentedData.DataSegmentType) As Boolean
            For i = 0 To _segments.Count - 1
                If _segments(i).Type = t Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Overrides Function ToString() As String
            Return "SegmentedDataFileHeader, nSegments: " & _segments.Count
        End Function

    End Class

    <Serializable()> Public Class SegmentInfo
        Dim _offset As Int64
        Dim _count As Int64
        Dim _type As DataSegmentType

        Public Sub New(offset As Int64, count As Int64, type As DataSegmentType)
            _count = count
            _offset = offset
            _type = type
        End Sub

        ''' <summary>
        ''' Offset nBytes in the total datafile from 0
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Offset As Int64
            Get
                Return _offset
            End Get
        End Property

        ''' <summary>
        ''' Byte size of the segment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Int64
            Get
                Return _count
            End Get
        End Property

        ''' <summary>
        ''' Type of the segment
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Type As DataSegmentType
            Get
                Return _type
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _type.ToString
        End Function

    End Class

End Namespace