Imports MathNet.Numerics
Imports CytoSense.MeasurementSettings
Imports CytoSense.Data.ParticleHandling.Channel
Imports CytoSense.Data.SegmentedData
Imports System.Threading
Imports CytoSense.CytoSettings

Namespace Data.ParticleHandling
    ''' <summary>
    ''' Contains all data for a CytoSense particle. Most data is calculated on demand and accessible from properties.
    ''' </summary>
    ''' <remarks>
    ''' NOTE,WARNING: DO NOT LOCK ON A PARTICLE. SyncLock Me is used internally.
    ''' </remarks>
    <Serializable()> Public Class Particle
        Private _channelData As ChannelData() 
        Private _channelData_hardware As ChannelData_Hardware()
        Private _virtualChannelData As ChannelData() 'nr (0) is gereserveerd voor FWScurvature, nr (1) voor een FLREDsplitchannel, nr (2) voor een ratiochannel (CC3)
        Public _ID As Int32
        Private _timeOfArrivalRelative As Single

        Protected _CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
        Protected _measurement As CytoSense.MeasurementSettings.Measurement
        Private _clusterInfo As Cluster

        <NonSerialized> Public Index As Integer = -1 ' to indicate it is not initialized

        'Restrain from adding additional variables, due to memory usage in big files!

        ''' <summary>
        ''' Constructor for particle when multiplexed byte is available. Only used for DSP IIF processing. 
        ''' </summary>
        ''' <param name="MultiPlexedData"></param>
        ''' <param name="ID"></param>
        ''' <param name="CytoSettings"></param>
        ''' <param name="measurement"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal MultiPlexedData As Byte(), ByVal ID As Int32, ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurement As CytoSense.MeasurementSettings.Measurement)
            Me.New(SpitChannels(MultiPlexedData, CytoSettings, False, measurement.ChannelDataConversion), ID, CytoSettings, measurement)
        End Sub

        Public Sub New(ByVal chData() As ChannelData_Hardware, ByVal ID As Int32, ByRef cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByRef measurement As CytoSense.MeasurementSettings.Measurement)
            _ID = ID
            _channelData_hardware = chData
            _CytoSettings = cytoSettings
            _measurement = measurement
        End Sub

        Public Sub New(other As Particle)
            _ID = other._ID
            Index = other.Index
            _channelData_hardware = other._channelData_hardware
            _CytoSettings = other._CytoSettings
            _measurement = other._measurement
            _timeOfArrivalRelative = other._timeOfArrivalRelative
        End Sub

        Public Sub New(p As RawParticle, ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurement As CytoSense.MeasurementSettings.Measurement, start As DateTime)
            _CytoSettings = cytoSettings
            _measurement = measurement
            _ID = p.ID
            _clusterInfo = p.ClusterInfo

            ReDim _channelData_hardware(p.ChannelData.Length - 1)

            _timeOfArrivalRelative = CSng((p.TimeOfArrival - start).TotalMilliseconds / 1000.0)

            For i = 0 To _channelData_hardware.Length - 1
                _channelData_hardware(i) = New ChannelData_Hardware(_measurement.ChannelDataConversion(i), p.ChannelData(i), cytoSettings.channels(i), cytoSettings, TimeOfArrivalRelative)
            Next
        End Sub

        ''' <summary>
        ''' Test function, calculate all parameter for all channels. (Except SWS Covariance for now).
        ''' </summary>
        Public Sub PrecalculateAllParameters()
            For j = 0 To ChannelData.Length - 1
                ChannelData(j).SetParametersFast()
            Next
        End Sub

        Public Sub PrecalculateParameters(mask As Boolean())
            For j = 0 To ChannelData.Length - 1
                ChannelData(j).SetParametersFast(mask)
            Next
        End Sub

        Public Property ClusterInfo As Cluster
            Get
                Return _clusterInfo
            End Get
            Set(value As Cluster)
                _clusterInfo = value
            End Set
        End Property

        ''' <summary>
        ''' 'Array length in samples, so not the CytoClus length feature!
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Length As Integer
            Get
                Return _channelData_hardware(0).Data_Raw.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return "#samples: " & Length
        End Function

        ''' <summary>
        ''' Transition Of Focus of this particle: the total length of the signal in um.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TOF() As Single
            Get
                Return (_channelData_hardware(0).Data_Raw.Length - 1) * _CytoSettings.Sample_to_um_ConversionFactor

            End Get
        End Property

        ''' <summary>
        ''' Contains the absolute time the particle arrived
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TimeOfArrivalAbsolute(start As DateTime) As DateTime
            Get
                Return start.Add(New TimeSpan(0, 0, 0, 0, CInt(_timeOfArrivalRelative * 1000)))
            End Get
        End Property

        ''' <summary>
        ''' In seconds
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TimeOfArrivalRelative As Single
            Get
                Return _timeOfArrivalRelative
            End Get
        End Property

        Public Sub setArrivalTime(timeOfArrivalRelative As Single)
            _timeOfArrivalRelative = timeOfArrivalRelative

            If _channelData IsNot Nothing Then
                For i = 0 To _channelData.Length - 1
                    _channelData(i).TimeOfArrival = _timeOfArrivalRelative
                Next
            End If

            If _channelData_hardware IsNot Nothing Then
                For i = 0 To _channelData_hardware.Length - 1
                    _channelData_hardware(i).TimeOfArrival = _timeOfArrivalRelative
                Next
            End If
        End Sub

        Public ReadOnly Property CytoSettings As CytoSense.CytoSettings.CytoSenseSetting
            Get
                Return _CytoSettings
            End Get
        End Property

        Public ReadOnly Property MeasurementSettings As CytoSense.MeasurementSettings.Measurement
            Get
                Return _measurement
            End Get
        End Property

        Public Function getDataForAxis(dimension As Analysis.Axis) As Single
            Return dimension.GetValue(Me)
        End Function

        ''' <summary>
        ''' Creates multiplexed byte data, as like it would come directly from the CytoSense hardware
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Copy of createMultiPlexedByteData from CytoSense.Data</remarks>
        Public Function getMultiPlexedByteData() As Byte()
            Dim syncs(_CytoSettings.channels.Length - 1) As Byte     'in some older files the syncpulses were put wrong in the machine configuration file

            For i = 0 To syncs.Length - 1
                syncs(i) = BitConverter.GetBytes(_CytoSettings.channels(i).SyncPulseValue)(0)   'copy the good ones
            Next

            Dim multiPlexedData As New List(Of Byte)

            For k = 0 To ChannelData_Hardware(0).Data_Raw.Length - 1
                For channelID = 0 To ChannelData_Hardware.Length - 1
                    multiPlexedData.Add(ChannelData_Hardware(channelID).Data_Raw(k))
                Next
            Next

            For i = 0 To 4
                For channelID = 0 To ChannelData_Hardware.Length - 1
                    multiPlexedData.Add(syncs(channelID))
                Next
            Next

            Return multiPlexedData.ToArray
        End Function

        ''' <summary>
        ''' V10 has a new multiplexed format, it consists of the actual particle data,
        ''' followed by a single trailer byte with he value 252, then a channel number, and
        ''' finally 2 bytes with the particle number.  So we have 4 bytes of trailer per
        ''' channel, i.e. numChannels * 4 bytes appended
        ''' </summary>
        ''' <param name="counter"></param>
        ''' <returns></returns>
        Public Function getMultiPlexedByteData_FJ(counter As UInt64) As Byte()
            Dim partNum As UShort = CUShort(counter Mod &HFFFF)
            Dim partNumHigh As Byte = CByte(partNum >> 8)
            Dim partNumLow As Byte = CByte(partNum And &HFF)

            Dim numChannels = ChannelData_Hardware.Length
            Dim numSamples = ChannelData_Hardware(0).Data_Raw.Length
            Dim data((numChannels * (numSamples + 4)) - 1) As Byte
            Dim trailerStart = numChannels * numSamples

            Dim dataIdx As Integer = 0

            For sampleIdx = 0 To numSamples - 1
                For chanIdx = 0 To numChannels - 1
                    data(dataIdx) = ChannelData_Hardware(chanIdx).Data_Raw(sampleIdx)
                    dataIdx += 1
                Next
            Next

            For chanIdx = 0 To numChannels - 1
                data(trailerStart + chanIdx) = 252
                data(trailerStart + 1 * numChannels + chanIdx) = CByte(chanIdx)
                data(trailerStart + 2 * numChannels + chanIdx) = partNumHigh
                data(trailerStart + 3 * numChannels + chanIdx) = partNumLow
            Next

            Return data
        End Function

        ''' <summary>
        ''' Represents a unique number for this particle in this file. 
        ''' </summary>
        Public ReadOnly Property ID As Int32
            Get
                Return _ID
            End Get
        End Property

        ''' <summary>
        ''' Only to be used by parallel implementation of split toparticles
        ''' </summary>
        ''' <param name="id"></param>
        ''' <remarks></remarks>
        Public Sub setID(id As Int32)
            _ID = id
        End Sub

        Public ReadOnly Property ChannelData_Hardware() As ChannelData_Hardware()
            Get
                Return _channelData_hardware
            End Get
        End Property

        ''' <summary>
        ''' Returns channel data for a channel that was defined later in software. 0 = FWScurvature, 1 = flredsplit, 2 = ratiochannel
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>0 is only available if the machine is curvature enabled, 1 and 2 need to be initialized in CytoClus!</remarks>
        Public ReadOnly Property ChannelData_Virtual() As ChannelData()
            Get
                If Object.Equals(Nothing, _virtualChannelData) Then
                    initVirtualChannels()
                End If

                Return _virtualChannelData
            End Get
        End Property

        Public Enum VirtualChannelIDs
            FWSCurvature = 0
            FlRedSplit = 1
            Ratiochannel = 2
        End Enum

        ''' <summary>
        ''' Summed Spike count for all hardware channels.
        ''' Spike counts are not cached
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRawSpikeCount() As Integer
            Dim spikeCount = 0

            For i = 0 To _channelData_hardware.Length - 1
                If _channelData_hardware(i).Information.visible Then
                    spikeCount += _channelData_hardware(i).GetRawSpikeCount()
                End If
            Next

            Return spikeCount
        End Function

        ''' <summary>
        ''' Summed Spike count for all hardware channels, for filtered data!
        ''' </summary>
        ''' <returns></returns>
        Public Function GetFilteredSpikeCount() As Integer
            Dim spikeCount = 0

            For i = 0 To _channelData_hardware.Length - 1
                If _channelData_hardware(i).Information.visible Then
                    spikeCount += _channelData_hardware(i).GetFilteredSpikeCount()
                End If
            Next

            Return spikeCount
        End Function

        <NonSerialized()> Private _curChannelAccessMode As CytoSettings.ChannelAccessMode = CytoSense.CytoSettings.ChannelAccessMode.Normal
        Public ReadOnly Property currentChannelAccessMode() As CytoSettings.ChannelAccessMode
            Get
                Return _curChannelAccessMode
            End Get
        End Property

        Private Function GenerateChannelData() As ChannelData()
            Dim output As New List(Of ChannelData)(12) 'Preallocate a capacity to improve memory performance

            For i = 0 To _CytoSettings.ChannelList.Count - 1 ' get a list of channels to be created from the cytosettings 
                If _CytoSettings.ChannelList(i).IsHWChannel Then
                    'if the channel is a hardware channel, it can just be copied from the hardware channel list
                    output.Add(ChannelData_Hardware(_CytoSettings.ChannelList(i).HW_channel_id))
                Else
                    'but if it is a virtual channel, it needs to be created:
                    Select Case _CytoSettings.ChannelList(i).VirtualChannelType
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.FWS_L_plus_R
                            output.Add(ChannelData_Virtual(VirtualChannelIDs.FWSCurvature))
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.Curvature_L_div_R
                            output.Add(New ChannelData_Curvature(_CytoSettings, DirectCast(ChannelData_Virtual(VirtualChannelIDs.FWSCurvature),ChannelData_FWSCurvature), DirectCast(_CytoSettings.ChannelList(i).ChannelInfo, VirtualChannelInfo), TimeOfArrivalRelative))
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.DualFocusLeft
                            Dim baseChannelID As Integer = CType(_CytoSettings.ChannelList(i).ChannelInfo, CytoSettings.VirtualDualFocusChannelInfo).HWChannelIndex
                            Dim dualfocuschannel As New ChannelData_DualFocus(_CytoSettings, ChannelData_Hardware(baseChannelID))
                            Dim thishalf As New ChannelData_DualFocusHalf(_CytoSettings, dualfocuschannel, DirectCast(_CytoSettings.ChannelList(i).ChannelInfo,VirtualChannelInfo), ChannelData_DualFocus.LeftOrRightHalf.LeftHalf)
                            output.Add(thishalf)
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.DualFocusRight
                            Dim baseChannelID As Integer = CType(_CytoSettings.ChannelList(i).ChannelInfo, CytoSettings.VirtualDualFocusChannelInfo).HWChannelIndex
                            Dim dualfocuschannel As New ChannelData_DualFocus(_CytoSettings, ChannelData_Hardware(baseChannelID))
                            Dim thishalf As New ChannelData_DualFocusHalf(_CytoSettings, dualfocuschannel, DirectCast(_CytoSettings.ChannelList(i).ChannelInfo, VirtualChannelInfo), ChannelData_DualFocus.LeftOrRightHalf.RightHalf)
                            output.Add(thishalf)
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.HF_plus_LF_Summed
                            Dim tmp As CytoSettings.VirtualHFSummedChannelInfo = DirectCast(_CytoSettings.ChannelList(i).ChannelInfo,VirtualHFSummedChannelInfo)
                            output.Add(New ChannelData_HFSubtractChannel(_CytoSettings, ChannelData_Hardware(tmp.HF_HardwareChannelIndex), ChannelData_Hardware(tmp.LF_HardwareChannelIndex), tmp))
                        Case CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.LF_Filtered
                            Dim tmp As CytoSettings.VirtualLFFilteredChannelInfo = DirectCast(_CytoSettings.ChannelList(i).ChannelInfo,VirtualLFFilteredChannelInfo)
                            output.Add(New ChannelData_LFFilteredChannel(_CytoSettings, ChannelData_Hardware(tmp.LF_HardwareChannelIndex), ChannelData_Hardware(tmp.HF_HardwareChannelIndex), tmp))
                        Case Else
                            Throw New NotImplementedException("Not yet implemented")
                    End Select
                End If
            Next
            Return output.ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>
        ''' To protect (re)generating the channel data, a synclock is required. However, no internal candidate can be used because all objects in particle can be nothing. 
        ''' Creating a new object just for locking, which is the normal solution, costs 32 bytes per particle, which is adds up significantly in case of big measurements.
        ''' </remarks>
        Public Overloads ReadOnly Property ChannelData() As ChannelData()
            Get
                If currentChannelAccessMode <> _CytoSettings.getChannelVisualisationMode Then
                    _channelData = Nothing
                End If
                If _channelData Is Nothing Then
                    SyncLock Me
                        If _channelData Is Nothing Then
                            _curChannelAccessMode = _CytoSettings.getChannelVisualisationMode
                            Dim tmpChannelData = GenerateChannelData()
                            ' Set SWS channel ID for the SWS Covariance parameter...
                            Dim id As Integer = _CytoSettings.getChannellistItemByType(CytoSense.CytoSettings.ChannelTypesEnum.SWS).HW_channel_id
                            For i = 0 To tmpChannelData.Length - 1
                                tmpChannelData(i).sws = _channelData_hardware(id)
                            Next
                            Thread.MemoryBarrier() ' Make sure no memory write reordering happens here.
                            _channelData = tmpChannelData
                        End If ' Else data was allready updated by other thread.
                    End SyncLock
                End If
                Return _channelData
            End Get
        End Property

        ''' <summary>
        ''' The function will force eawag filtering of all the visible hardware data, and of the curvature channel if it is 
        ''' present.  It will generate the channel data as well if that is not present.
        ''' NOTE: Very hardcoded to EAWAG machine
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub EawagFilterChannelData()
            Dim numHardwareChannels = _channelData_hardware.Length
            Dim spikeChannel As Integer = -1
            Dim spikeIdx As Integer = -1
            Dim spikeScore As Double = Double.MinValue

            For chanIdx As Integer = 0 To numHardwareChannels - 1
                Dim ch = _channelData_hardware(chanIdx)

                If ch.Information.visible Then
                    Dim si = ch.FindMostLikelySpike()

                    If si.Score > spikeScore Then
                        spikeChannel = chanIdx
                        spikeIdx = si.Index
                        spikeScore = si.Score
                    End If
                End If
            Next

            If spikeChannel < 0 Then
                Return 'No Spike found in this particle, so nothing to filter.
            End If

            Dim numSamples = _channelData_hardware(0).Data_Raw.Length
            Dim currChannelIdx = spikeChannel
            Dim currSpikeIdx = spikeIdx

            While currChannelIdx < numHardwareChannels
                Dim ch = _channelData_hardware(currChannelIdx)

                If ch.Information.visible Then
                    ch.ForceEawagSpikeFilter(currSpikeIdx)
                    If ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSL OrElse ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSR Then
                        Dim chFwsCombined = DirectCast(getChannelByType(CytoSense.CytoSettings.ChannelTypesEnum.FWS), ChannelData_FWSCurvature)
                        chFwsCombined.ForceEawagSpikeFilter(ch.Information.Channel_Type, currSpikeIdx)
                    End If
                End If 'Else invisible channel, do not filter.

                currChannelIdx += 2
                currSpikeIdx += 6

                If currSpikeIdx >= numSamples Then
                    currSpikeIdx -= 31
                End If
            End While

            currChannelIdx = spikeChannel - 2
            currSpikeIdx = spikeIdx - 6

            If currSpikeIdx < 0 Then
                currSpikeIdx += 31
            End If

            While currChannelIdx >= 0
                Dim ch = _channelData_hardware(currChannelIdx)

                If ch.Information.visible Then
                    ch.ForceEawagSpikeFilter(currSpikeIdx)
                    If ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSL OrElse ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSR Then
                        Dim chFwsCombined = DirectCast(getChannelByType(CytoSense.CytoSettings.ChannelTypesEnum.FWS), ChannelData_FWSCurvature)
                        chFwsCombined.ForceEawagSpikeFilter(ch.Information.Channel_Type, currSpikeIdx)
                    End If
                End If 'Else invisible channel, do not filter.

                currChannelIdx -= 2
                currSpikeIdx -= 6

                If currSpikeIdx < 0 Then
                    currSpikeIdx += 31
                End If
            End While
        End Sub

        Private Shared Function ChannelTypeMatches(ch As ChannelData, tp As CytoSettings.ChannelTypesEnum) As Boolean
            If ch.Information.Channel_Type = tp Then
                Return True
            ElseIf tp = CytoSense.CytoSettings.ChannelTypesEnum.FWSLR AndAlso
                    (ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWS OrElse
                     ch.Information.Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSL) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Find a channel by type match. Returns the first match (in case of HS/LS channels this might be undesired).
        ''' May return nothing if the channel is not present or if the visualisationmode is not suitable.
        ''' </summary>
        Public Function getChannelByType(ByVal channeltype As CytoSettings.ChannelTypesEnum) As ChannelData
            Dim cd = Array.Find(ChannelData(), Function(chan As ChannelData) ChannelTypeMatches(chan, channeltype))

            If cd Is Nothing Then 'Check if we can find the channel in the hardware channel data as a fallback.  To allow acces to FWS L and FWS R regardless of the visualisation mode. 
                cd = Array.Find(ChannelData_Hardware(), Function(chan As ChannelData) ChannelTypeMatches(chan, channeltype))
            End If

            Return cd
        End Function

        ''' <summary>
        ''' NOTE: If we are in normal visualization mode, we still need to be able to handle
        ''' FWS-L and FWS-R queries for the alignment plot. This is REALLY UGLY, in that case
        ''' THe boolean included in the results indicates it is an index in the
        ''' ChannelData_Hardware instead of in the ChannelData array.
        ''' </summary>
        ''' <param name="channelName"></param>
        ''' <returns></returns>
        Public Function GetChannelIndexByName(channelName As String) As Tuple(Of Boolean, Integer)
            For i = 0 To ChannelData.Length - 1
                If ChannelData(i).Information.name = channelName Then
                    Return Tuple.Create(False, i)
                End If
            Next

            ' Check if we can find the channel in the hardware channel data as a fall back.
            ' To allow access to FWS L and FWS R regardless of the visualization mode.

            For i = 0 To ChannelData_Hardware.Length - 1
                If ChannelData_Hardware(i).Information.name = channelName Then
                    Return Tuple.Create(True, i)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Determine the channelData array and channelIndex for a channelType
        ''' the type of a channel is derived by matching the channel on its name 
        ''' (function NameToChannelType is called) 
        ''' </summary>
        ''' <param name="channelType"></param>
        ''' <returns></returns>
        Public Function GetChannelIndexByType(channelType As CytoSettings.ChannelTypesEnum) As Tuple(Of Boolean, Integer)
            For i = 0 To ChannelData.Length - 1
                If ChannelData(i).Information.Channel_Type = channelType Then
                    Return Tuple.Create(False, i)
                End If
            Next

            For i = 0 To ChannelData_Hardware.Length - 1
                If ChannelData_Hardware(i).Information.Channel_Type = channelType Then
                    Return Tuple.Create(True, i)
                End If
            Next

            Return Nothing
        End Function

        Public Function getChannelByName(chanName As String) As ChannelData
            For i = 0 To ChannelData.Length - 1
                Dim c = ChannelData(i)
                If c.Information.name = chanName Then
                    Return c
                End If
            Next

            ' Check if we can find the channel in the hardware channel data as a fall back.
            ' To allow access to FWS L and FWS R regardless of the visualization mode.
            For i = 0 To ChannelData_Hardware.Length - 1
                Dim c = ChannelData_Hardware(i)
                If c.Information.name = chanName Then
                    Return c
                End If
            Next

            Return Nothing
        End Function

        Private Sub initVirtualChannels()
            If _CytoSettings.hasCurvature Then
                ReDim _virtualChannelData(0)
                Dim fwsLeftIdx = -1
                Dim fwsRightIdx = -1

                For i = 0 To _CytoSettings.channels.Length - 1
                    If _CytoSettings.channels(i).Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSL Then
                        fwsLeftIdx = i

                        If fwsRightIdx = -1 Then
                            Continue For
                        Else
                            Exit For
                        End If
                    End If

                    If _CytoSettings.channels(i).Channel_Type = CytoSense.CytoSettings.ChannelTypesEnum.FWSR Then
                        fwsRightIdx = i

                        If fwsLeftIdx = -1 Then
                            Continue For
                        Else
                            Exit For
                        End If
                    End If
                Next
                _virtualChannelData(0) = New ChannelData_FWSCurvature(_measurement.ChannelDataConversion(fwsLeftIdx), _channelData_hardware(fwsLeftIdx), _channelData_hardware(fwsRightIdx), _CytoSettings)
            End If

            If _CytoSettings.hasDualLaserDistance Or _CytoSettings.name = "Izasa Acciona" Then 'Oh the legacy...
                addFLREDsplitVirtualChannel(_CytoSettings, Me.ChannelData_Hardware(_CytoSettings.getChannelIndex("FL red")))
            End If
        End Sub

        ''' <summary>
        ''' Specialized function for splitting up flred channel with double spaced laser setup
        ''' </summary>
        ''' <param name="cytosettings"></param>
        ''' <param name="flredChannel"></param>
        ''' <remarks></remarks>
        Public Sub addFLREDsplitVirtualChannel(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ByRef flredChannel As ChannelData_Hardware)
            If Object.Equals(_virtualChannelData, Nothing) OrElse _virtualChannelData.Length <= 1 Then
                ReDim Preserve _virtualChannelData(1)
            End If

            _virtualChannelData(1) = New ChannelData_DualFocus(cytosettings, flredChannel)
        End Sub

        ''' <summary>
        ''' Only used by CC3. Needs a thorough look before it can be used in cc4
        ''' </summary>
        Public Sub addRatioChannel(ByRef firstChannel As ChannelData, ByRef secondChannel As ChannelData, ByRef operation As ChannelData_ratioChannel.channelOperator)
            If Object.Equals(_virtualChannelData, Nothing) OrElse _virtualChannelData.Length <> 3 Then
                ReDim Preserve _virtualChannelData(2)
            End If

            _virtualChannelData(2) = New ChannelData_ratioChannel(_CytoSettings, firstChannel, secondChannel, operation)
        End Sub

        Public Sub removeRatioChannel()
            ReDim Preserve _virtualChannelData(1)
        End Sub

        ''' <summary>
        ''' Interpolates all channels to preferred size, adds them to one big vector
        ''' </summary>
        ''' <param name="PreferedInterPolateLength">Interpolation length. Set to 0 to disable interpolation</param>
        ''' <param name="cutZeros">Due to the smoothing process used, the first and last data sample are always pulled to zero, if visualization is not in order they can cut off for speed</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ParticleVector(ByVal PreferedInterPolateLength As Int32, normalize As NormalizeMode, cutZeros As Boolean) As Double()

            Dim numberOfSelectedChannels As Integer = ChannelData.Length
            Dim res() As Double

            If PreferedInterPolateLength = 0 Then
                ReDim res(ChannelData(0).Data.Length * numberOfSelectedChannels - 1)
            Else
                ReDim res(PreferedInterPolateLength * numberOfSelectedChannels - 1)
            End If

            Dim count As Integer = 0

            For i = 0 To ChannelData.Length - 1
                'convert to double
                Dim tmpdouble(ChannelData(i).Data.Length - 1) As Double

                For j = 0 To tmpdouble.Length - 1
                    tmpdouble(j) = ChannelData(i).Data(j)
                Next

                If cutZeros Then
                    If ChannelData(i).Data.Length > 2 Then
                        Dim tmp2double(ChannelData(i).Data.Length - 3) As Double
                        Array.ConstrainedCopy(tmpdouble, 1, tmp2double, 0, tmp2double.Length)
                        tmpdouble = tmp2double
                    Else
                        'Particle sizes of < 2 cannot exist, so this a data artifact. Ignore it
                    End If
                End If

                If PreferedInterPolateLength = 0 Then
                    Array.ConstrainedCopy(tmpdouble, 0, res, count, tmpdouble.Length)
                    count += tmpdouble.Length
                Else
                    Dim tmp() As Double = Interpolate(tmpdouble, PreferedInterPolateLength)

                    Array.ConstrainedCopy(tmp, 0, res, count, tmp.Length)
                    count += PreferedInterPolateLength
                End If
            Next

            If normalize = NormalizeMode.maximize_between_zero_to_one Then
                Dim max As Double = 0

                For j = 0 To res.Length - 1
                    If max < res(j) Then
                        max = res(j)
                    End If
                Next

                For j = 0 To res.Length - 1
                    res(j) = res(j) / max
                Next
            ElseIf normalize = NormalizeMode.linear_map_between_zero_to_one Then
                For j = 0 To res.Length - 1
                    res(j) = res(j) / 5000
                Next
            End If

            Return res
        End Function

        ''' <summary>
        ''' Calculate a separate fft of each channel, append to one big vector
        ''' </summary>
        ''' <param name="fftsamples">Determines how many samples will be kept in result fft per channel. Zeros are padded when fft was to small, or fft is cutoff if too big</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function ParticleVector_fft(ByVal fftsamples As Int32) As Double()
            Dim numberOfSelectedChannels As Integer = numberOfVisibleChannels

            Dim res(fftsamples * numberOfSelectedChannels - 1) As Double
            Dim count As Integer = 0
            For i = 0 To ChannelData_Hardware.Length - 1
                If ChannelData_Hardware(i).Information.visible Then

                    'remove dc component and copy data to complex array
                    'dc component needs removing because of blinding effect in sensors
                    Dim tmpcomplex(_channelData_hardware(i).Data_mV_unsmoothed.Length - 1) As System.Numerics.Complex
                    Dim sum As Double = 0
                    For j = 0 To tmpcomplex.Length - 1
                        sum += _channelData_hardware(i).Data_mV_unsmoothed(j)
                    Next
                    sum /= tmpcomplex.Length
                    For j = 0 To tmpcomplex.Length - 1
                        tmpcomplex(j) = _channelData_hardware(i).Data_mV_unsmoothed(j) - sum
                    Next

                    'perform fft
                    MathNet.Numerics.IntegralTransforms.Fourier.Forward(tmpcomplex, IntegralTransforms.FourierOptions.Matlab)

                    'append zeros if fft is smaller then fftsamples, or cutoff if bigger
                    Dim tmpdouble(fftsamples - 1) As Double
                    Dim stopat As Integer
                    If fftsamples > Math.Ceiling(tmpcomplex.Length / 2) Then
                        stopat = CInt(Math.Ceiling(tmpcomplex.Length / 2))
                    Else
                        stopat = fftsamples
                    End If

                    'calc abs and put back in doubles.
                    For j = 0 To stopat - 1
                        tmpdouble(j) = tmpcomplex(j).Magnitude
                        If tmpdouble(j) = 0 Then
                            tmpdouble(j) = 0
                        Else
                            tmpdouble(j) = Math.Log10(tmpdouble(j))
                        End If

                    Next

                    'copy to big vector
                    Array.ConstrainedCopy(tmpdouble, 0, res, count, tmpdouble.Length)
                    count += tmpdouble.Length
                End If
            Next

            'normalize to 1
            Dim max As Double = 0
            For i = 0 To res.Length - 1
                If Math.Abs(res(i)) > max Then
                    max = Math.Abs(res(i))
                End If
            Next

            If max > 0 Then
                For i = 0 To res.Length - 1
                    res(i) = res(i) / max
                Next
            End If

            Return res
        End Function

        ''' <summary>
        ''' Interpolates all channels to preferred size, adds them to one big vector.
        ''' </summary>
        ''' <param name="PreferedInterPolateLength">This option allows for strechting/reducing of the pulsehape part of the particlevector to the specified length. Set to 0 to disable.</param>
        ''' <param name="normalize">This option allows for normalisation of the pulsehape part of the particlevector</param>
        ''' <param name="mode">Allows for selection of which parts are included in the returned particlevector</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ParticleVector(ByVal PreferedInterPolateLength As Int32, ByVal mode As VectorMode, normalize As NormalizeMode) As Double()
            If mode = VectorMode.All Then
                Dim res(PreferedInterPolateLength * numberOfVisibleChannels - 1 + CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterNames.Length * numberOfVisibleChannels) As Double
                Dim pVec() As Double = ParticleVector(PreferedInterPolateLength, normalize, True)
                Dim pars As Double() = getParameterVector()
                Array.ConstrainedCopy(pVec, 0, res, 0, pVec.Length)
                Array.ConstrainedCopy(pars, 0, res, pVec.Length, pars.Length)
                Return res
            ElseIf mode = VectorMode.Channel Then
                Dim pVec As Double() = ParticleVector(PreferedInterPolateLength, normalize, True)
                Return pVec
            ElseIf mode = VectorMode.fft Then
                Dim tmp As Double() = ParticleVector_fft(PreferedInterPolateLength)
                Return tmp
            Else
                Return getParameterVector()
            End If
        End Function

        Private Function correlation_function(ByRef d As Double()) As Double()
            Dim res(d.Length - 1) As Double

            For k = 0 To d.Length - 1
                Dim sum As Double = 0

                For i = 0 To d.Length - 1 - k
                    sum = sum + d(i) * d(i + k)
                Next i

                res(k) = (1 / (d.Length - k)) * sum
            Next k

            Return res
        End Function

        ''' <summary>
        ''' Puts all CytoClus parameters in an array
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function getParameterVector() As Double()
            Dim pars(CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterNames.Length * numberOfVisibleChannels - 1) As Double
            Dim count As Integer = 0

            For i = 0 To ChannelData_Hardware.Length - 1
                If ChannelData_Hardware(i).Information.visible Then
                    Dim par(CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterNames.Length - 1) As Double
                    Dim ch As ChannelData

                    If _CytoSettings.hasCurvature And ChannelData_Hardware(i).Information.name.ToLower.StartsWith("fws") Then
                        ch = _virtualChannelData(0)
                        i += 2
                    Else
                        ch = ChannelData_Hardware(i)
                    End If

                    For k = 0 To CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterNames.Length - 1
                        par(k) = Math.Log10(ch.Parameter(CType(k,ChannelData.ParameterSelector)))

                        If Double.IsInfinity(par(k)) Or Double.IsNaN(par(k)) Then
                            par(k) = 0
                        End If
                    Next

                    Array.ConstrainedCopy(par, 0, pars, count * par.Length, par.Length)
                    count += 1
                End If
            Next

            Return pars
        End Function

        Public Enum VectorMode
            All
            Channel
            parameters
            fft
        End Enum

        Public Enum NormalizeMode
            None
            linear_map_between_zero_to_one '0-5000mV -> 0-1
            maximize_between_zero_to_one
        End Enum

        Public ReadOnly Property numberOfVisibleChannels As Integer
            Get
                Dim count As Integer = 0

                For i = 0 To ChannelData_Hardware.Length - 1
                    If ChannelData_Hardware(i).Information.visible Then
                        count += 1
                    End If
                Next

                Return count
            End Get
        End Property

        Private Function Interpolate(ByVal ChData As Double(), ByVal MeanPartLength As Double) As Double()

            If ChData.Length < 4 Then
                Dim res_fout(CInt(MeanPartLength - 1)) As Double
                Return res_fout
            End If

            Dim b As New System.Collections.Generic.List(Of Double)
            Dim T() As Double = createTArray(ChData.Length)

            Dim res(CInt(MeanPartLength - 1)) As Double
            Dim inpo = Interpolation.LinearSpline.Interpolate(T, ChData)

            For j = 0 To MeanPartLength - 1
                res(CInt(j)) = inpo.Interpolate(j / (MeanPartLength / ChData.Length))
            Next

            Return res
        End Function

        Private Function createTArray(ByVal length As Int32) As Double()
            Dim res(length - 1) As Double

            For i = 0 To res.Length - 1
                res(i) = i
            Next

            Return res
        End Function

        Private Function getchannelid(ByVal name As String) As Integer
            For i = 0 To ChannelData_Hardware.Length - 1
                If ChannelData_Hardware(i).Information.name = name Then
                    Return i
                End If
            Next

            Return -1
        End Function

        ''' <summary>
        ''' Returns a list of singular values of this particle. The SVD is done over a matrix with the rows as channels 
        ''' and the columns as time instants.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property getSVD() As MathNet.Numerics.LinearAlgebra.Factorization.Svd(Of Complex32)
            Get
                Dim channelmat As New MathNet.Numerics.LinearAlgebra.Complex32.DenseMatrix(ChannelData.Length, ChannelData(1).Data.Length)

                For row As Integer = 0 To ChannelData.Length - 1
                    For column As Integer = 0 To ChannelData(1).Data.Length - 1
                        channelmat.Item(row, column) = ChannelData(row).Data(column)
                    Next
                Next

                Return channelmat.Svd
            End Get
        End Property

        ''' <summary>
        ''' Compares two particles by means of their DSPChannel data (SWS in most cases). Only returns true if the data matches exactly.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Original access Data as well as Data_Raw, causing the smoothing to run during the loading
        ''' of the file, even if not required. Changed this to only use data raw for this comparison.
        ''' </remarks>
        Public Shared Operator =(ByVal p1 As Particle, ByVal p2 As Particle) As Boolean
            Dim p1HwChannel1DataRaw = p1.ChannelData_Hardware(1).Data_Raw
            Dim p2HwChannel1DataRaw = p2.ChannelData_Hardware(1).Data_Raw

            Dim numSamplesP1 = p1HwChannel1DataRaw.Length
            Dim numSamplesP2 = p2HwChannel1DataRaw.Length

            If numSamplesP1 <> numSamplesP2 Then
                Return False
            End If

            Dim p1HwChannel3DataRaw = p1.ChannelData_Hardware(3).Data_Raw
            Dim p2HwChannel3DataRaw = p2.ChannelData_Hardware(3).Data_Raw

            For i As Integer = 0 To numSamplesP1 - 1
                If p1HwChannel3DataRaw(i) <> p2HwChannel3DataRaw(i) Or p1HwChannel1DataRaw(i) <> p2HwChannel1DataRaw(i) Then
                    Return False
                End If
            Next

            Return True
        End Operator

        ''' <summary>
        ''' Compares two particles by means of their DSPChannel data (SWS in most cases). Only returns false if the data matches exactly.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator =(ByVal p1 As Particle, ByVal p2 As SegmentedData.RawParticle) As Boolean
            ' Compare SWS and FWS_x channels

            If Not p1.ChannelData_Hardware(3).Data_Raw.SequenceEqual(p2.ChannelData(3)) Then Return False
            If Not p1.ChannelData_Hardware(1).Data_Raw.SequenceEqual(p2.ChannelData(1)) Then Return False

            Return True
        End Operator

        ''' <summary>
        ''' Compares two particles by means of their DSPChannel data (SWS in most cases). Only returns false if the data matches exactly.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator <>(ByVal p1 As Particle, ByVal p2 As SegmentedData.RawParticle) As Boolean
            Return Not (p1 = p2)
        End Operator

        ''' <summary>
        ''' Compares two particles by means of their DSPChannel data (SWS in most cases). Only returns false if the data matches exactly.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Operator <>(ByVal p1 As Particle, ByVal p2 As Particle) As Boolean
            Return Not (p1 = p2)
        End Operator

        Public Overridable ReadOnly Property hasImage As Boolean
            Get
                Return False
            End Get
        End Property
    End Class
End Namespace
