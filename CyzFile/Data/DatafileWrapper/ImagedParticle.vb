Imports CytoSense.Data.SegmentedData

Namespace Data.ParticleHandling


    <Serializable> Public Class ImagedParticle
        Inherits Particle

        Private _image As CytoImage

        Public Sub New(p As CytoSense.Data.ParticleHandling.Particle, dspparticle As DSP.DSPParticle)
            MyBase.New(p.ChannelData_Hardware, p.ID, p.CytoSettings, p.MeasurementSettings)
            _image = New CytoImage(CytoSettings, dspparticle.ImageStream)
        End Sub

        Public Sub New(ByVal p As CytoSense.Data.ParticleHandling.Particle, imagestream As IO.MemoryStream)
            MyBase.New(p.ChannelData_Hardware, p.ID, p.CytoSettings, p.MeasurementSettings)
            _image = New CytoImage(CytoSettings, imagestream)
        End Sub
        Public Sub New(ByVal p As CytoSense.Data.ParticleHandling.Particle, image As CytoImage)
            MyBase.New(p.ChannelData_Hardware, p.ID, p.CytoSettings, p.MeasurementSettings)
            _image = image
        End Sub
        Public Sub New(p As CytoSense.Data.ParticleHandling.Particle, image As OpenCvSharp.Mat, imagestream As IO.MemoryStream, cropRect As OpenCvSharp.Rect)
            MyBase.New(p.ChannelData_Hardware, p.ID, p.CytoSettings, p.MeasurementSettings)
            _image = New CytoImage(CytoSettings, image, imagestream, cropRect)
        End Sub
        Public Sub New(p As RawIIFParticle, cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, measurement As CytoSense.MeasurementSettings.Measurement, start As DateTime)
            MyBase.New(p, cytoSettings, measurement, start)
            _image = New CytoImage(cytoSettings, p.ImageStream, p.CropRect)
        End Sub

        

        'Public ReadOnly Property Image As Image
        '    Get
        '        Return _image.Image
        '    End Get
        'End Property

        ''' <summary>
        ''' Gives access to the images and related functions and properties
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ImageHandling As CytoImage
            Get
                Return _image
            End Get
        End Property

        Public Overrides ReadOnly Property hasImage As Boolean
            Get
                Return True
            End Get
        End Property
        Public ReadOnly Property IsProcessed As Boolean
            Get
                Return _image.isProcessed
            End Get
        End Property
        Public ReadOnly Property CroppedResult As CytoImage.CropResultEnum
            Get
                Return _image.CropResult
            End Get
        End Property
    End Class
End Namespace