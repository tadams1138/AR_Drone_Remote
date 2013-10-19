using System;
using Windows.UI.Xaml.Data;

namespace AR_Drone_Remote_for_Windows_8
{
    class AngleInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double result = 0.0;

            if (value is float)
            {
                result += (float)value;
            }
            else
            {
                result += (double)value;
            }

            result *= -1.0;
            result = NormalizeDegrees(result);
            return result;
        }

        private static double NormalizeDegrees(double degrees)
        {
            degrees %= 360;
            if (degrees < 0)
            {
                degrees += 360;
            }

            return degrees;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
