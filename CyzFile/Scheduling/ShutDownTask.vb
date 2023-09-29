Namespace Scheduling

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' Currently inherits all the measurement stuf from interval task, but this is not needed, perhaps change
    ''' inheritence hierarchy a bit.
    ''' </remarks>

    <Serializable()> Public Class ShutDownTask
        Inherits IntervalTask

        Public Overrides Property ShutDown_Afterwards() As Boolean
            Get
                Return True
            End Get
            Set(ByVal value As Boolean)
            End Set
        End Property


        Public Overrides Function Check() As Boolean
            Return True
        End Function

        Public Overrides Property Done() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)

            End Set
        End Property


        Public Overrides Function isIntervalMeasurement() As Boolean
            Return True
        End Function

        Public Overrides Property Name() As String
            Get
                Return "Shutdown"
            End Get
            Set(ByVal value As String)

            End Set
        End Property

        Public Overrides ReadOnly Property Number_Of_Measurements() As Integer
            Get
                Return 0
            End Get
        End Property



        Public Overrides Function isShutdowntask() As Boolean
            Return True
        End Function



        Public Overrides Sub Run(scheduler As ITaskScheduler)
            Started(Now)
            scheduler.ShutDown( Me )
        End Sub


    End Class

End Namespace