Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports CytoSense.Data.Analysis
Imports System.Drawing

<TestClass()> Public Class GateClasses



    <TestMethod()> _
    Public Sub TestCaculateBoundingBox1()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( 0.0, 0.0) )

        Dim exp = New RectangleF(0.0, 0.0, 0.0, 0.0)

        Dim act = PolygonGate.CalculateBoundingBox(polPoints)

        Assert.AreEqual( exp, act)
    End Sub


    <TestMethod()> _
    Public Sub TestCaculateBoundingBox2()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( 0.0, 0.0) )
        polPoints.Add( New PointF( -12.0, 3.0) )

        Dim exp = New RectangleF(-12.0, 0.0, 12.0, 3.0)

        Dim act = PolygonGate.CalculateBoundingBox(polPoints)

        Assert.AreEqual( exp, act)
    End Sub


    <TestMethod()> _
    Public Sub TestCaculateBoundingBox3()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF(   0.0,  0.0) )
        polPoints.Add( New PointF( -12.0,  0.0) )
        polPoints.Add( New PointF(   3.0, 12.0) )

        Dim exp = New RectangleF(-12.0, 0.0, 15.0, 12.0)

        Dim act = PolygonGate.CalculateBoundingBox(polPoints)

        Assert.AreEqual( exp, act)
    End Sub


    <TestMethod()> _
    Public Sub TestCaculateBoundingBox4()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( 0.0, 0.0) )
        polPoints.Add( New PointF( 0.0, 1.0) )
        polPoints.Add( New PointF( 1.0, 1.0) )
        polPoints.Add( New PointF( 2.0, 3.0) )
        polPoints.Add( New PointF(-3.0, 0.0) )
        polPoints.Add( New PointF( 0.0, 12.0) )
        polPoints.Add( New PointF( -2.9, 0.0) )

        Dim exp = New RectangleF(-3.0, 0.0, 5.0, 12)
        Dim act = PolygonGate.CalculateBoundingBox(polPoints)

        Assert.AreEqual( exp, act)
    End Sub


    <TestMethod()> _
    Public Sub TestCaculateBoundingBox5()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( -0.6, -0.6) )   ' 0
        polPoints.Add( New PointF( -0.2,  0.8) )   ' 1
        polPoints.Add( New PointF(  0.6,  0.2) )   ' 2
        polPoints.Add( New PointF(  1.4,  0.4) )   ' 3
        polPoints.Add( New PointF(  1.8,  1.0) )   ' 4
        polPoints.Add( New PointF(  2.6,  1.0) )   ' 5
        polPoints.Add( New PointF(  3.2,  1.4) )   ' 6
        polPoints.Add( New PointF(  3.0,  2.0) )   ' 7
        polPoints.Add( New PointF(  2.6,  2.8) )   ' 8
        polPoints.Add( New PointF(  1.4,  2.2) )   ' 9
        polPoints.Add( New PointF(  0.8,  2.2) )   ' 10
        polPoints.Add( New PointF(  1.8,  3.2) )   ' 11
        polPoints.Add( New PointF(  3.0,  3.8) )   ' 12
        polPoints.Add( New PointF(  3.6,  3.4) )   ' 13
        polPoints.Add( New PointF(  3.6,  2.2) )   ' 14
        polPoints.Add( New PointF(  4.0,  2.2) )   ' 15
        polPoints.Add( New PointF(  3.0,  3.2) )   ' 16
        polPoints.Add( New PointF(  3.6,  3.6) )   ' 17
        polPoints.Add( New PointF(  2.8,  4.0) )   ' 18
        polPoints.Add( New PointF(  3.6,  4.2) )   ' 19
        polPoints.Add( New PointF(  4.0,  4.0) )   ' 20
        polPoints.Add( New PointF(  4.4,  3.4) )   ' 21
        polPoints.Add( New PointF(  4.4,  1.8) )   ' 22
        polPoints.Add( New PointF(  3.6,  0.6) )   ' 23
        polPoints.Add( New PointF(  0.2, -1.6) )   ' 24
        polPoints.Add( New PointF(  0.0, -0.2) )   ' 25
        Dim exp = New RectangleF(-0.6, -1.6, 5.0, 5.8)
        Dim act = PolygonGate.CalculateBoundingBox(polPoints)

        Assert.AreEqual( exp.X,      act.X,      0.000001)
        Assert.AreEqual( exp.Y,      act.Y,      0.000001)
        Assert.AreEqual( exp.Width,  act.Width,  0.000001)
        Assert.AreEqual( exp.Height, act.Height, 0.000001)
    End Sub



    <TestMethod()> _
    Public Sub PointInPolygon1()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( 0.0, 0.0) )
        polPoints.Add( New PointF( 0.0, 1.0) )
        polPoints.Add( New PointF( 1.0, 1.0) )
        polPoints.Add( New PointF( 1.0, 0.0) )


        Dim p1 As New PointF(0.5, -1.5)
        Dim p2 As New PointF(0.5, -0.5)
        Dim p3 As New PointF(0.5, -0.01)
        Dim p4 As New PointF(0.5,  0.01)
        Dim p5 As New PointF(0.5,  0.5)
        Dim p6 As New PointF(0.5,  0.99)
        Dim p7 As New PointF(0.5,  1.01)
        Dim p8 As New PointF(0.5,  213.0)


        Assert.AreEqual( False, PolygonGate.PointInPolygon( p1, polPoints) )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( p2, polPoints) )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( p3, polPoints) )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( p4, polPoints) )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( p5, polPoints) )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( p6, polPoints) )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( p7, polPoints) )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( p8, polPoints) )

    End Sub


    <TestMethod()> _
    Public Sub PointInPolygon2()
        
        Dim polPoints As New List(Of PointF)()
        polPoints.Add( New PointF( -0.6, -0.6) )   ' 0
        polPoints.Add( New PointF( -0.2,  0.8) )   ' 1
        polPoints.Add( New PointF(  0.6,  0.2) )   ' 2
        polPoints.Add( New PointF(  1.4,  0.4) )   ' 3
        polPoints.Add( New PointF(  1.8,  1.0) )   ' 4
        polPoints.Add( New PointF(  2.6,  1.0) )   ' 5
        polPoints.Add( New PointF(  3.2,  1.4) )   ' 6
        polPoints.Add( New PointF(  3.0,  2.0) )   ' 7
        polPoints.Add( New PointF(  2.6,  2.8) )   ' 8
        polPoints.Add( New PointF(  1.4,  2.2) )   ' 9
        polPoints.Add( New PointF(  0.8,  2.2) )   ' 10
        polPoints.Add( New PointF(  1.8,  3.2) )   ' 11
        polPoints.Add( New PointF(  3.0,  3.8) )   ' 12
        polPoints.Add( New PointF(  3.6,  3.4) )   ' 13
        polPoints.Add( New PointF(  3.6,  2.2) )   ' 14
        polPoints.Add( New PointF(  4.0,  2.2) )   ' 15
        polPoints.Add( New PointF(  3.0,  3.2) )   ' 16
        polPoints.Add( New PointF(  3.6,  3.6) )   ' 17
        polPoints.Add( New PointF(  2.8,  4.0) )   ' 18
        polPoints.Add( New PointF(  3.6,  4.2) )   ' 19
        polPoints.Add( New PointF(  4.0,  4.0) )   ' 20
        polPoints.Add( New PointF(  4.4,  3.4) )   ' 21
        polPoints.Add( New PointF(  4.4,  1.8) )   ' 22
        polPoints.Add( New PointF(  3.6,  0.6) )   ' 23
        polPoints.Add( New PointF(  0.2, -1.6) )   ' 24
        polPoints.Add( New PointF(  0.0, -0.2) )   ' 25



        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(-0.8, -0.8), polPoints), "-0.8, -0.8" )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF( 1.2,  0.4), polPoints), "1.2,  0.4" )
        Assert.AreEqual( True,  PolygonGate.PointInPolygon( New PointF( 1.8,  0.8), polPoints), "1.8,  0.8"  )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF( 2.5,  1.2), polPoints), "2.5,  1.2"  )
        Assert.AreEqual( True,  PolygonGate.PointInPolygon( New PointF(3.8, 2.0), polPoints), "3.8, 2.0"  )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(3.9, 0.1), polPoints), "3.9, 0.1"  )
        Assert.AreEqual( True,  PolygonGate.PointInPolygon( New PointF(4.00001, 2.1), polPoints), "4.00001, 2.1"  )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( New PointF(4.399999, 2.2), polPoints), "4.399999, 2.2"  )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(4.400001, 2.2), polPoints), "4.400001, 2.2"  )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(4.5, 2.3), polPoints), "4.5, 2.3"  )

        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(2.6, 2.6), polPoints), "2.6, 2.6"  )
        Assert.AreEqual( False, PolygonGate.PointInPolygon( New PointF(2.4, 2.6), polPoints), "2.4, 2.6"  )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( New PointF(2.4, 2.8), polPoints), "2.4, 2.8"  )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( New PointF(2.8, 2.6), polPoints), "2.8, 2.6"  )
        Assert.AreEqual( True, PolygonGate.PointInPolygon( New PointF(2.0, 2.6), polPoints), "2.0, 2.6"  )
    End Sub



End Class