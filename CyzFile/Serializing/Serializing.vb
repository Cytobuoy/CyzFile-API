Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Namespace Serializing
    ''' <summary>
    ''' Function for saving or opening general serialized objects
    ''' </summary>
    Public Module Serializing


        <Serializable()> Public Class VersionTrackableClass
            Private _ClassReleaseDate As Date
            Public Sub New(ByVal releaseDate As DateTime)
                _ClassReleaseDate = releaseDate
            End Sub

            Public ReadOnly Property ReleaseDate As DateTime
                Get
                    Return _ClassReleaseDate
                End Get
            End Property

            ''' <summary>
            ''' Usage: program a property Release in the class in question of a VersionTrackableClass. Initialize this VersionTrackableClass hardcoded in the base class: Public Release as VersionTrackableClass(release date).Then when a class needs to be checked for compatability:            
            ''' </summary>           
            ''' <value>If true this means some of the variables may not be properly initialized</value>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public ReadOnly Property needsUpgrade(ByVal release As VersionTrackableClass) As Boolean
                Get
                    If Object.Equals(release, Nothing) Then
                        Return True
                    Else
                        Return release.ReleaseDate <> Me._ClassReleaseDate
                    End If
                End Get
            End Property
        End Class


        ''' <summary>
        ''' For .Net 7 there is an issue with serializing the System.IO.MemoryStream, we need to override that and plug in
        ''' our own class to make it work.  We do this by creating a special SerializationBinder and plugging that into
        ''' the binary formatter.  I think we only need it for data files, and not for the other objects, but for now I will
        ''' add this formatter to every serialization. It should not hurt, and it minimizes the impact on other code because
        ''' we do not need to create specific datafile serialization code.
        ''' </summary>
        ''' <returns></returns>
        Public Function CreateBinaryFormatter() As BinaryFormatter
            Return New BinaryFormatter With {
                .Binder = New CytoSerializationBinder()
            }
        End Function



        ''' <summary>
        ''' Load an object from file and convert to the specified type.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        Public Function loadFromFile(Of T)(ByVal filename As String) As T
            Return DirectCast(loadFromFile(filename),T)
        End Function


        Public Function loadFromFile(ByVal filename As String) As Object

            Dim BF As BinaryFormatter = CreateBinaryFormatter()
            Dim ms As New System.IO.FileStream(filename, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
            Try
#Disable Warning SYSLIB0011
                Dim resFromFile As Object = BF.Deserialize(ms)
#Enable Warning SYSLIB0011
                ms.Close()
                Return resFromFile
            Catch e As Exception
                ms.Close()
                Throw ' e
            End Try
        End Function


        ''' <summary>
        ''' Save object 'o' to the specified filename.  The function does this by saving to a temporary
        ''' file in the same location and then replacing the original file with the temp one. 
        ''' A backup of the original will be created.  This backup is just to make sure that if the
        ''' original is open in another application, e.g. the scheduler that the saving will still
        ''' proceed.  The backup file will be deleted the next time that save is called.  So there
        ''' will always be only one backup.
        ''' The temp file will be named by appending '.tmp' to filename and the backup will be created
        ''' by appending '.backup' to the filename.  If either the tmp or the backup exists they will be
        ''' deleted at the start of the function. 
        ''' 
        ''' This way the function has transaction semantics, either it succeeds and the new settings are
        ''' written to disc completely, or it fails and the original file is still there. This is also
        ''' because the replace file API has transactional semantics. (On POSIX systems, move with the
        ''' correct flags is used to achieve the same functionality, except without the backup file,
        ''' that is not supported in POSIX, I think, whereas in windows not using the backup file will
        ''' cause failure if the file happens to be open, even if the share delete flag is set).
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <param name="o"></param>
        Public Sub SaveToFile(filename As String, o As Object)
            Dim tmpName = filename + ".tmp"
            Dim backupName = filename + ".backup"

            Dim DirName = Path.GetDirectoryName(filename)
            If Not String.IsNullOrEmpty(DirName) Then
               Directory.CreateDirectory(DirName)
            End If

            File.Delete(tmpName)    ' Left over from previous failed attempt to save
            File.Delete(backupName) ' backup file from previous save.

            Using tmpStream As New FileStream(tmpName, FileMode.CreateNew)
                Dim formatter As BinaryFormatter = CreateBinaryFormatter()
#Disable Warning SYSLIB0011
                formatter.Serialize(tmpStream,o)
#Enable Warning SYSLIB0011
                tmpStream.Flush(flushToDisk:=True) ' Force writing all data to disk before continuing. This can in theory prevent corrupted datafiles when there is a power failure.
            End Using

            If File.Exists(filename) Then
                File.Replace(tmpName,filename,backupName,True)
            Else
                File.Move(tmpName,filename)
            End If
        End Sub


        Public Function deserializeStream(Of T)( s As Stream ) As T
            Dim BF As BinaryFormatter = CreateBinaryFormatter()
#Disable Warning SYSLIB0011
            Return CType(BF.Deserialize(s), T)
#Enable Warning SYSLIB0011
        End Function


        Public Function deserializeStream(buf() As Byte) As Object
            Dim MS As New IO.MemoryStream(buf)
            Dim BF As BinaryFormatter = CreateBinaryFormatter()
#Disable Warning SYSLIB0011
            Dim resFromFile As Object = BF.Deserialize(MS)
#Enable Warning SYSLIB0011
            Return resFromFile
        End Function

        Public Function serializeToStream(ByVal C As Object) As Byte()
            Dim MS As New System.IO.MemoryStream()
            Dim BF As BinaryFormatter = CreateBinaryFormatter()
#Disable Warning SYSLIB0011
            BF.Serialize(MS, C)
#Enable Warning SYSLIB0011
            MS.Seek(0, IO.SeekOrigin.Begin)
            Dim buf(CInt(MS.Length - 1)) As Byte
            MS.Read(buf, 0, CInt(MS.Length))
            Return buf
        End Function

		<Serializable>
		Public Class MDIWindowStatesData
			Public windows As List(Of MDIWindowStateData)
		End Class

		<Serializable>
		Public Class MDIWindowStateData
			Public x As Integer
			Public y As Integer
			Public width As Integer
			Public height As Integer
			Public openend As Boolean
			Public text As String
		End Class

    End Module

End Namespace