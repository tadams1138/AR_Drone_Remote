using System.Windows;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class BatteryIndicator
    {
        public BatteryIndicator()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty BatteryPercentageProperty = DependencyProperty.Register(
            "BatteryPercentage", typeof (uint), typeof (BatteryIndicator), new PropertyMetadata((uint)0));

        public uint BatteryPercentage
        {
            get { return (uint)GetValue(BatteryPercentageProperty); }
            set { SetValue(BatteryPercentageProperty, value); }
        }
    }
}
