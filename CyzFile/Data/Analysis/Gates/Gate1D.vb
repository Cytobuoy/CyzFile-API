Imports System.Xml
Imports CytoSense.CytoSettings
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public MustInherit Class Gate1D
        Inherits Gate

        <NonSerialized> Public values As Single()

        Public Sub New()
            MyBase.New()
        End Sub

        Protected Sub New(ax As Axis)
            MyBase.New()
            _Axis = ax
        End Sub

        Protected Sub New(other As Gate1D)
            _datafile = other._datafile
            _Axis = other._Axis
        End Sub

        Public Overrides Sub LoadAxisValues()
            If _datafile Is Nothing Then
                values = Nothing
            Else
                values = _Axis.GetValues(_datafile)
            End If
        End Sub

        Public ReadOnly Property Axis() As Axis
            Get
                Return _Axis
            End Get
        End Property

        Public Overrides Function HasAxis(thisAxis As Axis) As Boolean
            Return Axis = thisAxis
        End Function

        ''' <summary>
        ''' Check if the other gate has the same axis as we do.
        ''' </summary>
        ''' <param name="other"></param>
        ''' <returns>True if the other gate has the same axis as we do, false if not.</returns>
        Public Overrides Function HasAxis(other As IGate) As Boolean
            Return other.HasAxis(_Axis)
        End Function

        ''' <summary>
        ''' Return True if the axis used matches one of the 2 axis.  It does not matter which one. This definition
        ''' makes it easier when drawing gates in dot plots.  They need to care less about the
        ''' type of the gate.
        ''' </summary>
        ''' <param name="xAx"></param>
        ''' <param name="yAx"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function HasAxis(xAx As Axis, yAx As Axis) As Boolean
            Return Axis = xAx OrElse Axis = yAx
        End Function

        Private _Axis As Axis

        Public Overrides Function ToString() As String
            Return String.Format("{0} on {1}", Type.ToString, Axis.Name)
        End Function

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            _Axis.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "Axis"))
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)

            Dim axisElement As XmlElement = parentNode.Item("Axis")

            Select Case axisElement.GetAttribute("Type")
                Case GetType(SingleAxis).Name
                    _Axis = New SingleAxis()
                    _Axis.XmlDocumentRead(document, axisElement)

                Case GetType(RatioAxis).Name
                    _Axis = New RatioAxis()
                    _Axis.XmlDocumentRead(document, axisElement)
            End Select
        End Sub

        Public Overrides Sub UpdateCytoSettings(settings As CytoSenseSetting) 
            _Axis.UpdateCytoSettings(settings)
        End Sub

    End Class
End Namespace
