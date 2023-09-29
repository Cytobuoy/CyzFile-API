Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data.GPS
Imports System.IO
Imports System.Drawing

''' <summary>
''' Test strings are data provided by lotty, as set in an email.
''' Very basic, we only validate the lattitude and longtitude.
''' Some of the others are not implemented correctly, e.g. the altitude is
''' incorrect, but for now I do not care :-(
''' </summary>
<TestClass()> Public Class GpsTest


    <TestMethod()> _
    Public Sub TestCtor()
        Dim gps As New GPSCoordinate("GPGGA,094107,5419.7005,N,01010.7055,E,1,10,0.9,6.1,M,42.9,M,,*45")
    End Sub


    <TestMethod()> _
    Public Sub TestString1()

        Dim isGGA As Boolean = True
        Dim Latitude As Double = 54.328342
        Dim Longtitude As Double = 10.178425
        Dim Altitude As Double = 6.1

        Dim gps As New GPSCoordinate("GPGGA,094107,5419.7005,N,01010.7055,E,1,10,0.9,6.1,M,42.9,M,,*45")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub

    <TestMethod()> _
    Public Sub TestString2()

        Dim isGGA As Boolean = True
        Dim Latitude As Double = 54.328342
        Dim Longtitude As Double = 10.178425
        Dim Altitude As Double = 6.1

        Dim gps As New GPSCoordinate("GPGGA,094112,5419.7005,N,01010.7055,E,1,10,1.0,6.1,M,42.9,M,,*49")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub
    <TestMethod()> _
    Public Sub TestString3()

        Dim isGGA As Boolean = True
        Dim Latitude As Double = 54.328343
        Dim Longtitude As Double = 10.178425
        Dim Altitude As Double = 6.1

        Dim gps As New GPSCoordinate("GPGGA,094117,5419.7006,N,01010.7055,E,1,10,1.0,6.2,M,42.9,M,,*4C")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub

    <TestMethod()> _
    Public Sub TestString4()

        Dim isGGA As Boolean = True
        Dim Latitude As Double = 43.300395
        Dim Longtitude As Double = 5.36128
        Dim Altitude As Double = 6.1

        Dim gps As New GPSCoordinate("GPGGA,153940,4318.0237,N,00521.6768,E,2,10,1.0,33.1,M,48.5,M,,*72")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub



    <TestMethod()> _
    Public Sub TestString_Gerald_1()

        Dim isGGA As Boolean = True
        Dim Latitude As Double   = 40.9935668
        Dim Longtitude As Double =  4.5847173

        Dim gps As New GPSCoordinate("INGGA,000148.40,4059.61401,N,00435.08304,E,4,18,00.0,046.406,M,0.0,M,0.0,0000*50")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub



    <TestMethod()> _
    Public Sub TestString_Gerald_2()

        Dim isGGA As Boolean = True
        Dim Latitude As Double   = 41.001408
        Dim Longtitude As Double = 4.581156833

        Dim gps As New GPSCoordinate("INGGA,000634.40,4100.08448,N,00434.86941,E,4,18,00.0,046.364,M,0.0,M,0.0,0000*5C")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub



    <TestMethod()> _
    Public Sub TestString_Gerald_3()

        Dim isGGA As Boolean = True
        Dim Latitude As Double   = 41.00240333
        Dim Longtitude As Double =  4.58068866
        Dim Altitude As Double = 6.1

        Dim gps As New GPSCoordinate("INGGA,000710.40,4100.14420,N,00434.84132,E,4,18,00.0,046.387,M,0.0,M,0.0,0000*5B")

        Assert.AreEqual( isGGA, gps.isGGA)
        
        Assert.AreEqual( Latitude,          gps.Latitude,  0.000001)
        Assert.AreEqual( Longtitude,        gps.Longitude, 0.000001)
    End Sub



    ' Open the log file, read through it and create GPS strings of all the
    ' strings.  We just check it works, we do not do any tests yet, just make sure we
    ' do not crash on the strings.
    <TestMethod()> _
    Public Sub TestExampleGpsLog()


        Using f = File.OpenText("Misc/GPS_20121104_134730.log")
            While Not f.EndOfStream()
                Dim line = f.ReadLine().Substring(1) ' Single line, with a $ at the start.  The GPS coordinate class does not want
                                                     ' the $, so we need to remove that.
                Dim gps As New GPSCoordinate(line)
                If gps.isGGA Then
                    Dim dummy = gps.Latitude
                Else
                     ' Do nothing
                End If
            End While
        End Using


    End Sub


    ' Open the log file, read through it and create GPS strings of all the
    ' strings.  We just check it works, we do not do any tests yet, just make sure we
    ' do not crash on the strings.
    ' This is a GPS log from Gerald which uses a different talker ID.
    ' There should be approx 300 GPS strings in there (a total of 1900 lines).
    ' And they should be in a certain range, not a very exact test, but should find big issues.
    <TestMethod()> _
    Public Sub TestExampleGpsLog2()

        Dim lineCtr As Integer = 0
        Dim ggaCtr As Integer = 0
        Using f = File.OpenText("Misc/GPS_GERALD_2023-04-28 23h59.log")
            While Not f.EndOfStream()
                Dim line = f.ReadLine().Substring(1) ' Single line, with a $ at the start.  The GPS coordinate class does not want
                                                     ' the $, so we need to remove that.
                lineCtr += 1
                Dim gps As New GPSCoordinate(line)
                If gps.isGGA Then
                    ggaCtr += 1
                    Assert.IsTrue(  40.99 <= gps.Latitude  AndAlso gps.Latitude  <= 41.01)
                    Assert.IsTrue(   4.58 <= gps.Longitude AndAlso gps.Longitude <=  4.59)
                Else
                     ' Do nothing
                End If
            End While
        End Using

        Assert.AreEqual( 323,  ggaCtr  )
        Assert.AreEqual( 1938, lineCtr )
    End Sub



End Class
