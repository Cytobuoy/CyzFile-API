
Namespace Calibration

    Public Structure SingleCalibrationPoint
        Public speed As Double
        Public ratio As Double
        Public Sub New(ByVal speed As Double, ByVal ratio As Double)
            Me.speed = speed
            Me.ratio = ratio
        End Sub

    End Structure


End Namespace
