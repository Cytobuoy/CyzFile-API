Imports System.Runtime.Serialization

Namespace Scheduling


    <Serializable()> Public Class IntervalTask

        Inherits MeasurementTask

        Private _done_at As New CytoCollection()

          Public Overrides Function isIntervalMeasurement() As Boolean
            Return True
        End Function


        Public Overrides Function Check() As Boolean
            Return True
        End Function

        Public Overrides Property done() As Boolean
            Get
                Return False
            End Get
            Set(ByVal value As Boolean)

            End Set
        End Property

        Public Sub Started(ByVal d As Date)
            _done_at.Add(d)
        End Sub

        ''' <summary>
        ''' Function not really pretty, we should be able to make smarter.
        ''' Returns the time to the next start of this task, with a maximum of 1 week in
        ''' the future.  That is because we use a week as the basics for our scheduling.
        ''' If there is no start time in the next week the function returns a timespan
        ''' that is negative.  NOTE: probably -1, but the only guarantee is negative.
        ''' There is no relation to the last start time e.g.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function TimeToStart() As TimeSpan
            Dim tolerance As New TimeSpan(0,1,0) '1 minute
            Dim t0 As DateTime = Now
            For i = 0 To 10080 + 10 'one week in minutes (+ 10 minutes)

                Dim tn As DateTime = t0.AddMinutes(i)
                If i > 0 Then
                    tn = tn.Subtract(New TimeSpan(0, 0, t0.Second))
                End If

                If RequestStart(t0.AddMinutes(i),tolerance, True) Then
                    Dim t As New TimeSpan(0, i, 0)
                    Return t
                End If
            Next
            Return New TimeSpan(-1)
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

			If IsContinuous Then
				Return False
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


        ''' <summary>
        ''' Only public for testing! The actual implementation of RequestStart, use that function instead!!
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="ignorelaststart"></param>
        ''' <returns></returns>
        ''' <remarks>The function takes some state as parameter, to make automated testing
        ''' easier, not everything, just things I currently want to tryout in testing..
        ''' When we are starting because of a turn on timer, we use a fuzzy matching,
        ''' there will be a 10 minute plus or minus used for testing.  That way
        ''' the clocks do not have be perfectly synchronized.
        ''' 
        ''' The function checks each date by checking if there is a schedule date
        ''' within the matching window.  Unfortunately we do not have a list of scheduled
        ''' events, so it takes a bit of work.
        ''' 
        ''' A nicer way would be to generate the Next n events, (and keep the generator
        ''' as a member)  Then we would know what is coming, and we would only have to
        ''' compare to the first one.  That would be much faster.
        ''' But the current solution works, and the other one requires more
        ''' time which I do not have at the moment.
        ''' 
        ''' </remarks>
        Public Function RequestStart(t As DateTime, tolerance As TimeSpan, ignorelaststart As Boolean) As Boolean
            If Start > t Or Finish < t Then
                Return False
            End If

			If IsContinuous Then
				Return True
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

        Public Function LastStarted() As Date
            If Object.Equals(Nothing, _done_at) Then
                Return New Date(0)
            ElseIf _done_at.Count = 0 Then
                Return New Date(0)
            Else
                Return CType(_done_at.Last(),Date)
            End If
        End Function


        Public Overrides Function isExternalTriggerTask() As Boolean
            Return False
        End Function

        Public Overrides Function IsSpecificTimeMeasurement() As Boolean
            Return False
        End Function

        Public Overrides Function ShouldRun( t As DateTime, tolerance As TimeSpan, scheduler As ITaskScheduler ) As Boolean
            Return RequestStart(t, tolerance,False) AndAlso scheduler.getLocationIsOK(Me)
        End Function

        Public Overrides Sub Run(scheduler As ITaskScheduler)
            Started(Now)
            Dim m = getMeasurements
            m.ScheduleTask = Me
            scheduler.Acquire(m, ShutDown_Afterwards, CytoSense.MeasurementSettings.StartedBy.Schedule)
        End Sub


    End Class

    ''' <summary>
    ''' Small Utility class that you initialize with the schedule dates, and that
    ''' generates new valid dates when you call next.  It will start with 
    ''' If you want to reuse it, you need to call reset.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ScheduleGenerator
        Public Sub New( start As DateTime, finish As DateTime, dayList As Integer(), hourList As Integer(), minuteList As Integer())
            _start      = start
            _finish     = finish
            _dayList    = dayList
            _hourList   = hourList
            _minuteList = minuteList
            Reset()
        End Sub

        Public Sub Reset()
            _done = False
            _dayIdx = 0
            _hourIdx = 0
            _minuteIdx = 0
            _weekStart = _start.Date - New TimeSpan(CInt(_start.DayOfWeek),0,0,0)
            CalculateNextDate()
            While _currentDate < _start AndAlso Not _done 
                CalculateNextDate()
            End While
        End Sub
        Public Function Finished() As Boolean
            Return _done
        End Function
        Public Function GetNextEntry As DateTime
            If _done Then
                Throw New Exception("Reached End of schedule")
            End If

            Dim dt = _currentDate
            CalculateNextDate()
            Return dt
        End Function

        ''' <summary>
        ''' It was a loop that went through everything, we are sort of building an Enumerator.
        ''' In C# we could use yield, but I do not know how that works.
        ''' It was a triple nested loop.
        ''' The indexes should be setup for the next time to create. We do that then
        ''' we increment the indexes and check if we reach the end.
        ''' 
        ''' Look at when and how the indexes are incremented.
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub CalculateNextDate()
            Dim checkDate = _weekStart + New TimeSpan(_dayList(_dayIdx), _hourList(_hourIdx), _minuteList(_minuteIdx), 0)
            If checkDate > _finish Then
                _done = True
                Return
            End If
            If _minuteIdx < _minuteList.Count - 1 Then
                _minuteIdx += 1
            Else
                _minuteIdx = 0
                If _hourIdx < _hourList.Count - 1 Then
                    _hourIdx += 1
                Else
                    _hourIdx = 0
                    If _dayIdx < _dayList.Count - 1 Then
                        _dayIdx += 1
                    Else
                        _dayIdx = 0
                        _weekStart += New TimeSpan(7,0,0,0)
                    End If
                End If
            End If

            _currentDate = checkDate
        End Sub


        ''' <summary>
        ''' Check if checkTime parameter is in the schedule or not. (maxDistance is the tolerance)
        ''' </summary>
        ''' <param name="checkTime"></param>
        ''' <param name="maxDistance"></param>
        ''' <returns></returns>
        ''' <remarks>If checkTime is withing maxDistance from a point in the schedule the function
        ''' returns true, else false.
        ''' We need to find the closest point in the schedule to checkTime and see if that is
        ''' in range. I can generate them all and check, but that takes a long time. The best way
        ''' is probably to start from the checkTime and generate the ones before and after and
        ''' then compare.
        ''' </remarks>
        Public Function IsInSchedule(checkTime As DateTime, maxDistance As TimeSpan) As Boolean
            If checkTime < _start OrElse checkTime > _finish Then
                Return False
            End If

            Dim tmMinute = New TimeSpan(0,1,0)
            ' Before', check all entries before until maxDistance or until we find it..
            Dim currTime = checkTime
            While (checkTime - currTime) <= maxDistance 
                Dim dow = CInt(currTime.DayOfWeek)
                Dim h = CInt(currTime.Hour)
                Dim m = CInt(currTime.Minute)
                If _dayList.Contains(dow) AndAlso _hourList.Contains(h) AndAlso _minuteList.Contains(m) Then
                    Return True
                End If
                currTime -= tmMinute
            End While
            ' Now check after entries.
            currTime = checkTime + tmMinute ' We checked equals before.
            While (currTime - checkTime) <= maxDistance 
                Dim dow = CInt(currTime.DayOfWeek)
                Dim h = CInt(currTime.Hour)
                Dim m = CInt(currTime.Minute)
                If _dayList.Contains(dow) AndAlso _hourList.Contains(h) AndAlso _minuteList.Contains(m) Then
                    Return True
                End If
                currTime += tmMinute
            End While
            Return False
        End Function

        Private ReadOnly _start As DateTime
        Private ReadOnly _finish As DateTime
        Private ReadOnly _dayList As Integer() 
        Private ReadOnly _hourList As Integer() 
        Private ReadOnly _minuteList As Integer()
        'Algorithm state variables.
        Private _done As Boolean = False
        Private _currentDate As DateTime
        Private _weekStart As DateTime
        Private _dayStart As DateTime
        Private _hourStart As DateTime
        Private _minuteStart As DateTime
        Private _dayIdx As Integer
        Private _hourIdx As Integer
        Private _minuteIdx As Integer
    End Class

End Namespace
