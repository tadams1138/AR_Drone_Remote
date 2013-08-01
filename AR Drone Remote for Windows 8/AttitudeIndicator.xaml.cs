using Windows.UI.Xaml;

namespace AR_Drone_Remote_for_Windows_8
{
    public partial class AttitudeIndicator
    {
        public AttitudeIndicator()
        {
            DataContext = this;
            InitializeComponent();
        }

        public static readonly DependencyProperty PhiProperty = DependencyProperty.Register(
            "Phi", typeof (float), typeof (AttitudeIndicator), new PropertyMetadata(0f));

        public float Phi
        {
            get { return (float)GetValue(PhiProperty); }
            set { SetValue(PhiProperty, value); }
        }

        public static readonly DependencyProperty ThetaProperty = DependencyProperty.Register(
            "Theta", typeof(float), typeof(AttitudeIndicator), new PropertyMetadata(0f));

        public float Theta
        {
            get { return (float)GetValue(ThetaProperty); }
            set { SetValue(ThetaProperty, value); }
        }
    }
}
