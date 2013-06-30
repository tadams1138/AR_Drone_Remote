using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public class WifiColorConverter : IValueConverter
    {
        public SolidColorBrush LowStrengthColor { get; set; }
        public SolidColorBrush HighStrengthColor { get; set; }
        public double SegmentRange { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var segmentStart = double.Parse((string)parameter);
            var strength = (int)value;
            Brush result;

            if (strength < segmentStart)
            {
                result = LowStrengthColor;
            }
            else if (strength > segmentStart + SegmentRange)
            {
                result = HighStrengthColor;
            }
            else
            {
                double ratio = (strength - segmentStart) / SegmentRange;
                var newColor = GetNewColor(LowStrengthColor.Color, HighStrengthColor.Color, ratio);
                result = new SolidColorBrush(newColor);
            }

            return result;
        }

        private static Color GetNewColor(Color color1, Color color2, double ratio)
        {
            var newAlpha = GetNewColorComponent(color1.A, color2.A, ratio);
            var newRed = GetNewColorComponent(color1.R, color2.R, ratio);
            var newGreen = GetNewColorComponent(color1.G, color2.G, ratio);
            var newBlue = GetNewColorComponent(color1.B, color2.B, ratio);
            Color newColor = Color.FromArgb(newAlpha, newRed, newGreen, newBlue);
            return newColor;
        }

        private static byte GetNewColorComponent(byte color1, byte color2, double ratio)
        {
            int intResult = (byte)((color2 - color1) * ratio) + color1;
            var result = (byte)intResult;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}