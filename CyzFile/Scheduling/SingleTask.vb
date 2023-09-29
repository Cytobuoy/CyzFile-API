Namespace Scheduling

    <Serializable()> Public Class SingleTask
        Inherits MeasurementTask



        Private _done As Boolean = False


        Public Overrides Function TimeToStart() As TimeSpan
            Dim t As New TimeSpan(0, CInt(DateDiff(DateInterval.Minute, Now, Start)), 0)
            Return t
        End Function

        Public Overrides Function Check() As Boolean
            If done Then
                Return True
            End If
            If DateDiff(DateInterval.Minute, Start, Now) > 5 Then
                setProblemReport("TimedOut")
                done = True
                Return False
            End If
            Return True
        End Function

        Public Overrides Property Done() As Boolean
            Get
                Return _done
            End Get
            Set(ByVal value As Boolean)
                _done = value
            End Set
        End Property

        Public Overrides Function isExternalTriggerTask() As Boolean
            Return False
        End Function

        Public Overrides Function IsIntervalMeasurement() As Boolean
            Return False
        End Function

        Public Overrides Function IsSpecificTimeMeasurement() As Boolean
            Return True
        End Function

        Public Overrides Function ShouldRun( t As DateTime, tolerance As TimeSpan, scheduler As ITaskScheduler  ) As Boolean 
            Return Not done AndAlso Check() AndAlso Start < t AndAlso scheduler.getLocationIsOK(Me)
        End Function

        Public Overrides Sub Run(scheduler As ITaskScheduler)
            _done = True
            Dim m = getMeasurements
            m.ScheduleTask = Me
            scheduler.Acquire(m, ShutDown_Afterwards, CytoSense.MeasurementSettings.StartedBy.Schedule)
        End Sub



    End Class

End Namespace