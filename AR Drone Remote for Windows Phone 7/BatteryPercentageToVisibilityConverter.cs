using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public class BatteryPercentageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var threshold = uint.Parse((string)parameter);
            var strength = (uint) value;
            return strength < threshold ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}