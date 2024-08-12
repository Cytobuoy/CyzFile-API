Imports CytoSense.Data.ParticleHandling

Namespace Data.Analysis

    ''' <summary>
    ''' A set that contains all particles with an image in the current file
    ''' </summary>
    <Serializable()> Public Class AllImagesSet
        Inherits CytoSet

        Public Sub New()
            MyBase.New("All Imaged Particles", cytoSetType.allImages, Drawing.Color.Chartreuse)
        End Sub

        Public Sub New(name As String)
            MyBase.New(name, cytoSetType.allImages, Drawing.Color.Chartreuse)
        End Sub

        Public Sub New(ByRef datafile As CytoSense.Data.DataFileWrapper)
            MyBase.New("All Imaged Particles", cytoSetType.allImages, Drawing.Color.Chartreuse, datafile)

            RecalculateParticleIndices()
        End Sub

        Public Sub New(ByRef datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean)
            MyBase.New("All Imaged Particles", cytoSetType.allImages, Drawing.Color.Chartreuse, datafile, listId, vis)

            RecalculateParticleIndices()
        End Sub

        Public Sub New(other As AllImagesSet)
            MyBase.New(other.Name, other.Type, other.ColorOfSet, other._datafile, other.ListID, other.Visible)

            RecalculateParticleIndices()
        End Sub

        Public ReadOnly Overrides Property UseInIIF As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Property Datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get

            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value
                _ParticleIndices = Nothing
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Sub RecalculateParticleIndices()
            If _ParticleIndices IsNot Nothing OrElse _datafile Is Nothing Then
                Return ' static Set, calculation only needs to take place once
            End If

            Dim particles = _datafile.SplittedParticles
            Dim indices As New List(Of Integer)

            For i = 0 To particles.Length - 1
                If particles(i).hasImage Then
                    indices.Add(i)
                End If
            Next

            _ParticleIndices = indices.ToArray()
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            Return p.hasImage
        End Function

        Public Overrides Function Clone() As CytoSet
            Return New AllImagesSet(Me)
        End Function
    End Class

    '
    ' A set only to be used for Image focusing
    '
    <Serializable> Public Class ImageFocusSet
        Inherits AllImagesSet

        Enum FocusEnum
            BelowRange
            InsideRange
            AboveRange
        End Enum

        Private _ratioMin As Single = 0.5
        Private _ratioMax As Single = 1.5
        Private _focus As FocusEnum
        <NonSerialized> Protected x_axis, y_axis As SingleAxis
        <NonSerialized> Public Const ChannelParameter As Channel.ChannelData.ParameterSelector = Channel.ChannelData.ParameterSelector.Maximum
        <NonSerialized> Public Const ratioDelta As Single = 0.000001
        <NonSerialized> Private Shared _log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>
        ''' Updates all ImageFocusSets inside a setlist with new ratioMin/ratioMax values
        ''' </summary>
        ''' <param name="sets"></param>
        ''' <param name="ratioMin"></param>
        ''' <param name="ratioMax"></param>
        Public Shared Sub UpdateImageFocusSets(sets As SetsList, ratioMin As Single, ratioMax As Single)
            Dim imageFocusSet As ImageFocusSet

            For Each cytoSet As CytoSet In sets
                imageFocusSet = TryCast(cytoSet, ImageFocusSet)

                If imageFocusSet IsNot Nothing Then
                    Select Case imageFocusSet.Focus
                        Case ImageFocusSet.FocusEnum.BelowRange
                            imageFocusSet.ratioMax = ratioMin - ratioDelta
                        Case ImageFocusSet.FocusEnum.InsideRange
                            imageFocusSet.ratioMin = ratioMin
                            imageFocusSet.ratioMax = ratioMax
                        Case ImageFocusSet.FocusEnum.AboveRange
                            imageFocusSet.ratioMin = ratioMax + ratioDelta
                    End Select

                    Debug.WriteLine(String.Format("UpdateImageFocusSets [{0}] min [{1}] max [{2}]", imageFocusSet._datafile, imageFocusSet._ratioMin, imageFocusSet._ratioMax))
                End If
            Next
        End Sub

        Public Sub New(other As ImageFocusSet)
            Me.New(other._focus, other._datafile, other._ratioMin, other._ratioMax)

            ColorOfSet = other.ColorOfSet
            ListID = other.ListID
        End Sub

        Public Sub New(other As ImageFocusSet, datafile As DataFileWrapper)
            Me.New(other.Focus, datafile, other._ratioMin, other._ratioMax)

            ColorOfSet = other.ColorOfSet
            ListID = other.ListID
        End Sub

        Public Sub New(focus As FocusEnum, datafile As CytoSense.Data.DataFileWrapper, ratioMin As Single, ratioMax As Single)
            MyBase.New()

            _focus = focus

            Select Case _focus
                Case FocusEnum.BelowRange
                    Name = "Images focussed below range"
                Case FocusEnum.InsideRange
                    Name = "Images focussed inside range"
                Case FocusEnum.AboveRange
                    Name = "Images focussed above range"
            End Select

            _datafile = datafile

            If _datafile IsNot Nothing Then
                InitAxes(_datafile.CytoSettings)
            End If

            _Invalid = True
            Me.ratioMin = ratioMin
            Me.ratioMax = ratioMax

            RecalculateParticleIndices()
        End Sub

        Public Property ratioMin As Single
            Get
                Return _ratioMin
            End Get
            Set(value As Single)
                If value <> _ratioMin Then
                    _ratioMin = value
                    _Invalid = True

                    RecalculateParticleIndices()
                End If
            End Set
        End Property

        Public Property ratioMax As Single
            Get
                Return _ratioMax
            End Get
            Set(value As Single)
                If value <> _ratioMax Then
                    _ratioMax = value
                    _Invalid = True

                    RecalculateParticleIndices()
                End If
            End Set
        End Property

        Public Property Focus As FocusEnum
            Get
                Return _focus
            End Get
            Set(value As FocusEnum)
                _focus = value
            End Set
        End Property

        Public Sub InitAxes(settings As CytoSense.CytoSettings.CytoSenseSetting)
            Dim x_channel = settings.GetChannellistItemByType(CytoSettings.ChannelTypesEnum.FWSL)
            Dim y_channel = settings.GetChannellistItemByType(CytoSettings.ChannelTypesEnum.FWSR)

            If x_channel Is Nothing OrElse y_channel Is Nothing Then
                _log.Warn("You're trying to build an imageFocus plot for non-curvature machine!")
                ' Only happens when mixing new and old files of an instrument where this changed, so very rare.
                x_channel = settings.GetChannellistItemByType(CytoSettings.ChannelTypesEnum.FWS)
                y_channel = x_channel
            End If

            x_axis = New SingleAxis(x_channel, ChannelParameter, settings)
            y_axis = New SingleAxis(y_channel, ChannelParameter, settings)
        End Sub

        Public Overrides Function TestSingleParticle(particle As Particle) As Boolean
            If particle.hasImage Then
                If x_axis Is Nothing Then 'Still need to initialize the axis.
                    InitAxes(particle.CytoSettings)
                End If

                Dim x_value = x_axis.GetValue(particle) ' can be zero?
                Dim y_value = y_axis.GetValue(particle)
                Dim yx_ratio = y_value / x_value

                Return (yx_ratio >= ratioMin AndAlso yx_ratio <= ratioMax)
            Else
                Return False
            End If

            Return False
        End Function

        Public Overrides Sub RecalculateParticleIndices()
            If Not _Invalid OrElse Datafile Is Nothing Then
                Return
            End If

            Debug.WriteLine("ImageFocusSet.RecalculateParticleIndices for Set: {0}", Me)

            If Datafile.numberOfPictures = 0 OrElse Datafile.SplittedParticlesWithImages.Length = 0 Then
                _ParticleIndices = {}
            Else
                If x_axis Is Nothing Then 'Still need to initialize the axis.
                    InitAxes(Datafile.CytoSettings)
                End If

                Dim yx_ratio As Single
                Dim indices = New List(Of Integer)

                Dim particles = Datafile.SplittedParticles
                Dim x_values = x_axis.GetValues(Datafile)
                Dim y_values = y_axis.GetValues(Datafile)

                For i = 0 To particles.Length - 1
                    If particles(i).hasImage Then
                        yx_ratio = y_values(i) / x_values(i)

                        If (yx_ratio >= ratioMin AndAlso yx_ratio <= ratioMax) Then
                            indices.Add(i)
                        End If
                    End If
                Next

                _ParticleIndices = indices.ToArray()
            End If
        End Sub

        Public Overrides Function Clone() As CytoSet
            Return New ImageFocusSet(Me)
        End Function
    End Class
End Namespace