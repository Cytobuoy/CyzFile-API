
Namespace PlotSettings

    <Serializable()> Public Class ScatterPlotSettings


        Dim _AutoScale As Boolean = True
        Dim _RefreshRate As Integer = 300
        Dim _ParticleBuffer As Integer = 1000
        Dim _dotSize As Integer = 3

        Public Property AutoScale As Boolean
            Get
                Return _AutoScale
            End Get
            Set(value As Boolean)
                _AutoScale = value
            End Set
        End Property

        Public Property RefreshRate As Integer
            Get
                Return _RefreshRate
            End Get
            Set(value As Integer)
                _RefreshRate = value
            End Set
        End Property

        Public Property ParticleBuffer As Integer
            Get
                Return _ParticleBuffer
            End Get
            Set(value As Integer)
                _ParticleBuffer = value
            End Set
        End Property

        Public Property DotSize As Integer
            Get
                Return _dotSize
            End Get
            Set(value As Integer)
                _dotSize = value
            End Set
        End Property

    End Class

End Namespace
