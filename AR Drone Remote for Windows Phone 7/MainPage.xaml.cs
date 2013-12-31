using System.Device.Location;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    using AR_Drone_Controller;
    using Microsoft.Devices.Sensors;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Tasks;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using Telerik.Windows.Controls;

    public partial class MainPage
    {
        private static DroneController _droneController;
        private static Compass _compass;
        private static Accelerometer _accelerometer;
        private static bool _useAccelerometer;
        private bool _useLocationService;

        public GeoCoordinateWatcher GeoCoordinateWatcher { get; set; }

        public MainPage()
        {
            _droneController = new DroneController
            {
                SocketFactory = new SocketFactory(),
                ConnectParams = new ConnectParams(),
                Dispatcher = new DispatcherWrapper(Dispatcher)
            };

            InitializeComponent();
            InitializeCompass();
            InitializeAccelerometer();
            InitializeGeoCoordinateWatcher();
        }

        private void InitializeGeoCoordinateWatcher()
        {
            GeoCoordinateWatcher = new GeoCoordinateWatcher {MovementThreshold = 100};
            GeoCoordinateWatcher.PositionChanged += LocationPositionChanged;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            _droneController.Disconnect();

            if (_compass != null)
            {
                _compass.Stop();
            }

            if (_accelerometer != null && _useAccelerometer)
            {
                _accelerometer.Stop();
            }

            if (_useLocationService)
            {
                GeoCoordinateWatcher.Stop();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            VerifyLicense();

            if (_compass != null)
            {
                _compass.Start();
            }

            if (_accelerometer != null && _useAccelerometer)
            {
                _accelerometer.Start();
            }

            if (_useLocationService)
            {
                GeoCoordinateWatcher.Start();
            }
        }

        public DroneController DroneController { get { return _droneController; } }

        private void InitializeAccelerometer()
        {
            if (Accelerometer.IsSupported)
            {
                _accelerometer = new Accelerometer();
                _accelerometer.CurrentValueChanged += AccelerometerOnCurrentValueChanged;
                UseAccelerometer.Visibility = Visibility.Visible;
            }
            else
            {
                UseAccelerometer.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeCompass()
        {
            if (Compass.IsSupported)
            {
                _compass = new Compass();
                _compass.CurrentValueChanged += CompassOnCurrentValueChanged;
                AbsoluteControl.Visibility = Visibility.Visible;
                _compass.Start();
            }
            else
            {
                AbsoluteControl.Visibility = Visibility.Collapsed;
            }
        }

        private void MainPage_OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            try
            {
                VisualStateManager.GoToState(this, e.Orientation.ToString(), true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void AccelerometerOnCurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            Dispatcher.BeginInvoke(delegate
                {
                    if (SteerButton.IsPressed)
                    {
                        _droneController.Roll = e.SensorReading.Acceleration.Y * -1;
                        _droneController.Pitch = e.SensorReading.Acceleration.X * -1;
                    }
                    else
                    {
                        _droneController.Roll = 0;
                        _droneController.Pitch = 0;
                    }
                });
        }

        private void CompassOnCurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            var heading = (float)e.SensorReading.MagneticHeading + 90;
            Dispatcher.BeginInvoke(() => CompassIndicator.ControllerHeading = heading);
            _droneController.ControllerHeading = heading;
            _droneController.ControllerHeadingAccuracy = (float)e.SensorReading.HeadingAccuracy;
        }

        private void LeftJoystickOnXValueChanged(object sender, double e)
        {
            _droneController.Roll = (float)e;
        }

        private void LeftJoystickOnYValueChanged(object sender, double e)
        {
            _droneController.Pitch = (float)e;
        }

        private void RightJoystickOnXValueChanged(object sender, double e)
        {
            _droneController.Yaw = (float)e;
        }

        private void RightJoystickOnYValueChanged(object sender, double e)
        {
            _droneController.Gaz = -(float)e;
        }

        private void LaunchLand_Click(object sender, RoutedEventArgs e)
        {
            if (!_droneController.Flying)
            {
                _droneController.TakeOff();
            }
            else
            {
                _droneController.Land();
            }
        }

        private void Emergency_Click(object sender, RoutedEventArgs e)
        {
            _droneController.Emergency();
        }

        private void AbsoluteConstrol_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _droneController.AbsoluteControlMode = e.NewState;
        }

        private void UseAccelerometer_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _useAccelerometer = e.NewState;
            if (_useAccelerometer)
            {
                LeftJoystick.Visibility = Visibility.Collapsed;
                SteerButton.Visibility = Visibility.Visible;
                _accelerometer.Start();
            }
            else
            {
                _accelerometer.Stop();
                LeftJoystick.Visibility = Visibility.Visible;
                SteerButton.Visibility = Visibility.Collapsed;
                _droneController.Pitch = 0;
                _droneController.Roll = 0;
            }
        }

        private void Connect_OnClick(object sender, EventArgs e)
        {
            if (_droneController.Connected)
            {
                _droneController.Disconnect();
            }
            else
            {
                try
                {
                    _droneController.Connect();
                }
                catch
                {
                    MessageBox.Show("Could not connect to AR Drone. Please verify you are connected to drone's WIFI and that you are the only device connected. Restart the Drone if necessary.",
                                    "Unable to connect.", MessageBoxButton.OK);
                }
            }
        }

        private void ShowOptions_OnClick(object sender, EventArgs e)
        {
            LeftJoystick.Visibility = Visibility.Collapsed;
            RightJoystick.Visibility = Visibility.Collapsed;
            FlightControls.Visibility = Visibility.Collapsed;
            Tools.Visibility = Visibility.Visible;
        }

        private void ShowControls_OnClick(object sender, EventArgs e)
        {
            LeftJoystick.Visibility = UseAccelerometer.IsChecked ? Visibility.Collapsed : Visibility.Visible;
            RightJoystick.Visibility = Visibility.Visible;
            FlightControls.Visibility = Visibility.Visible;
            Tools.Visibility = Visibility.Collapsed;
        }

        private void HelpClick(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask { Uri = DroneController.HelpUri };
            webBrowserTask.Show();
        }

        private void PrivacyClick(object sender, EventArgs e)
        {
            var webBrowserTask = new WebBrowserTask { Uri = DroneController.PrivacyUri };
            webBrowserTask.Show();
        }

        private void RateAndReviewClick(object sender, EventArgs e)
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void LedAnimations_OnItemClick(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var button = (Button)sender;
                var ledAnimation = (ILedAnimation)button.Content;
                DroneController.SendLedAnimationCommand(ledAnimation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Unable to send LED command.", MessageBoxButton.OK);
            }
        }

        private void CalibrateCompass_Click(object sender, RoutedEventArgs e)
        {
            DroneController.CailbrateCompass();
        }

        private void FlightAnimations_OnItemClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = (Button)sender;
                var flightAnimation = (IFlightAnimation)button.Content;
                DroneController.SendFlightAnimationCommand(flightAnimation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                "Unable to send flight animation command.", MessageBoxButton.OK);
            }
        }

        private void FlatTrim_Click(object sender, RoutedEventArgs e)
        {
            DroneController.FlatTrim();
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (DroneController.UsbKeyIsRecording)
            {
                DroneController.StopRecording();
            }
            else
            {
                DroneController.StartRecording();
            }
        }

        private void VerifyLicense()
        {
            App.TrialReminder.Notify();
            if (ApplicationBar != null)
            {
                var connectButton = (ApplicationBarIconButton)ApplicationBar.Buttons[2];
                connectButton.IsEnabled = !App.TrialReminder.IsTrialExpired;
            }
        }

        void LocationPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Dispatcher.BeginInvoke(delegate
            {
                if (DroneController.Connected)
                {
                    var l = e.Position.Location;
                    DroneController.SetLocation(l.Latitude, l.Longitude, l.Altitude);
                }
            });
        }

        private void SendLocationInformation_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _useLocationService = e.NewState;
            if (e.NewState)
            {
                GeoCoordinateWatcher.Start();
            }
            else
            {
                GeoCoordinateWatcher.Stop();
            }
        }
    }
}