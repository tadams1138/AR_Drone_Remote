using System.Windows;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    public partial class CompassControl
    {
        public CompassControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DroneHeadingProperty = DependencyProperty.Register(
            "DroneHeading", typeof (double), typeof (CompassControl), new PropertyMetadata(0.0));

        public double DroneHeading
        {
            get { return (double)GetValue(DroneHeadingProperty); }
            set { SetValue(DroneHeadingProperty, value); }
        }

        public static readonly DependencyProperty ControllerHeadingProperty = DependencyProperty.Register(
            "ControllerHeading", typeof (double), typeof (CompassControl), new PropertyMetadata(0.0));

        public double ControllerHeading
        {
            get { return (double)GetValue(ControllerHeadingProperty); }
            set { SetValue(ControllerHeadingProperty, value); }
        }
    }
}