
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
            ' Debug.Print(assemblyName + "***" + typeName + "***")
            If typeName.Contains(".MemoryStream") Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Serializing.CytoMemoryStream"
            Else If typeName = "CytoSense.Data.ParticleHandling+Particle" Then
                ' In some old DLLs the particle class was in a Module ParticleHandleing, and now it is in a namespace.
                ' and the DSP particle incorrectly
                ' contained a reference to these particles.  So we have to remap them to the new class
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Particle"
            Else If typeName = "CytoSense.Data.ParticleHandling+ChannelData" Then
                assemblyName = Assembly.GetExecutingAssembly().FullName
                typeName     = "CytoSense.Data.ParticleHandling.Channel.ChannelData"
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
            End If

            ' The following line of code returns the type.
            'Debug.Print("==>" + assemblyName + "***" + typeName + "***")
            Dim tp As Type =  Type.GetType(String.Format("{0}, {1}", typeName, assemblyName))
            return tp
        End Function

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
            If serializedType.Name.Contains("CytoMemoryStream") Then
                assemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
                typeName     = "System.IO.MemoryStream"
            Else If serializedType.Name.Contains("CyzFileBitmap") Then
                ' We never write DspImage stuff anymore, but we do need to support writing background images
                ' this way.  So for that we need to support writing the CyzFileBitmap class, and name as a 
                ' System.Drawing.Bitmap
                assemblyName = "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
                typeName     = "System.Drawing.Bitmap"
            Else
                MyBase.BindToName(serializedType,assemblyName,typeName)
            End If
        End Sub

    End Class


End Namespace