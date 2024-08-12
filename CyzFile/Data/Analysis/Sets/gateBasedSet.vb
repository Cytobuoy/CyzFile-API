Imports System.Drawing
Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis

    <Serializable()> Public Class gateBasedSet
        Inherits CytoSet

        Private gatesUpdated As Boolean = False ' Not used anymore, but unsure if it can remove because it is serialized
        Private myGates As GateCollection

        <NonSerialized>
        Private _testSingleParticleResult As TestSingleParticleResultEnum = TestSingleParticleResultEnum.not_known

        'RVDH This event is not used anymore but workspaces fail to load if removed
        <NonSerialized>
        Public Event indexesChangedEvent(listId As Integer)

        ''' <summary>
        ''' Needed by XML (de)serializer code
        ''' </summary>
        Public Sub New()
            MyBase.New("XML deserialized set", cytoSetType.gateBased, Color.Red)
            myGates = New GateCollection
        End Sub

        Public Sub New(ByVal name As String, ByVal myColor As Color)
            MyBase.New(name, cytoSetType.gateBased, myColor)
            myGates = New GateCollection
        End Sub

        Public Sub New(ByVal name As String, ByVal myColor As Color, ByVal datafile As CytoSense.Data.DataFileWrapper)
            MyBase.New(name, cytoSetType.gateBased, myColor, datafile)
            myGates = New GateCollection
        End Sub

        Public Sub New(ByVal name As String, ByVal myColor As Color, ByVal datafile As CytoSense.Data.DataFileWrapper, listId As Integer, vis As Boolean)
            MyBase.New(name, cytoSetType.gateBased, myColor, datafile, listId, vis)
            myGates = New GateCollection
        End Sub

        ''' <summary>
        ''' Copy Constructor
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New(other As gateBasedSet)
            Me.New(other.Name, other.ColorOfSet, other.Datafile)
            For Each cgate As IGate In other.allGates
                AddGate(cgate.CreateWorkfileCopy(other.Datafile))
            Next
        End Sub

        Public Sub New(dfw As DataFileWrapper, other As gateBasedSet)
            Me.New(other.Name, other.ColorOfSet, dfw, other.ListID, other.Visible)

            For Each gate As IGate In other.allGates
                AddGate(gate)
            Next
        End Sub

#Region "Indexes calculation"

        Public Overrides Property Invalid As Boolean
            Get
                For Each gate In myGates
                    _Invalid = _Invalid Or gate.Invalid
                Next

                Return _Invalid
            End Get

            Set(value As Boolean)
                _Invalid = value
            End Set
        End Property

        ''' <summary>
        ''' Calculate ParticleIndices while taking into account ExclusiveSet mode
        ''' NOTE: This function adjusts a previous calculated _ParticleIndices value
        '''       that should contain the non-exclusive particleIndices
        ''' </summary>
        Public Sub CalculateExclusiveParticleIndices(usedParticleIndicesArrayList As List(Of Integer()))
            If _datafile Is Nothing Then
                Return
            End If

            Debug.WriteLine("CalculateExclusiveParticleIndices for set {0}", Me)

            Dim result As IEnumerable(Of Integer) = New List(Of Integer)(_ParticleIndices)

            ' exclude particleIndices used by higher-ranked sets

            For Each particleIndicesArray In usedParticleIndicesArrayList
                result = result.Except(particleIndicesArray)
            Next

            _Invalid = True
            _ParticleIndices = result.ToArray()
        End Sub

        ''' <summary>
        ''' Recalculate ParticleIndices while not taking into account ExclusiveSet mode
        ''' </summary>
        Public Overrides Sub RecalculateParticleIndices()
            If Not _Invalid OrElse _datafile Is Nothing Then
                Return
            End If

            Debug.WriteLine("gatebasedSet.RecalculateParticleIndices for Set: {0}", Me)

            Dim combinedIndices As List(Of Integer) = Nothing

            For Each gate As IGate In myGates
                gate.RecalculateParticleIndices()

                If combinedIndices Is Nothing Then
                    combinedIndices = New List(Of Integer)(gate.ParticleIndices)
                Else
                    combinedIndices = combinedIndices.Intersect(gate.ParticleIndices).ToList
                End If
            Next

            If combinedIndices IsNot Nothing Then
                _ParticleIndices = combinedIndices.Distinct.ToArray()
            Else
                _ParticleIndices = New Integer() {}
            End If
        End Sub

        ''' <summary>
        ''' Needed by DataExporter implementation of exclusive sets.
        ''' The implementation uses a pre-processor that sets the outcome of
        ''' TestSingleParticle because the set alone does not have info about
        ''' other sets and their priority
        ''' </summary>
        Public Enum TestSingleParticleResultEnum
            not_known
            return_true
            return_false
        End Enum

        Public Overrides Function TestSingleParticle(p As ParticleHandling.Particle) As Boolean
            Select Case _testSingleParticleResult
                Case TestSingleParticleResultEnum.not_known
                    If myGates.Count > 0 Then
                        For Each gateObj As IGate In myGates
                            If Not gateObj.TestSingleParticle(p) Then
                                Return False
                            End If
                        Next

                        Return True
                    Else 'if no gates, no particles are in the set (which is a bit counter intuitive)
                        Return False
                    End If

                Case TestSingleParticleResultEnum.return_true
                    Return True

                Case TestSingleParticleResultEnum.return_false
                    Return False
            End Select

            Return False ' to keep compiler happy
        End Function

#End Region

        Public Overrides Property Datafile As CytoSense.Data.DataFileWrapper
            Get
                Return _datafile
            End Get

            Set(ByVal value As CytoSense.Data.DataFileWrapper)
                _datafile = value
                _Invalid = True
            End Set
        End Property

        Public Overrides ReadOnly Property ChildOnly As Boolean
            Get
                Return False
            End Get
        End Property

#Region "Gate Logic"
        Public ReadOnly Property allGates As List(Of IGate)
            Get
                Return myGates.allGates
            End Get
        End Property

        Public Property TestSingleParticleResult As TestSingleParticleResultEnum
            Get
                Return _testSingleParticleResult
            End Get
            Set(value As TestSingleParticleResultEnum)
                _testSingleParticleResult = value
            End Set
        End Property

        Public Sub AddGate(ByRef thisGate As IGate)
            myGates.Add(thisGate.CreateWorkfileCopy(_datafile))

            _Invalid = True
        End Sub

        ''' <summary>
        ''' Look for a gate definition for the specified axis.  In theory there could be multiple
        ''' matches, A 1D gate on an axis, and a 2D gate that matches both axis. In that case we
        ''' will return the 2D variant, if available.
        ''' </summary>
        ''' <param name="ax1"></param>
        ''' <param name="ax2"></param>
        ''' <returns>The requested Gate, or Nothing.</returns>
        ''' <remarks>What if we have 2 1D gates matching on different axis.</remarks>
        Public Function findGate(ax1 As Axis, ax2 As Axis) As IGate
            Dim retGate As IGate = Nothing
            For i = 0 To myGates.Count - 1
                Dim g = myGates(i)
                If g.HasAxis(ax1, ax2) Then
                    If retGate Is Nothing Then
                        retGate = g
                    Else
                        If retGate.Type = GateType.Range AndAlso g.Type <> GateType.Range Then
                            retGate = g
                        End If ' Else, currently selected gate is better then newly found one.
                    End If
                End If 'Else gate does not match the definition.
            Next
            Return retGate
        End Function

        ''' <summary>
        ''' Deletes the gate if any is present in the specified axis dimension. If there is no gate, nothing changes.
        ''' </summary>
        Public Sub DeleteGate(ByRef xAxis As CytoSense.Data.Analysis.Axis, ByRef yAxis As CytoSense.Data.Analysis.Axis)
            If myGates.Delete(xAxis, yAxis) Then 'Returns true if something was actually changed
                _Invalid = True
            End If
        End Sub

        ''' <summary>
        ''' Update the definition of gate in the current set.  The gate definition that has the same axis
        ''' as the gate parameter is replaced with the gate parameter.
        ''' </summary>
        ''' <param name="gate"></param>
        Public Sub updateGateDefinition(gate As Gate)
            myGates.Delete(gate)
            Dim newIGate As IGate = gate.Clone()
            Dim newGate As Gate = DirectCast(newIGate,Gate)
            newGate.DataFile = _datafile
            myGates.Add(newIGate)
            _Invalid = True
        End Sub

        ''' <summary>
        ''' Removes the gate g from the gate collection IF it was present in the gate collection.
        ''' And recalculates the indexes if required.  If the gate is not present then 
        ''' nothing changes.
        ''' </summary>
        ''' <param name="g"></param>
        ''' <remarks></remarks>
        Public Sub RemoveGate(g As IGate)
            If myGates.Remove(g) Then
                _Invalid = True
            End If
        End Sub

#End Region

        ''' <summary>
        ''' Determines if this set has a gate in the given dimension
        ''' </summary>
        ''' <remarks></remarks>
        Public Function HasGateInDimension(x As Axis, y As Axis) As Boolean
            For i As Integer = 0 To myGates.Count - 1
                If myGates(i).HasAxis(x, y) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Public Overrides Function Clone() As CytoSet
            Return New gateBasedSet(Me)
        End Function

        Public Overrides Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentWrite(document, parentNode)

            myGates.XmlDocumentWrite(document, document.AppendChildElement(parentNode, "GateCollection"))
        End Sub

        Public Overrides Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) 
            MyBase.XmlDocumentRead(document, parentNode)

            myGates = New GateCollection()
            myGates.XmlDocumentRead(document, parentNode.Item("GateCollection"))
        End Sub

    End Class
End Namespace