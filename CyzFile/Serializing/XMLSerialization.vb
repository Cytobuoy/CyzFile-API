Imports System.Xml
Imports System.Globalization
Imports System.Runtime.CompilerServices

Namespace Serializing

    Public Interface IXmlDocumentIO
        ''' <summary>
        ''' Implementations of XmlDocumentWrite should append child nodes to
        ''' the parent node.
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parentNode"></param>
        Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement)

        ''' <summary>
        ''' Implementations of XmlRead should read child nodes of the parent
        ''' node into variables
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parentNode"></param>
        Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement)
    End Interface


    ''' <summary>
    ''' 
    ''' </summary>
    Public Module XMLDocumentExtensions
        Dim cultureIndependentFormat As NumberFormatInfo = New NumberFormatInfo()


        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String) As XmlAttribute
            Dim attribute = document.CreateAttribute(name)
            node.SetAttributeNode(attribute)
            Return attribute
        End Function

        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String, value As String) As XmlAttribute
            Dim attribute = document.CreateAttribute(name)
            attribute.Value = value
            node.SetAttributeNode(attribute)
            Return attribute
        End Function

        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String, value As Single) As XmlAttribute
            Return AddAttribute(document, node, name, value.ToString(cultureIndependentFormat))
        End Function

        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String, value As Double) As XmlAttribute
            Return AddAttribute(document, node, name, value.ToString(cultureIndependentFormat))
        End Function

        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String, value As Boolean) As XmlAttribute
            Return AddAttribute(document, node, name, value.ToString().ToLower())
        End Function


        <Extension>
        Public Function AddAttribute(document As XmlDocument, node As XmlElement, name As String, dt As DateTime ) As XmlAttribute
            Return AddAttribute(document, node, name, dt.ToString("o", cultureIndependentFormat))
        End Function

        <Extension>
        Public Function GetAttributeAsDateTime( node As XmlElement, name As String) As DateTime
            Return DateTime.Parse(node.GetAttributeNode(name).Value)
        End Function

        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As DateTime) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                Return DateTime.TryParse(attr.Value, value)
            Else
                Return False
            End If
        End Function




        <Extension>
        Public Function GetAttributeAsString( node As XmlElement, name As String) As String
            Return node.GetAttributeNode(name).Value
        End Function

        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As String) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = attr.Value
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As String, defValue As String) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = attr.Value
                Return True
            Else
                value = defValue
                Return False
            End If
        End Function

        <Extension>
        Public Function TryGetAttribute(Of T)( node As XmlElement, name As String, ByRef value As T) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = CType([Enum].Parse(GetType(T), attr.Value), T)
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function TryGetAttribute(Of T)( node As XmlElement, name As String, ByRef value As T, defValue As T) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = CType([Enum].Parse(GetType(T), attr.Value), T)
                Return True
            Else
                value = defValue
                Return False
            End If
        End Function

        <Extension>
        Public Function GetAttributeAsBoolean( node As XmlElement, name As String) As Boolean
            Return Boolean.Parse(node.GetAttributeNode(name).Value)
        End Function

        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As Boolean) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = Boolean.Parse(attr.Value)
                Return True
            Else
                Return False
            End If
        End Function
        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As Boolean, defValue As Boolean) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = Boolean.Parse(attr.Value)
                Return True
            Else
                value = defValue
                Return False
            End If
        End Function

        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As Integer) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = Integer.Parse(attr.Value)
                Return True
            Else
                Return False
            End If
        End Function
        <Extension>
        Public Function TryGetAttribute( node As XmlElement, name As String, ByRef value As Integer, defValue As Integer) As Boolean
            Dim attr = node.GetAttributeNode(name)
            If attr IsNot Nothing Then
                value = Integer.Parse(attr.Value)
                Return True
            Else
                value = defValue
                Return False
            End If
        End Function


        <Extension>
        Public Function GetAttributeAsSingle( node As XmlElement, name As String) As Single
            Return Single.Parse(node.GetAttributeNode(name).Value, cultureIndependentFormat)
        End Function

        <Extension>
        Public Function GetAttributeAsDouble( node As XmlElement, name As String) As Double
            Return Double.Parse(node.GetAttributeNode(name).Value, cultureIndependentFormat)
        End Function



        <Extension>
        Public Function AppendChildElement(document As XmlDocument, parentNode As XmlElement, name As String) As XmlElement
            Dim element As XmlElement = document.CreateElement(name)
            parentNode.AppendChild(element)
            Return element
        End Function

        <Extension>
        Public Function AppendChildElement(document As XmlDocument, parentNode As XmlElement, name As String, value As String) As XmlElement
            Dim element As XmlElement = document.CreateElement(name)
            element.InnerText = value
            parentNode.AppendChild(element)
            Return element
        End Function


        <Extension>
        Public Function AppendChildElement(document As XmlDocument, parentNode As XmlElement, name As String, value As Single) As XmlElement
            Return AppendChildElement(document, parentNode, name, value.ToString(cultureIndependentFormat))
        End Function

        <Extension>
        Public Function AppendChildElement(document As XmlDocument, parentNode As XmlElement, name As String, value As Double) As XmlElement
            Return AppendChildElement(document, parentNode, name, value.ToString(cultureIndependentFormat))
        End Function

        <Extension>
        Public Function AppendChildElement(document As XmlDocument, parentNode As XmlElement, name As String, value As Boolean) As XmlElement
            Return AppendChildElement(document, parentNode, name, value.ToString().ToLower())
        End Function

        <Extension>
        Public Function ReadChildElementAsSingle(parentNode As XmlElement, name As String) As Single
            Return Single.Parse(parentNode.Item(name).InnerText, cultureIndependentFormat)
        End Function

        <Extension>
        Public Function ReadChildElementAsDouble(parentNode As XmlElement, name As String) As Double
            Return Double.Parse(parentNode.Item(name).InnerText, cultureIndependentFormat)
        End Function

        <Extension>
        Public Function ReadChildElementAsInteger(parentNode As XmlElement, name As String) As Integer
            Return Integer.Parse(parentNode.Item(name).InnerText)
        End Function

        <Extension>
        Public Function ReadChildElementAsLong(parentNode As XmlElement, name As String) As Long
            Return Long.Parse(parentNode.Item(name).InnerText)
        End Function

        <Extension>
        Public Function ReadChildElementAsBoolean(parentNode As XmlElement, name As String) As Boolean
            Return Boolean.Parse(parentNode.Item(name).InnerText)
        End Function

        <Extension>
        Public Function ReadChildElementAsString(parentNode As XmlElement, name As String) As String
            Return parentNode.Item(name).InnerText
        End Function

        <Extension>
        Public Function TryReadChildElement(parentNode As XmlElement, name As String, ByRef value As String) As Boolean
            Dim item = parentNode.Item(name)
            If item IsNot Nothing Then
                value = item.InnerText
                Return True
            Else
                Return False
            End If
        End Function

        <Extension>
        Public Function TryReadChildElement(parentNode As XmlElement, name As String, ByRef value As String, defValue As String) As Boolean
            Dim item = parentNode.Item(name)
            If item IsNot Nothing Then
                value = item.InnerText
                Return True
            Else
                value = defValue
                Return False
            End If
        End Function

        <Extension>
        Public Function ReadChildElementAsEnum(Of T)(parentNode As XmlElement, name As String) As T
            Return CType([Enum].Parse(GetType(T), parentNode.Item(name).InnerText), T)
        End Function
    End Module

End Namespace
