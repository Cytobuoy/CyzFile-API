Imports System.Runtime.Serialization

Namespace CytoSettings




    ''' <summary>
    ''' The type of humidity sesnor inside the instrument, currently we only support a single type, so it is either None, or the AHT20.
    ''' </summary>
    public enum HumiditySensorType_t
        ''' <summary>No humidity sensor installed in the instrument.</summary>
        None = 0
        ''' <summary> A humidity sesnor fo type AHT20 installed. </summary>
        AHT20 = 1
    End Enum

    ''' <summary>
    ''' The cofniguration settings for the humidity sensor.
    ''' </summary>
    <Serializable()> 
    Public Class HumiditySettings
        ''' <summary>
        ''' What type of sensor, or None if no sensor was installed.
        ''' </summary>
        Public Type As HumiditySensorType_t = HumiditySensorType_t.None
        ''' <summary>
        ''' We read the humidity sensor less frequent, as it does not change that fast, and reading it to fast actually influences
        ''' the value.  The datasheet says not more then once every 2 second, but the default subsampling will be 5, so once every
        ''' 5 seconds.
        ''' </summary>
        Public SubsamplingInterval As Integer = 5
    End Class


    ''' <summary>
    ''' This class is still very small for now, the idea is to add support here for all settings that are part of the &lt;system&gt; element
    ''' in the new .cytoconfig files that we have for instruments with an STM.  Hopefully we can gradually move to a structure that 
    ''' matches that better.  I am not sure if a slow migration will work, but waiting for a big one shot migration has definately
    ''' not worked for the last 10 years. So let us try something new.
    ''' 
    ''' I added this when adding the humidty sensor, so for now that is all I support, the humidity sensor is new, so the default will
    ''' simply be type none, and interval 5. This class should be initialized in such a way that the defaults are workable
    ''' when no data is found on loading.  Add a check in the OnLoaded of the encapsualting class, and if no 
    ''' data was loaded, then simply add a default initialized one.  Later when we start adding more fields that are also
    ''' present in other locations, some more work may need to be done in the OnLoaded.
    ''' </summary>
    <Serializable()> 
    Public Class SystemSettings
        Public HumiditySensor As New HumiditySettings()
    End Class

End Namespace