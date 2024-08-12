Imports System.Xml
Imports CytoSense.Serializing
Imports CytoSense.Data.ParticleHandling

Namespace Data.Analysis

    ''' <summary>
    ''' Used internally for testing, a set containing all images that were successfully
    ''' cropped.  Note: The results will only be correct after the cropping has finished
    ''' completely.  So you may have to wait a while. This is not a pretty solution, 
    ''' so maybe we need to add some more before we actually release it.
    ''' For now it will only be present in Debug builds.
    ''' </summary>
    ''' <remarks>
    ''' The class will set a timer when it is first recalculated that will scan
    ''' all imaged particles once a minute or so.  It collects all successfully cropped particles,
    ''' if any copping waiting particles are found, then it will reset the timer.  If all particles
    ''' were either cropped or uncropped, the data is ready and the timer is stopped.
    ''' (Note: we change the name, remove the * from it, that was added initially).
''' NOTE: Need to some way to get the parameters for cropping in.  
''' Not sure on how to serialize them. But if we do not, then how can we load it.  Trick is what if
''' user changes the settings.  These sets are only internal debugging, so it is not a problem now.
''' but could become one later.
''' Need to add them to all ctors with parameters...
''' Also RvdH is currently working on restructuring the sets, that may influence this. (Or not)
''' NOTE: We do not really need the actual cropped image, we just need to know if we could, so
''' we do not need the margins, and we do not need the image, so this can be simplified.
''' (But not today).
    ''' </remarks>
    <Serializable()>
    Public Class SuccessFullyCroppedImagesSet
        Inherits CytoSet

        Public Const SETNAME As String = "Successfully Cropped Images"
        <NonSerialized> Private _calcFinished As Boolean 'Set to True to indicate that the calculations are finished, and we have all particles.
        <NonSerialized> Private _recalcTimer As System.Threading.Timer

        Public ReadOnly Overrides Property UseInIIF As Boolean
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property CropMarginBase As Integer
            Get
                Return _cropMarginBase
            End Get
        End Property
        Public ReadOnly Property CropMarginFactor As Double
            Get
                Return _cropMarginBase
            End Get
        End Property
        Public ReadOnly Property CropBgThreshold As Integer
            Get
                Return _cropBgThreshold
            End Get
        End Property
        Public ReadOnly Property CropErosionDilation As Integer
            Get
                Return _cropErosionDilation
            End Get
        End Property

        ' Cropping/image detection settings to us.
        Private _cropMarginBase      As Integer
        Private _cropMarginFactor    As Double
        Private _cropBgThreshold     As Integer 
        Private _cropErosionDilation As Integer

        ''' <summary>
        ''' Deserialization only.
        ''' </summary>
        Public Sub New()
            MyBase.New(SETNAME, cytoSetType.SuccessFullyCroppedImages, Drawing.Color.Blue)
        End Sub
        Public Sub New(ByRef datafile As CytoSense.Data.DataFileWrapper,
                   cropMarginBase As Integer, 
                   cropMarginFactor As Double, 
                   cropBgThreshold As Integer, 
                   cropErosionDilation As Integer)
            MyBase.New(SETNAME, cytoSetType.SuccessFullyCroppedImages, Drawing.Color.Blue, datafile)
            _cropMarginBase      = cropMarginBase
            _cropMarginFactor    = cropMarginFactor
            _cropBgThreshold     = cropBgThreshold
            _cropErosionDilation = cropErosionDilation

            RecalculateParticleIndices()
        End Sub

        Public Sub New(ByRef datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean,
                   cropMarginBase As Integer, 
                   cropMarginFactor As Double, 
                   cropBgThreshold As Integer, 
                   cropErosionDilation As Integer)
            MyBase.New(SETNAME, cytoSetType.SuccessFullyCroppedImages, Drawing.Color.Blue, datafile, listId, vis)
            _cropMarginBase      = cropMarginBase
            _cropMarginFactor    = cropMarginFactor
            _cropBgThreshold     = cropBgThreshold
            _cropErosionDilation = cropErosionDilation

            RecalculateParticleIndices()
        End Sub

        Public Sub New(other As SuccessFullyCroppedImagesSet)
            MyBase.New(other.Name, other.Type, other.ColorOfSet, other._datafile, other.ListID, other.Visible)
            _cropMarginBase      = other._cropMarginBase
            _cropMarginFactor    = other._cropMarginFactor
            _cropBgThreshold     = other._cropBgThreshold
            _cropErosionDilation = other._cropErosionDilation

            RecalculateParticleIndices()
        End Sub


        ''' <summary>
        ''' TEST CLASSES, Can behave strange if deserialized with different values as current ones..
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parentNode"></param>
        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            document.AppendChildElement(parentNode, "AutoCropMarginBase",   _cropMarginBase)
            document.AppendChildElement(parentNode, "AutoCropMarginFactor", _cropMarginFactor)
            document.AppendChildElement(parentNode, "BackgroundThreshold",  _cropBgThreshold)
            document.AppendChildElement(parentNode, "ErosionDilation",      _cropErosionDilation)
        End Sub

        ''' <summary>
        ''' TEST CLASSES, Can behave strange if deserialized with different values as current ones..
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parentNode"></param>
        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document,parentNode)

            _cropMarginBase      = parentNode.ReadChildElementAsInteger("AutoCropMarginBase")
            _cropMarginFactor    = parentNode.ReadChildElementAsDouble("AutoCropMarginFactor")
            _cropBgThreshold     = parentNode.ReadChildElementAsInteger("BackgroundThreshold")
            _cropErosionDilation = parentNode.ReadChildElementAsInteger("ErosionDilation")
        End Sub


        Public Overrides Property Datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get
            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value

                RecalculateParticleIndices()
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

        ''' <summary>
        ''' Loop through all particles to collect the successfully cropped ones. 
        ''' How to have the timer do this, and update the GUI...
        ''' </summary>
        Public Overrides Sub RecalculateParticleIndices()
            If _calcFinished OrElse _datafile Is Nothing Then
                Return 'No changes
            End If

            Dim foundCropNotReady As Boolean = False
            Dim allParticles = Datafile.SplittedParticles
            Dim IIFparticleIndices = Datafile.SplittedParticlesWithImagesIndices
            Dim indices = New List(Of Integer)

            For i = 0 To IIFparticleIndices.Length - 1
                With DirectCast(allParticles(IIFparticleIndices(i)), ImagedParticle)
                    foundCropNotReady = foundCropNotReady OrElse .ImageHandling.CropResult = CytoSense.Data.ParticleHandling.CytoImage.CropResultEnum.AwaitingCrop

                    If .ImageHandling.CropResult = CytoSense.Data.ParticleHandling.CytoImage.CropResultEnum.CropOK Then
                        indices.Add(IIFparticleIndices(i))
                    End If
                End With
            Next

            _ParticleIndices = indices.ToArray()
            _calcFinished = Not foundCropNotReady

            If Not _calcFinished Then
                If _recalcTimer Is Nothing Then
                    _recalcTimer = New System.Threading.Timer(Sub() Me.RecalculateParticleIndices())
                End If
                _recalcTimer.Change(10000, System.Threading.Timeout.Infinite)
            Else
                _recalcTimer = Nothing
            End If

            _Invalid = True ' for update UI (once it enters workFile.RecalculateParticleIndices)
        End Sub

        Public Overrides Function TestSingleParticle(p As Particle) As Boolean
            If p.hasImage Then 'Check if crop success full (NOTE: IF not cropped, then force cropping to get the result
                Dim imgP As ImagedParticle = DirectCast(p, ImagedParticle)
                Dim cropResult = imgP.ImageHandling.CropResult
                If cropResult = CytoSense.Data.ParticleHandling.CytoImage.CropResultEnum.AwaitingCrop Then
                    Dim tmpImg = imgP.ImageHandling.GetCroppedImage(_cropMarginBase, _cropMarginFactor, _cropBgThreshold,  _cropErosionDilation, False, False)
                    cropResult = imgP.ImageHandling.CropResult
                End If
                Return (cropResult = CytoSense.Data.ParticleHandling.CytoImage.CropResultEnum.CropOK)
            Else
                Return False
            End If
        End Function

        Public Overrides Function Clone() As CytoSet
            Return New SuccessFullyCroppedImagesSet(Me)
        End Function

    End Class
End Namespace