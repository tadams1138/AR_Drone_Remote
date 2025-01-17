﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Runtime.CompilerServices;

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

    public partial class MainPage : INotifyPropertyChanged
    {
        private const string ShowLeftJoyStickPropertyName = "ShowLeftJoyStick";
        private static DroneController _droneController;
        private static Compass _compass;
        private static Accelerometer _accelerometer;
        private bool _useAccelerometer;
        private bool _showControls = true;

        private readonly List<string> _settingsProperties = new List<string>
        {
            "UseAccelerometer",
            "AbsoluteControl",
            //"UseLocationService",
            "RecordFlightData",
            "RecordScreenshotDelayInSeconds",
            "CombineYaw",
            "MaxAltitudeInMeters",
            "MaxDeviceTiltInDegrees",
            "ShellOn",
            "Outdoor",
            "MaxIndoorYawDegrees",
            "MaxOutdoorYawDegrees",
            "MaxIndoorRollOrPitchDegrees",
            "MaxOutdoorRollOrPitchDegrees",
            "MaxIndoorVerticalCmPerSecond",
            "MaxOutdoorVerticalCmPerSecond",
            "LockDroneHeadingToDeviceHeading"
        };

        public event PropertyChangedEventHandler PropertyChanged;

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
            //GeoCoordinateWatcher = new GeoCoordinateWatcher { MovementThreshold = 100 };
            //GeoCoordinateWatcher.PositionChanged += LocationPositionChanged;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            _droneController.Disconnect();

            if (_compass != null)
            {
                _compass.Stop();
            }

            if (_accelerometer != null && UseAccelerometer)
            {
                _accelerometer.Stop();
            }

            //if (UseLocationService)
            //{
            //    GeoCoordinateWatcher.Stop();
            //}
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadSettings();

            if (_compass != null)
            {
                _compass.Start();
            }

            if (_accelerometer != null && UseAccelerometer)
            {
                _accelerometer.Start();
            }

            //if (UseLocationService)
            //{
            //    GeoCoordinateWatcher.Start();
            //}

            NotifyIfTrial();
        }

        public DroneController DroneController { get { return _droneController; } }

        //public GeoCoordinateWatcher GeoCoordinateWatcher { get; set; }

        public bool CompassIsSupported
        {
            get { return Compass.IsSupported; }
        }

        public bool AccelerometerIsSupported
        {
            get { return Accelerometer.IsSupported; }
        }

        //public bool LocationServicesSupported
        //{
        //    get
        //    {
        //        return GeoCoordinateWatcher.Permission == GeoPositionPermission.Granted;
        //    }
        //}

        public bool UseAccelerometer
        {
            get { return _useAccelerometer; }

            set
            {
                SavePropertyValue(value);

                if (_useAccelerometer != value && Accelerometer.IsSupported)
                {
                    _useAccelerometer = value;
                    OnPropertyChanged(ShowLeftJoyStickPropertyName);

                    if (value)
                    {
                        _accelerometer.Start();
                    }
                    else
                    {
                        _accelerometer.Stop();
                        _droneController.Pitch = 0;
                        _droneController.Roll = 0;
                    }
                }
            }
        }

        public bool AbsoluteControl
        {
            get
            {
                return _droneController.AbsoluteControlMode;
            }

            set
            {
                SavePropertyValue(value);
                _droneController.AbsoluteControlMode = value;
            }
        }

        public bool LockDroneHeadingToDeviceHeading
        {
            get
            {
                return _droneController.LockDroneHeadingToDeviceHeading;
            }

            set
            {
                SavePropertyValue(value);
                _droneController.LockDroneHeadingToDeviceHeading = value;
            }
        }

        public bool CombineYaw
        {
            get
            {
                return DroneController.CombineYaw;
            }

            set
            {
                DroneController.CombineYaw = value;
                SavePropertyValue(value);
            }
        }

        //public bool UseLocationService
        //{
        //    get { return _droneController.CanSendLocationInformation; }
        //    set
        //    {
        //        SavePropertyValue(value);

        //        if (_droneController.CanSendLocationInformation != value)
        //        {
        //            _droneController.CanSendLocationInformation = value;

        //            if (value)
        //            {
        //                GeoCoordinateWatcher.Start();
        //            }
        //            else
        //            {
        //                GeoCoordinateWatcher.Stop();
        //            }
        //        }
        //    }
        //}

        public bool RecordFlightData
        {
            get { return DroneController.RecordFlightData; }
            set
            {
                SavePropertyValue(value);
                DroneController.RecordFlightData = value;
            }
        }

        public int RecordScreenshotDelayInSeconds
        {
            get { return DroneController.RecordScreenshotDelayInSeconds; }
            set
            {
                SavePropertyValue(value);
                DroneController.RecordScreenshotDelayInSeconds = value;
            }
        }

        public int MaxAltitudeInMeters
        {
            get { return DroneController.MaxAltitudeInMeters; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxAltitudeInMeters = value;
            }
        }

        public bool Outdoor
        {
            get { return DroneController.Outdoor; }
            set
            {
                SavePropertyValue(value);
                DroneController.Outdoor = value;
            }
        }

        public bool ShellOn
        {
            get { return DroneController.ShellOn; }
            set
            {
                SavePropertyValue(value);
                DroneController.ShellOn = value;
            }
        }

        public int MaxDeviceTiltInDegrees
        {
            get { return DroneController.MaxDeviceTiltInDegrees; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxDeviceTiltInDegrees = value;
            }
        }

        public int MaxIndoorYawDegrees
        {
            get { return DroneController.MaxIndoorYawDegrees; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxIndoorYawDegrees = value;
            }
        }

        public int MaxOutdoorYawDegrees
        {
            get { return DroneController.MaxOutdoorYawDegrees; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxOutdoorYawDegrees = value;
            }
        }

        public int MaxIndoorRollOrPitchDegrees
        {
            get { return DroneController.MaxIndoorRollOrPitchDegrees; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxIndoorRollOrPitchDegrees = value;
            }
        }

        public int MaxOutdoorRollOrPitchDegrees
        {
            get { return DroneController.MaxOutdoorRollOrPitchDegrees; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxOutdoorRollOrPitchDegrees = value;
            }
        }

        public int MaxIndoorVerticalCmPerSecond
        {
            get { return DroneController.MaxIndoorVerticalCmPerSecond; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxIndoorVerticalCmPerSecond = value;
            }
        }

        public int MaxOutdoorVerticalCmPerSecond
        {
            get { return DroneController.MaxOutdoorVerticalCmPerSecond; }
            set
            {
                SavePropertyValue(value);
                DroneController.MaxOutdoorVerticalCmPerSecond = value;
            }
        }

        public bool ShowControls
        {
            get
            {
                return _showControls;
            }

            set
            {
                if (_showControls != value)
                {
                    _showControls = value;
                    OnPropertyChanged();
                    OnPropertyChanged(ShowLeftJoyStickPropertyName);
                }
            }
        }

        public bool ShowLeftJoyStick
        {
            get { return ShowControls && !UseAccelerometer; }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SavePropertyValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName != null)
            {
                IsolatedStorageSettings.ApplicationSettings[propertyName] = value;
            }
        }

        private void InitializeAccelerometer()
        {
            if (AccelerometerIsSupported)
            {
                _accelerometer = new Accelerometer();
                _accelerometer.CurrentValueChanged += AccelerometerOnCurrentValueChanged;
            }
        }

        private void InitializeCompass()
        {
            if (CompassIsSupported)
            {
                _compass = new Compass();
                _compass.CurrentValueChanged += CompassOnCurrentValueChanged;
                _compass.Start();
            }
        }

        private void LoadSettings()
        {
            foreach (string propertyName in _settingsProperties)
            {
                LoadSettings(propertyName);
            }
        }

        private void LoadSettings(string propertyName)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(propertyName))
            {
                var type = GetType();
                PropertyInfo property = type.GetProperty(propertyName);
                property.SetValue(this, IsolatedStorageSettings.ApplicationSettings[propertyName]);
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

        private void Connect_OnClick(object sender, EventArgs e)
        {
            if (_droneController.Connected)
            {
                _droneController.Disconnect();
            }
            else
            {
                Connect();
            }
        }

        private static void Connect()
        {
            if (App.TrialReminder.IsTrialExpired)
            {
                NotifyIfTrial();
            }
            else
            {
                CallDroneControllConnect();
            }
        }

        private static void CallDroneControllConnect()
        {
            try
            {
                _droneController.Connect();
            }
            catch
            {
                MessageBox.Show(
                    "Could not connect to AR Drone. Please verify you are connected to drone's WIFI and that you are the only device connected. Restart the Drone if necessary.",
                    "Unable to connect.", MessageBoxButton.OK);
            }
        }

        private static void NotifyIfTrial()
        {
            try
            {
                App.TrialReminder.Notify();
            }
            catch
            {
            }
        }

        private void ShowOptions_OnClick(object sender, EventArgs e)
        {
            ShowControls = false;
        }

        private void ShowControls_OnClick(object sender, EventArgs e)
        {
            ShowControls = true;
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

        //private void LocationPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        //{
        //    Dispatcher.BeginInvoke(delegate
        //    {
        //        var l = e.Position.Location;
        //        DroneController.SetLocation(l.Latitude, l.Longitude, l.Altitude);
        //    });
        //}

        private void ResetSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ResetSettings();
        }

        private void ResetSettings()
        {
            DroneController.ResetSettings();
            SaveSettings();
            RaisePropertyChangedEventForAllSettingsProperties();
        }

        private void RaisePropertyChangedEventForAllSettingsProperties()
        {
            foreach (string propertyName in _settingsProperties)
            {
                OnPropertyChanged(propertyName);
            }
        }

        private void SaveSettings()
        {
            foreach (string propertyName in _settingsProperties)
            {
                var type = GetType();
                PropertyInfo property = type.GetRuntimeProperty(propertyName);
                var value = property.GetValue(this);
                SavePropertyValue(value, propertyName);
            }
        }
    }
}