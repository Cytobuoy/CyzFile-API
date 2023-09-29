Imports System.Drawing
Imports System.Runtime.Serialization
Imports MathNet.Numerics
#Const SWSCOV = True

Namespace Data.ParticleHandling.Channel
<Serializable>
        Public MustInherit Class ChannelData
            Implements ISerializable

        ' Added Implementing ISerializable because some very old data files actually serialized channel data (incorrectly), they should not 
        ' have.  And they changed the data type of the _parameters array. The old one was double, the new one is single. Kind of a strange
        ' situation.  We will never use these objects, but we need to be able to de-serialize them correctly or the whole deserialization
        ' will fail.  Since we no longer save these kind of objects to disk, I will only support loading, and writing will
        ' immediately fail.
        Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
            Throw New NotImplementedException()
        End Sub

        ' Construct on Deserialization. These should never be used, so I could simply init everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' For now we load all members we find that we recognize, if we do not find a member that we expect, we simply ignore it,
        ' I treat all members as optional.  And if there are members in the stream we do not want, we ignore them as well.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_channelInformation" 
                        _channelInformation = DirectCast(current.Value,CytoSense.CytoSettings.channel)
                    Case "_parameters"
                        Dim doublePars() As Double = DirectCast(current.Value,Double())
                        ReDim _parameters(doublePars.Length-1)
                        For i As Integer = 0 To doublePars.Length-1
                            _parameters(i) = CSng(doublePars(i))
                        Next
                    Case "_cytosettings"
                        _cytosettings =  DirectCast(current.Value,CytoSense.CytoSettings.CytoSenseSetting)
                    Case "TimeOfArrival"
                        TimeOfArrival = CSng(current.Value)
                End Select
            End While
        End Sub

        Protected Friend _channelInformation As CytoSense.CytoSettings.channel
        Protected Friend _parameters() As Single
        Protected Friend _cytosettings As CytoSense.CytoSettings.CytoSenseSetting

        ' Do not cache pulse shapes, it costs a lot of memory and does not reallys peed things up
        ' if you use SetParameters fast.  This is optional so performance for Thomas Rutte and other people
        ' does not go down the drain.
        ' Set this to true on startup.
        Public Shared DO_NOT_CACHE_PULSE_SHAPES As Boolean = False


        Public Property TimeOfArrival As Single

        Public MustOverride ReadOnly Property Data() As Single()

        Public Sub New(ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, timeOfArrival As Single)
            _cytosettings = cytoSettings
            _TimeOfArrival = timeOfArrival
        End Sub


        Private _dataDeconv As Single()
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Not cached because non proven technology... (and therefor not used)</remarks>
        Public ReadOnly Property Data_Deconv() As Single()
            Get
                If _dataDeconv Is Nothing Then
                    Dim tmp = Deconvolve()
                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tmp
                    End If
                    _dataDeconv = tmp
                End If
                Return _dataDeconv
            End Get
        End Property
        Private Function createTArray(ByVal length As Int32) As Double()
            Dim res(length - 1) As Double
            For i = 0 To res.Length - 1
                res(i) = i
            Next
            Return res
        End Function
        Private Function Interpolate(ByVal ChData As Single(), ByVal length As Int16) As Double()

            If ChData.Length < 4 Then
                Dim res_fout(length - 1) As Double
                Return res_fout
            End If

            Dim indata(ChData.Length - 1) As Double
            For j = 0 To indata.Length - 1
                indata(j) = ChData(j)
            Next

            If length = ChData.Length Then
                Return indata
            End If

            Dim T() As Double = createTArray(indata.Length)
            Dim res(length - 1) As Double
            Dim inpo = Interpolation.LinearSpline.Interpolate(T, indata)

            For j = 0 To length - 1
                res(j) = inpo.Interpolate(j / (length / ChData.Length))
            Next

            Return res

        End Function

        Public Function getP2(n As Integer) As Integer
            If n > 64 Then
                Return 64
            ElseIf n > 32 Then
                Return 32
            ElseIf n > 16 Then
                Return 16
            ElseIf n > 8 Then
                Return 8
            ElseIf n > 4 Then
                Return 4
            ElseIf n > 2 Then
                Return 2
            Else
                Return 1
            End If
        End Function


        Private Shared ffts(64) As LinearAlgebra.Complex.DenseVector
        Private Shared sigma2 As Single = 5
        Private Shared Amplitude As Single = 1.65
        Private Shared sig_n As Single = 0.25
        Private Shared gamma As Single = 20
        Private shared _varLength As Single = 50 'as a percentage


        Private Function getBaseGFFT(ln As Integer, NOver2 As Integer) As LinearAlgebra.Complex.DenseVector
            If ffts(ln) Is Nothing Then
                Dim H(ln + NOver2 - 1) As Numerics.Complex
                For i = 0 To Math.Min(2 * NOver2, ln - 1)
                    H(i) = Amplitude / Math.Sqrt(2 * Math.PI * sigma2) * Math.Exp(-(i - NOver2) ^ 2 / sigma2)
                Next
                IntegralTransforms.Fourier.Forward(H, IntegralTransforms.FourierOptions.NoScaling)
                Dim Hvec As New LinearAlgebra.Complex.DenseVector(H)
                Dim Hi As New LinearAlgebra.Complex.DenseVector(ln + NOver2)
                For i = 0 To Hi.Count - 1
                    Hi(i) = 1
                Next
                Hi = LinearAlgebra.Complex.DenseVector.OfVector( Hi.PointwiseDivide(Hvec) )

                Dim TF As Single
                For i = 0 To Hi.Count - 1
                    TF = CSng(Numerics.Complex.Abs(H(i)) * gamma > 1) + 1
                    Hi(i) = Hi(i) * (1 - TF) + gamma * Numerics.Complex.Abs(H(i)) * Hi(i) * TF
                Next

                ffts(ln) = Hi
                Return Hi
            Else
                Return ffts(ln)
            End If
        End Function

        Public Function Deconvolve() As Single()

            Dim N As Integer = getP2(Data.Length)
            Dim Nover2 As Integer = CInt(N / 2 - 1)
            Dim d As Double() = Interpolate(Data, CShort(N - Nover2))

            Dim Y(d.Length + Nover2 - 1) As Numerics.Complex
            For i = 0 To d.Length - 1
                Y(Nover2 + i) = d(i)
            Next
            IntegralTransforms.Fourier.Forward(Y, IntegralTransforms.FourierOptions.NoScaling)

            Dim Yvec As New LinearAlgebra.Complex.DenseVector(Y)
            Dim Py As LinearAlgebra.Complex.Vector = LinearAlgebra.Complex.DenseVector.OfVector(Yvec.PointwiseMultiply(Yvec.Conjugate) / d.Length ^ 2)

            Dim Hi = getBaseGFFT(d.Length, Nover2)

            Dim W() As Numerics.Complex
            W = (((Py.PointwiseDivide(Py.Add(sig_n ^ 2))).PointwiseMultiply(Hi)).PointwiseMultiply(Yvec)).ToArray

            IntegralTransforms.Fourier.Inverse(W, IntegralTransforms.FourierOptions.NoScaling)

            Dim data_deconv(CInt(W.Length / 2 - 1)) As Single

            For i = 0 To data_deconv.Length - 1
                If W(i).Real > 0 Then data_deconv(i) = CSng(W(i).Real) Else data_deconv(i) = 0
            Next
            Return data_deconv
        End Function


        ''' <summary>
        ''' Contains unsmoothed, delogged data particle data in mV. Also, the 0-spike filtering  is not happening in here.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>This data is not cached as it saves a member field and is not often used.</remarks>
        Public MustOverride ReadOnly Property Data_mV_unsmoothed() As Single()

        ''' <summary>
        ''' This smoothing is done to filter out noise generated by the AD-converter. It is a low pass that gradually drops off between 1MHz and 2Mhz. 
        ''' Since there is a 1MHz low pass in place before the AD-converter, only noise generated after this filter is thrown out, not valuable data.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Protected Friend Function SmoothData(ByVal d() As Single) As Single()
            Dim res(d.Length - 1) As Single
            For i = 1 To d.Length - 2
                res(i) += CSng(0.25 * (d(i - 1) + d(i + 1)) + 0.5 * d(i))
            Next

            If d.Length > 2 Then
                res(0) = (2 * d(0) + d(1)) / 3
                res(d.Length - 1) = (2 * d(d.Length - 1) + d(d.Length - 2)) / 3
            End If
            Return res
        End Function

        Public shared Property VarLength() As Single
            Get
                Return _varLength
            End Get
            Set(value As Single)
                _varLength = value
                ParameterNames(ParameterSelector.variableLength) = String.Format("Length({0}%)", value)
            End Set
        End Property

        Public Overridable ReadOnly Property Parameter(ByVal k As ChannelData_Hardware.ParameterSelector) As Single
            Get
                If _parameters Is Nothing Then
                    ReDim _parameters(ParameterNames.Length - 1)
                    For i = 0 To _parameters.Length - 1
                        _parameters(i) = Single.NegativeInfinity
                    Next
                End If
                If Single.IsNegativeInfinity(_parameters(k)) Then
                    _parameters(k) = GetParameter(k)
                End If
                Return _parameters(k)
            End Get
        End Property

        Public Const SPIKE_THRESHOLD_START As Integer = 5  ' millivolt
        Public Const SPIKE_THRESHOLD_MAX   As Integer = 550  ' millivolt


        Public Function GetRawSpikeCount() As Integer
            Return CalculateSpikeCount(Data_mV_unsmoothed, SPIKE_THRESHOLD_START, SPIKE_THRESHOLD_MAX)
        End Function

        Public Function GetFilteredSpikeCount As Integer
            Return CalculateSpikeCount(Data, SPIKE_THRESHOLD_START, SPIKE_THRESHOLD_MAX)
        End Function


        ''' <summary>
        ''' Originally it was, 700, 1400, 2800 and then times 2 each time.
        ''' But I waat to try to start ab it lower, and then multiply sooner.
        ''' Originally was 8 * 40 is 320 at 2800, if we want to keep that
        ''' then scaling should be:
        ''' </summary>
        ''' <returns></returns>
        Private Function ScaleThreshold( start As Double, max As Double, val As Single ) As Double
            Return start + ((val/5000)) * (max-start)
        End Function

        ''' <summary>
        ''' When determining if i is a spike, it should look at d(i-1),d(i) and d(i+1) at the same
        ''' time.  This is some extra work perhaps, but it makes it more reliable.
        ''' </summary>
        ''' <param name="d"></param>
        ''' <returns></returns>
        Private Function CalculateSpikeCount( d As Single(), start As Integer, max As Integer) As Integer
            Dim spikeCounter As Integer = 0

            'Special for first/last particle 
            Dim diff = d(1) - d(0)
            If diff < -2*ScaleThreshold(start, max, d(1)) Then ' 2 times the threshold a bit more cricital here.
                spikeCounter += 1
            End If
            diff = d(d.length-1) - d(d.length-2)
            If diff > 2*ScaleThreshold(start, max, d(d.length-2)) Then ' 2 times the threshold a bit more cricital here.
            spikeCounter += 1
            End If
            'Start at 1, tmeporary
            For i = 1 To d.Length - 2 'hmm, -2? Yes! In this case it really should be. 
                Dim spikeThreshold = ScaleThreshold(start, max, (d(i-1)+ d(i+1))/2)

                Dim d1 = d(i) - d(i-1)
                Dim d2 = d(i + 1) - d(i)
                If d1 > spikeThreshold Then
                    If d2 < -1 * spikeThreshold Then
                        spikeCounter += 1
                    End If
                Else If d1 < -1 * spikeThreshold Then
                    If d2 > spikeThreshold Then
                        spikeCounter += 1
                    End If
                End If ' Else diff to small.
            Next
            Return spikeCounter
        End Function

        ''' <summary>
        ''' Information on a (possible) spike. It contains the sample index, and a score.
        ''' The score is basicly how far the actual sample is from the treshold.
        ''' 
        ''' Say e.g. the d1  = 100, d2 = 150, and the threshold is 50 then
        ''' the score will be (d1 - threshold)/d1 + (d2 - threshold) / d2
        ''' Not perfect, but should usually give the bigger one a higher score.
        ''' I think.
        ''' </summary>
        Public Structure SpikeInfo
            Public Index As Integer
            Public Score As Double
        End Structure


        Public Function FindMostLikelySpike() As SpikeInfo
            Dim d = Data_mV_unsmoothed
            Dim si As SpikeInfo
            si.Index = -1
            si.Score = Double.MinValue
            'Special for first/last particle 
            Dim diff = d(1) - d(0)
            Dim th = 2*ScaleThreshold(SPIKE_THRESHOLD_START, SPIKE_THRESHOLD_MAX, d(1))
            If diff < -1* th  Then ' 2 times the threshold a bit more cricital here.
                Dim score = (Math.Abs(diff) - th) / Math.Abs(diff)
                If score > si.Score Then
                    si.Index = 0
                    si.Score = score
                End If
            End If
            diff = d(d.length-1) - d(d.length-2)
            th = 2*ScaleThreshold(SPIKE_THRESHOLD_START, SPIKE_THRESHOLD_MAX, d(d.length-2))
            If diff > th Then ' 2 times the threshold a bit more cricital here.
                Dim score = (diff - th) / diff
                If score > si.Score Then
                    si.Index = d.length-1
                    si.Score = score
                End If
            End If
            'Start at 1, temporary
            For i = 1 To d.Length - 2 'hmm, -2? Yes! In this case it really should be. 
                Dim spikeThreshold = ScaleThreshold(SPIKE_THRESHOLD_START, SPIKE_THRESHOLD_MAX, (d(i-1)+ d(i+1))/2)

                Dim d1 = d(i) - d(i-1)
                Dim d2 = d(i + 1) - d(i)
                If d1 > spikeThreshold Then
                    If d2 < -1 * spikeThreshold Then
                        Dim score =  ((Math.Abs(d1) - spikeThreshold)/Math.Abs(d1)) + ((Math.Abs(d2) - spikeThreshold)/Math.Abs(d2))
                        If score > si.Score Then
                            si.Index = i
                            si.Score = score
                        End If
                    End If
                Else If d1 < -1 * spikeThreshold Then
                    If d2 > spikeThreshold Then
                        Dim score =  ((Math.Abs(d1) - spikeThreshold)/Math.Abs(d1)) + ((Math.Abs(d2) - spikeThreshold)/Math.Abs(d2))
                        If score > si.Score Then
                            si.Index = i
                            si.Score = score
                        End If
                    End If
                End If ' Else diff to small.
            Next
        Return si
End Function


#If SWSCOV Then
        Public Shared ParameterNames() As String = {"Length", "Total", "Maximum", "Average", "Inertia", "Center of gravity", "Fill factor", "Asymmetry", "Number of cells", "Sample Length", "Time of Arrival", "First", "Last", "Minimum", "SWS covariance", "Length(xx%)"}
        Public Shared ParameterUnits() As String = {"[µm]",   "",      "[mV]",    "[mV]",    "",        "",                  "",            "",          "",                "[µm]",          "[s]",             "[mV]",  "[mV]", "[mV]",    "", "[µm]"}


#Else
        Public Shared ParameterNames() As String = {"Length", "Total", "Maximum", "Average", "Inertia", "Center of gravity", "Fill factor", "Asymmetry", "Number of cells", "Sample Length", "Time of Arrival", "First", "Last", "Minimum" }
        Public Shared ParameterUnits() As String = {"[µm]",   "",      "[mV]",    "[mV]",    "",        "",                  "",            "",          "",                "[µs]",          "[s],              "[mV]",  "[mV]", "[mV]""}
#End If


        Public Shared Function getParameterFromName(ByRef name As String) As ParameterSelector
            For i As Integer = 0 To ChannelData.ParameterNames.Length - 1
                If name.ToUpper.Equals(ChannelData.ParameterNames(i).ToUpper) Then
                    Return CType(i, ParameterSelector)
                End If
            Next
            Throw New CytoSenseDoesNotSupportThisOptionException()
        End Function

        Private Function GetParameter(ByVal par_sel As ParameterSelector) As Single
            Return GetParameter(par_sel, Data, _parameters)
        End Function

        Public Function GetParameterWithoutCaching(ByVal par_sel As ParameterSelector) As Single
            Dim dummyParameters = New Single(ParameterNames.Length - 1) {}

            Return GetParameter(par_sel, Data, dummyParameters)
        End Function


        ''' <summary>
        ''' Fast implementation of the getParameter function, all in one shot.
        ''' In practise this actually seems to be slower!?
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub SetParametersFast()
            SetParametersFast({True, True, True, True, True, True, True, True, True, True, True, True, True, True, False})
        End Sub

        ''' <summary>
        ''' Fast implementation of the getParameter function, all in one shot.
        ''' In practise this actually seems to be slower!?
        ''' </summary>
        ''' <remarks></remarks>
        Public Function getParametersFast(mask() As Boolean) As Single()
            SetParametersFast(mask)
            Return _parameters
        End Function

        ''' <summary>
        ''' Fast implementation of the getParameter function, select which parameters in the mask. Same order as ParameterSelector
        ''' In practise this actually seems to be slower!?
        ''' </summary>
        ''' <param name="mask"></param>
        ''' <remarks></remarks>
        Public Sub SetParametersFast(mask() As Boolean)
            '
            ' NOTE: This code should not be used anymore, it may be out of sync with GetParameter...
            '       Throws an exception when used
            '

            Throw New Exception("SetParametersFast called")

            ReDim _parameters(ParameterNames.Length - 1)
            For parIdx = 0 To _parameters.Length - 1
                _parameters(parIdx) = Single.NegativeInfinity
            Next

            'in case of a broken particle, fill all the requested parameters with 1 and exit
            Dim dat As Single() = Data
            Dim i As Integer
            If dat.Length < 3 Then
                For i = 0 To _parameters.Length - 1
                    If mask(i) Then
                        _parameters(i) = 1
                    End If
                Next
                Exit Sub
            End If

            If mask(ParameterSelector.SampleLength) Then 'sample length cannot really be optimized 
                _parameters(ParameterSelector.SampleLength) = CSng(dat.Length - 1) * _cytosettings.Sample_to_um_ConversionFactor
            End If

            'tot
            Dim tot As Single
            Dim max As Single
            Dim min As Single = Single.MaxValue
            For i = 0 To dat.Length - 1
                tot += dat(i)
                If dat(i) > max Then max = dat(i)
                If dat(i) < min Then min = dat(i)
            Next
            Dim totmin1 As Single = tot - dat(dat.Length - 1)

            If mask(ParameterSelector.Total) Or mask(ParameterSelector.NumberOfCells) Then '#cells depends on tot
                _parameters(ParameterSelector.Total) = tot
            End If
            If mask(ParameterSelector.Maximum) Then
                _parameters(ParameterSelector.Maximum) = max
            End If
            If mask(ParameterSelector.Average) Then
                _parameters(ParameterSelector.Average) = totmin1 / (dat.Length - 1)
            End If
            If mask(ParameterSelector.Length) Then 'samplelength depends on max, so calc last
                _parameters(ParameterSelector.Length) = GetParameter(ParameterSelector.Length, dat, _parameters)
            End If
            If mask(ParameterSelector.VariableLength) Then 'samplelength depends on max, so calc last
                _parameters(ParameterSelector.VariableLength) = GetParameter(ParameterSelector.VariableLength, dat, _parameters)
            End If
            If mask(ParameterSelector.Minimum) Then
                _parameters(ParameterSelector.Minimum) = min
            End If
            If mask(ParameterSelector.First) Then
                _parameters(ParameterSelector.First) = dat(0)
            End If
            If mask(ParameterSelector.Last) Then
                _parameters(ParameterSelector.Last) = dat(dat.Length - 1)
            End If


            If mask(ParameterSelector.Inertia) Or
                mask(ParameterSelector.CentreOfGravity) Or
                mask(ParameterSelector.FillFactor) Or
                mask(ParameterSelector.Asymmetry) Then

                'these paramaters are not necessary as often, so only calculate the following shared variables if necessary

                Dim totcorrsquare As Single = 0
                Dim totcorr As Single = 0
                Dim totsquare As Single = 0

                For i = 0 To dat.Length - 2
                    totcorrsquare += Square(i) * dat(i)
                    totcorr += i * dat(i)
                    totsquare += Square(dat(i))
                Next

                If mask(ParameterSelector.CentreOfGravity) Or
                    mask(ParameterSelector.Inertia) Then

                    Dim cog As Single = totcorr / totmin1
                    _parameters(ParameterSelector.CentreOfGravity) = cog

                    If mask(ParameterSelector.Inertia) Then
                        Dim Mnormal As Single = CSng(Math.Abs(tot * Square((dat.Length - 1)) / 12))
                        Dim tmp As Single = totcorrsquare
                        If Mnormal <> 0 Then
                            tmp = CSng(Math.Abs(tmp - Square(cog) * tot) / Mnormal)
                        End If
                        _parameters(ParameterSelector.Inertia) = tmp
                    End If
                End If

                If mask(ParameterSelector.FillFactor) Then
                    _parameters(ParameterSelector.FillFactor) = Square(totmin1) / (totsquare * (dat.Length - 1))
                End If

                If mask(ParameterSelector.Asymmetry) Then
                    Dim tmp As Single = totcorr / totmin1
                    tmp = Math.Abs(2 * tmp / (dat.Length - 1) - 1)
                    _parameters(ParameterSelector.Asymmetry) = tmp
                End If
            End If

            If mask(ParameterSelector.NumberOfCells) Then 'Sadly, #Cells is difficult to optimize, while being the most time consuming operation by far
                '#cells depends on tot. Make sure it is below.
                _parameters(ParameterSelector.NumberOfCells) = GetParameter(ParameterSelector.NumberOfCells, dat, _parameters)
            End If

            _parameters(ParameterSelector.TimeOfArrival) = TimeOfArrival

#If SWSCOV Then
            If mask(ParameterSelector.SWSCOV) Then
                _parameters(ParameterSelector.SWSCOV) = CalculateSWSCov()
            End If
#End If
        End Sub

        ' Replace Pow function,  because ^pow is very inefficient
        ' Note the ^of a single returns a double in vb, so kept that some thing here.
        Private Shared Function Square( val As Single ) As Single
            Return val * val
        End Function

        Private Shared Function Square( val As Double ) As Double
            Return val * val
        End Function

        Private Shared Function Square( val As Integer ) As Int64
            Dim lVal As Int64 = CLng(val)
            Return lVal * lVal
        End Function


        ''' <summary>
        ''' Returns a selected parameter based on the data of one channel contained in ChannelData()
        ''' </summary>
        ''' <param name="par_sel"></param>
        ''' <param name="Data"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Protected Function GetParameter(ByVal par_sel As ParameterSelector, Data() As Single, parameters As Single()) As Single
            Dim dat = Data
            Dim i As Integer
            If dat.Length < 3 Then
                Return 1
            End If

            ' Calculate selected parameter
            Select Case par_sel
                Case ParameterSelector.Length
                    Dim threshold As Single
                    threshold = CSng(0.5 * CSng(Parameter(ParameterSelector.Maximum)))
                    parameters(par_sel) = CalculateLength(threshold, dat)

                Case ParameterSelector.variableLength
                    'Divide VarLength is set using a slider and displayed as percentage (0 to 100). Divide by 100 to get the proper value to use in calculation
                    Dim varThreshold As Single
                    varThreshold = CSng(VarLength/100 * CSng(Parameter(ParameterSelector.Maximum)))
                    Parameters(par_sel) = CalculateLength(varThreshold, dat)
                Case ParameterSelector.SampleLength
                    Dim TOF As Single = CSng(dat.Length - 1) * _cytosettings.Sample_to_um_ConversionFactor
                    parameters(par_sel) = TOF
                Case ParameterSelector.Total
                    Dim tot As Single = 0
                    For i = 0 To dat.Length - 1
                        tot += dat(i)
                    Next
                    parameters(par_sel) = tot

                Case ParameterSelector.Maximum
                    Dim max As Single = dat(0)
                    For i = 1 To dat.Length - 1
                        If dat(i) > max Then max = dat(i)
                    Next
                    parameters(par_sel) = max

                Case  (ParameterSelector.Minimum) 
                    Dim min As Single = dat(0)
                    For i = 1 To dat.Length - 1
                        If dat(i) < min Then min = dat(i)
                    Next
                    parameters(par_sel) = min

                Case ParameterSelector.First
                    parameters(par_sel) = dat(0)

                Case ParameterSelector.Last
                    parameters(par_sel) = dat(dat.Length - 1)

                Case ParameterSelector.Average
                    Dim totmin1 As Single = 0
                    For i = 0 To dat.Length - 2 'hmm, -2? Legacy...
                        totmin1 += dat(i)
                    Next
                    parameters(par_sel) = totmin1 / (dat.Length - 1)

                Case ParameterSelector.Inertia
                    Dim totcorrsquare As Single = 0
                    For i = 0 To dat.Length - 2 'hmm, -2? Legacy...
                        totcorrsquare += Square(i) * dat(i)
                    Next
                    
                    Dim Mnormal As Single = CSng(Math.Abs(Parameter(ParameterSelector.Total) * Square(dat.Length - 1) / 12))
                    If Mnormal <> 0 Then
                        totcorrsquare = CSng(Math.Abs(totcorrsquare - Square(Parameter(ParameterSelector.CentreOfGravity)) * Parameter(ParameterSelector.Total)) / Mnormal)
                    End If

                    parameters(par_sel) = totcorrsquare

                Case ParameterSelector.CentreOfGravity
                    Dim totcorr As Single = 0
                    Dim totmin1 As Single = 0
                    For i = 0 To dat.Length - 2 'hmm, -2? Does not matter if smoothing is enabled
                        totcorr += i * dat(i)
                        totmin1 += dat(i)
                    Next
                    totcorr /= totmin1
                    parameters(par_sel) = totcorr

                Case ParameterSelector.FillFactor
                    Dim sumDataSquared As Single = 0
                    Dim sumData As Single = 0

                    For i = 0 To dat.Length - 1
                        sumData += dat(i)
                        sumDataSquared += dat(i) * dat(i)
                    Next

                    If sumDataSquared = 0 Then
                        parameters(ParameterSelector.FillFactor) = 0
                    Else
                        parameters(ParameterSelector.FillFactor) = (sumData * sumData) / (dat.Length * sumDataSquared)
                    End If

                Case ParameterSelector.Asymmetry
                    Dim totmin1 As Single = 0
                    Dim totcorr As Single = 0
                    For i = 0 To dat.Length - 2 'hmm, -2? Does not matter if smoothing is enabled
                        totcorr += i * dat(i)
                        totmin1 += dat(i)
                    Next
                    totcorr /= totmin1 ' = CoG
                    totcorr = Math.Abs(2 * totcorr / (dat.Length - 1) - 1) 
                    parameters(par_sel) = totcorr

                Case ParameterSelector.NumberOfCells
                    Dim sumDeltaNeighbourSquared As Double = 0
                    Dim sumDataSquared As Double
                    Dim deltaNeighbour As Double
                    Dim par As Single = 0

                    sumDataSquared = dat(0) * dat(0)

                    For i = 1 To dat.Length - 1
                        deltaNeighbour = dat(i) - dat(i - 1)
                        sumDeltaNeighbourSquared += deltaNeighbour * deltaNeighbour
                        sumDataSquared += dat(i) * dat(i)
                    Next

                    Dim sumDataSquaredMinusTotalSquaredMean = sumDataSquared - Square(Parameter(ParameterSelector.Total)) / dat.Length

                    If sumDataSquaredMinusTotalSquaredMean = 0 Then
                        par = 0
                    Else
                        par = CSng(dat.Length / (2 * Math.PI) * Math.Sqrt(sumDeltaNeighbourSquared / sumDataSquaredMinusTotalSquaredMean))
                    End If
#If DEBUG Then
                    If Single.IsNaN(par) Then 
                        Console.WriteLine("sumDataSquared {0} total {1}", sumDataSquared, Parameter(ParameterSelector.Total))
                    End If
#End If
                    parameters(par_sel) = par
                Case ParameterSelector.TimeOfArrival
                    parameters(par_sel) = TimeOfArrival
#If SWSCOV Then
                Case ParameterSelector.SWSCOV
                    parameters(par_sel) = CalculateSWSCov()
#End If
            End Select

            If Double.IsInfinity(parameters(par_sel)) Then
                parameters(par_sel) = 0
            End If

            Return parameters(par_sel)
        End Function

        Public Property sws As ChannelData_Hardware

        Private Function CalculateLength(threshold As Single, dat() As Single) As Single
            Dim par As Single = 0
            'Determines the 'length' of a particle
            Dim Particle_start As Integer
            Dim Particle_end As Integer

            'Find the first point higher than threshold
            Particle_start = 0
            While Particle_start <= dat.Length - 1 AndAlso CSng(dat(Particle_start)) <= threshold 
                Particle_start = Particle_start + 1
            End While

            Particle_end = dat.Length - 1
            'Find the last point higher than threshold
            While Particle_end >= 0 AndAlso CSng(dat(Particle_end)) <= threshold 
                Particle_end = Particle_end - 1
            End While

            'Interpolate
            Dim AA As Single = 0.0
            If Particle_start > 0 AndAlso Particle_start < dat.Length Then ' Previous sample below threshold, so interpolate
                AA = Particle_start - (dat(Particle_start) - threshold) / (dat(Particle_start) - dat(Particle_start - 1))
            End If ' Else either the first sample was above the threshold or NO samples were above the threshold.
            Debug.Assert(AA=0 OrElse ((Particle_start -1) <= AA AndAlso AA <= Particle_start ), "Start Interpolation Failure when calculating length parameter")

            Dim BB As Single = dat.Length - 1
            If Particle_end < dat.Length - 1 AndAlso Particle_end >= 0 Then 'Sample after below, so interpolate.
                BB = Particle_end + (dat(Particle_end) - threshold) / (dat(Particle_end) - dat(Particle_end + 1))
            End If 'Else last sample above threshold or NO samples above threshold.
            Debug.Assert(BB=dat.Length - 1 OrElse (Particle_end <= BB AndAlso BB <= (Particle_end+1)),"End Interpolation Failure when calculating length parameter")

            Dim TOF As Single = (BB - AA) * _cytosettings.Sample_to_um_ConversionFactor

            'Correct for convolution
            Dim u As Double
            u = Square((_cytosettings.LaserBeamWidth / TOF))

            'Based on convolution of laser with semi-sphere
            TOF = CSng(TOF * 70 / (70 + 1 * u + 0.5 * u * u + 1.5 * u * u * u * u))

            If TOF <= 0.2 Then
                par = 0.2
            Else
                par = TOF
            End If
            Return par
        End Function
        Private Function CalculateSWSCov() As Single
            If _sws Is Nothing Then
                Return 0
            End If
            If _sws.Information.name = Information.name Then
                Return CalculateR1Cos()
            End If

            Return Covariance(Data_Deconv, sws.Data_Deconv)

        End Function

        ''' <summary>
        ''' Test parameter, to see if we can filter out some small fluorescent stuff, by trying to remove noise, by assuming
        ''' all small particles look like a Gaussian/ single cosine wave, and do a partial FFT to extract only that
        ''' frequency. And som some interesting calculation on them.
        ''' Can probably be optimized a bit.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function CalculateR1Cos() As Single
            Dim pCosSum As Double = 0.0
            Dim len As Integer = Data.Length
            Const M2PI As Double = 2 * Math.PI
            Dim sampRad As Double = M2PI / len    ' The number of radians per sample point.
            For i = 0 To len - 1
                pCosSum += Data(i) * Math.Cos(i * sampRad)
            Next
            If pCosSum >= 0 Then
                Return 0.0
            End If
            Return CSng(-2 * (pCosSum / len))
        End Function


        Function Covariance(ByRef dataX() As Single, ByRef dataY() As Single) As Single
            Dim res As Single = 0
            Dim avgx As Single = 0
            Dim avgy As Single = 0

            For i As Integer = 0 To dataX.Length - 1
                res += dataX(i) * dataY(i)
                avgx += dataX(i)
                avgy += dataY(i)
            Next
            avgx /= dataX.Length
            avgy /= dataX.Length
            res /= dataX.Length
            Return res - avgx * avgy
        End Function

        Public Overridable ReadOnly Property Information As CytoSense.CytoSettings.channel
            Get
                Return _channelInformation
            End Get
        End Property
        <Serializable> Public Enum ParameterSelector 'Serializable for dot plot presets in cc4
            Length = 0
            Total = 1
            Maximum = 2
            Average = 3
            Inertia = 4
            CentreOfGravity = 5
            FillFactor = 6
            Asymmetry = 7
            NumberOfCells = 8
            SampleLength = 9
            TimeOfArrival = 10
            First = 11
            Last = 12
            Minimum = 13
#If SWSCOV Then
            SWSCOV = 14
#End If
            VariableLength = 15

        End Enum

        Public Overrides Function ToString() As String
            Return _channelInformation.ToString
        End Function

        Public Function getListModeVector() As Single()
            Dim res(ParameterNames.Length - 1) As Single
            For i = 0 To ParameterNames.Length - 1
                res(i) = Parameter(CType(i,ParameterSelector))
            Next
            Return res
        End Function

        ''' <summary>
        ''' Lookup table and delog crorrection factor need for hte old ruud electronics.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getmVLookup_RuudElectronics() As Integer()
        Dim mV(255) As Integer
            For i = 0 To 255
                mV(i) = CInt(3.2767 * Math.Pow(10, CDbl(i) / 63.75))
            Next
            Return mV
        End Function
        Public Shared Function getmDelogCorrectionFactor_RuudElectronics() As Double
            Return 0.5 / 3.2767
        End Function

        Public Shared Function getNewmVLookup_RuudElectronics As Single()
            Dim mV(255) As Single
            For i = 0 To 255
                mV(i) = CSng(0.5 * Math.Pow(10, CDbl(i) / 63.75))
            Next
            Return mV
        End Function


        ''' <summary>
        ''' Lookup table and de-log correction factor needed for new V10 electronics.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function getmVLookup_FjElectronics() As Integer()
            Dim mV(255) As Integer

            For i = 0 To 255
                Dim exponent As Integer = (i And &HF0) >> 4
                Dim fraction = i And &HF
                If exponent < 4 Then
                    mV(i) = fraction
                Else
                    mV(i) = (&H10 Or fraction) << (exponent - 4)
                End If
            Next
            Return mV
        End Function
        Public Shared Function getmDelogCorrectionFactor_FjElectronics() As Single
            Return CSng(8192) / CSng(65535)
        End Function

        ''' <summary>
        ''' Lookup table for new electronics, note, will have multiple lookup tables
        ''' based on conversion per channel.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function getNewmVLookup_FjElectronics(dataConversion As MeasurementSettings.LogConversion) As Single()

            Select dataConversion 
                Case MeasurementSettings.LogConversion.OriginalLog 
                    Return calculateMvLookup_OriginalLogConversion()
                Case MeasurementSettings.LogConversion.Decade16_3bLog
                    Return calculateMvLookup_Decade16_3bLogConversion()
                Case MeasurementSettings.LogConversion.Linear8Bit_low       ' reg= 7 8 Bit linear, lowest 8 bits,  step= 0.125, max=  32 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(0)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_1 ' reg= 8 8 Bit linear, shifted 1 bit,  step= 0.250, max=  64 millivolt (minus a few).
Return calculateMvLookup_Linear_8bit(1)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_2 ' reg= 9 8 Bit linear, shifted 2 bit,  step= 0.500, max= 128 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(2)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_3 ' reg=10 8 Bit linear, shifted 3 bit,  step= 1.000, max= 256 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(3)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_4 ' reg=11 8 Bit linear, shifted 4 bit,  step= 2.000, max= 512 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(4)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_5 ' reg=12 8 Bit linear, shifted 5 bit,  step= 4.000, max=1024 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(5)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_6 ' reg=13 8 Bit linear, shifted 6 bit,  step= 8.000, max=2048 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(6)
                Case MeasurementSettings.LogConversion.Linear8Bit_shifted_7 ' reg=14 8 Bit linear, shifted 7 bit,  step=16.000, max=4096 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(7)
                Case MeasurementSettings.LogConversion.Linear8Bit_high      ' reg=15 8 Bit linear, highest 8 bits, step=32.000, max=8192 millivolt (minus a few).
                    Return calculateMvLookup_Linear_8bit(8)
                Case Else
                    Throw New Exception(String.Format("Unsupported dataConversion: '{0}'", dataConversion))
            End Select

        End Function

        Public Shared Function calculateMvLookup_OriginalLogConversion() As Single()
            Dim mV(255) As Single
            Dim tmp As Integer
            For i = 0 To 255
                Dim exponent As Integer = (i And &HF0) >> 4
                Dim fraction = i And &HF
                If exponent < 4 Then
                    tmp = fraction
                Else
                    tmp = (&H10 Or fraction) << (exponent - 4)
                End If
                mV(i) =  CSng(0.125 * tmp)
            Next
            Return mV
        End Function

        Public Shared Function calculateMvLookup_Decade16_3bLogConversion() As Single()
            Dim mV(255) As Single

            For i = 0 To 255
        		Dim new_decade = (i And &HF0) >> 4
		        Dim new_mantisse = i And &H0F
		        Dim result_int As Integer = 0
		
		        If new_decade = 15 Then
			        result_int = (&H03 << 14) Or (new_mantisse << 10) Or (1 << 9)
		        Else If new_decade = 14
			        result_int = (&H02 << 14) Or (new_mantisse << 10) Or (1 << 9)
		        Else If new_decade = 13
			        result_int = (&H01 << 14) Or (new_mantisse << 10) Or (1 << 9)

		        Else If new_decade = 12
			        result_int = (&H03 << 12) Or (new_mantisse << 8) Or (1 << 7)
		        Else If new_decade = 11
			        result_int = (&H02 << 12) Or (new_mantisse << 8) Or (1 << 7)
                else if new_decade = 10
                    result_int = (&H01 << 12) Or (new_mantisse << 8) Or (1 << 7)
                else if new_decade = 9
                    result_int = (1 << 11) Or (new_mantisse << 7) Or (1 << 6)
                else if new_decade = 8
                    result_int = (1 << 10) Or (new_mantisse << 6) Or (1 << 5)
                else if new_decade = 7
                    result_int = (1 << 9) Or (new_mantisse << 5) Or (1 << 4)
                else if new_decade = 6
                    result_int = (1 << 8) Or (new_mantisse << 4) Or (1 << 3)
		        else if new_decade = 5
			        result_int = (1 << 7) Or (new_mantisse << 3) Or (1 << 2)
                else if new_decade = 4
                    result_int = (1 << 6) Or (new_mantisse << 2) Or (1 << 1)
                else
                    result_int = (new_decade << 4) Or (new_mantisse)
                End If
                mV(i) =  CSng(0.125 * result_int)
            Next
            Return mV
        End Function

        Public Shared Function calculateMvLookup_Linear_8bit( shift As Integer) As Single()
            Dim mV(255) As Single
            Dim tmp As UInt16
            For i = 0 To 255
                tmp = CUShort(i) << shift
                mV(i) = CSng((0.125 * tmp))
            Next
            Return mV
        End Function
        End Class

        <Serializable()> Public Class ChannelData_Hardware
        Inherits ChannelData

        ' Added Implementing ISerializable because some very old data files actually serialized channel data (incorrectly), they should not 
        ' have.  And they changed the data type of the _parameters array. The old one was double, the new one is single. Kind of a strange
        ' situation.  We will never use these objects, but we need to be able to de-serialize them correctly or the whole deserialization
        ' will fail.  Since we no longer save these kind of objects to disk, I will only support loading, and writing will
        ' immediately fail.
        ' Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        '     MyBase.GetObjectData(info,context)
        ' End Sub
        ' Base class implementation will throw if ever used, so no need to override that.

        ' Construct on Deserialization. These should never be used, so I could simply initialize everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' Not the most efficient way, every subclass loops through the list of available properties, but should only be ever used looking
        ' for very old files.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info,context)

            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_data_raw" 
                        _data_raw = DirectCast(current.Value, Byte())
                    Case "_data"
                        _data = DirectCast(current.Value,Single())
                    Case "_channelDataConversion"
                        _channelDataConversion = DirectCast(current.Value,MeasurementSettings.LogConversion)
                End Select
            End While
        End Sub


        Protected Friend _data_raw As Byte()
        Protected Friend _data() As Single
        Protected Friend ReadOnly _channelDataConversion As MeasurementSettings.LogConversion 
        ''' <summary>
        ''' Basic constructor. 
        ''' </summary>
        Protected Friend Sub New(dataConversion As MeasurementSettings.LogConversion, ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, timeofarrival As Single)
            MyBase.New(cytoSettings, timeofarrival)
            _channelDataConversion  = dataConversion
        End Sub

        ''' <summary>
        ''' This constructor enables you to only calculate the mv_lookup table once, instead of every time you need the de-logged data for another particle
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="info"></param>
        ''' <param name="cytoSettings">Needed for some constants to calculate parameters</param>
        ''' <remarks>Parameter properties are still only computed on demand</remarks>
        Public Sub New(dataConversion As MeasurementSettings.LogConversion, ByVal data As Byte(), ByVal info As CytoSense.CytoSettings.channel, ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, timeofarrival As Single)
            MyBase.New(cytoSettings, timeofarrival)
            _channelDataConversion  = dataConversion 
            _channelInformation = info
            _data_raw = data
        End Sub


        ''' <summary>
        ''' TESTING PUPOSES ONLY! Create a particle using data already converted to mV and filtered. 
        ''' This used to create a channel with known millivolt values for verification of the parameter calculation.
        ''' It bypasses all the conversion/filtering logic so we can focus only on parameter calculations.
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="info"></param>
        ''' <param name="cytoSettings">Needed for some constants to calculate parameters</param>
        ''' <remarks>NOT A COMPLETE FUNCTIONING PARTICLE, TESTING ONLY</remarks>
        Public Sub New(dataConversion As MeasurementSettings.LogConversion, ByVal data As Single(), ByVal info As CytoSense.CytoSettings.channel, ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, timeofarrival As Single)
            MyBase.New(cytoSettings, timeofarrival)
            _channelDataConversion  = dataConversion 
            _channelInformation = info
            _data = data
        End Sub

        Public Sub New(ByVal dataRaw As Byte(), ByVal info As CytoSense.CytoSettings.channel, ByVal cytoSettings As CytoSense.CytoSettings.CytoSenseSetting, timeofarrival As Single)
            MyBase.New(cytoSettings, timeofarrival)
            _channelInformation = info
            _data_raw = dataRaw
        End Sub



        ''' <summary>
        ''' Original byte log data. Fast, bust should use smoothed mv data instead.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Data_Raw As Byte()
            Get
                Return _data_raw
            End Get
        End Property


        ''' <summary>
        ''' Process the data using an EAWAG spike filter.  spikeStart is the index a spike,
        ''' and then it simply adds/subtracts 31 to get to the other spikes.
        ''' Every spike value is replaced with the average value of the two.
        ''' This function is a copy of the Data property, but it adds the EawagFiltering in
        ''' the middle. The filtering is done on the byte values instead of on the
        ''' millivolts that way it is easier to integrate.  But we may change that later if
        ''' it turns out to be difficult.
        ''' </summary>
        ''' <param name="spikeStart"></param>
        Public Sub ForceEawagSpikeFilter( spikeStart As Integer )
            If _data IsNot Nothing Then
                Throw New Exception("Internal Error: Eawag SPike filtering should be called before the first data is loaded!")
            End If
            Dim len As Integer = _data_raw.Length
            Dim tmp(len - 1) As Single
            ConvertByteValuesAndFilterSync(tmp,_data_raw, _channelInformation.SyncPulseValue, _channelDataConversion )

            ' NOW FILTER THE EAWAG SPIKES ...
            EawagInplaceSpikeFilter(tmp, spikeStart)

            _data = SmoothData(tmp)
        End Sub

        Protected Sub EawagInplaceSpikeFilter(chanData As Single(), startIdx As Integer )
            Dim currIdx = startIdx
            While currIdx < chanData.Length
                If currIdx >= 0 Then
                    EawagFilterSingleSpikeInPlace(chanData, currIdx)
                End If
                currIdx += 31
            End While

            currIdx = startIdx - 31
            While currIdx >= 0
                If currIdx < chanData.Length Then
                    EawagFilterSingleSpikeInPlace(chanData, currIdx)
                End If
                currIdx -= 31
            End While
        End Sub

        Protected Sub EawagFilterSingleSpikeInPlace( chanData As Single() , spikeIdx As Integer)
            If spikeIdx = 0 Then
                chanData(0) = chanData(1)
            Else If spikeIdx = (chanData.Length - 1) Then
                chanData(spikeIdx) = chanData(spikeIdx-1)
            Else
                chanData(spikeIdx) = CSng(( chanData(spikeIdx-1) + chanData(spikeIdx+1) ) / 2)
            End If
        End Sub


        ''' <summary>
        ''' Convert the src  byte sample values to a single array, using the _cytosettings.mV_lookupNew
        ''' function.  This is the complete millivolt conversion.  It is no longer split into an
        ''' integer part and a final conversion to float.
        ''' 
        ''' During the conversion Sync values at the start/beginning are filtered, and 0 values in
        ''' the middle of the signal are replaced with the average of the neighbors to remove
        ''' artifacts caused by older versions of the CytoUsb.
        ''' </summary>
        ''' <param name="dest">Array where the converted single values are stored</param>
        ''' <param name="src">source containing the byte values from the source.</param>
        ''' <param name="syncPulseValue">synchPulse byte value for the channel.</param>
        Protected Sub ConvertByteValuesAndFilterSync(dest As Single(), src As Byte(), syncPulseValue As Short, _dataConv As MeasurementSettings.LogConversion)

            Dim mvLookup = _cytosettings.mV_lookupNew(_dataConv)

            Dim len = src.Length
            If len > 1 Then
                If src(0) = syncPulseValue Then
                    dest(0) = mvLookup(src(1))
                Else
                    dest(0) = mvLookup(src(0))
                End If
                If src(len - 1) = syncPulseValue Then
                    dest(len - 1) = mvLookup(src(len - 2))
                Else
                    dest(len - 1) = mvLookup(src(len - 1))
                End If
            End If
            For i = 1 To len - 2
                If src(i) = 0 AndAlso (i > 0 And i < len - 1) AndAlso src(i - 1) > 0 AndAlso src(i + 1) > 0 Then
                    'Fills up an missing data point to solve an issue with CytoUSB. 
                    'The issue is now solved, but data files having missing data points are still around!
                    dest(i) = mvLookup(CInt(CInt(CSng(src(i - 1)) + src(i + 1)) / 2))
                Else
                    dest(i) = mvLookup(src(i))
                End If
            Next
        End Sub


        ''' <summary>
        ''' Contains smoothed, de-logged data particle data in mV.  on the particle on loading.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property Data() As Single()
            Get
                If _data Is Nothing Then
                    Dim len As Integer = _data_raw.Length
                    Dim tmp(len - 1) As Single
                    ConvertByteValuesAndFilterSync(tmp,_data_raw, _channelInformation.SyncPulseValue, _channelDataConversion)

                    Dim tmpData As Single()

                    tmpData = SmoothData(tmp)

                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tmpData
                    End If

                    _data = tmpData
                End If
                Return _data
            End Get
        End Property


        ''' <summary>
        ''' Contains un smoothed, de-logged data particle data in mV. 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>The un smoothed data is always calculated, instead of 
        ''' cached like the normal Data. This saves a member field (which is unused 
        ''' if normal Data is used) but makes its usage slower. In practice non-smoothed should 
        ''' not be used often.</remarks>
        Public Overrides ReadOnly Property Data_mV_unsmoothed() As Single()
            Get
                Dim tmp(Data_Raw.Length - 1) As Single
                For i = 0 To Data_Raw.Length - 1
                    tmp(i) = _cytosettings.mV_lookupNew(_channelDataConversion)(Data_Raw(i))
                Next i
                Return tmp
            End Get
        End Property

    End Class

    ' NOTE[rl]  Should this be a subclass of ChannelDataHardWare?  SHould it not be a virtual channel instead?
    <Serializable()> Public Class ChannelData_FWSCurvature
        Inherits ChannelData_Hardware

        ' Construct on Deserialization. These should never be used, so I could simply init everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' Not the most efficient way, every subclass loops through the list of available properties, but should only be ever used looking
        ' for very old files.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info,context)

            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_curvData" 
                        _curvData = DirectCast(current.Value, Single())
                    Case "_fwsl"
                        _fwsl = DirectCast(current.Value,ChannelData_Hardware)
                    Case "_FWSR"
                        _FWSR = DirectCast(current.Value,ChannelData_Hardware)
                End Select
            End While
        End Sub

        Private _curvData() As Single
        Private _fwsl As ChannelData_Hardware
        Private _FWSR As ChannelData_Hardware
        Private Shared ReadOnly _channelDefinition As New CytoSense.CytoSettings.channel("FWS", Color.Black)


        Public Sub New(dataConversion As MeasurementSettings.LogConversion, ByRef FWSL As ChannelData_Hardware, ByRef FWSR As ChannelData_Hardware, ByRef cytoSettings As CytoSense.CytoSettings.CytoSenseSetting)
            MyBase.New(dataConversion, cytoSettings, FWSL.TimeOfArrival)
            _fwsl = FWSL
            _FWSR = FWSR
        End Sub

        Public ReadOnly Property Curvature As Single()
            Get
                If Object.Equals(Nothing, _curvData) Then
                    Dim tp = CalculateDataAndCurvature(True)
                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tp.Item2
                    End If
                    _data    = tp.Item1
                    _curvData = tp.Item2
                End If
                Return _curvData
            End Get
        End Property

        ''' <summary>
        ''' Unsmoothed is not cached!
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CurvatureUnsmoothed As Single()
            Get
                Dim tp = CalculateDataAndCurvature(True)
                Return tp.Item2
            End Get
        End Property



        Public Overrides ReadOnly Property Information As CytoSettings.channel
            Get
                Return _channelDefinition
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Not cached because it should not be used often</remarks>
        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
            Get
                Dim tp = CalculateDataAndCurvature(False)
                Return tp.Item1
            End Get
        End Property

        Public Overrides ReadOnly Property Data As Single()
            Get
                If Object.Equals(Nothing, _data) Then
                    Dim tp = CalculateDataAndCurvature(True)
                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tp.Item1
                    End If
                    _data    = tp.Item1
                    _curvData = tp.Item2
                End If
                Return _data
            End Get
        End Property



        ''' <summary>
        ''' Process the data using an EAWAG spike filter.  spikeStart is the index a spike,
        ''' and then it simply adds/subtracts 31 to get to the other spikes.
        ''' Every spike value is replaced with the average value of the two.
        ''' chan indicates which of the 2 channels FwsL or FwsR should be spike filtered.
        ''' Call this before any other access tot he data when forcing the 
        ''' EAWAG spike filter.
        ''' 
        ''' It is a copy of the calcData_andSetCurvData, with spike filtering added for the specified
        ''' channel just before calculating the combined channel/curvature.
        ''' 
        ''' </summary>
        ''' <param name="chan">The channels that contains the spike (FWS L or FWS R)</param>
        ''' <param name="spikeStart">Index of one sample that is considered a spike.</param>
        Public Overloads Sub ForceEawagSpikeFilter( chan As CytoSense.CytoSettings.ChannelTypesEnum, spikeStart As Integer )
            If _data IsNot Nothing Then
                Throw New Exception("Internal Error: Eawag Spike filtering should be called before the first data is loaded!")
            End If
            'Prepare the 2 base channels.

            Dim sampLen As Integer = _fwsl.Data_Raw.Length
            Dim tmpFwsL(sampLen - 1) As Single
            Dim tmpFwsR(sampLen - 1) As Single

            ConvertByteValuesAndFilterSync(tmpFwsL, _fwsl.Data_Raw, _fwsl.Information.SyncPulseValue, _channelDataConversion)
            ConvertByteValuesAndFilterSync(tmpFwsR, _FWSR.Data_Raw, _FWSR.Information.SyncPulseValue, _channelDataConversion)

            ' NOW FILTER THE EAWAG SPIKES For the specified channel.
            If chan = CytoSettings.ChannelTypesEnum.FWSL Then
                EawagInplaceSpikeFilter(tmpFwsL, spikeStart)
            Else
                EawagInplaceSpikeFilter(tmpFwsR, spikeStart)
            End If

            Dim data(_fwsl.Data_Raw.Length - 1) As Single
            Dim tmp(_fwsl.Data_Raw.Length - 1)  As Single
            Dim curv(_fwsl.Data_Raw.Length - 1) As Single
            Dim tot As Single = 0

            For i = 0 To tmp.Length - 1
                tmp(i) = tmpFwsL(i) + tmpFwsR(i)
                curv(i) = (CSng(tmpFwsR(i) - tmpFwsL(i)) / (tmp(i) + 1))
                tot += curv(i)
            Next i

            Dim offset As Single = tot / (tmp.Length)
            For j = 0 To tmp.Length - 1
                curv(j) = curv(j) - offset 'Offset from centre correction
            Next j

            data = SmoothData(tmp)

            _curvData = curv
            _data = data
        End Sub


        Private Function CalculateDataAndCurvature(smoothing As Boolean) As Tuple(Of Single(),Single())
            Dim data(_fwsl.Data_Raw.Length - 1) As Single
            Dim tmp(_fwsl.Data_Raw.Length - 1)  As Single
            Dim curv(_fwsl.Data_Raw.Length - 1) As Single
            Dim tot As Single = 0

            Dim sampLen As Integer = _fwsl.Data_Raw.Length
            Dim tmpFwsL(sampLen - 1) As Single
            Dim tmpFwsR(sampLen - 1) As Single

            ConvertByteValuesAndFilterSync(tmpFwsL, _fwsl.Data_Raw, _fwsl.Information.SyncPulseValue,_channelDataConversion)
            ConvertByteValuesAndFilterSync(tmpFwsR, _FWSR.Data_Raw, _FWSR.Information.SyncPulseValue,_channelDataConversion)

            For i = 0 To tmp.Length - 1
                tmp(i) = tmpFwsL(i) + tmpFwsR(i)
                curv(i) = (CSng(tmpFwsR(i) - tmpFwsL(i)) / (tmp(i) + 1))
                tot += curv(i)
            Next i

            Dim offset As Single = tot / (tmp.Length)
            For j = 0 To tmp.Length - 1
                curv(j) = curv(j) - offset 'Offset from center correction
            Next j

            If smoothing Then
                data = SmoothData(tmp)
            Else
                data = tmp
            End If
            Return New Tuple(Of Single(),Single())(data,curv)
        End Function


        Public Overrides Function ToString() As String
            Return "FWS (combined)"
        End Function

        Public ReadOnly Property FWSR As ChannelData_Hardware
            Get
                Return _FWSR
            End Get
        End Property


        Public ReadOnly Property FWSL As ChannelData_Hardware
            Get
                Return _fwsl
            End Get
        End Property
        End Class

    <Serializable()> Public Class ChannelData_Curvature
        Inherits ChannelData

        ' Construct on Deserialization. These should never be used, so I could simply init everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' Not the most efficient way, every subclass loops through the list of available properties, but should only be ever used looking
        ' for very old files.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info,context)

            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_FWS_curv_combined" 
                        _FWS_curv_combined = DirectCast(current.Value, ChannelData_FWSCurvature)
                End Select
            End While
        End Sub

        Private _FWS_curv_combined As ChannelData_FWSCurvature

        Public Sub New(cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ch As ChannelData_FWSCurvature, info As CytoSense.CytoSettings.VirtualChannelInfo, timeOfArrival As Single)
            MyBase.New(cytosettings, timeOfArrival)
            _channelInformation = info
            _FWS_curv_combined = ch
        End Sub


        Public Overrides ReadOnly Property Data As Single()
            Get
                Return _FWS_curv_combined.Curvature()
            End Get
        End Property

        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
        Get
            Return _FWS_curv_combined.CurvatureUnsmoothed()
        End Get
        End Property

        Public Overrides Function ToString() As String
            Return "Curvature"
        End Function
    End Class

    <Serializable()> Public Class ChannelData_ratioChannel
        Inherits ChannelData
        ' Construct on Deserialization. These should never be used, so I could simply init everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' Not the most efficient way, every subclass loops through the list of available properties, but should only be ever used looking
        ' for very old files.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info,context)

            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_data" 
                        _data = DirectCast(current.Value, Single())
                    Case "_operation" 
                        _operation = DirectCast(current.Value, channelOperator)
                    Case "_firstChannel" 
                        _firstChannel = DirectCast(current.Value, ChannelData)
                    Case "_secondChannel" 
                        _secondChannel = DirectCast(current.Value, ChannelData)
                End Select
            End While
        End Sub

        Protected Friend _data() As Single
        Public Shared Operators As String() = {"/", "*", "+", "-", "(-)^2"}
        Public _operation As channelOperator
        Private _firstChannel As ChannelData
        Private _secondChannel As ChannelData

        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, ByRef firstChannel As ChannelData, ByRef secondChannel As ChannelData, operation As channelOperator)
            MyBase.New(cytosettings, firstChannel.TimeOfArrival)
            _operation = operation
            _firstChannel = firstChannel
            _secondChannel = secondChannel

            ' init name, color, etc
            _channelInformation = New CytoSettings.channel(firstChannel.Information.name & " " & Operators(operation) & " " & secondChannel.Information.name, Color.Chartreuse)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Not cached because it should not be used often.</remarks>
        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
            Get
                Return calcData(_firstChannel.Data_mV_unsmoothed, _secondChannel.Data_mV_unsmoothed)
            End Get
        End Property

        Public Overrides ReadOnly Property Data As Single()
            Get
                If _data Is Nothing Then
                    Dim tmp =calcData(_firstChannel.Data, _secondChannel.Data)
                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tmp
                    End If
                    _data = tmp
                End If
                Return _data
            End Get
        End Property


        Private Function calcData(channel1() As Single, channel2() As Single) As Single()
            Dim data(channel1.Length - 1) As Single

            Select Case _operation
                Case channelOperator.division
                    For i As Integer = 0 To data.Length - 1
                        If _secondChannel.Data(i) <> 0 Then
                            data(i) = channel1(i) / channel2(i)
                        Else
                            data(i) = 0
                        End If
                    Next
                Case channelOperator.difference
                    For i As Integer = 0 To data.Length - 1
                        data(i) = channel1(i) - channel2(i)
                    Next
                Case channelOperator.sum
                    For i As Integer = 0 To data.Length - 1
                        data(i) = channel1(i) + channel2(i)
                    Next
                Case channelOperator.product
                    For i As Integer = 0 To data.Length - 1
                        data(i) = channel1(i) * channel2(i)
                    Next
                Case channelOperator.squaredDifference
                    For i As Integer = 0 To data.Length - 1
                        Dim y As Single = channel1(i) - channel2(i)
                        data(i) = y * y
                    Next
            End Select

            Return data
        End Function
        Public Enum channelOperator
division = 0
            product = 1
            sum = 2
            difference = 3
            squaredDifference = 4
        End Enum

        Public Overrides Function ToString() As String
            Return "Ratio: " & _firstChannel.ToString & Operators(_operation) & _secondChannel.ToString
        End Function
    End Class

    <Serializable()> Public Class ChannelData_DualFocus
        Inherits ChannelData

        ' Construct on Deserialization. These should never be used, so I could simply init everything to default, and load nothing
        ' from disk, but that seems quite bad, not sure what that would do to reference counts, and if it could result in silent failure
        ' later on. So lets load the data and convert doubles to singles.
        ' Not the most efficient way, every subclass loops through the list of available properties, but should only be ever used looking
        ' for very old files.
        Public Sub New(info As SerializationInfo, context As StreamingContext)
            MyBase.New(info,context)

            Dim enumerator = info.GetEnumerator()
            While(enumerator.MoveNext())
                Dim current = enumerator.Current()
                Select(current.Name)
                    Case "_baseChannel" 
                        _baseChannel = DirectCast(current.Value, ChannelData_Hardware)
                    Case "_dualParameters" 
                        _dualParameters = DirectCast(current.Value, Single()())
                End Select
            End While
        End Sub

        Private _baseChannel As ChannelData_Hardware
        Protected Friend _dualParameters(1)() As Single

        Public Sub New(cytosettings As CytoSettings.CytoSenseSetting, ByRef FLRED As ChannelData_Hardware)
            MyBase.New(cytosettings, FLRED.TimeOfArrival)
            'Things from the FLRED channel can be copied to this virtual channel
            _baseChannel = FLRED
        End Sub

        ''' <summary>
        ''' Returns parameters of the whole (unsplit) channel. Use an extra left/right-argument for splitted parameters.
        ''' </summary>
        ''' <param name="k"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property Parameter(ByVal k As ChannelData_Hardware.ParameterSelector) As Single
            Get
                Return _baseChannel.Parameter(k)
            End Get
        End Property

        Public Overloads ReadOnly Property Parameter(left_right As LeftOrRightHalf, ByVal k As ChannelData_Hardware.ParameterSelector) As Single
            Get
                If Object.Equals(_dualParameters(0), Nothing) Then
                    ReDim _dualParameters(0)(ParameterNames.Length - 1)
                    ReDim _dualParameters(1)(ParameterNames.Length - 1)
                    For i = 0 To _dualParameters(0).Length - 1
                        _dualParameters(0)(i) = Single.NegativeInfinity
                        _dualParameters(1)(i) = Single.NegativeInfinity
                    Next
                End If

                If Single.IsNegativeInfinity(_dualParameters(left_right)(k)) Then
                    _dualParameters(left_right)(k) = GetDualParameter(k, left_right)
                End If
                Return _dualParameters(left_right)(k)
            End Get
        End Property

        Private Function GetDualParameter(ByVal k As ParameterSelector, ByVal left_right As LeftOrRightHalf) As Single
            'Split the curvdata

            Dim halfLength As Integer = CInt(Math.Floor(Data.Length / 2))
            Dim halfdata(halfLength - 1) As Single
            'get the parameter over half the data
            If left_right = LeftOrRightHalf.LeftHalf Then
                Array.ConstrainedCopy(Data, 0, halfdata, 0, halfLength)
            Else
                Array.ConstrainedCopy(Data, halfLength, halfdata, 0, halfLength)
            End If

            Dim par As Single = GetParameter(k, halfdata, _dualParameters(left_right))

            Return par
        End Function

        'Maybe try something with cross-correlation?

        Public Enum LeftOrRightHalf
            LeftHalf = 0
            RightHalf = 1
        End Enum

        ''' <summary>
        ''' Returns the unsmoothed data of the whole referenced FLRED-channel
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property Data_mV_unsmoothed() As Single()
            Get
                Return _baseChannel.Data_mV_unsmoothed
            End Get
        End Property

        Public Overrides ReadOnly Property Data As Single()
            Get
                Return _baseChannel.Data
            End Get
        End Property

        ''' <summary>
        ''' Information of the original FLRED-channel
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property Information As CytoSettings.channel
            Get
                Return New CytoSettings.channel("FL Red Split", Color.OrangeRed)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _baseChannel._channelInformation.Description
        End Function
    End Class

    Public Class ChannelData_DualFocusHalf
        Inherits ChannelData

        Private _DualFocusChannel As ChannelData_DualFocus
        Private _half As ChannelData_DualFocus.LeftOrRightHalf

        Public Sub New(cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ch As ChannelData_DualFocus, info As CytoSense.CytoSettings.VirtualChannelInfo, half As ChannelData_DualFocus.LeftOrRightHalf)
            MyBase.New(cytosettings, ch.TimeOfArrival)
            _channelInformation = info
            _DualFocusChannel = ch
            _half = half
        End Sub

        Public Overrides ReadOnly Property Parameter(k As ChannelData.ParameterSelector) As Single
            Get
                Return _DualFocusChannel.Parameter(_half, k)
            End Get
        End Property

        Public Overrides ReadOnly Property Data As Single()
            Get
                Return _DualFocusChannel.Data
            End Get
        End Property

        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
            Get
                Return _DualFocusChannel.Data_mV_unsmoothed
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _channelInformation.Description
        End Function



    End Class

    ''' <summary>
    ''' Virtual channel for the SSL system (software subtract laser system). This channel contains the signal of the HF + LF laser. 
    ''' In order to get only the HF laser signal, the LF signal is subtracted
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ChannelData_HFSubtractChannel
        Inherits ChannelData

        Private _Ch_HF As ChannelData_Hardware
        Private _Ch_LF As ChannelData_Hardware


        Public Sub New(cytosettings As CytoSense.CytoSettings.CytoSenseSetting, Ch_HF As ChannelData_Hardware, Ch_LF As ChannelData_Hardware, info As CytoSense.CytoSettings.VirtualHFSummedChannelInfo)
            MyBase.New(cytosettings, Ch_HF.TimeOfArrival)
            _channelInformation = info
            _Ch_HF = Ch_HF
            _Ch_LF = Ch_LF
        End Sub

        Public ReadOnly Property VirtualInformation As CytoSettings.VirtualHFSummedChannelInfo
            Get
                Return CType(_channelInformation, CytoSettings.VirtualHFSummedChannelInfo)
            End Get
        End Property

        Private _data As Single()
        Public Overrides ReadOnly Property Data As Single()
            Get
                If Object.Equals(Nothing, _data) Then 'on demand
                    'One of the channels contains both LF and HF signal, the other only LF. To make it only HF, do HF-LF.
                    Dim tmp As Single()
                    ReDim tmp(_Ch_HF.Data.Length - 1)
                    For i = 0 To _Ch_HF.Data.Length - 1
                        tmp(i) = _Ch_HF.Data(i) - _Ch_LF.Data(i)
                    Next
                    If DO_NOT_CACHE_PULSE_SHAPES Then
                        Return tmp
                    End If
                    _data = tmp
                    ' - If the summed channel saturates, we lose information on the level of both the HF and LF signal
                End If
                Return _data
            End Get
        End Property

        Private _data_unsmoothed As Single()
        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
            Get
                If Object.Equals(Nothing, _data_unsmoothed) Then 'on demand
                    'One of the channels contains both LF and HF signal. To make it only HF, do HF-LF.


                    ReDim _data_unsmoothed(_Ch_HF.Data.Length - 1)
                    For i = 0 To _Ch_HF.Data.Length - 1
                        _data_unsmoothed(i) = _Ch_HF.Data_mV_unsmoothed(i) - _Ch_LF.Data_mV_unsmoothed(i)
                    Next

                End If
                Return _data_unsmoothed
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _channelInformation.name
        End Function
    End Class


    ''' <summary>
    ''' Virtual channel for the SSL system (software subtract laser system). This channel contains the already filtered LF laser signal. 
    ''' In principle no extra calculations are needed to get the LF signal, but mainly for clarity and in the future handling of extreme (clipping situations) still a special 
    ''' virtual channel is available
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ChannelData_LFFilteredChannel
        Inherits ChannelData

        Private _Ch_HF As ChannelData_Hardware
        Private _Ch_LF As ChannelData_Hardware


        Public Sub New(cytosettings As CytoSense.CytoSettings.CytoSenseSetting, Ch_LF As ChannelData_Hardware, Ch_HF As ChannelData_Hardware, info As CytoSense.CytoSettings.VirtualLFFilteredChannelInfo)
            MyBase.New(cytosettings, Ch_HF.TimeOfArrival)
            _channelInformation = info
            _Ch_LF = Ch_LF
            _Ch_HF = Ch_HF
        End Sub

        Public ReadOnly Property VirtualInformation As CytoSettings.VirtualLFFilteredChannelInfo
            Get
                Return CType(_channelInformation, CytoSettings.VirtualLFFilteredChannelInfo)
            End Get
        End Property

        Public Overrides ReadOnly Property Data As Single()
            Get
                Return _Ch_LF.Data
            End Get
        End Property

        Public Overrides ReadOnly Property Data_mV_unsmoothed As Single()
            Get

                Return _Ch_LF.Data_mV_unsmoothed
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return _channelInformation.Description
        End Function
    End Class


End Namespace