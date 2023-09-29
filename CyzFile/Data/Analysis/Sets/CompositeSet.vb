Imports System.Drawing

Namespace Data.Analysis

    <Serializable()> Public MustInherit Class CompositeSet
        Inherits CytoSet
        Public Sub New(name As String, type As cytoSetType, myColor As Color)
            MyBase.New(name, type, myColor)
        End Sub
        Public Sub New(name As String, type As cytoSetType, myColor As Color, datafile As CytoSense.Data.DataFileWrapper)
            MyBase.New(name, type, myColor, datafile)
        End Sub

        Public Sub New(name As String, type As cytoSetType, myColor As Color, datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean)
            MyBase.New(name, type, myColor, datafile, listId, vis)
        End Sub

        Public MustOverride ReadOnly Property ChildSets As List(Of CytoSet)
    End Class

End Namespace
