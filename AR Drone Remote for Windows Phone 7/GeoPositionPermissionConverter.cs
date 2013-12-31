using System;
using System.Device.Location;
using System.Globalization;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public class GeoPositionPermissionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (GeoPositionPermission) value == GeoPositionPermission.Granted;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
