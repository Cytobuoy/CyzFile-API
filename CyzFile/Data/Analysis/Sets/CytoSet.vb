Imports System.Drawing
Imports System.Xml
Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Serializing


Namespace Data.Analysis

    ''' <summary>
    ''' This is the abstract base class for various types of sets
    ''' List ID must be unique within the list it is in.  This should be handled
    ''' when adding to the list.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public MustInherit Class CytoSet
        Implements IXmlDocumentIO

        Protected _myType As cytoSetType
        Protected _name As String
        Protected _listID As Integer
        <NonSerialized> Protected _Invalid As Boolean = True
        <NonSerialized> Protected _ParticleIndices As Integer()
		<NonSerialized> Protected _reducedParticleIndices As Integer()
        <NonSerialized> Protected _datafile As CytoSense.Data.DataFileWrapper

        Public Sub New(ByVal name As String, ByVal type As cytoSetType, ByVal myColor As Color)
            _listID = -1
            _name = name
            _myType = type
            _myColor = myColor
            _visible = True
            _Invalid = True
        End Sub

        Public Sub New(ByVal name As String, ByVal type As cytoSetType, ByVal myColor As Color, ByVal datafile As CytoSense.Data.DataFileWrapper)
            Me.New(name, type, myColor)

            _datafile = datafile
        End Sub

        Public Sub New(ByVal name As String, ByVal type As cytoSetType, ByVal myColor As Color, ByVal datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean)
            Me.New(name, type, myColor, datafile)

            _listID = listId
            _visible = vis
        End Sub

#Region "General Set Properties"
        Public ReadOnly Overridable Property UseInIIF As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overridable Property Invalid As Boolean
            Get
                Return _Invalid
            End Get

            Set(value As Boolean)
                _Invalid = value
            End Set
        End Property

        <NonSerialized()> Private _conc As Double

        ''' <remarks>concentration in parts/uL</remarks>
        Public ReadOnly Property Concentration(ByVal mode As ConcentrationModeEnum) As Double
            Get
                _conc = (Me.ParticleIndices.Count / _datafile.MeasurementInfo.NumberofSavedParticles) / _datafile.LoadedParticlesFraction * _datafile.Concentration(mode)

                Return _conc
            End Get
        End Property

        Public ReadOnly Property Concentration As Double
            Get
                _conc = (Me.ParticleIndices.Count / _datafile.MeasurementInfo.NumberofSavedParticles) / _datafile.LoadedParticlesFraction * _datafile.Concentration()

                Return _conc
            End Get
        End Property


        Public ReadOnly Property Percentage() As Double
            Get
                Return (Me.ParticleIndices.Count / _datafile.MeasurementInfo.NumberofSavedParticles / _datafile.LoadedParticlesFraction)
            End Get
        End Property

        ''' <summary>
        ''' This function is for compatibility only, use the ParticleIndices property!
        ''' The results are calculated every call, so it will kill performance if used
        ''' I WOULD LIKE TO SEE THIS FUNCTION REMOVED
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ParticleIDs As Int32()
            Get
                Return _datafile.GetParticleIDs(ParticleIndices)
            End Get
        End Property

		Public ReadOnly Property ReducedParticleIndices As Integer()
			Get
				If _datafile.PercentageOfParticlesShown = 100 Then
					Return ParticleIndices
				End If
				Dim indices = ParticleIndices
				Dim stepsize = 100/_datafile.percentageOfParticlesShown
				Dim reducedIndices = new List(Of Integer)
				Dim counter = 0
				For i = 0 To indices.Length - 1
					If i >= stepsize * counter Then
						reducedIndices.Add(indices(i))
						counter+=1
					End If
				Next
				_reducedParticleIndices = reducedIndices.ToArray()
				Return _reducedParticleIndices
			End Get
		End Property
        
		Public ReadOnly Property ReducedParticles As Particle()
			Get
				If _datafile.PercentageOfParticlesShown = 100 Then
					Return Particles
				End If
				Dim indices = ParticleIndices
				dim reducedIndices = ReducedParticleIndices
                Dim allParticles = _datafile.SplittedParticles
				Dim reducedSetParticles() = New ParticleHandling.Particle(reducedIndices.Count - 1) {}

                For i = 0 To reducedIndices.Count - 1
                    reducedSetParticles(i) = allParticles(reducedIndices(i))
                Next

                Return reducedSetParticles
			End Get
		End Property

		''' <summary>
        ''' This function is for compatibility only, use the ParticleIndices property!
        ''' The results are calculated every call, so it will kill performance if used
        ''' I WOULD LIKE TO SEE THIS FUNCTION REMOVED
        ''' </summary>
        ''' <returns></returns>
		''' 
        Public ReadOnly Property Particles As ParticleHandling.Particle()
            Get
                Dim indices = ParticleIndices
                Dim setParticles() = New ParticleHandling.Particle(indices.Length - 1) {}
                Dim allParticles = _datafile.SplittedParticles

                For i = 0 To indices.Length - 1
                    setParticles(i) = allParticles(indices(i))
                Next

                Return setParticles
            End Get
        End Property

        Public Overridable ReadOnly Property ParticleIndices As Integer()
            Get
                If _ParticleIndices Is Nothing Then
                    'Return New Integer() {}
                    RecalculateParticleIndices()
                End If

                Return _ParticleIndices
            End Get
        End Property

        Protected _myColor As Color

        Public Property colorOfSet() As Color
            Get
                Return _myColor
            End Get
            Set(ByVal value As Color)
                _myColor = value
            End Set
        End Property

        Public ReadOnly Property contrastingColor() As Color
            Get
                'Transform to hsv for easy manipulation
                Dim h = _myColor.GetHue
                Dim s = _myColor.GetSaturation
                Dim v = _myColor.GetBrightness
                Return ColorFromHSV(h, 1, v)

            End Get
        End Property

        ''' <summary>
        ''' Set this property to true for sets that you want to display as a child node only, and not as a top level
        ''' node!  Sort of a hack to support automatic or sets.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property ChildOnly As Boolean

        Private Function ColorFromHSV(h As Single, s As Single, l As Single) As System.Drawing.Color
            ' see https://en.wikipedia.org/wiki/HSV_color_space#Converting_to_RGB

            Dim c = (1 - Math.Abs(2 * l - 1)) * s
            Dim hreg As Single = h / 60
            Dim x = c * (1 - Math.Abs(hreg Mod 2 - 1))
            Dim r, g, b As Single
            If hreg >= 0 AndAlso hreg <= 1 Then
                r = c
                g = x
                b = 0
            ElseIf hreg > 1 AndAlso hreg <= 2 Then
                r = x
                g = c
                b = 0
            ElseIf hreg > 2 AndAlso hreg <= 3 Then
                r = 0
                g = c
                b = x
            ElseIf hreg > 3 AndAlso hreg <= 4 Then
                r = 0
                g = x
                b = c
            ElseIf hreg > 4 AndAlso hreg <= 5 Then
                r = x
                g = 0
                b = c
            ElseIf hreg > 5 AndAlso hreg <= 6 Then
                r = c
                g = 0
                b = x
            Else
                r = 0
                g = 0
                b = 0
            End If
            Dim m = l - 0.5 * c
            Return Color.FromArgb(255, CInt((r + m) * 255), CInt((g + m) * 255), CInt((b + m) * 255))
        End Function

        Public Property Name() As String
            Set(ByVal value As String)
                _name = value
            End Set
            Get
                Return _name
            End Get
        End Property

        Public ReadOnly Property type As cytoSetType
            Get
                Return _myType
            End Get
        End Property

        Protected _visible As Boolean
        Public Property Visible() As Boolean
            Set(ByVal value As Boolean)
                _visible = value
            End Set
            Get
                Return _visible
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _name
        End Function

        ''' <summary>
        ''' The place in the sets list of this set (zero-based, only default set can have 0)
        ''' Used to be the place, currently simply an ID.
        ''' </summary>
        ''' <remarks>Mainly used for drawing the tree structure, and the list in the image viewer. This does not take hierarchy into account.</remarks>
        Public Property ListID As Integer
            Get
                Return _listID
            End Get
            Set(ByVal value As Integer)
                _listID = value
            End Set
        End Property

#End Region

        Public MustOverride Property datafile As CytoSense.Data.DataFileWrapper

        Public MustOverride Sub RecalculateParticleIndices()

        Public MustOverride Function TestSingleParticle(p As CytoSense.Data.ParticleHandling.Particle) As Boolean

        Public Function TestSingleParticleIndex(particleIndex As Integer) As Boolean
            If particleIndex = -1 Then
                Throw New Exception("TestSingleParticleIndex: index value = -1")
            End If

            For Each index In _ParticleIndices
                If index = particleIndex Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public MustOverride Function Clone() As CytoSet

        Public Overridable Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            parentNode.SetAttribute("ListID", _listID.ToString())
            parentNode.SetAttribute("Name", _name)
            parentNode.SetAttribute("Color", _myColor.Name)
            parentNode.SetAttribute("Visible", _visible.ToString())
        End Sub

        Public Overridable Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            If parentNode.TryGetAttribute(Of Integer)("ListID", _listID)
				_name = parentNode.GetAttribute("Name")
				_myColor = Color.FromName(parentNode.GetAttribute("Color"))
				_visible = parentNode.GetAttributeAsBoolean("Visible")
			Else		
				_listID = parentNode.ReadChildElementAsInteger("ListID")
				_name = parentNode.ReadChildElementAsString("Name")
				_myColor = Color.FromName(parentNode.ReadChildElementAsString("Color"))
				_visible = parentNode.ReadChildElementAsBoolean("Visible")
			End If
        End Sub
    End Class

    <Serializable()> Public Enum cytoSetType
        Invalid = 0
        gateBased
        indexBased
        combined
        allImages
        unassignedParticles
        DefaultAll ' The default set with all particles.
        OrSet
        SuccessFullyCroppedImages
        CropFailedImages
    End Enum

    ''' <summary>
    ''' A class that generates nice and bright colors for the sets.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class CytoSetDefaultColors
        Public Shared colorList As Color() = {Color.Blue, Color.Red, Color.Green, Color.Orange, Color.CornflowerBlue, Color.Purple, Color.Chartreuse, Color.Yellow}
        Public Shared colorIndex As Integer = 0

        Public Shared Function getNextColor() As Color
            Dim col As Color = getcolor(colorIndex)
            colorIndex += 1
            Return col
        End Function

        Public Shared Function getcolor(ByVal index As Integer) As Color
            If index < colorList.Length Then
                Return colorList(index)
            Else
                'Find some more colors
                For Each knownCol As Integer In [Enum].GetValues(GetType(KnownColor))
                    'check if the knownColor variable is a System color
                    If knownCol > KnownColor.Transparent And Not colorList.Contains(Color.FromKnownColor(CType(knownCol,KnownColor))) And Color.FromKnownColor(CType(knownCol,KnownColor)).GetSaturation > 0.3 And Color.FromKnownColor(CType(knownCol,KnownColor)).GetBrightness < 0.8 Then
                        'add it to our list
                        ReDim Preserve colorList(colorList.Length)
                        colorList(colorList.Length - 1) = Color.FromKnownColor(CType(knownCol,KnownColor))
                    End If
                Next
                If index < colorList.Length Then
                    Return colorList(index)
                Else
                    Return Color.Black ' Return black (after approx. 93 sets) let the user define his/her own colors, we give up.
                End If
            End If
        End Function

        Public Overloads Shared Sub resetCounter()
            colorIndex = 0
        End Sub

        Public Overloads Shared Sub resetCounter(startIndex As Integer)
            colorIndex = startIndex
        End Sub
    End Class

    ''' <summary>
    ''' During the (XML) de-serialization of a set list we need a class to store the ListID of sets that
    ''' are used by an OrSet or CombinedSet. This because the sets we need may not yet be deserialized. 
    ''' We use this dummy class to store the ListID of the class we need and later on, when we have
    ''' deserialized all sets, we replace the dummy classes with the "real" classes.
    ''' </summary>
    Public Class CytoSetDummy
        Inherits CytoSet

        Public Sub New(ListID As Integer)
            MyBase.New("dummy", cytoSetType.Invalid, Drawing.Color.LightCyan)

            _listID = ListID
        End Sub

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Overrides Property datafile As DataFileWrapper
            Get
                Throw New NotImplementedException()
            End Get
            Set(value As DataFileWrapper)
                Throw New NotImplementedException()
            End Set
        End Property

        Public Overrides Sub RecalculateParticleIndices()
            Throw New NotImplementedException()
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Overrides Function Clone() As CytoSet
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
