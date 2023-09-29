
Namespace Concentration
    <Serializable()> Public Class HWConcentration
        Public Sub New(ByVal count As Int32, ByVal counter_sec As Int32, ByVal timeDifference As Double)
            Me.count = count
            Me.TimeDifference = timeDifference
            Me.counter_sec = counter_sec
        End Sub
        Public count As Int32
        Public counter_sec As Int32
        Public TimeDifference As Double


        Public Shared Operator +(ByVal h1 As HWConcentration, ByVal h2 As HWConcentration) As HWConcentration
            Dim r As New HWConcentration(h1.count + h2.count, h1.counter_sec + h2.counter_sec, h1.TimeDifference + h2.TimeDifference)
            Return r
        End Operator

        Public Shared Operator /(ByVal h1 As HWConcentration, ByVal d As Double) As HWConcentration
            Dim r As New HWConcentration(CInt(h1.count / d), CInt(h1.counter_sec / d), h1.TimeDifference / d)
            Return r
        End Operator



        Public Function Interpolate(ByVal hw As HWConcentration) As HWConcentration

            If Me.count < hw.count Then
                Return (hw + Me) / 2
            Else
                Dim h As New HWConcentration(hw.count + 65536, hw.counter_sec + 65536, hw.TimeDifference)
                h = (h + Me) / 2
                h.count -= 65536
                h.counter_sec -= 65536
                Return h

            End If


        End Function



    End Class

End Namespace