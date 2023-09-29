Namespace DSP

    <Serializable()> Public Class DSPParticles

        Dim _DSPParticle() As DSPParticle
        Dim nParts As Int32 = 0


        Dim _cytoSettings As CytoSettings.CytoSenseSetting
        Dim _images As IIFImage()

        Protected Friend _unmatchedImages As List(Of DSPParticle) 'Images which cannot be matched to a splitted particle
        Protected Friend _matchedImages As List(Of DSPParticle) 'Images which were successfully matched to a splitted particle, with that particle's index

        ''' <summary>
        ''' This class will contain only particles from which the IIF DSP has determined that they fall within
        ''' the selectionset as specified by the IIF parameters
        ''' </summary>
        ''' <param name="CytoSettings">Each dspparticle needs some information about the machine which was used. For instance; numberOfChannels</param>
        Public Sub New(ByVal CytoSettings As CytoSettings.CytoSenseSetting)
            ReDim _DSPParticle(1000)
            nParts = 0
            _cytoSettings = CytoSettings

        End Sub

        ''' <summary>
        ''' The images which could be matched to a splitted particle. Does not always return a value!
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property matchedImages As List(Of DSPParticle)
            Get
                Return _matchedImages
            End Get
        End Property



        Public Sub New(ByVal DSPParticles As DSPParticle(), ByVal CytoSettings As CytoSettings.CytoSenseSetting)
            nParts = DSPParticles.Length
            _DSPParticle = DSPParticles
            _cytoSettings = CytoSettings
        End Sub

        Private Sub addDataMatch(ByVal D As DSPParticle)
            If nParts >= _DSPParticle.Length Then
                ReDim Preserve _DSPParticle(nParts * 2)
            End If
            _DSPParticle(nParts) = D
            nParts += 1

        End Sub

        Public Sub addParticle(ByVal p As DSPParticle)
            addDataMatch(p)
        End Sub

        ''' <summary>
        ''' Provides acces to matched particle, including an Image if found
        ''' </summary>
        ''' <param name="index">Index of particle to be retrieved</param>
        ''' <param name="onlyWithImageSucces">Filters out particles without an image matched to it. Param index is also influenced by this. Maximum index is getNumberOfSucceses</param>
        ''' <returns></returns>
        ''' <remarks>Pay attention to use getNumberOfSucceses as length instead of this property.length, when onlyWithImage = true</remarks>
        Public ReadOnly Property particles(ByVal index As Integer, ByVal onlyWithImageSucces As Boolean) As DSPParticle
            Get
                If index >= 0 Then
                    If onlyWithImageSucces Then
                        Dim length As Integer = Me.Length
                        Dim count As Integer = -1
                        For i = 0 To length - 1
                            If _DSPParticle(i).Succes Then
                                count += 1
                            End If
                            If count = index Then
                                Return _DSPParticle(i)
                            End If
                        Next
                        Throw New IndexOutOfRangeException

                    Else
                        Return _DSPParticle(index)
                    End If

                Else
                    Return Nothing
                End If
            End Get
        End Property

        ''' <summary>Number of particles that were (data)matched </summary>
        Public ReadOnly Property Length() As Int32
            Get
                Return nParts
            End Get
        End Property


        ''' <summary>
        ''' Calculates number of ImageMatched particles. Therefor, datamatch particles without an image matched are not counted.
        ''' </summary>
        Public Function getNumberOfSucceses() As Integer
            Dim tot As Integer = 0
            For i = 0 To nParts - 1
                If _DSPParticle(i).Succes Then
                    tot += 1
                End If
            Next
            Return tot

        End Function

        ''' <summary>
        ''' Solves memory issues with Image
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveImages_PlaceStreams()


            For i = 0 To Length - 1
                _DSPParticle(i).RemoveImages_PlaceStreams()
            Next

            ReDim Preserve _DSPParticle(Length - 1)
        End Sub


    End Class



End Namespace