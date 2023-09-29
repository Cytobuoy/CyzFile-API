Imports System
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting

    ''' <summary>
    ''' Assert exceptions are thrown when expected.
    ''' </summary>
    ''' <remarks>
    ''' Based on http://blogs.msdn.com/b/ddietric/archive/2009/01/06/the-ultimate-exceptionassert-throws-method.aspx
    ''' </remarks>
    Public  Class ExceptionAssert
        ''' <summary>
        ''' The type parameter Is used for passing in the expected exception type.
        ''' The where clause Is used to ensure that T Is Exception Or a subclass.
        ''' Thus an explicit check Is Not necessary.
        ''' </summary>
        Public Shared Sub Throws(Of T As Exception)(action As Action, validator As Action(Of T) )
            If action Is Nothing Then
                Throw New ArgumentNullException("action")
            End If

            ' // Executing the action in a try block since we expect it to throw.
            Try
                action()
            
            Catch e As Exception ' Catching the exception regardless of its type.
                ' Comparing the type of the exception to the type of the type
                ' parameter, failing the assert in case they are different,
                ' even if the type of the exception is derived from the expected type.
                If e.GetType() <> GetType(T) Then
                    throw New AssertFailedException(String.Format(
                        CultureInfo.InvariantCulture,
                        "ExceptionAssert.Throws failed. Expected exception type: {0}. Actual exception type: {1}. Exception message: {2}",
                        GetType(T).FullName,
                        e.GetType().FullName,
                        e.Message))
                End If
                ' Calling the validator for the exception object if one was
                ' provided by the caller. The validator is expected to use the
                ' Assert class for performing the verification.
                If validator IsNot Nothing Then
                    validator( CType(e, T))
                End If

                ' Type check passed and validator did not throw.
                ' Everything is fine.
                Return
            End Try

            ' Failing the assert since there was no exception.
            throw new AssertFailedException(String.Format(
                CultureInfo.InvariantCulture,
                "ExceptionAssert.Throws failed. No exception was thrown (expected exception type: {0}).",
                GetType(T).FullName))
        End Sub

        Private Sub New()
            ' Prevent Instantiation
        End Sub
    End Class
