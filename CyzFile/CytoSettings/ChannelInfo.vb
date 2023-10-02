Imports System.Drawing

Namespace CytoSettings
    ''' <summary>
    ''' Store information about the specific PMT, this is the information that was added for the XR.  Hopefully this is
    ''' a "temporary" class and when we start implementing the new file format meta-data this one gets replaced with a more
    ''' permanent structure.
    ''' 
    ''' It also contains calibration information, which is stored in the XR, but for now we do not use it.  In theory it can
    ''' be used to more accurately calculate the gain, and normalize for PMT variations.
    ''' The whole min/max/default settings stuff is stored outside of this class, it is just info on the PMT.
    ''' For now the type is stored as a string.  May change that to an enum, for now that tis to much work.
    ''' </summary>
    <Serializable()> Public Class PmtData

        Public Sub New(tp As String, serial As String, cs As Double, ans As Double, adc As Double, cbs As Double, roverw As Double)
            PmtType = tp
            SerialNumber = serial
            CathodeSens = cs
            AnodeSens = ans
            AnodeDarkCurrent = adc
            CathodeBlueSense = cbs
            RW = roverw
        End Sub

        Public ReadOnly PmtType As String
        Public ReadOnly SerialNumber As String

        Public ReadOnly CathodeSens As Double ' Cathode Luminous Sensitivity:  muA/lm
        Public ReadOnly AnodeSens As Double ' Anode Luminous Sensitivity:  A/lm 
        Public ReadOnly AnodeDarkCurrent As Double ' Anode Dark Current nA
        Public ReadOnly CathodeBlueSense As Double ' Cathode Blue Sensitivity Index
        Public ReadOnly RW As Double ' R/W  10^-3, only For R9980U-20 red sensitivity. (Red Sensitivity/White Sensitivity
    End Class


    ''' <summary>
    ''' Contains all meta-information about a channel, should therefore have been called ChannelInfo, but can't be changed due to existing serialized data files
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class channel
        Private _channelType As ChannelTypesEnum ' New value, originally not stored, so in OnDeserialized set it, if value is unknown.



        ''' <summary>
        ''' There are some very old data files that are incorrectly serialized, and it looks like they need this type somewhere.
        ''' </summary>
        ''' <remarks>Do not use this type, it is not used anywhere.</remarks>
        <Obsolete>
        Private Enum ChannelType
            Hardware = 0
            CurvatureFWS = 1 'deprecated
            Ratio = 2 'deprecated?
            SplitLeftRight = 3 'deprecated
            Virtual = 4
        End Enum



        ''' <summary>
        ''' Constructor for hardwired high sensitivity channel (Pico Plankton  A or C option)
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelSyncPulseValues"></param>
        ''' <param name="ChannelColor"></param>
        ''' <param name="ChannelIsVisible"></param>
        ''' <param name="ChannelIsHighSensitivity"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal ChannelSyncPulseValues As Int16, ByVal ChannelColor As Color, ByVal ChannelIsVisible As Boolean, ByVal ChannelIsHighSensitivity As Boolean)
            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = ChannelIsVisible
            highsensitivity = ChannelIsHighSensitivity
            color = ChannelColor
            lowcheck = 0
            hasLowCheck = False
            SyncPulseValue = ChannelSyncPulseValues
        End Sub

        ''' <summary>
        ''' Constructor for switchable selectable high low setting. (Pico Plankton  B option)
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelSyncPulseValues"></param>
        ''' <param name="c"></param>
        ''' <param name="ChannelIsVisible"></param>
        ''' <param name="ChannellowCheckID"></param>
        ''' <param name="ChannelHasLowCheck"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal ChannelSyncPulseValues As Int16, ByVal c As Color, ByVal ChannelIsVisible As Boolean, ByVal ChannellowCheckID As Integer, ByVal ChannelHasLowCheck As Boolean)
            _cytoSenseOpticalUnitProperty = PMTOptionsEnum.Pico_Plankton_B

            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = ChannelIsVisible
            highsensitivity = True
            color = c
            lowcheckID = ChannellowCheckID
            hasLowCheck = ChannelHasLowCheck
            SyncPulseValue = ChannelSyncPulseValues
        End Sub
        ''' <summary>
        ''' Constructor for 256 bit continuous sensitivity setting (new standard, no Pico Plankton option anymore)
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelSyncPulseValues"></param>
        ''' <param name="c"></param>
        ''' <param name="ChannelIsVisible"></param>
        ''' <param name="channelPMT_id">As printed on circuit board! Actual index address is -1, this is handled internally</param>        
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal ChannelSyncPulseValues As Int16, ByVal c As Color, ByVal ChannelIsVisible As Boolean, ByVal channelPMT_id As Byte, ByVal channelPMT_level_minimum As Byte, ByVal channelPMT_level_maximum As Byte)
            _cytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs

            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = ChannelIsVisible
            highsensitivity = True
            color = c
            hasI2CPMTLevel = True
            _PMT_id = CByte(channelPMT_id - 1)
            hasLowCheck = False
            SyncPulseValue = ChannelSyncPulseValues
            _PMTLevel_min = channelPMT_level_minimum
            _PMTLevel_max = channelPMT_level_maximum
        End Sub

        ''' <summary>
        ''' Constructor for new V10 electronics, no sync pulse values anymore.
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="c"></param>
        ''' <param name="channelPMT_id">As printed on circuit board! Actual index address is -1, this is handled internally</param>        
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal c As Color, ByVal channelPMT_id As Byte, ByVal channelPMT_level_minimum As Byte, ByVal channelPMT_level_maximum As Byte)
            _cytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs

            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = True
            highsensitivity = True
            color = c
            hasI2CPMTLevel = True
            _PMT_id = CByte(channelPMT_id - 1)
            hasLowCheck = False
            SyncPulseValue = -1
            _PMTLevel_min = channelPMT_level_minimum
            _PMTLevel_max = channelPMT_level_maximum
        End Sub

        ''' <summary>
        ''' Constructor for new V10 electronics FWS channel
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelColor"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal ChannelColor As Color)
            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = True
            highsensitivity = False
            color = ChannelColor
            lowcheck = 0
            hasLowCheck = False
            SyncPulseValue = -1
        End Sub

        ''' <summary>
        ''' Constructor for SCPI FWS channel
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelColor"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, chanType As ChannelTypesEnum, ByVal ChannelColor As Color)
            name = ChannelName
            _channelType = chanType

            visible = True
            highsensitivity = False
            color = ChannelColor
            lowcheck = 0
            hasLowCheck = False
            SyncPulseValue = -1
        End Sub


        ''' <summary>
        ''' Constructor for new SCPI, no sync pulse values anymore.
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="c"></param>
        ''' <param name="channelPMT_id">As printed on circuit board! Actual index address is -1, this is handled internally</param>        
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, chanType As ChannelTypesEnum, ByVal c As Color, ByVal channelPMT_id As Byte, ByVal channelPMT_level_minimum As Byte, ByVal channelPMT_level_maximum As Byte, Optional pmtInf As PmtData = Nothing)
            _cytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs

            name = ChannelName
            _channelType = chanType

            visible = True
            highsensitivity = True
            color = c
            hasI2CPMTLevel = True
            _PMT_id = CByte(channelPMT_id - 1)
            hasLowCheck = False
            SyncPulseValue = -1
            _PMTLevel_min = channelPMT_level_minimum
            _PMTLevel_max = channelPMT_level_maximum
            _pmtInfo = pmtInf
        End Sub



        ''' <summary>
        ''' Constructor for 256 bit continuous sensitivity setting, with legacy Pico Plankton C option
        ''' </summary>
        ''' <param name="ChannelName"></param>
        ''' <param name="ChannelSyncPulseValues"></param>
        ''' <param name="c"></param>
        ''' <param name="ChannelIsVisible"></param>
        ''' <param name="channelPMT_id">As printed on circuit board! Actual index address is -1, this is handled internally</param>        
        ''' <remarks></remarks>
        Public Sub New(ByVal ChannelName As String, ByVal ChannelSyncPulseValues As Int16, ByVal c As Color, ByVal ChannelIsVisible As Boolean, ByVal channelPMT_id As Byte, ByVal channelPMT_level_minimum As Byte, ByVal channelPMT_level_maximum As Byte, ByVal channelIsHighSens As Boolean)
            _cytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs_seperateHighLowPMTS

            name = ChannelName
            _channelType = NameToChannelType(name)

            visible = ChannelIsVisible
            highsensitivity = channelIsHighSens
            color = c
            hasI2CPMTLevel = True
            _PMT_id = CByte(channelPMT_id - 1)
            hasLowCheck = False
            SyncPulseValue = ChannelSyncPulseValues
            _PMTLevel_min = channelPMT_level_minimum
            _PMTLevel_max = channelPMT_level_maximum
        End Sub

        <Runtime.Serialization.OnDeserialized>
        Friend Sub OnDeserializedMethod(ByVal context As Runtime.Serialization.StreamingContext)
            If _channelType = ChannelTypesEnum.Unknown Then
                _channelType = NameToChannelType(name)
            End If
        End Sub

        ''' <summary>
        ''' Before de-serializing, make sure the _pmtInfo field is set to nothing, so we can recognize it
        ''' is not set later.
        ''' </summary>
        ''' <param name="context"></param>
        <Runtime.Serialization.OnDeserializing>
        Public Sub OnDeserialize(ByVal context As Runtime.Serialization.StreamingContext)
            _pmtInfo = Nothing
        End Sub

        Public Sub setAsSummedChannel(ByVal LF_channelIndex As Integer)
            _IsHFplusLFchannel = True
            _LFchannelIndex = LF_channelIndex
        End Sub
        Public Sub setAsAlreadySubtracted(ByVal HF_channelIndex As Integer)
            _isFilteredLFChannel = True
            _HFchannelIndex = HF_channelIndex
        End Sub

        Public hasLowCheck As Boolean
        Public lowcheck As Integer 'deprecated. Keep for backwards compatibility!
        Public lowcheckID As Integer
        Public name As String
        Public visible As Boolean
        Public highsensitivity As Boolean
        Public color As Color
        Public SyncPulseValue As Int16

        Private _cytoSenseOpticalUnitProperty As CytoSense.CytoSettings.PMTOptionsEnum 'be aware of legacy situations, as this flag was only added 16-16-2013 'Yeah right, 16-16
        Public ReadOnly Property CytoSenseOpticalUnitProperty As CytoSense.CytoSettings.PMTOptionsEnum
            Get
                Return _cytoSenseOpticalUnitProperty
            End Get
        End Property

        Private _PMTLevel_min As Byte
        Private _PMTLevel_max As Byte
        Public hasI2CPMTLevel As Boolean

        Private _laserColor As LaserColorEnum
        Public Property LaserColor As LaserColorEnum
            Get
                Return _laserColor
            End Get
            Set(ByVal value As LaserColorEnum)
                _laserColor = value
            End Set
        End Property

        Public ReadOnly Property LaserColorName As String
            Get
                Return getLaserColorEnumName(LaserColor)
            End Get
        End Property

        ''' <summary>
        ''' Describes laser that are in the machine (and excite this channel).
        ''' </summary>
        ''' <remarks>The descriptionAtrribute is the presentable name of the laser </remarks>
        Public Enum LaserColorEnum
            Unknown = 0
            Blue488 = 1
            Green = 2
            Red633 = 3
            Multiple = 4
            Green552
            Blue447
            Red642
        End Enum

        Private Function getLaserColorEnumName(ByRef enumVal As LaserColorEnum) As String
            Return LaserTrivialNamesDictionary.getTrivialName(enumVal)
        End Function

        Private _PMT_id As Byte

        Private _hasDualFocus As Boolean
        Public Property DualFocus As Boolean
            Get
                Return _hasDualFocus
            End Get
            Set(ByVal value As Boolean)
                _hasDualFocus = value
            End Set
        End Property

        ''' <summary>
        ''' Returns index of PNT on the DAC5578. Which is 0 based
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PMTLevel_id As Byte
            Get
                Return _PMT_id
            End Get
        End Property

        ''' <summary>
        ''' Minimum level of PMT on the DAC5578.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PMTLevel_min As Byte
            Get
                Return _PMTLevel_min
            End Get
            Set(ByVal value As Byte)
                _PMTLevel_min = value
            End Set
        End Property

        ''' <summary>
        ''' Maximum level of PMT on the DAC5578.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PMTLevel_max As Byte
            Get
                Return _PMTLevel_max
            End Get
            Set(ByVal value As Byte)
                _PMTLevel_max = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return Description
        End Function

        Protected Shared Function NameToChannelType(name As String) As ChannelTypesEnum
            If String.IsNullOrEmpty(name) Then
                Return ChannelTypesEnum.Unknown
            End If

            Select Case name
                Case "FWS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FWS
                Case "FWS L"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FWSL
                Case "FWS R"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FWSR

                Case "SWS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.SWS
                Case "SWS LS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.SWS
                Case "SWS HS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.SWS

                Case "FL Red"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLRed
                Case "FL Red LS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLRed
                Case "FL Red HS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLRed
                Case "2 FL Red HS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLRed
                Case "2 FL Red LS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLRed

                Case "FL Orange"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLOrange
                Case "FL Orange LS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLOrange
                Case "FL Orange HS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLOrange

                Case "FL Yellow"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLYellow
                Case "FL Yellow LS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLYellow
                Case "FL Yellow HS"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLYellow
                Case "FL Yellow HF" 'Excited by a 405 laser
                    Return ChannelTypesEnum.FLYellow

                Case "FL Green Yellow"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLYellow
                Case "FL Green"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.FLGreen

                Case "FL Blue"
                    Return ChannelTypesEnum.FLBlue
                Case "2 FL Purple"
                    Return ChannelTypesEnum.FLPurple

                Case "Curvature"
                    Return CytoSense.CytoSettings.ChannelTypesEnum.Curvature

                Case "Polarized"
                    Return ChannelTypesEnum.Polarized

                Case "DSP"
                    Return ChannelTypesEnum.DSP
                Case "Trigger1"
                    Return ChannelTypesEnum.Trigger1
                Case "Trigger2"
                    Return ChannelTypesEnum.Trigger2
                Case "Dummy"
                    Return ChannelTypesEnum.Dummy

                Case Else
                    If name.Contains("/") Then
                        Return ChannelTypesEnum.Ratio
                    Else
                        'Things are starting to look bad, do a last attempt at getting something sensible:
                        If name.ToLower.Contains("red") Then
                            Return ChannelTypesEnum.FLRed
                        End If

                        If name.ToLower.Contains("orange") Then
                            Return ChannelTypesEnum.FLOrange
                        End If

                        If name.ToLower.Contains("yellow") Then
                            Return ChannelTypesEnum.FLYellow
                        End If

                        If name.ToLower.Contains("purple") Then
                            Return ChannelTypesEnum.FLPurple
                        End If

                        If name.ToLower.Contains("blue") Then
                            Return ChannelTypesEnum.FLBlue
                        End If

                        If name.ToLower.Contains("green") Then
                            Return ChannelTypesEnum.FLGreen
                        End If

                        If name.ToLower.StartsWith("fws") Then
                            Return ChannelTypesEnum.FWS
                        End If

                        If name.ToLower.StartsWith("sws") Then
                            Return ChannelTypesEnum.SWS
                        End If

                        If name.ToLower.Equals("fl sws hs") Then
                            'Some completely weird channel by the NIOZ machine (which passed away long ago?)
                            Return ChannelTypesEnum.FLBlue
                        End If
                        If name.ToLower.Contains("fws") Then
                            Return CytoSense.CytoSettings.ChannelTypesEnum.FWS
                        End If
                        If name.ToLower.Contains("sws") Then
                            Return CytoSense.CytoSettings.ChannelTypesEnum.SWS
                        End If
                        ' Special test machine channels!
                        If name.ToLower() = "socketonly" Then
                            Return ChannelTypesEnum.SocketOnly
                        End If
                        If name.ToLower() = "grabberonly" Then
                            Return ChannelTypesEnum.GrabberOnly
                        End If
#If DEBUG Then
                        '                            Throw New NotImplementedException("An unimplemented channel is found!")
#End If
                    End If

                    Return ChannelTypesEnum.Unknown
            End Select
        End Function

        ''' <summary>
        ''' Defines the type of this channel, independent of sensitivity or laser excitation.
        ''' </summary>
        Public Overridable ReadOnly Property Channel_Type As CytoSense.CytoSettings.ChannelTypesEnum
            Get
                Return _channelType
            End Get
        End Property

        ''' <summary>
        ''' Defines the name string for this channel. 
        ''' Default for virtual channels, needs to be changed in overridden class. 
        ''' For hardware channels the name needs to be changed in the CytoSettings machine configuration
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property Description As String
            Get
                Return name
            End Get
        End Property

#Region "Software Subtract Laser system (ssl system)"

        Private _isFilteredLFChannel As Boolean
        ''' <summary>
        ''' Flags a filtered channel in the Software Subtract Laser (SSL) system
        ''' If this flag is true, the channel only needs information from the LF channel, as the HF signal was filtered out by the electronics. 
        ''' Still, the linked HF channel can be found from the HFchannelIndex property in this class
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsFilteredLFChannel() As Boolean
            Get
                Return _isFilteredLFChannel
            End Get
        End Property

        Private _IsHFplusLFchannel As Boolean
        ''' <summary>
        ''' Flags a summed channel in the Software Subtract Laser (SSL) system
        ''' If this flag is true, then the channel was mixed with both the filtered LF and the HF PMT signal, 
        ''' which still need to be subtracted in order to get the HF signal. The LF channel can be found by using the LFchannelIndex property in this class
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsHFplusLFchannel() As Boolean
            Get
                Return _IsHFplusLFchannel
            End Get
        End Property

        Private _LFchannelIndex As Integer = -1
        ''' <summary>
        ''' Contains the index in the HW channels of a linked LF channel, in case of a 2e laser software subtract-laser-system (SSL system) HF-channel. 
        ''' The channel pointed by this index needs to be subtracted from this channel.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property LF_HardwareChannelIndex() As Integer
            Get
                Return _LFchannelIndex
            End Get
        End Property

        Private _HFchannelIndex As Integer = -1
        ''' <summary>
        ''' Contains the index in the HW channels of a linked HF channel, in case of a 2e laser software subtract-laser-system (SSL system) LF-channel. 
        ''' The channel pointed by this index is already filtered out by the electronics, so this 
        ''' link may not be used except for extreme situations (clipping etc)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable ReadOnly Property HF_HardwareChannelIndex() As Integer
            Get
                Return _HFchannelIndex
            End Get
        End Property
#End Region


        Private Const R9880_SLOPE As Double = 8.5999511725556
        Private Const R9880_OFFSET As Double = -19.49949936378

        ''' <summary>
        ''' Converting the byte level PMT setting to a Gain value.  This depends on the high voltage print
        ''' used in the machine, so that has to be passed into the function.
        ''' </summary>
        ''' <param name="lvl"></param>
        ''' <param name="hvt"></param>
        ''' <returns>The gain factor for the specified byte level.</returns>
        ''' <remarks>Not sure this is the best place for this function, but it is the best I can think of for now.</remarks>
        Private Shared Function ByteLevel2PmtGain(lvl As Byte, hvt As HighVoltagePrintType, pmtId As Byte) As Double
            Dim lowVoltage As Double = (5.0 * lvl) / 255.0
            Dim highVoltage As Double = 0.0
            Dim rearPmtId As Integer = -1
            ' NOTE: Internal IDs are 1 less as the Ids printed on the boards!!!
            If hvt = CytoSense.CytoSettings.HighVoltagePrintType.FransJan_v1_0 Then
                rearPmtId = 8 - 1
            ElseIf hvt = CytoSense.CytoSettings.HighVoltagePrintType.Ruud Then
                rearPmtId = 6 - 1
            Else
                Throw New Exception(String.Format("Unknown PMT controller type: '{0}'", hvt))
            End If
            If pmtId = rearPmtId Then 'H10720, rear PMT
                highVoltage = lowVoltage * (1.087 / 5) * 1000
            Else 'R9880U, normal PMT
                highVoltage = lowVoltage * 250
            End If
            Return Math.Pow(10, R9880_OFFSET + R9880_SLOPE * Math.Log10(highVoltage))

        End Function

        ''' <summary>
        ''' Use the information from the pmtData to determine what gain is that corresponds to the specific
        ''' byte level. Currently we only care about PMT with embedded high voltage, and separate high voltage.
        ''' We ignore the actual calibration info on sensitivity, dark current, etc.  So in the future we
        ''' can do better if we add that. 
        ''' 
        ''' We look at the type of the PMTt, and use that to determine which formula for voltage
        ''' conversion to use. (NOTE: Currently we support only one formula :-( ).
        ''' 
        ''' The actual high voltage to gain conversion we use is always the same, I am not sure how accurate
        ''' this.
        ''' 
        ''' </summary>
        ''' <param name="lvl"></param>
        ''' <param name="pmt"></param>
        ''' <returns></returns>
        Private Shared Function ByteLevel2PmtGain(lvl As Byte, pmt As PmtData) As Double
            Dim lowVoltage As Double = (5.0 * lvl) / 255.0
            Dim highVoltage As Double = 0.0

            If pmt.PmtType = "H10720-01" OrElse
               pmt.PmtType = "H10720-110" OrElse
               pmt.PmtType = "H10720-120" OrElse
               pmt.PmtType = "H10720-210" Then
                highVoltage = lowVoltage * (1.087 / 5) * 1000

            Else 
                ' Not a rear PMT type, for classic CytoSense we support "normal" PMTs and high voltages
                 ' They use a different high voltage formula.
                ' _log.ErrorFormat("Unsupported PMT Type: '{0}'", pmt.PmtType)
                highVoltage = lowVoltage * 250
            End If
            Return Math.Pow(10, R9880_OFFSET + R9880_SLOPE * Math.Log10(highVoltage))
        End Function

        Private _pmtInfo As PmtData = Nothing


        ''' <summary>
        ''' Check our PMT configuration and call the correct configuration base on this.
        ''' Note: the high voltage type is only need for senses where we do not have the PMT info.
        ''' It would be nicer if we could fill out the PMT info on startup for the older instruments
        ''' but for now this is quicker.
        ''' </summary>
        ''' <param name="lvl"></param>
        ''' <returns></returns>
        Public Function ByteLevel2PmtGain(lvl As Byte, hvt As HighVoltagePrintType) As Double
            If Not hasI2CPMTLevel Then
                Throw New Exception(String.Format("This channel '{0}' does not have a (configurable) PMT.", name))
            End If
            If _pmtInfo IsNot Nothing Then
                Return ByteLevel2PmtGain(lvl, _pmtInfo)
            Else
                Return ByteLevel2PmtGain(lvl, hvt, PMTLevel_id)
            End If
        End Function

        <NonSerialized()>
        Private Shared ReadOnly _log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)
    End Class

    <Serializable()> Public Class LaserTrivialNamesDictionary
        Private Shared laserdic As New Dictionary(Of channel.LaserColorEnum, String) From
            {{channel.LaserColorEnum.Unknown, "Undefined laser"}, {channel.LaserColorEnum.Blue488, "Blue (488nm)"}, {channel.LaserColorEnum.Blue447, "Blue (447nm)"},
             {channel.LaserColorEnum.Green552, "Green (552nm)"}, {channel.LaserColorEnum.Red633, "Red (633nm)"}, {channel.LaserColorEnum.Red642, "Red (642)"}, {channel.LaserColorEnum.Multiple, "Multiple"}}

        Public Shared Function getTrivialName(ByRef enumVal As channel.LaserColorEnum) As String
            Return laserdic.Item(enumVal)
        End Function
    End Class

    ''' <summary>
    ''' Provides channel meta-information for a virtual channel.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public MustInherit Class VirtualChannelInfo
        Inherits channel 'In an ideal world this would be done via a shared interface between virtual and hardware ChannelInfo, because virtual ChannelInfo has a lot of meaningless properties now
        Private _virtualType As VirtualChannelType

        Public Sub New(ByVal ChannelName As String, ByVal color As Color, ByVal Vtype As VirtualChannelType)
            MyBase.New(ChannelName, color)
            _virtualType = Vtype
        End Sub

        Public ReadOnly Property VirtualType As VirtualChannelType
            Get
                Return _virtualType
            End Get
        End Property

        <Serializable()> Public Enum VirtualChannelType
            FWS_L_plus_R
            Curvature_L_div_R
            DualFocusLeft
            DualFocusRight
            HF_plus_LF_Summed   'SSL system
            LF_Filtered     'SSL system
        End Enum

        Public MustOverride Overrides ReadOnly Property Description As String
    End Class

    ''' <summary>
    ''' Used in case of a SSL system.
    ''' Contains the meta information about a summed HF/LF channel. In order to get the intended HF channel, the LF channel needs to be subtracted from the raw HF channel.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class VirtualHFSummedChannelInfo
        Inherits VirtualChannelInfo
        Private _HFChannel As channel
        Private _LFChannel As channel

        Public Sub New(ByVal ChannelName As String, color As Color, HFChannel As channel, LFChannel As channel)
            MyBase.New(BuildHFSummedChannelName(ChannelName, HFChannel, LFChannel), color, VirtualChannelType.HF_plus_LF_Summed)
            _HFChannel = HFChannel
            _LFChannel = LFChannel
        End Sub

        ''' <summary>
        ''' Contains the index of the HF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property HF_HardwareChannelIndex As Integer
            Get
                Return _LFChannel.HF_HardwareChannelIndex 'yes reversed, and all to keep the good peace... :)
            End Get
        End Property

        ''' <summary>
        ''' Contains the index of the LF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property LF_HardwareChannelIndex As Integer
            Get
                Return _HFChannel.LF_HardwareChannelIndex 'yes reversed, and all to keep the good peace... :)
            End Get
        End Property

        ''' <summary>
        ''' Contains the information of the HF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HFChannelInfo As channel
            Get
                Return _HFChannel
            End Get
        End Property

        ''' <summary>
        ''' Contains the information of the LF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LFChannelInfo As channel
            Get
                Return _LFChannel
            End Get
        End Property

        Public Overrides ReadOnly Property Description As String
            Get
                Return name
            End Get
        End Property

        Private Shared Function BuildHFSummedChannelName(baseName As String, hfC As channel, lfC As channel) As String
            Dim s As String = NameToChannelType(baseName).ToString()
            If hfC.CytoSenseOpticalUnitProperty = PMTOptionsEnum.Pico_Plankton_C Or hfC.CytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs_seperateHighLowPMTS Then
                If hfC.highsensitivity Then
                    s &= "-HS"
                Else
                    s &= "-LS"
                End If
            End If
            Return s & " by " & hfC.LaserColorName
        End Function

    End Class

    ''' <summary>
    ''' Used in case of a SSL system.
    ''' Contains the meta information about a de-summed HF/LF channel. 
    ''' Although it is virtual it contains references to the actual hardware channel, since the signals on those channels are actually identical.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class VirtualLFFilteredChannelInfo
        Inherits VirtualChannelInfo
        Private _HFChannel As channel
        Private _LFChannel As channel

        Public Sub New(ByVal ChannelName As String, color As Color, HFChannel As channel, LFChannel As channel)
            MyBase.New(BuildLFFilteredChannelName(ChannelName, HFChannel, LFChannel), color, VirtualChannelType.LF_Filtered)
            _HFChannel = HFChannel
            _LFChannel = LFChannel
        End Sub

        ''' <summary>
        ''' Contains the index of the HF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property HF_HardwareChannelIndex As Integer
            Get
                Return _LFChannel.HF_HardwareChannelIndex 'yes reversed, and all to keep the good peace... :)
            End Get
        End Property

        ''' <summary>
        ''' Contains the index of the LF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property LF_HardwareChannelIndex As Integer
            Get
                Return _HFChannel.LF_HardwareChannelIndex 'yes reversed, and all to keep the good peace... :)
            End Get
        End Property

        ''' <summary>
        ''' Contains the information of the HF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property HFChannelInfo As channel
            Get
                Return _HFChannel
            End Get
        End Property

        ''' <summary>
        ''' Contains the information of the LF channel in the hardware channel list
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property LFChannelInfo As channel
            Get
                Return _LFChannel
            End Get
        End Property

        Public Overrides ReadOnly Property Description As String
            Get
                Return name
            End Get
        End Property

        Private Shared Function BuildLFFilteredChannelName(baseName As String, hfC As channel, lfC As channel) As String
            Dim s As String = NameToChannelType(baseName).ToString()
            If lfC.CytoSenseOpticalUnitProperty = PMTOptionsEnum.Pico_Plankton_C Or lfC.CytoSenseOpticalUnitProperty = PMTOptionsEnum.AdjustablePMTs_seperateHighLowPMTS Then
                If lfC.highsensitivity Then
                    s &= "-HS"
                Else
                    s &= "-LS"
                End If
            End If
            Return s & " by " & lfC.LaserColorName
        End Function

    End Class

    ''' <summary>
    ''' Contains meta-info about the split up channels of a dual focus laser machine
    ''' </summary>
    ''' <remarks></remarks>
    Public Class VirtualDualFocusChannelInfo
        Inherits VirtualChannelInfo

        Private _HWChannelIndex As Integer

        Public Sub New(ByRef baseChannelName As String, ByVal color As Color, ByVal leftOrRight As VirtualChannelType, ByVal HWChannelIndex As Integer)
            MyBase.New(If(leftOrRight <> VirtualChannelType.DualFocusLeft, baseChannelName & " left", baseChannelName & " right"), color, leftOrRight)
            If leftOrRight <> VirtualChannelType.DualFocusLeft And leftOrRight <> VirtualChannelType.DualFocusRight Then
                Throw New NotImplementedException("Trying to create an invalid channel")
            End If
            _HWChannelIndex = HWChannelIndex
            Me.LaserColor = LaserColor
        End Sub

        Public ReadOnly Property HWChannelIndex As Integer
            Get
                Return _HWChannelIndex
            End Get
        End Property

        Public Overrides ReadOnly Property Description As String
            Get
                Return name
            End Get
        End Property

    End Class

    Public Class VirtualFWSCurvatureChannelInfo
        Inherits VirtualChannelInfo
        Private _FWSLChannelIndex As Integer
        Private _FWSRChannelIndex As Integer

        Public Sub New(FWSLChannelIndex As Integer, FWSRChannelIndex As Integer)
            MyBase.New("FWS", Drawing.Color.Black, VirtualChannelType.FWS_L_plus_R)
            _FWSLChannelIndex = FWSLChannelIndex
            _FWSRChannelIndex = FWSRChannelIndex
        End Sub

        Public ReadOnly Property FWSL As Integer
            Get
                Return _FWSLChannelIndex
            End Get
        End Property

        Public ReadOnly Property FWSR As Integer
            Get
                Return _FWSRChannelIndex
            End Get
        End Property

        Public Overrides ReadOnly Property Description As String
            Get
                Return "FWS"
            End Get
        End Property

    End Class

    <Serializable()> Public Class VirtualCurvatureChannelInfo
        Inherits VirtualChannelInfo
        Private _FWSLChannelIndex As Integer
        Private _FWSRChannelIndex As Integer

        Public Sub New(FWSLChannelIndex As Integer, FWSRChannelIndex As Integer)
            MyBase.New("Curvature", Drawing.Color.Purple, VirtualChannelType.Curvature_L_div_R)
            _FWSLChannelIndex = FWSLChannelIndex
            _FWSRChannelIndex = FWSRChannelIndex
        End Sub

        Public ReadOnly Property FWSL As Integer
            Get
                Return _FWSLChannelIndex
            End Get
        End Property

        Public ReadOnly Property FWSR As Integer
            Get
                Return _FWSRChannelIndex
            End Get
        End Property

        Public Overrides ReadOnly Property Description As String
            Get
                Return "Curvature"
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Wrapper class to provide more structured access to the different channel types provided by the particle class
    ''' </summary>
    ''' <remarks>Needed because _hw_channel_id is not serialized with normal channel. Also this class will not be serialized and thus can be changed without danger </remarks>
    <Serializable()> Public Class ChannelWrapper
        Private _channel As channel 'can be either virtual or hardware
        Private _is_hw_channel As Boolean
        Private _hw_channel_id As Integer
        Private _linetype As LineTypeEnum

        Private _idInList As Integer
        ''' <summary>
        ''' Contains the ID of this channel in the cytosettings.channellist
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ID As Integer
            Get
                Return _idInList
            End Get
        End Property

        ''' <summary>
        ''' Constructor for virtual channel with no direct link to a hardware channel
        ''' </summary>
        ''' <param name="channel"></param>
        ''' <remarks></remarks>
        Public Sub New(channel As channel, idInList As Integer, line As LineTypeEnum)
            _channel = channel
            _is_hw_channel = False
            _hw_channel_id = -1
            Dim virChanInf As VirtualChannelInfo = TryCast(channel, VirtualChannelInfo)
            ' NOTE: if you set the isHardware flag, then it will FOOL a lot of other parts into thinking it is hardware, and they
            ' will fail to work inside CC4, but we set that hardware channel id, because there really is an underlying hardware channel.
            ' When exporting IIF files, check the virtual channel type to see if it is an LF_Filtered one that can actually be exported.
            If Not Object.Equals(virChanInf, Nothing) AndAlso virChanInf.VirtualType = VirtualChannelInfo.VirtualChannelType.LF_Filtered Then
                _hw_channel_id = channel.LF_HardwareChannelIndex
            End If
            _idInList = idInList
            _linetype = line
        End Sub
        ''' <summary>
        ''' Constructor for any hardware channel
        ''' </summary>
        ''' <param name="hw_channel_id"></param>
        ''' <param name="channel"></param>
        ''' <remarks></remarks>
        Public Sub New(hw_channel_id As Integer, idInList As Integer, channel As channel, line As LineTypeEnum)
            _channel = channel
            _is_hw_channel = True
            _hw_channel_id = hw_channel_id
            _idInList = idInList
            _linetype = line
        End Sub

        Public Overrides Function ToString() As String
            Return _channel.Description
        End Function

        Public ReadOnly Property Description As String
            Get
                Return _channel.Description
            End Get
        End Property

        Public ReadOnly Property DefaultColor As Color
            Get
                If _channel.color <> Color.Yellow Then
                    Return _channel.color
                Else
                    Return Color.Gold 'Yellow is helemaal niet zichtbaar tegen witte achtergrond. 
                End If
            End Get
        End Property

        Public ReadOnly Property Channeltype As ChannelTypesEnum
            Get
                Return _channel.Channel_Type
            End Get
        End Property

        ''' <summary>
        ''' The virtual channel type of this channel, only if it is virtual. Otherwise returns Nothing.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property VirtualChannelType As VirtualChannelInfo.VirtualChannelType
            Get
                If Not IsHWChannel Then
                    Return CType(_channel, VirtualChannelInfo).VirtualType
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property ChannelInfo As channel
            Get
                Return _channel
            End Get
        End Property

        ''' <summary>
        ''' Flags this as a hardware channel (generated by instrument) or not
        ''' </summary>
        Public ReadOnly Property IsHWChannel As Boolean
            Get
                Return _is_hw_channel
            End Get
        End Property

        Public ReadOnly Property HW_channel_id As Integer
            Get
                Return _hw_channel_id
            End Get
        End Property

        Public Property DefaultLineType As LineTypeEnum
            Get
                Return _linetype
            End Get
            Set(ByVal value As LineTypeEnum)
                _linetype = value
            End Set
        End Property

    End Class

    <Serializable()> Public Enum LineTypeEnum
        line = 0
        stripe = 1
    End Enum

    <Serializable()> Public Enum ChannelTypesEnum 'Serializable for dotplot presets in cc4
        Unknown = 0
        FWS = 1
        FWSL = 2
        FWSR = 3
        SWS = 4
        FLRed = 5
        FLOrange = 6
        FLYellow = 7
        FLGreen = 8
        FLBlue = 9
        FLPurple = 10
        Trigger1 = 11
        Trigger2 = 12
        DSP = 13
        Curvature = 14
        Polarized = 15
        Ratio = 18
        Dummy = 13
        FWSLR = 19 'selects either FWS or FWS L according to which is available due to channel viz settings.
        ' Some special test machine cases.
        SocketOnly = 20
        GrabberOnly = 21
    End Enum

    ' Value below is referenced in some OLD datafiles, so it needs to be defined or they cannot be loaded,
    ' but it is not used anymore.
    <Obsolete()>
    <Serializable()>
    Public Enum ChannelTypes
        Unknown = 0
        FWS = 1
        FWSL = 2
        FWSR = 3
        SWS = 4
        FLRED = 5
        FLOrange = 6
        FLYellow = 7
        FLGreen = 8
        Trigger1 = 9
        Trigger2 = 10
        DSP = 11
        Curvature = 12
        Polarized = 13
    End Enum


    <Serializable()> Public Enum ChannelAccessMode
        Normal = 0
        Optical_debugging = 1
        Full_debugging = 2
    End Enum

End Namespace

