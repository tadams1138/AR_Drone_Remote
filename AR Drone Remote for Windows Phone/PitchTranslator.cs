using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public class PitchTranslator : DependencyObject, IValueConverter
    {
        public double ImageHeight { get; set; }
        public double ImageWidth { get; set; }

        public static readonly DependencyProperty ActualWidthProperty = DependencyProperty.Register(
            "ActualWidth", typeof(double), typeof(PitchTranslator), new PropertyMetadata(0.0));

        public double ActualWidth
        {
            get { return (double)GetValue(ActualWidthProperty); }
            set { SetValue(ActualWidthProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ratio = ImageHeight * ActualWidth / (ImageWidth * 180.0);
            return ratio * (float)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
