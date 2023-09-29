
Namespace CytoSettings.PIC

    <Serializable()> Public Class CytoSelector_settings
        Public Sub New()
            _isPresent = False
            _cytoTrays = New CytoSelectorTray(){}
        End Sub


        Public Sub New(trays As CytoSense.CytoSelectorTray())
            _isPresent = True
            _cytoTrays = trays
        End Sub

        Public Property NumberOfTrays As Integer
            Get
                Return _cytoTrays.Length
            End Get
            Set(value As Integer)
                Dim oldLength = _cytoTrays.Length
                ReDim Preserve _cytoTrays(value-1)
                ' If the array was enlarged we need to create default entries.
                While oldLength < value
                    Dim statNum = oldLength+1
                    _cytoTrays(oldLength) = New CytoSelectorTray("Station "+ statNum.ToString(), CByte(statNum), 0, 5.0* statNum, 5.0, 5.0*statNum)
                    oldLength += 1
                End While
            End Set
        End Property

        Public Property Trays As CytoSelectorTray()
            Get
                Return _cytoTrays
            End Get
            Set(value As CytoSelectorTray())
                _cytoTrays = value
            End Set
        End Property

        ''' <summary>
        ''' Calculates the needed flush time for a specific valve
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FlushTime(tray_id As Integer) As Double
            Get
                Return _cytoTrays(tray_id).Flushtime
            End Get
        End Property

        ''' <summary>
        ''' Special setting to enable reversing of the polarity. This solves most problems.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum MotorPolarity_t As Integer
            UNDEFINED = 0
            NORMAL = 1
            REVERSED = -1
        End Enum

        Public Property MotorPolarity As MotorPolarity_t
            Get
                If _MotorPolarity = MotorPolarity_t.UNDEFINED Then
                    Return MotorPolarity_t.NORMAL
                End If
                Return _MotorPolarity
            End Get
            Set(value As MotorPolarity_t)
                _MotorPolarity = value
            End Set
        End Property

        ''' <summary>
        ''' Indicates if a CytoSelector/Multi Point Selector is available with the system. This NOT the same
        ''' as connected.  A Multi Point Selector can be present at the site but not connected.
        ''' </summary>
        ''' <returns></returns>
        Public Property IsPresent As Boolean
            Get
                Return _isPresent
            End Get
            Set(value As Boolean)
                _isPresent = value
            End Set
        End Property


        <Obsolete("Not really obsolete, but a dirty hack that you should avoid using")>
        <NonSerialized>
        Public NeedsStoring As Boolean = False


        Private _cytoTrays As CytoSelectorTray()
        Private _MotorPolarity As MotorPolarity_t
        Private _isPresent As Boolean
    End Class

End Namespace