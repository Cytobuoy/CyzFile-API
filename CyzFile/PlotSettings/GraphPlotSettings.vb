
Namespace PlotSettings
    <Serializable()> Public Class GraphPlotSettings


        Dim _AutoScale As Boolean = True
        Dim _RefreshRateOneParticle As Integer = 50
        Dim _RefreshRateBlock As Integer = 100
        Dim _ParticleBuffer As Integer = 1000
        Dim _mode As DrawMode
        Dim _numberOfDrawParticlesInBlockMode As Integer = 100

        Public Property Mode As DrawMode
            Get
                Return _mode
            End Get
            Set(value As DrawMode)
                _mode = value
            End Set
        End Property

        Public Property AutoScale As Boolean
            Get
                Return _AutoScale
            End Get
            Set(value As Boolean)
                _AutoScale = value
            End Set
        End Property

        Public Property RefreshRateOneParticle As Integer
            Get
                Return _RefreshRateOneParticle
            End Get
            Set(value As Integer)
                _RefreshRateOneParticle = value
            End Set
        End Property
        Public Property RefreshRateBlock As Integer
            Get
                Return _RefreshRateBlock
            End Get
            Set(value As Integer)
                _RefreshRateBlock = value
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

        Public Property NumberOfDrawParticlesInBlockMode As Integer
            Get
                Return _numberOfDrawParticlesInBlockMode
            End Get
            Set(value As Integer)
                _numberOfDrawParticlesInBlockMode = value
            End Set
        End Property

        <Serializable()> Enum DrawMode
            OneParticle
            ContinuesParticles
            BlockParticle
            Concentration
        End Enum

    End Class


End Namespace
