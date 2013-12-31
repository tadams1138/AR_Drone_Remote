﻿using System;
using Windows.Devices.Geolocation;
using AR_Drone_Controller;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AR_Drone_Remote_for_Windows_8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public static DroneController StaticDroneController;
        private static Compass _compass;
        private static Accelerometer _accelerometer;
        private bool _useAccelerometer;
        private KeyboardInput _keyboardInput;
        private const float Gain = 1f;
        private bool _useLocationService;

        public Geolocator Geolocator { get; set; }

        public MainPage()
        {
            DroneController = new DroneController
            {
                SocketFactory = new SocketFactory(),
                ConnectParams = new ConnectParams(),
                Dispatcher = new DispatcherWrapper(Dispatcher)
            };

            InitializeComponent();

            InitializeKeyboardInput();
            InitializeCompass();
            InitializeAccelerometer();
            InitializeGeolocator();
        }

        public DroneController DroneController
        {
            get { return StaticDroneController; }
            set { StaticDroneController = value; }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DisplayProperties.AutoRotationPreferences = DisplayOrientations.Landscape;
        }

        private void InitializeAccelerometer()
        {
            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged += AccelerometerOnCurrentValueChanged;
                UseAccelerometer.Visibility = Visibility.Visible;
            }
            else
            {
                UseAccelerometer.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeCompass()
        {
            _compass = Compass.GetDefault();
            if (_compass != null)
            {
                _compass.ReadingChanged += CompassOnCurrentValueChanged;
                AbsoluteControl.Visibility = Visibility.Visible;
            }
            else
            {
                AbsoluteControl.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeKeyboardInput()
        {
            var keyStateIndicator = new KeyStateIndicator { CoreWindow = Window.Current.CoreWindow };
            _keyboardInput = new KeyboardInput
                {
                    DroneController = DroneController,
                    KeyStateIndicator = keyStateIndicator,
                    KeyMaps = KeyboardInput.GetDefautlKeyMap()
                };
            Window.Current.CoreWindow.KeyDown += KeyboardStateChanged;
            Window.Current.CoreWindow.KeyUp += KeyboardStateChanged;
        }
        private void InitializeGeolocator()
        {
            Geolocator = new Geolocator { MovementThreshold = 100 };
            Geolocator.StatusChanged += Geolocator_StatusChanged;
            Geolocator.PositionChanged += Geolocator_PositionChanged;
        }

        private void AccelerometerOnCurrentValueChanged(Accelerometer accelerometer, AccelerometerReadingChangedEventArgs args)
        {
            if (_useAccelerometer)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                {
                    if (SteerButton.IsPressed)
                    {
                        DroneController.Roll = (float)args.Reading.AccelerationY * -1;
                        DroneController.Pitch = (float)args.Reading.AccelerationX * -1;
                    }
                    else
                    {
                        DroneController.Roll = 0;
                        DroneController.Pitch = 0;
                    }
                });
            }
        }

        private void CompassOnCurrentValueChanged(Compass compass, CompassReadingChangedEventArgs args)
        {
            var heading = (float)args.Reading.HeadingMagneticNorth;
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CompassIndicator.ControllerHeading = heading);
            DroneController.ControllerHeading = heading;
        }

        private void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var l = args.Position.Coordinate;
                GeoLocationTextBlock.Text = string.Format("Lat: {0:0.0000}, Lon: {1:0.0000}, Alt: {2:0.0}", l.Latitude,
                    l.Longitude, l.Altitude);
                if (DroneController.Connected && _useLocationService)
                {
                    DroneController.SetLocation(l.Latitude, l.Longitude, l.Altitude ?? double.NaN);
                }
            });
        }

        void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool enabled = args.Status != PositionStatus.Disabled && args.Status != PositionStatus.NotAvailable;
                if (!enabled)
                {
                    SendLocationInformation.IsOn = false;
                }

                SendLocationInformation.IsEnabled = enabled;
            });
        }

        private void KeyboardStateChanged(CoreWindow sender, KeyEventArgs args)
        {
            args.Handled = false;
            _keyboardInput.UpdateDroneController();
        }

        private void LeftJoystickOnXValueChanged(object sender, double e)
        {
            DroneController.Roll = (float)e * Gain;
        }

        private void LeftJoystickOnYValueChanged(object sender, double e)
        {
            DroneController.Pitch = (float)e * Gain;
        }

        private void RightJoystickOnXValueChanged(object sender, double e)
        {
            DroneController.Yaw = (float)e * Gain;
        }

        private void RightJoystickOnYValueChanged(object sender, double e)
        {
            DroneController.Gaz = -(float)e * Gain;
        }

        private void LaunchLand_Click(object sender, RoutedEventArgs e)
        {
            if (!DroneController.Flying)
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

        private void AbsoluteControl_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            DroneController.AbsoluteControlMode = ((ToggleSwitch)sender).IsOn;
        }

        private void UseAccelerometer_OnCheckedChanged(object sender, RoutedEventArgs e)
        {
            _useAccelerometer = ((ToggleSwitch)sender).IsOn;
            if (_useAccelerometer)
            {
                LeftJoystick.Visibility = Visibility.Collapsed;
            }
            else
            {
                LeftJoystick.Visibility = Visibility.Visible;
                DroneController.Pitch = 0;
                DroneController.Roll = 0;
            }
        }

        private void Connect_OnClick(object sender, RoutedEventArgs routedEventArgs)
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
                    var dialog =
                        new MessageDialog(
                            "Could not connect to AR Drone. Please verify you are connected to the drone's WIFI and that you are the only device connected. If necessary, restart the drone.",
                            "Unable to connect.");
                    dialog.ShowAsync();
                }
            }
        }

        private void ShowOptions_OnClick(object sender, RoutedEventArgs e)
        {
            FlightControls.Visibility = Visibility.Collapsed;
            Tools.Visibility = Visibility.Visible;
        }

        private void ShowControls_OnClick(object sender, RoutedEventArgs e)
        {
            FlightControls.Visibility = Visibility.Visible;
            Tools.Visibility = Visibility.Collapsed;
        }

        private void LedCommands_OnItemClick(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var button = (Button)sender;
                var ledAnimation = (ILedAnimation)button.Content;
                DroneController.SendLedAnimationCommand(ledAnimation);
            }
            catch (Exception ex)
            {
                var dialog =
                    new MessageDialog(
                        ex.Message,
                        "Unable to send LED command.");
                dialog.ShowAsync();
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
            catch
            {
                var dialog =
                        new MessageDialog(
                            "Unable to send flight animation command.",
                            "Unable to connect.");
                dialog.ShowAsync();
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

        private void SendLocationInformation_Toggled(object sender, RoutedEventArgs e)
        {
            _useLocationService = ((ToggleSwitch)sender).IsOn;
        }

        private void UserBox_Start_Click(object sender, RoutedEventArgs e)
        {
            // TODO: test out these Userbox features
            DroneController.UserBoxStart();
        }

        private void UserBox_Stop_Click(object sender, RoutedEventArgs e)
        {
            DroneController.UserBoxStop();
        }

        private void UserBox_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DroneController.UserBoxCancel();
        }

        private void UserBox_Screenshot_Click(object sender, RoutedEventArgs e)
        {
            DroneController.UserBoxScreenShot(1,30);
        }
    }
}
