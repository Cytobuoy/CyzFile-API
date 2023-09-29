
Namespace CytoSettings.PIC

    <Serializable()> Public Class MultiSampler_settings

        Public Sub New(PinchValves As PinchValve(), PumpSea As Pump, PumpSoap As Pump)
            _pumpSea = PumpSea
            _pumpSoap = PumpSoap
            _PinchValves = PinchValves
        End Sub


        Private _pumpSea As Pump
        Public Property PumpSea As Pump
            Get
                Return _pumpSea
            End Get
            Set(value As Pump)
                _pumpSea = value
            End Set
        End Property

        Private _pumpSoap As Pump
        Public Property PumpSoap As Pump
            Get
                Return _pumpSoap
            End Get
            Set(value As Pump)
                _pumpSoap = value
            End Set
        End Property

        Private _PinchValves() As PinchValve
        Public ReadOnly Property PinchValves As PinchValve()
            Get
                Return _PinchValves
            End Get
        End Property

        ''' <summary>
        ''' Calculates the needed clean time for a specific valve
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CleanTime(pinchvalve_id As Integer) As Integer
            Get
                Dim pv As PinchValve = _PinchValves(pinchvalve_id)
                Dim res As Integer = CInt( (0.25 * Math.PI * (pv.TubingDiameter / 1000) ^ 2 * pv.TubingLength + 0.25 * Math.PI * (PumpSoap.TubingDiameter / 1000) ^ 2 * PumpSoap.TubingLength) / (PumpSoap.PumpingRate * 1000 * 1000))
                Return res
            End Get
        End Property

        ''' <summary>
        ''' Calculates the needed flush time for a specific valve
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property FlushTime(pinchvalve_id As Integer) As Integer
            Get
                Dim pv As PinchValve = _PinchValves(pinchvalve_id)
                Dim res As Integer = CInt( (0.25 * Math.PI * (pv.TubingDiameter / 1000) ^ 2 * pv.TubingLength + 0.25 * Math.PI * (PumpSea.TubingDiameter / 1000) ^ 2 * PumpSea.TubingLength) / (PumpSea.PumpingRate * 1000 * 1000) )
                Return res
            End Get
        End Property

        <Serializable()> Public Class GPPIO

            Public Sub New(BitAddress As Integer, Name As String)
                _BitAddress = BitAddress
                _name = Name
            End Sub


            Private _name As String
            Public ReadOnly Property Name As String
                Get
                    Return _name
                End Get
            End Property

            Private _BitAddress As Integer
            ''' <summary>
            ''' Bit address of the IO pin on the PC8574
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property BitAddress As Integer
                Get
                    Return _BitAddress
                End Get
            End Property

            Private _bitstate As Boolean
            Public ReadOnly Property BitState As Boolean
                Get
                    Return _bitstate
                End Get
            End Property
            Protected Friend Sub setBitState(b As Boolean)
                _bitstate = b
            End Sub

        End Class

        <Serializable()> Public Class PinchValve
            Inherits GPPIO


            Public Event PositionChanged()

            Public Sub New(BitAddress As Integer, TubingLength As Double, TubingDiameter As Double, StandardPosition As NormallyPosition, Name As String)
                MyBase.New(BitAddress, Name)
                _TubingLength = TubingLength
                _TubingDiameter = TubingDiameter
                _StandardPosition = If(StandardPosition = NormallyPosition.NormallyClosed, Position.Closed, Position.Open)
                _CurrentPosition = If(StandardPosition = NormallyPosition.NormallyClosed, Position.Closed, Position.Open)
                setBitState(False)
            End Sub

            Private _TubingLength As Double
            ''' <summary>
            ''' Length of the line connected to this pinchvalve in m
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property TubingLength As Double
                Get
                    Return _TubingLength
                End Get
                Set(value As Double)
                    _TubingLength = value
                End Set
            End Property

            Private _TubingDiameter As Double
            ''' <summary>
            ''' Diameter of the tubing connected to this pinchvalve in  mm
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property TubingDiameter As Double
                Get
                    Return _TubingDiameter
                End Get
                Set(value As Double)
                    _TubingDiameter = value
                End Set
            End Property


            Private _CurrentPosition As Position
            ''' <summary>
            ''' The current position of this pinchvalve (open/closed)
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property CurrentPosition As Position
                Get
                    Return _CurrentPosition
                End Get
                Set(value As Position)
                    _CurrentPosition = value
                    If StandardPosition = NormallyPosition.NormallyClosed Then
                        setBitState(If(value=Position.Open,True,False))
                    Else
                        If value = Position.Open Then
                            setBitState(False)
                        Else
                            setBitState(True)
                        End If
                    End If

                End Set
            End Property

            Private _StandardPosition As Position
            ''' <summary>
            '''  Position when not powered
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property StandardPosition As NormallyPosition
                Get
                    Return If(_StandardPosition=Position.Open,NormallyPosition.NormallyOpen,NormallyPosition.NormallyClosed)
                End Get
            End Property

            <Serializable()> Public Enum Position
                Closed
                Open
            End Enum
            <Serializable()> Public Enum NormallyPosition
                NormallyClosed
                NormallyOpen
            End Enum


            Public Function clone() As PinchValve
                Return New PinchValve(BitAddress, _TubingLength, _TubingDiameter, StandardPosition, Name)
            End Function

        End Class

        <Serializable()> Public Class Pump
            Inherits GPPIO
            Public Sub New(BitAdrress As Integer, PumpingRate As Double, TubingLength As Double, TubingDiameter As Double, Name As String)
                MyBase.New(BitAdrress, Name)
                _PumpingRate = PumpingRate
                _TubingLength = TubingLength
                _TubingDiameter = TubingDiameter
                setBitState(False)
            End Sub


            Private _CurrentPumpState As PumpState
            ''' <summary>
            ''' State of the pump. True = pump is pumping.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property CurrentPumpState As PumpState
                Get
                    Return _CurrentPumpState
                End Get
                Set(value As PumpState)
                    _CurrentPumpState = value
                    setBitState(If(value=PumpState.Pumping,True, False))
                End Set
            End Property

            <Serializable()> Enum PumpState
                Not_Pumping
                Pumping
            End Enum

            Private _PumpingRate As Double
            ''' <summary>
            ''' Speed of the pump.
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property PumpingRate As Double
                Get
                    Return (_PumpingRate)
                End Get
                Set(value As Double)
                    _PumpingRate = value
                End Set
            End Property

            Private _TubingLength As Double
            ''' <summary>
            ''' Length of the line connected to this pump
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property TubingLength As Double
                Get
                    Return _TubingLength
                End Get
                Set(value As Double)
                    _TubingLength = value
                End Set
            End Property

            Private _TubingDiameter As Double
            ''' <summary>
            ''' Diameter of the tubing connected to this pump
            ''' </summary>
            ''' <value></value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Property TubingDiameter As Double
                Get
                    Return _TubingDiameter
                End Get
                Set(value As Double)
                    _TubingDiameter = value
                End Set
            End Property

        End Class


    End Class


End Namespace