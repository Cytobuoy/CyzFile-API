Imports MathNet.Numerics
Imports System.Runtime.Serialization

Namespace Calibration


    Namespace SamplePump

        ''' <summary>
        ''' Contains the DC sample pump calibration. Can only be used when a hall sensor and a DC sample pump is available
        ''' </summary>
        ''' <remarks>
        ''' For the XR I am rearranging the sample pump code. The old version had specific byte values that
        ''' were in different order and had different meaning depending on what type of speed and controller
        ''' there is.
        ''' 
        ''' In the new interface there is no direction flag, instead we have speeds running from:
        ''' Max_Out .. FullStop .. Max In
        ''' Where at the moment (for current sample pumps) will be
        ''' 
        '''   -255 .. 0 .. 255
        '''   
        ''' For very old ones, the range is a bit smaller.
        ''' 
        ''' Calibration is only available for 0..255 (inwards), at the moment outwards is not calibrated.
        ''' 
        ''' The object stores a byteToSpeed conversion table, we keep that, but the original had some interesting
        ''' order of bytes, e.g. in some slow inwards was 255, where fastest inwards was 0.
        ''' 
        ''' I want to harmonize this, and all the different ways of actually sending these speed commands to the
        ''' sample pump will be handled by the sample pump object.  They will actually convert it if required.
        ''' So we keep this calibration array simple, from 0 to In_max.
        ''' 
        ''' This means if we load an older calibration we need to rearrange the values in the table.
        ''' Lets see if we have enough information for that on loading.  Perhaps we need a special
        ''' function call to be able to do this correctly.
        ''' 
        ''' At least we need a flag to indicate that Rearranging is required or not.
        ''' 
        ''' This will be named:  
        ''' 
        '''     _haveIntegerSpeeds
        ''' 
        ''' </remarks>
        <Serializable()> Public Class DCSamplePump
            Dim _volume1Round As Double
            Dim _calibrationDate As DateTime

            Dim _byteToSpeed(255) As Double


            Dim _TimeCalibrationPoints() As DCSamplePumpHallMeasurements
            Private _numberOfRounds As Integer = -1 ' to be able check if calibration was performed on enough rounds

            Dim _isReallyCalibrated As Boolean
            Private _numMagnets As Integer = -1 ' Number of magnets present in the sample pump. Only available in very new instruments.  <0 means not available.
            ' Speeds for this sample pump, where _outMaxSpeed < _stopSpeed < _inMaxSpeed
            ' For now lookup in table is done using:
            '  _byteToSpeed[speed-_stopSpeed]
            ' 
            Private _outMaxSpeed As Integer  ' Maximum outward pumping speed
            Private _stopSpeed As Integer
            Private _inMaxSpeed As Integer   ' Maximum in pumping speed
            Public ReadOnly Property IsReallyCalibrated As Boolean
                Get
                    Return _isReallyCalibrated
                End Get
            End Property
            Public ReadOnly Property Volume1Round As Double
                Get
                    Return _volume1Round
                End Get
            End Property

            Public ReadOnly Property NumMagnets As Integer
            Get
                    Return _numMagnets
            End Get
            End Property

            Public ReadOnly Property NumRounds As Integer
            Get
                    Return _numberOfRounds
            End Get
            End Property

            Public ReadOnly Property MaxSpeedSetting As Integer
            Get
                    Return _inMaxSpeed
            End Get
            End Property

            ''' <summary>
            ''' Return a default calibrated sample pump device. For newer devices we may have
            ''' the number of magnets available, if you do you can pass it, else just set it to -1
            ''' </summary>
            ''' <param name="numMagnets"></param>
            ''' <returns></returns>
            Public Shared Function DefaultCalibration() As DCSamplePump
                Return New DCSamplePump(0.1, New Date(1990, 1, 1), 0.00007, -1, -255,0,255)
            End Function

            Public Shared Function DefaultCalibration(numMagnets As Integer, maxOut As Integer, stopSpeed As Integer, maxIn As Integer) As DCSamplePump
                Return New DCSamplePump( 100/numMagnets, New Date(1990, 1, 1), 0.00007, numMagnets, maxOut, stopSpeed, maxIn)
            End Function

            ''' <summary>
            ''' Estimate calibration. Only use if no real calibration is available.
            ''' </summary>
            ''' <param name="Volume1Round"></param>
            ''' <param name="d"></param>
            ''' <param name="constant"></param>
            ''' <remarks></remarks>
            Private Sub New(ByVal Volume1Round As Double, ByVal d As DateTime, ByVal constant As Double, numMagnets As Integer, maxOut As Integer, stopSpeed As Integer, maxIn As Integer)
                For i = 0 To _byteToSpeed.Length - 1
                    _byteToSpeed(i) = i * constant * 1000
                Next
                _constante = constant
                _calibrationDate = d
                _isReallyCalibrated = False
                _TimeCalibrationPoints = Nothing
                _volume1Round = Volume1Round
                _numMagnets = numMagnets 
                _outMaxSpeed = maxOut
                _stopSpeed   = stopSpeed
                _inMaxSpeed  = maxIn
            End Sub
            <Obsolete>
            Dim _calibratedConstants() As Double

            ''' <summary>
            ''' New calibration constructor where we pass in the calibrated speed array, and the actual calibration object
            ''' only stores and does not do the calibration itself.  The actual calculations are handled inside the
            ''' calibration wizard. (Not sure this is is pretty)
            ''' </summary>
            ''' <param name="numberofRounds"></param>
            ''' <param name="measuredVolume"></param>
            ''' <param name="d"></param>
            ''' <param name="calibrationPoints"></param>
            Public Sub New(ByVal numberofRounds As Integer, ByVal measuredVolume As Double, ByVal d As DateTime, ByVal calibrationPoints As List(Of DCSamplePumpHallMeasurements), speeds() As Double, numMagnets As Integer, maxOut As Integer, stopSpeed As Integer, maxIn As Integer)
                _numberOfRounds = numberofRounds
                _volume1Round = measuredVolume / numberofRounds 'uL

                _calibrationDate = d
                _TimeCalibrationPoints = calibrationPoints.ToArray()

                For i = 0 To _byteToSpeed.Length - 1
                    _byteToSpeed(i) = speeds(i)
                Next
                _numMagnets = numMagnets

                _outMaxSpeed = maxOut
                _stopSpeed   = stopSpeed
                _inMaxSpeed  = maxIn

                _isReallyCalibrated = True
            End Sub


            Public ReadOnly Property TimeCalibrationPoints As DCSamplePumpHallMeasurements()
                Get
                    Return _TimeCalibrationPoints
                End Get
            End Property

            Public Function getTimeCalibration() As Double()()
                Dim res(_TimeCalibrationPoints.Length - 1)() As Double
                For i = 0 To _TimeCalibrationPoints.Length - 1
                    ReDim res(i)(1)
                    res(i) = {_TimeCalibrationPoints(i).speedbyte, _TimeCalibrationPoints(i).timeOneRound}
                Next
                Return res
            End Function

            Private _constante As Double

            ''' <summary>
            ''' Calculates the round time of the sample pump with the provided speed in milliseconds
            ''' </summary>
            ''' <param name="speed"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function getExpectedRoundTime(speed As Integer) As Double
                Dim res As Double = (_volume1Round / getFlowSpeed(speed)) * 1000
                If Double.IsNaN(res) Or Double.IsInfinity(res) Then 'during recalibration temporarily the _constante can be 0, causing problems in the samplepump speed checker thread
                    res = 5000
                End If
                Return res
            End Function


            Public ReadOnly Property CalibrationDate As DateTime
                Get
                    Return _calibrationDate
                End Get
            End Property

            Public ReadOnly Property Constante As Double
                Get
                    Return _constante
                End Get
            End Property


            Public Function GetFlowSpeed(speed As Integer) As Double
                Debug.Assert(_stopSpeed <= speed AndAlso speed <= _inMaxSpeed)
                Dim idx As Integer = (speed - _stopSpeed)
                Return _byteToSpeed(idx)
            End Function

            ''' <summary>
            ''' Make sure the num magnets is initialized to -1 so we can recognize
            ''' it after we load the calibration from file.
            ''' </summary>
            ''' <param name="sc"></param>
            <OnDeserializing>
            Private Sub OnDeserializing( sc As StreamingContext)
                _numMagnets = -1
                _outMaxSpeed = Integer.MinValue  ' Maximum outward pumping speed
                _stopSpeed   = Integer.MinValue
                _inMaxSpeed  = Integer.MinValue
            End Sub

            ''' <summary>
            ''' After deserializing we need to check if this is an old or a new one.
            ''' If the in/out/stop speeds were not set. We need to add them.
            ''' This object is only used for he DC sample pump.  And it has a
            ''' range -255 .. 0 .. 255
            ''' </summary>
            ''' <param name="sc"></param>
            <OnDeserialized>
            Private Sub OnDeserialized( sc As StreamingContext)
                If _outMaxSpeed = Integer.MinValue  Then ' Not initialized.
                    _outMaxSpeed = -255
                    _stopSpeed   = 0
                    _inMaxSpeed  = 255
                End If
            End Sub


        End Class

        <Serializable()> Public Structure DCSamplePumpHallMeasurements
            Dim speedbyte As Byte
            Dim timeOneRound As Double

            Sub New(ByVal speedByte As Byte, ByVal timeOneRound As Double)
                Me.speedbyte = speedByte
                Me.timeOneRound = timeOneRound
            End Sub

        End Structure

        ''' <summary>
        ''' Contains the old stepper motor sample pump calibration. Cannot be used with a DC sample pump. (even if it has no hall sensor)
        ''' </summary>
        ''' <remarks></remarks>
        <Serializable()> Public Class SamplePumpCalibrationData
            Dim _calibrationpoints()() As CalibrationPoint
            Dim add_index() As Int16
            Dim _speedIndex As Int16
            Dim _calibrationDate As Date
            Dim _seringeSize As Double

            Dim _depriciatedConstant As Double = 0


            Dim _ratio(255) As Double

            Public Sub New(ByVal numberOfSpeeds As Int16, ByVal calibrationPointsPerSpeed As Int16, ByVal seringeSize As Double)

                ReDim _calibrationpoints(numberOfSpeeds - 1)
                ReDim add_index(numberOfSpeeds - 1)
                For i = 0 To numberOfSpeeds - 1
                    ReDim _calibrationpoints(i)(calibrationPointsPerSpeed - 1)
                Next
                _speedIndex = 0
                _seringeSize = seringeSize
                _calibrationDate = Now
            End Sub

            ''' <summary>
            ''' Backwards compatabilty to not yet calibrated samplepumps
            ''' </summary>
            ''' <param name="SamplePumpConstant"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal SamplePumpConstant As Double)
                _depriciatedConstant = SamplePumpConstant
            End Sub


            Public ReadOnly Property isMultiSpeedCalibrated As Boolean
                Get
                    If _depriciatedConstant = 0 Then
                        Return True
                    Else
                        Return False
                    End If
                End Get
            End Property

            ''' <summary>
            ''' To plot dots of the calibration points as determined during the wizard. Only to be used after calibration, in support graphs
            ''' </summary>
            ''' <param name="speedIndex"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function getOriginalCalibrationPoints(ByVal speedIndex As Int16) As SingleCalibrationPoint

                Dim speedList(NumberOfSpeeds - 1) As Double
                Dim Ratios(NumberOfSpeeds - 1) As Double
                For i = 0 To NumberOfSpeeds - 1
                    speedList(i) = _calibrationpoints(i)(0).Speed
                    Ratios(i) = getOriginalRatio(CShort(i))
                Next

                Dim res As New SingleCalibrationPoint(speedList(speedIndex), Ratios(speedIndex))
                Return res

            End Function


            ''' <summary>
            ''' Specifies the number of different speeds on which the sample pump is calibrated / calibrating
            ''' </summary>
            Public ReadOnly Property NumberOfSpeeds As Int16
                Get
                    Return CShort(_calibrationpoints.Length)
                End Get
            End Property

            Public ReadOnly Property CalbrationDate As Date
                Get
                    Return _calibrationDate
                End Get
            End Property

            Public Function CalibrateNextSpeed() As CalibrationSpeedSelectionStatus
                _speedIndex = CShort(_speedIndex + 1)
                If _speedIndex < _calibrationpoints.Length Then
                    Return CalibrationSpeedSelectionStatus.OK
                Else
                    Return CalibrationSpeedSelectionStatus.Finished
                End If
            End Function
            Public ReadOnly Property currentlyCalibratingSpeedIndex As Short
                Get
                    Return _speedIndex
                End Get
            End Property

            Public Sub add(ByVal CalibrationPoint As CalibrationPoint)
                _calibrationpoints(_speedIndex)(add_index(_speedIndex)) = CalibrationPoint
                add_index(_speedIndex) = CShort(add_index(_speedIndex) + 1)
            End Sub

            ''' <summary>
            ''' Recalculates a best fit on available calibration points. Needs to be done after calibration cycle was finished
            ''' </summary>
            ''' <remarks>Only used during calibration</remarks>
            Public Sub doCalculations()
                _depriciatedConstant = 0        'get rid of the old one speed constant!
                'if old constant isn't made 0-> getFlowSpeed will use old constant in stead of multi speed calibration

                Dim speedList(NumberOfSpeeds - 1) As Double
                Dim Ratios(NumberOfSpeeds - 1) As Double
                For i = 0 To NumberOfSpeeds - 1
                    speedList(i) = _calibrationpoints(i)(0).Speed
                    Ratios(i) = getOriginalRatio(CShort(i)) ^ -1
                Next


                ReDim _ratio(255)

                Dim inpo = Interpolation.LinearSpline.Interpolate(speedList, Ratios)
                For i = 0 To _ratio.Length - 1
                    _ratio(i) = inpo.Interpolate(i)
                Next
            End Sub

            ''' <summary>
            ''' Will compare the speed against the ratio table, specifying the best fit on the calibration data
            ''' </summary>
            ''' <param name="speed">byte value of the sample pump speed</param>
            ''' <returns>Ratio belonging to speed</returns>
            ''' <remarks>Should only be used when calibration was completed successful</remarks>
            Private Function getRatio(ByVal speed As Byte) As Double
                Return _ratio(speed)
            End Function

            ''' <summary>
            ''' Will calculate accurate flow rate in uL/s from calibration ratio at specified speed
            ''' </summary>
            ''' <param name="speed">Direct byte level speed</param>
            ''' <returns></returns>
            Public Function getFlowSpeed(ByVal speed As Byte) As Double
                If speed = 0 Then
                    Return 0.0
                End If
                speed = CByte(256 - speed)
                If _depriciatedConstant = 0 Then
                    Dim tmp As Double = getRatio(speed)
                    If tmp > 1 Then
                        tmp = tmp ^ -1
                    End If
                    Return 1 / (tmp * speed)
                Else
                    Return 1 / (_depriciatedConstant * speed)
                End If
            End Function

            ''' <summary>
            ''' tries to determine best ratio that can be calculated during calibration with available calibration points, on the currently calibrating speed
            ''' </summary>
            ''' <returns></returns>
            ''' <remarks>Only used during calibration</remarks>
            Public Function getRatioWhileCalibrating() As Double
                Return getOriginalRatio(_speedIndex)
            End Function


            Private Function getOriginalRatio(ByVal speedIndex As Int16) As Double
                If speedIndex = add_index.Length Then
                    Return -1       'calibration runs are done
                End If

                If add_index(speedIndex) = 0 Then
                    Throw New NotEnoughCalibrationPoints
                End If




                Dim flowrates(add_index(speedIndex) - 1) As Double

                For i = 0 To add_index(speedIndex) - 1
                    flowrates(i) = getFlowRate(CShort(i), speedIndex)
                Next


                Dim meanFlowRate_uL As Double = mean(flowrates) * 1000

                'calculation for flowrate:
                'flowrate = 1 / (cytosettings.sampleSpeedRatio * SamplePompSpeed)
                'f = c^-1*speed^-1 ->c^-1 = f / speed
                'Dim newConstant As Double = 1 / (meanFlowRate_uL * currentSuggestedSpeed)


                Dim newConstant As Double = 1 / (meanFlowRate_uL * _calibrationpoints(speedIndex)(0).Speed)
                Return newConstant

            End Function

            ''' <summary>
            ''' Returns flow rate of one calibration point, in muL/s
            ''' </summary>
            ''' <remarks>Only used during calibration</remarks>
            Private Function getFlowRate(ByVal pointIndex As Int16, ByVal speedIndex As Int16) As Double
                Dim processedVolume As Double = _seringeSize / _calibrationpoints(speedIndex).Length
                Dim flowrate As Double = processedVolume / _calibrationpoints(speedIndex)(pointIndex).Time
                Return flowrate
            End Function

            Private Function mean(ByVal d() As Double) As Double
                Dim tot As Double = 0
                For i = 0 To d.Length - 1
                    tot += d(i)
                Next
                tot /= d.Length
                Return tot
            End Function

        End Class

        Public Class NotEnoughCalibrationPoints
            Inherits Exception
        End Class

        Public Enum CalibrationSpeedSelectionStatus
            OK = 0
            Finished = 1
        End Enum

        <Serializable()> Public Structure CalibrationPoint
            Public Time As Double
            Public VolumeLevel As Double
            Public Speed As Byte


            Public Sub New(ByVal Time As Double, ByVal VolumeLevel As Double, ByVal Speed As Byte)
                Me.Time = Time
                Me.VolumeLevel = VolumeLevel
                Me.Speed = Speed
            End Sub
        End Structure

    End Namespace

End Namespace