
Imports System.Reflection
Imports System.Runtime.Serialization
Imports CytoSense.Data.Analysis

Namespace Serializing

    ''' <summary>
    ''' Binder so we can step in and handle deserialization and serialization of memory streams. The new .Net 7.0 does not support this anymore
    ''' but we still have a large amount of old files, so we need a way to handle that. 
    ''' 
    ''' After adding this we also added support for some very old data files, the original method of handling them
    ''' was very complex and required old DLLs, etc.  With the binder we can remap the classes to newer without jumping through
    ''' several hoops.
    ''' </summary>
    Public Class CytoSerializationBinder
        Inherits SerializationBinder

        ''' <summary>
        ''' On deserialization we replace the MemoryStream with our own (compatible type)  Based on:
        ''' 
        ''' https://learn.microsoft.com/en-us/dotnet/api/system.runtime.serialization.serializationbinder?view=net-7.0
        ''' 
        ''' We also check for some very old files where the data classes were in a module instead of a namespace, and
        ''' that resulted in incompatible files.  The previous solution as to read the entire datafile into memory,
        ''' and then search/replace some strings, write to disk and load again.  This involved some magic, but
        ''' using the SerializationBinder we can rename the classes to the new ones that replace the old ones.
        ''' This is a much nicer solution, and we can dump the complete old DLL that we are still carrying around.
        ''' </summary>
        ''' <param name="assemblyName"></param>
        ''' <param name="typeName"></param>
        ''' <returns></returns>
        Public Overrides Function BindToType(assemblyName As String, typeName As String) As Type
            Debug.Assert(Not (assemblyName.StartsWith("CyzFile") ) , "Serialized datafile contains reference to 'CyzFile' dll.")
            Debug.Assert(Not (assemblyName.StartsWith("CytoUtils") ) , "Serialized datafile contains reference to 'CytoUtils' dll.")
'#If DEBUG
'            Dim savedAssemblyName As String = assemblyName
'            Dim savedTypeName As String     = typeName
'#End If
            If typeName.Contains(".MemoryStream") Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Serializing.CytoMemoryStream"
			Else If typeName.Contains("VisualBasic.Collection") Then
				assemblyName = GetType(CytoCollection).Assembly.FullName
				typeName = GetType(CytoCollection).FullName
			Else If typeName.Contains("System.Globalization.CultureInfo") Then
				assemblyName = GetType(CytoCultureInfo).Assembly.FullName
				typeName = GetType(CytoCultureInfo).FullName
			Else If typeName.Contains("System.Globalization.TextInfo") Then
				assemblyName = GetType(CytoTextInfo).Assembly.FullName
				typeName = GetType(CytoTextInfo).FullName
			Else If typeName.Contains("System.Globalization.NumberFormatInfo") Then
				assemblyName = GetType(CytoNumberFormatInfo).Assembly.FullName
				typeName = GetType(CytoNumberFormatInfo).FullName
			Else If typeName.Contains("System.Globalization.DateTimeFormatInfo") Then
				assemblyName = GetType(CytoDateTimeFormatInfo).Assembly.FullName
				typeName = GetType(CytoDateTimeFormatInfo).FullName
			Else If typeName.Contains("System.Globalization.") AndAlso typeName.Contains("Calendar") Then
				assemblyName = GetType(CytoCalendar).Assembly.FullName
				typeName = GetType(CytoCalendar).FullName
			Else If typeName = "CytoSense.Data.ParticleHandling+Particle" Then
                ' In some old DLLs the particle class was in a Module ParticleHandleing, and now it is in a namespace.
                ' and the DSP particle incorrectly
                ' contained a reference to these particles.  So we have to remap them to the new class
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Particle"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                ' The new Channeldata is an abstract baseclass, but from some very old data files, it is a 
                ' a normal class. So we cannot replace it with the abstract class.  Instead we replace it with
                ' it's most basic subclass the ChannelData_Hardware class.  Since the actual content of the class is not
                ' used, it does not really matter, and this allows us to load the file without error.
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_Hardware" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_FWSCurvarture" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_FWSCurvature" ' NOTE: Also fixed a spelling error in Curvature, not just a namespace change.
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_FWSCurvature" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_FWSCurvature"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_Curvature" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_Curvature"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_ratioChannel" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_ratioChannel"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData_SplitFLRED" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData_DualFocus"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData+ParameterSelector" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData+ParameterSelector"
            Else If typeName = "CytoSense.Concentration.DataPointList" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.Data+DataPointList"
            Else If typeName.StartsWith("System.Collections.Generic.List") Then
                Dim oldTypeName = typeName
                Dim commaIdx As Integer = typeName.IndexOf(",")
                Dim genericTypeAssemblyName = typeName.Substring(commaIdx+2) ' Assembly name of subtype (and ]] at the end).
                If genericTypeAssemblyName.StartsWith("CytoSense,") Then
                    typeName = typeName.Substring(0,commaIdx+2) + Assembly.GetExecutingAssembly().FullName + "]]"
                End If
            Else If typeName = "System.Drawing.Bitmap" Then
                ' The original Bitmap class is not available on .Net core, replace with our own class.
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Imaging.CyzFileBitmap"
            Else If typeName = "CytoSense.Data.Data+DataFileIndex" Then
                Throw New Exception("Sectioned datafiles are not supported, if these files are important to you please contact CytoBuoy to see if we can find a solution!")
                ' NOTE: Support code for this can be found in the Cyto__OLD 2.8.0.0 DLL.
            Else If assemblyName.StartsWith("CytoSense")
                assemblyName = Assembly.GetExecutingAssembly().FullName  ' NOTE: We renamed the assembly when making it public.

			Else If typeName.Contains("+MDIWindowStates+MDIWindowState") Then
				If typeName.Contains("System.Collections.Generic.List")
					typeName = GetType(List(Of MDIWindowStateData)).FullName
				Else
					typeName = GetType(MDIWindowStateData).FullName
					assemblyName = Assembly.GetExecutingAssembly().FullName	
				End If
			ElseIf typeName.Contains("+MDIWindowStates")
				typeName = GetType(MDIWindowStatesData).FullName
				assemblyName = Assembly.GetExecutingAssembly().FullName
            End If

'#If DEBUG
'            Debug.WriteLine("SAVED Typename: {0}", savedTypeName, "Dummy")
'            Debug.WriteLine("NEW   Typename: {0}", typeName, "Dummy")
'            Debug.WriteLine("SAVED AssmblyName: {0}", savedAssemblyName, "Dummy")
'            Debug.WriteLine("NEW   AssmblyName: {0}", assemblyName, "Dummy")
'#End If
            Return Type.GetType(String.Format("{0}, {1}", typeName, assemblyName))
        End Function

        Private const CYTOSENSE_DLL_NAME As String = "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null"

        ''' <summary>
        ''' Wen writing, if the file has to be compatible we have to replace it with the name of the original
        ''' memory stream, and we have to make sure that the members we write are compatible with the original
        ''' memory stream.  We record it in the file as a System.IO.Memory stream so the data is compatible with
        ''' older versions of the software.
        ''' </summary>
        ''' <param name="serializedType"></param>
        ''' <param name="assemblyName"></param>
        ''' <param name="typeName"></param>
        Public  Overrides Sub BindToName(serializedType As Type, ByRef assemblyName As string, ByRef typeName As String)
            If serializedType.FullName.Contains("CytoMemoryStream") Then
                assemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                typeName     = "System.IO.MemoryStream"
			Else If serializedType.FullName.Contains("CytoCollection") Then
				assemblyName = GetType(Collection).Assembly.FullName
				typeName = GetType(Collection).FullName
			Else If serializedType.FullName.Contains("CyzFileBitmap") Then
                ' We never write DspImage stuff anymore, but we do need to support writing background images
                ' this way.  So for that we need to support writing the CyzFileBitmap class, and name as a 
                ' System.Drawing.Bitmap
                assemblyName = "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                typeName     = "System.Drawing.Bitmap"
            Else If serializedType.Assembly.FullName.StartsWith("CyzFile") Then
                ' Record it with the name of the original DLL, so the data files stay compatible.
                typeName = Nothing
                assemblyName = CYTOSENSE_DLL_NAME
            Else If serializedType.FullName.StartsWith("System.Collections.Generic.List") Then
                Dim oldTypeName = serializedType.FullName
#If DEBUG
                Dim genLevel = 0
                Dim genIdx = oldTypeName.IndexOf("[[")
                While genIdx >=0 
                    genLevel += 1
                    genIdx = oldTypeName.IndexOf("[[", genIdx+2)
                End While
                Debug.Assert(genLevel=1, "Only a single level of generics is supported!")
#End If
                Dim commaIdx As Integer = oldTypeName.IndexOf(",")
                Dim genericTypeAssemblyName = oldTypeName.Substring(commaIdx+2) ' Assembly name of subtype (and ]] at the end).
                If genericTypeAssemblyName.StartsWith("CyzFile,") Then
                    assemblyName = Nothing ' Behavior of the baseclass for system types, set to Nothing.
                    typeName = oldTypeName.Substring(0,commaIdx+2) + CYTOSENSE_DLL_NAME + "]]"
                Else
                    MyBase.BindToName(serializedType,assemblyName,typeName)
                End If
            Else
                MyBase.BindToName(serializedType,assemblyName,typeName)
            End If
        End Sub

    End Class


End Namespace