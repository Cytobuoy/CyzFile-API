Imports CytoSense.Data.ParticleHandling
Imports CytoSense.Data.ParticleHandling.Channel

Namespace Data

    Public Module RawDataHandling
        ''' <summary>
        '''Input:  A array of bytes
        '''Output: A jagged array containing the pulse data for each separate channel
        '''Output data is separated by two rows of -255
        '''Throws a descriptive exception if no sinc sequences are found
        ''' </summary>
        ''' <param name="rdbytes"></param>
        ''' <param name="nChannels">the number of fluorescence data channels</param>
        ''' <param name="particleCount"></param>
        ''' <param name="totChannels">the total no. of channels</param>
        ''' <param name="ends"></param>
        ''' <param name="starts"></param>
        ''' <param name="DSPChnNo"></param>
        ''' <param name="imgdata">(optionally) specifies a list of bytes corresponding to the raw data of imaged and matched particles</param>
        ''' <param name="emptyChn"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function splitData(ByVal rdbytes() As Byte, ByVal nChannels As Integer, ByRef particleCount As Integer, ByVal totChannels As Integer, ByRef ends As List(Of Integer), ByRef starts As List(Of Integer), Optional ByVal DSPChnNo As Integer = 0, Optional ByVal imgdata As List(Of Byte) = Nothing, Optional ByVal emptyChn As List(Of Integer) = Nothing) As Int16()()
            ' Input:  A array of bytes
            ' Output: A jagged array containing the pulse data for each separate channel
            ' Output data is separated by two rows of -255
            ' Throws a descriptive exception if no sinc sequences are found
            ' totChannels is the total no. of channels
            ' nChannels is the number of fluorescence data channels
            ' imgdata (optional) specifies a list of bytes corresponding to the raw data of imaged and matched particles

            'Determine cutoff point
            Dim cutoff As Integer = findFirstSINC(rdbytes, totChannels, nChannels, DSPChnNo)

            Dim emptyChannels As New List(Of Integer)
            If Not emptyChn Is Nothing Then
                emptyChannels = emptyChn
            End If

            Dim imagedata As New List(Of Byte)
            If Not imgdata Is Nothing Then
                imagedata = imgdata
            End If

            starts.Clear()
            ends.Clear()

            Dim i As Integer = 0, j As Integer = 0
            Dim Bytes As Byte()

            If cutoff = -1 Then '   Add leading "<128"
                ReDim Bytes(rdbytes.Length + imagedata.Count)
                Bytes(0) = 0
                For i = 0 To rdbytes.Length - 1
                    Bytes(i + 1) = rdbytes(i)
                Next i
                ' Append image in flow data
                For j = 0 To imagedata.Count - 1
                    Bytes(i + 1 + j) = imagedata(j)
                Next
            Else    '   Remove garbage before 1st SINC
                ReDim Bytes(rdbytes.Length - cutoff + imagedata.Count)
                For i = cutoff To rdbytes.Length - 1
                    Bytes(i - cutoff) = rdbytes(i)
                Next i
                ' Append image in flow data
                i -= cutoff
                For j = 0 To imagedata.Count - 1
                    Bytes(i + 1 + j) = imagedata(j)
                Next
            End If

            Dim data(totChannels)() As Int16


            Dim tel% = 0, k% = 0, l% = 0, m% = 0
            j = 0
            For i = 0 To totChannels 'Allocate memory for channels
                ReDim data(i)(CInt(UBound(Bytes) / (totChannels + 1) - 1))
            Next i

            'write channel data to separate arrays (jagged)

            Dim skip As Integer = totChannels + 1
            Dim sinc() As Integer = {130, 132, 134, 136, 138, 140, 142, 144, 146, 148, 150, 152, 154, 156, 158}

            Dim ChannelOffsets(totChannels - 1) As Integer
            If DSPChnNo <> 0 Then ReDim ChannelOffsets(ChannelOffsets.Length - 2)
            If totChannels > 7 Then ReDim ChannelOffsets(ChannelOffsets.Length - 2)
            ' Subtract empty (dummy) channels
            ReDim ChannelOffsets(ChannelOffsets.Length - 1 - emptyChannels.Count)
            If totChannels >= 14 Then
                'Throw New Exception("Code should be checked before using more than 13 channels")
                ReDim ChannelOffsets(ChannelOffsets.Length - 2) 'Check this one!
            End If

            ' Define offsets from "<128" to the other channel delimiters
            j = 1
            For i = 0 To ChannelOffsets.Length - 1
                If Not (j = DSPChnNo Or j = 8 Or j = 14 Or emptyChannels.Contains(j)) Then
                    ChannelOffsets(i) = j
                    j += 1
                Else
                    j += 1
                    i -= 1
                End If
            Next

            't.tic("Start splitting")
            '   FIND STARTS AND ENDS OF SINC SEQUENCES
            Dim sincstart, sincend As Integer
            Dim cnt As Integer
            Try
                'A SINC is recognised if at least one of the 5 characteristic series is present

                For i = 0 To Bytes.Length - 1 - skip

                    If Bytes(i) = 130 And Bytes(i + 1) = 132 Then
                        'Is this a SINC pulse?
                        cnt = 0
                        For j = 1 To ChannelOffsets.Length - 1
                            If Bytes(i + ChannelOffsets(j) - 1) = sinc(ChannelOffsets(j) - 1) Then
                                cnt += 1
                            End If
                        Next
                        'For a complete SINC, cnt should equal channelOffsets.length
                        If cnt = ChannelOffsets.Length - 1 Then
                            'This is a SINC sequence

                            sincstart = i - 1
                            sincend = i + skip - 1

                            'Check for SINC sequences before this one
                            k = i
                            Do
                                k -= skip
                                If k < 0 Then Exit Do
                                cnt = 0
                                For j = 0 To ChannelOffsets.Length - 1
                                    If Bytes(k + ChannelOffsets(j) - 1) = sinc(ChannelOffsets(j) - 1) Then
                                        cnt += 1
                                    End If
                                Next
                                If cnt >= ChannelOffsets.Length - 2 Then
                                    'Allows for one corrupted value
                                    sincstart = k - 1
                                Else
                                    Exit Do
                                End If
                            Loop

                            'Check for SINC sequence after this one
                            k = sincstart + 2 * skip + 1
                            Do
                                k += skip
                                If k > Bytes.Length - 1 - skip Then Exit Do
                                cnt = 0
                                For j = 0 To ChannelOffsets.Length - 1
                                    If Bytes(k + ChannelOffsets(j) - 1) = sinc(ChannelOffsets(j) - 1) Then
                                        cnt += 1
                                    End If
                                Next
                                If cnt >= ChannelOffsets.Length - 2 Then
                                    'Allows for one corrupted value
                                    sincend = k - 1 + skip
                                Else
                                    Exit Do
                                End If
                            Loop

                            'If all correct; sincstart and sincend contain begin and start of SINC
                            starts.Add(sincstart)
                            ends.Add(sincend)

                            i = sincend
                        End If
                    End If
                Next
            Catch ex As Exception
                Dim e As New Exception("The data file does not seem to contain any separate particles.", ex)
                Throw e
            End Try

again:
            For i = 0 To starts.Count - 2
                If starts(i + 1) - ends(i) < skip Then
                    ends.RemoveAt(i)
                    starts.RemoveAt(i + 1)
                    GoTo again
                End If
            Next
            '   SPLIT THE DATA INTO SEPARATE STREAMS
            For i = 0 To starts.Count - 2

                If Bytes(ends(i)) = 66 And Bytes(ends(i) + 1) = 66 And Bytes(ends(i) + 2) = 66 And Bytes(ends(i) + 3) = 66 And Bytes(ends(i) + 4) = 66 Then
                    i += 1
                    'No data particles anymore?
                End If
                particleCount += 1

                ' write SINC (twice)
                For l = 1 To 2
                    For j = 0 To totChannels
                        If j <> DSPChnNo Then
                            data(j)(tel) = -255
                        End If
                    Next j
                    tel += 1
                Next l


                'Write data

                For j = ends(i) To starts(i + 1) - 1 Step totChannels + 1
                    For l = 0 To totChannels
                        data(l)(tel) = Bytes(j + l)
                    Next l
                    tel += 1
                Next j
            Next

            'Terminate with SINC
            For l = 1 To 2
                For j = 0 To totChannels
                    data(j)(tel) = -255
                Next j
                tel += 1
            Next l

            For j = 0 To totChannels
                ReDim Preserve data(j)(tel - 1)
            Next j

            Return data
        End Function

        ''' <summary>
        ''' Depreciated. Kanaal volgorde zit verkeerd om...
        ''' </summary>
        ''' <param name="MixedParticle"></param>
        ''' <param name="cytosettings"></param>
        ''' <returns></returns>
        ''' <remarks>Depreciated. Komt uit CytoUSB, Liever niet gebruiken dus</remarks>
        Public Function splitData(ByVal MixedParticle As Byte(), ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting) As Byte()()

            Dim outdata(cytosettings.channels.Length - 1)() As Byte
            Dim tel As Int32 = 0
            Dim j As Int32 = 0
            Dim i As Int32 = 0

            ' RVDH - I think this is supposed to work as follows: 
            '
            ' - the MixedParticle buffer contains records with a length of cytosettings.channels.length
            ' - for each channel a record contains a datavalue, or the channels sync value
            ' - the channeldata of a particle is stored between sync values
            ' - the MixedParticle buffer does not have to start with sync records but can start
            '   with data left over from the previous buffer.
            '
            ' The purpose of this function is to split the buffer into arrays with channelData (including sync values)

            For start As Int32 = 0 To MixedParticle.Length - (4 + cytosettings.channels.Length)

                ' RVDH - The IF statement compares against TWO sync records but the MixedParticle buffer does 
                ' Not have to start with two (Or more) sync records.
                ' calculate tel, the position of first record to process which may contain data values or sync values. 

                If MixedParticle(start) = 130 And MixedParticle(start + 1) = 132 And MixedParticle(start + 2) = 134 And MixedParticle(start + cytosettings.channels.Length) = 130 Then
                    start -= CInt(Math.Floor(start / cytosettings.channels.Length) * cytosettings.channels.Length)
                    tel = start
                    Exit For
                End If
            Next

            ' RVDH - because of UBound, the outputdata does not include data from the last record 

            For i = 0 To cytosettings.channels.Length - 1
                ReDim outdata(i)(CInt((UBound(MixedParticle) / (cytosettings.channels.Length) - 1)))
            Next i

            Dim maxMixedParticle = UBound(MixedParticle) - 1

            For j = 0 To CInt(UBound(MixedParticle) / (cytosettings.channels.Length) - 1)

                For i = cytosettings.channels.Length - 1 To 0 Step -1   'anders is de kanaal volgorder verkeerd om
                    Try
                        outdata(i)(j) = MixedParticle(tel)
                        tel = tel + 1
                        If tel > maxMixedParticle Then
                            Exit For
                        End If
                    Catch ex As IndexOutOfRangeException
                        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! niet erg wel langzaam!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        ' Most cases should be handled by the If above, but given the complexity of hte code, and the fact that I do not
                        ' really understand what is happening.  I leave the catch here as a fall back, so if something other then
                        ' rounding can also happen then it will still have the old behavior. 
                    End Try
                Next i

            Next j

            Return outdata

        End Function

        ''' <summary>
        ''' FJ electronics has a completely new data format with different syncs.  Each channel has 4 bytes appended, the first is the 
        ''' sync byte 252, then the channel number, and finally 2 bytes particle ID.  Each channel has 4 bytes, so if the trailer
        ''' is still present we have to drop 4 * numChannel bytes to get the actual data. Also this 252 is never present in the
        ''' actual data.
        ''' </summary>
        ''' <param name="multiplexedParticle"></param>
        ''' <param name="CytoSettings"></param>
        ''' <param name="detectsyncs"></param>
        ''' <returns></returns>
        ''' <remarks>FOR FJ data, we do not actually detect syncs, we simply assume they are there, and remove the data
        ''' from the end (numChannels * 4 bytes).</remarks>
        Public Function SpitChannels(ByVal multiplexedParticle As Byte(), ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, detectsyncs As Boolean, dataConversions() As MeasurementSettings.LogConversion) As ChannelData_Hardware()
            Dim numChannels = CytoSettings.channels.Length
            If CytoSettings.hasFJDataElectronics Then
                Dim channels(numChannels - 1) As ChannelData_Hardware
                Dim numSamples = multiplexedParticle.Length \ CytoSettings.channels.Length

                If detectsyncs Then
                    numSamples -= 4 ' Last 4 are trailer bytes.
                    ' Validate trailer block here perhaps?
                End If
                For channelIdx = 0 To numChannels - 1
                    Dim channel_buffer(numSamples - 1) As Byte
                    For sampleIdx = 0 To numSamples - 1
                        channel_buffer(sampleIdx) = multiplexedParticle((sampleIdx * numChannels) + channelIdx)
                    Next
                    channels(channelIdx) = New ChannelData_Hardware(dataConversions(channelIdx), channel_buffer, CytoSettings.channels(channelIdx), CytoSettings, 0)
                Next
                Return channels
            Else 'old skool Ruud electronics. Syncs are Trigger1,130,132,134...,DSP, Trigger2, ...


                Dim ch_buf(CytoSettings.channels.Length - 1)() As Byte
                For i = 0 To ch_buf.Length - 1
                    ReDim ch_buf(i)(multiplexedParticle.Length \ CytoSettings.channels.Length - 1)
                Next

                For i = 0 To multiplexedParticle.Length \ CytoSettings.channels.Length - 1
                    For j = 0 To CytoSettings.channels.Length - 1
                        ch_buf(j)(i) = multiplexedParticle(i * CytoSettings.channels.Length + j)
                    Next
                Next
                Dim channels(CytoSettings.channels.Length - 1) As ChannelData_Hardware

                If detectsyncs Then
                    Dim foundat As Integer = -1
                    Dim chlength As Integer = ch_buf(0).Length - 1

                    Dim tmp As Double = multiplexedParticle.Length / CytoSettings.channels.Length
                    If Not Math.Round(tmp) = tmp Then
                        Return Nothing
                    End If

                    For i = 0 To ch_buf.Length - 1
                        If ch_buf(i)(chlength - 3) = 130 And ch_buf(i)(chlength - 1) = 130 And ch_buf(i)(chlength - 2) = 130 Then '130... hmmm that is Ruud only.... how to solve this for FJ?
                            foundat = i
                            Exit For
                        End If
                    Next

                    If foundat = -1 Then
                        Return Nothing
                    End If

                    For i = 0 To ch_buf.Length - 1
                        Dim id As Integer = (i + foundat) Mod CytoSettings.channels.Length
                        Dim id_cyt As Integer = (i + 1) Mod CytoSettings.channels.Length ' Ah + 1 because of infamous triggerchannel in Ruud electronics... how to solve this for FJ?

                        Dim sync As Integer = CytoSettings.channels(id_cyt).SyncPulseValue

                        If ch_buf(id)(chlength - 1) = sync Or sync = 0 Then

                            'remove syncs:
                            Dim tmpb(ch_buf(id).Length - 6) As Byte '5 syncs
                            Array.ConstrainedCopy(ch_buf(id), 0, tmpb, 0, tmpb.Length)

                            channels(id_cyt) = New ChannelData_Hardware(dataConversions(id_cyt), tmpb, CytoSettings.channels(id_cyt), CytoSettings, 0)
                        Else
                            Return Nothing
                        End If
                    Next


                Else

                    For i = 0 To channels.Length - 1
                        channels(i) = New ChannelData_Hardware(dataConversions(i), ch_buf(i), CytoSettings.channels(i), CytoSettings, 0)
                    Next

                End If

                Return channels
            End If
        End Function

        ''' <summary>
        ''' THe new FJ format has 4 bytes ate the end per channel. one byte with value 252, one with 
        ''' the channel number, and then the last 2 is the particle number.  They should be the same
        ''' on every channel, so simply taking the last 2 bytes of one channel should be enough.
        ''' Which means the original code simply works. :-)
        ''' </summary>
        ''' <param name="multiplexedParticle"></param>
        ''' <param name="CytoSettings"></param>
        ''' <returns></returns>
        Public Function getParticleIDFromFJSync(ByVal multiplexedParticle As Byte(), ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting) As UInt16
            Dim b0 As Byte = multiplexedParticle(multiplexedParticle.Length - 1)
            Dim b1 As Byte = multiplexedParticle(multiplexedParticle.Length - 1 - CytoSettings.channels.Length)
            Dim res As UInt16 = CUShort(b0 + b1 * 256)
            Return res
        End Function



        ''' <summary>
        ''' Creates particles from data_sp continue mode particle buffer
        ''' </summary>
        ''' <param name="rawParticles">Data_sp from datafile</param>
        ''' <param name="CytoSettings">Needed to create particle information</param>
        ''' <param name="measurement">Needed to create particle information</param>
        ''' <returns>Array of CytoSense particles</returns>
        ''' <remarks></remarks>
        Public Function getParticles(ByVal rawParticles As List(Of CytoSense.Data.SegmentedData.RawParticle), ByRef CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByRef measurement As CytoSense.MeasurementSettings.Measurement, start As DateTime) As Particle()

            Dim p As New List(Of CytoSense.Data.ParticleHandling.Particle)
            For i = 0 To rawParticles.Count - 1
                If rawParticles(i) IsNot Nothing Then
                    p.Add(New Particle(rawParticles(i), CytoSettings, measurement, start))
                End If
            Next


            Dim res As Particle() = p.ToArray

            Return res

        End Function


        ''' <summary>
        ''' Parallel implementation of splitToParticles demultiplex function. Splits the data into nCores chunks and processes those concurrently.
        ''' If nCores is 1, nothing is parallelized and no extra overhead is created.
        ''' </summary>
        ''' <param name="multiplexedData"></param>
        ''' <param name="CytoSettings"></param>
        ''' <param name="measurementInfo"></param>
        ''' <param name="nCores"></param>
        ''' <param name="sets"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function splitToParticles(ByVal multiplexedData As Byte(), ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurementInfo As CytoSense.MeasurementSettings.Measurement, nCores As Integer, Optional ByVal sets As Cluster() = Nothing, Optional ByVal ArrivalTimes As Single() = Nothing) As Particle()
            If nCores = 1 Then
                Return splitToParticles(multiplexedData, CytoSettings, measurementInfo, True, sets)
            Else


                Dim splitpoints(nCores) As UInt32 '1 longer, to hold end point
                For i = 0 To nCores - 1
                    splitpoints(i) = CUInt(i * (multiplexedData.Length / nCores))
                Next
                splitpoints(nCores) = CUInt(multiplexedData.Length)

                For i = 0 To nCores - 1
                    splitpoints(i) = findNextSync(multiplexedData, CInt(splitpoints(i)), CInt(splitpoints(i + 1)), CytoSettings)
                Next

                'split data:
                Dim chunks(nCores - 1)() As Byte
                For i = 0 To nCores - 1
                    Dim endpoint As Integer = CInt(splitpoints(i + 1) - 1 + 5 * CytoSettings.channels.Length)
                    If endpoint > multiplexedData.Length - 1 Then
                        endpoint = multiplexedData.Length
                    End If
                    ReDim chunks(i)(CInt(endpoint - splitpoints(i) - 1))
                    Array.Copy(multiplexedData, splitpoints(i), chunks(i), 0, chunks(i).Length)
                Next

                Dim particles(nCores - 1)() As CytoSense.Data.ParticleHandling.Particle
                Threading.Tasks.Parallel.For(0, nCores, Sub(i)
                                                            particles(i) = splitToParticles(chunks(i), CytoSettings, measurementInfo, False)
                                                        End Sub)

                'set particle ids in order:
                For i = 1 To nCores - 1
                    Dim startid As Integer = particles(i - 1)(particles(i - 1).Length - 1).ID
                    For j = 0 To particles(i).Length - 1
                        particles(i)(j).setID(startid + j + 1)
                    Next
                Next

                'determine amount of particles
                Dim nParts As Integer
                For i = 0 To nCores - 1
                    nParts += particles(i).Length
                Next

                'copy all particles into one 1D array
                Dim res(nParts - 1) As Particle
                Dim counter As Integer = 0
                For i = 0 To nCores - 1
                    Array.Copy(particles(i), 0, res, counter, particles(i).Length)
                    counter += particles(i).Length
                Next

                setclusters(sets, res)

                res = checkParticles(res)
                Return res
            End If
        End Function

        ''' <summary>
        ''' Wrapper function for good old Gijs Part function
        ''' </summary>
        ''' <param name="multiplexedData"></param>
        ''' <param name="n"></param>
        ''' <param name="nMax"></param>
        ''' <param name="CytoSettings"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function findNextSync(ByVal multiplexedData As Byte(), n As Integer, nMax As Integer, ByRef CytoSettings As CytoSense.CytoSettings.CytoSenseSetting) As UInt32
            Dim emptyChannels As New Generic.List(Of Integer)
            Dim DSPchnNO As Integer
            For i = 0 To CytoSettings.channels.Length - 1
                If CytoSettings.channels(i).name.ToUpper.Contains("DSP") Then DSPchnNO = i

                If CytoSettings.channels(i).name.ToUpper.Contains("DUMMY") Then emptyChannels.Add(i)
            Next i

            Dim id As Integer = findFirstSINC(multiplexedData, CytoSettings.channels.Length - 1, CytoSettings.channels.Length - 1, DSPchnNO, n, CUInt(nMax))
            If id < 0 Then
                id = 0
            End If
            Return CUInt(id)
        End Function


        ''' <summary>
        ''' Will split the multiplexedData to particles. Updated to use CytoClus splitting of muxed data
        ''' </summary>
        ''' <param name="multiplexedData"></param>
        ''' <param name="CytoSettings">Takes channel information from CytoSense, and connect it to right channel in each particle</param>
        ''' <returns></returns>
        ''' <remarks>Not parallelized</remarks>
        Private Function splitToParticles(ByVal multiplexedData As Byte(), ByVal CytoSettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal measurementInfo As CytoSense.MeasurementSettings.Measurement, performCheckParticles As Boolean, Optional ByVal sets As Cluster() = Nothing, Optional ArrivalTimes As Single() = Nothing) As Particle()
            _log.DebugFormat("splitToParticles(multiplexedData.Length={0}, performCheckParticles={1})", multiplexedData.Length, performCheckParticles)

            Dim particleCount As Integer
            Dim ends As New Generic.List(Of Integer)
            Dim starts As New Generic.List(Of Integer)

            Dim emptyChannels As New Generic.List(Of Integer)
            Dim imagedata As New List(Of Byte)

            Dim DSPchnNO As Integer
            For i = 0 To CytoSettings.channels.Length - 1
                If CytoSettings.channels(i).name.ToUpper.Contains("DSP") Then DSPchnNO = i

                If CytoSettings.channels(i).name.ToUpper.Contains("DUMMY") Then emptyChannels.Add(i)
            Next i

            Dim Gdata()() As Int16 = splitData(multiplexedData, CytoSettings.channels.Length - 1, particleCount, CytoSettings.channels.Length - 1, ends, starts, DSPchnNO, imagedata, emptyChannels)
            _log.InfoFormat("splitData returned: starts.Count={0}, ends.Count={1}", starts.Count, ends.Count)
            If _log.IsDebugEnabled AndAlso ends.Count = 0 Then
                Dim format = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\My CytoSense\Datafiles\" + "datablock_{0}.bin"
                Dim ctr = 0
                Dim dumpName = String.Format(format, ctr)
                While IO.File.Exists(dumpName)
                    ctr += 1
                    If ctr > 999 Then
                        ctr = 0
                        dumpName = String.Format(format, ctr)
                        Exit While
                    End If
                    dumpName = String.Format(format, ctr)
                End While
                Using dumpFile = System.IO.File.Open(dumpName, IO.FileMode.Create)
                    dumpFile.Write(multiplexedData, 0, multiplexedData.Length)
                End Using
            End If
            Dim p(ends.Count - 2) As Particle
            Dim count As Integer = 0

            For i = 0 To Gdata(1).Length - 3
                If Gdata(1)(i) = -255 Then
                    For j = i + 2 To Gdata(1).Length - 1
                        If Gdata(1)(j) = -255 Then

                            Dim chData(Gdata.Length - 1) As ChannelData_Hardware

                            For k = 0 To Gdata.Length - 1
                                Dim ch(j - i - 3) As Byte
                                For l = i + 2 To j - 1
                                    ch(l - i - 2) = CByte(Gdata(k)(l))
                                Next
                                chData(k) = New ChannelData_Hardware(measurementInfo.ChannelDataConversion(k), ch, CytoSettings.channels(k), CytoSettings, 0)


                            Next

                            p(count) = New Particle(chData, count, CytoSettings, measurementInfo)
                            i += p(count).ChannelData_Hardware(0).Data_Raw.Length + 1
                            count += 1

                            Exit For

                        End If
                    Next
                End If
            Next

            setclusters(sets, p)

            If performCheckParticles Then
                Return checkParticles(p)
            Else
                Return p
            End If


        End Function
        Private Sub setclusters(sets As Cluster(), p As Particle())
            If Not Object.Equals(Nothing, sets) Then
                For i = 0 To sets.Length - 1
                    For j = 0 To sets(i).partIDs.Length - 1
                        If sets(i).partIDs(j) < p.Length And sets(i).partIDs(j) >= 0 Then
                            p(sets(i).partIDs(j)).ClusterInfo = sets(i)
                        End If
                    Next
                Next
            End If
        End Sub

        Private Function checkParticles(p As Particle()) As Particle()
            Dim res As New List(Of Particle)
            For i = 0 To p.Length - 1
                'Only add valid particles to the new list
                If checkParticle(p(i)) AndAlso Not Object.Equals(p(i), Nothing) Then
                    res.Add(p(i))
                End If

            Next

            Return res.ToArray
        End Function


        Private _log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        ''' <summary>
        ''' This function performs 3 checks: 
        '''    i) a particle must be at least 4 samples. 
        '''    ii) a particle may not contain loose sync plateaus. 
        '''    iii) a particle may not contain any channel which starts with a sync
        ''' I made the last check a little stronger. It now checks if there are at least 3 channels that have
        ''' a sync value in the first 3 samples or the last 3 samples.  The original check resulted in quite a
        ''' large number of false positive in certain files.
        ''' </summary>
        ''' <param name="p"></param>
        ''' <returns></returns>
        ''' <remarks>Only did very simple optimizations, storing everything that is used more then once in a local 
        ''' variable instead of going through all properties and array indexes each time.  This made my test 
        ''' (checking 457000 particles) go from 16.67 to 1.83 seconds. 
        ''' Currently
        ''' For Each channel
        '''      -  For each sample value
        '''         - For i =0 to 3.
        '''  So each sample value is looked at 4 times.  These are all close together, there
        '''  should be a lot in the chance, but still. If we do it only once, that should 
        '''  save us some time.
        '''  Instead of looking at 4 values each time, we could simply look at each byte only
        '''  once, keeping track of how big the sequence is, and once we find 4 we have the 
        '''  plato, if we find a value that does not match, we reset the counter to 0.
        '''  A next step, since we are looking for a sequence of 4 in a row that is usually
        '''  not present, is to not look at every byte, but skip forward 4 each time we do not
        '''  have a match, and then when we do have a match we do something complicated. 
        '''  Because most particles will not contain this data, this stepping 4 ahead each time
        ''' (very loosely based on Boyer Moore strings earch, but much simpler since we are looking
        ''' looking for sequence of 4 similar characters) May in theory give us another factor of 4.
        '''  4 is the theoretical max, if the synch pulse value never occurred.
        ''' The original implementation, when loading a 450000 particle file, checkParticle was 
        ''' responsible for 9990 ms and the current one, is responsible for 910 ms, so less then a 
        ''' second. There is still room for improvement, but for now there is more to gain in 
        ''' other locations. I think.
        '''  </remarks>
        Public Function checkParticle(p As CytoSense.Data.ParticleHandling.Particle) As Boolean

            If p.Length < 4 Then
                Return False
            End If

            Dim hwChannels = p.ChannelData_Hardware
            Dim numChannels = hwChannels.Length

            Dim altarofdeath As Integer = 0
            For i = 0 To numChannels - 1
                Dim curChannel = hwChannels(i)
                If curChannel.Information.visible Then
                    Dim curSyncPulseValue = curChannel.Information.SyncPulseValue
                    Dim rawData = curChannel.Data_Raw
                    Dim rawDataLength = rawData.Length
                    Dim synCtr = 0  ' Keeps track of how many sync values we saw in a row, once it reaches 4 we found a syncplato, and we can abort the search.
                    For j = 0 To rawDataLength + 4
                        If rawData(j Mod (rawDataLength - 1)) = curSyncPulseValue Then
                            synCtr += 1
                        Else
                            synCtr = 0
                        End If
                        If synCtr = 4 Then ' if 4 or more of these sync plateaus are found, conclude a fault
                            altarofdeath += 1
                            Exit For
                        End If
                    Next
                End If
            Next
            If altarofdeath > 3 Then
                '_log.WarnFormat("Reject because of sync plato: {0}", altarofdeath)
                Return False
            End If

            Dim stairwaytoheaven As Integer = 0
            For i = 1 To numChannels - 1
                Dim curChannel = hwChannels(i)
                If curChannel.Information.visible Then
                    Dim curSyncPulseValue = curChannel.Information.SyncPulseValue
                    Dim rawData = curChannel.Data_Raw
                    For j = 0 To 3
                        If rawData(j) = curSyncPulseValue Then
                            stairwaytoheaven += 1
                            Exit For
                        End If
                    Next
                End If
            Next
            If stairwaytoheaven >= 3 Then
                '_log.WarnFormat("Reject because of sync start: {0}", stairwaytoheaven)
                Return False
            End If

            Dim stairwaytohell As Integer = 0
            For i = 1 To numChannels - 1
                Dim curChannel = hwChannels(i)
                If curChannel.Information.visible Then
                    Dim curSyncPulseValue = curChannel.Information.SyncPulseValue
                    Dim rawData = curChannel.Data_Raw
                    Dim rawDataLength = rawData.Length
                    For j = 0 To 3
                        If rawData(rawDataLength - j - 1) = curSyncPulseValue Then
                            stairwaytohell += 1
                            Exit For
                        End If
                    Next
                End If
            Next
            If stairwaytohell >= 3 Then
                '_log.WarnFormat("Reject because of sync end: {0}", stairwaytohell)
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Good old Gijs part function...
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="totchannels"></param>
        ''' <param name="nchannels"></param>
        ''' <param name="DSPChnNo"></param>
        ''' <param name="starti"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function findFirstSINC(ByVal data As Byte(), ByVal totchannels As Integer, ByVal nchannels As Integer, Optional ByVal DSPChnNo As Integer = 0, Optional starti As Integer = 0, Optional maxi As UInt32 = 1000000) As Integer
            'Finds the index to the first COMPLETE SINC sequence in the file
            Dim cutoff As Integer = 0
            Dim i, cnt As Integer
            Dim limit As UInteger
            Dim skip As Integer = totchannels + 1
            Dim sinc() As Integer = {130, 132, 134, 136, 138, 140, 142, 144, 146, 148, 150, 152, 154, 156, 158}

            Dim ChannelOffsets(nchannels - 1) As Integer
            If DSPChnNo <> 0 Then ReDim ChannelOffsets(ChannelOffsets.Length - 2)
            If nchannels > 7 Then ReDim ChannelOffsets(ChannelOffsets.Length - 2)
            If nchannels > 14 Then
                MsgBox("HELP!")
                ReDim ChannelOffsets(ChannelOffsets.Length - 2) 'Check this one!
            End If

            ' Define offsets from "<128" to the other channel delimiters
            Dim j As Integer = 1
            For i = 0 To ChannelOffsets.Length - 1
                If Not (j = DSPChnNo Or j = 8) Then
                    ChannelOffsets(i) = j
                    j += 1
                Else
                    j += 1
                    i -= 1
                End If
            Next

            Try
                If UBound(data) <= maxi + starti Then limit = CUInt(UBound(data)) Else limit = CUInt(maxi + starti)
                For i = starti To CInt(limit - (totchannels + 3))
                    If data(i) = 130 And data(i + 1) = 132 Then
                        'Is this a SINC pulse?
                        cnt = 0
                        For j = 1 To ChannelOffsets.Length - 1
                            If data(i + ChannelOffsets(j) - 1) = sinc(ChannelOffsets(j) - 1) Then
                                cnt += 1
                            End If
                        Next
                        'For a complete SINC, cnt should equal channelOffsets.length
                        If cnt = ChannelOffsets.Length - 1 Then
                            'This is a SINC sequence
                            cutoff = i - 1
                            Exit For
                        End If
                    End If
                Next
                Return cutoff
            Catch ex As Exception
                Dim e As New Exception("The data file does not seem to contain any separate particles.", ex)
                Throw e
            End Try
        End Function


    End Module


End Namespace