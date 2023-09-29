Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Data.SegmentedData

Namespace Data

''' <summary>
''' Very simple interface for a source of particles, it works like an iterator.  You can 
''' call GetNext until you reach the end.  
''' </summary>
Public Interface IParticleProvider
    ''' <summary>
    ''' Get the next particle.  The function returns true if it did, and false if no more
    ''' particles were available.
    ''' </summary>
    ''' <param name="p">Set to the next particle if one was available. Set to nothing
    ''' if no new particle was available.</param>
    ''' <returns>True if a particle was available, false if not.</returns>
    Function GetNext( ByRef p As Particle ) As Boolean
    ''' <summary>
    ''' Check if the iterator is done yet.  If it returns false, no more particles are available. 
    ''' If it returns true then more particles MAY be available.  It has not reached the end yet,
    ''' but that may happen in the next call and in that case GetNext will return false and no particle.
    ''' SO if Done returns false there is no guarantee that GetNext will still return a particle.
    ''' </summary>
    ''' <returns></returns>
    Function Done() As Boolean
End Interface


''' <summary>
''' This class wraps around a datafile wrapper, and works like an iterator.  It allows you to
''' retrieve one particle at a time, and for newer segmented data files it will only load
''' 1 or 2 segments in memory at one time, and not all off them.
''' 
''' In C++ terms it is a forward iterator only.  You create it and you go through all the particles
''' until the end, and then you are done, you cannot reset it.  If you need to restart, you need to
''' create a new particle provider.
''' </summary>
''' <remarks>It is based on 3 original classes, a ParticleProvider, an unfiltered particle provider
''' and a Raw Particle Provider.
''' 
''' The original also provided backwards compatibility options for CC3, but I am dropping that now.
''' Currently there is support for older unsegmented data files, and segmented data files, nothing else.
''' 
''' Filtering is no longer handled in the particle provider, instead it is done elsewhere.
''' 
''' THere are basically 2 possibilities, a segmented file, and a unsegmented.  The unsegemented is
''' very simple.
''' </remarks>
Public MustInherit Class ParticleProvider
        Implements IParticleProvider

        Public Shared Function Create( dfw As DataFileWrapper ) As IParticleProvider
            If dfw.MeasurementSettings.SegmentedDatafile Then
                If dfw.MeasurementSettings.IIFCheck Then
                    If dfw.CytoSettings.PIC.PICIIF Then
                        Return New SegmentedIifParticleProvider(dfw)
                    Else ' With DSP, the optimized loading of imaged particles is not possible, so
                         ' we simply load the whole file as if it was an unsegmented datafile.
                        Return New UnsegmentedParticleProvider(dfw)
                    End If
                Else
                    Return New SegmentedNoIifParticleProvider(dfw)
                End If
            Else
                Return New UnsegmentedParticleProvider(dfw)
            End If
        End Function

        Public Function GetNext( ByRef p As Particle) As Boolean Implements IParticleProvider.GetNext
            If _done Then
                p = Nothing
                Return False
            End If

            Return GetNextImpl(p)
        End Function

        Public Function Done() As Boolean Implements IParticleProvider.Done
            Return _done
        End Function


        Protected Sub New(dfw As DataFileWrapper)
            _dfw  = dfw
            _done = false
        End Sub


        Protected MustOverride Function GetNextImpl( ByRef p As Particle) As Boolean

        Protected ReadOnly _dfw As DataFileWrapper
        Protected _done As Boolean ' We reached the end
End Class

''' <summary>
''' Implementation of a particle provider for unsegmented files.
''' </summary>
Public Class UnsegmentedParticleProvider
        Inherits ParticleProvider

    Public Sub New(dfw As DataFileWrapper)
        MyBase.New(dfw)
        _nextParticleIdx = 0
        _numParticles = _dfw.SplittedParticles.Length
    End Sub

    Protected Overrides Function GetNextImpl( ByRef p As Particle) As Boolean
        If _nextParticleIdx >= _numParticles Then
            _done = True
            p = Nothing
            Return False
        End If
        p = _dfw.SplittedParticles(_nextParticleIdx)
        _nextParticleIdx += 1
        Return True
    End Function

    Private ReadOnly _numParticles As Integer
    Private _nextParticleIdx As Integer
End Class


''' <summary>
''' Implementation of a particle provider for Segmented returning particles and images datafiles.
''' </summary>
''' <remarks>This is the tricky one, it is based mainly on the raw particle provider, except that
''' we return complete Particles, including images.
''' NOTE: We do not reuse the NoIif particle provider, not pretty, but just copying and adding the
''' code is quicker, if we need to do more to this class we could look at if we can partially merge
''' the 2 implementations.  For now it will do. 
''' </remarks>
Public Class SegmentedIifParticleProvider
        Inherits ParticleProvider

    Public Sub New(dfw As DataFileWrapper)
        MyBase.New(dfw)
        _filePath = dfw.rawDataFile.path ' Path variable is set on loading to contain the full path.
        _dataSegmentType = If( dfw.rawDataFile.MeasurementSettings.SmartTriggeringEnabled, DataSegmentType.SmartTriggerData, DataSegmentType.AllParticleData)

        Dim dataSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType(_dataSegmentType, 0)
        If dataSegInfo Is Nothing Then 'Reached end of stream, no particles in the datafile.
            _currentDataSegment = Nothing
            _currentImageSegment = Nothing
            _done = True
            Return
        End If

        _currentDataSegmentIdx = 0
        _nextDataParticleIdx   = 0
        _currentDataSegment    = DataFunctions.loadDatafileSegment( _filePath, dataSegInfo)

        If _dfw.MeasurementSettings.IIFCheck Then
            Dim imgSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType( DataSegmentType.IIFParticleData , 0)
            If imgSegInfo Is Nothing Then 'Reached end image segments (in this case no image segments). Not eof yet.
                _currentImageSegment    = Nothing
            Else
                Dim tmpImgData          = DataFunctions.loadDatafileSegment( _filePath, imgSegInfo)
                _currentImageSegmentIdx = 0
                _nextImagedParticleIdx  = 0
                _currentImageSegment    = tmpImgData
            End If
        End If

    End Sub

    'A bit simpler because we do not do the filtering here.  We just get the next one
    Protected Overrides Function GetNextImpl( ByRef p As Particle) As Boolean
        p = Nothing
        LoadNextSegmentsIfRequired()
        If _done Then
            Return False
        End If

        Dim rawPart = _currentDataSegment.Particles( _nextDataParticleIdx )

        If _currentImageSegment IsNot Nothing Then
            If _currentImageSegment.Particles(_nextImagedParticleIdx).ID = rawPart.ID Then
                Dim imgRawPart = DirectCast(_currentImageSegment.Particles(_nextImagedParticleIdx), RawIIFParticle)
                p = New ImagedParticle(imgRawPart, _dfw.CytoSettings, _dfw.MeasurementSettings, _dfw.MeasurementInfo.MeasurementStart)
            Else
                p = New  Particle(rawPart,_dfw.CytoSettings,_dfw.MeasurementSettings, _dfw.MeasurementInfo.MeasurementStart)
            End If ' Else IDs did not match.
            If _currentImageSegment.Particles(_nextImagedParticleIdx).ID <= rawPart.ID Then ' This refers to old or current particle, increment index
                _nextImagedParticleIdx += 1
            End If 'Else next image is for a particle in the future.
        Else 'No images (left) so just return the particle.
            p = New  Particle(rawPart,_dfw.CytoSettings,_dfw.MeasurementSettings, _dfw.MeasurementInfo.MeasurementStart)
        End If
        _nextDataParticleIdx += 1
        Return True ' If we get here, we must have found something that matches.
    End Function

    ''' <summary>
    ''' Check if the nextIdx for parts and images is still in the currently loaded segment, if not
    ''' load the next segment and reset the index. (Checks both the particle and image indexes).
    ''' If we reach the end of the data segments, then eof is set.  If we reach the end of the image
    ''' index, this is not an eof.
    ''' </summary>
    Private Sub LoadNextSegmentsIfRequired()
        If _done Then
            Return 'Nothing to do.
        End If
        If _nextDataParticleIdx >= _currentDataSegment.Particles.Count Then
            Dim dataSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType(_dataSegmentType,_currentDataSegmentIdx+1)
            If dataSegInfo Is Nothing Then 'Reached end of stream.
                _currentDataSegment = Nothing
                _currentImageSegment = Nothing
                _done = True
                Return
            End If

            Dim tmpData = DataFunctions.loadDatafileSegment( _filePath, dataSegInfo)

            _currentDataSegmentIdx += 1
            _nextDataParticleIdx   = 0
            _currentDataSegment   = tmpData
        End If

        If _currentImageSegment IsNot Nothing AndAlso _nextImagedParticleIdx >= _currentImageSegment.Particles.Count Then
            Dim imgSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType( DataSegmentType.IIFParticleData ,_currentImageSegmentIdx+1)
            If imgSegInfo Is Nothing Then 'Reached end image segments. Not eof yet.
                _currentImageSegment = Nothing
            Else
                Dim tmpImgData = DataFunctions.loadDatafileSegment( _filePath, imgSegInfo)
                _currentImageSegmentIdx += 1
                _nextImagedParticleIdx  = 0
                _currentImageSegment    = tmpImgData
            End If
        End If
    End Sub

    Private ReadOnly _filePath As String ' Full path of the datafile.
    Private ReadOnly _dataSegmentType As DataSegmentType

    Private _currentDataSegment As ParticleDataSegment = Nothing  'Current segment of the raw particles we are processing.
    Private _currentDataSegmentIdx As Integer = 0 'Index of the current data segment.
    Private _nextDataParticleIdx As Integer = 0 ' The index in the segment of the next particle to look at.        

    Private _currentImageSegment As ParticleDataSegment = Nothing 'Current segment of the imaged particles we are processing.
    Private _currentImageSegmentIdx As Integer = 0 'Index of the current image segment
    Private _nextImagedParticleIdx As Integer = 0  ' The index in the segment of the next imaged particle to look at.        
End Class


''' <summary>
''' Implementation of a particle provider for Segmented returning particles without images.
''' Use this if the file has no images or you are not interested in the images.
''' </summary>
Public Class SegmentedNoIifParticleProvider
        Inherits ParticleProvider

    Public Sub New(dfw As DataFileWrapper)
        MyBase.New(dfw)
        _filePath = dfw.rawDataFile.path ' Path variable is set on loading to contain the full path.
        _dataSegmentType = If( dfw.rawDataFile.MeasurementSettings.SmartTriggeringEnabled, DataSegmentType.SmartTriggerData, DataSegmentType.AllParticleData)

        Dim dataSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType(_dataSegmentType, 0)
        If dataSegInfo Is Nothing Then 'Reached end of stream, no particles in the datafile.
            _currentDataSegment = Nothing
            _done = True
            Return
        End If

        _currentDataSegmentIdx = 0
        _nextDataParticleIdx   = 0
        _currentDataSegment = DataFunctions.loadDatafileSegment( _filePath, dataSegInfo)
    End Sub

    'A bit simpler because we do not do the filtering here.  We just get the next one
    Protected Overrides Function GetNextImpl( ByRef p As Particle) As Boolean
        p = Nothing

        LoadNextSegmentsIfRequired()

        If _done Then
            Return False
        End If

        Dim rawPart = _currentDataSegment.Particles( _nextDataParticleIdx )
         p = New  Particle(rawPart,_dfw.CytoSettings,_dfw.MeasurementSettings, _dfw.MeasurementInfo.MeasurementStart)
        _nextDataParticleIdx += 1
        Return True ' If we get here, we must have found something that matches.
    End Function

    ''' <summary>
    ''' Check if the nextIdx for parts and images is still in the currently loaded segment, if not
    ''' load the next segment and reset the index. (Checks both the particle and image indexes).
    ''' If we reach the end of the data segments, then eof is set.  If we reach the end of the image
    ''' index, this is not an eof.
    ''' </summary>
    Private Sub LoadNextSegmentsIfRequired()
        If _done Then
            Return 'Nothing to do.
        End If
        If _nextDataParticleIdx >= _currentDataSegment.Particles.Count Then
            Dim dataSegInfo = _dfw.rawDataFile.dfi.GetSegmentInfoOfType(_dataSegmentType,_currentDataSegmentIdx+1)
            If dataSegInfo Is Nothing Then 'Reached end of stream.
                _currentDataSegment = Nothing
                _done = True
                Return
            End If

            Dim tmpData = DataFunctions.loadDatafileSegment( _filePath, dataSegInfo )
            _currentDataSegmentIdx += 1
            _nextDataParticleIdx    = 0
            _currentDataSegment     = tmpData
        End If
    End Sub

    Private ReadOnly _filePath As String ' Full path of the datafile.
    Private ReadOnly _dataSegmentType As DataSegmentType

    Private _currentDataSegment As ParticleDataSegment = Nothing  'Current segment of the raw particles we are processing.
    Private _currentDataSegmentIdx As Integer = 0 'Index of the current data segment.
    Private _nextDataParticleIdx As Integer = 0 ' The index in the segment of the next particle to look at.        
End Class



End Namespace
