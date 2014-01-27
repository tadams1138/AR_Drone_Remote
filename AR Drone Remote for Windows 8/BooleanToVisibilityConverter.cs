using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace AR_Drone_Remote_for_Windows_8
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = ConvertObjectToBool(value);
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        private static bool ConvertObjectToBool(object value)
        {
            if (value is bool)
            {
                return (bool)value;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
