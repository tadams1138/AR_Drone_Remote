using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using AR_Drone_Controller;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AR_Drone_Remote_for_Windows_8.Annotations;

namespace AR_Drone_Remote_for_Windows_8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        private const string ShowLeftJoyStickPropertyName = "ShowLeftJoyStick";
        public static DroneController StaticDroneController;
        private static Compass _compass;
        private static Accelerometer _accelerometer;
        private bool _useAccelerometer;
        private KeyboardInput _keyboardInput;
        private const float Gain = 1f;
        private string _location;
        private bool _locationServicesSupported;
        private bool _showControls = true;

        private readonly List<string> _settingsProperties = new List<string>
        {
            "UseAccelerometer",
            "AbsoluteControl",
            "UseLocationService",
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
            "MaxOutdoorVerticalCmPerSecond"
        };

        public event PropertyChangedEventHandler PropertyChanged;

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

        public Geolocator Geolocator { get; set; }

        public bool CompassIsSupported
        {
            get
            {
                return _compass != null;
            }
        }

        public bool AccelerometerIsSupported
        {
            get { return _accelerometer != null; }
        }

        public bool LocationServicesSupported
        {
            get { return _locationServicesSupported; }
            private set
            {
                _locationServicesSupported = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get { return _location; }
            private set
            {
                _location = value;
                OnPropertyChanged();
            }
        }

        public bool UseAccelerometer
        {
            get { return _useAccelerometer; }

            set
            {
                SavePropertyValue(value);
                if (_useAccelerometer != value && AccelerometerIsSupported)
                {
                    _useAccelerometer = value;
                    OnPropertyChanged(ShowLeftJoyStickPropertyName);

                    if (!value)
                    {
                        DroneController.Pitch = 0;
                        DroneController.Roll = 0;
                    }
                }
            }
        }

        public bool AbsoluteControl
        {
            get
            {
                return DroneController.AbsoluteControlMode;
            }

            set
            {
                SavePropertyValue(value);
                DroneController.AbsoluteControlMode = value;
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
                SavePropertyValue(value);
                DroneController.CombineYaw = value;
            }
        }

        public bool UseLocationService
        {
            get { return DroneController.CanSendLocationInformation; }
            set
            {
                SavePropertyValue(value); 
                DroneController.CanSendLocationInformation = value;
            }
        }

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

        public int MaxDeviceTiltInDegrees
        {
            get { return DroneController.MaxDeviceTiltInDegrees; }
            set
            {
                SavePropertyValue(value); 
                DroneController.MaxDeviceTiltInDegrees = value;
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

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DisplayProperties.AutoRotationPreferences = DisplayOrientations.Landscape;
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
            LoadSettings();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            SettingsPane.GetForCurrentView().CommandsRequested -= OnCommandsRequested;
        }

        private void InitializeAccelerometer()
        {
            _accelerometer = Accelerometer.GetDefault();
            if (_accelerometer != null)
            {
                _accelerometer.ReadingChanged += AccelerometerOnCurrentValueChanged;
            }
        }

        private void InitializeCompass()
        {
            _compass = Compass.GetDefault();
            if (_compass != null)
            {
                _compass.ReadingChanged += CompassOnCurrentValueChanged;
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

        private void OnCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            var defaultsCommand = new SettingsCommand("settings", "Settings",
                handler =>
                {
                    var sf = new Settings(this);
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(defaultsCommand);
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
                Location = string.Format("Lat: {0:0.0000}, Lon: {1:0.0000}, Alt: {2:0.0}", l.Latitude,
                    l.Longitude, l.Altitude);
                DroneController.SetLocation(l.Latitude, l.Longitude, l.Altitude ?? double.NaN);
            });
        }

        private void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LocationServicesSupported = args.Status != PositionStatus.Disabled && args.Status != PositionStatus.NotAvailable;
                if (!LocationServicesSupported)
                {
                    UseLocationService = false;
                }
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
            ShowControls = false;
        }

        private void ShowControls_OnClick(object sender, RoutedEventArgs e)
        {
            ShowControls = true;
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

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
            var value = ApplicationData.Current.LocalSettings.Values[propertyName];
            if (value != null)
            {
                var type = GetType();
                PropertyInfo property = type.GetRuntimeProperty(propertyName);
                property.SetValue(this, value);
            }
        }

        private void SavePropertyValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName != null)
            {
                ApplicationData.Current.LocalSettings.Values[propertyName] = value;
            }
        }

        public void ResetSettings()
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
