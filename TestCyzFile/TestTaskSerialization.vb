Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports CytoSense.Serializing
Imports CytoSense.Scheduling


''' <summary>
''' Some simple test functions that check serializing the different task classes to a stream and deserializing them again.
''' The main function is to call serialize for each of the classes to make sure that .Net has no problems with serializing
''' them.  When switching to .net 8 some classes are no longer supported and we need to add special handling for them.
''' Calling serialize for each ensures that we notice this.  Some tasks are used very rarely and are easy to miss when testing.
''' 
''' We add a few properties and check these, but we basically on check that it does not crash, it is not a complete test of
''' all properties.
''' </summary>
<TestClass()>
Public Class TestTaskSerialization

    <TestMethod()>
    Public Sub TestSerializeIntervalTask()

        Dim t = New IntervalTask()
        t.Enabled = True
        t.Start   = New DateTime(2024,10,11,12,24,53)
        t.Finish  = New DateTime(2024,10,12,12,24,53)
        t.Started(New Date(2024,10,11,12,30,00))
        Dim bytes  = serializeToStream(t)

    End Sub

    <TestMethod()>
    Public Sub TestSerializeExternalTriggerTask()

        Dim t = New ExternalTriggerTask()
        t.Enabled = True
        t.Start   = New DateTime(2024,10,11,12,24,53)
        t.Finish  = New DateTime(2024,10,12,12,24,53)
        Dim bytes  = serializeToStream(t)

    End Sub

    <TestMethod()>
    Public Sub TestSerializeSheathCleaningTask()

        Dim t = New SheathCleaningTask( SheathCleaningTask.ScheduleType.SingleShot, false)
        t.Enabled = True
        t.Start   = New DateTime(2024,10,11,12,24,53)
        t.Finish  = New DateTime(2024,10,12,12,24,53)
        t.Started(New Date(2024,10,11,12,30,00))
        Dim bytes  = serializeToStream(t)

    End Sub

    <TestMethod()>
    Public Sub TestSerializeShutDownTask()

        Dim t = New ShutDownTask()
        t.Enabled = True
        t.Start   = New DateTime(2024,10,11,12,24,53)
        t.Finish  = New DateTime(2024,10,12,12,24,53)
        t.Started(New Date(2024,10,11,12,30,00))
        Dim bytes  = serializeToStream(t)

    End Sub

    <TestMethod()>
    Public Sub TestSerializeSingleTask()

        Dim t = New SingleTask()
        t.Enabled = True
        t.Start   = New DateTime(2024,10,11,12,24,53)
        t.Finish  = New DateTime(2024,10,12,12,24,53)
        Dim bytes  = serializeToStream(t)

    End Sub

    <TestMethod()>
    Public Sub TestDeSerializeIntervalTask()

        Dim exp = New IntervalTask()
        exp.Enabled = True
        exp.Start   = New Date(2024,10,11,12,24,53)
        exp.Finish  = New Date(2024,10,12,12,24,53)
        exp.Started(New Date(2024,10,11,12,30,00))

        Dim bytes  = serializeToStream(exp)
        Dim act = CTYpe(deserializeStream(bytes), IntervalTask)

        Assert.AreEqual(exp.Enabled, act.Enabled)
        Assert.AreEqual(exp.Start, act.Start)
        Assert.AreEqual(exp.Finish, act.Finish)
        Assert.AreEqual(New Date(2024,10,11,12,30,00), act.LastStarted())
    End Sub

    <TestMethod()>
    Public Sub TestDeSerializeExternalTriggerTask()

        Dim exp = New ExternalTriggerTask()
        exp.Enabled = True
        exp.Start   = New Date(2024,10,11,12,24,53)
        exp.Finish  = New Date(2024,10,12,12,24,53)

        Dim bytes  = serializeToStream(exp)
        Dim act = CTYpe(deserializeStream(bytes), ExternalTriggerTask)

        Assert.AreEqual(exp.Enabled, act.Enabled)
        Assert.AreEqual(exp.Start, act.Start)
        Assert.AreEqual(exp.Finish, act.Finish)
        Assert.AreEqual(New Date(1,1,1,0,0,0), act.LastStarted())

    End Sub

    <TestMethod()>
    Public Sub TestDeSerializeSheathCleaningTask()

        Dim exp = New SheathCleaningTask( SheathCleaningTask.ScheduleType.SingleShot, false)
        exp.Enabled = True
        exp.Start   = New Date(2024,10,11,12,24,53)
        exp.Finish  = New Date(2024,10,12,12,24,53)
        exp.Started(New Date(2024,10,11,12,30,00))

        Dim bytes  = serializeToStream(exp)
        Dim act = CTYpe(deserializeStream(bytes), SheathCleaningTask)

        Assert.AreEqual(exp.Enabled, act.Enabled)
        Assert.AreEqual(exp.Start, act.Start)
        Assert.AreEqual(exp.Finish, act.Finish)
        Assert.AreEqual(New Date(2024,10,11,12,30,00), act.LastStarted())

    End Sub

    <TestMethod()>
    Public Sub TestDeSerializeShutDownTask()

        Dim exp = New ShutDownTask()
        exp.Enabled = True
        exp.Start   = New Date(2024,10,11,12,24,53)
        exp.Finish  = New Date(2024,10,12,12,24,53)
        exp.Started(New Date(2024,10,11,12,30,00))

        Dim bytes  = serializeToStream(exp)
        Dim act = CTYpe(deserializeStream(bytes), ShutDownTask)

        Assert.AreEqual(exp.Enabled, act.Enabled)
        Assert.AreEqual(exp.Start, act.Start)
        Assert.AreEqual(exp.Finish, act.Finish)
        Assert.AreEqual(New Date(2024,10,11,12,30,00), act.LastStarted())

    End Sub

    <TestMethod()>
    Public Sub TestDeSerializeSingleTask()

        Dim exp = New SingleTask()
        exp.Enabled = True
        exp.Start   = New Date(2024,10,11,12,24,53)
        exp.Finish  = New Date(2024,10,12,12,24,53)

        Dim bytes  = serializeToStream(exp)
        Dim act = CTYpe(deserializeStream(bytes), SingleTask)

        Assert.AreEqual(exp.Enabled, act.Enabled)
        Assert.AreEqual(exp.Start, act.Start)
        Assert.AreEqual(exp.Finish, act.Finish)

    End Sub


End Class