
Namespace Concentration
    <Serializable()> Public Class HighResTimer
        '
        'The Two calls below are the main API calls to get the data from the performanceCounter. You MUST 
        'set the parameter 'ByRef' and NOT ByVal. The value will be received by the variable Not read by the 
        'function.
        '
        'If the Integer - 'Return Values' for either of the QueryPerformance api calls returns a '0', then 
        'the computer does NOT support this high resolution timer. Pertty much EVERY single cpu from the 
        'original Intel Pentium and newer supports this timer. So, you will probably NEVER deal with a 
        'computer that doesn't support this timer.
        '
        'Gives you the current tick counts for the cpu since the computer has been running.
        '

        'resolution of this timer is 0.006ms


        Private Declare Function QueryPerformanceCounter Lib "kernel32" (ByRef _
            counts As Long) As Integer
        '
        'Returns the frequency of the performanceCounter in counts-per-second.
        Private Declare Function QueryPerformanceFrequency Lib "kernel32" _
            (ByRef frequency As Long) As Integer

        Dim counterFreq As Long
        Dim startTime As Long
        Dim endTime As Long
        Dim result As Long


        Public Sub setStartPoint()
            'Get the frequency of the counter.
            QueryPerformanceFrequency(counterFreq)
            'Get the current ticks value to start the baseline for the timer.
            QueryPerformanceCounter(startTime)

        End Sub

        Public Sub setStopPoint()
            'Get the current ticks of the timer after the code has executed.
            QueryPerformanceCounter(endTime)
        End Sub

        Public Function GetDifference() As Double
            'Get the difference between startTime and endTime which equals the time it took to execute the code.
            result = (endTime - startTime)
            '
            'This variable will contains the results calculation related to seconds. Remember.. The 
            'frequency of the counter is supposed to be the 'ticks per second' for the counter.
            Return (result / counterFreq)
        End Function

        Public Function GetDifferenceWithoutStopping() As Double
            'Get the difference between startTime and endTime which equals the time it took to execute the code.
            QueryPerformanceCounter(endTime)
            result = (endTime - startTime)
            '
            'This variable will contains the results calculation related to seconds. Remember.. The 
            'frequency of the counter is supposed to be the 'ticks per second' for the counter.
            Return (result / counterFreq)
        End Function

        Public Function GetDifferenceMiliSecond() As Double
            'Get the difference between startTime and endTime which equals the time it took to execute the code.
            result = (endTime - startTime)
            If result = 0 Then
                Return -1
            End If
            '
            'This variable will contains the results calculation related to seconds. Remember.. The 
            'frequency of the counter is supposed to be the 'ticks per second' for the counter.
            Return Math.Abs((result / counterFreq) * 1000)
        End Function

    End Class



End Namespace