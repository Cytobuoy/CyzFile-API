Imports System
Imports System.Collections.Generic

Namespace Scheduling

''' <summary>
''' Run a sheath cleaning
''' </summary>
''' <remarks>THe task can be both an interval task, and single task.  The interval task
''' "logic" is copied form the interval task, and the single task as well. Not
''' very pretty but I did not feel like creating two classes.
''' I tried to create a single interface that can be used by all classes.
''' So we can remove the If Else If ... chain in the scheduler.
''' Perhaps the one shot can also be handled using an interval, and not a flag.
''' </remarks>

<Serializable()> _
Public Class SheathCleaningTask
    Inherits task

        Public Enum ScheduleType
            Invalid = 0
            SingleShot
            Repeating
        End Enum

        ''' <summary>
        ''' Create a new SheatCleaning Task.
        ''' </summary>
        ''' <param name="schedule">Is this a one shot, or a repeating measurement!</param>
        ''' <remarks></remarks>
        Public Sub New( schedule As ScheduleType, injectChlorine As Boolean )
            _scheduleType   = schedule
            _injectChlorine = injectChlorine
        End Sub

        Public Overrides Function getMeasurements() As MeasurementSettings.Measurements
            Throw New NotImplementedException()
        End Function
        Public Overrides Sub SetMeasurements(m As MeasurementSettings.Measurements)
            Throw New NotImplementedException()
        End Sub

        Public Overrides ReadOnly Property Number_Of_Measurements As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overrides Sub ReloadCytoSettings(c As CytoSettings.CytoSenseSetting)
                'Nothing to do here. Only needed For Measurment tasks....
        End Sub


        Public Overrides Function IsShutdownTask() As Boolean
                Return False
        End Function

        Public Overrides Function IsMeasurementTask() As Boolean
                Return False
        End Function

        Public Overrides Function IsExternalTriggerTask() As Boolean
                Return False
        End Function

        Public Overrides Function IsSpecificTimeMeasurement() As Boolean
                Return _scheduleType = ScheduleType.SingleShot
        End Function

        Public Overrides Function IsIntervalMeasurement() As Boolean
                Return _scheduleType = ScheduleType.Repeating
        End Function


        ''' <summary>
        ''' Return True if the task is currently active, I.e. the task is
        '''   - Enabled
        '''   - Start LT Now and Now LT Finish
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function IsActive() As Boolean
            Return Enabled AndAlso Now > Start AndAlso Now < Finish
        End Function

        ''' <summary>
        ''' Check if there is a measurement planned for this task in the specified time interval.
        ''' If there is no task planed within the specified interval then the sense can be powered
        ''' down.
        ''' </summary>
        ''' <param name="shutdownDelay">Interval in seconds</param>
        ''' <returns></returns>
        Public Function InstrumentCanShutDown(shutdownDelay As Integer) As Boolean

            If Not IsActive() Then
                Return True
            End If

            Dim rightDay As Boolean = False
            Dim rightHour As Boolean = False
            Dim rightMinute As Boolean = False
            Dim NoLastStartProblem As Boolean = DateDiff(DateInterval.Minute, LastStarted, Now) > 5

            Dim mindiff As Integer = Integer.MaxValue
            For i = 0 To _DayList.Length - 1
                Dim d(6) As DayOfWeek
                d(0) = DayOfWeek.Sunday
                d(1) = DayOfWeek.Monday
                d(2) = DayOfWeek.Tuesday
                d(3) = DayOfWeek.Wednesday
                d(4) = DayOfWeek.Thursday
                d(5) = DayOfWeek.Friday
                d(6) = DayOfWeek.Saturday

                Dim daydiff As Integer = 0
                For daydiff = 0 To 6
                    If Now.AddDays(daydiff).DayOfWeek = d(_DayList(i)) Then
                        Exit For
                    End If
                Next



                Dim tmp As New DateTime(Now.Year, Now.Month, Now.Day, 0, 0, 0)
                If Math.Abs(DateDiff(DateInterval.Day, Now, tmp)) < 2 Then
                    For j = 0 To _HourList.Length - 1
                        Dim tmpd As New DateTime(tmp.Year, tmp.Month, tmp.Day, _HourList(j), 0, 0)
                        If Math.Abs(DateDiff(DateInterval.Minute, Now, tmpd)) < 61 Then
                            For k = 0 To _MinuteList.Length - 1
                                Dim tmpdh As New DateTime(tmp.Year, tmp.Month, tmp.Day, _HourList(j), _MinuteList(k), 0)

                                If mindiff > DateDiff(DateInterval.Second, Now, tmpdh) And DateDiff(DateInterval.Second, Now, tmpdh) > 0 Then
                                    mindiff = CInt(DateDiff(DateInterval.Second, Now, tmpdh))
                                End If
                            Next
                        End If
                    Next
                End If
            Next

            If mindiff < shutdownDelay Then
                Return False
            Else
                Return True
            End If

        End Function


        Public Overrides Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal n As String)
                _name = n
            End Set
        End Property


        ' Single task has a property done, and it checks if the
        ' Start < now, AND not done  AND not more then 5 minutes past start.
        ' So basicly it is run oncein an interval [start, start+5>
        Public Overrides Property Done() As Boolean
            Get
                Return _done
            End Get
            Set(ByVal value As Boolean)
                _done = value
            End Set
        End Property

        ''' <summary></summary>
        ''' <returns></returns>
        ''' <remarks>Only used for SingleShot, copied from SingleTask implementation</remarks>
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


        Public Function LastStarted() As Date
            If Object.Equals(Nothing, _done_at) Then
                Return New Date(0)
            ElseIf _done_at.Count = 0 Then
                Return New Date(0)
            Else
                Return _done_at(_done_at.Count-1)
            End If
        End Function

        ''' <summary>
        ''' Returnst True if the task should be started NOW.
        ''' </summary>
        ''' <returns></returns>
        Public Function RequestStart(t As DateTime, tolerance As TimeSpan, ignorelaststart As Boolean) As Boolean
            If Start > t Or Finish < t Then
                Return False
            End If

            ' Need at least 9 minutes since the previous start ... (why?), because we can only schedule every 10 minutes,
            ' in the current setup, if I add 15 and 45 as options, then it should probably be < 4 or so.
            If  (t - LastStarted) < New TimeSpan(0,9,0) Then
                Return False
            End If

            Dim NoLastStartProblem As Boolean = DateDiff(DateInterval.Minute, lastStarted, t) > 5
            If ignorelaststart Then
                NoLastStartProblem = True '[R] Not sure what a last start problem is...
            End If

            Dim generator = New ScheduleGenerator( start, finish, _DayList, _HourList, _MinuteList )
            Dim dateOK = generator.IsInSchedule(t, tolerance)
            Return dateOK And NoLastStartProblem
        End Function


        ' This is can be a one shot, or an interval task.  To keep the implementation as close to the
        ' other tasks as possible, I copied both the single task functions and the IntervalTask
        ' Functions.
        ' In theory could create strategy classes for one shot/interval, and extract that logic
        ' from the actual tasks.  Separating scheduling and the actual task are something to consider
        ' in the future.
        Public Overrides Function ShouldRun( t As DateTime, tolerance As TimeSpan, scheduler As ITaskScheduler  ) As Boolean
            If _scheduleType = ScheduleType.SingleShot Then
                Return Not done AndAlso Check() AndAlso Start < t AndAlso scheduler.getLocationIsOK(Me)
            Else If _scheduleType = ScheduleType.Repeating Then
                Return RequestStart(t, tolerance, False) AndAlso scheduler.getLocationIsOK(Me)
            Else
                Throw New ArgumentException("Unknown task type","Me._scheduleType")
            End If
        End Function

        Public Overrides Sub Run(scheduler As ITaskScheduler)

            If _scheduleType = ScheduleType.SingleShot Then
                done = True
                scheduler.CleanSheath(_injectChlorine, ShutDown_Afterwards)
            Else If _scheduleType = ScheduleType.Repeating Then
                Started(Now)
                scheduler.CleanSheath(_injectChlorine, ShutDown_Afterwards )
            Else
                Throw New ArgumentException("Unknown task type","Me._scheduleType")
            End If

        End Sub

        Public Sub Started(ByVal d As Date)
            _done_at.Add(d)
        End Sub

        Public Property InjectChlorine As Boolean
            Get
                Return _injectChlorine
            End Get
            Set (value As Boolean)
                _injectChlorine = value
            End Set
        End Property

        Private _name As String
        Private _scheduleType As ScheduleType
        Private _injectChlorine As Boolean = True

        ' Single Task Properties
        Private _done As Boolean
        'Interval Task Properties
        Private _done_at As New List(Of Date)()

End Class

End Namespace
