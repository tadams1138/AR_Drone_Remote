using System;
using System.Globalization;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public class DegreeToCompassHeadingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int degrees =  (int)Math.Truncate((double)value);
            switch (degrees)
            {
                case 0:
                    return "N";
                case 45:
                    return "NE";
                case 90:
                    return "E";
                case 135:
                    return "SE";
                case 180:
                    return "S";
                case 225:
                    return "SW";
                case 270:
                    return "W";
                case 315:
                    return "NW";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
