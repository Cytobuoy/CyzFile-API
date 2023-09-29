<Serializable()> Public Class CytoSelectorTray
    Private _name As String
    Private _hasCleaning As Boolean
    Private _pv_address_sample As Byte
    Private _pv_address_clean As Byte
    Private _enabled As Boolean
    Private _flushtime As Double
    Private _cleanTime As Double
    Private _soaptime As Double
    Private _isConnected As Boolean = True' Set to indicate if the tray was actually connected or not. If not connected we cannot use it.

    Public Sub New(name As String, pv_address_sample As Byte, pv_address_clean As Byte, flushtime As Double, cleantime As Double, soaptime As Double)
        _hasCleaning = True
        _name = name
        _pv_address_clean = pv_address_clean
        _pv_address_sample = pv_address_sample
        _flushtime = flushtime
        _cleanTime = cleantime
        _soaptime = soaptime
        _enabled = True
    End Sub

    Public Sub New(name As String, pv_address_beads As Byte, flushtime As Double, cleantime As Double, soaptime As Double)
        _hasCleaning = False
        _name = name
        _pv_address_sample = pv_address_beads
        _flushtime = flushtime
        _cleanTime = cleantime
        _soaptime = soaptime
        _enabled = True
    End Sub

    Public Property Name As String
        Get
            Return _name
        End Get
        Set
            _name = value
        End Set
    End Property

    Public Property HasCleaning As Boolean
        Get
            Return _hasCleaning
        End Get
        Set(value As Boolean)
            _hasCleaning = value
        End Set
    End Property

    Public Property AddressSample As Byte
        Get
            Return _pv_address_sample
        End Get
        Set(value As Byte)
            _pv_address_sample = value
        End Set
    End Property
    Public ReadOnly Property AddressClean As Byte
        Get
            Return _pv_address_clean
        End Get
    End Property

    Public Property Enabled As Boolean
        Get
            Return _enabled
        End Get
        Set(value As Boolean)
            _enabled = value
        End Set
    End Property

    Public Property Flushtime As Double
        Get
            Return _flushtime
        End Get
        Set(ByVal value As Double)
            _flushtime = value
        End Set
    End Property

    Public Property CleanTime As Double
        Get
            Return _cleanTime
        End Get
        Set(ByVal value As Double)
            _cleanTime = value
        End Set
    End Property

    Public Property SoapTime As Double
        Get
            Return _soaptime
        End Get
        Set(ByVal value As Double)
            _soaptime = value
        End Set
    End Property

    ''' <summary>
    ''' Is the sample station actually connected or not.  The default is to assume it is, and for modern versions
    ''' we scan on startup to see which is connected and which is not.
    ''' </summary>
    ''' <returns></returns>
    Public Property IsConnected As Boolean
        Get
            Return _isConnected
        End Get
        Set(value As Boolean)
            _isConnected = value
        End Set
    End Property

    ''' <summary>
    ''' Simple compare address function, used for sorting in a consistent manner.
    ''' </summary>
    ''' <param name="lhs"></param>
    ''' <param name="rhs"></param>
    ''' <returns></returns>
    Public Shared Function CompareByAddress( lhs As CytoSelectorTray, rhs As CytoSelectorTray) As integer
        Return lhs.AddressSample.CompareTo(rhs.AddressSample)
    End Function

End Class
