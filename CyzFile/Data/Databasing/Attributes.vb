Namespace Data.DataBase.Attributes

    Public Class FormatAttribute
        Inherits System.Attribute

        Public Property Format As String

        Public Sub New(format As String)
            _Format = format
        End Sub

    End Class

End Namespace