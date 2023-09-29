Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Runtime.InteropServices
Imports System.ComponentModel
Imports System
Imports CytoSense.Data

<TestClass()> Public Class TestDataFile
    Private Const BASE_DATA_DIR As String = "TEST_DATA_BASE" 
    Private Const DATA_DIR_USER As String = "TEST_DATA_USER" 
    Private Const DATA_DIR_PWD As String = "TEST_DATA_PWD" 

    Private _testContext As TestContext

    Public Property TestContext As TestContext
      Get
            Return _testContext
        End Get
        Set(ByVal value As TestContext)
            _testContext = value
        End Set        
    End Property

    ''' <summary>
    ''' Use class initialize and class class cleanup to setup/teardown connections to the server
    ''' that stores the test datafiles.  DO this only if the correct environment variables are
    ''' set.
    ''' </summary>
    Private Shared _networkConnectionPath As String = ""
    <ClassInitialize()> _
    Public Shared Sub ClassInit(ByVal context As TestContext)
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        Dim dataUser = Environment.GetEnvironmentVariable(DATA_DIR_USER)
        Dim dataPwd = Environment.GetEnvironmentVariable(DATA_DIR_PWD)

        If Not String.IsNullOrEmpty(baseDir) AndAlso Not String.IsNullOrEmpty(dataUser) AndAlso Not String.IsNullOrEmpty(dataPwd) Then
            AddConnection(baseDir, dataUser, dataPwd)
            _networkConnectionPath = baseDir
        End If ' Else not all three are set, assume we don't need to do anything.
    End Sub 

    <ClassCleanup()> _
    Public Shared Sub ClassCleanup()
        If Not String.IsNullOrEmpty(_networkConnectionPath) Then
            RemoveConnection(_networkConnectionPath)
            _networkConnectionPath = ""
        End If ' Else no connected network path, so do not need to release it.
    End Sub 
#Region "Win32 Support functions"
    ' Move this stuff to some global Win32 support class/DLL somewhere.
    Private Enum ResourceUsage As UInteger
        Connectable = &H1
        Container = &H2
        NoLocalDevice = &H4
        Sibling = &H8
        Attached = &H10
        All = Connectable Or Container Or Attached
        Reserved = &H80000000UI
    End Enum

    Private Enum ResourceDisplayType As UInteger
        Generic = &H0
        Domaim = &H1
        Server = &H2
        Share = &H3
        File = &H4
        Group = &H5
        Network = &H6
        Root = &H7
        ShareAdmin = &H8
        Directory = &H9
        Tree = &HA
        NDSContainer = &HB
    End Enum

    Private Enum ResourceType As UInteger
        Any = &H0
        Disk = &H1
        Print = &H2
        Reserved = &H8
        Unknown = &HFFFFFFFFUI
    End Enum

    Private Enum ResourceScope As UInteger
        Connected = &H1
        GlobalNet = &H2
        Remembered = &H3
        Recent = &H4
        Context = &H5
    End Enum

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure NETRESOURCE
        Public dwScope As ResourceScope
        Public dwType As ResourceType
        Public dwDisplayType As ResourceDisplayType
        Public dwUsage As ResourceUsage
        Public lpLocalName As String
        Public lpRemoteName As String
        Public lpComment As String
        Public lpProvider As String
    End Structure

    Private Const NO_ERROR As UInteger = 0
	Private Const ERROR_EXTENDED_ERROR As UInteger = &H800704B8UI


    <DllImport("mpr.dll", SetLastError := True)> _
    Private Shared Function WNetAddConnection2( _
                           ByRef lpNetResource As NetResource, _
                           ByVal lpPassword As String, _
                           ByVal lpUserName As String, _
                           ByVal dwFlags As Integer) As UInteger
    End Function

    <DllImport("mpr.dll", SetLastError := True)> _
    Private Shared Function WNetCancelConnection2( _
                          ByVal lpName As String, _
                          ByVal dwFlags As Long, _
                          ByVal fForce As Long) As UInteger
    End Function


    <DllImport("mpr.dll", SetLastError := True)> _
    Private Shared Function WNetGetLastError( _
                          ByRef lpError As Integer, _
                          ByVal lpErrorBuf As StringBuilder, _
                          ByVal nErrorBufSize As Integer, _
                          ByVal lpNameBuf As StringBuilder, _
                          ByVal nNameBufSize As Integer) As UInteger
    End Function

    Private Const ERROR_SUCCESS As Integer = 0

    Private Shared Sub AddConnection( path As String, user As String, pwd As String )
        Dim connInfo As New NetResource()
        connInfo.lpRemoteName = path
        connInfo.lpLocalName  = Nothing
        connInfo.dwType       = ResourceType.Disk
        connInfo.lpProvider   = Nothing

        Dim result = WNetAddConnection2( connInfo, pwd, user, 0)
        If result <> ERROR_SUCCESS Then
            If (result <> ERROR_EXTENDED_ERROR)
                Throw New Win32Exception(UIntError2IntError(result))
            End If
            Dim errNum As Integer = 0
            Dim errBuf As New StringBuilder(1024)
            Dim nameBuf As New StringBuilder(1024)
            Dim r2 = WNetGetLastError(errNum, errBuf, 1024, nameBuf, 1024)
            If r2 = NO_ERROR Then
                Throw New Exception(String.Format("Error: {0} - '{1}' ({2})", errNum, errBuf.ToString(), nameBuf.ToString()))
            End If

            Throw New Exception("Unknown error in use connection")

        End If
    End Sub

    Private Shared Sub RemoveConnection( path As String )
        Dim r = WNetCancelConnection2(path,0,1)
        If r <> NO_ERROR Then
            Throw New Win32Exception(UIntError2IntError(r))
        End If
    End Sub

	Public Shared Function UIntError2IntError(e As UInteger) As Integer
		Dim bts As Byte() = BitConverter.GetBytes(e)
		return BitConverter.ToInt32(bts, 0)
    End Function

#End Region



    ' Single is only guaranteed to have 6 decimal digits, 
    Public Const DELTA As Single = 0.001  ' We check to see that the result is within Math.Abs(DELTA*Expected).
#If False
    ''' <summary>
    ''' Access properties of the datafilewrapper of each file and compare with previous data. Also compare average parameter results for particle 0.
    ''' </summary>
    ''' <remarks></remarks>
    <DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\TestDataFiles.xml", "DatafilesData", DataAccessMethod.Sequential)> _
    <DeploymentItem("Test\TestCytoSettings\TestDataFiles.xml")> _
    <TestMethod()>
    Public Sub TestDatafileWrappers()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        Dim testFile = Path.Combine(baseDir, CStr(TestContext.DataRow("filename")))

        Try
            Dim wrapper As New CytoSense.Data.DataFileWrapper(testFile)
            Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

            'The aggregates
            Assert.AreEqual(TestContext.DataRow("numsamples"), wrapper.SplittedParticles.Length, String.Format("The number of samples is different for file {0}", testFile))
            Assert.AreEqual(TestContext.DataRow("numsamples"), wrapper.SplittedParticles.Length, String.Format("The number of samples is different for file {0}", testFile))
            Try
                Assert.AreEqual(TestContext.DataRow("concentration"), wrapper.Concentration, String.Format("The concentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("actualconcentration"), wrapper.ActualConcentration, String.Format("The actualconcentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("analyzedvolume"), wrapper.analyzedVolume, String.Format("The analyzed volume is different for file {0}", testFile))
            Catch ex As CytoSense.ConcentrationMisMatchException
                Assert.AreEqual(TestContext.DataRow("concentration"), wrapper.Concentration(ConcentrationModeEnum.During_measurement_PIC), String.Format("The concentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("actualconcentration"), wrapper.ActualConcentration(ConcentrationModeEnum.During_measurement_PIC), String.Format("The actualconcentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("analyzedvolume"), wrapper.analyzedVolume(ConcentrationModeEnum.During_measurement_PIC), String.Format("The analyzed volume is different for file {0}", testFile))
            End Try
            Assert.AreEqual(TestContext.DataRow("ftdiconcentration"), wrapper.Concentration(ConcentrationModeEnum.During_measurement_FTDI), String.Format("The FTDI-concentration is different for file {0}", testFile))

            If CInt(TestContext.DataRow("numsamples")) > 0 Then
                'Stuff for particle 0
                Dim part = wrapper.SplittedParticles(0)
                'TOF
                Dim expectedTof = Single.Parse(TestContext.DataRow("particle0TOF").ToString())
                Assert.AreEqual(expectedTof, part.TOF, Math.Abs(expectedTof * DELTA), String.Format("The TOF is different for file {0}", testFile))

                'Average parameter value over all channels for every parameter
                For j As Integer = 0 To [Enum].GetValues(GetType(CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector)).Length - 1
                    Dim avg As Single = 0
                    For c As Integer = 0 To part.ChannelData.Length - 1
                        avg += part.ChannelData(c).Parameter(CType(j, ParticleHandling.Channel.ChannelData.ParameterSelector))
                    Next
                    avg /= part.ChannelData.Length

                    Dim headername = String.Format("avg{0},", [Enum].GetName(GetType(CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), j))
                    Dim expected = Single.Parse(TestContext.DataRow(headername).ToString())
                    If Not (Single.IsNaN(expected) AndAlso Single.IsNaN(avg)) Then
                        If Math.Abs(expected-avg) > Math.Abs(expected * DELTA) Then
                            Assert.Fail(String.Format("The parameter {0} is different for file {1}", [Enum].GetName(GetType(CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), j), testFile))
                        End If
                        Assert.AreEqual(expected, avg, Math.Abs(expected * DELTA), String.Format("The parameter {0} is different for file {1}", [Enum].GetName(GetType(CytoSense.Data.ParticleHandling.Channel.ChannelData.ParameterSelector), j), testFile))
                    End If ' Else both are NaN, consider them equal

                Next
            End If
        Catch ioex As IOException
            TestContext.WriteLine("IO Exception - '{0}'", ioex.Message)
            For Each d In ioex.Data 
                TestContext.WriteLine("Prop - '{0}'", d)            
            Next
            Throw
        End Try

    End Sub
#End If

#If False
    ''' <summary>
    ''' Do a header only opening of the all the files, should only differ for the segmented files,
    ''' but just to be sure, and check of all the properties that should be supported are still there.
    ''' This is a restricted list of course, not the fill list.
    ''' </summary>
    ''' <remarks></remarks>
    <DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\TestDataFiles.xml", "DatafilesData", DataAccessMethod.Sequential)> _
    <DeploymentItem("Test\TestCytoSettings\TestDataFiles.xml")> _
    <TestMethod()>
    Public Sub TestDatafileWrappersHEaderOnly()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        Dim testFile = Path.Combine(baseDir, CStr(TestContext.DataRow("filename")))

        Try
            Dim wrapper As New CytoSense.Data.DataFileWrapper(testFile)
            Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

            'The aggregates
            Assert.AreEqual(TestContext.DataRow("numsamples"), wrapper.SplittedParticles.Length, String.Format("The number of samples is different for file {0}", testFile))
            Assert.AreEqual(TestContext.DataRow("numpictures"),  wrapper.numberOfPictures, String.Format("The number of pictures is different for file {0}", testFile))

            Try
                Assert.AreEqual(TestContext.DataRow("concentration"), wrapper.Concentration, String.Format("The concentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("actualconcentration"), wrapper.ActualConcentration, String.Format("The actualconcentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("analyzedvolume"), wrapper.analyzedVolume, String.Format("The analyzed volume is different for file {0}", testFile))
            Catch ex As CytoSense.ConcentrationMisMatchException
                Assert.AreEqual(TestContext.DataRow("concentration"), wrapper.Concentration(ConcentrationModeEnum.During_measurement_PIC), String.Format("The concentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("actualconcentration"), wrapper.ActualConcentration(ConcentrationModeEnum.During_measurement_PIC), String.Format("The actualconcentration is different for file {0}", testFile))
                Assert.AreEqual(TestContext.DataRow("analyzedvolume"), wrapper.analyzedVolume(ConcentrationModeEnum.During_measurement_PIC), String.Format("The analyzed volume is different for file {0}", testFile))
            End Try
            Assert.AreEqual(TestContext.DataRow("ftdiconcentration"), wrapper.Concentration(ConcentrationModeEnum.During_measurement_FTDI), String.Format("The FTDI-concentration is different for file {0}", testFile))

            'Header only, so can do no particle stuff (at least fo rhte newer files, older will still have all the data)
        Catch ioex As IOException
            TestContext.WriteLine("IO Exception - '{0}'", ioex.Message)
            For Each d In ioex.Data 
                TestContext.WriteLine("Prop - '{0}'", d)            
            Next
            Throw
        End Try

    End Sub

#End If

    ''' <summary>
    ''' A limited test, we load a segmented file in header only mode, as is used by the database view.
    ''' And we test the access of eveyr property used by the database view to see if that succeeds.
    ''' We do not check the values, just verify it can be loaded.
    ''' And we check the particles/data is indeed not loaded.
    ''' </summary>
    ''' <remarks></remarks>
    <TestMethod()>
    Public Sub TestDataSegmentedFileHeaderLoadWrapperProperties()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        'Dim testFile = Path.Combine(baseDir, "Segmented\Segmented 2014-09-30 15u13.cyz")
		Dim testFile = "DataFiles/Segmented 2014-09-30 15u13.cyz"

        Dim wrapper As New DataFileWrapper(testFile)
        Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

        For Each prop As System.ComponentModel.PropertyDescriptor In System.ComponentModel.TypeDescriptor.GetProperties(GetType(DataFileWrapper))
            If prop.IsBrowsable Then
                ' We ignore the visible/invisbile in the different mode attributes for now.
                'Simply access the property, and see if it explodes.
                Dim propValue = prop.GetValue(wrapper)
            End If ' Ignore non browsable properties for now.
        Next
    End Sub


    <TestMethod()>
    Public Sub TestDataSegmentedFileHeaderLoadMeasurementInfoProperties()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        'Dim testFile = Path.Combine(baseDir, "Segmented\Segmented 2014-09-30 15u13.cyz")
		Dim testFile = "DataFiles/Segmented 2014-09-30 15u13.cyz"

        Dim wrapper As New DataFileWrapper(testFile)
        Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

        For Each prop As System.ComponentModel.PropertyDescriptor In System.ComponentModel.TypeDescriptor.GetProperties(GetType(MeasurementInfo))
            If prop.IsBrowsable Then
                ' We ignore the visible/invisbile in the different mode attributes for now.
                'Simply access the property, and see if it explodes.
                Dim propValue = prop.GetValue(wrapper.MeasurementInfo)
            End If ' Ignore non browsable properties for now.
        Next
    End Sub

    <TestMethod()>
    Public Sub TestDataSegmentedFileHeaderLoadMeasurementProperties()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        'Dim testFile = Path.Combine(baseDir, "Segmented\Segmented 2014-09-30 15u13.cyz")
		Dim testFile = "DataFiles/Segmented 2014-09-30 15u13.cyz"

        Dim wrapper As New DataFileWrapper(testFile)
        Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

        For Each prop As System.ComponentModel.PropertyDescriptor In System.ComponentModel.TypeDescriptor.GetProperties(GetType(CytoSense.MeasurementSettings.Measurement))
            If prop.IsBrowsable Then
                ' We ignore the visible/invisbile in the different mode attributes for now.
                'Simply access the property, and see if it explodes.
                Dim propValue = prop.GetValue(wrapper.MeasurementSettings)
            End If ' Ignore non browsable properties for now.
        Next
    End Sub


    <TestMethod()>
    Public Sub TestDataSegmentedFileHeaderLoadCytoSenseSettingProperties()
        Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
        'Dim testFile = Path.Combine(baseDir, "Segmented\Segmented 2014-09-30 15u13.cyz")
		Dim testFile = "DataFiles/Segmented 2014-09-30 15u13.cyz"

        Dim wrapper As New DataFileWrapper(testFile)
        Assert.IsNotNull(wrapper, "The datawrapper did not load for file " & testFile)

        For Each prop As System.ComponentModel.PropertyDescriptor In System.ComponentModel.TypeDescriptor.GetProperties(GetType(CytoSense.CytoSettings.CytoSenseSetting))
            If prop.IsBrowsable Then
                ' We ignore the visible/invisbile in the different mode attributes for now.
                'Simply access the property, and see if it explodes.
                Dim propValue = prop.GetValue(wrapper.CytoSettings)
            End If ' Ignore non browsable properties for now.
        Next
    End Sub





    ' '''' <summary>
    ' '''' Access all parameters for 10 random particles out of each file, to see if we can actually load them.
    ' '''' </summary>
    ' '''' <remarks></remarks>
    '<DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\TestDataFiles.xml", "DatafilesData", DataAccessMethod.Sequential)> _
    '<DeploymentItem("Test\TestCytoSettings\TestDataFiles.xml")> _
    '<TestMethod()> _
    'Public Sub TestLoadParameterValues()
    '    Dim baseDir = Environment.GetEnvironmentVariable(BASE_DATA_DIR)
    '    Dim testFile = Path.Combine(baseDir, CStr(TestContext.DataRow("filename")))
    '    Dim numSamples As Integer = CInt(TestContext.DataRow("numsamples"))

    '    If numSamples = 0 Then
    '        Return ' Cannot do tests if num samples.
    '    End If

    '    'GC.Collect(2, GCCollectionMode.Forced) : TestContext.WriteLine("Memory used before load: {0:N0}", GC.GetTotalMemory(False))

    '    Dim wrapper As New CytoSense.Data.DataFileWrapper(testFile)
    '    Assert.IsNotNull(wrapper)

    '    'GC.Collect(2, GCCollectionMode.Forced) : TestContext.WriteLine("Memory after before load: {0:N0}", GC.GetTotalMemory(False))

    '    Dim numChannels = wrapper.SplittedParticles(0).ChannelData_Hardware.Length
    '    Dim numParticles = wrapper.SplittedParticles.Length
    '    Dim numParameters = CytoSense.Data.ParticleHandling.Channel.ChannelData_Hardware.ParameterNames.Length

    '    Dim numTests As Integer = 10
    '    Dim partArray(numTests - 1) As Integer
    '    For i As Integer = 0 To numTests - 1
    '        partArray(i) = CInt(_rand.Next(0, numParticles - 1))
    '    Next

    '    Dim dummy As Single
    '    For i As Integer = 0 To partArray.Length - 1
    '        Dim particleIdx = partArray(i)
    '        Dim tmpParticle = wrapper.SplittedParticles(particleIdx)
    '        For chanIdx As Integer = 0 To numChannels - 1 'Force calculation of parameters
    '            Dim tmpChannelData = tmpParticle.ChannelData_Hardware(chanIdx)
    '            For parIdx As Integer = 0 To numParameters - 1
    '                dummy = tmpChannelData.Parameter(CType(parIdx, ParticleHandling.Channel.ChannelData.ParameterSelector))
    '            Next
    '        Next
    '    Next
    '    'GC.Collect(2, GCCollectionMode.Forced) : TestContext.WriteLine("Memory after parameter access: {0:N0}", GC.GetTotalMemory(False))
    'End Sub


End Class