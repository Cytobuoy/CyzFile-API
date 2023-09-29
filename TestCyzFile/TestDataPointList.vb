Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data

''' <summary>
''' I am about to make some changes tot he datapoint list to add some locking, and make
''' a few simple modifications to some functions.  To make sure it remains working I create the
''' tests first.  Note: The test do not check if the locking is correct, it only checks if
''' the numerical results stay the same.
''' </summary>
<TestClass()> Public Class TestDataPointList

    <TestMethod()> Public Sub TDPL_Ctor()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)
        Dim dps = dpl.DataList
        Assert.IsNotNull(dpl)
        Assert.AreEqual(-1, dpl.Length)
        Assert.AreEqual(0,dps.Length)
        Assert.AreEqual(DataPointList.SensorLogTypes.PIC_PMTTemperature, dpl.Type)
    End Sub

    <TestMethod()> Public Sub TDPL_Ctor2()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim interval = New TimeSpan(0,0,31)

        dpl.add(23.5, start)
        dpl.add(25.5, start + interval)

        Dim dpl2 = New DataPointList(DataPointList.SensorLogTypes.PIC_BuoyTemperature, dpl)
        Dim datapoints = dpl2.DataList

        Assert.AreEqual(1, dpl2.Length)
        Assert.AreEqual(25.5, dpl.getLast(), 0.001 )
        Assert.AreEqual(1,datapoints.Length)
        Assert.AreEqual(25.5, datapoints(0).data, 0.001)
        Assert.AreEqual(start + interval, datapoints(0).time)
        Assert.AreEqual(DataPointList.SensorLogTypes.PIC_BuoyTemperature, dpl2.Type)

    End Sub

    <TestMethod()> Public Sub TDPL_AddValues_unchecked()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,13)
        Dim val=3.1415297
        For i = 0 To 3600
            dpl.add(val,currentTime)
            currentTime += interval
        Next

        Dim datapoints = dpl.DataList

        Assert.AreEqual(3601, dpl.Length)
        Assert.AreEqual(3.1415297, dpl.getLast(), 0.00000001 )
        Assert.AreEqual(3601,datapoints.Length)
        Assert.AreEqual(3.1415297, datapoints(0).data, 0.00000001)
        Assert.AreEqual(start, datapoints(0).time)

        Assert.AreEqual(3.1415297, datapoints(1).data, 0.00000001)
        Assert.AreEqual(start + interval, datapoints(1).time )

        Assert.AreEqual(3.1415297, datapoints(111).data, 0.00000001)
        Assert.AreEqual(start + TimeSpan.FromTicks(interval.Ticks * 111) , datapoints(111).time )
    End Sub

    <TestMethod()> Public Sub TDPL_AddValues_checked_1()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        dpl.add( 10.0, 2.0, 5)

        Dim datapoints = dpl.DataList

        Assert.AreEqual(1, dpl.Length)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001 )
        Assert.AreEqual(1, datapoints.Length)
        Assert.AreEqual(10.0, datapoints(0).data, 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_AddValues_checked_3()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)

        Dim datapoints = dpl.DataList

        Assert.AreEqual(3, dpl.Length)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001 )
        Assert.AreEqual(3, datapoints.Length)
        Assert.AreEqual(10.0, datapoints(0).data, 0.001)
        Assert.AreEqual(10.0, datapoints(1).data, 0.001)
        Assert.AreEqual(10.0, datapoints(2).data, 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_AddValues_checked_10()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)

        Dim datapoints = dpl.DataList

        Assert.AreEqual(10, dpl.Length)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001 )
        Assert.AreEqual(10, datapoints.Length)
        Assert.AreEqual(10.0, datapoints(0).data, 0.001)
        Assert.AreEqual(10.0, datapoints(1).data, 0.001)
        Assert.AreEqual(10.0, datapoints(2).data, 0.001)
        Assert.AreEqual(10.0, datapoints(3).data, 0.001)
        Assert.AreEqual(10.0, datapoints(4).data, 0.001)
        Assert.AreEqual(10.0, datapoints(5).data, 0.001)
        Assert.AreEqual(10.0, datapoints(6).data, 0.001)
        Assert.AreEqual(10.0, datapoints(7).data, 0.001)
        Assert.AreEqual(10.0, datapoints(8).data, 0.001)
        Assert.AreEqual(10.0, datapoints(9).data, 0.001)
    End Sub

    ''' <summary>
    ''' Add values and suddenly change more then max.  It should drop several
    ''' sampels before the average is close enough.
    ''' </summary>
    <TestMethod()> Public Sub TDPL_AddValues_checked_22_changing()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)
        dpl.add( 10.0, 2.0, 5)

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, OK
        dpl.add( 15.0, 2.0, 5) ' Avg = 10, DROPPED (Could be 11?)
        dpl.add( 15.0, 2.0, 5) ' Avg = 11, DROPPED
        dpl.add( 15.0, 2.0, 5) ' Avg = 12, DROPPED
        dpl.add( 15.0, 2.0, 5) ' Avg = 13, Kept (maybe dropped)

        dpl.add( 15.0, 2.0, 5) ' Avg = 14, Kept
        dpl.add( 15.0, 2.0, 5) ' Avg = 15, Kept
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept

        dpl.add( 10.0, 2.0, 5) ' Avg = 15, dropped
        dpl.add( 10.0, 2.0, 5) ' Avg = 14, dropped
        dpl.add( 10.0, 2.0, 5) ' avg = 13, dropped
        dpl.add( 10.0, 2.0, 5) ' Avg = 12, kept
        dpl.add( 10.0, 2.0, 5) ' Avg = 11, kept

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept
        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept



        Dim datapoints = dpl.DataList

        Assert.AreEqual(16, dpl.Length)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001 )
        Assert.AreEqual(16, datapoints.Length)
        Assert.AreEqual(10.0, datapoints( 0).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 1).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 2).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 3).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 4).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 5).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 6).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 7).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 8).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 9).data, 0.001)
        Assert.AreEqual(15.0, datapoints(10).data, 0.001)
        Assert.AreEqual(15.0, datapoints(11).data, 0.001)
        Assert.AreEqual(10.0, datapoints(12).data, 0.001)
        Assert.AreEqual(10.0, datapoints(13).data, 0.001)
        Assert.AreEqual(10.0, datapoints(14).data, 0.001)
        Assert.AreEqual(10.0, datapoints(15).data, 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_GetMean_AddValues_checked_22_changing()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, OK
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 10, DROPPED (Could be 11?)
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 11, DROPPED
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 12, DROPPED
        Assert.AreEqual(10.0, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 13, Kept (maybe dropped)
        Assert.AreEqual(10.714, dpl.getMean(), 0.001)

        dpl.add( 15.0, 2.0, 5) ' Avg = 14, Kept
        Assert.AreEqual(11.250, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 15, Kept
        Assert.AreEqual(11.667, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(12.000, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(12.272, dpl.getMean(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(12.500, dpl.getMean(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 15, dropped
        Assert.AreEqual(12.500, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 14, dropped
        Assert.AreEqual(12.500, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' avg = 13, dropped
        Assert.AreEqual(12.500, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 12, kept
        Assert.AreEqual(12.307, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 11, kept
        Assert.AreEqual(12.143, dpl.getMean(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept
        Assert.AreEqual(12.000, dpl.getMean(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept
        Assert.AreEqual(11.875, dpl.getMean(), 0.001)

        Dim datapoints = dpl.DataList

        Assert.AreEqual(16, dpl.Length)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001 )
        Assert.AreEqual(16, datapoints.Length)
        Assert.AreEqual(10.0, datapoints( 0).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 1).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 2).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 3).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 4).data, 0.001)
        Assert.AreEqual(10.0, datapoints( 5).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 6).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 7).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 8).data, 0.001)
        Assert.AreEqual(15.0, datapoints( 9).data, 0.001)
        Assert.AreEqual(15.0, datapoints(10).data, 0.001)
        Assert.AreEqual(15.0, datapoints(11).data, 0.001)
        Assert.AreEqual(10.0, datapoints(12).data, 0.001)
        Assert.AreEqual(10.0, datapoints(13).data, 0.001)
        Assert.AreEqual(10.0, datapoints(14).data, 0.001)
        Assert.AreEqual(10.0, datapoints(15).data, 0.001)
    End Sub


    <TestMethod()> Public Sub TDPL_GetLast_checked_add()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Assert.IsTrue(Double.IsNaN(dpl.getLast()))
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, OK
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 10, DROPPED (Could be 11?)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 11, DROPPED
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 12, DROPPED
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 13, Kept (maybe dropped)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)

        dpl.add( 15.0, 2.0, 5) ' Avg = 14, Kept
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg = 15, Kept
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0, 2.0, 5) ' Avg  =15, Kept
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 15, dropped
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 14, dropped
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' avg = 13, dropped
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 12, kept
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 11, kept
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)

        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0, 2.0, 5) ' Avg = 10, kept
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_GetLast_unchecked_add()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Assert.IsTrue(Double.IsNaN(dpl.getLast()))
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)

        dpl.add( 10.0) 
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 15.0) 
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0) 
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)

        dpl.add( 15.0) 
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)
        dpl.add( 15.0)
        Assert.AreEqual(15.0, dpl.getLast(), 0.001)

        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)

        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
        dpl.add( 10.0)
        Assert.AreEqual(10.0, dpl.getLast(), 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_GetLastTime_A()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 10
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next

        Dim lastTimes = dpl.getLastTime(10)
        Assert.AreEqual(9, lastTimes.Count)
        Assert.AreEqual(1.0, lastTimes(0))
        Assert.AreEqual(2.0, lastTimes(1))
        Assert.AreEqual(3.0, lastTimes(2))
        Assert.AreEqual(4.0, lastTimes(3))
        Assert.AreEqual(5.0, lastTimes(4))
        Assert.AreEqual(6.0, lastTimes(5))
        Assert.AreEqual(7.0, lastTimes(6))
        Assert.AreEqual(8.0, lastTimes(7))
        Assert.AreEqual(9.0, lastTimes(8))
    End Sub

    <TestMethod()> Public Sub TDPL_GetLastTime_B()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 20
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next

        Dim lastTimes = dpl.getLastTime(10)
        Assert.AreEqual(9, lastTimes.Count)
        Assert.AreEqual(11.0, lastTimes(0))
        Assert.AreEqual(12.0, lastTimes(1))
        Assert.AreEqual(13.0, lastTimes(2))
        Assert.AreEqual(14.0, lastTimes(3))
        Assert.AreEqual(15.0, lastTimes(4))
        Assert.AreEqual(16.0, lastTimes(5))
        Assert.AreEqual(17.0, lastTimes(6))
        Assert.AreEqual(18.0, lastTimes(7))
        Assert.AreEqual(19.0, lastTimes(8))
    End Sub

    <TestMethod()> Public Sub TDPL_GetLastTime_C()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 5
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next

        Dim lastTimes = dpl.getLastTime(10)
        Assert.AreEqual(5, lastTimes.Count)
        Assert.AreEqual(0.0, lastTimes(0))
        Assert.AreEqual(1.0, lastTimes(1))
        Assert.AreEqual(2.0, lastTimes(2))
        Assert.AreEqual(3.0, lastTimes(3))
    End Sub

    <TestMethod()> Public Sub TDPL_GetMeanTime_A()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 10
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next

        Assert.AreEqual(5.0, dpl.getMeanTime(10), 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_GetMeanTime_B()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 20
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next
        Assert.AreEqual(15.0, dpl.getMeanTime(10), 0.001)
       
    End Sub

    <TestMethod()> Public Sub TDPL_GetMeanTime_C()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now
        Dim currentTime = start
        Dim interval = New TimeSpan(0,0,0,1,0)
        Dim val=3.1415297
        For i = 0 To 5
            dpl.add(val,currentTime)
            currentTime += TimeSpan.FromTicks(interval.Ticks * i)
        Next

        Assert.AreEqual(2.0, dpl.getMeanTime(10), 0.001)
    End Sub

    <TestMethod()> Public Sub TDPL_DataList_A()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)
        Dim dl = dpl.DataList()
        Assert.IsNotNull(dl)
        Assert.AreEqual(0, dl.Length)
    End Sub

    <TestMethod()> Public Sub TDPL_DataList_B()
        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)
        Dim dl = dpl.DataList()
        Assert.IsNotNull(dl)
        Assert.AreEqual(0, dl.Length)

        For i = 0 To 5
            dpl.add(i)
        Next

        Dim dl2 = dpl.DataList()
        Assert.IsNotNull(dl2)
        Assert.AreEqual(6, dl2.Length)
        For i = 0 To 5
            Assert.AreEqual(CDbl(i), dl2(i).data, String.Format("Idx={0}", i) )
        Next

        For i = 6 To 10
            dpl.add(i)
        Next
        Dim dl3 = dpl.DataList()
        Assert.IsNotNull(dl3)
        Assert.AreEqual(11, dl3.Length)
        For i = 0 To 10
            Assert.AreEqual(CDbl(i), dl3(i).data, String.Format("Idx={0}", i) )
        Next

    End Sub


    <TestMethod()> Public Sub TDPL_GetMean_Interval()

        Dim dpl As New DataPointList(DataPointList.SensorLogTypes.PIC_PMTTemperature)

        Dim start = DateTime.Now

        dpl.add( 10.0, start + New TimeSpan(0,0, 10))
        dpl.add( 10.0, start + New TimeSpan(0,0, 20))
        dpl.add( 10.0, start + New TimeSpan(0,0, 30))
        dpl.add( 10.0, start + New TimeSpan(0,0, 40))
        dpl.add( 11.0, start + New TimeSpan(0,0, 50))
        dpl.add( 11.0, start + New TimeSpan(0,0, 60))
        dpl.add( 11.0, start + New TimeSpan(0,0, 70))
        dpl.add( 11.0, start + New TimeSpan(0,0, 80))
        dpl.add( 12.0, start + New TimeSpan(0,0, 90))
        dpl.add( 12.0, start + New TimeSpan(0,0,100))
        dpl.add( 12.0, start + New TimeSpan(0,0,110))
        dpl.add( 14.0, start + New TimeSpan(0,0,120))
        dpl.add( 14.0, start + New TimeSpan(0,0,130))

        Assert.AreEqual(10.0, dpl.getMean(start, New TimeSpan(0,0,20)), 0.001)
        Assert.AreEqual(10.0, dpl.getMean(start, New TimeSpan(0,0,40)), 0.001)
        Assert.AreEqual(10.2, dpl.getMean(start, New TimeSpan(0,0,50)), 0.001)

        Assert.AreEqual(11.0, dpl.getMean(start + New TimeSpan(0,0,45), New TimeSpan(0,0,30)), 0.001)

        Assert.AreEqual(11.429, dpl.getMean(start + New TimeSpan(0,0,45), New TimeSpan(0,0,70)), 0.001)

        Assert.AreEqual(12.0, dpl.getMean(start + New TimeSpan(0,0,85), New TimeSpan(0,0,30)), 0.001)

        Assert.AreEqual(14.0, dpl.getMean(start + New TimeSpan(0,0,115), New TimeSpan(0,0,20)), 0.001)
        Assert.AreEqual(14.0, dpl.getMean(start + New TimeSpan(0,0,115), New TimeSpan(0,10,20)), 0.001)


    End Sub


End Class