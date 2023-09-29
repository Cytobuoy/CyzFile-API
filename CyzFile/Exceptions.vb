
Public Class TriggerChannelDetectionFailedException
    Inherits Exception
    Public Sub New()
        MyBase.New("Detection of triggerchannel failed.")
    End Sub 'New
End Class

Public Class CytoSenseDoesNotSupportThisOptionException
    Inherits Exception


    Public Sub New()
        MyBase.New("This CytoSense does not support this feature.")
    End Sub 'New

End Class

Public Class CytoSenseDoesNotHaveThisChannelException
    Inherits Exception


    Public Sub New()
        MyBase.New("This CytoSense does not have this channel")
    End Sub 'New

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

End Class

''' <summary>
''' This exception should not happen, the conflict should be resolved, so this is an internal
''' programming error!
''' </summary>

Public Class ConcentrationMisMatchException
    Inherits Exception


    Public Sub New()
        MyBase.New("Concentration MisMatch error, please contact CytoBuoy!")
    End Sub 'New

End Class




Public Class MeasurementNotFoundException
    Inherits Exception


    Public Sub New()
        MyBase.New("The measurement was not found.")
    End Sub 'New

End Class

Public Class InvalidIIFParametersException
    Inherits Exception

    Public Sub New()
        Me.New("")
    End Sub
    Public Sub New(extraMessage As String)
        MyBase.New("The settings for this IIF target are not valid." & vbCrLf & extraMessage)
    End Sub
End Class