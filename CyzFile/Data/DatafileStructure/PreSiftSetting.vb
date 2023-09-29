Namespace Data
    'depriciated, keep for old datafiles
    'replaced by CytoSense.MeasurementSettings.SmartTriggerSettings
    <Serializable()> Public Class PreSiftSetting

        Dim _channel As CytoSense.CytoSettings.channel
        Dim _channelid As Integer
        Dim _parameter As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector
        Dim _minimumValue As Int32 = 0
        Dim _maximumValue As Int32 = 0


        Sub New(ByVal channel As CytoSense.CytoSettings.channel, ByVal par As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector, ByVal minimum As Int32, ByVal maximum As Int32, ByVal channelID As Integer)
            _channel = channel
            _parameter = par
            _minimumValue = minimum
            _maximumValue = maximum
            _channelid = channelID
        End Sub

        ''' <summary>
        ''' Copy CTor
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New(other As PreSiftSetting)
            _channel = other._channel 'Shalow copy
            _channelid = other._channelid
            _parameter = other._parameter
            _minimumValue = other._minimumValue
            _maximumValue = other._maximumValue
        End Sub

        ''' <summary>
        ''' Returns the channel id in the machine channel list. Only use when sure that machine can not have changed since creating of the presiftsettings! Otherwise use .channel
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ChannelID As Integer
            Get
                Return _channelid
            End Get
        End Property

        Public ReadOnly Property Channel As CytoSense.CytoSettings.channel
            Get
                Return _channel
            End Get
        End Property

        Public ReadOnly Property Parameter As CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterSelector
            Get
                Return _parameter
            End Get
        End Property

        Public ReadOnly Property Minimum As Int32
            Get
                Return _minimumValue
            End Get
        End Property


        Public ReadOnly Property Maximum As Int32
            Get
                Return _maximumValue
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Channel.name & " " & Parameter.ToString & " > " & Minimum
        End Function
    End Class

End Namespace