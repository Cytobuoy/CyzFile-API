Imports System.Xml
Imports CytoSense.CytoSettings
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public MustInherit Class Gate2D
        Inherits Gate

        Private _xAxis As Axis
        Private _yAxis As Axis
        <NonSerialized> Public xValues As Single()
        <NonSerialized> Public yValues As Single()

        ''' <summary>
        ''' Needed for XML (de-)serialization
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        Protected Sub New(xAx As Axis, yAx As Axis)
            MyBase.New()
            _xAxis = xAx
            _yAxis = yAx
        End Sub

        Protected Sub New(other As Gate2D)
            MyBase.New()
            _datafile = other._datafile
            _xAxis = other._xAxis
            _yAxis = other._yAxis
        End Sub

        Public Overrides Sub LoadAxisValues()
            If _datafile Is Nothing Then
                xValues = Nothing
                yValues = Nothing
            Else
                xValues = XAxis.GetValues(_datafile)
                yValues = YAxis.GetValues(_datafile)
            End If
        End Sub

        Public ReadOnly Property YAxis() As Axis
            Get
                Return _yAxis
            End Get
        End Property

        Public ReadOnly Property XAxis() As Axis
            Get
                Return _xAxis
            End Get
        End Property

        Public Overrides Function HasAxis(thisAxis As Axis) As Boolean
            Return XAxis = thisAxis OrElse YAxis = thisAxis
        End Function

        ''' <summary>
        ''' Check if the other gate has the same axis as we do.
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns>True if the other gate has the same axis as we do, false if not.</returns>
        Public Overrides Function HasAxis(other As IGate) As Boolean
            Return other.HasAxis(_xAxis, _yAxis)
        End Function

        Public Overrides Function HasAxis(ByVal Ax1 As Axis, ByVal Ax2 As Axis) As Boolean
            Return (Ax1 = XAxis AndAlso Ax2 = YAxis) OrElse (Ax1 = YAxis AndAlso Ax2 = XAxis)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} on {1} vs {2}", Type.ToString, XAxis.Name, YAxis.Name)
        End Function

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            _xAxis.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "XAxis"))
            _yAxis.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "YAxis"))
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)

            Dim axisElement As XmlElement

            axisElement = parentNode.Item("XAxis")

            Select Case axisElement.GetAttribute("Type")
                Case GetType(SingleAxis).Name
                    _xAxis = New SingleAxis()
                    _xAxis.XmlDocumentRead(document, axisElement)

                Case GetType(RatioAxis).Name
                    _xAxis = New RatioAxis()
                    _xAxis.XmlDocumentRead(document, axisElement)
            End Select

            axisElement = parentNode.Item("YAxis")

            Select Case axisElement.GetAttribute("Type")
                Case GetType(SingleAxis).Name
                    _yAxis = New SingleAxis()
                    _yAxis.XmlDocumentRead(document, axisElement)

                Case GetType(RatioAxis).Name
                    _yAxis = New RatioAxis()
                    _yAxis.XmlDocumentRead(document, axisElement)
            End Select
        End Sub

        Public Overrides Sub UpdateCytoSettings(settings As CytoSenseSetting) 
            _xAxis.UpdateCytoSettings(settings)
            _yAxis.UpdateCytoSettings(settings)
        End Sub


        Public Overrides ReadOnly Property Axes As Axis()
            Get
                Return New Axis() {XAxis, YAxis}
            End Get
        End Property

    End Class
End Namespace
