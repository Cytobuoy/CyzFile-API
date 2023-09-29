Imports System.Runtime.Serialization

Namespace CytoSettings
    <Serializable()> Public Class PICSettings


        Private _powerSaveMode As PowerSaveModeEnum = PowerSaveModeEnum.none
        Public Property PowerSaveMode As PowerSaveModeEnum
            Get
                Return _powerSaveMode
            End Get
            Set(value As PowerSaveModeEnum)
                _powerSaveMode = value
            End Set
        End Property

        Public Enum PowerSaveModeEnum
            none = 0
            direct = 1
        End Enum

        Private hasSheathFlowMeter As Boolean = False
        Public Property SheathFlowMeter As Boolean
            Get
                Return hasSheathFlowMeter
            End Get
            Set(value As Boolean)
                hasSheathFlowMeter = value
            End Set
        End Property


        Private hasI2C As Boolean = False
        Public Property I2C As Boolean
            Get
                Return hasI2C
            End Get
            Set(value As Boolean)
                hasI2C = value
            End Set
        End Property

        Private hasI2CTemp_Sheath As Boolean = False
        Public Property I2CTemp_Sheath As Boolean
            Get
                Return hasI2CTemp_Sheath
            End Get
            Set(value As Boolean)
                hasI2CTemp_Sheath = value
            End Set
        End Property

        Private hasI2CTemp_Laser As Boolean = False
        Public Property I2CTemp_Laser As Boolean
            Get
                Return hasI2CTemp_Laser
            End Get
            Set(value As Boolean)
                hasI2CTemp_Laser = value
            End Set
        End Property

        Private hasI2CTemp_PMT As Boolean = False
        Public Property I2CTemp_PMT As Boolean
            Get
                Return hasI2CTemp_PMT
            End Get
            Set(value As Boolean)
                hasI2CTemp_PMT = value
            End Set
        End Property

        Private hasI2CTemp_Buoy As Boolean = False 'same address as pmt!
        Public Property I2CTemp_Buoy As Boolean
            Get
                Return hasI2CTemp_Buoy
            End Get
            Set(value As Boolean)
                hasI2CTemp_Buoy = value
            End Set
        End Property

        Private hasI2CTemp_System As Boolean = False
        Public Property I2CTemp_System As Boolean
            Get
                Return hasI2CTemp_System
            End Get
            Set(value As Boolean)
                hasI2CTemp_System = value
            End Set
        End Property

        Private hasI2CExtPressure As Boolean = False
        Public Property I2CExtPressure As Boolean
            Get
                Return hasI2CExtPressure
            End Get
            Set(value As Boolean)
                hasI2CExtPressure = value
            End Set
        End Property


        Private hasFlowThroughPressureSensors As Boolean = False
        ''' <summary>
        ''' Set to true if the instrument has flow through pressure sensors.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>The data is still stored in the same variables as the old abs and differential pressure sensors,
        ''' but the interpretation is no different.  The old Abs pressure sensor, is now the pressure BEFORE the
        ''' pump, and the old differential pressure is now the pressure AFTER the pump.</remarks>
        Public Property FlowThroughPressureSensors As Boolean
            Get
                Return hasFlowThroughPressureSensors
            End Get
            Set(value As Boolean)
                hasFlowThroughPressureSensors = value
            End Set
        End Property

        Private hasI2CAbsPressure As Boolean = False
        Public Property I2CAbsPressure As Boolean
            Get
                Return hasI2CAbsPressure
            End Get
            Set(value As Boolean)
                hasI2CAbsPressure = value
            End Set
        End Property

        Private hasI2CDiffPressure As Boolean = False
        Public Property I2CDiffPressure As Boolean
            Get
                Return hasI2CDiffPressure
            End Get
            Set(value As Boolean)
                hasI2CDiffPressure = value
            End Set
        End Property

        Private hasI2CFJDataElectronics As Boolean = False
        Public Property I2CFJDataElectronics As Boolean
            Get
                Return hasI2CFJDataElectronics
            End Get
            Set(value As Boolean)
                hasI2CFJDataElectronics = value
            End Set
        End Property


        Private hasHallSensor As Boolean = False
        Public Property HallSensor As Boolean
            Get
                Return hasHallSensor
            End Get
            Set(value As Boolean)
                hasHallSensor = value
            End Set
        End Property

        Private hasTurnOnTimer As Boolean = False
        Public Property TurnOnTimer As Boolean
            Get
                Return hasTurnOnTimer
            End Get
            Set(value As Boolean)
                hasTurnOnTimer = value
            End Set
        End Property

        Private polarityTurnOnTimer As TurnOnTimerPolarityEnum = TurnOnTimerPolarityEnum.Normal
        Public Property TurnOnTimerPolarity As TurnOnTimerPolarityEnum
            Get
                Return polarityTurnOnTimer
            End Get
            Set(value As TurnOnTimerPolarityEnum)
                polarityTurnOnTimer = value
            End Set
        End Property
        Public Enum TurnOnTimerPolarityEnum
            Normal = 0
            Reversed = 1
        End Enum

        Private externalSupplyPowerMode As ExternalSupplyPowerModeEnum = ExternalSupplyPowerModeEnum.USBPowered
        Public Property ExternalSupplyPower As ExternalSupplyPowerModeEnum
            Get
                Return externalSupplyPowerMode
            End Get
            Set(value As ExternalSupplyPowerModeEnum)
                externalSupplyPowerMode = value
            End Set
        End Property
        Public Enum ExternalSupplyPowerModeEnum
            USBPowered
            InternalBattery_noExternalSupply
            InternalBattery_ExternalSupply
        End Enum


        Private hasI2CSamplePump As Boolean = False
        Public Property I2CSamplePump As Boolean
            Get
                Return hasI2CSamplePump
            End Get
            Set(value As Boolean)
                hasI2CSamplePump = value
            End Set
        End Property

        Private hasI2CSheathPump As Boolean = False
        Public Property I2CSheathPump As Boolean
            Get
                Return hasI2CSheathPump
            End Get
            Set(value As Boolean)
                hasI2CSheathPump = value
            End Set
        End Property

        <Obsolete>
        Private _defaultSheathSpeed As Byte
        Public Property DefaultSheathSpeed As Double
            Get
                Return _newDefaultSheathSpeed
            End Get
            Set(value As Double)
                _newDefaultSheathSpeed = value
            End Set
        End Property

        Private hasI2CPMTLevelControl As Boolean = False
        Public Property I2CPMTLevelControl As Boolean
            Get
                Return hasI2CPMTLevelControl
            End Get
            Set(value As Boolean)
                hasI2CPMTLevelControl = value
            End Set
        End Property


        Private hasConcentrationCounter As Boolean = False
        Public Property ConcentrationCounter As Boolean
            Get
                Return hasConcentrationCounter
            End Get
            Set(value As Boolean)
                hasConcentrationCounter = value
            End Set
        End Property

        Private hasMultiSampler As Boolean
        Public Property MultiSampler As Boolean
            Get
                Return hasMultiSampler
            End Get
            Set(value As Boolean)
                hasMultiSampler = value
            End Set
        End Property

        Private hasdehuiler As Boolean
        Public Property DeHuiler As Boolean
            Get
                Return hasdehuiler
            End Get
            Set(value As Boolean)
                hasdehuiler = value
            End Set
        End Property

        Private hasI2CFTDIPowerSwitch As Boolean
        Public Property I2CFTDIPowerSwitch As Boolean
            Get
                Return hasI2CFTDIPowerSwitch
            End Get
            Set(value As Boolean)
                hasI2CFTDIPowerSwitch = value
            End Set
        End Property

        Private hasWaterSensor As Boolean
        Public Property WaterSensor As Boolean
            Get
                Return hasWaterSensor
            End Get
            Set(value As Boolean)
                hasWaterSensor = value
            End Set
        End Property

        Private hasPICIIF As Boolean
        Public Property PICIIF As Boolean
            Get
                Return hasPICIIF
            End Get
            Set(value As Boolean)
                hasPICIIF = value
            End Set
        End Property

        <Obsolete> Private hasCytoSelector As Boolean = False
        ''' <summary>
        ''' Store settings for the CytoSelector, the default settings contain a flag to indicate that no CytoSelector is present.
        ''' If you want to configure one, you need to override this setting.
        ''' </summary>
        Private _cytoSelectorSettings As PIC.CytoSelector_settings = New PIC.CytoSelector_settings()
        Public Property CytoSelectorSettings As PIC.CytoSelector_settings
            Get
                Return _cytoSelectorSettings
            End Get
            Set(value As PIC.CytoSelector_settings)
                _cytoSelectorSettings = value
            End Set
        End Property

        Private _multiSamplerSettings As PIC.MultiSampler_settings
        Public Property MultiSampleSettings As PIC.MultiSampler_settings
            Get
                Return _multiSamplerSettings
            End Get
            Set(value As PIC.MultiSampler_settings)
                _multiSamplerSettings = value
            End Set
        End Property

        Public Function getSpeed_ml_per_minute(speed As Byte) As Double
            Dim res As Double = speed
            res -= 12
            Return res
        End Function

        Private hasPowerboard As Boolean
        ''' <summary>
        ''' Denotes whether the PIC is connected to the I2C power board designed by koen
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Powerboard As Boolean
            Get
                Return hasPowerboard
            End Get
            Set(value As Boolean)
                hasPowerboard = value
            End Set
        End Property

        <OptionalField> _
        Public FirmwareVersion As String = Nothing

        <OnDeserializing>
        Private Sub SetDesrializeDefaults(sc As StreamingContext)
            FirmwareVersion        = Nothing
            _newDefaultSheathSpeed = Double.NaN
        End Sub


        ''' <summary>
        ''' Fix older files that were deserialized.
        ''' </summary>
        ''' <param name="sc"></param>
        <OnDeserialized>
        Private Sub AfterDeserialize(sc As StreamingContext)
            If Double.IsNaN(_newDefaultSheathSpeed) Then
#Disable Warning BC40008 ' Type or member is obsolete
                _newDefaultSheathSpeed = CDbl(_defaultSheathSpeed) / 2.55 'Copy old value
#Enable Warning BC40008 ' Type or member is obsolete
            End If
            If _cytoSelectorSettings Is Nothing Then ' Loaded an older version that was serialized without the settings, this is no longer allowed, so add a default one.
                _cytoSelectorSettings = New PIC.CytoSelector_settings()
            End If
        End Sub

        ''' <summary>
        ''' The old value was stored as a byte, but for the newer instruments we need a double. So now we use this one
        ''' instead of the old one.  On deserialization we copy the old value into the new value, if it is a file
        ''' that was saved using the old version.
        ''' </summary>
        Private _newDefaultSheathSpeed As Double


    End Class


End Namespace