using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public class BatteryPercentageToColorConverter : IValueConverter
    {
        public int LowPowerThreshold { get; set; }
        public SolidColorBrush GoodStrengthColor { get; set; }
        public SolidColorBrush LowStrengthColor { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((uint)value <= LowPowerThreshold)
            {
                return LowStrengthColor.Color;
            }

            return GoodStrengthColor.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}