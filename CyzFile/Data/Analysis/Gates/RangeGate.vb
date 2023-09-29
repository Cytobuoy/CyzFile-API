Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public Class RangeGate
        Inherits Gate1D

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal ax As Axis, ByVal rangeMin As Single, ByVal rangeMax As Single)
            MyBase.New(ax)
            _rangeMin = rangeMin
            _rangeMax = rangeMax
        End Sub

        Public Sub New(ByVal other As RangeGate)
            MyBase.New(other)
            _rangeMin = other._rangeMin
            _rangeMax = other._rangeMax
        End Sub

        Public Overrides Sub RecalculateParticleIndices()
            If Not _Invalid OrElse _datafile Is Nothing Then
                Return
            End If

            If values Is Nothing OrElse values.Length = 0 Then
                LoadAxisValues()
            End If

            Debug.WriteLine("    RangeGate.RecalculateParticleIndices for Axis: {0}", Axis)

            Dim particles = _datafile.SplittedParticles
            Dim indices = New List(Of Integer)

            For i As Integer = 0 To particles.Length - 1
                If values(i) > _rangeMin AndAlso values(i) < _rangeMax Then
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
            Dim val As Single = Axis.GetValue(particle)

            If val > _rangeMin AndAlso val < _rangeMax Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Sequence: (maximum,-Inf), (maximum,+Inf), (minimal,+Inf),(minimal,-Inf)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property Outline As System.Collections.Generic.List(Of System.Drawing.PointF)
            Get
                Dim tmpList As New List(Of Drawing.PointF)
                tmpList.Add(New Drawing.PointF(_rangeMax, Single.NegativeInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMax, Single.PositiveInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMin, Single.PositiveInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMin, Single.NegativeInfinity))
                Return tmpList
            End Get
        End Property

        Public Overrides Function GetOutline(axis1 As Axis, axis2 As Axis) As List(Of Drawing.PointF)
            Dim tmpList As New List(Of Drawing.PointF)
            If axis1 IsNot Nothing AndAlso HasAxis(axis1) Then
                tmpList.Add(New Drawing.PointF(_rangeMax, Single.NegativeInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMax, Single.PositiveInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMin, Single.PositiveInfinity))
                tmpList.Add(New Drawing.PointF(_rangeMin, Single.NegativeInfinity))
            ElseIf axis2 IsNot Nothing AndAlso HasAxis(axis2) Then
                tmpList.Add(New Drawing.PointF(Single.NegativeInfinity, _rangeMax))
                tmpList.Add(New Drawing.PointF(Single.PositiveInfinity, _rangeMax))
                tmpList.Add(New Drawing.PointF(Single.PositiveInfinity, _rangeMin))
                tmpList.Add(New Drawing.PointF(Single.NegativeInfinity, _rangeMin))
            Else
                Throw New ArgumentException(String.Format("Requested axis combination for outline is not supported!(axis1={0},axis2={1})", axis1.Name, axis2.Name))
            End If
            Return tmpList
        End Function

        Public Overrides Function Clone() As IGate
            Return New RangeGate(Me)
        End Function

        Public Overrides Function CreateWorkfileCopy(datafile As DataFileWrapper) As IGate
            Dim clone As RangeGate = New RangeGate(Me)

            clone.DataFile = datafile

            If datafile IsNot Nothing Then
                clone.Axis.CytoSettings = datafile.CytoSettings
            End If

            Return clone
        End Function

        Public Overrides ReadOnly Property Type As GateType
            Get
                Return GateType.Range
            End Get
        End Property

        Public Property Min As Single
            Get
                Return _rangeMin
            End Get
            Set(value As Single)
                _rangeMin = value
                _Invalid = True
            End Set
        End Property

        Public Property Max As Single
            Get
                Return _rangeMax
            End Get
            Set(value As Single)
                _rangeMax = value
                _Invalid = True
            End Set
        End Property

#Region "Private Data"
        Private _rangeMin As Single
        Private _rangeMax As Single
#End Region '"Private Data"  

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentWrite(document, parentNode)

            document.AppendChildElement(parentNode, "RangeMin", _rangeMin)
            document.AppendChildElement(parentNode, "RangeMax", _rangeMax)
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement)
            MyBase.XmlDocumentRead(document, parentNode)

            _rangeMin = parentNode.ReadChildElementAsSingle("RangeMin")
            _rangeMax = parentNode.ReadChildElementAsSingle("RangeMax")
        End Sub

        Public Overrides ReadOnly Property Axes As Axis()
            Get
                Return New Axis() {Axis}
            End Get
        End Property

    End Class
End Namespace
