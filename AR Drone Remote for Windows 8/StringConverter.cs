using System;
using Windows.UI.Xaml.Data;

namespace AR_Drone_Remote_for_Windows_8
{
    public class StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var format = (string)parameter;
            return string.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
