Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public Class RectangleGate
        Inherits Gate2D

        ''' <summary>
        ''' Needed for XML (de-)serialization
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Instantiates a rectangular gate
        ''' </summary>
        ''' <param name="rectangle">The rectangle in axis coordinates. Note that we adopt the convention from the drawing namespace: the location (x,y) of the 
        ''' rectangle are the minimal coordinates. In axis coordinates this is the lower left corner, and when transformed to screen coordinates, other classes
        ''' will have to adapt.</param>
        ''' <remarks></remarks>
        Public Sub New(xAx As Axis, yAx As Axis, rectangle As Drawing.RectangleF)
            MyBase.New(xAx, yAx)
            _rectangle = rectangle
            _ParticleIndices = Nothing
        End Sub

        ''' <summary>
        ''' Instantiates a rectangular gate
        ''' </summary>
        ''' <param name="xMin">The minimal x value in axis coordinates</param>
        ''' <param name="xMax">In axis coordinates</param>
        ''' <param name="yMin">The minimal x value in axis coordinates</param>
        ''' <param name="yMax">In axis coordinates</param>
        Public Sub New(ByRef xAxis As CytoSense.Data.Analysis.Axis, ByRef yAxis As CytoSense.Data.Analysis.Axis, ByVal xMin As Single, ByVal xMax As Single, ByVal yMin As Single, ByVal yMax As Single)
            Me.New(xAxis, yAxis, New Drawing.RectangleF(xMin, yMin, (xMax - xMin), (yMax - yMin)))
            _ParticleIndices = Nothing
        End Sub

        Public Sub New(other As RectangleGate)
            MyBase.New(other)
            _rectangle = other._rectangle
            _ParticleIndices = Nothing
        End Sub

        Public Overrides ReadOnly Property Type As GateType
            Get
                Return GateType.Rectangle
            End Get
        End Property

        Public Overrides Function Clone() As IGate
            Return New RectangleGate(Me)
        End Function

        Public Overrides Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate
            Dim clone As RectangleGate = New RectangleGate(Me)

            clone.DataFile = datafile

            If datafile IsNot Nothing Then
                clone.XAxis.CytoSettings = datafile.CytoSettings
                clone.YAxis.CytoSettings = datafile.CytoSettings
            End If

            Return clone
        End Function

        Public Overrides Sub RecalculateParticleIndices()
            If Not _Invalid OrElse _datafile Is Nothing Then
                Return
            End If

            If xValues Is Nothing OrElse yValues Is Nothing Then
                LoadAxisValues()
            End If

            Debug.WriteLine("    RectangleGate.RecalculateParticleIndices for XAxis: {0}", XAxis)
            Debug.WriteLine("    RectangleGate.RecalculateParticleIndices for YAxis: {0}", YAxis)

            Dim particles = _datafile.SplittedParticles
            Dim indices = New List(Of Integer)

            For i = 0 To particles.Length - 1
                If _rectangle.Contains(New Drawing.PointF(xValues(i), yValues(i))) Then
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

            If _rectangle.Contains(New Drawing.PointF(x, y)) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Returns the outline of the rectangle (in axis coordinates), starting with the minimal x-y pair, and then clockwise (first y increase).
        ''' </summary>
        Public Overrides ReadOnly Property Outline As System.Collections.Generic.List(Of System.Drawing.PointF)
            Get
                Dim tmpList As New List(Of Drawing.PointF)
                tmpList.Add(New Drawing.PointF(_rectangle.X, _rectangle.Y))
                tmpList.Add(New Drawing.PointF(_rectangle.X, _rectangle.Y + _rectangle.Height))
                tmpList.Add(New Drawing.PointF(_rectangle.X + _rectangle.Width, _rectangle.Y + _rectangle.Height))
                tmpList.Add(New Drawing.PointF(_rectangle.X + _rectangle.Width, _rectangle.Y))
                Return tmpList
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
                tmpList.Add(New Drawing.PointF(_rectangle.X, _rectangle.Y))
                tmpList.Add(New Drawing.PointF(_rectangle.X, _rectangle.Y + _rectangle.Height))
                tmpList.Add(New Drawing.PointF(_rectangle.X + _rectangle.Width, _rectangle.Y + _rectangle.Height))
                tmpList.Add(New Drawing.PointF(_rectangle.X + _rectangle.Width, _rectangle.Y))
            ElseIf axis1 = YAxis And axis2 = XAxis Then
                tmpList.Add(New Drawing.PointF(_rectangle.Y, _rectangle.X))
                tmpList.Add(New Drawing.PointF(_rectangle.Y + _rectangle.Height, _rectangle.X))
                tmpList.Add(New Drawing.PointF(_rectangle.Y + _rectangle.Height, _rectangle.X + _rectangle.Width))
                tmpList.Add(New Drawing.PointF(_rectangle.Y, _rectangle.X + _rectangle.Width))
            Else
                Throw New ArgumentException(String.Format("Requested axis combination for outline is not supported!(axis1={0},axis2={1})", axis1.Name, axis2.Name))
            End If
            Return tmpList
        End Function

        Public Property Rectangle As Drawing.RectangleF
            Get
                Return _rectangle
            End Get
            Set(value As Drawing.RectangleF)
                _rectangle = value
                _datafile = Nothing ' re-enable RecalculateIDs
                _ParticleIndices = Nothing
            End Set
        End Property

        Private _rectangle As Drawing.RectangleF

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentWrite(document, parentNode)

            parentNode.SetAttribute("X", _rectangle.X.ToString(cultureIndependentFormat))
            parentNode.SetAttribute("Y", _rectangle.Y.ToString(cultureIndependentFormat))
            parentNode.SetAttribute("Width", _rectangle.Width.ToString(cultureIndependentFormat))
            parentNode.SetAttribute("Height", _rectangle.Height.ToString(cultureIndependentFormat))
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentRead(document, parentNode)

            If parentNode.TryGetAttribute(Of Single)("X", _rectangle.X) Then
				_rectangle.Y = parentNode.GetAttributeAsSingle("Y")
				_rectangle.Width = parentNode.GetAttributeAsSingle("Width")
				_rectangle.Height = parentNode.GetAttributeAsSingle("Height")
			Else
				_rectangle.X = parentNode.ReadChildElementAsSingle("X")
				_rectangle.Y = parentNode.ReadChildElementAsSingle("Y")
				_rectangle.Width = parentNode.ReadChildElementAsSingle("Width")
				_rectangle.Height = parentNode.ReadChildElementAsSingle("Height")
			End If
        End Sub

    End Class
End Namespace
