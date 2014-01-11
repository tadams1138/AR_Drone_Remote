using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;


namespace AR_Drone_Remote_for_Windows_8
{
    [ContentProperty(Name = "Content")]
    public partial class BatteryIndicator
    {
        public BatteryIndicator()
        {
            InitializeComponent();
        }

        public int CurrentTransformation
        {
            get { return (int)GetValue(CurrentTransformationProperty); }
            private set { SetValue(CurrentTransformationProperty, value); }
        }

        public static readonly DependencyProperty CurrentTransformationProperty =
            DependencyProperty.Register("CurrentTransformation", typeof(int), typeof(BatteryIndicator), new PropertyMetadata(null));



        public static readonly DependencyProperty BatteryPercentageProperty = DependencyProperty.Register(
            "BatteryPercentage", typeof (uint), typeof (BatteryIndicator), new PropertyMetadata((uint)0));

        public uint BatteryPercentage
        {
            get { return (uint)GetValue(BatteryPercentageProperty); }
            set { SetValue(BatteryPercentageProperty, value); }
        }
    }
}
