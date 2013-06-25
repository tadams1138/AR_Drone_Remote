using System.Windows;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class AttitudeIndicator
    {
        public AttitudeIndicator()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PhiProperty = DependencyProperty.Register(
            "Phi", typeof (float), typeof (AttitudeIndicator), new PropertyMetadata(0f));

        public float Phi
        {
            get { return (float)GetValue(PhiProperty); }
            set { SetValue(ThetaProperty, value); }
        }

        public static readonly DependencyProperty ThetaProperty = DependencyProperty.Register(
            "Theta", typeof(float), typeof(AttitudeIndicator), new PropertyMetadata(0f));

        public float Theta
        {
            get { return (float)GetValue(ThetaProperty); }
            set { SetValue(PhiProperty, value); }
        }
    }
}
