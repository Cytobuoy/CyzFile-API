Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Xml
Imports CytoSense.Serializing

Namespace Data.Analysis
    ''' <summary>
    ''' A list of sets, with some specific functionality. This wraps around a normal list, to implement specific ways of adding and removing sets, a.o.
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class SetsList
        Implements IEnumerable, IEnumerable(Of CytoSet)
        Implements IXmlDocumentIO

        Private _list As List(Of CytoSet)
        Private _exclusiveSets As Boolean
		Private _serialNumber As String

        Public Property ExclusiveSets As Boolean
            Get
                Return _exclusiveSets
            End Get
            Set(value As Boolean)
                _exclusiveSets = value
            End Set
        End Property

		Public Property SerialNumber As String
			Get
				Return _serialNumber
			End Get
			Set(value As String)
				If String.IsNullOrEmpty(value) Then
					Debug.WriteLine("Tried to write empty string to serialNumber")
					Return
				End If
				_serialNumber = value
			End Set
		End Property

		Private Sub New()
			_list = New List(Of CytoSet) From {New DefaultSet(Color.Gray)}
			ExclusiveSets = False
		End Sub

        Public Sub New(serialNumber As String)
			Me.New()
			_serialNumber = serialNumber
        End Sub


        ''' <summary>
        ''' Simply delete all sets, except the default one, we MUST always have the default set, that cannot be deleted.
        ''' This is a single function instead of removing every item one by one because that involves a lot of processing
        ''' to find references, and when doing it like this we can simply remove all items except the first without 
        ''' processing references which is much easier
        ''' </summary>
        Public Sub DeleteAllNonDefaultSets()
            _list.RemoveRange(1,_list.Count-1)
        End Sub

        ''' <summary>
        ''' You cannot simply copy individual sets because some sets refer to other sets in the
        ''' list, so just cloning individual sets does not work.
        ''' The implementation for this was taken from the WorkFile.vb where it was first used, but
        ''' it is needed elsewhere as well, so I moved it here now.
        ''' We first copy all the basic info, and then in the next round we copy all
        ''' the reference stuff.
        ''' </summary>
        ''' <returns></returns>
        Public Function Clone(targetDfw As DataFileWrapper) As SetsList
            Dim newSetList = New SetsList(_serialNumber)
            newSetList.ExclusiveSets = ExclusiveSets

            For Each s In _list
                newSetList.AddBasicSetInfo(s, targetDfw)
            Next

            For Each s In _list
                newSetList.CompleteAddSet(s)
            Next

            Return newSetList
        End Function

        ''' <summary>
        ''' Add the basic set information, but for combined sets, do not link it to subsets yet.
        ''' The linking to subsets is postponed to the next phase, after the basic set information
        ''' is present.  When loading a workspace into a work file, first use AddBasicSetInfo for
        ''' all sets and then call CompleteAddSet to finish the process.
        ''' </summary>
        ''' <param name="s"></param>
        Public Sub AddBasicSetInfo(s As CytoSet, dfw As DataFileWrapper)
            If s.type = cytoSetType.allImages Then
                Dim imageFocusSet As ImageFocusSet = TryCast(s, ImageFocusSet)

                If imageFocusSet IsNot Nothing Then
                    Add(New ImageFocusSet(imageFocusSet, dfw))
                Else
                    Add(New AllImagesSet(dfw, s.ListID, s.Visible))
                End If
            ElseIf s.type = cytoSetType.SuccessFullyCroppedImages Then
                Dim sfcImgSet = DirectCast(s, SuccessFullyCroppedImagesSet)
                Add(New SuccessFullyCroppedImagesSet(dfw, s.ListID, s.Visible, sfcImgSet.CropMarginBase, sfcImgSet.CropMarginFactor, sfcImgSet.CropBgThreshold, sfcImgSet.CropErosionDilation))
            ElseIf s.type = cytoSetType.CropFailedImages Then
                Dim usfcImgSet = DirectCast(s, FailedCropImagesSet)
                Add(New FailedCropImagesSet(dfw, s.ListID, s.Visible, usfcImgSet.CropMarginBase, usfcImgSet.CropMarginFactor, usfcImgSet.CropBgThreshold, usfcImgSet.CropErosionDilation))
            ElseIf s.type = cytoSetType.unassignedParticles Then
                Add(New UnassignedParticlesSet(Me, s.colorOfSet, dfw, s.ListID, s.Visible))
            ElseIf s.type = cytoSetType.DefaultAll Then
				'Setlist now always has a defaultSet, so adding it makes no sense anymore. Still perform all actions that happen when normally creating a set with these paramaters
                _list(0).colorOfSet = s.colorOfSet
				_list(0).Visible = s.Visible
				_list(0).datafile = dfw
				If dfw IsNot Nothing Then
					_list(0).RecalculateParticleIndices()
				End If	
            ElseIf s.type = cytoSetType.gateBased Then
                Add(New GateBasedSet(dfw, DirectCast(s,GateBasedSet)))
            ElseIf s.type = cytoSetType.combined Then ' Subsets may not be present, we finalize later.
                Dim combSet As CombinedSet = CType(s, CombinedSet)
                Add(New CombinedSet(combSet.Name, combSet.colorOfSet, Nothing, Nothing, combSet.GateCombination, dfw, s.ListID, s.Visible))
            ElseIf s.type = cytoSetType.OrSet Then ' Subsets may not be present, we finalize later.
                Dim incOrSet = DirectCast(s, OrSet)
                Add(New OrSet(incOrSet.Name, incOrSet.colorOfSet, New List(Of CytoSet)(), dfw, incOrSet.AutoSet, s.ListID, s.Visible))
            ElseIf s.type = cytoSetType.indexBased Then
                Throw New NotImplementedException("Index based sets currently not supported")
            Else
                Throw New NotImplementedException("Gate type unknown")
            End If
        End Sub

    Public Sub CompleteAddSet(srcSet As CytoSet)
        If srcSet.type = cytoSetType.allImages Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.SuccessFullyCroppedImages Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.CropFailedImages Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.unassignedParticles Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.DefaultAll Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.gateBased Then
            ' Nothing to do, completely added in Add BasicSetInfo
        ElseIf srcSet.type = cytoSetType.combined Then
            Dim combSrcSet As CombinedSet = CType(srcSet, CombinedSet)
            Dim destSet As CombinedSet = CType(FindSetById(combSrcSet.ListID), CombinedSet)
            'find the sets that are combined
            Dim destSet1 As CytoSet = FindSetById(combSrcSet.Set1.ListID)
            Dim destSet2 As CytoSet = FindSetById(combSrcSet.Set2.ListID)
            destSet.UpdateSubSets(destSet1, destSet2)
        ElseIf srcSet.type = cytoSetType.OrSet Then
            Dim incOrSet = DirectCast(srcSet, OrSet)
            Dim incomingSetsList = incOrSet.SetList
            Dim destOrSet = CType(FindSetById(incOrSet.ListID), OrSet)
            destOrSet.UpdateSets(incomingSetsList.Select(Function(incSet) FindSetById(incSet.ListID)).ToList())
        ElseIf srcSet.type = cytoSetType.indexBased Then
            Throw New NotImplementedException("Index based sets currently not supported")
        Else
            Throw New NotImplementedException("Gate type unknown")
        End If
    End Sub



        ''' <summary>
        ''' Remove a set from the list.  If this set is used in (one or more) 
        ''' combined sets, then the combined sets have to be removed as well.
        ''' Since sets combining can be nested, we need to do this scanning 
        ''' recursively.
        ''' Find all composite sets that reference this set, then remove
        ''' it, repeat for all sets until we have no more sets let to remove.
        ''' Composite, can also be Or set, so we have 2 options, perhaps
        ''' we should create a generic composite set class.
        ''' It also works the other way around, if this is a combined set, and
        ''' one of the children direct children of a set to be removed is a
        ''' child only set, then that child only set has to be removed as well.
        ''' </summary>
        ''' <param name="item"></param>
        ''' <remarks></remarks>
        Public Sub Remove(ByRef item As CytoSet)

			If item.type = cytoSetType.DefaultAll Then
                If _list.Find(Function(cytoSet As CytoSet) cytoSet.type = cytoSetType.DefaultAll) IsNot Nothing Then
					Debug.Assert(False, "Removing the default set is kinda illegal")
                    Return
                End If
            End If

            Dim toRemove As New Queue(Of CytoSet)()
            toRemove.Enqueue(item)

            While toRemove.Count > 0
                Dim s = toRemove.Dequeue()
                Dim sComp = TryCast(s, CompositeSet)
                If sComp IsNot Nothing Then
                    For Each childSet In sComp.ChildSets
                        If childSet.ChildOnly Then
                            toRemove.Enqueue(childSet)
                        End If
                    Next
                End If

                For i = 0 To _list.Count - 1
                    Dim t = TryCast(_list(i), CompositeSet)
                    If t IsNot Nothing Then
                        If t.ChildSets.Contains(s) Then
                            toRemove.Enqueue(t) ' Need to remove this set as well
                        End If
                    End If
                Next
                _list.Remove(s)
            End While
        End Sub

        ''' <summary>
        ''' Add an item to the set.  If it is gate based we connect an event handler to get
        ''' updates of changes in the set.  And we make sure the set has a Unique list ID.
        ''' We try to avoid changing ids of existing sets to avoid problems with stored IDs
        ''' in a sets view.  The reordering that was used in the old version is removed.
        ''' Item is added at the end, except if there is an unassigned particle set we leave that
        ''' at the end.
        ''' </summary>
        ''' <param name="item">The set to add.</param>
        ''' <remarks></remarks>
        Public Sub Add(item As CytoSet)
            ' check to prevent adding singleton CytoSets 

            If item.type = cytoSetType.DefaultAll Then
                If _list.Find(Function(cytoSet As CytoSet) cytoSet.type = cytoSetType.DefaultAll) IsNot Nothing Then
					Debug.Assert(False, "Adding defaultSet when it already exists")
                    Return
                End If
            End If

            If item.type = cytoSetType.unassignedParticles Then
                If _list.Find(Function(cytoSet As CytoSet) cytoSet.type = cytoSetType.unassignedParticles) IsNot Nothing Then
                    Return
                End If
            End If

            If item.ListID < 0 Then ' If it has no ID, generate one, else leave it alone.
                item.ListID = GetNextFreeId()
            End If

            Dim lastUnassignedParticleSet = If(_list.Count > 0, TryCast(_list(_list.Count - 1), UnassignedParticlesSet), Nothing)

            If lastUnassignedParticleSet Is Nothing Then
                _list.Add(item)
            Else
                _list.Insert(_list.Count - 1, item)
            End If
        End Sub

        ''' <summary>
        ''' Get the next free ID to be used for a set.  
        ''' We simply scan the list for the highest ID, and then add 1 tot that.
        ''' </summary>
        ''' <returns>The next free ID.</returns>
        ''' <remarks>Not the most efficient implementation, but the lists should be short and
        ''' adding new sets happen infrequently so ti should not matter that much.</remarks>
        Private Function GetNextFreeId() As Integer
            If _list.Count = 0 Then
                Return 0
            End If
            Dim max = _list.Max(Function(c) c.ListID)
            Return max + 1
        End Function

        Public Function FindSetById(listId As Integer) As CytoSet
            For Each s In _list
                If s.ListID = listId Then
                    Return s
                End If
            Next
            Return Nothing
        End Function

        'RVDH - NOT USED ANYMORE BUT STILL NEEDED FOR DESERIALIZATION OF GATEBASEDSETS
        Private Sub indexesChangedEventHandler(listId As Integer)
            Throw New Exception("SetsList.indexesChangedEventHandler called, THIS SHOULD NOT HAPPEN!")
        End Sub

        ''' <summary>
        ''' It turns out we somehow created a combination set that only referenced a single
        ''' set, instead of 2, so we need to handle this here.  That set should never have existed,
        ''' but somehow we created it.
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="listId"></param>
        ''' <returns></returns>
        Private Function SetIsAffectedByChange(s As CytoSet, listId As Integer) As Boolean
            Dim cSet = TryCast(s, CombinedSet)
            If cSet Is Nothing Then
                Return False
            End If

            If cSet.Set1 IsNot Nothing AndAlso (cSet.Set1.ListID = listId OrElse SetIsAffectedByChange(cSet.Set1, listId)) Then
                Return True
            End If
            If cSet.Set2 IsNot Nothing AndAlso (cSet.Set2.ListID = listId OrElse SetIsAffectedByChange(cSet.Set2, listId)) Then
                Return True
            End If

            Return False 'If we get here none of the subsets matched, so the answer is false.
        End Function

#Region "Exposing regular list methods"
        <System.Diagnostics.DebuggerStepThrough()>
        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of CytoSet) Implements System.Collections.Generic.IEnumerable(Of CytoSet).GetEnumerator
            Return _list.GetEnumerator
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Function Count() As Integer
            Return _list.Count
        End Function

        Default ReadOnly Public Property Item(index As Integer) As CytoSet
          Get
            Return _list(index)
          End Get
        End Property

        <System.Diagnostics.DebuggerStepThrough()>
        Public Sub Clear()
            _list = New List(Of CytoSet) From {New DefaultSet(Color.Gray)}
        End Sub

        <System.Diagnostics.DebuggerStepThrough()>
        Public Function indexOf(ByRef item As CytoSet) As Integer
            Return _list.IndexOf(item)
        End Function

        <System.Diagnostics.DebuggerStepThrough()>
        Public Function Find(ByRef match As System.Predicate(Of CytoSet)) As CytoSet
            Return _list.Find(match)
        End Function

#End Region

        ''' <summary>
        ''' Moves the given set (of level 0) up or down if possible. This influences draw order in plots.
        ''' NOTE: Need to rethink this, all set can be reordered, we only have
        ''' level 0 at the moment, so all can be moved for the set. except the default set,
        ''' and possibly the unassigned set which should be last.
        ''' </summary>
        Public Sub moveSet(ByRef selset As CytoSet, ByRef up As Boolean)
            Dim orgIdx = _list.IndexOf(selset)

            If up And orgIdx > 1 Then
                If orgIdx < (_list.Count - 1) Then
                    _list(orgIdx) = _list(orgIdx - 1)
                    _list(orgIdx - 1) = selset
                ElseIf orgIdx = (_list.Count - 1) Then
                    Dim lastSet = TryCast(_list(_list.Count - 1), UnassignedParticlesSet)
                    If lastSet Is Nothing Then
                        _list(orgIdx) = _list(orgIdx - 1)
                        _list(orgIdx - 1) = selset
                    End If ' Else last set is unassigned particle set, so leave it at the end.
                End If
            End If

            If Not up Then
                If orgIdx < _list.Count - 2 Then
                    _list(orgIdx) = _list(orgIdx + 1)
                    _list(orgIdx + 1) = selset
                ElseIf orgIdx = _list.Count - 2 Then 'Check if the last set is the unassigned set to see if we can move it.
                    Dim lastSet = TryCast(_list(_list.Count - 1), UnassignedParticlesSet)
                    If lastSet Is Nothing Then
                        _list(orgIdx) = _list(orgIdx + 1)
                        _list(orgIdx + 1) = selset
                    End If ' Else last set is unassigned particle set, so leave it at the end.
                End If
            End If
        End Sub

        Public Sub InvalidateGatebasedSets()
            For Each currentSet In _list
                Dim gbs = TryCast(currentSet, GateBasedSet)
                If gbs IsNot Nothing Then
                    gbs.Invalid = True
                End If
            Next
        End Sub

        Public Sub XmlDocumentWrite(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentWrite
            parentNode.SetAttribute("ExclusiveSets", ExclusiveSets.ToString())
			parentNode.SetAttribute("SerialNumber", SerialNumber)

            For Each cytoSet In _list
                If TryCast(cytoSet, IXmlDocumentIO) IsNot Nothing Then
                    Select Case cytoSet.GetType()
                        Case GetType(GateBasedSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "GateBasedSet"))

                        Case GetType(DefaultSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "DefaultSet"))

                        Case GetType(AllImagesSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "AllImagesSet"))

                        Case GetType(CombinedSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "CombinedSet"))

                        Case GetType(OrSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "OrSet"))

                        Case GetType(UnassignedParticlesSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "UnassignedParticlesSet"))

                        Case GetType(FailedCropImagesSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "FailedCropImagesSet"))

                        Case GetType(SuccessFullyCroppedImagesSet)
                            DirectCast(cytoSet, IXmlDocumentIO).XmlDocumentWrite(document, document.AppendChildElement(parentNode, "SuccessFullyCroppedImagesSet"))
                    End Select
                End If
            Next
        End Sub

        Public Sub XmlDocumentRead(document As XmlDocument, parentNode As XmlElement) Implements IXmlDocumentIO.XmlDocumentRead
            ExclusiveSets = Boolean.Parse(parentNode.GetAttribute("ExclusiveSets"))

			Dim tmpserialNumber = parentNode.GetAttribute("SerialNumber")
			'Only write it if the xml version is not empty.
			If Not String.IsNullOrEmpty(tmpserialNumber) AndAlso String.Equals(tmpserialNumber, SerialNumber) Then 
				SerialNumber = tmpserialNumber
			Else
				Debug.WriteLine("Sets Serialnumber was empty or different from workspace")
			End If

            For Each setNode As XmlElement In parentNode.ChildNodes()
                Select Case setNode.Name
                    Case "GateBasedSet"
                        Dim cytoSet As New GateBasedSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "DefaultSet"
                        ' The default set is always present, even in newly created setlist, so we do not add it/
                        ' Instead retrieve it and let it load its properties from the XmlDocument.
                        Dim defSet = FindSetById(0)
                        defSet.XmlDocumentRead(document, setNode)
                    Case "AllImagesSet"
                        Dim cytoSet = New AllImagesSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "CombinedSet"
                        Dim cytoSet = New CombinedSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "OrSet"
                        Dim cytoSet = New OrSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "UnassignedParticlesSet"
                        Dim cytoSet = New UnassignedParticlesSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "FailedCropImagesSet"
                        Dim cytoSet = New FailedCropImagesSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)

                    Case "SuccessFullyCroppedImagesSet"
                        Dim cytoSet = New SuccessFullyCroppedImagesSet()
                        cytoSet.XmlDocumentRead(document, setNode)
                        _list.Add(cytoSet)
                End Select
            Next
        End Sub

        ''' <summary>
        ''' XmlRead only loads the basic data, then we need some post processing to connect everything together. So
        ''' after calling XmlRead on the SetsList, call this function to connect the dots.
        ''' 
        ''' Post processing depends on the subclass, currently I cast to the different classes, which is not pretty.
        ''' Maybe better to add a PostProcessXmlRead call to all the sub classes.
        ''' </summary>
        ''' <param name="cytoSettings"></param>
        Public Sub PostProcessXmlRead(cytoSettings As CytoSettings.CytoSenseSetting)
            ' replace Dummy Sets that only store a listID by sets from the list

            For Each cytoSet In _list
                Dim orSet = TryCast(cytoSet, OrSet)

                If orSet IsNot Nothing Then
                    Dim realChildSets = New List(Of CytoSet)()

                    For Each childSet In orSet.ChildSets
                        realChildSets.Add(FindSetById(childSet.ListID))
                    Next

                    orSet.UpdateSets(realChildSets)
                End If

                Dim combinedSet = TryCast(cytoSet, CombinedSet)

                If combinedSet IsNot Nothing Then
                    Dim realSet1 = FindSetById(combinedSet.Set1.ListID)
                    Dim realSet2 = FindSetById(combinedSet.Set2.ListID)
                    combinedSet.UpdateSubSets(realSet1, realSet2)
                End If


                Dim unassignedSet = TryCast(cytoSet, UnassignedParticlesSet)
                If unassignedSet IsNot Nothing Then
                    unassignedSet.allSets = Me
                End If

                Dim gateSet = TryCast(cytoSet, GateBasedSet)
                If gateSet IsNot Nothing AndAlso cytoSettings IsNot Nothing Then
                    For Each gate In gateSet.allGates
                        gate.UpdateCytoSettings(cytoSettings)
                    Next
                End If
            Next

			If String.IsNullOrEmpty(SerialNumber) AndAlso cytoSettings IsNot Nothing Then
				SerialNumber = cytoSettings.SerialNumber
			End If
        End Sub
        

        ''' <summary>
		''' Used in serializing for exporting sets or setlists from the set library
		''' </summary>
		''' <param name="setsList"></param>
		''' <param name="Serial"></param>
		''' <param name="ConfigDate"></param>
		''' <param name="filename"></param>
        Public Shared Sub XmlSerialize(setsList As SetsList, ConfigDate As Date, filename As String) 
            Dim xmlDocument As New XmlDocument()
            Dim rootElement As XmlElement = xmlDocument.CreateElement("SetList")

            xmlDocument.AppendChild(rootElement)
            xmlDocument.AddAttribute(rootElement, "configuration_date", ConfigDate)

            setsList.XmlDocumentWrite(xmlDocument, rootElement)

            Using outputFile As New StreamWriter(filename, False, Encoding.UTF8)
                xmlDocument.Save(outputFile)
            End Using
        End Sub

        Public Shared Function XmlDeSerialize(cytoSettings As CytoSettings.CytoSenseSetting, filename As String) As SetsList
            Dim xmlDocument As New XmlDocument()
            Dim setList As New SetsList()

            xmlDocument.Load(filename)

            Dim rootElement As XmlElement = xmlDocument.Item("SetList")

            setList.XmlDocumentRead(xmlDocument, rootElement)
            setList.PostProcessXmlRead(cytoSettings)
            Return setList
        End Function

        Public Shared Function XmlDeSerializeString(cytoSettings As CytoSettings.CytoSenseSetting, xml As String) As SetsList
            Dim xmlDocument As New XmlDocument()
            Dim setList As New SetsList()

            xmlDocument.LoadXml(xml)

            Dim rootElement As XmlElement = xmlDocument.Item("SetList")

            setList.XmlDocumentRead(xmlDocument, rootElement)
            setList.PostProcessXmlRead(cytoSettings)
            Return setList
        End Function


        ''' <summary>
        ''' Returns a set with the same name as setDefinition.name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function FindSetByName(name As String) As CytoSet
            For Each s As CytoSet In _list
                If name = s.Name Then
                    Return s
                End If
            Next

            Return Nothing
        End Function

        Public Sub updateGateDefinition(selectedSet As GateBasedSet, gate As Gate)
            Dim setInList As CytoSet = FindSetByName(selectedSet.Name)

            If setInList IsNot Nothing Then
                DirectCast(setInList, GateBasedSet).updateGateDefinition(gate)
            End If
        End Sub

        Public Sub deleteGate(selectedSet As GateBasedSet, xAxis As Axis, yAxis As Axis)
            Dim setInList As CytoSet = FindSetByName(selectedSet.Name)

            If setInList IsNot Nothing Then
                DirectCast(setInList, GateBasedSet).deleteGate(xAxis, yAxis)
            End If
        End Sub
    End Class
End Namespace
