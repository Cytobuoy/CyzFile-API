Imports System.Runtime.Serialization
Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis
    ''' <summary>
    ''' Base class for both the polygon gate and the free form gate.  Basically these two are the same but we make 
    ''' some distinction with respect to how they are drawn and can be modified.  So all the real work is in this 
    ''' base class, and we only have some constructors in the subclasses.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()>
    Public MustInherit Class PointPathGate
        Inherits Gate2D

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal xAx As Axis, ByVal yAx As Axis, ByVal path As List(Of Drawing.PointF))
            MyBase.New(xAx, yAx)
            _pathPoints = path
            _boundingBox = CalculateBoundingBox(Transform(_pathPoints, XAxis.isLog, YAxis.isLog))
        End Sub

        Public Sub New(ByVal other As PointPathGate)
            MyBase.New(other)
            For Each p In other._pathPoints
                _pathPoints.Add(p)
            Next
            _boundingBox = CalculateBoundingBox(Transform(_pathPoints, XAxis.isLog, YAxis.isLog))
        End Sub

        Public Sub SetNewPointList(path As List(Of Drawing.PointF))
            _pathPoints = path
            _Invalid = True
            _boundingBox = CalculateBoundingBox(Transform(_pathPoints, XAxis.isLog, YAxis.isLog))
        End Sub

        ''' <summary>
        ''' This is the tricky one, here we need to recalculate for every point in the path if it is inside the polygon
        ''' that was specified in _pathPoints.
        ''' The original version used a ZedGraph object for this and bitmaps, etc.  We want to remove the ZedGraph dependency
        ''' from this object.  However, we do need to perform this test on a lot of points, so it needs to be fast.
        ''' A simple point in polygon may not be fast enough.  We probably need to create some kind
        ''' of bitmap that we color and do a lookup.  For now I will start with the simplest 
        ''' solution
        ''' 
        ''' Option 1) Create a bounding box, for a simple first test to eliminate a lot of points, then do a real
        '''    algorithm for inside the box. (Calculate number of line intersections)
        ''' 
        ''' Option 1 is fast enough, there is a minor problem for axis that have a log scale.  A straight line
        ''' drawn in a logarithmic scale is not a straight line in a the normal scale.
        ''' A possible solution is to transform all the coordinates and the path points into
        ''' a logarithmic "space" before making the check.
        ''' 
        ''' We take the 10 log of the all values for axis that are displayed in log scale. 
        ''' </summary>
        Public Overrides Sub RecalculateParticleIndices()
            If Not Invalid OrElse _datafile Is Nothing Then
                Return
            End If

            If xValues Is Nothing OrElse yValues Is Nothing Then
                LoadAxisValues()
            End If

            Debug.WriteLine("    PointPathGate.RecalculateParticleIndices for XAxis: {0}", XAxis)
            Debug.WriteLine("    PointPathGate.RecalculateParticleIndices for YAxis: {0}", YAxis)

            Dim particles = _datafile.SplittedParticles
            Dim indices = New List(Of Integer)

            Dim xIsLog As Boolean = XAxis.isLog
            Dim yIsLog As Boolean = YAxis.isLog

            Dim points = Transform(_pathPoints, xIsLog, yIsLog)

            For i As Integer = 0 To particles.Length - 1
                Dim x As Single = xValues(i)

                If xIsLog Then
                    x = CSng(Math.Log10(x))
                End If

                ' prevent calculation of Y-value if X-value out-of-range
                If x < _boundingBox.X OrElse x > _boundingBox.X + _boundingBox.Width Then
                    Continue For
                End If

                Dim y As Single = yValues(i)

                If yIsLog Then
                    y = CSng(Math.Log10(y))
                End If

                Dim p = New Drawing.PointF(x, y)

                If _boundingBox.Contains(p) AndAlso PointInPolygon(p, points) Then
                    indices.Add(i)
                End If
            Next

            _ParticleIndices = indices.ToArray()
            _Invalid = False
        End Sub

        ''' <summary>
        ''' Tests a single particle against the gate
        ''' </summary>
        ''' <param name="particle"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function TestSingleParticle(particle As CytoSense.Data.ParticleHandling.Particle) As Boolean
            Dim x As Single
            Dim y As Single

            x = XAxis.GetValue(particle)
            y = YAxis.GetValue(particle)

            Dim points = Transform(_pathPoints, XAxis.isLog, YAxis.isLog)

            If XAxis.isLog Then
                x = CSng(Math.Log10(x))
            End If

            If YAxis.isLog Then
                y = CSng(Math.Log10(y))
            End If

            Dim p = New Drawing.PointF(x, y)

            If _boundingBox.Contains(p) AndAlso PointInPolygon(p, points) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Check if the X,y coordinates are in side the polygon, or not.
        ''' Apparently we need to do a log conversions or the lines in the log scale do not match the
        ''' on screen stuff.  This is only required for the polygon gate, and not for the others.
        '''
        ''' </summary>
        ''' <param name="p">Point to test.In parameter values, not screen coordinates.</param>
        ''' <returns></returns>
        Public Function ContainsPoint(p As Drawing.PointF) As Boolean
            Dim points = Transform(_pathPoints, XAxis.isLog, YAxis.isLog)
            Dim x As Single = p.X
            Dim y As Single = p.Y
            If XAxis.isLog Then
                x = CSng(Math.Log10(x))
            End If
            If YAxis.isLog Then
                y = CSng(Math.Log10(y))
            End If

            Dim np = New Drawing.PointF(x, y)
            Return _boundingBox.Contains(np) AndAlso PointInPolygon(np, points)
        End Function

        ''' <summary>
        ''' If one or more of the axis is on a log scale we need to transform that axis its coordinates, to
        ''' make the correct particle selection.
        ''' </summary>
        ''' <param name="orgPoints">The original unprocessed points.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function Transform(orgPoints As List(Of Drawing.PointF), xIsLog As Boolean, yIsLog As Boolean) As List(Of Drawing.PointF)
            If Not xIsLog AndAlso Not yIsLog Then
                Return orgPoints
            End If

            Return orgPoints.Select(Function(p)
                                        Return Transform(p, xIsLog, yIslog)
                                    End Function).ToList()
        End Function

        Private Shared Function Transform(p As Drawing.PointF, xIsLog As Boolean, yIslog As Boolean) As Drawing.PointF
            If Not xIsLog AndAlso Not yIslog Then
                Return p
            End If
            Dim r As New Drawing.PointF(p.X, p.Y)
            If xIsLog Then
                r.X = CSng(Math.Log10(r.X))
            End If
            If yIslog Then
                r.Y = CSng(Math.Log10(r.Y))
            End If
            Return r
        End Function

        ''' <summary>
        ''' Initial implementation, based on line crossings.  Probably not fast enough.
        ''' Will need to do something more clever.  But just want to get it working now.
        ''' Will be really inefficient for e.g. FreeForm with lots of line segments.
        ''' 
        ''' Basically we consider the polygon to consist of numPoints sections, section 0 is from point
        ''' 0 to point 1, and section 1 is form point 1 to point 2, and section n is from point n to
        ''' point 0.  So the last section is a special case.
        ''' 
        ''' Points that are exactly on the line can go both ways. It depends on the orientation of
        ''' the segment.
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="polygon"></param>
        ''' <returns></returns>
        ''' <remarks>Public, to facilitate easy testing. Based on 
        ''' http://alienryderflex.com/polygon/ 
        ''' http://geomalgorithms.com/a03-_inclusion.html
        ''' http://www.codeproject.com/Tips/84226/Is-a-Point-inside-a-Polygon
        ''' 
        ''' </remarks>
        Public Shared Function PointInPolygon(ByVal p As Drawing.PointF, ByVal polygon As List(Of Drawing.PointF)) As Boolean
            Dim inside As Boolean = False
            Dim numPoints As Integer = polygon.Count

            For pntIdx As Integer = 0 To numPoints - 2
                Dim segP1 = polygon(pntIdx)
                Dim segP2 = polygon(pntIdx + 1)

                If (segP1.Y > p.Y) <> (segP2.Y > p.Y) Then
                    If (p.X < (segP2.X - segP1.X) * (p.Y - segP1.Y) / (segP2.Y - segP1.Y) + segP1.X) Then
                        inside = Not inside
                    End If
                End If ' Else both segment points have Y on the same side of p, no change of intersection.
            Next
            'Last section
            Dim lastP1 = polygon(numPoints - 1)
            Dim lastP2 = polygon(0)

            If (lastP1.Y > p.Y) <> (lastP2.Y > p.Y) Then
                If (p.X < (lastP2.X - lastP1.X) * (p.Y - lastP1.Y) / (lastP2.Y - lastP1.Y) + lastP1.X) Then
                    inside = Not inside
                End If
            End If ' Else both segment points have Y on the same side of p, no change of intersection.

            Return inside
        End Function

        ''' <summary>
        ''' Calculate the bounding box for a polygon, to speed up checking if a polygon is 
        ''' inside.
        ''' </summary>
        ''' <param name="points"></param>
        ''' <returns></returns>
        ''' <remarks>Public for testing purposes only.</remarks>
        Public Shared Function CalculateBoundingBox(ByVal points As List(Of Drawing.PointF)) As Drawing.RectangleF
            Dim minX As Single = Single.PositiveInfinity
            Dim minY As Single = Single.PositiveInfinity
            Dim maxX As Single = Single.NegativeInfinity
            Dim maxY As Single = Single.NegativeInfinity

            For Each p In points
                If p.X > maxX Then
                    maxX = p.X
                End If
                If p.Y > maxY Then
                    maxY = p.Y
                End If
                If p.X < minX Then
                    minX = p.X
                End If
                If p.Y < minY Then
                    minY = p.Y
                End If
            Next
            Return New Drawing.RectangleF(minX, minY, maxX - minX, maxY - minY)
        End Function

        ''' <summary>
        ''' Calculate all the non serialized data. At the moment only the bounding box
        ''' </summary>
        ''' <param name="context"></param>
        ''' <remarks></remarks>
        <OnDeserialized()>
        Private Sub SetValuesOnDeserialized(ByVal context As StreamingContext)
            _boundingBox = CalculateBoundingBox(Transform(_pathPoints, XAxis.isLog, YAxis.isLog))
        End Sub

        Public Overrides ReadOnly Property Outline As System.Collections.Generic.List(Of System.Drawing.PointF)
            Get
                Return _pathPoints
            End Get
        End Property

        ''' <summary>
        ''' Returns the outline in axis coordinates, it used the axis1 as xAxis, and axis2 as yAxis, regards of
        ''' the way the gate was originally defined!
        ''' If one of the requested axis is not present then an argument exception is thrown!
        ''' </summary>
        ''' <param name="axis1"></param>
        ''' <param name="axis2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function GetOutline(axis1 As Axis, axis2 As Axis) As List(Of Drawing.PointF)
            Dim tmpList As New List(Of Drawing.PointF)
            If axis1 = XAxis AndAlso axis2 = YAxis Then
                For i As Integer = 0 To _pathPoints.Count - 1
                    tmpList.Add(_pathPoints(i))
                Next
            ElseIf axis1 = YAxis And axis2 = XAxis Then
                For i As Integer = 0 To _pathPoints.Count - 1
                    tmpList.Add(New Drawing.PointF(_pathPoints(i).Y, _pathPoints(i).X))
                Next
            Else
                Throw New ArgumentException(String.Format("Requested axis combination for outline is not supported!(axis1={0},axis2={1})", axis1.Name, axis2.Name))
            End If
            Return tmpList
        End Function

#Region "Private Data"
        Private _pathPoints As New List(Of Drawing.PointF)

        <NonSerialized()>
        Private _boundingBox As Drawing.RectangleF
#End Region

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentWrite(document, parentNode)

            Dim pathElement = document.AppendChildElement(parentNode, "Path")

            For Each point In _pathPoints
                Dim pointElement = document.AppendChildElement(pathElement, "Point")
                pointElement.SetAttribute("X", point.X.ToString(cultureIndependentFormat))
                pointElement.SetAttribute("Y", point.Y.ToString(cultureIndependentFormat))
            Next
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentRead(document, parentNode)

            _pathPoints = New List(Of Drawing.PointF)

            Dim pathNode As XmlNode = parentNode.Item("Path")

            If pathNode IsNot Nothing Then
                If pathNode.ChildNodes()(0).HasChildNodes Then
					For Each pointNode As XmlElement In pathNode.ChildNodes()
						_pathPoints.Add(New Drawing.PointF(pointNode.ReadChildElementAsSingle("X"), pointNode.ReadChildElementAsSingle("Y")))
					Next
				Else
					For Each pointNode As XmlElement In pathNode.ChildNodes()
						_pathPoints.Add(New Drawing.PointF(pointNode.GetAttributeAsSingle("X"), pointNode.GetAttributeAsSingle("Y")))
					Next
				End If 
            End If

            SetValuesOnDeserialized(Nothing)
        End Sub
    End Class

    <Serializable()>
    Public Class PolygonGate
        Inherits PointPathGate

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal xAx As Axis, ByVal yAx As Axis, ByVal path As List(Of Drawing.PointF))
            MyBase.New(xAx, yAx, path)
        End Sub

        Public Sub New(ByVal other As PolygonGate)
            MyBase.New(other)
        End Sub

        Public Overrides Function Clone() As IGate
            Return New PolygonGate(Me)
        End Function

        Public Overrides Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate
            Dim clone As PolygonGate = New PolygonGate(Me)

            clone._datafile = datafile

            If datafile IsNot Nothing Then
                clone.XAxis.CytoSettings = datafile.CytoSettings
                clone.YAxis.CytoSettings = datafile.CytoSettings
            End If

            Return clone
        End Function

        Public Overrides ReadOnly Property Type As GateType
            Get
                Return GateType.Polygon
            End Get
        End Property

    End Class

    <Serializable()>
    Public Class FreeFormGate
        Inherits PointPathGate

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal xAx As Axis, ByVal yAx As Axis, ByVal path As List(Of Drawing.PointF))
            MyBase.New(xAx, yAx, path)
        End Sub

        Public Sub New(ByVal other As FreeFormGate)
            MyBase.New(other)
        End Sub

        Public Overrides Function Clone() As IGate
            Return New FreeFormGate(Me)
        End Function

        Public Overrides Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate
            Dim clone As FreeFormGate = New FreeFormGate(Me)

            clone._datafile = datafile
            If datafile IsNot Nothing Then
                clone.XAxis.CytoSettings = datafile.CytoSettings
                clone.YAxis.CytoSettings = datafile.CytoSettings
            End If
            Return clone
        End Function

        Public Overrides ReadOnly Property Type As GateType
            Get
                Return GateType.FreeForm
            End Get
        End Property
    End Class
End Namespace
