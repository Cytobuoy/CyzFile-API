
Namespace MeasurementSettings
    <Serializable()> Public Class Measurements
        Private MeasurementSettings() As Measurement
        Private _currentMeasurementIndex As Integer = 0
        Private _cytosettings As CytoSense.CytoSettings.CytoSenseSetting
        Public release As New Serializing.VersionTrackableClass(New Date(2011, 7, 11))

        Public Sub New(ByVal cytosettings As CytoSense.CytoSettings.CytoSenseSetting, ByVal addMain As Boolean)
            _cytosettings = cytosettings
            If addMain Then
                Dim tmpMeasurement As New CytoSense.MeasurementSettings.Measurement("Main", cytosettings)
                addMeasurement(tmpMeasurement)
            End If
        End Sub

        Public Sub New(ByVal m As Measurement)
            _cytosettings = m.CytoSettings
            addMeasurement(m)
        End Sub

        Public Sub New()

        End Sub


        Dim _startedBy As StartedBy
        ''' <summary>
        ''' Denotes who has started the measurements. Consider using the property AutonomousMode instead.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Started_By As StartedBy
            Get
                Return _startedBy
            End Get
            Set(ByVal value As StartedBy)
                _startedBy = value
            End Set
        End Property
        Public ReadOnly Property AutonomousMode As Boolean
            Get
                Return Not Started_By = CytoSense.MeasurementSettings.StartedBy.User
            End Get
        End Property



        Dim _scheduleTask As CytoSense.Scheduling.task
        Public Property ScheduleTask As CytoSense.Scheduling.task
            Get
                Return _scheduleTask
            End Get
            Set(ByVal value As CytoSense.Scheduling.task)
                _scheduleTask = value
            End Set
        End Property



        Public Sub changeMeasurement(ByVal tab As Measurement)
            Dim tmp As Int32 = getMeasurementIndex(tab.TabName)
            MeasurementSettings(tmp) = tab
            _currentMeasurementIndex = tmp
        End Sub
        Public Function getMeasurement(ByVal tabname As String) As Measurement

            Return getMeasurement(getMeasurementIndex(tabname))
        End Function
        Public Function getMeasurementIndex(ByVal tabname As String) As Int32
            For i As Int16 = 0 To CShort(MeasurementSettings.Length - 1)
                If MeasurementSettings(i).TabName = tabname Then
                    Return i
                End If
            Next

            Throw New MeasurementNotFoundException
        End Function
        Public Function getMeasurement(ByVal i As Int32) As Measurement
            _currentMeasurementIndex = i
            Return MeasurementSettings(i)
        End Function
        Public Sub addMeasurement(ByVal tab As Measurement)
            If Object.Equals(MeasurementSettings, Nothing) Then
                ReDim MeasurementSettings(0)
            Else
                ReDim Preserve MeasurementSettings(MeasurementSettings.Length)
            End If


            MeasurementSettings(MeasurementSettings.Length - 1) = tab
        End Sub
        Public Sub removeMeasurement(ByVal tabname As String)
            Dim i As Int16
            For i = 0 To CShort(MeasurementSettings.Length - 1)
                Dim t As Measurement = MeasurementSettings(i)
                If t.TabName = tabname Then
                    Exit For
                End If
            Next

            Dim new_MeasurementSettings(MeasurementSettings.Length - 2) As Measurement
            For j As Int16 = 0 To CShort(i - 1)
                new_MeasurementSettings(j) = MeasurementSettings(j)
            Next
            For j As Int16 = CShort(i + 1) To CShort(MeasurementSettings.Length - 1)
                new_MeasurementSettings(j - 1) = MeasurementSettings(j)
            Next
            MeasurementSettings = new_MeasurementSettings
        End Sub


        Public ReadOnly Property length() As Int16
            Get
                If Object.Equals(Nothing, MeasurementSettings) Then
                    Return 0
                End If
                Return CShort(MeasurementSettings.Length)
            End Get
        End Property

        Public Sub clear()
            MeasurementSettings = Nothing
        End Sub
        Public Function contains(ByVal Tabname As String) As Boolean
            For Each t As Measurement In MeasurementSettings
                If t.TabName = Tabname Then
                    Return True
                End If
            Next
            Return False
        End Function
        ''' <summary>
        ''' Calculates the total time to be taken if this measurement configuration is performed
        ''' </summary>
        ''' <param name="cytosettings"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MeasurementsTime(ByVal cytosettings As CytoSettings.CytoSenseSetting) As Int32
            Get

                'returns the total length of al the tab measurements in seconds
                Dim total As Int32 = 0
                'high pressure mode duurt ook langer!!!

                For i As Int16 = 0 To CShort(Me.length - 1)
                    Dim tab As CytoSense.MeasurementSettings.Measurement = Me.getMeasurement(i)

                    If Not cytosettings.EnableSubmode Then
                        total += tab.StopafterTimertext + 10
                        If tab.FlushCheck Then
                            total += CInt(cytosettings.FlushTime)  ' 60

                        End If
                        If tab.SeperateConcentration Then
                            total += CInt(cytosettings.SeperateConcentrationText)
                        End If
                        If cytosettings.EnableGVModule Then
                            total += 60
                            If tab.GVCheck Then
                                total += 6
                            End If
                        End If
                    Else
                        'voor iedere 200seconden meting moet er een keer gereflushed worden in submode:
                        total += CInt((Math.Floor(tab.StopafterTimertext / 200) + 1) * 220 + tab.StopafterTimertext)


                    End If

                Next

                Return total + 10

            End Get
        End Property

        ''' <summary>
        ''' Calculates the total used volume if this measurement configuration is performed
        ''' </summary>
        ''' <param name="cytosettings"></param>
        ''' <value></value>
        ''' <returns>Volume in uL</returns>
        ''' <remarks>Adds 5% for certainty</remarks>
        Public ReadOnly Property MeasurementsVolume(ByVal cytosettings As CytoSettings.CytoSenseSetting) As Double
            Get

                'returns the total length of al the tab measurements in seconds
                Dim total As Double = 0
                'high pressure mode duurt ook langer!!!

                For i As Int16 = 0 To CShort(Me.length - 1)
                    Dim tab As CytoSense.MeasurementSettings.Measurement = Me.getMeasurement(i)
                    Dim measurementPumpSpeed As Double = tab.getFlowrate(cytosettings)
                    Dim flushpumpspeed As Double = cytosettings.getFlowSpeed(cytosettings.MaxForwardSpeed)
                    Dim ConcentrationtPumpSpeed As Double = tab.getFlowrate(cytosettings)

                    total += tab.StopafterTimertext * measurementPumpSpeed
                    If tab.FlushCheck Then
                        total += cytosettings.FlushTime * flushpumpspeed

                    End If
                    If tab.SeperateConcentration Then
                        total += cytosettings.SeperateConcentrationText * ConcentrationtPumpSpeed
                    End If



                Next

                Return total * 1.05  'add 5% extra to be sure

            End Get
        End Property


        ''' <summary>
        ''' Returns the measurements which CytoUSB is performing during acquiring
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CurrentMeasurement As Measurement
            Get
                Return MeasurementSettings(_currentMeasurementIndex)
            End Get
        End Property

        Public Sub ReloadCytoSettings(c As CytoSettings.CytoSenseSetting)
            _cytosettings = c

            For i = 0 To length - 1
                MeasurementSettings(i).CytoSettings = c
            Next

        End Sub


    End Class
End Namespace
