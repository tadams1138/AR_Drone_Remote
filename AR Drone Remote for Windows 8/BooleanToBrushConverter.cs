using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace AR_Drone_Remote_for_Windows_8 
{
    class BooleanToBrushConverter : IValueConverter
    {
        public Brush TrueColor { get; set; }
        public Brush FalseColor { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (bool) value)
            {
                return TrueColor;
            }
            
            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
