using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AR_Drone_Remote_for_Windows_Phone 
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public Brush TrueColor { get; set; }
        public Brush FalseColor { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                return TrueColor;
            }

            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
