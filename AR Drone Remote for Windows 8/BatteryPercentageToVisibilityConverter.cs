using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AR_Drone_Remote_for_Windows_8
{
    public class BatteryPercentageToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            var threshold = uint.Parse((string)parameter);
            var strength = (uint) value;
            return strength <= threshold ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}