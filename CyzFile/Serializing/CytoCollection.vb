Imports System.Runtime.Serialization

<Serializable>
Public Class CytoCollection
	Implements ISerializable
	Implements ICollection, IList

	Private _items As List(Of Object)

	Public Sub New()
		_items = New List(Of Object)
	End Sub

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
		_items = CType(info.GetValue("Values", GetType(Object())), Object()).ToList()
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
		info.AddValue("Values", _items.ToArray())
	End Sub

#Region "List and collection implementation"

	Public ReadOnly Property Count As Integer Implements ICollection.Count
		Get
			Return DirectCast(_items, ICollection).Count
		End Get
	End Property

	Public ReadOnly Property IsSynchronized As Boolean Implements ICollection.IsSynchronized
		Get
			Return DirectCast(_items, ICollection).IsSynchronized
		End Get
	End Property

	Public ReadOnly Property SyncRoot As Object Implements ICollection.SyncRoot
		Get
			Return DirectCast(_items, ICollection).SyncRoot
		End Get
	End Property

	Public ReadOnly Property IsFixedSize As Boolean Implements IList.IsFixedSize
		Get
			Return DirectCast(_items, IList).IsFixedSize
		End Get
	End Property

	Public ReadOnly Property IsReadOnly As Boolean Implements IList.IsReadOnly
		Get
			Return DirectCast(_items, IList).IsReadOnly
		End Get
	End Property

	Default Public Property Item(index As Integer) As Object Implements IList.Item
		Get
			Return DirectCast(_items, IList)(index)
		End Get
		Set(value As Object)
			DirectCast(_items, IList)(index) = value
		End Set
	End Property

	Public Sub CopyTo(array As Array, index As Integer) Implements ICollection.CopyTo
		DirectCast(_items, ICollection).CopyTo(array, index)
	End Sub

	Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
		Return DirectCast(_items, IEnumerable).GetEnumerator()
	End Function

	Public Function Add(value As Object) As Integer Implements IList.Add
		Return DirectCast(_items, IList).Add(value)
	End Function

	Public Sub Clear() Implements IList.Clear
		DirectCast(_items, IList).Clear()
	End Sub

	Public Function Contains(value As Object) As Boolean Implements IList.Contains
		Return DirectCast(_items, IList).Contains(value)
	End Function

	Public Function IndexOf(value As Object) As Integer Implements IList.IndexOf
		Return DirectCast(_items, IList).IndexOf(value)
	End Function

	Public Sub Insert(index As Integer, value As Object) Implements IList.Insert
		DirectCast(_items, IList).Insert(index, value)
	End Sub

	Public Sub Remove(value As Object) Implements IList.Remove
		DirectCast(_items, IList).Remove(value)
	End Sub

	Public Sub RemoveAt(index As Integer) Implements IList.RemoveAt
		DirectCast(_items, IList).RemoveAt(index)
	End Sub

	Public Function First() As Object
		Return _items.First()
	End Function

	Public Function Last() As Object
		Return _items.Last()
	End Function

#End Region

End Class

#Region "Underlying members that also needed serializable"
'We don't actually need CultureInfo or any of its members in the collections, so its all empty. We don't care about the data

<Serializable>
Public Class CytoCultureInfo
	Implements ISerializable

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
	End Sub
End Class

<Serializable>
Public Class CytoTextInfo
	Implements ISerializable

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
	End Sub
End Class

<Serializable>
Public Class CytoNumberFormatInfo
	Implements ISerializable

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
	End Sub
End Class

<Serializable>
Public Class CytoDateTimeFormatInfo
	Implements ISerializable

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
	End Sub
End Class

<Serializable>
Public Class CytoCalendar
	Implements ISerializable

	Protected Sub New(info As SerializationInfo, context As StreamingContext)
	End Sub

	Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
	End Sub
End Class
#End Region