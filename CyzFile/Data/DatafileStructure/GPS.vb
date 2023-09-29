
Namespace Data.GPS

    ''' <summary>
    ''' This class implements the NMEA 0183 GPS protocol
    ''' </summary>
    ''' <remarks></remarks>
    <Serializable()> Public Class GPSCoordinate

        Private _string As String
        Private _timestamp As DateTime
        
        ''' <summary>
        ''' Construct with a separate NMEA 0183 GPS string
        ''' </summary>
        ''' <param name="s"></param>
        ''' <remarks></remarks>
        Public Sub New(s As String)
            _string = s
            _timestamp = Now
        End Sub

        ''' <summary>
        ''' For (a part of) a position, either latitude, or longitude.  The position is formatted in
        ''' degrees, minutes and decimals of minutes which is what is mostly used in navigation (at sea)
        ''' 
        ''' We have a separate function for latitude and longitude because they use a slightly
        ''' different format.,
        ''' </summary>
        ''' <param name="pos"></param>
        ''' <returns></returns>
        Private Shared Function FormatLongitude( pos As Double, NS As String) As String
            Dim degrees As Integer = CInt(Conversion.Int(pos))
            Dim minutes As Double  = Math.Round((pos - degrees)*60,1,MidpointRounding.AwayFromZero)
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0:000}°{1:00.0}'{2}",degrees,minutes,NS)
        End Function

        Private Shared Function FormatLatitude( pos As Double, NS As String) As String
            Dim degrees As Integer = CInt(Conversion.Int(pos))
            Dim minutes As Double  = Math.Round((pos - degrees)*60,1,MidpointRounding.AwayFromZero)
            Return String.Format(Globalization.CultureInfo.InvariantCulture, "{0:00}°{1:00.0}'{2}",degrees,minutes,NS)
        End Function

        ''' <summary>
        ''' Original complete GPS data strings start with GPGGA, but the GP part is the talker ID, and that can be different for other
        ''' sources such as Galileo, GLONAS, etc. Or in the case of a ship from MIO, it is IN, which is not an official ID I can find
        ''' anywhere.  SO I decided to not look for GPGGA anymore, instead I look for ??GGA, so any source will do.  That should work
        ''' better in the future.
        ''' </summary>
        Private Sub init()
            If String.IsNullOrEmpty(_string) Then 
                Return ' Empty string, definitely not a location.
            End If
            Dim ss() As String = _string.Split(","c)
            If ss.Length < 1 Then
                Return ' Split returned an empty array, nothing to check.
            End If
            If (ss(0).Substring(2) = "GGA") Then

                Try
                    'Latitude

                    Dim dLat As Double = Double.Parse(ss(2), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                    Dim latdegrees As Integer = CInt(Conversion.Int(dLat / 100))
                    Dim latdecimaldegrees As Double = (dLat - (100 * latdegrees)) / 60.0
                    _latitude = latdegrees + latdecimaldegrees
                    _latitude_string = FormatLatitude(_latitude, ss(3))  'ss(3).ToString() + _latitude.ToString(".######")
                    If ss(3).ToString() = "S" Then
                        'Southern hemisphere is negative values
                        _latitude = -_latitude
                    End If


                    ' Longitude
                    Dim dLon As Double = Double.Parse(ss(4), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                    Dim londegrees As Integer = CInt(Conversion.Int(dLon / 100))
                    Dim londecimaldegrees As Double = (dLon - (100 * londegrees)) / 60.0
                    _longitude = londegrees + londecimaldegrees
                    _longitude_string =FormatLongitude(_longitude, ss(5)) ' ss(5).ToString() + _longitude.ToString(".######")
                    If ss(5).ToString = "W" Then
                        _longitude = -_longitude
                    End If

                    _GPStime = ss(1)
                    _Quality = ss(6)
                    _nSatelites = ss(7)
                    _HDOP = ss(8)
                    _altitude = Double.Parse(ss(9))
                    _isGGA = True

                Catch
                    ' Can't Read GPS values, ignore error.
                End Try

            End If


            isInitialised = True
        End Sub

        <NonSerialized()> Private isInitialised As Boolean
        <NonSerialized()> Private _isGGA As Boolean
        <NonSerialized()> Private _latitude_string As String
        <NonSerialized()> Private _latitude As Double
        <NonSerialized()> Private _longitude_string As String
        <NonSerialized()> Private _longitude As Double
        <NonSerialized()> Private _altitude As Double
        <NonSerialized()> Private _GPStime As String
        <NonSerialized()> Private _Quality As String
        <NonSerialized()> Private _HDOP As String
        <NonSerialized()> Private _nSatelites As String




        Public ReadOnly Property isGGA As Boolean
            Get
                If Not isInitialised Then
                    init()
                End If
                Return _isGGA
            End Get
        End Property

        Public ReadOnly Property Latitude_string As String
            Get
                Return _latitude_string
            End Get
        End Property

        ''' <summary>
        ''' The longitudinal position in degrees. The western hemisphere is negative.
        ''' </summary>
        Public ReadOnly Property Longitude As Double
            Get
                Return _longitude
            End Get
        End Property

        ''' <summary>
        ''' The latitudinal (?) position in degrees. The southern hemisphere is negative.
        ''' </summary>
        Public ReadOnly Property Latitude As Double
            Get
                Return _latitude
            End Get
        End Property
        Public ReadOnly Property Longitude_string As String
            Get
                Return _longitude_string
            End Get
        End Property
        Public ReadOnly Property Altitude As Double
            Get
                Return _altitude
            End Get
        End Property
        Public ReadOnly Property GPSTime As String
            Get
                Return _GPStime
            End Get
        End Property

        ''' <summary>
        ''' Returns the raw gps data string as it was received from the GPS device
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property RawString As String
            Get
                Return _string
            End Get
        End Property

        ''' <summary>
        ''' Returns the timestamp as measured by the pc when receiving this coordinate set
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>Made it writeable so we can modify times when inserting GPS tracks into datafiles for testing.</remarks>
        Public Property TimeStamp As DateTime
            Get
                Return _timestamp
            End Get
            Set(value As DateTime)
                _timestamp = value
            End Set
        End Property

        ''' <summary>
        ''' The position as a point (pair of floats)
        ''' </summary>
        Public ReadOnly Property latlonPoint As System.Drawing.PointF
            Get
                Return New System.Drawing.PointF(CSng(Latitude), CSng(Longitude))
            End Get
        End Property


        Public Overrides Function ToString() As String
            If isGGA Then
                Return _GPStime & "; " & Latitude_string & "; " & Longitude_string
            Else
                Return "Not GGA"
            End If
        End Function


        ''' <summary>
        ''' Calculates the distance between 2 gps coordinate sets
        ''' </summary>
        ''' <param name="Lat1"></param>
        ''' <param name="Lon1"></param>
        ''' <param name="Lat2"></param>
        ''' <param name="Lon2"></param>
        ''' <returns>Returns distance in km</returns>
        ''' <remarks></remarks>
        Public Shared Function CalDistance(ByVal Lat1 As Double, ByVal Lon1 As Double, ByVal Lat2 As Double, ByVal Lon2 As Double) As Double
            'Haversine Formula
            Dim dLat As Double = toRadian(Lat2 - Lat1)
            Dim dLong As Double = toRadian(Lon2 - Lon1)
            Dim a As Double = Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0) + Math.Cos(toRadian(Lat1)) * Math.Cos(toRadian(Lat2)) * Math.Sin(dLong / 2.0) * Math.Sin(dLong / 2.0)
            Dim c As Double = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a)) '  2 * Math.Asin(Math.Min(1, Math.Sqrt(a))) is the same

            Return 6371 * c  '6371 ->radius earth
        End Function

        Private Shared Function toRadian(value As Double) As Double
            Return (Math.PI / 180) * value
        End Function

        Public Shared Function CalBearing(ByVal Lat1 As Double, ByVal Lon1 As Double, ByVal Lat2 As Double, ByVal Lon2 As Double) As Double
            Dim BearingRad As Double = Math.Atan2((Math.Sin(Lon2 - Lon1) * Math.Cos(Lat2)), Math.Cos(Lat1) * Math.Sin(Lat2) - Math.Sin(Lat1) * Math.Cos(Lat2) * Math.Cos(Lon2 - Lon1))
            Dim res As Double = (180.0 * BearingRad / Math.PI) Mod 360
            Return res
        End Function


    End Class



End Namespace
