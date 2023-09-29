Namespace Scheduling

    ''' <summary>
    ''' The interface the tasks can use to callback to the scheduler.
    ''' </summary>
    ''' <remarks>Not a real pretty solution, but there are some issues because all the tasks are in
    ''' the CytoSettings DLL and the rest of the logic is not.  This makes it impossible to implement
    ''' the logic in the task classes themselves instead of in the scheduler.  Doing this way is
    ''' sort of a half ass solution, but it removes the need for the if on task type in
    ''' the scheduler.</remarks>
    Public Interface ITaskScheduler
        Function getLocationIsOK(task As CytoSense.Scheduling.task) As Boolean ' Call this to perform a location check for task.
        Sub Acquire(m As CytoSense.MeasurementSettings.Measurements, ShutDownComputerAfterwards As Boolean, startedby As CytoSense.MeasurementSettings.StartedBy)
        Sub CleanSheath( injectChlorine As Boolean, shutdownComputerAfterwards As Boolean) 'Run a sheath cleaning cycle, pass true to also inject chlorine auto-magically.
        Sub Shutdown( t As Task ) 'Stop everything and shutdown the computer
    End Interface

    <Serializable()> Public MustInherit Class task

        Private _StartDate As DateTime
        Private _FinishDate As DateTime
		Private _isContinuous as boolean = False

        Private _ProblemReport As String = ""
        Private _shutdownAfterTask As Boolean
        Private _BackFlush As Boolean

        Private _GPSset As List(Of ScheduleGPSSetting)


        Friend _DayList() As Integer
        Friend _HourList() As Integer
        Friend _MinuteList() As Integer

        Public MustOverride Function IsIntervalMeasurement() As Boolean
        Public MustOverride Function IsSpecificTimeMeasurement() As Boolean
        Public MustOverride Function IsShutdownTask() As Boolean

        ''' <summary>
        ''' True if the task requires a measurement object, false if not.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>NOTE: A new property, for existing tasks it is the same
        ''' as NOT IsShutDownTask, so I provided that as default implementation for now.
        ''' </remarks>
        Public Overridable Function IsMeasurementTask() As Boolean
            Return Not IsShutdownTask() ' Works for current tasks, only the new SheathCleaning does not work correctly here.
        End Function
        Public MustOverride Function IsExternalTriggerTask() As Boolean
        Public MustOverride Function Check() As Boolean

        Public Property Enabled As Boolean

        Public MustOverride Sub ReloadCytoSettings(c As CytoSense.CytoSettings.CytoSenseSetting)

        ''' <summary>
        ''' Check if the current task should run at the specified date and time.
        ''' </summary>
        ''' <param name="t">The time to check for running.</param>
        ''' <param name="tolerance">The maximum tolerance to use when comparing start times</param>
        ''' <param name="scheduler">The scheduler that wants to know. Used e.g. to check the location if GPS conditions exist.</param>
        ''' <returns>True if the task should be run, false if not.</returns>
        ''' <remarks></remarks>
        Public MustOverride Function ShouldRun( t As DateTime, tolerance As TimeSpan,  scheduler As ITaskScheduler ) As Boolean

        ''' <summary>
        ''' Run this task.
        ''' </summary>
        ''' <param name="scheduler">The scheduler that wants to run.</param>
        ''' <remarks>The tasks uses actual callbacks to the scheduler to do it's work.</remarks>
        Public MustOverride Sub Run(scheduler As ITaskScheduler)


        Public Sub setDayList(ByVal l() As Integer)
            _DayList = l
        End Sub
        Public Sub setHourList(ByVal l() As Integer)
            _HourList = l
        End Sub
        Public Sub setMinuteList(ByVal l() As Integer)
            _MinuteList = l
        End Sub

        Public Function getDayList() As Integer()
            Return _DayList
        End Function
        Public Function getHourList() As Integer()
            Return _HourList
        End Function
        Public Function getMinuteList() As Integer()
            Return _MinuteList
        End Function

        Public Function getStartDateTime() As DateTime
            Return _StartDate
        End Function
        Public Function getFinishDateTime() As DateTime
            Return _FinishDate
        End Function




        Public Property Start() As DateTime
            Get
                Return _StartDate
            End Get
            Set(ByVal d As DateTime)
                _StartDate = d
            End Set
        End Property


        Public Property Finish() As DateTime
            Get
                Return _FinishDate
            End Get
            Set(ByVal d As DateTime)
                _FinishDate = d
            End Set
        End Property

        ''' <summary>
        ''' Checks if this task is allowed to run according to the GPS settings, if no settings are available returns true
        ''' </summary>
        ''' <param name="currentPosition"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getLocationIsOK(currentPosition As CytoSense.Data.GPS.GPSCoordinate) As Boolean
            If Not GPS Then
                Return True
            Else
                For i = 0 To _GPSset.Count - 1
                    If Not _GPSset(i).allowedToRun(currentPosition) Then
                        Return False
                    End If
                Next
                Return True
            End If
        End Function
        Public ReadOnly Property GPS As Boolean
            Get
                If Object.Equals(Nothing, _GPSset) Then
                    Return False
                Else
                    Return _GPSset.Count > 0
                End If
            End Get
        End Property

        Public ReadOnly Property External_Trigger As Boolean
            Get
                Return IsExternalTriggerTask()
            End Get
        End Property

        Public Enum GPSEnum
            Disabled = 0
            In_Range = 1
            Out_Range = 2
        End Enum

        Public Sub setGPSSettings(s As List(Of ScheduleGPSSetting))
            _GPSset = s
        End Sub

        Public Function getGPSSettings() As List(Of ScheduleGPSSetting)
            Return _GPSset
        End Function

        Public MustOverride Sub SetMeasurements(ByVal m As CytoSense.MeasurementSettings.Measurements)
        Public MustOverride Function getMeasurements() As CytoSense.MeasurementSettings.Measurements
        Public MustOverride Property Name() As String
        Public MustOverride ReadOnly Property Number_Of_Measurements() As Integer

		Public Property IsContinuous As Boolean
		Get
			Return _isContinuous
		End Get
		    Set(value As Boolean)
				_isContinuous = value
		    End Set
		End Property

        Public ReadOnly Property Times_scheduled_per_day() As String
            Get
                If Not Object.Equals(Nothing, _HourList) Then
                    Return (_HourList.Length * _MinuteList.Length).ToString()
                Else
                    Return "-"
                End If
            End Get
        End Property

        Public ReadOnly Property Scheduled_days() As String
            Get
                If Not Object.Equals(Nothing, _DayList) Then
                    Dim s As String = ""
                    For i = 0 To _DayList.Length - 1
                        s &= ", "
                        If i = 0 Then
                            s = ""

                        End If
                        Dim d(6) As DayOfWeek
                        d(0) = DayOfWeek.Sunday
                        d(1) = DayOfWeek.Monday
                        d(2) = DayOfWeek.Tuesday
                        d(3) = DayOfWeek.Wednesday
                        d(4) = DayOfWeek.Thursday
                        d(5) = DayOfWeek.Friday
                        d(6) = DayOfWeek.Saturday


                        s &= d(_DayList(i)).ToString.Substring(0, 2)
                    Next

                    Return s
                Else
                    Return ""
                End If
            End Get
        End Property

        ''' <summary>
        ''' Makes CytoUSB shutdown the computer
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Property ShutDown_Afterwards() As Boolean
            Get
                Return _shutdownAfterTask
            End Get
            Set(ByVal value As Boolean)
                _shutdownAfterTask = value
            End Set
        End Property

        Public Property BackFlush() As Boolean
            Get
                Return _BackFlush
            End Get
            Set(ByVal value As Boolean)
                _BackFlush = value
            End Set
        End Property


        Public MustOverride Property Done() As Boolean


        Public Sub setProblemReport(ByVal s As String)
            _ProblemReport = s
        End Sub


        Public Sub New()
            Enabled = True
        End Sub
    End Class

End Namespace