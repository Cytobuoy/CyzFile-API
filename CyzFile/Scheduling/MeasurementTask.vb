Namespace Scheduling

    <Serializable()> Public MustInherit Class MeasurementTask
        Inherits task



        Private _Measurements As CytoSense.MeasurementSettings.Measurements
        Dim _name As String

        Public Overrides Sub ReloadCytoSettings(c As CytoSettings.CytoSenseSetting)
            _Measurements.ReloadCytoSettings(c)
        End Sub

        Public Overrides Function IsShutdownTask() As Boolean
            Return False
        End Function

        Public MustOverride Function TimeToStart() As TimeSpan


        Public Overrides Function getMeasurements() As CytoSense.MeasurementSettings.Measurements
            Return _Measurements
        End Function

        Public Overrides Sub SetMeasurements(ByVal m As CytoSense.MeasurementSettings.Measurements)
            _Measurements = m
        End Sub

        Public Overrides Property Name() As String
            Get
                Return _name
            End Get
            Set(ByVal n As String)
                _name = n
            End Set
        End Property

        Public Overrides ReadOnly Property Number_Of_Measurements() As Integer

            Get
                If Object.Equals(Nothing, _Measurements) Then
                    Return 0
                End If
                Dim tot As Integer = 0
                For i As Integer = 0 To _Measurements.length - 1
                    tot += _Measurements.getMeasurement(i).repeat
                Next
                Return tot
            End Get
        End Property

    End Class


End Namespace