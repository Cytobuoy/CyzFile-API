
Namespace Scheduling

    <Serializable()> Public Class ExternalTriggerTask
        Inherits MeasurementTask

        Private _done_at As New Microsoft.VisualBasic.Collection

        Private _name As String
        Public Overrides Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal value As String)
                _name = value
            End Set
        End Property

        ''' <summary>
        ''' Not applicable
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function TimeToStart() As System.TimeSpan
            Return New TimeSpan(0)
        End Function

        Public Function RequestStart(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting) As Boolean

            Dim NoLastStartProblem As Boolean = DateDiff(DateInterval.Minute, LastStarted, Now) > MyBase.getMeasurements.MeasurementsTime(cytosettings) 

            Return NoLastStartProblem

        End Function

        Public Function LastStarted() As Date
            If Object.Equals(Nothing, _done_at) Then
                Return New Date(0)
            ElseIf _done_at.Count = 0 Then
                Return New Date(0)
            Else
                Return CType(_done_at(_done_at.Count),Date)
            End If
        End Function


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
            Return False
        End Function

        Public Overrides Function isExternalTriggerTask() As Boolean
            Return True
        End Function


        Public Overrides Function IsSpecificTimeMeasurement() As Boolean
            Return False
        End Function

        Public Overrides Sub ReloadCytoSettings(c As CytoSettings.CytoSenseSetting)
            'nothing to do needed
        End Sub

        ''' <summary>
        ''' Always return false, starting for external events is handled differently.
        ''' </summary>
        ''' <param name="t"></param>
        ''' <param name="scheduler"></param>
        ''' <returns></returns>
        ''' <remarks>The Scheduler has special code for starting external trigger tasks. So
        ''' the ShouldRun that is called from the timer simply always returns false.  And the
        ''' Run used in the timer will throw a NotImplemented exception</remarks>
        Public Overrides Function ShouldRun( t As DateTime, tolerance As TimeSpan, scheduler As ITaskScheduler ) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Not Implemented
        ''' </summary>
        ''' <param name="scheduler"></param>
        ''' <remarks>The run function should never be called for an external trigger task.</remarks>
        Public Overrides Sub Run(scheduler As ITaskScheduler)
            Throw New NotImplementedException()
        End Sub

    End Class


End Namespace
