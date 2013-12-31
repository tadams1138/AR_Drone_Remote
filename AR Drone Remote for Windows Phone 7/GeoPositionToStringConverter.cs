using System;
using System.Globalization;
using System.Windows.Data;
using Windows.Devices.Geolocation;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public class GeoPositionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var l = (Geocoordinate)value;
            return string.Format("Lat: {0:0.0000}, Lon: {1:0.0000}, Alt: {2:0.0}", l.Latitude, l.Longitude, l.Altitude);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
