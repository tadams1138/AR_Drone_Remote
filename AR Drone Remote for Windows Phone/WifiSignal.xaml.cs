using System.Windows;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class WifiSignal
    {
        public WifiSignal()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(
            "Strength", typeof(int), typeof(WifiSignal), new PropertyMetadata(0));

        public int Strength
        {
            get { return (int)GetValue(StrengthProperty); }
            set { SetValue(StrengthProperty, value); }
        }
    }
}
