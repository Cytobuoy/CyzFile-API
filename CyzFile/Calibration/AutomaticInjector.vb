Option Strict

Namespace Calibration
    <Serializable()> Public Class AutomaticInjector
        Private _injectorFocus As Integer
        Private _injectorPos As Integer
        Private _sheathSpeed As Double

        Private _maxAverageSharpness As Double
        Private _averageXPos As Double
        Private _averageYPos As Double
        'Private _averageCorePos As Double

        Private _date As DateTime

        Public ReadOnly Property InjectorFocus As Integer
            Get
                Return _injectorFocus
            End Get
        End Property

        Public ReadOnly Property InjectorPos As Integer
            Get
                Return _injectorPos
            End Get
        End Property

        Public ReadOnly Property SheathSpeed As Double
            Get
                Return _sheathSpeed
            End Get
        End Property

        Public ReadOnly Property MaxAverageSharpness As Double
            Get
                Return _maxAverageSharpness
            End Get
        End Property

        Public ReadOnly Property AverageXPos As Double
            Get
                Return _averageXPos
            End Get
        End Property

        Public ReadOnly Property AverageYPos As Double
            Get
                Return _averageYPos
            End Get
        End Property

        'Public ReadOnly Property AverageCorePos As Double
        '    Get
        '        Return _averageCorePos
        '    End Get
        'End Property

        Public ReadOnly Property DatePerformed As DateTime
            Get
                Return _date
            End Get
        End Property

        Public Sub New(injectorFocus As Integer, injectorPos As Integer, sheathSpeed As Double, maxAverageSharpness As Double, averageX As Double, averageY As Double, datePerformed As DateTime)
            _injectorFocus = injectorFocus
            _injectorPos = injectorPos
            _sheathSpeed = sheathSpeed

            _maxAverageSharpness = maxAverageSharpness
            _averageXPos = averageX
            _averageYPos = averageY
            '_averageCorePos = averageCorePos

            _date = datePerformed
        End Sub
    End Class
End Namespace


