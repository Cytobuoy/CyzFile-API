Imports System.Drawing

Namespace CytoSettings

    <Serializable()> Public Structure digitalOutput 

        Public Enum DI_Type
            UnUsed
            SheathPumpAdjust
            ChannelHighLow
            DSPSetup
            DSPReset
            ExternalTriggerLed
            GVPinchValveA_B
            GVPinchValveC_Pump
            PinchValveBypass
            PinchValveBST
            ExternalPinchValve
            DeflatePinchValve
            BeadsPinchValve 'Internal beads system, also configured in the PIC EEPROM.
            'Internal multi-sampler originally for INOGS
            Port1MultiSampler
            Port2MultiSampler
        End Enum


        Public Sub New(ByVal LowCheck_name As String, ByVal LowCheck_Visible As Boolean, ByVal LowCheck_location As Point, ByVal t As DI_Type)
            name = LowCheck_name
            Visible = LowCheck_Visible
            location = LowCheck_location
            type = t

        End Sub
        ''' <summary>
        ''' Update the current values, it updates all but the location. I have no idea what the location is used for at all,
        ''' so for now I just leave it in as it is.  This functions makes the machines.vb file, several lines shorter. And
        ''' in theory easier to read.
        ''' </summary>
        ''' <param name="nm"></param>
        ''' <param name="vis"></param>
        ''' <param name="tp"></param>
        ''' <remarks></remarks>
        Public Sub Configure( nm As String, vis As Boolean,  tp As DI_Type)
            name = nm
            Visible = vis
            type = tp
        End Sub

        Dim type As DI_Type
        Public Shared release As New Serializing.VersionTrackableClass(New Date(2011, 7, 3))

        Dim Visible As Boolean
        Dim name As String
        Dim location As Point

    End Structure

End Namespace

