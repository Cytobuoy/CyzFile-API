Imports CytoSense.Data.ParticleHandling.Channel

Namespace MeasurementSettings


    ''' <summary>
    ''' If multiple criteria are used for a smart trigger you can specify if they should be combined using
    ''' an And or an Or function. That way you can e.g. require fluorescence, but any channel will do.
    ''' </summary>
    Public Enum SmartTriggerCombinationT 
        SmartTriggerAnd = 0  ' Default, used to be the only one, make it defautl so loading oild stuff works.
        SmartTriggerOr
    End Enum

    <Serializable()> Public Class SmartTriggerSettings


        Dim _channelID As Integer
        Dim _channelName As String
        Dim _parameter As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector
        Dim _minimumValue As Int32 = 0 'legacy
        Dim _maximumValue As Int32 = 0 'legacy
        Dim _minimumValue_single As Single = 0
        Dim _maximumValue_single As Single = 0
        Dim _accessMode As CytoSense.CytoSettings.ChannelAccessMode

        Sub New(ByVal channel As CytoSense.CytoSettings.ChannelWrapper, ByVal par As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector, ByVal minimum As Single, ByVal maximum As Single)
            _channelID = channel.ID
            _channelName = channel.ChannelInfo.name
            _parameter = par
            _minimumValue_single = minimum
            _maximumValue_single = maximum
        End Sub

        Public Sub New( other As SmartTriggerSettings )
            _channelID           = other._channelID
            _channelName         = other._channelName
            _parameter           = other._parameter
            _minimumValue        = other._minimumValue
            _maximumValue        = other._maximumValue
            _minimumValue_single = other._minimumValue_single
            _maximumValue_single = other._maximumValue_single
            _accessMode          = other._accessMode
        End Sub

        ''' <summary>
        ''' Contains the name of the used channel 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ChannelName As String
            Get
                Return _channelName
            End Get
        End Property

        Public ReadOnly Property ParameterName As String
            Get
                Return ChannelData.ParameterNames(Parameter)
            End Get
        End Property

         <ComponentModel.Browsable(False)>
        Public ReadOnly Property Parameter As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector
            Get
                Return _parameter
            End Get
        End Property

        Public ReadOnly Property Minimum As Single
            Get
                If _minimumValue_single = 0 Then 'legacy
                    _minimumValue_single = _minimumValue
                End If

                Return _minimumValue_single
            End Get
        End Property

        ''' <summary>
        ''' Maximum is not used at the moment.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>Browsable set to false, it is not used, so we should not display it to the user.</remarks>
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property Maximum As Single
            Get
                If _maximumValue_single = 0 Then 'legacy
                    _maximumValue_single = _maximumValue
                End If
                Return _maximumValue_single
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _channelName & " " & ChannelData.ParameterNames(parameter) & " > " & Minimum
        End Function

        ''' <summary>
        ''' Contains the ID of the used channel in cytosettings.channellist
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Browsable set to false so it is not displayed in the smartgrid settings dialog.</remarks>
        <ComponentModel.Browsable(False)>
        Public ReadOnly Property ChannelListID As Integer
            Get
                Return _channelID
            End Get
        End Property

    End Class
End Namespace