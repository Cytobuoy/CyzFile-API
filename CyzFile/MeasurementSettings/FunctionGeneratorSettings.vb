Namespace MeasurementSettings

    <Serializable()> Public Class FunctionGeneratorSettings

        Public Sub New()
            enableFunctionGenerator = False
            enableIIFTriggerGenerator = False
            levelLow = 10
            levelHighNormal = 100
            levelHighSpecial = 1000
            timeLow = 225
            timeHighNormal = 25
            timeHighSpecial = 250
            iifTriggersPerSecond = 10
        End Sub

        Public Sub New( other As FunctionGeneratorSettings)
            enableFunctionGenerator   = other.enableFunctionGenerator
            enableIIFTriggerGenerator = other.enableIIFTriggerGenerator
            levelLow                  = other.levelLow
            levelHighNormal           = other.levelHighNormal
            levelHighSpecial          = other.levelHighSpecial
            timeLow                   = other.timeLow
            timeHighNormal            = other.timeHighNormal
            timeHighSpecial           = other.timeHighSpecial
            iifTriggersPerSecond      = other.iifTriggersPerSecond
        End Sub


        Public Property enableFunctionGenerator As Boolean
        Public Property enableIIFTriggerGenerator As Boolean

        Public Property levelHighNormal As Integer
        Public Property levelHighSpecial As Integer
        Public Property levelLow As Integer
        Public Property timeHighNormal As Integer
        Public Property timeHighSpecial As Integer
        Public Property timeLow As Integer
        Public Property iifTriggersPerSecond As Integer
    End Class

End Namespace