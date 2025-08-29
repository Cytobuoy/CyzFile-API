Imports CytoSense.Data.SegmentedData
Imports CytoSense.Data.ParticleHandling
Imports System.Runtime.CompilerServices
Imports System.IO

Namespace Data
    Public Module DataFunctions

        ''' <summary>
        ''' Load particle data from the segmented data file.
        ''' </summary>
        ''' <param name="df">The datafile to load the data from, must be segmented. </param>
        ''' <param name="maxNumParticles">The maximum number of particles to load, 0 means unlimited.</param>
        ''' <returns>And array with all the loaded particles.</returns>
        ''' <remarks>The functions loads SmartTriggerData segments if the the files was made with smarttriggering and
        ''' alldata segements if no smarttriggering was used. If the number of particles is more then the specified maximum
        ''' then subsampling is used to reduce the number of loaded particles. Set maxNumParticles to 0 for no subsampling.
        ''' Version that uses parallel for loop, to do a smuch in parallell as it can. Not this means the filtering is a little
        ''' less exact, we may end up with a few less particles because of parital particles at the end of a segnment, but that should not
        ''' matter to much. (we could move filtering to after the loading step to avoid this, but doing it early is a bit more efficient.
        ''' NOTE: Particles are NOT ordered in ayway after loading, because of paralellization.
        ''' </remarks>
        Public Function loadParticlesFromSegments(df As DataFile, maxNumParticles As Integer) As Particle()
            Dim divider As Single
            Dim segType As DataSegmentType
            If df.MeasurementSettings.SmartTriggeringEnabled Then
                divider = CSng(If(maxNumParticles > 0, df.MeasurementInfo.NumberofParticles_smartTriggered / maxNumParticles, 0.0))
                segType = DataSegmentType.SmartTriggerData
            Else
                divider = CSng(If(maxNumParticles > 0, df.MeasurementInfo.NumberofSavedParticles / maxNumParticles, 0.0))
                segType = DataSegmentType.AllParticleData
            End If
            Dim nSegs = df.dfi.NumberOfSegmentsOfType(segType)

            Dim cs = df.CytoSenseSetting
            Dim measurement = df.MeasurementSettings
            Dim start = df.MeasurementInfo.MeasurementStart

            Dim loadedParticles As New List(Of Particle)()

            Parallel.ForEach(df.dfi.Segments,
                Sub(segInfo)
                    If segInfo.Type <> segType Then
                        Return ' We are only interested in a specific type, ignore the others.
                    End If

                    Dim segment As ParticleDataSegment = loadDatafileSegment(df.path, segInfo) 'Loading sub segment, so no limit here, we impose that after filtering.
                    Dim parts = segment.Particles
                    Dim localParts As New List(Of Particle)()
                    Dim k As Integer = 0
                    If divider <= 1.0 Then ' No need to subsample, just use them all.
                        For partIdx = 0 To parts.Count - 1
                            localParts.Add(New Particle(parts(partIdx), cs, measurement, start))
                        Next
                    Else ' Need to subsample
                        Dim partIdx As Integer = CInt(Math.Round(k * divider))
                        While partIdx < parts.Count
                            localParts.Add(New Particle(parts(partIdx), cs, measurement, start))
                            k += 1
                            partIdx = CInt(Math.Round(k * divider))
                        End While
                    End If
                    SyncLock loadedParticles
                        loadedParticles.AddRange(localParts)
                    End SyncLock
                End Sub)
            ' Check if we need to FIX duplicate segments written by CytoUsb 5.8.0.5 and CytoUsb 5.8.0.6 Remove all duplicate
            ' IDs. 
            Dim vers = cs.CytoUSBSettings.CytoUSBVersion

            Dim resultParticles As Particle()

            If RequireDuplicateSegmentFix(cs.CytoUSBSettings.CytoUSBVersion) Then
                resultParticles = loadedParticles.DistinctBy(Function(p) p.ID).ToArray()
            Else
                resultParticles = loadedParticles.ToArray()
            End If

            Array.Sort(resultParticles, Function(p1 As Particle, p2 As Particle)
                                            Return p1.ID.CompareTo(p2.ID)
                                        End Function)

            Return resultParticles
        End Function


        ' Support method,  DistinctBy is supported in LINQ on .net core, but not on the .net framework package. So
        ' Added custom implementation here for the time being.  Once we drop support for .net framework this one can be removed.

        ''' <summary>
        ''' The distinct method, but looking only at a single member, instead of the entire
        ''' object.  Shamefully copied/based on the MoreLinq package but I did not want to include the whole
        ''' package just for this one method.
        ''' The method was included for the duplicate segment fix.  A custome implementation may be faster
        ''' but it should not be used very often so the performance should not be a problem.
        ''' </summary>
        <Extension()>
        Private Function DistinctBy(Of TSource, TKey)(source As IEnumerable(Of TSource), keySelector As Func(Of TSource, TKey) ) As IEnumerable(Of TSource)
            Return source.DistinctBy(keySelector, Nothing)
        End Function

        ''' <summary>
        ''' The distinct method, but looking only at a single member, instead of the entire
        ''' object.  Shamefully copied/based on the MoreLinq package but I did not want to include the whole
        ''' package just for this one method.
        ''' The method was included for the duplicate segment fix.  A custome implementation may be faster
        ''' but it should not be used very often so the performance should not be a problem.
        ''' </summary>
            <Extension()>
        Private Iterator Function DistinctBy(Of TSource, TKey)(source As IEnumerable(Of TSource), keySelector As Func(Of TSource, TKey), comparer As IEqualityComparer(Of TKey) ) As IEnumerable(Of TSource)
            If source Is Nothing  Then
                Throw New ArgumentNullException( NameOf(source) )
            End If
            If keySelector Is Nothing Then
                throw New ArgumentNullException(NameOf(keySelector))
            End If
            Dim knownKeys = new HashSet(Of TKey)(comparer)
            For Each element In source
                If knownKeys.Add(keySelector(element)) Then
                    Yield element
                End If
            Next
        End Function

        ''' <summary>
        ''' The first 5.8 releases of CytoUsb contained a bug that could cause crashes when doing an IIF measurement
        ''' but when it did not crash it would write every segment into the datafile twice!.  So if this was the case
        ''' then we need to fix this.  So this function parses the version string and Decides if the version is affected.
        ''' Since there are only a few possible version we just compare the version strings and do not really parse.
        ''' The only offical release that has it is 5.8.0.6 and there is a RC candidate 5.8.0.5 that has it, so only
        ''' these 2 versions are affected.
        ''' </summary>
        ''' <param name="CytoUSBVersion"></param>
        ''' <returns>True if the duplicate segment fix is required.</returns>
        Private Function RequireDuplicateSegmentFix( CytoUSBVersion As String ) As Boolean
            Return CytoUSBVersion = "CytoUSB Version: 5.8.0.6" OrElse CytoUSBVersion = "CytoUSB Version: 5.8.0.5"
        End Function


        ''' <summary>
        ''' Load the datafile info (datafile structure) from a segmented file.  
        ''' The offset and size specify where in the file it is located and the size.
        ''' </summary>
        ''' <param name="fs">The stream load the info from</param>
        ''' <param name="offset">The start position in the stream</param>
        ''' <param name="size">The number of bytes to load.</param>
        ''' <returns>The datafile object loaded from specified position</returns>
        ''' <remarks>We assert that the number of bytes read from the stream is the number of bytes
        ''' expected as an extra check on file structure. (only in debug versions)</remarks>
        Public Function loadDataFileInfoFromStream( fs As IO.Stream, offset As Long, size As Long ) As DataFile
            Return loadObjectAtOffset(Of DataFile)(fs, offset, size)
        End Function

        ''' <summary>
        ''' Loads the index segment (header) of a segmented datafile. The index segment comprises from the first 65535 bytes of the file. 
        ''' </summary>
        ''' <remarks>We cannot use the templated base version because we do not know the exact size of the header, we only
        ''' have a maximum size, so although the base code would work, the debug assertions would fail.</remarks>
        Public Function loadDatafileHeader( fs As IO.Stream ) As SegmentedData.SegmentedDataFileHeader
            fs.Seek(0, IO.SeekOrigin.Begin)
            Dim startPos As Int64 = fs.Position
            Dim index = Serializing.deserializeStream(Of SegmentedData.SegmentedDataFileHeader)(fs)
            Dim endPos As Int64 = fs.Position
            Debug.Assert(endPos-startPos <= 65535) ' We do not know the exact size, but it can never be smaller, the end is filled with 0's but they should not be read.
            Return index
        End Function


        Private Function loadSegmentAtOffset(fs As IO.Stream, offset As Int64, size As Int64) As SegmentedData.ParticleDataSegment
            Return loadObjectAtOffset(Of SegmentedData.ParticleDataSegment)(fs, offset, size)
        End Function


        Private Function loadObjectAtOffset(Of T) (fs As IO.Stream, offset As Int64, size As Int64) As T
            fs.Seek(offset, IO.SeekOrigin.Begin)
            Dim startPos As Int64 = fs.Position
            Debug.Assert(startPos = offset)
            Dim r = Serializing.deserializeStream(Of T)(fs)
            Dim endPos As Int64 = fs.Position
            Debug.Assert(endPos-startPos = size)
            Return r
        End Function


        ''' <summary>
        ''' Load the segment described in segInfo from the 'filePath'
        ''' </summary>
        ''' <param name="filePath"></param>
        ''' <param name="segInfo"></param>
        ''' <returns></returns>
        Public Function loadDatafileSegment( filePath As String, segInfo As SegmentInfo ) As ParticleDataSegment
            Using fs = New IO.FileStream(filePath, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                Return loadSegmentAtOffset(fs, segInfo.Offset, segInfo.Count)
            End Using
        End Function


        ''' <summary>
        ''' Load a specified data segment.
        ''' </summary>
        ''' <param name="df"></param>
        ''' <param name="type"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function loadDatafileSegment(ByRef df As DataFile, type As SegmentedData.DataSegmentType, n As Integer) As SegmentedData.ParticleDataSegment

            If type = SegmentedData.DataSegmentType.DataFileInfo Or type = SegmentedData.DataSegmentType.Header Then
                Throw New NotImplementedException("Only particle segments can be loaded using this function.")
            End If

            Dim count As Integer = 0
            For i = 0 To df.dfi.Segments.Count - 1
                If df.dfi.Segments(i).Type = type And count = n Then
                    Return loadDatafileSegment(df.path, df.dfi.Segments(i) )
                ElseIf df.dfi.Segments(i).Type = type Then
                    count += 1
                End If
            Next
            Return Nothing 'no such segment can be found. 
        End Function

        ''' <summary>
        ''' A backwards compatible datafile loader. If a old type directly serialized datafile structure is to be loaded, 
        ''' it will load as usually. If a new type segmented data file is to be loaded and segments is nothing, it will only load the datafile meta data.
        ''' Any other data will automatically be fetched on demand. 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function loadDatafile(path As String) As DataFile
            Dim df As DataFile
            Try
                Using fs = New IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
                    Dim dfi As SegmentedData.SegmentedDataFileHeader = loadDatafileHeader(fs) 'load the header
                    df = loadDataFileInfoFromStream(fs, dfi.df_info.Offset, dfi.df_info.Count)
                    df.path = path
                    df.dfi = dfi
                End Using
            Catch ex As Exception
                'normal data file with direct data placement through serialization
                df = Serializing.loadFromFile(Of DataFile)(path)
                df.path = path
            End Try

            df.MeasurementSettings.CytoSettings = df.CytoSenseSetting 'legacy fix for some datafiles which don't have an up to date cytosettings in the measurementsettings
            Return df
        End Function



    End Module





End Namespace

