

<Serializable>
Public Class InternalMultiSamplerSettingsPortInfo
    Public Sub New( portNr As Integer, lbl As String, primeTime As TimeSpan, cleanTime As TimeSpan )
        _portNr    = portNr
        _label     = lbl
        _primeTime = primeTime
        _cleanTime = cleanTime
    End Sub

    Public ReadOnly Property Port As Integer
        Get
            Return _portNr
        End Get
    End Property


    Public Property Label As String
        Get
            Return _label
        End Get
        Set(value As String)
            _label = value
        End Set
    End Property

    Public Property PrimeTime As TimeSpan
        Get
            Return _primeTime
        End Get
        Set(value As TimeSpan)
            _primeTime = value
        End Set
    End Property

    Public Property CleanTime As TimeSpan
        Get
            Return _cleanTime
        End Get
        Set(value As TimeSpan)
            _cleanTime = value
        End Set
    End Property

    Private ReadOnly _portNr As Integer
    Private _label As String
    Private _primeTime As TimeSpan
    Private _cleanTime As TimeSpan
End Class


''' <summary>
''' Stores the settings used for the new internal Multi sampler to be use in INOGS.
''' Also used for the old version, but then it just stores the _backFlushDuringClean.
''' Option.
''' </summary>
<Serializable>
Public Class NewMultiSamplerSettings
    Public Sub New( ppIo As Integer, cpIo As Integer, backFlushDuringClean As Boolean , pInfo As List(Of InternalMultiSamplerSettingsPortInfo))
        _configured           = True
        _primePumpIo          = ppIo
        _cleanPumpIo          = cpIo
        _backFlushDuringClean = backFlushDuringClean
        _portInfo             = pInfo
    End Sub

    Public Sub New( )
        _configured           = False
        _primePumpIo          = -1
        _cleanPumpIo          = -1
        _backFlushDuringClean = False
        _portInfo             = Nothing
    End Sub

    Public ReadOnly Property PrimePumpIo As Integer
        Get
            Return _primePumpIo
        End Get
    End Property

    Public ReadOnly Property CleanPumpIo As Integer
        Get
            Return _cleanPumpIo
        End Get
    End Property

    Public Property BackFlushDuringClean As Boolean
        Get
            Return _backFlushDuringClean
        End Get
        Set(value As Boolean)
            _backFlushDuringClean = value
        End Set
    End Property
    

    Public ReadOnly Property PortInfo As List(Of InternalMultiSamplerSettingsPortInfo)
        Get
            Return _portInfo
        End Get
    End Property

    Public ReadOnly Property Configured As Boolean
        Get
            Return _configured
        End Get
    End Property

    Private ReadOnly _configured As Boolean = False
    Private ReadOnly _primePumpIo  As Integer  ' Number of IO pin on Ruuds USB print where the pump is connected.
    Private ReadOnly _cleanPumpIo  As Integer  ' Number of IO pin on Ruuds USB print where the pump is connected.
    Private _backFlushDuringClean As Boolean
    Private ReadOnly _portInfo As New List(Of InternalMultiSamplerSettingsPortInfo)()
End Class
