Namespace CytoSettings
    <Serializable()> Public Class LowerTabs

        Private manualcontrol As Boolean = False
        Private IIF As Boolean = False
        Private schedule As Boolean = False
        Private advanced As Boolean = False
        Private realtimedata As Boolean = False
        Public Sub New(ByVal tabindex As Int16)
            SetTabindex(tabindex)
        End Sub

        Public Sub SetTabindex(ByVal tabindex As Int16)
            If tabindex = 0 Then
                setScheduleTabEnabled()
            ElseIf tabindex = 1 Then
                setRealtimedataTabEnabled()
            ElseIf tabindex = 2 Then
                setAdvancedTabEnabled()
            ElseIf tabindex = 3 Then
                setManualcontrolTabEnabled()
            ElseIf tabindex = 4 Then
                setIIFTabEnabled()
            End If
        End Sub

        Public Function getScheduleTab() As Boolean
            Return (schedule)
        End Function
        Public Function getRealtimedataTab() As Boolean
            Return (realtimedata)
        End Function
        Public Function getAdvancedTab() As Boolean
            Return (advanced)
        End Function
        Public Function getManualcontrolTab() As Boolean
            Return (manualcontrol)
        End Function
        Public Function getIIFTab() As Boolean
            Return (IIF)
        End Function

        Public Sub setScheduleTabEnabled()
            schedule = True
            realtimedata = False
            advanced = False
            manualcontrol = False
            IIF = False
        End Sub
        Public Sub setRealtimedataTabEnabled()
            schedule = False
            realtimedata = True
            advanced = False
            manualcontrol = False
            IIF = False
        End Sub
        Public Sub setAdvancedTabEnabled()
            schedule = False
            realtimedata = False
            advanced = True
            manualcontrol = False
            IIF = False
        End Sub
        Public Sub setManualcontrolTabEnabled()
            schedule = False
            realtimedata = False
            advanced = False
            manualcontrol = True
            IIF = False
        End Sub
        Public Sub setIIFTabEnabled()
            schedule = False
            realtimedata = False
            advanced = False
            manualcontrol = False
            IIF = True
        End Sub

    End Class

End Namespace