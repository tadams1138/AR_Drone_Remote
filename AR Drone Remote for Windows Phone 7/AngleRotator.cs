using System;
using System.Globalization;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public class AngleRotator : IValueConverter
    {
        private double _factor = 1.0;

        public double Offset { get; set; }

        public bool InvertDegrees
        {
            get { return _factor < 0; }
            set { _factor = value ? -1.0 : 1.0; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = Offset;

            if (value is float)
            {
                result += (float) value;
            }
            else
            {
                result += (double) value;
            }

            result *= _factor;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
