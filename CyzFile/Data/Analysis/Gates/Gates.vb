Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()>
    Public Class GateCollection
        Implements IEnumerable, IEnumerable(Of IGate)
        Implements IXmlDocumentIO

        Private _gates As New List(Of IGate)

        Public ReadOnly Property allGates As List(Of IGate)
            Get
                Return _gates
            End Get
        End Property

        Public Sub New()
        End Sub

        Public Sub Add(ByRef thisgate As IGate)
            _gates.Add(thisgate)
        End Sub

        ''' <summary>
        ''' deletes a gate defined in these axes, if there is any. Returns false if nothing is changed
        ''' </summary>
        ''' <param name="xAxis"></param>
        ''' <param name="yAxis"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Delete(xAxis As CytoSense.Data.Analysis.Axis, yAxis As CytoSense.Data.Analysis.Axis) As Boolean
            Dim removed = _gates.RemoveAll(Function(g As IGate) g.HasAxis(xAxis, yAxis))
            Return removed > 0
        End Function

        ''' <summary>
        ''' deletes a gate if it has the same axis as the one gate parameter
        ''' </summary>
        ''' <param name="gate">The "gate" to delete, the parameter specifies the axis to delete.</param>
        ''' <returns>True if one or more gates were deleted, false if none were deleted.</returns>
        ''' <remarks></remarks>
        Public Function Delete(gate As IGate) As Boolean
            Dim removed = _gates.RemoveAll(Function(g As IGate) g.HasAxis(gate))
            Return removed > 0
        End Function

        ''' <summary>
        ''' Remove the gate g from the collection, if g was present.
        ''' If g is not in the collection, the function has no effect.
        ''' </summary>
        ''' <param name="g">The gate to remove.</param>
        ''' <returns>True if g was removed, false if nothing was removed.</returns>
        ''' <remarks></remarks>
        Public Function Remove(g As IGate) As Boolean
            Return _gates.Remove(g)
        End Function

#Region "IEnumerable implemenation"

        Public Function GetEnumerator() As IEnumerator(Of IGate) Implements IEnumerable(Of IGate).GetEnumerator
            Return _gates.GetEnumerator
        End Function

        Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        Public Function Count() As Integer
            Return _gates.Count
        End Function

        Public Sub Clear()
            _gates.Clear()
        End Sub

        Public Function indexOf(ByRef item As IGate) As Integer
            Return _gates.IndexOf(item)
        End Function

        Public Function Find(ByRef match As System.Predicate(Of IGate)) As IGate
            Return _gates.Find(match)
        End Function

#End Region '"IEnumerable implementation"

        Public Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            For Each gate In _gates
                If TryCast(gate, IXmlDocumentIO) IsNot Nothing Then
                    Select Case gate.Type
                        Case GateType.Range
                            DirectCast(gate, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "RangeGate"))

                        Case GateType.Rectangle
                            DirectCast(gate, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "RectangleGate"))

                        Case GateType.Polygon
                            DirectCast(gate, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "PolygonGate"))

                        Case GateType.FreeForm
                            DirectCast(gate, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "FreeformGate"))
                    End Select
                End If
            Next
        End Sub

        Public Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            _gates = New List(Of IGate)()

            For Each childNode As XmlElement In parentNode.ChildNodes()
                Select Case childNode.Name
                    Case "RangeGate"
                        Dim gate As RangeGate = New RangeGate()
                        gate.XmlDocumentRead(document, childNode)
                        _gates.Add(gate)

                    Case "RectangleGate"
                        Dim gate As RectangleGate = New RectangleGate()
                        gate.XmlDocumentRead(document, childNode)
                        _gates.Add(gate)

                    Case "PolygonGate"
                        Dim gate As PolygonGate = New PolygonGate()
                        gate.XmlDocumentRead(document, childNode)
                        _gates.Add(gate)

                    Case "FreeformGate"
                        Dim gate As FreeFormGate = New FreeFormGate()
                        gate.XmlDocumentRead(document, childNode)
                        _gates.Add(gate)
                End Select
            Next

        End Sub
    End Class

End Namespace