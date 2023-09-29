''' <summary>
''' Store the current stats of the automatic injector.  There are not really any settings, we
''' just store the current state here, this consists of current position, number or writes,
''' and that is about it.  And if it is connected or not.
''' This field should not be present in the CytoSense settings by default, it will only be created
''' once an automatic injector is connected.  It will stay in it when disconnected.
''' If an automatic injector is really removed from the device, a factory reset is
''' required in order for the UI to be removed.  It will automatically be disabled
''' when disconnected, but not removed completely until a factory reset.
''' </summary>
<Serializable()>
Public Class CAutomaticInjectorSettings
    Public Connected As Boolean            ' Is the automatic injector connected or not.
    Public CameraFocusPosition As Integer  ' The current motor position in the camera focus orientation
    Public LaserFocusPosition As Integer   ' The current motor position in the laser focus orientation.
    Public NumberOfEepromWrites As Integer ' The number of writes to the EEPROM, used to keep track of the current motor position.
    Public FirmwareCompileDate As DateTime ' The date/time the firmware was compile.
    Public FirmwareVersion As String       ' Git version of the firmware
End Class
