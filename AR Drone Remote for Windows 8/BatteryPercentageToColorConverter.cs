using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace AR_Drone_Remote_for_Windows_8
{
    class BatteryPercentageToColorConverter : IValueConverter
    {
        public int LowPowerThreshold { get; set; }
        public SolidColorBrush GoodStrengthColor { get; set; }
        public SolidColorBrush LowStrengthColor { get; set; }
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if ((uint)value <= LowPowerThreshold)
            {
                return LowStrengthColor.Color;
            }

            return GoodStrengthColor.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
