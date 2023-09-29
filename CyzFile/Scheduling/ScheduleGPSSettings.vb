Namespace Scheduling

    ''' <summary>
    ''' This settings is currently implemented based on the use case that the instrument may not run in the harbor when running a autonomous schedule. 
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class ScheduleGPSSetting


        Public Sub New(lat As Double, lon As Double, range As Double)
            _latitude = lat
            _longitude = lon
            _Range = range
        End Sub

        ''' <summary>
        ''' Checks if the current position is within the forbidden _range
        ''' </summary>
        ''' <param name="currentposition"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function allowedToRun(currentposition As CytoSense.Data.GPS.GPSCoordinate) As Boolean
            If currentposition IsNot Nothing AndAlso currentposition.isGGA Then
                Dim dis As Double = CytoSense.Data.GPS.GPSCoordinate.CalDistance(latitude, longitude, currentposition.Latitude, currentposition.Longitude)
                Return dis > _Range
            Else
                Return True ' no gps fix or something, we don't know where we are, so just allow.
            End If
        End Function

        Public Property latitude As Double
        Public Property longitude As Double
        Public Property Range As Double


    End Class
End Namespace
