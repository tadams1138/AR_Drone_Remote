using System;
using System.ComponentModel;
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
        private const string RecordFlightDataPropertyName = "RecordFlightDataPropertyName";
        private const string RecordScreenshotDelayInSecondsPropertyName = "RecordScreenshotDelayInSeconds"; 
        private const string ShowLeftJoyStickPropertyName = "ShowLeftJoyStick";
        private const string UseAccelerometerPropertyName = "UseAccelerometer";
        private const string AbsoluteControlPropertyName = "AbsoluteControl";
        private const string UseLocationServicePropertyName = "UseLocationService";
        public static DroneController StaticDroneController;
        private static Compass _compass;
        private static Accelerometer _accelerometer;
        private bool _useAccelerometer;
        private KeyboardInput _keyboardInput;
        private const float Gain = 1f;
        private string _location;
        private bool _locationServicesSupported;
        private bool _showControls = true;

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
                ApplicationData.Current.LocalSettings.Values[UseAccelerometerPropertyName] = value;

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
                DroneController.AbsoluteControlMode = value;
                ApplicationData.Current.LocalSettings.Values[AbsoluteControlPropertyName] = value;
            }
        }

        public bool UseLocationService
        {
            get { return DroneController.CanSendLocationInformation; }
            set
            {
                ApplicationData.Current.LocalSettings.Values[UseLocationServicePropertyName] = value;
                DroneController.CanSendLocationInformation = value;
            }
        }

        public bool RecordFlightData
        {
            get { return DroneController.RecordFlightData; }
            set
            {
                ApplicationData.Current.LocalSettings.Values[RecordFlightDataPropertyName] = value;
                DroneController.RecordFlightData = value;
            }
        }

        public int RecordScreenshotDelayInSeconds
        {
            get { return DroneController.RecordScreenshotDelayInSeconds; }
            set
            {
                ApplicationData.Current.LocalSettings.Values[RecordScreenshotDelayInSecondsPropertyName] = value;
                DroneController.RecordScreenshotDelayInSeconds = value;
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

        private void LoadSettings()
        {
            SetUseAccelerometer();
            SetAbsoluteControl();
            SetUseLocationService();
            SetRecordFlightData();
            SetRecordScreenshotDelayInSeconds();
        }

        private void SetRecordFlightData()
        {
            var value = ApplicationData.Current.LocalSettings.Values[RecordFlightDataPropertyName];

            if (value != null)
            {
                RecordFlightData = (bool)value;
            }
        }

        private void SetRecordScreenshotDelayInSeconds()
        {
            var value = ApplicationData.Current.LocalSettings.Values[RecordScreenshotDelayInSecondsPropertyName];

            if (value != null)
            {
                RecordScreenshotDelayInSeconds = (int)value;
            }
        }

        private void SetUseLocationService()
        {
            var value = ApplicationData.Current.LocalSettings.Values[UseLocationServicePropertyName];

            if (value != null)
            {
                UseLocationService = (bool)value;
            }
        }

        private void SetAbsoluteControl()
        {
            var value = ApplicationData.Current.LocalSettings.Values[AbsoluteControlPropertyName];

            if (value != null)
            {
                AbsoluteControl = (bool)value;
            }
        }

        private void SetUseAccelerometer()
        {
            var value = ApplicationData.Current.LocalSettings.Values[UseAccelerometerPropertyName];

            if (value != null)
            {
                UseAccelerometer = (bool)value;
            }
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
