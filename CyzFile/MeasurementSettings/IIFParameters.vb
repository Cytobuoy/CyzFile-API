Imports System.Data

Namespace MeasurementSettings
    <Serializable()> Public Structure IIFParameters
        ''' <summary>
        ''' Legacy CC3 constructor. Do not use.
        ''' </summary>
        Public Sub New(ByVal NumberOfChannels As Integer)
            ReDim Me.Channels(NumberOfChannels - 1)
            Me.SelectOnCurvature = False
        End Sub

        Private _cytosettings As CytoSettings.CytoSenseSetting
        Public ReadOnly Property Cytosettings As CytoSettings.CytoSenseSetting
            Get
                Return _cytosettings
            End Get
        End Property

        ''' <summary>
        ''' Create a DEEP copy of the IIFParameters structure.
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New( other As IIFParameters )
            If other.Channels IsNot Nothing Then
                ReDim Channels(other.Channels.Length-1)
                For i = 0 To Channels.Length - 1
                    Channels(i) = New Channel(other.Channels(i))
                Next
            End If

            If other.xml IsNot Nothing Then
                xml = other.xml.Copy()
            End If

            If other.cc4set IsNot Nothing Then
                cc4set = other.cc4set.Clone()
            End If

            Filename  = other.Filename
            Filedate  = other.Filedate
            CoreSpeed = other.CoreSpeed
            BeamWidth = other.BeamWidth
            Curvature = other.Curvature

            'Stuff for select on curvature option. 
            'Was probably a one-time test
            SelectOnCurvature       = other.SelectOnCurvature 
            CurvatureLowerTolerance = other.CurvatureLowerTolerance
            CurvatureUpperTolerance = other.CurvatureUpperTolerance
            parameterstring         = other.parameterstring
        End Sub

        ''' <summary>
        ''' Function will check if the Dsp supports the type, if not an Invalid parameter exception is thrown.
        ''' </summary>
        ''' <param name="par"></param>
        Private Shared Sub VerifyDspSupport( par As Data.ParticleHandling.Channel.ChannelData.ParameterSelector) 
            If par = Data.ParticleHandling.Channel.ChannelData.ParameterSelector.TimeOfArrival Then
                        Throw New InvalidIIFParametersException(String.Format("Time of arrival is not supported for targetted imaging."))
            End If
            If   par = Data.ParticleHandling.Channel.ChannelData.ParameterSelector.SWSCOV  Then
                        Throw New InvalidIIFParametersException(String.Format("SWS Covariance is not supported for targetted imaging."))
            End If
        End Sub

        ''' <summary>
        ''' Creates and IIF-object from a selection set, ready to be serialized.
        ''' </summary>
        Public Sub New(cytosets As CytoSettings.CytoSenseSetting, selset As CytoSense.Data.Analysis.CytoSet)
            _cytosettings = cytosets
            'Do some checks on the validity of the set
            If Not selset.type = Data.Analysis.cytoSetType.gateBased Then
                Throw New InvalidIIFParametersException("Only single gatebased sets can be used in hardware.")
            End If
            Dim gbset = TryCast(selset, CytoSense.Data.Analysis.GateBasedSet)
            For Each g As CytoSense.Data.Analysis.Gate In gbset.allGates
                If Not (g.Type = Data.Analysis.GateType.Rectangle OrElse g.Type = Data.Analysis.GateType.Range) Then
                    Throw New InvalidIIFParametersException(String.Format("Only ranges and rectangles can be processed by the DSP, you used something else: {0}", g.ToString))
                End If
            Next

            'Ok make it
            Dim channelList As New List(Of CytoSettings.ChannelWrapper)
            Dim pars As New List(Of Data.ParticleHandling.Channel.ChannelData.ParameterSelector)
            Dim mins As New List(Of Single)
            Dim maxs As New List(Of Single)

            For i As Integer = 0 To gbset.allGates.Count - 1
                'For each gate
                If gbset.allGates(i).Type = Data.Analysis.GateType.Rectangle Then
                    Dim g = DirectCast(gbset.allGates(i), Data.Analysis.RectangleGate)
                    Dim xAx = TryCast(g.XAxis, Data.Analysis.SingleAxis)
                    Dim yAx = TryCast(g.YAxis, Data.Analysis.SingleAxis)
                    If Object.Equals(yAx, Nothing) OrElse Object.Equals(xAx, Nothing) Then
                        Throw New InvalidIIFParametersException(String.Format("Only axes with a single channel are supported, you tried to use {0} vs {1}", g.XAxis.Name, g.YAxis.Name))
                    End If
                    VerifyDspSupport(xAx.Parameter)
                    VerifyDspSupport(yAx.Parameter)
                    channelList.Add(xAx.Channel)
                    pars.Add(xAx.Parameter)
                    mins.Add(g.Outline(0).X) 'it starts with minimal, then clockwise, so 0 is min, 2 is max
                    maxs.Add(g.Outline(2).X)

                    channelList.Add(yAx.Channel)
                    pars.Add(yAx.Parameter)
                    mins.Add(g.Outline(0).Y) 'it starts with minimal, then clockwise, so 0 is min, 2 is max
                    maxs.Add(g.Outline(2).Y)
                ElseIf gbset.allGates(i).Type = Data.Analysis.GateType.Range Then
                    Dim g = DirectCast(gbset.allGates(i), Data.Analysis.RangeGate)
                    Dim Ax = TryCast(g.Axis, Data.Analysis.SingleAxis)
                    If Object.Equals(Ax, Nothing) Then
                        Throw New InvalidIIFParametersException(String.Format("Only axes with a single channel are supported, you tried to use {0}", g.Axis.Name))
                    End If
                    VerifyDspSupport(ax.Parameter)
                    channelList.Add(Ax.Channel)
                    pars.Add(Ax.Parameter)
                    maxs.Add(g.Max)
                    mins.Add(g.Min)
                End If
            Next

            Create(cytosets, channelWrappersToHardwareChannelsWithChecks(cytosets, channelList, pars), pars, mins, maxs)
            cc4set = selset
        End Sub

        ''' <summary>
        ''' Creates an IIF-object that can be directly serialized to file, using channellist channelwrappers
        ''' </summary>
        ''' <remarks>Throws InvalidIIFParametersException with descriptive text if the parameters are impossible. </remarks>
        Public Sub New(cytosets As CytoSettings.CytoSenseSetting, channelWrappers As List(Of CytoSense.CytoSettings.ChannelWrapper), parameters As List(Of CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), min As List(Of Single), max As List(Of Single))
            _cytosettings = cytosets
            Me.Create(cytosets, channelWrappersToHardwareChannelsWithChecks(cytosets, channelWrappers, parameters), parameters, min, max)
        End Sub

        ''' <summary>
        ''' Converts channelwrapper objects to their corresponding hardwarechannels, if possible. Otherwise throws an InvalidIIFParametersException with text.
        ''' </summary>
        ''' <param name="parameters">Needed because some parameter/channel parameters are allowable (TOF for example)</param>
        ''' <remarks>Newer (V10) electronics has no DSP so we do not actually convert to hardware channels, or indeed refuse the 
        ''' use of the TOF parameter.  So basically for newer electronics we allow much more. NOTE: We can probably support even more
        ''' here, but that requires more work.</remarks>
        Private Shared Function channelWrappersToHardwareChannelsWithChecks(cytosets As CytoSense.CytoSettings.CytoSenseSetting, channelWrappers As List(Of CytoSense.CytoSettings.ChannelWrapper), parameters As List(Of CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector)) As List(Of CytoSettings.channel)

            If cytosets.hasFJDataElectronics Then
                Return channelWrappers.Select( Function(cw) cw.ChannelInfo).ToList() ' For new electronics no filtering/processing is needed.
            End If ' Else older Cytosense, use existing code.

            Dim chanList As New List(Of CytoSettings.channel)
            For i As Integer = 0 To channelWrappers.Count - 1
                If parameters(i) = Data.ParticleHandling.Channel.ChannelData.ParameterSelector.SampleLength Then
                    'Dodgy but allowable: we can easily use a different hwchannel for this parameter. Get the first 'visible' one:
                    For Each c As CytoSettings.channel In cytosets.channels
                        If c.visible Then
                            chanList.Add(c)
                            Exit For
                        End If
                    Next

                ElseIf cytosets.hasCurvature AndAlso channelWrappers(i).Channeltype = cytosense.CytoSettings.ChannelTypesEnum.FWS Then
                    'Apparently this is not true: (Mantis #308)
                    'Throw New InvalidIIFParametersException("The FWS channel is not accessible by the DSP in machines with curvature.")
                    'But we need to code it as a FWS L channel:
                    For c As Integer = 0 To cytosets.channels.Length - 1
                        If cytosets.channels(c).name.ToUpper.Contains("FWS L") Then
                            chanList.Add(cytosets.channels(c))
                            Exit For
                        End If
                    Next
                Else
                    If channelWrappers(i).IsHWChannel Then
                        chanList.Add(cytosets.channels(channelWrappers(i).HW_channel_id))
                    Else
                        If channelWrappers(i).VirtualChannelType = CytoSense.CytoSettings.VirtualChannelInfo.VirtualChannelType.LF_Filtered Then
                            ' Special case, this virtual channel, is sortof a hardware channel as well.
                            chanList.Add(cytosets.channels(channelWrappers(i).HW_channel_id))
                        Else
                            Throw New InvalidIIFParametersException(String.Format("The channel {0} is not available in the hardware.", channelWrappers(i).Description))
                        End If
                    End If
                End If
            Next

            Return chanList

        End Function


        ''' <summary>
        ''' Creates an IIF-object that can be directly serialized to file, using hardware channels
        ''' </summary>
        ''' <remarks>Throws InvalidIIFParametersException with descriptive text if the parameters are impossible. </remarks>
        Public Sub New(cytosets As CytoSettings.CytoSenseSetting, hardwareChannels As List(Of CytoSense.CytoSettings.channel), parameters As List(Of CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), min As List(Of Single), max As List(Of Single))
            _cytosettings = cytosets
            Create(cytosets, hardwareChannels, parameters, min, max)
        End Sub

        Private Sub Create(cytosets As CytoSettings.CytoSenseSetting, triggerChannels As List(Of CytoSense.CytoSettings.channel), parameters As List(Of CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), min As List(Of Single), max As List(Of Single))
            If triggerChannels.Count <> parameters.Count OrElse parameters.Count <> min.Count OrElse min.Count <> max.Count Then
                Throw New InvalidIIFParametersException("Something is wrong in the definition of the IIF parameters: the lists are of unequal length")
            End If

            Dim machineChannels() As CytoSettings.channel
            Dim maxChannelNumber = 0 
            If cytosets.hasFJDataElectronics Then ' Has no limit all available channels are allowed.
                machineChannels  = cytosets.ChannelList.Select(Function (cw) cw.ChannelInfo).ToArray()
                maxChannelNumber = cytosets.ChannelList.Count
            Else 'The DSP can access only the first 6 data channels of a machine,
                maxChannelNumber = Math.Min(6, cytosets.channels.Length - 1)
                machineChannels  = cytosets.channels
                For i As Integer = 0 To triggerChannels.Count - 1
                    If cytosets.getChannelIndex(triggerChannels(i).name) > 6 Then '0 is trigger1, so 6 is 6th datachannel
                        Throw New InvalidIIFParametersException("One of the selected channels is not accessible by the DSP, please use only the six channels first in line.")
                    End If
                Next
            End If
            ReDim Me.Channels(maxChannelNumber-1) 

            Me.SelectOnCurvature = False

            Dim u As Integer = 0
            For i As Integer = 0 To Me.Channels.Length - 1 'For all channels
                If machineChannels(u).name.Contains("Trigger") Then
                    u += 1
                    ' Continue For 'Skip this channel
                End If

                Me.Channels(i).name = machineChannels(u).name 'Channel 0 in settings.channels is trigger1, 0 for DSP is FWS (L)
                ReDim Me.Channels(i).Parameters(9)

                'Find index of current channel
                Dim curChannel As Channel = Me.Channels(i)
                Dim id As Integer = triggerChannels.FindIndex(Function(item As CytoSettings.channel) item.name = curChannel.name)
                Dim thisChansIds As New List(Of Integer)
                While id <> -1
                    thisChansIds.Add(id)
                    id = triggerChannels.FindIndex(Math.Min(id + 1, triggerChannels.Count), Function(item As CytoSettings.channel) item.name = curChannel.name)
                End While

                'Write parameters
                For j As Integer = 0 To 9 'All parameters (Did not parametrize because DSP depends on this to be 9)
                    If thisChansIds.Count > 0 Then
                        'There is a channel, check if this parameter is needed
                        Dim found As Boolean = False
                        For k As Integer = 0 To thisChansIds.Count - 1
                            If parameters(thisChansIds(k)) = j Then
                                Me.Channels(i).Parameters(j) = New Parameter(Data.ParticleHandling.Channel.ChannelData.ParameterNames(j), True, min(thisChansIds(k)), max(thisChansIds(k)))
                                found = True
                            End If
                        Next
                        If Not found Then
                            Me.Channels(i).Parameters(j) = New Parameter(Data.ParticleHandling.Channel.ChannelData.ParameterNames(j), False, 0, 0)
                        End If
                    Else
                        'Not selected
                        Me.Channels(i).Parameters(j) = New Parameter(Data.ParticleHandling.Channel.ChannelData.ParameterNames(j), False, 0, 0)

                    End If

                Next
                u += 1
            Next

            Me.CoreSpeed = cytosets.SampleCorespeed
            Me.BeamWidth = cytosets.LaserBeamWidth
            Me.Filedate = Now

            placeReadableString()
        End Sub

        <Serializable()> Structure Parameter
            Public Sub New(ByVal ParameterName As String, ByVal Enable As Boolean, ByVal MinValue As Single, ByVal MaxValue As Single)
                Me.name = ParameterName
                Me.enable = Enable
                Me.max = MaxValue
                Me.min = MinValue
            End Sub
            Public Sub New( other As Parameter)
                name   = other.name
                enable = other.enable
                max    = other.max
                min    = other.min
            End Sub
            Dim name As String
            Dim enable As Boolean
            Dim min As Single
            Dim max As Single

            Public Overrides Function ToString() As String
                Dim s As String = "Off, "
                If enable Then
                    s = "On, "
                End If
                Return s & name & ": " & min & " - " & max
            End Function

        End Structure

        <Serializable()> Structure Channel
            Public Sub New(ByVal ChannelName As String, ByVal Parameters() As Parameter)
                Me.Parameters = Parameters
                Me.name = ChannelName
            End Sub
            Public Sub New( other As Channel)
                name = other.name
                If other.Parameters IsNot Nothing Then
                    Redim Parameters(other.Parameters.Length-1)
                    For i = 0 To Parameters.Length - 1
                        Parameters(i) = New Parameter(other.Parameters(i))
                    Next
                End If
            End Sub
            Dim name As String
            Dim Parameters As Parameter()

            Public Overrides Function ToString() As String
                If Parameters IsNot Nothing Then
                    For i = 0 To Parameters.Length - 1
                        If Parameters(i).enable Then
                            Return "Enabled IIF channel"
                        End If
                    Next
                    Return "Disabled IIF channel"
                Else
                    Return "Empty IIF channel"
                End If
            End Function
        End Structure

        Dim Channels() As Channel 'Channel and parameter in one construct

        Dim xml As DataSet ' The cc3 workspace as xml
        Dim cc4set As CytoSense.Data.Analysis.CytoSet ' The cc4 workspace 

        Dim Filename As String
        Dim Filedate As Date
        Dim CoreSpeed As Single
        Dim BeamWidth As Single
        Dim Curvature As Boolean

        'Stuff for select on curvature option. 
        'Was probably a one-time test
        Dim SelectOnCurvature As Boolean
        Dim CurvatureLowerTolerance As Single
        Dim CurvatureUpperTolerance As Single


        Dim parameterstring As String ' Text representation of the selection data

        Private Const FLOAT_WIDTH As Integer = 14

        ''' <summary>
        ''' Generates the readable string representation of the selected parameters, and places it in this object. Also returns that value.
        ''' </summary>
        Public Function placeReadableString() As String
            'Een stukje plaintext zodat mensen ook begrijpen wat hier staat
            Dim t As String = ""
            t = t & LSet("Parameter", 27) & LSet("Enable", 8) & LSet("Min", FLOAT_WIDTH) & LSet("Max", FLOAT_WIDTH) & vbCrLf
            t = t & "=======================================================" & vbCrLf

            For i As Integer = 0 To Channels.Length - 1 ' Channels
                For j As Integer = 0 To 9 ' Parameters
                    t = t & LSet(Channels(i).name & ", " & CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterNames(j) & ": ", 27) & LSet(Channels(i).Parameters(j).enable.ToString(), 8) & LSet(Channels(i).Parameters(j).min.ToString(), FLOAT_WIDTH) & LSet(Channels(i).Parameters(j).max.ToString(), FLOAT_WIDTH) & vbCrLf
                Next j
            Next i
            parameterstring = t
            Return parameterstring
        End Function

        Public Sub saveToFile(path As String)
            Me.Filename = path
            CytoSense.Serializing.SaveToFile(path, Me)
        End Sub
    End Structure
End Namespace
