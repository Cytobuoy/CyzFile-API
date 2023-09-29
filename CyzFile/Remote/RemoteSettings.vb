Imports System.Runtime.Serialization

Namespace Remote

    <Serializable()>
    Public Enum TransferProtocol
        Invalid    = 0
        SFTP       = 1
        SMB        = 2
        CytoServer = 3
    End Enum

    <Serializable()> Public Class RemoteSettings
        Private _Port As Integer = 6662

        Public Sub New()
            Mode = Remote.Mode.None
        End Sub

        Public Sub New(mode As Remote.Mode)
            Me.Mode = mode
        End Sub

        Public Sub New( other As RemoteSettings)
            _Port                    = other._Port
            _ServerAddress           = other._ServerAddress
            _mode                    = other._mode
            _bandwidth               = other._bandwidth
            _defaultDownloadLocation = other._defaultDownloadLocation
            SerNr                    = other.SerNr
            _protocol                = other._protocol
            _destinationFolder       = other._destinationFolder
            _shareName               = other._shareName
        End Sub

        Public Property Port() As Integer
            Get
                Return _Port
            End Get
            Set(value As Integer)
                _Port = value
            End Set
        End Property

        Private _ServerAddress As String
        Public Property ServerAddress() As String
            Get
                Return _ServerAddress
            End Get
            Set(ByVal value As String)
                _ServerAddress = value
            End Set
        End Property

        Private _mode As Mode = Remote.Mode.None
        Public Property Mode As Remote.Mode
            Get
                Return _mode
            End Get
            Set(value As Remote.Mode)
                _mode = value
            End Set
        End Property

        Private _bandwidth As Integer = 1000
        <Obsolete()>Public Property BandWidth As Integer
            Get
                Return _bandwidth
            End Get
            Set(value As Integer)
                _bandwidth = value
            End Set
        End Property

        Private _destinationFolder As String
        Public Property DestinationFolder As String
            Get
                Return _destinationFolder
            End Get
            Set(value As String)
                _destinationFolder = value
            End Set
        End Property

        Private _shareName As String
        Public Property ShareName As String
            Get
                Return _shareName
            End Get
            Set(value As String)
                _shareName = value
            End Set
        End Property

        Private _defaultDownloadLocation As String
        Public Property DefaultDownloadLocation As String
            Get
                If _defaultDownloadLocation = "" Then
                    Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\My CytoSense\Datafiles\Cloud\" ' Probably unused, and should be removed
                Else
                    Return _defaultDownloadLocation
                End If
            End Get
            Set(value As String)
                _defaultDownloadLocation = value
            End Set
        End Property

        ''' <summary>
        ''' The SerNr of the instrument (CS-20xx), used as identifier to which files access is provided remotely
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SerNr As String

        Private _protocol As TransferProtocol
        Public Property Protocol As TransferProtocol
            Get
                Return _protocol
            End Get
            Set(value As TransferProtocol)
                _protocol = value
            End Set
        End Property


        ''' <summary>
        ''' When de-serializing a stored class, if the _protocol is still set to invalid after loading from
        ''' disk, it means that it was an old settings object that was stored before the protocol member was
        ''' added.  Before this member was added there was only one option, namely TransferProtocol.CytoServer,
        ''' so in that case we set it.  If it has another value, we leave it alone.
        ''' </summary>
        ''' <param name="context"></param>
        <OnDeserialized()>
        Private Sub OnDeserialized(context As StreamingContext)
            If _protocol = TransferProtocol.Invalid Then
                _protocol = TransferProtocol.CytoServer  ' It was an old structure, so set the correct protocol
            End If ' Else a value was stored, so leave it alone.
        End Sub

    End Class

    <Flags>
    Public Enum Mode
        None = 0
        Reserved = 1
        SendDataFileToServer = 2
        SendExportToServer = 4
    End Enum
End Namespace
