
Namespace MeasurementSettings
    <Serializable()> Public Structure IIFFreeFormSelection

        Dim freeform As Byte()()


        Dim name As String
        Private _cytosettings As CytoSettings.CytoSenseSetting
        Public ReadOnly Property Cytosettings As CytoSettings.CytoSenseSetting
            Get
                Return _cytosettings
            End Get
        End Property

        Dim channelX_ID As Byte 'nummer van het (hardware!)-kanaal in de hardwarechannelarray
        Dim ParameterX_ID As Byte 'zie Enum ParameterSelector in CytoSense.vb
        Dim StapX As Integer 'hoeveel een 'pixel' is in termen van de parameter
        Dim ResolutionX As Integer 'Hoeveel decades de freeform beslaat
        Dim ScaleX As Integer 'de maximale waarde

        Dim channelY_ID As Byte
        Dim ParameterY_ID As Byte
        Dim StapY As Integer
        Dim ResolutionY As Integer
        Dim ScaleY As Integer

        Public Sub New(ByVal selectionName As String, ByVal x As Data.Analysis.SingleAxis, ByVal y As Data.Analysis.SingleAxis, ByVal cytosettings As CytoSettings.CytoSenseSetting)
            Me.name = selectionName
            _cytosettings = cytosettings

            Me.channelX_ID = CByte(x.Channel.HW_channel_id)
            Me.ParameterX_ID = CByte(x.Parameter)

            Me.channelY_ID = CByte(y.Channel.HW_channel_id)
            Me.ParameterY_ID = CByte(y.Parameter)

        End Sub

        ''' <summary>
        ''' DEEP copy of the free form selection (except for the cytosettings reference)
        ''' </summary>
        ''' <param name="other"></param>
        Public Sub New( other As IIFFreeFormSelection)
            If other.freeform IsNot Nothing Then
                Redim freeform( other.freeform.Length ) 
                For i = 0 To freeform.Length  -1
                    If other.freeform(i) IsNot Nothing Then
                        freeform(i) = CType(other.freeform(i).Clone(),Byte())
                    End If
                Next
            End If
            
            name          = other.name
            _cytosettings = other._cytosettings

            channelX_ID   = other.channelX_ID
            ParameterX_ID = other.ParameterX_ID
            StapX         = other.StapX
            ResolutionX   = other.ResolutionX
            ScaleX        = other.ScaleX

            channelY_ID   = other.channelY_ID
            ParameterY_ID = other.ParameterY_ID
            StapY         = other.StapY
            ResolutionY   = other.ResolutionY
            ScaleY        = other.ScaleY
        End Sub

    End Structure
End Namespace
