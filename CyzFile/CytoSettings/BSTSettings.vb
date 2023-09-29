

''' <summary>
''' Beads Sheath-Toxin system.
''' </summary>
''' <remarks>NOTE: Only present for backwards compatibility, no longer used.  THe whole BST system has been
''' replaced, but we want to be able to load older experiments, so we leave it in.
''' Made everything readonly, and removed all ctor's except the default one needed
''' for deserialization and made that one private.</remarks>
<Serializable()> Public Class BSTSettings
    Private _isPresent As Boolean
    Private _samplepumpBackFlushTime As Double
    Private _reHomogenisationTime As Double

    Private Sub New()
        _isPresent = False
    End Sub

    Public ReadOnly Property IsPresent As Boolean
        Get
            Return _isPresent
        End Get
    End Property

    ''' <summary>
    ''' For backwards compatibility we need to be able to create an empty, not present BST settings
    ''' object.  But I made the constructor private to avoid accidental use.  So I created this
    ''' function.  THat way we can create them if required, but accidental use is less likely.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function CreateEmptyNotPresent() As BSTSettings
        Return New BSTSettings()
    End Function

    Public Sub New(SamplePumpBackFlushTime As Double, ReHomogenisationTime As Double)
        _isPresent = True
        _samplepumpBackFlushTime = SamplePumpBackFlushTime
        _reHomogenisationTime = ReHomogenisationTime
    End Sub

    Public ReadOnly Property SamplePumpBackFlushTime As Double
        Get
            Return _samplepumpBackFlushTime
        End Get
    End Property

    Public ReadOnly Property ReHomogenisationTime As Double
        Get
            Return _reHomogenisationTime
        End Get
    End Property

End Class

