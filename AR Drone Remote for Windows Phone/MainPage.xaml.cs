using AR_Drone_Controller;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System;
using System.Windows;
using Telerik.Windows.Controls;

namespace AR_Drone_Remote_for_Windows_Phone
{
    public partial class MainPage
    {
        private readonly BindableCompass _compass;
        private float gain = 1f;

        // Constructor
        public MainPage()
        {
            DroneController = new DroneController
            {
                SocketFactory = new SocketFactory(),
                Dispatcher = new DispatcherWrapper(Dispatcher)
            };

            _compass = new BindableCompass { Dispatcher = Dispatcher };
            _compass.CurrentValueChanged += CompassOnCurrentValueChanged;
            _compass.Start();

            InitializeComponent();
            
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void CompassOnCurrentValueChanged(object sender, CompassReading compassReading)
        {
            DroneController.ControllerHeading = (float)compassReading.MagneticHeading;
            DroneController.ControllerHeadingAccuracy = (float)compassReading.HeadingAccuracy;
        }

        private void RightJoystickOnYValueChanged(object sender, double d)
        {
            DroneController.Gaz = -(float)d * gain;
        }

        private void RightJoystickOnXValueChanged(object sender, double d)
        {
            DroneController.Yaw = (float)d * gain;
        }

        public BindableCompass ControllerDirection
        {
            get { return _compass; }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        public DroneController DroneController
        {
            get { return StaticDroneController; } 
            set { StaticDroneController = value; }
        }

        public static DroneController StaticDroneController;

        private void LaunchLand_Click(object sender, RoutedEventArgs e)
        {
            if (!DroneController.NavData.Flying)
            {
                DroneController.TakeOff();
            }
            else
            {
                DroneController.Land();
            }
        }

        private void Emergency_Click(object sender, RoutedEventArgs e)
        {
            DroneController.Emergency();
        }

        private void Blink_Click(object sender, RoutedEventArgs e)
        {
            DroneController.Blink();
        }

        private void MainPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                VisualStateManager.GoToState(this, e.Orientation.ToString(), true);
            }
            catch
            {
                // TODO: log this crap
            }
        }

        private void PrivacyClick(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote-privacy-policy.html") };
            webBrowserTask.Show();
        }

        private void RateAndReviewClick(object sender, EventArgs e)
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void Connect_OnClick(object sender, EventArgs e)
        {
            if (DroneController.Connected)
            {
                DroneController.Disconnect();
            }
            else
            {
                try
                {
                    DroneController.Connect();
                }
                catch
                {
                    MessageBox.Show("Could not connect to AR Drone. Please verify you are connected to drone's WIFI.",
                                    "Unable to connect.", MessageBoxButton.OK);
                }
            }
        }

        private void ShowOptions_OnClick(object sender, EventArgs e)
        {
            FlightControls.Visibility = Visibility.Collapsed;
            Tools.Visibility = Visibility.Visible;
        }

        private void ShowControls_OnClick(object sender, EventArgs e)
        {
            FlightControls.Visibility = Visibility.Visible;
            Tools.Visibility = Visibility.Collapsed;
        }

        private void Record_OnClick(object sender, EventArgs e)
        {
            // TODO
        }

        private void LockToCompass_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            DroneController.AbsoluteControlMode = e.NewState;
        }

        private void LeftJoystickOnXValueChanged(object sender, double e)
        {
            DroneController.Roll = (float)e * gain;
        }

        private void LeftJoystickOnYValueChanged(object sender, double e)
        {
            DroneController.Pitch = (float)e * gain;
        }

        private void UsePhoneTilt_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            LeftJoystick.Visibility = e.NewState ? Visibility.Collapsed : Visibility.Visible;
        }

        private void HelpClick(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask { Uri = new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote_4115.html") };
            webBrowserTask.Show();
        }
    }
}