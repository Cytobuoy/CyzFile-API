Imports System.ComponentModel
Imports System.Runtime.Serialization
Imports System.Data
Imports CytoSense.CytoSettings

Namespace Data
    ''' <summary>
    ''' The following classes cannot be moved due to serialization limitations
    ''' </summary>
    Public Module Data   
        <Serializable()> Public Structure DataFile
            Public Shared release As New Serializing.VersionTrackableClass(New Date(2012, 2, 18))
            Dim Version As String
            Dim Data() As Byte
            Dim CytoSenseSetting As CytoSettings.CytoSenseSetting
            Dim MeasurementSettings As MeasurementSettings.Measurement
            Dim MeasurementInfo As MeasurementInfo
            Dim Log() As String
            Dim ReductionInfo As ReductionInfo

            Private IIF As DSP.DSPParticles
            Public Property DSPIIF As DSP.DSPParticles
                Get
                    Return IIF
                End Get
                Set(value As DSP.DSPParticles)
                    IIF = value
                End Set
            End Property

            Dim sets As Cluster()

            'Segmented datafile stuff:           
            Dim Sections_length As Integer
            <NonSerialized> Dim path As String
            <NonSerialized> Dim dfi As SegmentedData.SegmentedDataFileHeader


        ''' <summary>
        ''' After deserialization set the smart trigger info on MeasuermentInfo, so the counters can be interpreted in the
        ''' correct way.
        ''' </summary>
        ''' <param name="context">Deserialization context</param>
        <OnDeserialized()> _
        Friend Sub OnDeserializedMethod(ByVal context As StreamingContext)
            MeasurementInfo.InitializeMeasurementWasSmartTriggered(If(MeasurementSettings.SmartTriggeringEnabled,MeasurementSmartTrigger.Yes,MeasurementSmartTrigger.No))
            MeasurementInfo.SetMeasurementSettings(MeasurementSettings)
            '
            ' HERE BE DRAGONS
            '
            ' We forgot to update the optical magnification/mu per pixel settings for the CS-2015-68 when it was
            ' was upgraded in 2019-02-22, so it created files with an incorrect magnification until today
            ' 2021-02-03.  So I detect these files and fix  the magnification/mu per pixel after loading.
            If  CytoSenseSetting.SerialNumber = "CS-2015-68" Then
                 If MeasurementInfo.MeasurementStart > New Date(2019,2,22) AndAlso CytoSenseSetting.machineConfigurationRelease.ReleaseDate < New Date(2021,2,2) Then
                    CytoSenseSetting.iif.opticalMagnification = 16
                    CytoSenseSetting.iif.ImageScaleMuPerPixelP = CytoSenseSetting.iif.CameraFeatures.PixelPitch / CytoSenseSetting.iif.opticalMagnification
                 End If ' Else file is not from the period where incorrect settings were added.
            End If ' Else different instrument, do nothing.

        End Sub

        End Structure
        ''' <summary>
        ''' Was the measurement smart triggered or Not?  Used for a hack int he measurement info structure.
        ''' </summary>
        Public Enum MeasurementSmartTrigger
            Invalid
            No
            Yes
        End Enum

        <Serializable()> Public Class Cluster
            Public partIDs() As Integer
            Public name As String
            Public PError As Double
            Public ID As Integer


            Public Sub New(ByVal name As String)
                Me.name = name
            End Sub
            Public Sub New(ByVal name As String, ByVal partIDs() As Integer, Optional ByVal PError As Double = -1)
                Me.name = name
                Me.partIDs = partIDs
                Me.PError = PError
            End Sub

        End Class

        <Serializable>
        Public Class MeasurementCountersT
            ' Normal grabber counters
            Public MaskedTriggerCount As UInt32
            Public ValidTriggerCount As UInt32
            Public RecordedParticleCount As UInt32
            'Imaging counters
            Public CameraTriggeredCount As UInt32
            Public CameraBusyCount As UInt32
            Public CameraToLate As UInt32
        End Class


        <Serializable()> Public Structure MeasurementInfo

            ''' <summary>
            ''' Deprecated. Use a clone methode instead
            ''' </summary>
            ''' <param name="MeasurementStartTime"></param>
            ''' <param name="MeasureTime"></param>
            ''' <param name="sensorLogs"></param>
            ''' <param name="ConcentrationClass"></param>
            ''' <param name="preConcentrationClass"></param>
            ''' <param name="backgroundlevel"></param>
            ''' <param name="numberofParticles_allDownloaded"></param>
            ''' <param name="numberofParticles_smartTriggered"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal MeasurementStartTime As Date, ByVal MeasureTime As Single, Optional ByVal sensorLogs As SensorDataPointLogs = Nothing, Optional ByVal ConcentrationClass As CytoSense.Concentration.HWConcentrations = Nothing, Optional ByVal preConcentrationClass As CytoSense.Concentration.HWConcentrations = Nothing, Optional ByVal backgroundlevel As DetectorBackgroundNoiselevel = Nothing, Optional ByVal numberofParticles_allDownloaded As Integer = -1, Optional ByVal numberofParticles_smartTriggered As Integer = -1, Optional _SequentialHallTimeoutsOccurred As Integer = 0)
                Me.MeasurementStartTime = MeasurementStartTime
                Me.MeasureTime = MeasureTime
                Me.ConcentrationClass = ConcentrationClass
                Me.PreConcentrationClass = preConcentrationClass
                Me.sensorLogs = sensorLogs
                Me.DetectorBackgroundLevel = backgroundlevel
                Me.BlockInfo = BlockInfo
                Me.NumberofParticles = numberofParticles_allDownloaded
                Me.NumberofParticles_smartTriggered = numberofParticles_smartTriggered
				Me._SequentialHalltimeoutsOccurred = _SequentialHallTimeoutsOccurred
            End Sub

			Public Sub New(ByVal MeasurementStartTime As Date, ByVal backgroundlevel As DetectorBackgroundNoiselevel)
				Me.MeasurementStartTime = MeasurementStartTime
				Me.DetectorBackgroundLevel = backgroundlevel
			 End Sub

            Private MeasurementStartTime As DateTime 'depreciated. Replaced by log. Keep for legacy compatibility reasons
            Private MeasureTime As Single 'Depreciated. Replaced by log. Keep for legacy compatibility reasons
            Public ConcentrationClass As CytoSense.Concentration.HWConcentrations
            Public PreConcentrationClass As CytoSense.Concentration.HWConcentrations
            Dim sensorLogs As SensorDataPointLogs
            Private NumberofParticles As Integer
            Private _NumberofParticles As Integer
            Public NumberofParticles_smartTriggered As Integer
            Dim BlockInfo As DataPointList
            Dim BlockAcquireTimes As DataPointList
            Dim BlockRoundTripTimes As DataPointList
            Dim BlockSizes As DataPointList

            Dim log As MeasurementLog
            Dim maxTimeOuts As List(Of Date)
			Private _SequentialHallTimeoutsOccurred As Integer
            Public RealtimeTuningWarning As Boolean

            <ComponentModel.Browsable(False)>
            Public Property GPSData As List(Of GPS.GPSCoordinate)
            <OptionalField> _
            Private _numberOfPictures As Integer
            <OptionalField>
            Public MeasurementCounters As MeasurementCountersT

            <NonSerialized>
            Private _dfw As DataFileWrapper

            ''' <summary>
            ''' Set a back reference to the containing DataFileWrapper. This is needed to find the concentration mode that is
            ''' being used for the datafile, and we need this to calculate the correct number of hardware detected particles
            ''' for some (very) old instruments where there is a big difference between the pre-concentration and the concentration.
            ''' </summary>
            ''' <param name="dfw"></param>
            Public Sub SetDataFileWrapper( dfw As DataFileWrapper)
                Debug.Assert(dfw IsNot Nothing )
                _dfw = dfw
            End Sub


            ''' <summary>
            ''' Initialize optional fields before de-serializing, so we can see later if they were present in the loaded object,
            ''' or not.  Currently only numberOfPictures is set.  This way we can detect if it was loaded, >= 0 , or not -1.
            ''' </summary>
            ''' <remarks></remarks>
            <OnDeserializing> _
            Private Sub InitOptionalFields(sc As StreamingContext)
                _numberOfPictures = -1
            End Sub

            'kind of complex to serve the right numbers up from here, so added some unserialized magic flag
            'that needs to be set on deserialization by the DataFile.
            Public Sub InitializeMeasurementWasSmartTriggered( smartTrigger As MeasurementSmartTrigger )
                MeasurementWasSmartTriggered = smartTrigger
            End Sub


            <NonSerialized()> _
            Private MeasurementWasSmartTriggered As MeasurementSmartTrigger

            <NonSerialized()> _
            Private _measurementSettings As MeasurementSettings.Measurement

            ' Call this on deserialization to set the measurement, this is needed so we can calculated the
            ' correct concentration for older files.
            Public Sub SetMeasurementSettings( mset As MeasurementSettings.Measurement)
                _measurementSettings = mset
            End Sub



            ' OK, another legacy bug fix situation. Once upon a time we had this nice little variable NumberofParticles. 
            ' Allegedly, this variable wasn't always set correctly. So, it was fixed by creating a new variable _numberofParticles
            ' and a property NumberofParticles. This was however a very, very bad fix, as it created two kinds of data files. One
            ' with the information in _NumberofParticles variable, and one in the old variable NumberofParticles.
            ' Now, in comes Smart Triggering, which has a variable of its own: NumberofParticles_smartTriggered. 
            ' This NumberofParticles_smartTriggered is used to find out how many particles of the hardware triggered/counted
            ' particles were saved. But, this can only be done when there is correct NumberofParticles information.
            ' Except of course, for another exception in which this number was directly saved to _NumberofParticles_smartTriggered,
            ' but in which case _numberofparticles = 0. (which is of course wrong)
            ' And based on the numbers the system cannot distinguish between smart triggered measurements and normal,
            ' so I added a non serialized field that is set on loading. 
            ' Also when the file was reduced, we need to return the smart triggered number for number of saved particles.
            ' However we do not know at this point if it was reduced, so we simply check to see if the number of particles
            ' smart triggered <> 0 , that should detect that case.
            ''' <summary>
            ''' Represents the number of particles actually accepted by the hardware triggering,'
            ''' acquired in the instrument, downloaded to the computer, and finally (if applicable) accepted by smart triggering. 
            ''' And also not dropped by a save subset as Cyz file. This is tricky.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Category("Measurement results"), DisplayName("Particles (smart triggered)"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("F0")>
            Public Property NumberofSavedParticles As Integer
                Get
                    Debug.Assert(MeasurementWasSmartTriggered <> MeasurementSmartTrigger.Invalid, "The 'MeasurementWasSmartTriggered' member needs to be initialized before accessing this field!")

                    If MeasurementWasSmartTriggered = MeasurementSmartTrigger.Yes OrElse NumberofParticles_smartTriggered <> 0  Then
                        Return NumberofParticles_smartTriggered
                    Else 'No smart trigger value set, older file
                        If _NumberofParticles > 0 Then
                            Return _NumberofParticles
                        Else
                            Return NumberOfParticles
                        End IF
                    End If
                End Get
                Set(value As Integer)
                    NumberofParticles = 0
                    _NumberofParticles = value
                End Set
            End Property
            ''' <summary>
            ''' Represents the numberofparticles actually accepted by the hardware triggering, acquired in the instrument and downloaded to the computer. If smarttriggering was enabled, not all these particles are saved to the datafile.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Category("Measurement results"), DisplayName("Particles (downloaded)"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("F0")>
            Public ReadOnly Property NumberofDownloadedParticles As Integer
                Get
                    If NumberofParticles = 0 Then
                        Return _NumberofParticles
                    Else
                        Return NumberofParticles
                    End If
                End Get
            End Property

			Public ReadOnly Property SequentialHalltimeoutsOccurred As Integer
				Get
					Return _SequentialHallTimeoutsOccurred
				End Get
			End Property

            ''' <summary>
            ''' All the counters for number of particles are a total mess in the metadata, it needs to be
            ''' redone completely when we go to a new fileformat. But currently I do not dare touch it.
            ''' I added this function to be able to fix reduced smart triggered files on loading.
            ''' </summary>
            ''' <param name="numParts"></param>
            Public Sub UpdateNumberofDownloadedParticles( numParts As Integer)
                NumberofParticles  = 0
                _NumberofParticles = numParts
            End Sub


            ''' <summary>
            ''' Represents the numberofparticles actually detected by the hardware triggering. Not all particles may have been acquired due to dead time.
            ''' Probably a lot have not been.  For V10 electronics there are special counters to use instead of hte use fo concentration values.
            ''' For very old machines that have no PIC, we need to do something else.  As a quick and VERY UGLY fix, I basicly copied
            ''' the DataFileWrapper.getAutomaticConcentration here and I use that now.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Category("Measurement results"), DisplayName("Particles (counted)"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("F0")>
            Public ReadOnly Property NumberofCountedParticles As Int64
                Get
                    If MeasurementCounters IsNot Nothing Then
                        Return MeasurementCounters.ValidTriggerCount
                    Else
                        Return GetAutomaticNumberOfCountedParticles()
                    End IF
                End Get
            End Property
   
        Private Function GetAutomaticNumberOfCountedParticles() As Int64
            Dim numDbl As Double = -1
            If sensorLogs.PICConcentration IsNot Nothing AndAlso sensorLogs.PICConcentration.DataList.Length > 0 Then
                Dim sum As Double = 0
                For i = 0 To sensorLogs.PICConcentration.DataList.Length - 1
                    sum += sensorLogs.PICConcentration.DataList(i).data
                Next
                
                numDbl = ActualMeasureTime * (sum / sensorLogs.PICConcentration.DataList.Length)
            Else If Me.ConcentrationClass IsNot Nothing Then 'if PIC count is not available, check if FTDI count is available:
                Debug.Assert(_dfw IsNot Nothing, "We need a reference to the enclosing DataFileWrapper to get the correct concentration mode." )
                
                Dim concentration = _dfw.Concentration()
                ' We have concentration instead of particle rate, so we need pumped volume to calculate the total.
                Dim pumpedVolume = ActualMeasureTime * _measurementSettings.SamplePompSpeed
                numDbl = (concentration * pumpedVolume)
            End If 
            Try
                Dim num As Long = CLng(numDbl)
                Return num
            Catch ex As Exception 'Probably a conversion error because of a NaN or something.
                Return -1
            End Try
        End Function


            ''' <summary>
            ''' Due to a bug in CytoUSB  in which the MeasureTime variable started just before the actual 
            ''' measurement (as it should) but ended just before saving to hd (and thus taking overhead 
            ''' such as measuring pmt background levels), a new better and improved measurement time was 
            ''' needed for precise concentration/volume calculations.
            ''' This actualMeasureTime is thus only measuring the time pulseshape data download was active.
            ''' It is calculated by measuring the difference between the MeasurementStartTime and the time the 
            ''' last block was received.
            ''' If however, no block info is available for whatever reason, the actualMeasureTime is reverted 
            ''' back to the original MeasureTime, because in the early days, no overhead at the end of a measurement
            ''' was present back then anyway so this should be ok.
            ''' 
            ''' With newer files a measurementlog was added, in which each task is logged with a time stamp. In this case 
            ''' it is a simple matter to calculate the time difference between the beginning end ending of the task "acquiring".
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <Category("Measurement results"), DisplayName("Duration"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \s")>
            Public ReadOnly Property ActualMeasureTime As Single
                Get
                    Try
                        If Not Object.Equals(Nothing, log) Then
                            Return log.getAqcuireDuration
                        Else
                            Return CSng(DateDiff(DateInterval.Second, MeasurementStartTime, BlockInfo.DataList(BlockInfo.Length - 1).time))
                        End If
                    Catch e As Exception
                        Return MeasureTime
                    End Try
                End Get
            End Property

            ''' <summary>
            ''' The moment (in seconds in the measurement) at which data acquisition starts.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <ComponentModel.Browsable(False)>
            Public ReadOnly Property startAcquireTime As Integer
                Get
                    If Not Object.Equals(log, Nothing) Then
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStart, log.getAcquireStart))
                    Else
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStartTime, MeasurementStart))
                    End If

                End Get
            End Property

            ''' <summary>
            ''' The date time at which data acquisition starts.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <ComponentModel.Browsable(False)>
            Public ReadOnly Property ActualAcquireStart As DateTime
                Get
                    Return MeasurementStart.Add(New TimeSpan(0, 0, startAcquireTime))
                End Get
            End Property

            ''' <summary>
            ''' The moment (in seconds in the measurement) at which flushing (sucking in of sample) starts.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <ComponentModel.Browsable(False)>
            Public ReadOnly Property startFlushTime As Integer
                Get
                    If Not Object.Equals(log, Nothing) Then
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStart, log.getTask(MeasurementLog.Tasks.Flush, MeasurementLog.BeginOrEndEnum.Begining).time))
                    Else
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStartTime, MeasurementStart))
                    End If

                End Get
            End Property



            ''' <summary>
            ''' The moment (in seconds in the measurement) at which data acquisition ends.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            <ComponentModel.Browsable(False)>
            Public ReadOnly Property endAcquireTime As Integer
                Get
                    If Not Object.Equals(log, Nothing) Then
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStart, log.getAcquireEnd))
                    Else
                        Return CInt(DateDiff(DateInterval.Second, MeasurementStart, BlockInfo.DataList(BlockInfo.Length - 1).time))
                    End If
                End Get
            End Property

            <Category("Measurement results"), DisplayName("Start"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("g")>
            Public ReadOnly Property MeasurementStart As DateTime
                Get
                    If Object.Equals(Nothing, log) Then
                        Return MeasurementStartTime
                    Else
                        Return log.getTask(MeasurementLog.Tasks.Acquiring, MeasurementLog.BeginOrEndEnum.Begining).time
                    End If
                End Get
            End Property

            Private _particleArrivalTimes As Single()
            <ComponentModel.Browsable(False)>
            Public ReadOnly Property ParticleArrivalTimes As Single()
                Get
                    If _particleArrivalTimes Is Nothing Then
                        calculateArrivalTimes()
                    End If
                    Return _particleArrivalTimes
                End Get
            End Property
            Public Sub calculateArrivalTimes()
                Dim mStart = MeasurementStart
                Dim prevBlockEnd As DateTime = mStart
                Dim prevBlockCount As Integer = 0
                Dim partIdx As Integer = 0

                Dim currtime As Integer = 0

                Dim partCountBlockEnd As Integer = 0

                Dim x As Double
                ReDim _particleArrivalTimes(NumberofSavedParticles - 1)

                Dim numBlocks = BlockInfo.Length
                For Each bi In BlockInfo.DataList
                    Dim newBlockCount As Integer = CInt(bi.data)
                    Dim sampDuration As Double = (bi.time - prevBlockEnd).TotalSeconds / (newBlockCount - prevBlockCount) ' Recalculate sample duration for each block
                    partCountBlockEnd = Math.Min(newBlockCount, NumberofSavedParticles) ' THe total count in the block, and the actual count in the splitted particles may differ a few, so need to check this.
                    While partIdx < partCountBlockEnd
                        x = currtime
                        currtime += CInt(sampDuration)
                        _particleArrivalTimes(partIdx) = currtime
                        partIdx += 1
                    End While
                    prevBlockCount = newBlockCount
                    prevBlockEnd = bi.time
                Next
            End Sub


            Dim DetectorBackgroundLevel As DetectorBackgroundNoiselevel


#Region "Sensorlog database properties"

            <Category("Auxilary sensors"), DisplayName("Sheath temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property SheathTemp As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.SheathTemp))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Laser temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property LaserTemp As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.LaserTemp))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser Base temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property Laser1BaseTemperature As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.Laser1BaseTemperature))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser Diode temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property Laser1DiodeTemperature As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.Laser1DiodeTemperature))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser Diode Current"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 mA")>
            Public ReadOnly Property Laser1DiodeCurrent As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.Laser1DiodeCurrent))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser TEC Load"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \%")>
            Public ReadOnly Property Laser1TecLoad As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.Laser1TecLoad))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser Input Voltage"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.00 V")>
            Public ReadOnly Property Laser1InputVoltage As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.Laser1InputVoltage))
                End Get
            End Property

            <Category("Laser Data"), DisplayName("Laser Mode"), DescriptionAttribute("Laser mode at the end of the measurement."), ComponentModel.Browsable(True)>
            Public ReadOnly Property Laser1Mode As String
                Get
                        If sensorLogs.Laser1Mode IsNot Nothing AndAlso sensorLogs.Laser1Mode.Length > 0 Then
                            Return sensorLogs.Laser1Mode.GetLast().ToString()
                        Else
                            Return "Invalid"
                        End If
                End Get
            End Property



            <Category("Auxilary sensors"), DisplayName("System temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property SystemTemp As Single
                Get
                    If HasValues(sensorLogs.SystemTemp)  Then
                        Return CSng(sensorLogs.SystemTemp.getMean())
                    ElseIf HasValues(sensorLogs.internalTemperature) Then
                        Return CSng(sensorLogs.internalTemperature.getMean())
                    Else
                        Return Single.NaN
                    End If
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Buoy temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property BuoyTemp As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.BuoyTemp))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("PMT temperature"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \°C")>
            Public ReadOnly Property PMTTemp As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.PMTTemp))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Reference voltage ratio"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0")>
            Public ReadOnly Property VRefFactor As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.VRefFactor))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("External power supply voltage"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \V")>
            Public ReadOnly Property ExtSupplyPowerVoltage As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.extSupplyPowerVoltage))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Buoy Voltage"), DescriptionAttribute("Voltage available in the Buoy Battery"), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 V")>
            Public ReadOnly Property BuoyVoltage As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.BuoyExtBatteryVoltage))
                End Get
            End Property

            ''' <summary>
            ''' Calculate the mean value of the datapoint list if it has any values and is not nothing,
            ''' else return NaN
            ''' </summary>
            ''' <param name="dpl"></param>
            ''' <returns></returns>
            Private Shared Function MeanOrNaN(dpl As DataPointList) As Double
                If dpl IsNot Nothing AndAlso dpl.Length > 0 Then
                    Return dpl.getMean()
                Else
                    Return Single.NaN
                End If
            End Function

            Private Function HasValues( dpl As DataPointList ) As Boolean
                Return dpl IsNot Nothing AndAlso dpl.Length > 0
            End Function

            <Category("Auxilary sensors"), DisplayName("External pressure"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \B")>
            Public ReadOnly Property ExtPressure As Single
                Get
                    If HasValues(sensorLogs.extPressure)Then
                        Return CSng(sensorLogs.extPressure.getMean())
                    ElseIf HasValues(sensorLogs.ExternalPressureDataPIC) Then
                        Return CSng(sensorLogs.ExternalPressureDataPIC.getMean())
                    Else
                        Return Single.NaN
                    End If
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Absolute pressure"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \mB")>
            Public ReadOnly Property ABSPressure As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.PICintPressure_absolute))
                End Get
            End Property


            <Category("Auxilary sensors"), DisplayName("Differential pressure"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \mB")>
            Public ReadOnly Property DiffPressure As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.PICintPressure_differential))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Internal voltage"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0.0 \V")>
            Public ReadOnly Property intVoltage As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.intVoltage))
                End Get
            End Property

            <Category("Auxilary sensors"), DisplayName("Recharge current"), DescriptionAttribute(""), ComponentModel.Browsable(True), CytoSense.Data.DataBase.Attributes.Format("#0 \mA")>
            Public ReadOnly Property internalRecharge As Single
                Get
                    Return CSng(MeanOrNaN(sensorLogs.internalRecharge))
                End Get
            End Property
#End Region

            ''' <summary>
            ''' The number of pictures stored recorded during a measurement.  This was added so we do not need to
            ''' load the entire file just to get the info needed for the database view in CC4. (Note this only
            ''' works of the new segmented datafiles, and not for the older complete files).
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks>The properties for the category, displayname, etc. Are still on the NumberOfPictures property in the
            ''' datawrapper.  This is for backwards compatibility.</remarks>
            <ComponentModel.Browsable(False)>
            Public Property NumberOfPictures As Integer
                Get
                    Return _numberOfPictures
                End Get
                Set(value As Integer)
                    _numberOfPictures = value
                End Set
            End Property

        End Structure

        <Serializable()> Public Class DetectorBackgroundNoiselevel
            Private _buffer As Byte()
            Dim _backgrounds() As Double
            Dim _dcRestore As New List(Of Single)

            ''' <summary>
            ''' Average background noise level in mV. Order is same as cytosensesettings.channels.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property DetectorBackgrounds As Double()
                Get
                    Return _backgrounds
                End Get
            End Property

            ''' <summary>
            ''' Contains block of data which was obtained using non-trigger modus. This block is the base of the Detectorbackgrounds, and is only kept to have a means to check the backgrounds afterwards
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property Buffer As Byte()
                Get
                    Return _buffer
                End Get
            End Property

            Public ReadOnly Property DcRestore As List(Of Single)
                Get 
                    Return _dcRestore
                End Get
            End Property


            ''' <summary>
            ''' Provide an non-triggered block of data to have the backgroundlevels of the detectors calculated.
            ''' </summary>
            ''' <param name="buffer"></param>
            ''' <param name="CytoSettings"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal buffer As Byte(), ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, dataConversions() As MeasurementSettings.LogConversion)
                _buffer = buffer
                Dim tmp As Byte()() = CytoSense.Data.splitData(buffer, CytoSettings)
                Dim chb(tmp.Length - 1)() As Byte
                For i = 0 To tmp.Length - 1
                    chb(tmp.Length - i - 1) = tmp(i)
                Next

                Dim triggerChannel(chb.Length - 1) As Boolean
                Dim numberOfDetectedTriggerChannels As Int16 = 0
                For i As Integer = 0 To chb.Length - 1
                    triggerChannel(i) = True
                    Dim triggercorrectCount As Integer = 0
                    For j As Integer = 100 To chb(i).Length - 1
                        If (chb(i)(j) < 128 Or chb(i)(j) > 191) And Not chb(i)(j) = 0 Then
                        Else
                            triggercorrectCount += 1
                        End If
                    Next

                    If (chb(i).Count-100) * 0.99 > triggercorrectCount Then
                        triggerChannel(i) = False
                    End If

                    If triggerChannel(i) = True Then
                        numberOfDetectedTriggerChannels = CShort(numberOfDetectedTriggerChannels + 1)
                    End If
                Next

                If CytoSettings.channels.Length >= 9 And numberOfDetectedTriggerChannels <> 2 Then
                    Throw New TriggerChannelDetectionFailedException
                ElseIf CytoSettings.channels.Length < 9 And numberOfDetectedTriggerChannels <> 1 Then

                    'due to the fact that Ruud and I have been creating test prints that change the triggerchannel, and at 
                    'least one of them accidentaly made it into a production instrument, this temporary fix:
                    numberOfDetectedTriggerChannels = 0
                    Dim counter191 As Integer = 0
                    For i As Integer = 0 To chb.Length - 1

                        For j As Integer = 0 To chb(i).Length - 1
                            If chb(i)(j) = 191 Then
                                counter191 += 1
                            Else
                                counter191 = 0
                            End If
                            If counter191 > 10 Then
                                triggerChannel(i) = True
                                numberOfDetectedTriggerChannels =  CShort(numberOfDetectedTriggerChannels + 1)
                                Exit For
                            End If
                        Next
                    Next

                    If CytoSettings.channels.Length < 9 And numberOfDetectedTriggerChannels <> 1 Then
                        Throw New TriggerChannelDetectionFailedException
                    End If
                ElseIf numberOfDetectedTriggerChannels > 2 Then
                    Throw New TriggerChannelDetectionFailedException()
                End If

                While Not triggerChannel(0)
                    RotateChannelsLeft(triggerChannel)
                    RotateChannelsLeft(chb)
                End While

                If numberOfDetectedTriggerChannels = 2 Then
                    Dim s As String = ""
                    For i As Integer = 1 To triggerChannel.Length - 1   'ignore first, since thats a second trigger channel
                        If triggerChannel(i) Then
                            s &= 1
                        Else
                            s &= 0
                        End If
                    Next

                    Dim tmps As String() = s.Split("1"c)
                    If tmps(0).Length < tmps(1).Length Then
                        'rotate to dat wel
                        For k As Integer = 0 To tmps(0).Length
                            RotateChannelsLeft(triggerChannel)
                            RotateChannelsLeft(chb)
                        Next
                    End If
                End If


                Dim channels(CytoSettings.channels.Length - 1) As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware
                For i As Integer = 0 To CytoSettings.channels.Length - 1
                    channels(i) = New CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware(dataConversions(i), chb(i), CytoSettings.channels(i), CytoSettings, 0)
                Next

                Dim backgrounds(channels.Length - 1) As Double
                For i As Integer = 0 To channels.Length - 1
                    If channels(i).Information.visible Then
                        backgrounds(i) = mean(channels(i).Data)
                    Else
                        backgrounds(i) = Double.NegativeInfinity
                    End If
                Next

                _backgrounds = backgrounds
            End Sub

            ''' <summary>
            ''' Individual channels, extracted from the background buffer block, after rotating and
            ''' searching for the trigger channel. Currently we only use this in CC4 for displaying
            ''' the background levels. The data is not stored.
            ''' This is copied from the orig
            ''' </summary>
            <NonSerialized> Private _bgChannels As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware()

            ''' <summary>
            ''' Get a channel with the actual data for a Ruud electronics block. NOTE all channels are calculated on
            ''' the first call and cached, the next call will just return data from the cache, calling it with other
            ''' settings or data conversions will not change that!
            ''' UCKY FUNCTION
            ''' </summary>
            ''' <param name="cytoSettings"></param>
            ''' <param name="dataConversions"></param>
            ''' <param name="chanIdx"></param>
            ''' <returns></returns>
            Public Function GetChannelDataRuud(cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, dataConversions() As MeasurementSettings.LogConversion, chanIdx As Integer) As Byte()
                If _bgChannels Is Nothing Then
                    Dim tmp As Byte()() = CytoSense.Data.splitData(_buffer, CytoSettings)
                    Dim chb(tmp.Length - 1)() As Byte
                    For i = 0 To tmp.Length - 1
                        chb(tmp.Length - i - 1) = tmp(i)
                    Next

                    Dim triggerChannel(chb.Length - 1) As Boolean
                    Dim numberOfDetectedTriggerChannels As Int16 = 0
                    For i As Integer = 0 To chb.Length - 1
                        triggerChannel(i) = True
                        Dim triggercorrectCount As Integer = 0
                        For j As Integer = 100 To chb(i).Length - 1
                            If (chb(i)(j) < 128 Or chb(i)(j) > 191) And Not chb(i)(j) = 0 Then
                            Else
                                triggercorrectCount += 1
                            End If
                        Next

                        If (chb(i).Count-100) * 0.99 > triggercorrectCount Then
                            triggerChannel(i) = False
                        End If

                        If triggerChannel(i) = True Then
                            numberOfDetectedTriggerChannels = CShort(numberOfDetectedTriggerChannels + 1)
                        End If
                    Next

                    If CytoSettings.channels.Length >= 9 And numberOfDetectedTriggerChannels <> 2 Then
                        Throw New TriggerChannelDetectionFailedException
                    ElseIf CytoSettings.channels.Length < 9 And numberOfDetectedTriggerChannels <> 1 Then

                        'due to the fact that Ruud and I have been creating test prints that change the triggerchannel, and at 
                        'least one of them accidentaly made it into a production instrument, this temporary fix:
                        numberOfDetectedTriggerChannels = 0
                        Dim counter191 As Integer = 0
                        For i As Integer = 0 To chb.Length - 1

                            For j As Integer = 0 To chb(i).Length - 1
                                If chb(i)(j) = 191 Then
                                    counter191 += 1
                                Else
                                    counter191 = 0
                                End If
                                If counter191 > 10 Then
                                    triggerChannel(i) = True
                                    numberOfDetectedTriggerChannels = CShort(numberOfDetectedTriggerChannels + 1)
                                    Exit For
                                End If
                            Next
                        Next

                        If CytoSettings.channels.Length < 9 And numberOfDetectedTriggerChannels <> 1 Then
                            Throw New TriggerChannelDetectionFailedException
                        End If
                    ElseIf numberOfDetectedTriggerChannels > 2 Then
                        Throw New TriggerChannelDetectionFailedException()
                    End If

                    While Not triggerChannel(0)
                        RotateChannelsLeft(triggerChannel)
                        RotateChannelsLeft(chb)
                    End While

                    If numberOfDetectedTriggerChannels = 2 Then
                        Dim s As String = ""
                        For i As Integer = 1 To triggerChannel.Length - 1   'ignore first, since thats a second trigger channel
                            If triggerChannel(i) Then
                                s &= 1
                            Else
                                s &= 0
                            End If
                        Next

                        Dim tmps As String() = s.Split("1"c)
                        If tmps(0).Length < tmps(1).Length Then
                            'rotate to dat wel
                            For k As Integer = 0 To tmps(0).Length
                                RotateChannelsLeft(triggerChannel)
                                RotateChannelsLeft(chb)
                            Next
                        End If
                    End If

                    Redim _bgChannels(CytoSettings.channels.Length - 1)
                    For i As Integer = 0 To CytoSettings.channels.Length - 1
                        _bgChannels(i) = New CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware(dataConversions(i), chb(i), CytoSettings.channels(i), CytoSettings, 0)
                    Next
                End If
                ' Find the requested channel, we have an index of visible channels, so we need to skip all
                ' the channels that are invisible.
                For i = 0 to _bgChannels.Length - 1
                    If _bgChannels(i).Information.visible Then
                        If chanIdx = 0 Then
                            Return _bgChannels(i).Data_Raw
                        Else 'This is not the requested channel, decrement index and try the next one.
                            chanIdx -= 1
                        End If
                    End If 'Else invisible channel
                Next
                Throw New Exception("Could not find requested channel in background data.")
            End Function


            ''' <summary>
            ''' New Background level constructor for the background levels from V10.  
            ''' not sure if we need a new class or store it in here, for now we just store it in here. :-)
            ''' </summary>
            ''' <param name="settings"></param>
            ''' <param name="dataConversions"></param>
            ''' <param name="dcRestore"></param>
            ''' <param name="backgroundBuffer"></param>
            ''' <remarks></remarks>
            Public Sub New( settings As CytoSense.CytoSettings.CytoSenseSetting, dataConversions() As MeasurementSettings.LogConversion, dcRestore As List(Of UShort), backgroundBuffer() As Byte) 
                _buffer = backgroundBuffer
                _dcRestore = New List(Of Single)()
                For i=0 To dcRestore.Count - 1
                    _dcRestore.Add( CSng(dcRestore(i)/8.192))
                Next
                ' Convert to particle some how, (maybe not need to) and calculate averages per channel for each channel
                ' Just calculate averages per channel, assume we have a complete block with 65535 samples, so we just 
                ' take the first n samples in one run.
                Dim nChannels = dataConversions.Length
                Dim sampleOffset As Integer = 0
                Dim totals(nChannels-1) As Double
                For sampleIdx = 0 To UShort.MaxValue-1
                    sampleOffset = sampleIdx * nChannels
                    For chanIdx = 0 To nChannels - 1
                        totals(chanIdx) += settings.mV_lookupNew(dataConversions(chanIdx))(_buffer(sampleOffset+chanIdx))
                    Next
                Next                
                ReDim _backgrounds(nChannels-1)
                For chanIdx = 0 To nChannels-1
                    _backgrounds(chanIdx) = totals(chanIdx)/UShort.MaxValue
                Next
            End Sub

            Private Function mean(ByRef data() As Single) As Double
                Dim tmp As Double = 0
                For i As Integer = 0 To data.Length - 1
                    tmp += data(i)
                Next
                Return tmp / data.Length
            End Function
            Private Sub RotateChannelsLeft(ByRef array() As Boolean)
                Dim tmp As Boolean = array(0)
                For i As Integer = 1 To array.Length - 1
                    array(i - 1) = array(i)
                Next
                array(array.Length - 1) = tmp
            End Sub
            Private Sub RotateChannelsLeft(ByRef array()() As Byte)
                Dim tmp() As Byte = array(0)
                For i As Integer = 1 To array.Length - 1
                    array(i - 1) = array(i)
                Next
                array(array.Length - 1) = tmp
            End Sub

        End Class

        <Serializable()> Public Structure SensorDataPointLogs
            Dim PICPreConcentration As DataPointList
            Dim PICConcentration As DataPointList

            Dim SheathFlow As DataPointList
            Dim SheathTemp As DataPointList
            Dim LaserTemp As DataPointList
            Dim PMTTemp As DataPointList
            Dim BuoyTemp As DataPointList
            Dim SystemTemp As DataPointList
            Dim HallInterrupts As DataPointList

            Dim extPressure As DataPointList
            Dim ExternalPressureDataPIC As DataPointList
            Dim extSupplyPowerVoltage As DataPointList
            Dim internalRecharge As DataPointList
            Dim internalTemperature As DataPointList
            Dim intVoltage As DataPointList
            Dim intPressure_absolute As DataPointList
            Dim intPressure_differential As DataPointList
            Dim VRefFactor As DataPointList

            Dim PICIIFTimings As DataPointList 'may be temporary


            Dim PICintPressure_absolute As DataPointList
            Dim PICintPressure_differential As DataPointList

            <OptionalField> Dim extBatteryVoltage As DataPointList
            <OptionalField> Dim BuoyExtBatteryVoltage As DataPointList ' Voltages from the 4GModem RUT955 analog input, connected to the external battery in the Buoy. The real external battery voltage.

            Dim PwrMainVoltageData As DataPointList
            Dim PwrMainCurrentData As DataPointList
            Dim PwrChargerVoltageData As DataPointList
            Dim PwrChargerCurrentData As DataPointList
            Dim PwrBatteryVoltageData As DataPointList
            Dim PwrBatteryCurrentData As DataPointList
            Dim PwrExtBatteryVoltageData As DataPointList
            Dim PwrExtBatteryCurrentData As DataPointList
            Dim PwrEmbPcVoltageData As DataPointList
            Dim PwrEmbPcCurrentData As DataPointList
            Dim PwrLaser1VoltageData As DataPointList
            Dim PwrLaser1CurrentData As DataPointList
            Dim PwrLaser2VoltageData As DataPointList
            Dim PwrLaser2CurrentData As DataPointList
            Dim PwrSpareVoltageData As DataPointList
            Dim PwrSpareCurrentData As DataPointList
            Dim PwrExtBatteryVoltageBeforeDcDcData As DataPointList

            Dim ExternalFiltersPressureData As DataPointList


            Dim Laser1DiodeTemperature As DataPointList
            Dim Laser1BaseTemperature  As DataPointList
            Dim Laser1DiodeCurrent     As DataPointList
            Dim Laser1TecLoad          As DataPointList
            Dim Laser1InputVoltage     As DataPointList
            Dim Laser1Mode             As LaserModeDataPointList



           <OnDeserializing> _ 
            Private Sub OnDes(sc As StreamingContext)
                extBatteryVoltage = New CytoSense.Data.DataPointList(DataPointList.SensorLogTypes.FTDI_ExternalBatteryVoltage)
                BuoyExtBatteryVoltage = New CytoSense.Data.DataPointList(DataPointList.SensorLogTypes.Buoy_ExternalBattery_Voltage)
            End Sub
        End Structure
        <Serializable()> Public Structure ReductionInfo
            Public WasReduced As Boolean
            Public ReductionDate As DateTime
            Public OriginalSize As Single 'MegaBytes
            Public OriginalNumber As Integer 'Original number of particles. If smarttriggered, this is the untriggered (bigger) number. Triggered number is in particles_smarttriggered
            Public SourceFileName As String
            Public TextComment As String
            <Runtime.Serialization.OptionalField()> Public IsConverted As Boolean
            <Runtime.Serialization.OptionalField()> Public OriginalFileParticleIDs() As Integer 'Zero-based indices of particles in original file

        End Structure
        ''' <summary>
        ''' Changed the locking, everything that accesses the arrays of the data needs to locks, as these things
        ''' are resized every now and then from different threads and that is not present.  For now the single
        ''' properties do not lock, they are never actually changed, so they should not cause any locking issue.
        ''' </summary>
        <Serializable()> Public Class DataPointList

            Dim data() As Double
            Dim d() As DateTime
            Dim count As Int32 = -1

            Dim _sensorType As SensorLogTypes

            'depreciated, but existing in old data files:
            Dim _xString As String
            Dim _yString As String
            Dim _description As String


            <NonSerialized> Private _lock As Object ' Used to synchronize access to this object.

            Public Sub New(t As SensorLogTypes)
                _sensorType = t
                _lock = New Object()
            End Sub

            ''' <summary>
            ''' Creates a new DataPointList, but retains the last value d. Makes for a smooth transition in CytoUSB when the lists are cleared for a new measurement
            ''' </summary>
            ''' <param name="t"></param>
            ''' <param name="d"></param>
            ''' <remarks>NOTE: Constructor does not need to lock, Me._lock as it is still the only possible thread in this object
            ''' but somebody else could be accessing d, so we need to lock d._lock.</remarks>
            Public Sub New(t As SensorLogTypes, d As DataPointList)
                Me.New(t)

                If d IsNot Nothing AndAlso d.count > 0 Then
                    SyncLock d._lock
                        Dim dat  = d.DataList(d.DataList.Count - 1).data
                        Dim time = d.DataList(d.DataList.Count - 1).time
                        add(dat, time)
                    End SyncLock
                End If

            End Sub

            ''' <summary>
            ''' Create a copy, like a copy constructor, but to avoid confusion
            ''' with the other constructor that only copies the last data item, I create
            ''' a shared function.
            ''' </summary>
            ''' <param name="d"></param>
            ''' <returns></returns>
            Public Shared Function Clone(d As DataPointList) As DataPointList
                Dim result As DataPointList = New DataPointList(d._sensorType)
                If d.count > 0 Then
                    SyncLock d._lock
                        For i = 0 To d.count - 1
                            Dim entry = d.DataList(i)
                            result.add(entry.data, entry.time)
                        Next
                    End SyncLock
                End If
                Return result
            End Function


            ' When de-serializing make sure that the lock object is initialized.
            <OnDeserialized()> _
            Private Sub OnDeserializedMethod(context As StreamingContext )
                _lock = New Object() 
            End Sub

            Public ReadOnly Property X_axis As String
                Get
                    If _sensorType = SensorLogTypes.Unknown Then
                        retrieveType()
                    End If

                    Select Case (_sensorType)
                        Case SensorLogTypes.Unknown
                            Return _xString
                        Case Else
                            Return "Time [s]" 'this seems to be the only option... 
                    End Select
                End Get
            End Property

            Public ReadOnly Property Y_axis As String
                Get
                    If _sensorType = SensorLogTypes.Unknown Then
                        retrieveType()
                    End If

                    Select Case (_sensorType)
                        Case SensorLogTypes.FTDI_ExternalVoltage
                            Return "Volt [V]"
                        Case SensorLogTypes.FTDI_Externalpressure
                            Return "Pressure [Bar]"
                        Case SensorLogTypes.FTDI_InternalVoltage
                            Return "Volt [V]"
                        Case SensorLogTypes.FTDI_VRef
                            Return "Volt [V]"
                        Case SensorLogTypes.FTDI_InternalCurrent
                            Return "Current [mA]"
                        Case SensorLogTypes.FTDI_Temperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.FTDI_Blocks
                            Return "Particles"
                        Case SensorLogTypes.FTDI_BlockAcquireTimes
                            Return "Block acquire times [ms]"
                        Case SensorLogTypes.FTDI_BlockRoundTripTimes
                            Return "Block round trip times [ms]"
                        Case SensorLogTypes.FTDI_BlockSizes
                            Return "Blocksizes {4,16,64,556}k"
                        Case SensorLogTypes.PIC_Concentration
                            Return "Count [particles/s]"
                        Case SensorLogTypes.PIC_PreConcentration
                            Return "Count [particles/s]"
                        Case SensorLogTypes.PIC_SheathTemperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.PIC_PMTTemperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.PIC_BuoyTemperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.PIC_LaserTemperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.PIC_SystemTemperature
                            Return "Temperature [°C]"
                        Case SensorLogTypes.PIC_ABSPressure
                            Return "Pressure [mBar]"
                        Case SensorLogTypes.PIC_DiffPressure
                            Return "Pressure [mBar]"
                        Case SensorLogTypes.PIC_ExternalFilterPressure
                            Return "Pressure [mbar]"
                        Case SensorLogTypes.PIC_Externalpressure
                            Return "Pressure [Bar]"
                        Case SensorLogTypes.PIC_Hall
                            Return "Sample pump passes [ms]"
                        Case SensorLogTypes.PIC_IIFTIMING
                            Return "PIC IIF Timing [μs]"
                        Case SensorLogTypes.MAIN_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.MAIN_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.CHARGE_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.CHARGE_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.BATTERY_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.BATTERY_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.EXTERNAL_BATTERY_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.EXTERNAL_BATTERY_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.EMBEDDEDPC_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.EMBEDDEDPC_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.LASER1_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.LASER1_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.LASER2_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.LASER2_POWER_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.SPARE_POWER_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.SPARE_POWER_CURRENT
                            Return "Current [mA]"
                       Case SensorLogTypes.FTDI_ExternalBatteryVoltage
                            Return "Volt [V]"                            
                       Case SensorLogTypes.Buoy_ExternalBattery_Voltage
                            Return "Volt [V]"
                       Case SensorLogTypes.EXTERNAL_BATTERY_POWER_VOLTAGE_BEFORE_DC_DC
                            Return "Volt [V]"
                       Case SensorLogTypes.SHEATH_FLOW
                            Return "Milliliter/Minute [ml/min]"
                        Case SensorLogTypes.LASER_DIODE_TEMPERATURE
                            Return "Temperature [°C]"
                        Case SensorLogTypes.LASER_BASE_TEMPERATURE
                            Return "Temperature [°C]"
                        Case SensorLogTypes.LASER_DIODE_CURRENT
                            Return "Current [mA]"
                        Case SensorLogTypes.LASER_TEC_LOAD
                            Return "Load [%]"
                        Case SensorLogTypes.LASER_INPUT_VOLTAGE
                            Return "Volt [V]"
                        Case SensorLogTypes.Unknown
                            Return _yString
                        Case Else
                            Return ""
                    End Select
                End Get
            End Property

            Public ReadOnly Property Description As String
                Get
                    If _sensorType = SensorLogTypes.Unknown Then
                        retrieveType()
                    End If

                    Select Case (_sensorType)
                        Case SensorLogTypes.FTDI_ExternalVoltage
                            Return "Wallpower voltage"
                        Case SensorLogTypes.FTDI_Externalpressure
                            Return "External pressure"
                        Case SensorLogTypes.FTDI_InternalVoltage
                            Return "Internal voltage"
                        Case SensorLogTypes.FTDI_VRef
                            Return "Internal reference voltage"
                        Case SensorLogTypes.FTDI_InternalCurrent
                            Return "Internal battery recharge current"
                        Case SensorLogTypes.FTDI_Temperature
                            Return "System temperature (FTDI)"
                        Case SensorLogTypes.FTDI_Blocks
                            Return "Data blocks endings"
                        Case SensorLogTypes.FTDI_BlockAcquireTimes
                            Return "Data blocks acquire times"
                        Case SensorLogTypes.FTDI_BlockRoundTripTimes
                            Return "Data blocks round trip times"
                        Case SensorLogTypes.FTDI_BlockSizes
                            Return "Data blocks sizes"
                        Case SensorLogTypes.PIC_Concentration
                            Return "Concentration"
                        Case SensorLogTypes.PIC_PreConcentration
                            Return "Pre-concentration"
                        Case SensorLogTypes.PIC_SheathTemperature
                            Return "Sheath temperature"
                        Case SensorLogTypes.PIC_BuoyTemperature
                            Return "Buoy temperature"
                        Case SensorLogTypes.PIC_PMTTemperature
                            Return "PMT temperature"
                        Case SensorLogTypes.PIC_LaserTemperature
                            Return "Laser temperature"
                        Case SensorLogTypes.PIC_SystemTemperature
                            Return "System temperature (PIC)"
                        Case SensorLogTypes.PIC_ABSPressure
                            Return "Absolute sheath fluid pressure"
                        Case SensorLogTypes.PIC_ExternalFilterPressure
                            Return "Pressure over external filters"
                        Case SensorLogTypes.PIC_DiffPressure
                            Return "Differential pressure over sheath pump"
                        Case SensorLogTypes.PIC_Externalpressure
                            Return "External pressure"
                        Case SensorLogTypes.PIC_Hall
                            Return "Sample pump passes"
                        Case SensorLogTypes.PIC_IIFTIMING
                            Return "PIC IIF Timing"
                        Case SensorLogTypes.MAIN_POWER_VOLTAGE
                            Return "Wallpower voltage"
                        Case SensorLogTypes.MAIN_POWER_CURRENT
                            Return "Wallpower current"
                        Case SensorLogTypes.CHARGE_POWER_VOLTAGE
                            Return "Battery charger voltage"
                        Case SensorLogTypes.CHARGE_POWER_CURRENT
                            Return "Battery charger current"
                        Case SensorLogTypes.BATTERY_POWER_VOLTAGE
                            Return "Battery voltage"
                        Case SensorLogTypes.BATTERY_POWER_CURRENT
                            Return "Battery current"
                        Case SensorLogTypes.EXTERNAL_BATTERY_POWER_VOLTAGE
                            Return "External battery voltage"
                        Case SensorLogTypes.EXTERNAL_BATTERY_POWER_CURRENT
                            Return "External battery current"
                        Case SensorLogTypes.EMBEDDEDPC_POWER_VOLTAGE
                            Return "Embeded PC voltage"
                        Case SensorLogTypes.EMBEDDEDPC_POWER_CURRENT
                            Return "Embedded PC current"
                        Case SensorLogTypes.LASER1_POWER_VOLTAGE
                            Return "Laser 1 voltage"
                        Case SensorLogTypes.LASER1_POWER_CURRENT
                            Return "Laser 1 current"
                        Case SensorLogTypes.LASER2_POWER_VOLTAGE
                            Return "Laser 2 voltage"
                        Case SensorLogTypes.LASER2_POWER_CURRENT
                            Return "Laser 2 current"
                        Case SensorLogTypes.SPARE_POWER_VOLTAGE
                            Return "Spare voltage"
                        Case SensorLogTypes.SPARE_POWER_CURRENT
                            Return "Spare current"
                       Case SensorLogTypes.FTDI_ExternalBatteryVoltage
                            Return "External Battery Voltage"
                       Case SensorLogTypes.Buoy_ExternalBattery_Voltage
                            Return "Buoy Battery Voltage"
                       Case SensorLogTypes.EXTERNAL_BATTERY_POWER_VOLTAGE_BEFORE_DC_DC
                            Return "Incoming External Battery Voltage"
                       Case SensorLogTypes.SHEATH_FLOW
                            Return "Sheath Flow"
                        Case SensorLogTypes.FTDI_ConcentrationCount
                            Return "Concentration"
                        Case SensorLogTypes.FTDI_PreConcentrationCount
                            Return "Pre-concentration"
                        Case SensorLogTypes.LASER_DIODE_TEMPERATURE
                            Return "Laser Diode Temperature"
                        Case SensorLogTypes.LASER_BASE_TEMPERATURE
                            Return "Laser Base Temperature"
                        Case SensorLogTypes.LASER_DIODE_CURRENT
                            Return "Laser Diode Current"
                        Case SensorLogTypes.LASER_TEC_LOAD
                            Return "Laser Tec Load"
                        Case SensorLogTypes.LASER_INPUT_VOLTAGE
                            Return "Laser Input Voltage"
                        Case SensorLogTypes.Unknown
                            Return _description
                        Case Else
                            Return ""
                    End Select
                End Get
            End Property

            Public Property Type As SensorLogTypes
                Get
                    Return _sensorType
                End Get
                Set(value As SensorLogTypes)
                    _sensorType = value
                End Set
            End Property


            ''' <summary>
            ''' legacy function to retrieve the correct _sensorType for old files that only had a _description serialized directly
            ''' </summary>
            ''' <remarks></remarks>
            Private Sub retrieveType()

                If _description = "Sheath flow" Then
                    _sensorType = SensorLogTypes.Unknown 'does not exist anymore
                ElseIf _description = "Sheath temperature" Then
                    _sensorType = SensorLogTypes.PIC_SheathTemperature
                ElseIf _description = "PMT temperature" Then
                    _sensorType = SensorLogTypes.PIC_PMTTemperature
                ElseIf _description = "System temperature" Then 'unfortunately this was double defined for PIC system temp pressure as well...
                    _sensorType = SensorLogTypes.FTDI_Temperature
                ElseIf _description = "Laser temperature" Then
                    _sensorType = SensorLogTypes.PIC_LaserTemperature
                ElseIf _description = "Filters pressure " Then
                    _sensorType = SensorLogTypes.PIC_DiffPressure
                ElseIf _description = "Sheath pump pressure " Then
                    _sensorType = SensorLogTypes.PIC_ABSPressure
                ElseIf _description = "Sample pump rotations" Then
                    _sensorType = SensorLogTypes.PIC_Hall
                ElseIf _description = "External pressure" Then
                    _sensorType = SensorLogTypes.FTDI_Externalpressure 'unfortunately this was double defined for PIC external pressure as well...
                ElseIf _description = "Concentration (PIC)" Then
                    _sensorType = SensorLogTypes.PIC_Concentration
                ElseIf _description = "Supply voltage" Then
                    _sensorType = SensorLogTypes.FTDI_ExternalVoltage
                ElseIf _description = "Internal battery recharge current" Then
                    _sensorType = SensorLogTypes.FTDI_InternalCurrent
                ElseIf _description = "Internal battery voltage" Then
                    _sensorType = SensorLogTypes.FTDI_InternalVoltage
                ElseIf _description = "Reference voltage" Then
                    _sensorType = SensorLogTypes.FTDI_VRef
                ElseIf _description = "Block endings" Then
                    _sensorType = SensorLogTypes.FTDI_Blocks
                    'not necessary to add new lists, as this is a legacy function!
                End If

            End Sub



            Private uncheckedValues As New List(Of Double) 'contains unchecked values.... only for use in CytoUSB to filter out strange stuff from FTDI
            ''' <summary>
            ''' Adds value, but only if checkinputOK checks out
            ''' </summary>
            ''' <param name="var"></param>
            ''' <param name="maxChange"></param>
            ''' <param name="count"></param>
            ''' <returns>checkinputOK()</returns>
            ''' <remarks>NOTE: When no data is available to calculate an average, we assume it is allright.  Else
            ''' we calculate the average of last values added, with a maximum of count.  Initially less
            ''' values will be available.</remarks>
            Public Function add(ByVal var As Double, maxChange As Double, count As Integer) As Boolean
                SyncLock _lock
                    uncheckedValues.Add(var)
                    If uncheckedValues.Count = 1 OrElse checkInputOK_LOCKED(maxChange, count, var) Then
                        add_LOCKED(var, DateTime.Now)
                        Return True
                    End If
                    Return False
                End SyncLock
            End Function

            ''' <summary>
            ''' Adds value, without input check.
            ''' </summary>
            ''' <param name="var"></param>
            Public Sub add(ByVal var As Double, time As DateTime)
                SyncLock _lock
                    add_LOCKED(var, time)
                End SyncLock
            End Sub

            '''' <summary>
            '''' Adds value, without input check. 
            '''' </summary>
            '''' <param name="var"></param>
            Public Sub add(ByVal var As Double)
                SyncLock _lock
                    add_LOCKED(var, DateTime.Now)
                End SyncLock
            End Sub


            ''' <summary>
            ''' Add a data point to the data/and time arrays.  ONLY Call this if you have
            ''' locked Me._lock. 
            ''' </summary>
            ''' <param name="var"></param>
            ''' <param name="time"></param>
            Private Sub add_LOCKED(var As Double, time As DateTime)
                If count < 0 Then
                    ReDim data(20)
                    ReDim d(20)
                    count = 0
                End If
                If count > data.Length - 1 Then
                    ReDim Preserve data(count + 100)
                    ReDim Preserve d(count + 100)
                End If
                data(count) = var
                d(count) = time
                count += 1
            End Sub


            Public Function getLast() As Double
                SyncLock _lock
                    If count >= 0 Then
                        Return data(count - 1)
                    Else
                        Return Double.NaN
                    End If
                End SyncLock

            End Function

            ''' <summary>
            ''' Get last n entries of the time information
            ''' </summary>
            ''' <param name="n"></param>
            ''' <returns></returns>
            ''' <remarks>Calculates the difference between the datetime values, in seconds.
            '''  NOTE: The function uses DateDiff, and that returns longs, so the
            ''' function actually returns only complete seconds, and not fractions, even though
            ''' it is returned as a list of doubles.
            ''' </remarks>
            Public Function getLastTime(n As Integer) As List(Of Double)
                SyncLock _lock
                    Dim res As New List(Of Double)
                    If Not Object.Equals(Nothing, d) Then
                        Dim numEntries = Math.Min(n-1,count-1)
                        Dim minIndex As Integer = count - numEntries
                        For i = minIndex To count - 1 
                           res.Add(DateDiff(DateInterval.Second, d(i - 1), d(i)))
                        Next
                        Return res
                    Else
                        Return Nothing
                    End If
                End SyncLock
            End Function


            Public Function Length() As Integer
                SyncLock _lock  ' Is not really necessary since it is a single value.  
                    Return count
                End SyncLock
            End Function

            <Obsolete("Dangerous reference to internal array, unlocked, etc. Be careful!")>
            Public ReadOnly Property DataArray As Double()
            Get
                    Return data
            End Get
            End Property

            <NonSerialized()> Private _datapoints() As datapoint
            Public ReadOnly Property DataList() As datapoint()
                Get
                    SyncLock _lock
                        If Object.Equals(_datapoints, Nothing) Or count < 0 Then
                            ReDim _datapoints(-1)  '!!! DIT HEB IK VERANDERD
                            'Return _datapoints
                        End If
                        If _datapoints.Length = count OrElse count < 0 Then
                            Return _datapoints
                        Else
                            ReDim _datapoints(count - 1)
                            For i = 0 To count - 1
                                _datapoints(i) = New datapoint(data(i), d(i))
                            Next
                            Return _datapoints
                        End If
                    End SyncLock
                End Get
            End Property

            Public Function getMean() As Double
                SyncLock _lock
                    Return mean(data, count)
                End SyncLock
            End Function

            ''' <summary>
            ''' Calculates mean of the data recorded during the specified time interval
            ''' </summary>
            ''' <param name="t"></param>
            ''' <returns></returns>
            ''' <remarks>Could be optimized further because the list is sorted on time, so we
            ''' could use a binary search to find the first one and then loop until the end, and 
            ''' stop there, but I am not sure it is worth the effort.</remarks>
            Public Function getMean(ByVal start As DateTime, ByVal t As TimeSpan) As Double
                SyncLock _lock
                    Dim total As Double = 0.0
                    Dim tmpc As Integer = 0
                    Dim endTime = start + t
                    For i = 0 To count - 1
                        If d(i) > start And d(i) <= endTime Then
                            total += data(i)
                            tmpc += 1
                        End If
                    Next
                    Return total / tmpc
                End SyncLock
            End Function

            ''' <summary>
            ''' Get mean data value last n entries
            ''' </summary>
            ''' <param name="n">number of entries to average</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function getMean(n As Integer) As Double
                SyncLock _lock
                    If Not Object.Equals(Nothing, data) Then
                        Dim total As Double
                        Dim numEntries = Math.Min(n-1,count-1)
                        Dim minIndex As Integer = count - numEntries
                        For i = minIndex To count - 1 
                            total += data(i)
                        Next
                        Return total / numEntries
                    Else
                        Return -1
                    End If
                End SyncLock
            End Function



            ''' <summary>
            ''' Get mean over last n entries
            ''' </summary>
            ''' <param name="n"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function getMeanTime(n As Integer) As Double
                SyncLock _lock
                    If Not Object.Equals(Nothing, data) Then
                        Dim total As Double
                        Dim numEntries = Math.Min(n-1,count-1)
                        Dim minIndex As Integer = count - numEntries
                        For i = minIndex To count - 1 
                            total += DateDiff(DateInterval.Second, d(i - 1), d(i))
                        Next
                        Return total / numEntries
                    Else
                        Return -1
                    End If
                End SyncLock
            End Function

            ''' <summary>
            ''' Calculates mean of the data recorded from the last x samples
            ''' </summary>
            ''' <param name="x"></param>
            ''' <returns></returns>
            ''' <remarks>It uses Min(x, num avail samples-1), so it will not crash.  If numsamples is 0,
            ''' then we cannot calculate any value, so this results in an exception!.
            ''' </remarks>
            Private Function getMeanUncheckValues_LOCKED(ByVal x As Integer) As Double
                If uncheckedValues.Count <= 1 Then
                    Throw New ArgumentException("No bytes available to calculate average!")
                End If
                Dim numSamples = Math.Min(x, uncheckedValues.Count - 1)

                Dim tot As Double
                For i = uncheckedValues.Count - 1 - numSamples To uncheckedValues.Count - 1
                    tot += uncheckedValues(i)
                Next
                Return tot / (numSamples + 1)
            End Function


            ''' <summary>
            ''' Checks if input value has smaller change then maxChange during count last samples
            ''' </summary>
            ''' <param name="maxChange"></param>
            ''' <param name="count"></param>
            ''' <param name="input"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Private Function checkInputOK_LOCKED(maxChange As Double, count As Integer, input As Double) As Boolean
                Return Math.Abs(getMeanUncheckValues_LOCKED(count) - input) < maxChange
            End Function


            ''' <summary>
            ''' Returns a formatted table of data points
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function getTable() As DataTable
                Dim dt As New DataTable(_description)

                dt.Columns.Add(New DataColumn(X_axis, GetType(System.String)))
                dt.Columns.Add(New DataColumn(Description & " " & Y_axis, GetType(Single)))
                dt.PrimaryKey = {dt.Columns(0)}

                SyncLock _lock
                    Dim res(count) As Double
                    Dim tmpc As Integer = 0
                    For i = 0 To count - 1
                        If Object.Equals(dt.Rows.Find(d(i).GetDateTimeFormats(CChar("T"))(0)), Nothing) Then
                            dt.Rows.Add({d(i).GetDateTimeFormats(CChar("T"))(0), data(i)})
                        End If
                    Next
                End SyncLock
                Return dt
            End Function

            Private Shared Function mean(ByRef data() As Double, ByVal length As Integer) As Double
                Dim tmp As Double = 0
                For i As Integer = 0 To length - 1
                    tmp += data(i)
                Next
                Return tmp / length
            End Function

            <Serializable()> Public Structure datapoint
                Public Sub New(ByVal d As Double, ByVal t As DateTime)
                    data = d
                    time = t
                End Sub
                Dim data As Double
                Dim time As DateTime
            End Structure

            Public Enum SensorLogTypes
                Unknown
                FTDI_Temperature
                FTDI_InternalCurrent
                FTDI_VRef
                FTDI_InternalVoltage
                FTDI_ExternalVoltage
                FTDI_Externalpressure
                FTDI_Blocks
                PIC_SheathTemperature
                PIC_PMTTemperature
                PIC_BuoyTemperature
                PIC_LaserTemperature
                PIC_SystemTemperature
                PIC_Concentration
                PIC_PreConcentration
                PIC_ABSPressure
                PIC_DiffPressure
                PIC_Externalpressure
                PIC_Hall
                PIC_IIFTIMING
                FTDI_BlockRoundTripTimes
                FTDI_BlockAcquireTimes
                FTDI_BlockSizes
                PIC_IIFImages
                REMOVED_PIC_FanControllerLocalTemperature
                REMOVED_PIC_FanControllerRemoteTemperature
                REMOVED_PIC_FanControllerFanSpeed
                MAIN_POWER_VOLTAGE
                MAIN_POWER_CURRENT
                CHARGE_POWER_VOLTAGE
                CHARGE_POWER_CURRENT
                BATTERY_POWER_VOLTAGE
                BATTERY_POWER_CURRENT
                EXTERNAL_BATTERY_POWER_VOLTAGE
                EXTERNAL_BATTERY_POWER_CURRENT
                EMBEDDEDPC_POWER_VOLTAGE
                EMBEDDEDPC_POWER_CURRENT
                LASER1_POWER_VOLTAGE
                LASER1_POWER_CURRENT
                LASER2_POWER_VOLTAGE
                LASER2_POWER_CURRENT
                SPARE_POWER_VOLTAGE
                SPARE_POWER_CURRENT
		        FTDI_ExternalBatteryVoltage
                Buoy_ExternalBattery_Voltage
                EXTERNAL_BATTERY_POWER_VOLTAGE_BEFORE_DC_DC
                PIC_ExternalFilterPressure
                SHEATH_FLOW
                FTDI_ConcentrationCount    ' Added to supports CS-2007-18 instrument, not stored in the sensor logs, just used for handling in CC4.
                FTDI_PreConcentrationCount ' Added to supports CS-2007-18 instrument, not stored in the sensor logs, just used for handling in CC4.
                LASER_DIODE_TEMPERATURE
                LASER_BASE_TEMPERATURE
                LASER_DIODE_CURRENT
                LASER_TEC_LOAD
                LASER_INPUT_VOLTAGE
                'when adding fields, don't forget to add descriptions in Y_axis en X_axis!
            End Enum

        End Class


        ''' <summary>
        ''' Track datapoints fo a certain state, stored in an enum, in this case the mode of a laser. So we cannot implement averages,
        ''' or all the other shit that the old datapointlist is used for.  We just track it, and store it in the file, and later we
        ''' look at it, or display it.  This is also an attempt to resturcutre it a bit, but that is tricky to do for the old
        ''' lists, because we need to stay backwards compatible.
        ''' 
        ''' I originally created a template, but that resulted in templates of templates and our mapping code to support
        ''' binary serialization on .net core and remapping stuff to the CyzFile.dll broke on that.  Because I do not plan
        ''' to add a lot of these, I am simply making it a non template structure, adn hope we will someday get to the
        ''' CytoFile format.
        ''' </summary>
        <Serializable()> Public Class LaserModeDataPointList


            Public Readonly Property Name As String
                Get
                    Return _name
                End Get
            End Property

            Public Readonly Property Description As String
                Get
                    Return _description
                End Get
            End Property


            <Serializable>
            Public Structure LaserModeDataPoint
                Public Value As LaserMode_t
                Public dt As DateTime
            End Structure

            Private _dataPoints As List(Of LaserModeDataPoint) = New List(Of LaserModeDataPoint)()
            Private Readonly _description As String
            Private Readonly _name As String

            <NonSerialized> Private _lock As Object ' Used to synchronize access to this object. Not sure we still need it, copied from the previous variant

            Public Sub New( descr As String, name As String)
                _description = descr
                _name = name
                _lock = New Object()
            End Sub

            ''' <summary>
            ''' Creates a new DataPointList, but retains the last value d. Makes for a smooth transition in CytoUSB when the lists are cleared for a new measurement
            ''' </summary>
            ''' <param name="t"></param>
            ''' <param name="d"></param>
            ''' <remarks>NOTE: Constructor does not need to lock, Me._lock as it is still the only possible thread in this object
            ''' but somebody else could be accessing d, so we need to lock d._lock.</remarks>
            Public Sub New(other As LaserModeDataPointList)
                Me.New(other.Description,other.Name)
                SyncLock other._lock
                    If other._dataPoints.Count > 0 Then
                        _dataPoints.Add(other._dataPoints(other._dataPoints.Count-1))
                    End If
                End SyncLock
            End Sub

            ''' <summary>
            ''' Create a copy, like a copy constructor, but to avoid confusion
            ''' with the other constructor that only copies the last data item, I create
            ''' a shared function. Kind of weird, and not sure we need it. Should probably make a special
            ''' function to do the copy only one value, and have the CTOR make a clone. To late for that now.
            ''' </summary>
            ''' <param name="d"></param>
            ''' <returns></returns>
            Public Shared Function Clone(other As LaserModeDataPointList ) As LaserModeDataPointList
                Dim result As LaserModeDataPointList = New LaserModeDataPointList(other.Description, other.Name)
                SyncLock other._lock
                    For dpIdx = 0 To other._dataPoints.count - 1
                        result._dataPoints.Add(other._dataPoints(dpIdx))
                    Next
                End SyncLock
                Return result
            End Function

            ' When de-serializing make sure that the lock object is initialized.
            <OnDeserialized()> _
            Private Sub OnDeserializedMethod(context As StreamingContext )
                _lock = New Object() 
            End Sub

            ''' <summary>
            ''' Adds value
            ''' </summary>
            ''' <param name="var"></param>
            Public Sub Add(val As LaserMode_t, time As DateTime)
                SyncLock _lock
                    _dataPoints.Add(New LaserModeDataPoint() With { .Value = val, .dt = time})
                End SyncLock
            End Sub

            '''' <summary>
            '''' Adds value
            '''' </summary>
            '''' <param name="var"></param>
            Public Sub Add(val As LaserMode_t)
                SyncLock _lock
                    _dataPoints.Add(New LaserModeDataPoint() With { .Value = val, .dt = DateTime.Now})
                End SyncLock
            End Sub

            Public Function GetLast() As LaserMode_t
                SyncLock _lock
                    If _dataPoints.Count > 0 Then
                        Return _dataPoints(_dataPoints.Count-1).Value
                    Else
                        Return Nothing ' Will this work for enums, the internet seems to think it will return the 0 value.
                    End If
                End SyncLock
            End Function

            Public Function Length() As Integer
                SyncLock _lock
                    Return _dataPoints.Count
                End SyncLock
            End Function

            ' I deleted most of the stuff copied from the original datapointlist that stores doubles.
            ' Will have to add stuff again later, if I need it.

        End Class


        <Serializable()> Public Class MeasurementLog

            Dim tasklog As New List(Of LogItem)

            Public Sub setTask(t As Tasks, b As BeginOrEndEnum)
                tasklog.Add(New LogItem(t, b, Now))
            End Sub

            <Serializable()> Public Enum Tasks
                FillingHPLoop = 1
                GettingSampleToLoop = 2
                Flush = 3
                LaserWarmingUp = 4
                BackFlush = 5
                Acquiring = 6
                Analysing = 7
                PreConcentration = 8
                CalibrateCamera = 9
                MultiSamplerFlush = 10
                GVFlush = 11
                InitIIF = 12
                SensorDataCleared = 13
                SetCytoSenseToMeasuringSettings = 13
                MeasureBackGroundLevels = 14
                Saving = 15
            End Enum

            <Serializable()> Public Structure LogItem
                Public Sub New(task As Tasks, b As BeginOrEndEnum, time As DateTime)
                    Me.task = task
                    Me.time = time
                    Me.BeginorEnd = b
                End Sub
                Dim task As Tasks
                Dim BeginorEnd As BeginOrEndEnum
                Dim time As DateTime

                Public Overrides Function ToString() As String
                    Return time.ToLongTimeString & ": " & task.ToString & " (" & BeginorEnd.ToString & ")"
                End Function
            End Structure

            <Serializable()> Public Enum BeginOrEndEnum
                Begining
                Ending
                NA
            End Enum


            Public Function getAqcuireDuration() As Integer
                Dim start As Date
                Dim ending As Date
                For i = 0 To tasklog.Count - 1
                    If tasklog(i).task = Tasks.Acquiring Then
                        If tasklog(i).BeginorEnd = BeginOrEndEnum.Begining Then
                            start = tasklog(i).time
                        Else
                            ending = tasklog(i).time
                            Exit For
                        End If
                    End If
                Next

                Return CInt(DateDiff(DateInterval.Second, start, ending))

            End Function

            Public Function getAcquireStart() As Date
                For i = 0 To tasklog.Count - 1
                    If tasklog(i).task = Tasks.Acquiring AndAlso tasklog(i).BeginorEnd = BeginOrEndEnum.Begining Then
                        Return tasklog(i).time
                    End If
                Next
                Return Nothing
            End Function

            Public Function getAcquireEnd() As Date
                For i = 0 To tasklog.Count - 1
                    If tasklog(i).task = Tasks.Acquiring AndAlso tasklog(i).BeginorEnd = BeginOrEndEnum.Ending Then
                        Return tasklog(i).time
                    End If
                Next
                Return Nothing
            End Function

            Public Function getTask(task As Tasks, b As BeginOrEndEnum) As LogItem
                For i = 0 To tasklog.Count - 1
                    If tasklog(i).task = task And tasklog(i).BeginorEnd = b Then
                        Return tasklog(i)
                    End If
                Next

                Throw New ItemCannotBeFoundException
            End Function

            Public Class ItemCannotBeFoundException
                Inherits Exception

            End Class
        End Class
    End Module

End Namespace

