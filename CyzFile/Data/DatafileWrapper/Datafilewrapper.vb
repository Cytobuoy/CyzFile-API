Imports CytoSense.Data.ParticleHandling
Imports System.ComponentModel
Imports CytoSense.Data.SegmentedData
Imports System.Text.RegularExpressions

Namespace Data

    <Serializable()> Public Class DataFileWrapper 'Not really necessary to be Serializable anymore
#If MEM_DEBUG Then
        <NonSerialized>
        Public Shared NumberOfInstances As Integer = 0
#End If
        Dim _dataFile As DataFile
        Dim _splittedParticles As Particle()
        Dim _IIFParticles As ImagedParticle()
        Dim _disableParallism As Boolean

        Protected _IDs As Integer()

        <NonSerialized> Public _mismatchedIIFParticles As New List(Of RawIIFParticle) ' For old electronics  
        <NonSerialized> Public _axisValuesCache As New Analysis.AxisValuesCache()
        <NonSerialized> Public LoadedParticlesFraction As Double = 1

        Public Function GetParticleIDs(particleIndices As Integer()) As Integer()
            Dim particleIDs = New Integer(particleIndices.Length - 1) {}

            For i = 0 To particleIndices.Length - 1
                particleIDs(i) = _IDs(particleIndices(i))
            Next

            Return particleIDs
        End Function

        ''' <summary>
        ''' Constructor, create a wrapper around a datafile.
        ''' </summary>
        ''' <param name="datafile"></param>
        ''' <param name="DisableParallism"></param>
        ''' <remarks>Currently all the Reduced datafiles have an error in their counters.  The reduction process updates
        ''' the metadata incorrectly and that influences the counters resulting in bad concentrations and counts.
        ''' All the info we need is actually available thanks to the data stored in the reduction info. After loading
        ''' the datafile we use that information to reconstruct the data.
        ''' 
        ''' The total number of downloaded particles is stored in the ReductionInfo.OriginalNumber, this is the number
        ''' before smart triggering and we need to store it in the NumberOfDownloadedParticles.
        ''' 
        ''' The number of DownloadParticles actually returns the number of particles in the file, which should
        ''' be reported as the NumberOfSaved particles (and stored in the _NumberOfParticles_Smarttriggered)
        ''' Although that is debatable as it was not the smart triggering that decreased it.
        ''' When there was no smart triggering we do the same restoration process, because in the end, the
        ''' reduction is sort of like smart triggering.
        ''' </remarks>
        Public Sub New(ByVal datafile As DataFile, Optional DisableParallism As Boolean = False)
#If MEM_DEBUG Then
            Debug.WriteLine(String.Format("New DatafileWrapper, NumberOfInstances: {0} ", Interlocked.Increment(NumberOfInstances)))
#End If
            _dataFile = datafile
            _disableParallism = DisableParallism
            If Object.Equals(Nothing, _dataFile.MeasurementSettings.CytoSettings) Then  'old files may not have CytoSettings set in measurement settings
                _dataFile.MeasurementSettings.CytoSettings = _dataFile.CytoSenseSetting
            End If

            _dataFile.MeasurementInfo.SetDataFileWrapper(Me)

            If Me.ReductionInfo.WasReduced Then
                'Get all the data we need/want
                Dim newNumberDownloaded = ReductionInfo.OriginalNumber
                Dim newNumberOfSavedParticles = MeasurementInfo.NumberofDownloadedParticles
                Dim originalSmartTriggered = MeasurementInfo.NumberofParticles_smartTriggered
                Dim tmp = MeasurementInfo.NumberofSavedParticles ' (reports smart triggered for smart triggered files.)

                'Now update the data.
                If newNumberDownloaded > newNumberOfSavedParticles Then
                    _dataFile.MeasurementInfo.UpdateNumberofDownloadedParticles(newNumberDownloaded)
                    _dataFile.MeasurementInfo.NumberofParticles_smartTriggered = newNumberOfSavedParticles
                End If
            End If
            _concentrationHealthStatus = CheckConcentrationHealthStatus()
        End Sub

        Public Sub New(ByVal filename As String, Optional DisableParallism As Boolean = False)
            Me.New(loadDatafile(filename), DisableParallism)
        End Sub
        
        
        ''' <summary>
        ''' OverrideSettings only apply to files that were open when it was set or are applied when files are opened.
        ''' When exporting files these files are often not opened before export, so these changed overrides still need 
        ''' to be applied to these files during export.
        ''' </summary>
        ''' <param name="overrideSettings"></param>

        Public Sub SetOverrideValues(overrideSettings As CytoSettings.OverrideSettings)
            If overrideSettings.OverrideBeamWidthEnabled Then
                CytoSettings.EnableOverrideBeamWidth = overrideSettings.OverrideBeamWidthEnabled
                CytoSettings.OverrideLaserBeamWidth = overrideSettings.OverrideBeamWidth
            End If
            If overrideSettings.OverrideSampleCoreSpeedEnabled Then
                CytoSettings.EnableOverrideSampleCoreSpeed = overrideSettings.OverrideSampleCoreSpeedEnabled
                CytoSettings.OverrideSampleCoreSpeed = overrideSettings.OverrideSampleCoreSpeed
            End If
            If overrideSettings.OverrideMuPixelEnabled Then
                CytoSettings.iif.EnableOverrideMuPerPixel = overrideSettings.OverrideMuPixelEnabled
                CytoSettings.iif.OverrideMuPerPixel = overrideSettings.OverrideMuPixel    
            End If    
        End Sub


        ''' <summary>
        ''' This function is only needed for very old files, instruments without a PIC had an 
        ''' issue sometimes with concentrations, and there we need to check if preconcentration
        ''' and concentration are approximately the same.  If not the user needs to put the
        ''' datafilewrapper into a specific mode. Either
        '''  ConcentrationMode = ConcentrationModeEnum.Pre_measurement_FTDI
        '''         Or
        '''     ConcentrationMode = ConcentrationModeEnum.During_measurement_FTDI
        '''  If there is a problem, normally the pre-concentration is the correct one, but we 
        '''  should let the user choose.  
        '''  
        ''' The function returns True if the user needs to make a choice, False if not.
        ''' To make it easy to display the dialog, the function returns the concentration
        ''' and preconcentration IF the function returns True, i.e. when a dialog is needed.
        ''' If no dialog is needed, then the parameters are left UNTOUCHED! 
        ''' 
        ''' Add the following lines of code to every file opening
        ''' 
        '''  If (_datafile.CheckConcentration(concentration,preconcentration)) Then
        '''  
        '''     ... Display a message to the user and ask them what to do. 
        ''' 
        '''  End If
        ''' 
        ''' Unless you want to make the choice in a different way (global default, magic,...)
        ''' If you do not make a choice then trying to access the concentration in these cases
        ''' will result in an exception.
        ''' 
        ''' </summary>
        ''' <param name="concentration"></param>
        ''' <param name="preconcentration"></param>
        ''' <returns></returns>
        Public Function CheckConcentration( ByRef concentration As Double, ByRef preconcentration As Double) As Boolean
            If CytoSettings.hasaPIC Then
                Return False ' Nothing to do if it has a PIC (Or STM), only for Very old stuff.
            End If
            If MeasurementInfo.ConcentrationClass IsNot Nothing AndAlso MeasurementInfo.PreConcentrationClass IsNot Nothing Then
                Dim tmpConcentration    = MeasurementInfo.ConcentrationClass.Concentration(CytoSettings, MeasurementSettings)
                Dim tmpPreconcentration = MeasurementInfo.PreConcentrationClass.Concentration(CytoSettings, MeasurementSettings)
                
                If tmpConcentration / tmpPreconcentration < 0.7 OrElse tmpPreconcentration / tmpConcentration < 0.7 Then
                    concentration = tmpConcentration
                    preconcentration = tmpPreconcentration
                    Return True
                Else
                    Return False
                End If ' Else concentrations are approximately the same, so do not take any action.
            Else
                Return False ' We need both concentration, or we can do no check, so the caller cannot do anything.
            End If 
        End Function

#If MEM_DEBUG Then
        <OnDeserialized>
        Public Sub OnDeserialized(context As StreamingContext)
            'Interlocked.Increment(NumberOfInstances)
            Debug.WriteLine(String.Format("New DatafileWrapper deserialized, NumberOfInstances: {0} ", Interlocked.Increment(NumberOfInstances)))
        End Sub

        ''' <summary>
        ''' Needed for debugging "closed datafile resources are not released"
        ''' </summary>
        Protected Overrides Sub Finalize()
            ' check if this is an object for which the constructor failed (see sub New)
            If _dataFile.path IsNot Nothing AndAlso _dataFile.path <> "" Then
                Debug.WriteLine(String.Format("Datafilewrapper::Finalize - InstancesLeft {0} - filename [{1}]", Interlocked.Decrement(NumberOfInstances), _dataFile.path))
            End If
            MyBase.Finalize()
        End Sub
#End If
#Region "Original dataFile acces"
        <Browsable(False)>
        Public ReadOnly Property rawDataFile As DataFile
            Get
                Return _dataFile
            End Get
        End Property

        <Browsable(False)>
        Public ReadOnly Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _dataFile.CytoSenseSetting
            End Get
        End Property

        <Browsable(False)>
        Public ReadOnly Property MeasurementLog As String()
            Get
                Return _dataFile.Log
            End Get
        End Property

        <Browsable(False)>
        Public ReadOnly Property MeasurementSettings As CytoSense.MeasurementSettings.Measurement
            Get
                Return _dataFile.MeasurementSettings
            End Get
        End Property

        <Browsable(False)>
        Public ReadOnly Property MeasurementInfo As CytoSense.Data.MeasurementInfo
            Get
                If _dataFile.MeasurementInfo.NumberofSavedParticles = 0 AndAlso SplittedParticles.Length <> 0 Then
                    'the number of particles is incorrect (happens with some files made around march to june 2011)
                    _dataFile.MeasurementInfo.NumberofSavedParticles = _splittedParticles.Length - 1
                End If
                Return _dataFile.MeasurementInfo
            End Get
        End Property

        <Category("Measurement results"), DisplayName("Detector noise levels"), DescriptionAttribute(""), Browsable(True)>
        Public ReadOnly Property DectorBackgroundLevels_str As String
            Get
                Dim res As String = ""
                If _dataFile.MeasurementInfo.DetectorBackgroundLevel IsNot Nothing Then

                    For i = 0 To _dataFile.MeasurementInfo.DetectorBackgroundLevel.DetectorBackgrounds.Count - 1
                        If CytoSettings.channels(i).visible Then
                            res &= CytoSettings.channels(i).name & ": " & Math.Round(_dataFile.MeasurementInfo.DetectorBackgroundLevel.DetectorBackgrounds(i), 1) & "mV, "
                        End If
                    Next
                    If res.Length > 3 Then
                        res = res.Substring(0, res.Length - 2) 'remove last comma
                    End If
                End If
                Return res
            End Get
        End Property

        <Browsable(False)>
        Public ReadOnly Property ReductionInfo As CytoSense.Data.ReductionInfo
            Get
                Return _dataFile.ReductionInfo
            End Get
        End Property

#End Region

		Private _percentageOfParticlesShown As Integer = 100
		Public Property PercentageOfParticlesShown As Integer
			Get
				Return _percentageOfParticlesShown
			End Get
		    Set(value As Integer)
				_percentageOfParticlesShown = value
		    End Set
		End Property

        <NonSerialized> Private _ParticleReducer As Int64 = -1
        ''' <summary>
        ''' Subsamples the particle array. 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Browsable(False)>
        Public Property ParticleReducer As Int64
            Get
                Return _ParticleReducer
            End Get
            Set(value As Int64)
                _ParticleReducer = value
            End Set
        End Property

        ''' <summary>
        ''' When loading the file, we check if this is an EAWAG II file.  If it is then we check
        ''' the firs n particles to see how many spikes it contains.  If it contains too many
        ''' spikes, then we force an EAWAG spike filter on the file.
        ''' And we set flag to indicate it was filtered.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)>
        Public ReadOnly Property SplittedParticles As Particle()
            Get
                If Object.Equals(Nothing, _splittedParticles) Then
                    Dim i As Integer = 0
                    Dim particlesReadCount As Integer

                    If _dataFile.MeasurementSettings.SegmentedDatafile Then
                        particlesReadCount = _dataFile.MeasurementInfo.NumberofSavedParticles

                        If _dataFile.MeasurementInfo.NumberofSavedParticles = 0 Then
                            Dim tmp(-1) As CytoSense.Data.ParticleHandling.Particle
                            _splittedParticles = tmp
                            Return _splittedParticles
                        End If
                        _splittedParticles = loadParticlesFromSegments(_dataFile, CInt(ParticleReducer))
                        If MeasurementSettings.IIFCheck Then
                            findImagesInSegment() 'Match images to particles
                        End If
                    Else
                        If _dataFile.Data Is Nothing Then
                            ReDim _dataFile.Data(-1)
                        End If

                        Dim nThreads As Integer = 1 'denotes number of parallel threads that will be started to perform de-multiplexing
                        If _dataFile.Data.Length > 10000000.0 And Not _disableParallism Then 'this number may need tuning to find the optimal turnover point
                            nThreads = Environment.ProcessorCount
                        End If

                        If _dataFile.Data.Length > 0 Then
                            _splittedParticles = splitToParticles(_dataFile.Data, CytoSettings, MeasurementSettings, nThreads, _dataFile.sets)
                        Else
                            ReDim _splittedParticles(-1)
                        End If

                        _dataFile.Data = Nothing 'no point keeping this in memory

                        setArrivalTimesFromBlocks()

                        If CytoSettings.hasImageAndFlow And Not Object.Equals(Nothing, _dataFile.DSPIIF) AndAlso MeasurementSettings.IIFCheck Then findDSPImages() 'Match images to particles

                        particlesReadCount = _splittedParticles.Length
                    End If

                    If ParticleReducer > 0 Then
                        Dim newamount As Single
                        Dim stepsize As Single
                        If _splittedParticles.Length < ParticleReducer Then
                            newamount = _splittedParticles.Length
                            stepsize = 1
                        Else
                            newamount = ParticleReducer
                            stepsize = CSng(_splittedParticles.Length / ParticleReducer)
                        End If
                        Dim tmp(CInt(newamount - 1)) As Particle
                        For i = 0 To tmp.Length - 1
                            tmp(i) = _splittedParticles(CInt(i * stepsize))
                        Next
                        _splittedParticles = tmp
                    End If

                    LoadedParticlesFraction = _splittedParticles.Length / particlesReadCount

                    SortParticlesOnIdAndCacheIDs() ' to provide fast search capabilities for CC

                    ' Give particles an Index

                    i = 0
                    For Each particle In _splittedParticles
                        particle.Index = i
                        i += 1
                    Next

                    ' check if all iifParticles have an index
                    If _IIFParticles IsNot Nothing Then
                        For Each iifParticle In _IIFParticles
                            If iifParticle.Index = -1 Then
                                _log.InfoFormat("NO INDEX for IIFParticle ID [{0}]", iifParticle.ID)
                            End If
                        Next
                    End If

                    If _enableEawagFiltering AndAlso CytoSettings.SerialNumber = "CS-2014-66" AndAlso ParticlesContainEawagSpikes() Then
                        ForceEawagSpikeFiltering()
                        _eawagFiltered = True
                    End If

                    TryFixImageMatchError()
                End If
                ' Particles are loaded, so now check if we need to filter, and force filtering.

                Return _splittedParticles
            End Get
        End Property

        Private Shared _cytoUsbRegex As New Regex("5.8.0.(\d+)")

        ''' <summary>
        ''' Check if the CytoUsb version is between 5.8.0.0 and 5.8.0.7, in that case it is 
        ''' made by a CytoUsb version with bad image matching.
        ''' </summary>
        ''' <returns></returns>
        Public Function IsBadImageMatchingCytoUsbVersion() As Boolean
            Dim m = _cytoUsbRegex.Match(CytoSettings.CytoUSBVersion)
            Return m.Success AndAlso Integer.Parse(m.Groups(1).Value) < 8
        End Function

        ''' <summary>
        ''' There was an image matching error in the first CytoUsb 5.8.0.n versions for the V08 electronics.
        ''' Because of a change made for the V10 electronics, the images were offset by one.  We fixed this
        ''' in CytoUsb starting with release 5.8.0.8, but all versions 5.8.0.n versions where n &lt; 8 have the error
        ''' and need to be fixed after loading the datafile.  So call this after all the particles and images
        ''' have been loaded. (NOTE: Perhaps we could actually add this in the image matching functions instead
        ''' of doing it afterwards, but in that case we would need to do it twice.
        ''' Look at how we will do this here, and how in the 2 find functions, which ever is easier.
        ''' 
        ''' Looking at the data, it looks like if we have an imaged particle N then the picture that
        ''' belongs to that particle is in fact stored at imaged particle N +1.  So to fix this
        ''' we have to loop through all the images particles, and for every imaged particle (PseudoCode):
        ''' 
        ''' ImagedParticle[N].ImageStream =  ImagedParticle[N+1].ImagedStream
        ''' 
        ''' And the last imaged particle we have to convert to a normal particle instead of an imaged particle.
        ''' 
        ''' This fix function is called inside the SplittedParticles property, so we cannot use SplittedParticlesWithImages
        ''' as that calls SplittedParticles, and then things would really become interesting.  When this is called all
        ''' image matching stuff (Inside FindDspImages, or its segmented counterpart) have already been done, so the 
        ''' _splittedParticles array contains all particles and if they have images they are already ImagedParticles.
        ''' 
        ''' When the datafile was reduced, it is to risky to do anything with it.  We cannot predict how many gaps there 
        ''' are and if the datafile was already fixed before subsetting, or not, or?  So I leave that alone for now.
        ''' To bad, but I cannot help that.  The file will be flagged as suspect in the CC4 and that is all we can do for now.
        ''' </summary>
        Private Sub TryFixImageMatchError()
            If MeasurementSettings.IIFCheck AndAlso IsBadImageMatchingCytoUsbVersion() AndAlso Not ReductionInfo.WasReduced Then
                Dim numParticles = _splittedParticles.Length
                Dim prevImgPart As ImagedParticle = Nothing
                Dim prevImgPartIdx As Integer = -1
                For partIdx As Integer = 0 To numParticles - 1
                    Dim p = _splittedParticles(partIdx)
                    If p.hasImage Then
                        Dim imgPart = CType(p, ImagedParticle)
                        If prevImgPart IsNot Nothing Then 'Move this image to the previous particle.
                            prevImgPart.ImageHandling.ImageStream = imgPart.ImageHandling.ImageStream
                        End If
                        prevImgPart = imgPart ' Store current one for future reference
                        prevImgPartIdx = partIdx
                    End If 'Else no image, just ignore it.
                Next
                ' Finally, change last particle into a normal particle
                If prevImgPart IsNot Nothing Then
                    _splittedParticles(prevImgPartIdx) = New Particle(prevImgPart)
                End If
            End If ' Else no images, or no bad version, so nothing to do.
        End Sub

        <NonSerialized()> Private _eawagFiltered As Boolean = False
        ''' <summary>
        ''' Set to true if custom Eawag filtering was applied to this file, false if not.
        ''' </summary>
        ''' <returns></returns>
        <Browsable(False)>
        Public ReadOnly Property EawagFiltered As Boolean
            Get
                Return _eawagFiltered
            End Get
        End Property

        <NonSerialized()> Private _enableEawagFiltering As Boolean = False
        <Browsable(False)>
        Public Property EnableEawagFiltering As Boolean
            Get
                Return _enableEawagFiltering
            End Get
            Set(value As Boolean)
                _enableEawagFiltering = value
            End Set
        End Property



        ''' <summary>
        ''' Check the spike count of the first 100 particles, if that is more then 100, then
        ''' we consider the file to need filtering.  
        ''' </summary>
        ''' <returns></returns>
        Private Function ParticlesContainEawagSpikes() As Boolean
            Dim rawSpikeCounter As Integer = 0
            Dim checkNumParticles = 100
            If checkNumParticles > _splittedParticles.Length Then
                checkNumParticles = _splittedParticles.Length
            End If

            For i As Integer = 0 To checkNumParticles - 1
                rawSpikeCounter += _splittedParticles(i).GetRawSpikeCount()
            Next
            Return rawSpikeCounter >= checkNumParticles
        End Function

        ''' <summary>
        ''' Simply loop over all particles and call the filtering function.
        ''' </summary>
        ''' <remarks>This has to be done on loading before any access to the particles is done.
        ''' The force function will preload the particle with filtered data, so the 
        ''' when later accessing the data the preloaded version is used.
        ''' NOTE: Look at using a paralell loop for this, may speed things up, and maybe not.
        ''' </remarks>
        Private Sub ForceEawagSpikeFiltering()
            For Each p In _splittedParticles
                p.EawagFilterChannelData()
            Next
        End Sub

        Private Shared Function CompareByIds(lhs As Particle, rhs As Particle) As Integer
            Return lhs.ID.CompareTo(rhs.ID)
        End Function

        ''' <summary>
        ''' It looks like the new subsampling methods for segmented datafiles results in particles becoming out of order in
        ''' the particles array.  But when doing a lookup on IDs I use a binary search. That fails horribly when the data is not
        ''' sorted. So Sort them first.
        ''' FIXME: This is a temp hack, costs extra time.  Probably should rearrange the particle handling in CC4 completely.
        ''' </summary>
        Private Sub SortParticlesOnIdAndCacheIDs()
            Array.Sort(Of Particle)(_splittedParticles, AddressOf CompareByIds)
            ReDim _IDs(_splittedParticles.Length - 1)
            For i As Integer = 0 To _splittedParticles.Length - 1
                _IDs(i) = _splittedParticles(i).ID
            Next
        End Sub

        <Browsable(False)>
        Public ReadOnly Property SplittedParticlesWithImages As ImagedParticle()
            Get
                Dim parts As Particle() = SplittedParticles 'compulsory call to SplittedParticles. In here, several important fields are set.
                If _IIFParticles Is Nothing Then
                    Dim res As New List(Of ImagedParticle)
                    For i = 0 To parts.Length - 1
                        If parts(i).hasImage Then
                            res.Add(DirectCast(parts(i), ImagedParticle))
                        End If
                    Next
                    _IIFParticles = res.ToArray
                End If
                Return _IIFParticles
            End Get
        End Property

        <NonSerialized>
        Private _IIFParticleIndices As Integer()

        <Browsable(False)>
        Public ReadOnly Property SplittedParticlesWithImagesIndices As Integer()
            Get
                Dim particles As Particle() = SplittedParticles 'compulsory call to SplittedParticles. In here, several important fields are set.

                If _IIFParticleIndices Is Nothing Then
                    Dim indices = New List(Of Integer)

                    For i = 0 To particles.Length - 1
                        If particles(i).hasImage Then
                            indices.Add(i)
                        End If
                    Next

                    _IIFParticleIndices = indices.ToArray
                End If

                Return _IIFParticleIndices
            End Get
        End Property

        ''' <summary>
        ''' For multiplexed byte based datafiles only the arrival times are not saved during measuring and need to be reconstructed
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub setArrivalTimesFromBlocks()

            Dim particleArrivalTimes(_splittedParticles.Length - 1) As Single
            If MeasurementInfo.BlockInfo Is Nothing Then
                'no block info available; just interpolate over complete measurement time
                Dim x As Single = MeasurementInfo.ActualMeasureTime / _splittedParticles.Length
                For i = 0 To _splittedParticles.Length - 1
                    particleArrivalTimes(i) = i * x
                Next
            Else
                'block info available; perform local interpolation per block
                Dim prevBlockEnd As DateTime = MeasurementInfo.MeasurementStart
                Dim prevBlockCount As Integer = 0
                Dim numBlocks = MeasurementInfo.BlockInfo.Length
                Dim currtime As Single = 0
                Dim partIdx As Integer = 0
                Dim maxtimeoutIdx As Integer = 0
                For blockIdx = 0 To MeasurementInfo.BlockInfo.DataList.Length - 1
                    Dim bi As DataPointList.datapoint = MeasurementInfo.BlockInfo.DataList(blockIdx)

                    'check and adjust for maxtimeouts:
                    If MeasurementInfo.maxTimeOuts IsNot Nothing AndAlso maxtimeoutIdx < MeasurementInfo.maxTimeOuts.Count Then
                        If bi.time > MeasurementInfo.maxTimeOuts(maxtimeoutIdx) Then
                            prevBlockEnd = MeasurementInfo.maxTimeOuts(maxtimeoutIdx)
                            currtime = CSng(MeasurementInfo.maxTimeOuts(maxtimeoutIdx).Subtract(MeasurementInfo.MeasurementStart).Milliseconds / 1000)
                            maxtimeoutIdx += 1
                        End If
                    End If

                    Dim newBlockCount As Integer = CInt(bi.data)
                    Dim sampDuration As Double = (bi.time - prevBlockEnd).TotalSeconds / (newBlockCount - prevBlockCount) ' Recalculate sample duration (time per particle) for each block 
                    Dim partCountBlockEnd As Integer = Math.Min(newBlockCount, _splittedParticles.Length) ' The total count in the block, and the actual count in the splitted particles may differ a few, so need to check this.
                    While partIdx < partCountBlockEnd
                        currtime = CSng(currtime + sampDuration)
                        particleArrivalTimes(partIdx) = currtime
                        partIdx += 1
                    End While

                    currtime = CSng(bi.time.Subtract(MeasurementInfo.MeasurementStart).TotalMilliseconds / 1000)
                    prevBlockCount = newBlockCount
                    prevBlockEnd = bi.time
                Next
            End If

            Dim start As DateTime = MeasurementInfo.ActualAcquireStart
            For i = 0 To _splittedParticles.Length - 1
                '_splittedParticles(i).TimeOfArrival = start.Add(New TimeSpan(0, 0, 0, 0, particleArrivalTimes(i) * 1000))
                _splittedParticles(i).setArrivalTime(particleArrivalTimes(i))
            Next
        End Sub

        ''' <summary>
        ''' Returns a particle with the given particle.ID. Returns nothing if that particle can't be found
        ''' </summary>
        Public Function getParticleByID(ID As Integer) As Particle
            If _IDs Is Nothing Then
                Return Nothing
            End If
            Dim index As Integer = Array.BinarySearch(_IDs, ID)
            If index < 0 Then
                Return Nothing
            End If
            Return SplittedParticles(index)
        End Function

        Public Function getParticlesWithIDs(IDs As Integer()) As Particle()
            Dim res(IDs.Length - 1) As Particle
            For i As Integer = 0 To IDs.Length - 1
                res(i) = getParticleByID(IDs(i))
            Next
            Return res
        End Function

        ''' <summary>
        ''' Finds the given particle in the SplittedParticles array, and returns its index. Returns -1 if the particle can't be found
        ''' </summary>
        Public Function getIndexOfParticle(part As Particle) As Integer
            Dim index As Integer = Array.BinarySearch(Me.SplittedParticles, part, New ParticleComparer)
            Return index
        End Function

        Public Class ParticleComparer
            Implements IComparer(Of Particle)

            Public Function Compare(x As ParticleHandling.Particle, y As ParticleHandling.Particle) As Integer Implements System.Collections.Generic.IComparer(Of ParticleHandling.Particle).Compare
                Return x.ID.CompareTo(y.ID)
            End Function
        End Class

        Public Function getIndexOfID(ID As Integer) As Integer
            Return Array.BinarySearch(Me._IDs, ID)
        End Function

        ''' <summary>
        ''' Compares the ID's of a Particle and a RawParticle, To be used in Array.BinarySearch
        ''' </summary>
        Public Class CompareSplittedAndRawIDs
            Implements IComparer

            Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
                Return CType(x, Particle).ID.CompareTo(CType(y, RawParticle).ID)
            End Function
        End Class


        Public Sub Debug_CheckDuplicates_SplittedParticles()
            ' RVDH test for duplicate particles

            For particleIndex = 0 To _splittedParticles.Length - 1
                Dim particleChannels = _splittedParticles(particleIndex).ChannelData_Hardware

                For compareIndex = 0 To _splittedParticles.Length - 1
                    If compareIndex = particleIndex Then Continue For

                    Dim compareChannels = _splittedParticles(compareIndex).ChannelData_Hardware
                    Dim channelsMatch = False

                    For channelIndex = 1 To particleChannels.Length - 1
                        Dim particle_channel As Byte() = particleChannels(channelIndex).Data_Raw
                        Dim compare_channel As Byte() = compareChannels(channelIndex).Data_Raw

                        channelsMatch = particle_channel.SequenceEqual(compare_channel)

                        If Not (channelsMatch) Then
                            Exit For
                        End If
                    Next

                    If channelsMatch Then
                        Console.WriteLine("particleID [{0}] has same HW channels as particleID [{1}]",
                                          _splittedParticles(particleIndex).ID, _splittedParticles(compareIndex).ID)
                    End If
                Next
            Next
        End Sub

        Public Sub Debug_Dump_Hardware_Channel(channel As Byte())
            For Each b In channel
                Console.Write("{0,4}", b)
            Next
        End Sub



        ''' <summary>
        ''' In the segmented datafile, seperate segments exist for IIF particles. Due to dataflow restrictions in CytoUSB, these particles are copies 
        ''' from the other segments (AllParticleData or SmartriggerData segments) extended with image data. The function below finds any particle in the
        ''' currently loaded segments that are also in the image segment.
        ''' </summary>
        ''' <remarks>To reduce the memory load on the CytoUsb, and save data to disk before the measurement is finished, we store
        ''' smaller segments to disk. So we now (can) have more then one image segment. (current settings give us 10, but that will change
        ''' in the future) So we need to load all image segments and process them.
        ''' Not a terribly efficient implementation I think. but for now it should do.</remarks>
        Private Sub findImagesInSegment()
            Dim rawIIFParticle As RawIIFParticle
            Dim iifParticles As New List(Of ImagedParticle)()
            ReDim _IIFParticles(-1)

            Dim numImageSegments = _dataFile.dfi.NumberOfSegmentsOfType(DataSegmentType.IIFParticleData)
            If numImageSegments = 0 Then
                Return
            End If

            'Debug_CheckDuplicates_SplittedParticles()

            Dim rawIIFParticles = New List(Of RawIIFParticle)
            Dim splittedParticleMatches = New Integer(_splittedParticles.Length - 1) {} ' = New List(Of List(Of Integer)) ' contains a list of particleindices that match on SWS signal

            Dim measurementStart As DateTime = MeasurementInfo.ActualAcquireStart
            Dim comparer As IComparer = New CompareSplittedAndRawIDs()

            For segmentIndex = 0 To numImageSegments - 1
                Dim s = loadDatafileSegment(_dataFile, DataSegmentType.IIFParticleData, segmentIndex)
                Dim rawParticles = s.Particles

                If rawParticles.Count > 0 Then
                    If CytoSettings.PIC.PICIIF Then 'PICIIF delivers particles directly to the splittedparticle array through ID
                        For rawParticleIndex = 0 To rawParticles.Count - 1
                            Dim splittedIndex As Integer = Array.BinarySearch(_splittedParticles, rawParticles(rawParticleIndex), comparer)

                            If (splittedIndex >= 0) Then
                                If Not _splittedParticles(splittedIndex).hasImage Then
                                    _splittedParticles(splittedIndex) = New ImagedParticle(DirectCast(rawParticles(rawParticleIndex),RawIIFParticle), CytoSettings, MeasurementSettings, measurementStart)
                                    iifParticles.Add(DirectCast(_splittedParticles(splittedIndex),ImagedParticle))
                                End If 'ELSE DUPLICATE IMAGE FROM BUGGY DATAFILES GENERATED BY CYTOUSB 5.8.0.5 and CYTOUSB 5.8.0.6
                            Else
                                rawIIFParticle = CType(rawParticles(rawParticleIndex), RawIIFParticle)
                                rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.NoMatch
                                _mismatchedIIFParticles.Add(rawIIFParticle)
                            End If
                        Next

                        _IIFParticles = iifParticles.ToArray()
                    Else
                        For rawParticleIndex = 0 To rawParticles.Count - 1
                            rawIIFParticle = CType(rawParticles(rawParticleIndex), RawIIFParticle)
                            rawIIFParticle.SegmentIndex = segmentIndex
                            rawIIFParticle.SegmentParticleIndex = rawParticleIndex
                            rawIIFParticle.SplittedParticleIndex = -1 ' No match
                            rawIIFParticles.Add(rawIIFParticle)

                            For splittedParticleIndex = 0 To _splittedParticles.Length - 1
                                If _splittedParticles(splittedParticleIndex) = rawParticles(rawParticleIndex) Then
                                    If rawIIFParticle.SplittedParticleIndex = -1 Then
                                        rawIIFParticle.SplittedParticleIndex = splittedParticleIndex
                                    Else
                                        rawIIFParticle.SplittedParticleIndex = Integer.MaxValue ' indicates multiple matches
                                    End If

                                    splittedParticleMatches(splittedParticleIndex) += 1
                                End If
                            Next
                        Next
                    End If
                End If ' Else empty segment ?!?, ignore it nothing to do.
            Next

            ' NOTE rawIIFParticles only used by old SWS matching (loop  does not run for ID matching)
            For Each rawIIFParticle In rawIIFParticles
                If rawIIFParticle.SplittedParticleIndex = -1 Then
                    rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.NoMatch
                ElseIf rawIIFParticle.SplittedParticleIndex = Integer.MaxValue Then
                    rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.MultipleMatches
                ElseIf (splittedParticleMatches(rawIIFParticle.SplittedParticleIndex) = 1) Then
                    ' match is exclusive
                    rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.Success
                Else
                    rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.MultipleMatches
                End If

                If rawIIFParticle.MatchResult = RawIIFParticle.ImageMatchResult.Success Then
                    rawIIFParticle.reSetID(_splittedParticles(rawIIFParticle.SplittedParticleIndex).ID)
                    _splittedParticles(rawIIFParticle.SplittedParticleIndex) = New ImagedParticle(rawIIFParticle, CytoSettings, MeasurementSettings, measurementStart)
                    iifParticles.Add(DirectCast(_splittedParticles(rawIIFParticle.SplittedParticleIndex),ImagedParticle))
                Else
                    _mismatchedIIFParticles.Add(rawIIFParticle)
                End If
            Next

            _IIFParticles = iifParticles.ToArray()
        End Sub


        ''' <summary>
        ''' Locate the IIFImages in the splittedparticles. 
        ''' Every IIFImage is already matched against pulseshape data, but due to a discrepancy 
        ''' between how the particles are cut in realtime and using the normal splittoparticle 
        ''' function, some IIFImages are not in the Splittedparticles.
        ''' 
        ''' The particle and images are in the order they were received, so the next match should
        ''' always be further in the particle array.  So if we have a match, then the next image
        ''' should always belong to a particle that is further down the line.
        ''' TODO: Replace calls to _dataFile.DSPIIF.particles(i, True), with direct access to
        ''' the array. (Then again, a max of approx. 150 calls, so may not be a big issue.)
        ''' NOTE:  In theory the loop:                 
        '''    For j = 0 To numParticles - 1
        ''' Should run from partIdx to numParticles, because the next image should always be further in the 
        ''' file then the previous.  But that results in large differences with current implementation.
        ''' However, this does not completely match the old implementation, it is possible for
        ''' CytoUsb to (incorrectly) match particles in the wrong order.
        ''' Because of that, if we do not find a match we start looking back from the current partIdx
        ''' to the front of the array.  This way we still consider all particles the same as the old
        ''' implementation.
        ''' </summary>
        ''' <remarks>Unfound particles are placed in a list in the datafilewrapper.iif-properties</remarks>
        Private Sub findDSPImages()
            _dataFile.DSPIIF.RemoveImages_PlaceStreams() 'Old files: no stream available

            Dim unFound As New List(Of DSP.DSPParticle)
            Dim found As New List(Of DSP.DSPParticle)

            Dim imgIdx As Integer = 0
            Dim partIdx As Integer = 0 ' Index of next particle to try for an image match.
            Dim numImages = _dataFile.DSPIIF.getNumberOfSucceses
            Dim splitParticles = SplittedParticles
            Dim numParticles = splitParticles.Length

            While imgIdx < numImages AndAlso partIdx < numParticles
                Dim tmp As DSP.DSPParticle = _dataFile.DSPIIF.particles(imgIdx, True)
                Dim tmppar As New CytoSense.Data.ParticleHandling.Particle(tmp.ChannelData, 1, CytoSettings, MeasurementSettings)
                Dim matched As Boolean = False
                For j = partIdx To numParticles - 1
                    If tmppar = splitParticles(j) Then
                        tmp._PartID = j
                        found.Add(tmp)
                        matched = True
                        partIdx = j + 1
                        Exit For
                    End If
                Next
                If Not matched Then
                    ' Could not find it in the expected location where it should be.  Now we search back from the current
                    ' location to the front, to see if we can find it there.  It should not be, but in some cases it is.
                    ' In extreme cases this can result in a particle being matched to 2 images because it is reconsidered 
                    ' we scan backwards.  This should normally not happen.
                    ' We start at partIfx - 2, because partIdx was already checked, and partIdx - 1 was the 
                    ' latest match.
                    Dim j = partIdx - 2
                    While j >= 0
                        If tmppar = splitParticles(j) Then
                            tmp._PartID = j
                            found.Add(tmp)
                            matched = True 'We do not set the partIdx because this was a funny match.
                            Exit While
                        End If
                        j -= 1
                    End While
                    If Not matched Then
                        tmp._PartID = -1
                        unFound.Add(tmp)
                    End If
                End If
                imgIdx += 1
            End While

            While imgIdx < numImages ' Unmatched images left at the end.
                Dim tmp As DSP.DSPParticle = _dataFile.DSPIIF.particles(imgIdx, True)
                tmp._PartID = -1
                unFound.Add(tmp)
                imgIdx += 1
            End While
            ' No need to process remaining particles, if all the images were processed, then we can simply ignore the remaining particles.

            'for CC3 support:
            _dataFile.DSPIIF._unmatchedImages = unFound
            _dataFile.DSPIIF._matchedImages = found

            'for CC4 support:

            ReDim _IIFParticles(found.Count - 1)
            For i = 0 To found.Count - 1
                Dim imagedParticle = New ImagedParticle(_splittedParticles(found(i)._PartID), found(i))

                _IIFParticles(i) = imagedParticle
                _splittedParticles(found(i)._PartID) = imagedParticle
            Next
        End Sub


        ''' <summary>
        ''' Returns the INDEXES (NOT IDs) of particles with images in the splitted particles list
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetImageIDs() As Integer()
            Dim res As New List(Of Integer)

            For i = 0 To SplittedParticles.Length - 1
                If SplittedParticles(i).hasImage Then
                    res.Add(i)
                End If
            Next
            Return res.ToArray
        End Function

        Private _concentration As Double = Double.NegativeInfinity
        ''' <summary>
        ''' Returns concentration (parts/uL), uses method set by ConcentrationMode. 
        ''' Can throw ConcentrationMisMatchException when in automatic mode and no PIC concentration count was available.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Measurement results"), DisplayName("Concentration"), DescriptionAttribute(""), Browsable(True), CytoSense.Data.DataBase.Attributes.Format("0.#E0 \p/µL")>
        Public ReadOnly Property Concentration As Double
            Get
                If Double.IsNegativeInfinity(_concentration) Then
                    getConcentration()
                End If
                Return _concentration
            End Get
        End Property

        Private _actualConcentration As Double = Double.NegativeInfinity
        ''' <summary>
        ''' Returns concentration (parts/uL), uses method set by ConcentrationMode. 
        ''' Can throw ConcentrationMisMatchException when in automatic mode and no PIC concentration count was available.
        ''' Calculates concentration as measured directly by the concentration counter. 
        ''' For this only the hardware triggering matters, smart triggering, file reduction etc does not.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property ActualConcentration As Double
            Get
                If Double.IsNegativeInfinity(_actualConcentration) Then
                    getActualConcentration()
                End If
                Return _actualConcentration
            End Get
        End Property

        ''' <summary>
        ''' Returns the concentration in the specified mode. If returns concentration of -1 the mode is not available.
        ''' Automatically adjust for smart triggering of CC reduction
        ''' </summary>
        ''' <param name="mode"></param>
        ''' <value></value>
        ''' <returns>concentratin in parts/uL</returns>
        ''' <remarks>Recalculates every time the mode changes</remarks>
        <Browsable(False)>
        Public ReadOnly Property Concentration(ByVal mode As ConcentrationModeEnum) As Double
            Get
                'Only recalc if needed
                If Not mode = ConcentrationMode Then
                    ConcentrationMode = mode
                    getConcentration()
                ElseIf Double.IsNegativeInfinity(_concentration) Then
                    getConcentration()
                End If
                Return _concentration
            End Get
        End Property

        ''' <summary>
        ''' Returns concentration (parts/uL), uses method set by the argument. 
        ''' Can throw ConcentrationMisMatchException when in automatic mode and no PIC concentration count was available.
        ''' Calculates concentration as measured directly by the concentration counter. 
        ''' For this only the hardware triggering matters, smart triggering, file reduction etc does not.
        ''' </summary>
        ''' <remarks></remarks>
        <Browsable(False)>
        Public ReadOnly Property ActualConcentration(ByVal concmode As ConcentrationModeEnum) As Double
            Get
                If Not concmode = ConcentrationMode Then
                    ConcentrationMode = concmode
                    getActualConcentration()
                ElseIf Double.IsNegativeInfinity(_actualConcentration) Then
                    getActualConcentration()
                End If
                Return _actualConcentration
            End Get
        End Property

        Private _concentrationmode As ConcentrationModeEnum = ConcentrationModeEnum.Automatic
        ''' <summary>
        ''' Selects which concentration count is to be used. Recommend to leave at automatic mode. 
        ''' If however automatic mode fails, a manual choice can be made here. 
        ''' When in automatic mode it returns -1, no concentration count is available
        ''' When in another mode it returns -1, the chosen concentration count is not available
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Recommended: automatic</remarks>
        <Browsable(False)>
        Public Property ConcentrationMode As ConcentrationModeEnum
            Get
                Return _concentrationmode
            End Get
            Set(value As ConcentrationModeEnum)
                _concentrationmode = value
                _concentration = Double.NegativeInfinity   'reset concentration. so it will be recalculated
                _actualConcentration = Double.NegativeInfinity
            End Set
        End Property

        Public Enum ConcentrationHealthStatusEnum
            OK
            Warning
            Err
            Invalid ' Normally I use, the first 0 value, for this, but adding it to an existing Enum, and I am not sure that is not serialized somewhere.
        End Enum
        Private _concentrationHealthStatus As ConcentrationHealthStatusEnum = ConcentrationHealthStatusEnum.Invalid
        <DisplayName("Concentration Health Status "), Browsable(False)>
        Public ReadOnly Property ConcentrationHealthStatus As ConcentrationHealthStatusEnum
            Get
                Return _concentrationHealthStatus
            End Get
        End Property


        Private Sub getActualConcentration()
            If CytoSettings.hasFJDataElectronics Then
                _actualConcentration = CDbl(MeasurementInfo.MeasurementCounters.ValidTriggerCount) / pumpedVolume
            Else
                Select Case _concentrationmode
                    Case ConcentrationModeEnum.During_measurement_FTDI
                        Try
                            _actualConcentration = _dataFile.MeasurementInfo.ConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)
                        Catch exp As Exception
                            _actualConcentration = -1
                        End Try
                    Case ConcentrationModeEnum.Pre_measurement_FTDI
                        Try
                            _actualConcentration = _dataFile.MeasurementInfo.PreConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)
                        Catch exp As Exception
                            _actualConcentration = -1
                        End Try
                    Case ConcentrationModeEnum.During_measurement_PIC
                        Try
                            If Not CytoSettings.PIC.ConcentrationCounter Then
                                _actualConcentration = -1
                            Else
                                _actualConcentration = _dataFile.MeasurementInfo.sensorLogs.PICConcentration.getMean(MeasurementInfo.MeasurementStart, New System.TimeSpan(0, 0, CInt(MeasurementInfo.ActualMeasureTime))) / _dataFile.MeasurementSettings.SamplePompSpeed
                            End If
                        Catch exp As Exception
                            _actualConcentration = -1
                        End Try
                    Case ConcentrationModeEnum.Pre_measurement_PIC
                        Try
                            If Not CytoSettings.PIC.ConcentrationCounter Then
                                _actualConcentration = -1
                            Else
                                _actualConcentration = _dataFile.MeasurementInfo.sensorLogs.PICPreConcentration.getMean() / _dataFile.MeasurementSettings.SamplePompSpeed
                            End If
                        Catch exp As Exception
                            _actualConcentration = -1
                        End Try
                    Case ConcentrationModeEnum.Automatic    'recommended!
                        getAutomaticConcentration()
                End Select
            End If
        End Sub

        ''' <summary>
        ''' For old instruments that have no PIC, if there is a concentration warning we want the
        ''' user to select which count to use, it could be an issue with the counts instead
        ''' of the measurements. This should be done on loading, so in here, if that is the
        ''' case, we simply throw an exception. This SHOULD NEVER HAPPEN, only a programming
        ''' error in the users of the DataFileWrapper class can cause this.
        ''' </summary>
        Private Sub getAutomaticConcentration()
            If _dataFile.CytoSenseSetting.hasaPIC AndAlso _dataFile.CytoSenseSetting.PIC.ConcentrationCounter AndAlso Not Object.Equals(Nothing, _dataFile.MeasurementInfo.sensorLogs.PICConcentration) Then 'prefer to use PIC count:
                _actualConcentration = _dataFile.MeasurementInfo.sensorLogs.PICConcentration.getMean(MeasurementInfo.MeasurementStart, New System.TimeSpan(0, 0, CInt(MeasurementInfo.ActualMeasureTime))) / _dataFile.MeasurementSettings.SamplePompSpeed
            ElseIf Not Object.Equals(Nothing, _dataFile.MeasurementInfo.ConcentrationClass) Then 'if PIC count is not available, check if FTDI count is available:
                Dim c_tmp As Double = _dataFile.MeasurementInfo.ConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)

                If _dataFile.MeasurementSettings.SeperateConcentration AndAlso Not Object.Equals(_dataFile.MeasurementInfo.PreConcentrationClass, Nothing) Then 'check if FTDI count matches pre count (if available)
                    Dim cpre_tmp As Double = _dataFile.MeasurementInfo.PreConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)
                    If c_tmp / cpre_tmp < 0.7 Or cpre_tmp / c_tmp < 0.7 Then 'check if there is a difference between pre and post counts
                        Throw New ConcentrationMisMatchException
                    End If
                End If
                _actualConcentration = c_tmp
            ElseIf Not Object.Equals(Nothing, _dataFile.MeasurementInfo.PreConcentrationClass) Then 'only pre measured ftdi count seems available
                _actualConcentration = _dataFile.MeasurementInfo.PreConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)
            Else    'no concentration count seems available
                _actualConcentration = -1
            End If
        End Sub

        Private Function CheckConcentrationHealthStatus() As ConcentrationHealthStatusEnum
            If _dataFile.CytoSenseSetting.hasaPIC AndAlso _dataFile.CytoSenseSetting.PIC.ConcentrationCounter AndAlso _dataFile.MeasurementInfo.sensorLogs.PICConcentration IsNot Nothing Then 
                If _dataFile.MeasurementInfo.sensorLogs.PICPreConcentration IsNot Nothing AndAlso _dataFile.MeasurementInfo.sensorLogs.PICPreConcentration.Length > 0 Then
                    Dim conc = _dataFile.MeasurementInfo.sensorLogs.PICConcentration.getMean(MeasurementInfo.MeasurementStart, New System.TimeSpan(0, 0, CInt(MeasurementInfo.ActualMeasureTime))) / _dataFile.MeasurementSettings.SamplePompSpeed
                    Dim preconc = _dataFile.MeasurementInfo.sensorLogs.PICPreConcentration.getMean(MeasurementInfo.MeasurementStart, New System.TimeSpan(0, 0, CInt(CytoSettings.SeperateConcentrationText))) / _dataFile.MeasurementSettings.ConfiguredSamplePompSpeed

                    If conc / preconc < 0.7 Or preconc / conc < 0.7 Then 
                        Return ConcentrationHealthStatusEnum.Warning
                    Else
                        Return ConcentrationHealthStatusEnum.OK
                    End If
                Else ' No pre concentration, so nothing to check
                    Return ConcentrationHealthStatusEnum.OK
                End If

            ElseIf Not Object.Equals(Nothing, _dataFile.MeasurementInfo.ConcentrationClass) Then 'if PIC count is not available, check if FTDI count is available:
                Dim c_tmp As Double = _dataFile.MeasurementInfo.ConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)

                If _dataFile.MeasurementSettings.SeperateConcentration AndAlso Not Object.Equals(_dataFile.MeasurementInfo.PreConcentrationClass, Nothing) Then 'check if FTDI count matches pre count (if available)
                    Dim cpre_tmp As Double = _dataFile.MeasurementInfo.PreConcentrationClass.Concentration(_dataFile.CytoSenseSetting, _dataFile.MeasurementSettings)

                    If c_tmp / cpre_tmp < 0.7 Or cpre_tmp / c_tmp < 0.7 Then 'check if there is a difference between pre and post counts
                        Return ConcentrationHealthStatusEnum.Warning
                    Else
                        Return ConcentrationHealthStatusEnum.OK
                    End If
                Else ' No pre concentration, so nothing to check
                    Return ConcentrationHealthStatusEnum.OK
                End If
            ElseIf Not Object.Equals(Nothing, _dataFile.MeasurementInfo.PreConcentrationClass) Then 'only pre measured ftdi count seems available
                Return ConcentrationHealthStatusEnum.Warning
            Else    'no concentration count seems available
                Return ConcentrationHealthStatusEnum.Err
            End If
        End Function

        Private Sub getConcentration()
            getActualConcentration()
            _concentration = _actualConcentration
            _concentration = _concentration * (_dataFile.MeasurementInfo.NumberofSavedParticles  / _dataFile.MeasurementInfo.NumberofDownloadedParticles)

            If _concentration <= 0 Then _concentration = -1 'If a -1 got scrambled in the compensations

        End Sub


        ''' <summary>
        ''' Stored in the measurement settings in newer files, but not in older ones, so we leave the property here, for compatibility,
        ''' but we use the one from measurement info when available.  If not we check the SplittedParticles, and if
        ''' that fails, it means someone is trying to do a header only load of a segmented datafile that does not
        ''' contain the NumberOfPictures in the header info field.  This should happen rarely, as these files are not widely used.
        ''' In that case we just return a -1.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Measurement results"), DisplayName("Pictures"), DescriptionAttribute(""), Browsable(True), CytoSense.Data.DataBase.Attributes.Format("D")>
        Public ReadOnly Property numberOfPictures As Integer
            Get
                Dim num = MeasurementInfo.NumberOfPictures
                If num >= 0 Then
                    Return num
                End If ' Else number of pictures not in measurement info, try to load from original location, only works if the actual particles are
                ' are loaded. Using exception for this is dirty, but it should happen rarely as these files should not exist in the real world.
                Try
                    Return SplittedParticlesWithImages.Length
                Catch ex As Exception
                    Return -1
                End Try
            End Get
        End Property

        ''' <summary>
        ''' Total volume by sample pump speed (uL/s) * duration (s) = Vol (uL)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Category("Measurement results"), DisplayName("Pumped volume"), DescriptionAttribute(""), Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \µL")>
        Public ReadOnly Property pumpedVolume As Double
            Get
                Return (MeasurementInfo.ActualMeasureTime * MeasurementSettings.SamplePompSpeed)
            End Get
        End Property


        ''' <summary>
        ''' Analyzed volume by #particles (parts)/ concentration (parts/uL) = Volume (uL). 
        ''' Is compensated for smart triggering and file reduction.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>New electronics does not use concentration anymore.</remarks>
        <Category("Measurement results"), DisplayName("Analysed volume"), DescriptionAttribute(""), Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \µL")>
        Public ReadOnly Property analyzedVolume As Double
            Get
                If CytoSettings.hasFJDataElectronics Then
                    Return (CDbl(MeasurementInfo.NumberofDownloadedParticles) / CDbl(MeasurementInfo.MeasurementCounters.ValidTriggerCount)) * pumpedVolume
                Else
                    Return MeasurementInfo.NumberofDownloadedParticles / ActualConcentration
                End If
            End Get
        End Property

        ''' <summary>
        ''' Analyzed volume by #particles (parts)/ concentration (parts/uL) = Volume (uL). 
        ''' Is compensated for smart triggering and file reduction.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>New electronics does not use the concentration mode anymore.</remarks>
        Public ReadOnly Property analyzedVolume(concentrationmode As ConcentrationModeEnum) As Double
            Get
                If CytoSettings.hasFJDataElectronics Then
                    Return (CDbl(MeasurementInfo.NumberofSavedParticles) / CDbl(MeasurementInfo.MeasurementCounters.ValidTriggerCount)) * pumpedVolume
                Else
                    Return MeasurementInfo.NumberofSavedParticles / ActualConcentration(concentrationmode)
                End If
            End Get
        End Property

        Private Shared _log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    End Class

    Public Enum ConcentrationModeEnum
        Automatic
        Pre_measurement_FTDI
        During_measurement_FTDI
        Pre_measurement_PIC
        During_measurement_PIC
    End Enum


End Namespace