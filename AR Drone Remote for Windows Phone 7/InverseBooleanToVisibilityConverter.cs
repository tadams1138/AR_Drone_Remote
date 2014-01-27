using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = ConvertObjectToBool(value);
            return b ? Visibility.Collapsed : Visibility.Visible;
        }

        private static bool ConvertObjectToBool(object value)
        {
            if (value is bool)
            {
                return (bool)value;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
