
Namespace Concentration

    <Serializable()> Public Class HWConcentrations

        Dim SampleIntervalTimer As New HighResTimer
        Dim _fallbackConcentration As Double = -6
        Dim _fallbackMode As Boolean = False

        Public ReadOnly Property FallBackMode As Boolean
            Get
                Return _fallbackMode
            End Get
        End Property

        ''' <summary>
        ''' CytoUSB normal usage.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            ReDim points(100) '100 is random
            number = 0
        End Sub


        ''' <summary>
        ''' Used to directly input a concentration. Not recommended. Class will go into fall-back mode, and you need to use getFallbackConcentration instead of getConcentrationFromPoint array.
        ''' </summary>
        ''' <param name="concentration"></param>
        ''' <remarks></remarks>
        Public Sub New(ByVal concentration As Double)
            _fallbackConcentration = concentration
            _fallbackMode = True
        End Sub
        Public Function getFallbackConcentration() As Double
            If FallBackMode Then
                Return _fallbackConcentration
            Else
                Throw New Exception("Is not in fallback mode. Use getConcentrationFromPoint array instead")
            End If
        End Function

        Dim points() As HWConcentration
        Dim number As Int16

        Public Sub addConcentration(ByVal count As Int32, ByVal counter_sec As Int32)
            If number = 0 Then
                SampleIntervalTimer.setStartPoint()
            End If
            If number > points.Length - 1 Then
                ReDim Preserve points(points.Length * 2)
            End If
            points(number) = New HWConcentration(count, counter_sec, SampleIntervalTimer.GetDifferenceWithoutStopping)
            number = CShort(number + 1)

        End Sub

        Public Sub addConcentration(ByVal count As Int32, ByVal counter_sec As Int32, ByVal Difference As Int32)
            If number = 0 Then
                SampleIntervalTimer.setStartPoint()
            End If
            If number + 2 > points.Length Then
                ReDim Preserve points(points.Length * 2)
            End If
            points(number) = New HWConcentration(count, counter_sec, Difference)
            number = CShort(number + 1)

        End Sub


        Public Sub clear()
            number = 0
            ReDim points(100)
        End Sub

        Public Function count() As Int16
            Return number
        End Function

        ''' <summary>
        ''' Only use if when using (datafiles with) CytoSense dll release after august 2011. Else use getCounterPoints(cytosettings , measurement , UseInterpolationRecovering )
        ''' </summary>
        ''' <param name="measurement"></param>
        ''' <param name="UseInterpolationRecovering"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getCounterPoints(ByVal measurement As CytoSense.MeasurementSettings.Measurement, ByVal UseInterpolationRecovering As Boolean) As HWConcentration()()
            Return getCounterPoints(measurement.MaxTimeOut, UseInterpolationRecovering)
        End Function

        Public Function getCounterPoints(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurement As CytoSense.MeasurementSettings.Measurement, ByVal UseInterpolationRecovering As Boolean) As HWConcentration()()
            If Object.Equals(Nothing, cytosettings.dllRelease) Then
                Return getCounterPoints(cytosettings.MaxTimeOut, UseInterpolationRecovering)
            End If

            If cytosettings.dllRelease.ReleaseDate > New Date(2011, 8, 1) Then
                Return getCounterPoints(measurement.MaxTimeOut, UseInterpolationRecovering)
            Else
                Return getCounterPoints(cytosettings.MaxTimeOut, UseInterpolationRecovering)
            End If
        End Function

        Public Function getCounterPoints(ByVal maxtimeout As Double, ByVal UseInterpolationRecovering As Boolean) As HWConcentration()()
            If Object.Equals(Nothing, points) Then
                Return Nothing
            End If

            If Object.Equals(points(1), Nothing) Then
                Return Nothing
            End If

            Dim TmpPoints() As HWConcentration = CType(points.Clone(),HWConcentration())
 
            Dim CompensatedPoints(100)() As HWConcentration '100 is random...
            Dim CPCounter As Int16 = 0

            Dim CountOffset As Int32 = -TmpPoints(1).count 
            Dim TimeOffset As Int32 = 0
            For i As Int16 = 1 To CShort(number - 1)

                If Object.Equals(Nothing, TmpPoints(i)) Then
                    number = CShort(number - 1)
                    Exit For
                End If

                Dim p As Int32 = TmpPoints(i).count 
                Dim t As Double = TmpPoints(i).TimeDifference


                If TmpPoints(i).TimeDifference - TmpPoints(i - 1).TimeDifference > maxtimeout - 1 Then ' allowedTime    'minimum time in which the concentrationcounter can round
                    ExtendCompensatedList(CompensatedPoints, CPCounter, i, TmpPoints)
                    CPCounter = CShort(CPCounter + 1)
                End If

            Next
            ExtendCompensatedList(CompensatedPoints, CPCounter, number, TmpPoints)
            CPCounter = CShort(CPCounter + 1)
            ReDim Preserve CompensatedPoints(CPCounter - 1)

            Return MakePointArray(CompensatedPoints, CPCounter, UseInterpolationRecovering, 0) 

        End Function
        Private Sub ExtendCompensatedList(ByRef CompensatedPoints()() As HWConcentration, ByVal CPCounter As Int16, ByVal stopPoint As Int16, ByVal TmpPoints() As HWConcentration)

            If CPCounter > CompensatedPoints.Length - 2 Then
                ReDim Preserve CompensatedPoints(2 * CPCounter)
            End If

            ReDim CompensatedPoints(CPCounter)(100)
            Dim tel As Int16 = 0
            For j As Int16 = 0 To CShort(stopPoint - 1)
                If Not Object.Equals(Nothing, TmpPoints(j)) Then
                    CompensatedPoints(CPCounter)(tel) = TmpPoints(j)
                    tel = CShort(tel + 1)
                    If tel > CompensatedPoints(CPCounter).Length - 1 Then
                        ReDim Preserve CompensatedPoints(CPCounter)(2 * tel)
                    End If
                    TmpPoints(j) = Nothing
                End If
            Next
            ReDim Preserve CompensatedPoints(CPCounter)(tel - 1)
            CPCounter = CShort(CPCounter + 1)
        End Sub
        Private Function MakePointArray(ByVal CompensatedPoints()() As HWConcentration, ByVal CPCounter As Int16, ByVal UseInterpolationRecovering As Boolean, ByVal rec_i As Int32) As CytoSense.Concentration.HWConcentration()()


            Dim recursive As Boolean = False
            Dim forGraph(CPCounter - 1)() As CytoSense.Concentration.HWConcentration

            For i As Int16 = 0 To CShort(CompensatedPoints.Length - 1)

                Dim CountOffset As Int32 = -CompensatedPoints(i)(0).count 'points(0).count
                Dim TimeOffset As Double = CompensatedPoints(i)(0).TimeDifference

                ReDim forGraph(i)(CompensatedPoints(i).Length - 1)  'first point is used for dertermening the offset of the HWcounter
                forGraph(i)(0) = New CytoSense.Concentration.HWConcentration(0, 0, 0)
                For j As Int16 = 1 To CShort(CompensatedPoints(i).Length - 1)

                    Dim p_prev As Int32 = CompensatedPoints(i)(j - 1).count + CountOffset
                    Dim p As Int32 = CompensatedPoints(i)(j).count + CountOffset
                    Dim t As Double = CompensatedPoints(i)(j).TimeDifference - TimeOffset
                    If p < p_prev Then
                        If CompensatedPoints(i)(j).counter_sec > (p_prev - CountOffset) Then
                            p = CompensatedPoints(i)(j).counter_sec + CountOffset
                        Else
                            CountOffset += 65536
                            p += 65536
                        End If

                    End If

                    forGraph(i)(j) = New CytoSense.Concentration.HWConcentration(p, 0, t)
                Next

                Dim tmpdif(forGraph(i).Length - 2) As Double
                If tmpdif.Length > 5 Then
                    For j = 1 To forGraph(i).Length - 1
                        tmpdif(j - 1) = forGraph(i)(j).count - forGraph(i)(j - 1).count
                    Next
                    Dim std As Double = StdDev(tmpdif)
                    Dim mea As Double = mean(tmpdif)


                    For j = 0 To tmpdif.Length - 1
                        If tmpdif(j) > 2 * (mea + std) Then

                            Try
                                CompensatedPoints(i)(j + 1) = CompensatedPoints(i)(j).Interpolate(CompensatedPoints(i)(j + 2))
                            Catch ex As Exception 'interpolating failed... remove data points
                                ReDim Preserve CompensatedPoints(i)(j - 1)
                            End Try

                            recursive = True
                        End If

                    Next

                End If
            Next

            If recursive And UseInterpolationRecovering And Not rec_i > 100 Then
                Return MakePointArray(CompensatedPoints, CPCounter, UseInterpolationRecovering, rec_i + 1)
            Else
                Return forGraph
            End If

        End Function

        Function mean(ByVal data() As Double) As Double
            Dim tmp As Double = 0
            For i As Integer = 0 To data.Length - 1
                tmp += data(i)
            Next
            Return tmp / data.Length
        End Function
        Function StdDev(ByVal data() As Double) As Double
            Dim tmp(data.Length) As Double
            For i As Integer = 0 To data.Length - 1
                tmp(i) = data(i) ^ 2
            Next
            Return Math.Sqrt(mean(tmp) - mean(data) ^ 2)
        End Function

        Public Function getConcentration(ByVal MeasurementSettings As MeasurementSettings.Measurement, ByVal CytoSenseSetting As CytoSense.CytoSettings.CytoSenseSetting) As Double
            If FallBackMode Then
                Return _fallbackConcentration
            Else
                If Object.Equals(Nothing, CytoSenseSetting.dllRelease) Then
                    Return getConcentrationFromPointArray(getCounterPoints(CytoSenseSetting.MaxTimeOut, True), MeasurementSettings, CytoSenseSetting)
                End If

                If CytoSenseSetting.dllRelease.ReleaseDate > New Date(2011, 8, 1) Then
                    Return getConcentrationFromPointArray(getCounterPoints(MeasurementSettings.MaxTimeOut, True), MeasurementSettings, CytoSenseSetting)
                Else
                    Return getConcentrationFromPointArray(getCounterPoints(CytoSenseSetting.MaxTimeOut, True), MeasurementSettings, CytoSenseSetting)
                End If

            End If
        End Function

        Public Function getConcentrationFromPointArray(ByVal forGraph As CytoSense.Concentration.HWConcentration()(), ByVal measurmentsettings As CytoSense.MeasurementSettings.Measurement, ByVal cytosettings As CytoSettings.CytoSenseSetting) As Double
            Return getConcentrationFromPointArray(forGraph, measurmentsettings.getFlowrate(cytosettings), cytosettings)
        End Function
        Public Function getConcentrationFromPointArray(ByVal forGraph As CytoSense.Concentration.HWConcentration()(), ByVal FlowRate As Double, ByVal cytosettings As CytoSettings.CytoSenseSetting) As Double

            If _fallbackMode = True Then
                Throw New Exception("Is in fallback concentration mode, use getFallbackConcentration instead.")
            End If


            If Object.Equals(Nothing, forGraph) Then
                Return -1
            End If

            Dim tel As Int16 = 0
            Dim cons(forGraph.Length - 1) As Double
            Dim totcon As Double = 0
            For i As Int16 = 0 To CShort(forGraph.Length - 1)

                If forGraph(i).Length > 4 Then 'ignore concentration measurements smaller then 3 points
                    'this means that cons will include a place for each ignored concentrations, although being 0!
                    Dim con As Double = forGraph(i)(forGraph(i).Length - 1).count / forGraph(i)(forGraph(i).Length - 1).TimeDifference
                    cons(i) = con
                    totcon += cons(i)
                    tel = CShort(tel + 1)
                End If

            Next
            Dim res As Double = totcon / tel
            Return res / FlowRate


        End Function


        Public ReadOnly Property Concentration(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurementsettings As CytoSense.MeasurementSettings.Measurement) As Double
            Get
                Return getConcentration(measurementsettings, cytosettings)
            End Get
        End Property

    End Class

End Namespace