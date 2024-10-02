Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports CytoSense.Serializing
Imports System.Drawing.Imaging

''' <summary>
''' Some simple test functions to check the new serialize function that uses saving to temp
''' and renaming the file.  We check if it works correctly if the bak/temp files exists, if directories
''' are actually created, and if after a save a bak file is present, if an original was present.
''' </summary>
<TestClass()>
Public Class TestSerializationBinder

    ''' <summary>
    ''' A simple save and reload, when there is no previous file to overwrite.
    ''' </summary>
    <TestMethod()>
    Public Sub TestCtor()
        Dim bndr = New CytoSerializationBinder()
        Assert.IsNotNull(bndr)
    End Sub

    ' <DataRow(", CyzFile",               "",               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>

    <DataTestMethod()>
    <DataRow("System.Drawing.Color, System.Drawing",                                       "System.Drawing.Color",                                               "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")>
    <DataRow("System.Drawing.Point, System.Drawing",                                       "System.Drawing.Point",                                               "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")>
    <DataRow("System.IO.Ports.Handshake, System",                                          "System.IO.Ports.Handshake",                                          "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("CytoSense.Calibration.SamplePump.DCSamplePump, CyzFile",                     "CytoSense.Calibration.SamplePump.DCSamplePump",                      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Calibration.SamplePump.DCSamplePumpHallMeasurements, CyzFile",     "CytoSense.Calibration.SamplePump.DCSamplePumpHallMeasurements",      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Calibration.SamplePump.CalibrationPoint[], CyzFile",               "CytoSense.Calibration.SamplePump.CalibrationPoint[]",                "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CAutomaticInjectorSettings, CyzFile",                              "CytoSense.CAutomaticInjectorSettings",                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSelectorTray, CyzFile",                                        "CytoSense.CytoSelectorTray",                                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BackflushTimeModeType, CyzFile",                      "CytoSense.CytoSettings.BackflushTimeModeType",                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BeadsModuleOptionsT, CyzFile",                        "CytoSense.CytoSettings.BeadsModuleOptionsT",                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BiocideModuleOptionsT, CyzFile",                      "CytoSense.CytoSettings.BiocideModuleOptionsT",                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BitDepth, CyzFile",                                   "CytoSense.CytoSettings.BitDepth",                                    "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CameraDescriptors, CyzFile",                          "CytoSense.CytoSettings.CameraDescriptors",                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CameraFeatures, CyzFile",                             "CytoSense.CytoSettings.CameraFeatures",                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CarbonFilterModuleOptionsT, CyzFile",                 "CytoSense.CytoSettings.CarbonFilterModuleOptionsT",                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel, CyzFile",                                    "CytoSense.CytoSettings.channel",                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel, CyzFile",                                    "CytoSense.CytoSettings.channel",                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel+LaserColorEnum, CyzFile",                     "CytoSense.CytoSettings.channel+LaserColorEnum",                      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ChannelAccessMode, CyzFile",                          "CytoSense.CytoSettings.ChannelAccessMode",                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ChannelTypesEnum, CyzFile",                           "CytoSense.CytoSettings.ChannelTypesEnum",                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CytoSenseSetting, CyzFile",                           "CytoSense.CytoSettings.CytoSenseSetting",                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CytoSenseSetting+WaterDetectionEnum, CyzFile",        "CytoSense.CytoSettings.CytoSenseSetting+WaterDetectionEnum",         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.digitalOutput, CyzFile",                              "CytoSense.CytoSettings.digitalOutput",                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.digitalOutput+DI_Type, CyzFile",                      "CytoSense.CytoSettings.digitalOutput+DI_Type",                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterModuleOptionsT, CyzFile",               "CytoSense.CytoSettings.ExternalFilterModuleOptionsT",                "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterState, CyzFile",                        "CytoSense.CytoSettings.ExternalFilterState",                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterWastePosition, CyzFile",                "CytoSense.CytoSettings.ExternalFilterWastePosition",                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.GpsSourceType, CyzFile",                              "CytoSense.CytoSettings.GpsSourceType",                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.HighVoltagePrintType, CyzFile",                       "CytoSense.CytoSettings.HighVoltagePrintType",                        "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IIFSettings, CyzFile",                                "CytoSense.CytoSettings.IIFSettings",                                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IoExpanderType, CyzFile",                             "CytoSense.CytoSettings.IoExpanderType",                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IoPinCfg, CyzFile",                                   "CytoSense.CytoSettings.IoPinCfg",                                    "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.LaserInfoT, CyzFile",                                 "CytoSense.CytoSettings.LaserInfoT",                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.LowerTabs, CyzFile",                                  "CytoSense.CytoSettings.LowerTabs",                                   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PIC.CytoSelector_settings, CyzFile",                  "CytoSense.CytoSettings.PIC.CytoSelector_settings",                   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PIC.CytoSelector_settings+MotorPolarity_t, CyzFile",  "CytoSense.CytoSettings.PIC.CytoSelector_settings+MotorPolarity_t",   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings, CyzFile",                                "CytoSense.CytoSettings.PICSettings",                                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+ExternalSupplyPowerModeEnum, CyzFile",    "CytoSense.CytoSettings.PICSettings+ExternalSupplyPowerModeEnum",     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+PowerSaveModeEnum, CyzFile",              "CytoSense.CytoSettings.PICSettings+PowerSaveModeEnum",               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+TurnOnTimerPolarityEnum, CyzFile",        "CytoSense.CytoSettings.PICSettings+TurnOnTimerPolarityEnum",         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PressureSensoreCfg, CyzFile",                         "CytoSense.CytoSettings.PressureSensoreCfg",                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PmtData, CyzFile",                                    "CytoSense.CytoSettings.PmtData",                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PMTOptionsEnum, CyzFile",                             "CytoSense.CytoSettings.PMTOptionsEnum",                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.RoiInfo, CyzFile",                                    "CytoSense.CytoSettings.RoiInfo",                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.SheathEnum, CyzFile",                                 "CytoSense.CytoSettings.SheathEnum",                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.SheathPumpControllerType_t, CyzFile",                 "CytoSense.CytoSettings.SheathPumpControllerType_t",                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.CytoUSBSettings, CyzFile",                         "CytoSense.CytoUSBSettings.CytoUSBSettings",                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.FatalErrorAction, CyzFile",                        "CytoSense.CytoUSBSettings.FatalErrorAction",                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.iifFileSyncMode, CyzFile",                         "CytoSense.CytoUSBSettings.iifFileSyncMode",                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.LowerTabpages, CyzFile",                           "CytoSense.CytoUSBSettings.LowerTabpages",                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.RemoteSettings, CyzFile",                          "CytoSense.CytoUSBSettings.RemoteSettings",                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Imaging.CyzFileBitmap, CyzFile",                                   "System.Drawing.Bitmap",                                              "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")>
    <DataRow("CytoSense.MeasurementSettings.IifSetSelectionInfo, CyzFile",                 "CytoSense.MeasurementSettings.IifSetSelectionInfo",                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.LogConversion, CyzFile",                       "CytoSense.MeasurementSettings.LogConversion",                        "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.Measurement, CyzFile",                         "CytoSense.MeasurementSettings.Measurement",                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.Measurements, CyzFile",                        "CytoSense.MeasurementSettings.Measurements",                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.StartedBy, CyzFile",                           "CytoSense.MeasurementSettings.StartedBy",                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.NewMultiSamplerSettings, CyzFile",                                 "CytoSense.NewMultiSamplerSettings",                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.GraphPlotSettings, CyzFile",                          "CytoSense.PlotSettings.GraphPlotSettings",                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.GraphPlotSettings+DrawMode, CyzFile",                 "CytoSense.PlotSettings.GraphPlotSettings+DrawMode",                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.ScatterPlotSettings, CyzFile",                        "CytoSense.PlotSettings.ScatterPlotSettings",                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Remote.Mode, CyzFile",                                             "CytoSense.Remote.Mode",                                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Remote.TransferProtocol, CyzFile",                                 "CytoSense.Remote.TransferProtocol",                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.SensorHealthLimits, CyzFile",                                      "CytoSense.SensorHealthLimits",                                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.SensorHealthLimits+SensorLimit, CyzFile",                          "CytoSense.SensorHealthLimits+SensorLimit",                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Serializing.CytoMemoryStream, CyzFile",                            "System.IO.MemoryStream",                                             "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("CytoSense.Serializing.Serializing+VersionTrackableClass, CyzFile",           "CytoSense.Serializing.Serializing+VersionTrackableClass",            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                 "System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                  "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("System.Collections.Generic.List`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                "System.Collections.Generic.List`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                 "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.PlotSettings.GraphPlotSettings, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",           "System.Collections.Generic.List`1[[CytoSense.PlotSettings.GraphPlotSettings, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]",          "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.PlotSettings.ScatterPlotSettings, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",         "System.Collections.Generic.List`1[[CytoSense.PlotSettings.ScatterPlotSettings, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]",        "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.MeasurementSettings.IifSetSelectionInfo, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",  "System.Collections.Generic.List`1[[CytoSense.MeasurementSettings.IifSetSelectionInfo, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]", "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    Public Sub TestBindToType( expQualifiedName As String, typeName As String, assemblyName As String)
        Dim bndr = New CytoSerializationBinder()

        Dim exp = Type.GetType(expQualifiedName)
        Dim act = bndr.BindToType( assemblyName, typeName )

        Assert.AreEqual(exp, act)
    End Sub
    

    <DataTestMethod()>
    <DataRow("System.Drawing.Color, System.Drawing",                                       Nothing,                                                              Nothing)>
    <DataRow("System.Drawing.Point, System.Drawing",                                       Nothing,                                                              Nothing)>
    <DataRow("System.IO.Ports.Handshake, System",                                          Nothing,                                                              Nothing)>
    <DataRow("CytoSense.Calibration.SamplePump.DCSamplePump, CyzFile",                     Nothing,                      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Calibration.SamplePump.DCSamplePumpHallMeasurements, CyzFile",     Nothing,      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Calibration.SamplePump.CalibrationPoint[], CyzFile",               Nothing,      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CAutomaticInjectorSettings, CyzFile",                              Nothing,                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSelectorTray, CyzFile",                                        Nothing,                                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BackflushTimeModeType, CyzFile",                      Nothing,                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BeadsModuleOptionsT, CyzFile",                        Nothing,                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BiocideModuleOptionsT, CyzFile",                      Nothing,                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.BitDepth, CyzFile",                                   Nothing,                                    "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CameraDescriptors, CyzFile",                          Nothing,                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CameraFeatures, CyzFile",                             Nothing,                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CarbonFilterModuleOptionsT, CyzFile",                 Nothing,                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel, CyzFile",                                    Nothing,                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel, CyzFile",                                    Nothing,                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.channel+LaserColorEnum, CyzFile",                     Nothing,                      "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ChannelAccessMode, CyzFile",                          Nothing,                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ChannelTypesEnum, CyzFile",                           Nothing,                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CytoSenseSetting, CyzFile",                           Nothing,                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.CytoSenseSetting+WaterDetectionEnum, CyzFile",        Nothing,         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.digitalOutput, CyzFile",                              Nothing,                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.digitalOutput+DI_Type, CyzFile",                      Nothing,                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterModuleOptionsT, CyzFile",               Nothing,                "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterState, CyzFile",                        Nothing,                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.ExternalFilterWastePosition, CyzFile",                Nothing,                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.GpsSourceType, CyzFile",                              Nothing,                               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.HighVoltagePrintType, CyzFile",                       Nothing,                        "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IIFSettings, CyzFile",                                Nothing,                                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IoExpanderType, CyzFile",                             Nothing,                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.IoPinCfg, CyzFile",                                   Nothing,                                    "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.LaserInfoT, CyzFile",                                 Nothing,                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.LowerTabs, CyzFile",                                  Nothing,                                   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PIC.CytoSelector_settings, CyzFile",                  Nothing,                   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PIC.CytoSelector_settings+MotorPolarity_t, CyzFile",  Nothing,   "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings, CyzFile",                                Nothing,                                 "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+ExternalSupplyPowerModeEnum, CyzFile",    Nothing,     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+PowerSaveModeEnum, CyzFile",              Nothing,               "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PICSettings+TurnOnTimerPolarityEnum, CyzFile",        Nothing,         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PressureSensoreCfg, CyzFile",                         Nothing,                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PmtData, CyzFile",                                    Nothing,                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.PMTOptionsEnum, CyzFile",                             Nothing,                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.RoiInfo, CyzFile",                                    Nothing,                                     "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.SheathEnum, CyzFile",                                 Nothing,                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoSettings.SheathPumpControllerType_t, CyzFile",                 Nothing,                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.CytoUSBSettings, CyzFile",                         Nothing,                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.FatalErrorAction, CyzFile",                        Nothing,                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.iifFileSyncMode, CyzFile",                         Nothing,                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.LowerTabpages, CyzFile",                           Nothing,                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.CytoUSBSettings.RemoteSettings, CyzFile",                          Nothing,                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Imaging.CyzFileBitmap, CyzFile",                                   "System.Drawing.Bitmap",                                              "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")>
    <DataRow("CytoSense.MeasurementSettings.IifSetSelectionInfo, CyzFile",                 Nothing,                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.LogConversion, CyzFile",                       Nothing,                        "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.Measurement, CyzFile",                         Nothing,                          "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.Measurements, CyzFile",                        Nothing,                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.MeasurementSettings.StartedBy, CyzFile",                           Nothing,                            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.NewMultiSamplerSettings, CyzFile",                                 Nothing,                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.GraphPlotSettings, CyzFile",                          Nothing,                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.GraphPlotSettings+DrawMode, CyzFile",                 Nothing,                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.PlotSettings.ScatterPlotSettings, CyzFile",                        Nothing,                         "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Remote.Mode, CyzFile",                                             Nothing,                                              "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Remote.TransferProtocol, CyzFile",                                 Nothing,                                  "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.SensorHealthLimits, CyzFile",                                      Nothing,                                       "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.SensorHealthLimits+SensorLimit, CyzFile",                          Nothing,                           "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("CytoSense.Serializing.CytoMemoryStream, CyzFile",                            "System.IO.MemoryStream",                                             "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")>
    <DataRow("CytoSense.Serializing.Serializing+VersionTrackableClass, CyzFile",           Nothing,            "CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null")>
    <DataRow("System.Collections.Generic.List`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                 Nothing,                                   Nothing)>
    <DataRow("System.Collections.Generic.List`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",                                Nothing,                                  Nothing)>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.PlotSettings.GraphPlotSettings, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",           "System.Collections.Generic.List`1[[CytoSense.PlotSettings.GraphPlotSettings, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]",           Nothing)>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.PlotSettings.ScatterPlotSettings, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",         "System.Collections.Generic.List`1[[CytoSense.PlotSettings.ScatterPlotSettings, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]",         Nothing)>
    <DataRow("System.Collections.Generic.List`1[[CytoSense.MeasurementSettings.IifSetSelectionInfo, CyzFile, Version=1.0.8731.18742, Culture=neutral, PublicKeyToken=null]]",  "System.Collections.Generic.List`1[[CytoSense.MeasurementSettings.IifSetSelectionInfo, CytoSense, Version=1.0.8697.18894, Culture=neutral, PublicKeyToken=null]]",  Nothing)>
    Public Sub TestBindToName( qualifiedName As String, expTypeName As String, expAssemblyName As String)
        Dim bndr = New CytoSerializationBinder()

        Dim tp = Type.GetType(qualifiedName)
        Dim actTypeName As String = ""
        Dim actAssemblyName As String = ""
        bndr.BindToName(tp, actAssemblyName, actTypeName)

        Assert.AreEqual(expAssemblyName, actAssemblyName)
        Assert.AreEqual(expTypeName,     actTypeName)
    End Sub



End Class