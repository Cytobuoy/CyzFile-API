Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data.Analysis

' Very simple gates collection testing, nowhere near complete

<TestClass()> _
Public Class TestGateCollection

    <TestMethod()> _
    Public Sub CTor()
        Dim collection As New GateCollection()
        Assert.IsNotNull(collection)
    End Sub

    <TestMethod()> _
    Public Sub IsIenumerable()
        Dim collection As IEnumerable = CType(New GateCollection(), IEnumerable)
        Assert.IsNotNull(collection)
    End Sub

    <TestMethod()> _
    Public Sub IsIenumerableGate()
        Dim collection As IEnumerable(Of IGate) = CType(New GateCollection(), IEnumerable(Of IGate))
        Assert.IsNotNull(collection)
    End Sub

    <TestMethod()> _
    Public Sub NewIsEmpty()
        Dim collection As New GateCollection()
        Assert.AreEqual(0, collection.Count(), "New Collection not empty." )
        Assert.IsNotNull(collection)
    End Sub


End Class