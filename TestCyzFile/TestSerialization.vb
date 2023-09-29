Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO
Imports CytoSense.Serializing
 
<Serializable>
Public Class SerializeTestClass
    Public DummyString As String
    Public DummyCounter As Integer
End Class

''' <summary>
''' Some simple test functions to check the new serialize function that uses saving to temp
''' and renaming the file.  We check if it works correctly if the bak/temp files exists, if directories
''' are actually created, and if after a save a bak file is present, if an original was present.
''' </summary>
<TestClass()>
Public Class TestSerialization

    ''' <summary>
    ''' A simple save and reload, when there is no previous file to overwrite.
    ''' </summary>
    <TestMethod()>
    Public Sub TestSimpelSaveNoPreviousFile()

        Dim testName = "TestSimpelSaveNoPreviousFile.dat"
        Dim tmpPath = Path.GetTempPath()

        Dim testFileName = Path.Combine(tmpPath,testName)
        Dim testTmpFileName = Path.Combine(tmpPath,testName+".tmp")
        Dim testBakFileName = Path.Combine(tmpPath,testName+".backup")

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)

        Dim tc As New SerializeTestClass()
        tc.DummyString = "Hello World!"
        tc.DummyCounter = 42

        Serializing.SaveToFile(testFileName,tc)

        Dim reloaded = CType(Serializing.loadFromFile(testFileName), SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsFalse( File.Exists(testBakFileName) )
        Assert.AreEqual(tc.DummyString,  reloaded.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloaded.DummyCounter)
    End Sub

    ''' <summary>
    ''' Save it two times, so the previous version is overwritten, and it makes a backup file.
    ''' </summary>
    <TestMethod()>
    Public Sub TestSimpelSaveTwoTimes()

        Dim testName = "TestSimpelSaveTwoTimes.dat"
        Dim tmpPath = Path.GetTempPath()

        Dim testFileName = Path.Combine(tmpPath,testName)
        Dim testTmpFileName = Path.Combine(tmpPath,testName+".tmp")
        Dim testBakFileName = Path.Combine(tmpPath,testName+".backup")

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)

        'Save and verify first time.
        Dim tc As New SerializeTestClass()
        tc.DummyString = "Hello World!"
        tc.DummyCounter = 42

        Serializing.SaveToFile(testFileName,tc)

        Dim reloaded = CType(Serializing.loadFromFile(testFileName), SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsFalse( File.Exists(testBakFileName) )
        Assert.AreEqual(tc.DummyString,  reloaded.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloaded.DummyCounter)

        'Second time, there is allready a file, so this time we should end up with a .bak file
        'that contains the old content.

        Dim tc2 As New SerializeTestClass()
        tc2.DummyString = "Good Bye"
        tc2.DummyCounter = 43
        Serializing.SaveToFile(testFileName,tc2)

        Dim reloadedBak = CType(Serializing.loadFromFile(testBakFileName), SerializeTestClass)

        Dim reloaded2 = CType(Serializing.loadFromFile(testFileName), SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsTrue(  File.Exists(testBakFileName) )

        Assert.AreEqual(tc.DummyString,  reloadedBak.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloadedBak.DummyCounter)

        Assert.AreEqual(tc2.DummyString,  reloaded2.DummyString )
        Assert.AreEqual(tc2.DummyCounter, reloaded2.DummyCounter)

    End Sub

    ' Saving three times means the previous bak file will have to be overridden/removed
    ' that is why we do it.
    ' We still verify the intermediate steps, allthough that is allready taken care of in other tests.
    <TestMethod()>
    Public Sub TestSimpelSaveThreeTimes()

        Dim testName = "TestSimpelSaveThreeTimes.dat"
        Dim tmpPath = Path.GetTempPath()

        Dim testFileName = Path.Combine(tmpPath,testName)
        Dim testTmpFileName = Path.Combine(tmpPath,testName+".tmp")
        Dim testBakFileName = Path.Combine(tmpPath,testName+".backup")

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        File.Delete(testTmpFileName)

        'Save and verify first time.
        Dim tc As New SerializeTestClass()
        tc.DummyString = "Hello World!"
        tc.DummyCounter = 42

        Serializing.SaveToFile(testFileName,tc)

        Dim reloaded = CType(Serializing.loadFromFile(testFileName), SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsFalse( File.Exists(testBakFileName) )
        Assert.AreEqual(tc.DummyString,  reloaded.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloaded.DummyCounter)

        'Second time, there is allready a file, so this time we should end up with a .bak file
        'that contains the old content.

        Dim tc2 As New SerializeTestClass()
        tc2.DummyString = "Good Bye"
        tc2.DummyCounter = 43
        Serializing.SaveToFile(testFileName,tc2)

        Dim reloadedBak = CType(Serializing.loadFromFile(testBakFileName), SerializeTestClass)
        Dim reloaded2   = CType(Serializing.loadFromFile(testFileName),    SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsTrue(  File.Exists(testBakFileName) )

        Assert.AreEqual(tc.DummyString,  reloadedBak.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloadedBak.DummyCounter)

        Assert.AreEqual(tc2.DummyString,  reloaded2.DummyString )
        Assert.AreEqual(tc2.DummyCounter, reloaded2.DummyCounter)


        Dim tc3 As New SerializeTestClass()
        tc3.DummyString = "More Fun"
        tc3.DummyCounter = 156
        Serializing.SaveToFile(testFileName,tc3)

        Dim reloadedBak2 = CType(Serializing.loadFromFile(testBakFileName), SerializeTestClass)
        Dim reloaded3    = CType(Serializing.loadFromFile(testFileName),    SerializeTestClass)

        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsTrue(  File.Exists(testBakFileName) )

        Assert.AreEqual(tc2.DummyString,  reloadedBak2.DummyString )
        Assert.AreEqual(tc2.DummyCounter, reloadedBak2.DummyCounter)

        Assert.AreEqual(tc3.DummyString,  reloaded3.DummyString )
        Assert.AreEqual(tc3.DummyCounter, reloaded3.DummyCounter)
    End Sub

    ''' <summary>
    ''' Create the temp file first so it has to be overwritten.
    ''' </summary>
    <TestMethod()>
    Public Sub TestSimpelSaveLeftOverTempFile()

        Dim testName = "TestSimpelSaveLeftOverTempFile.dat"
        Dim tmpPath = Path.GetTempPath()

        Dim testFileName = Path.Combine(tmpPath,testName)
        Dim testTmpFileName = Path.Combine(tmpPath,testName+".tmp")
        Dim testBakFileName = Path.Combine(tmpPath,testName+".backup")

        File.Delete(testFileName)
        File.Delete(testBakFileName)
        Using tmpF = File.CreateText(testTmpFileName)
            tmpF.WriteLine("Prutser")
        End Using

        Dim tc As New SerializeTestClass()
        tc.DummyString = "Hello World!"
        tc.DummyCounter = 42

        Serializing.SaveToFile(testFileName,tc)

        Dim reloaded = CType(Serializing.loadFromFile(testFileName), SerializeTestClass)


        Assert.IsTrue(  File.Exists(testFileName)    )
        Assert.IsFalse( File.Exists(testTmpFileName) )
        Assert.IsFalse( File.Exists(testBakFileName) )
        Assert.AreEqual(tc.DummyString,  reloaded.DummyString )
        Assert.AreEqual(tc.DummyCounter, reloaded.DummyCounter)

    End Sub


End Class