using AR_Drone_Controller.NavData;
using System;
using System.Collections.Generic;
using PropertyChanged;

namespace AR_Drone_Controller
{
    [ImplementPropertyChanged]
    public class DroneController : IProgressiveCommand
    {
        internal const uint NavDataOptions = (1 << (ushort)NavData.NavData.NavDataTag.Demo) +
                                             (1 <<
                                              (ushort)NavData.NavData.NavDataTag.HdVideoStream);

        internal const string VideoOnUsbConfigKey = "video:video_on_usb";
        internal const string FalseConfigValue = "FALSE";
        internal const string TrueConfigValue = "TRUE";
        internal const string VideoCodecConfigKey = "video:video_codec";
        internal const string GeneralVideoEnableConfigKey = "general:video_enable";
        internal const string VideoBitrateCtrlModeConfigKey = "video:bitrate_ctrl_mode";
        internal const string VideoVideoChannelConfigKey = "video:video_channel";
        internal const string GeneralNavdataDemo = "general:navdata_demo";
        internal const string GeneralNavdataOptionsConfigKey = "general:navdata_options";
        internal const string GpsLatitudeConfigKey = "gps:latitude";
        internal const string GpsLongitudeConfigCommand = "gps:longitude";
        internal const string GpsAltitudeConfigCommand = "gps:altitude";
        internal const int OptimalDelayBetweenCommandsInMilliseconds = 30;
        internal const string UserboxConfigKey = "userbox:userbox_cmd";
        internal const string UserBoxCommandDateFormat = "yyyyMMdd_HHmmss";
        internal static readonly string H264Codec720PConfigValue = String.Format("{0}", 0x82);

        public enum UserBoxCommands
        {
            Stop = 0,
            Start = 1,
            ScreenShot = 2,
            Cancel = 3
        }

        internal WorkerFactory WorkerFactory;
        internal TimerFactory TimerFactory;
        internal DoubleToInt64Converter DoubleToInt64Converter;
        internal CommandWorker CommandWorker;
        internal VideoWorker VideoWorker;
        internal NavDataWorker NavDataWorker;
        internal ControlWorker ControlWorker;
        internal IDisposable CommandTimer;
        internal DateTimeFactory DateTimeFactory;

        private readonly object _threadLock = new object();

        public DroneController()
        {
            DoubleToInt64Converter = new DoubleToInt64Converter();
            WorkerFactory = new WorkerFactory();
            DateTimeFactory = new DateTimeFactory();
            TimerFactory = new TimerFactory
            {
                TimerCallback = DoWork,
                Period = OptimalDelayBetweenCommandsInMilliseconds
            };
        }

        public ISocketFactory SocketFactory
        {
            set { WorkerFactory.SocketFactory = value; }
        }

        public ConnectParams ConnectParams { set { WorkerFactory.ConnectParams = value; } }

        public IDispatcher Dispatcher { get; set; }

        public Uri HelpUri
        {
            get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote_4115.html"); }
        }

        public Uri PrivacyUri
        {
            get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote-privacy-policy.html"); }
        }

        public List<LedAnimation> LedAnimations
        {
            get { return LedAnimation.GenerateLedAnimationList(); }
        }

        public List<FlightAnimation> FlightAnimations
        {
            get { return FlightAnimation.GenerateFlightAnimationList(); }
        }

        #region Control properties

        public virtual float Pitch { get; set; }

        public virtual float Roll { get; set; }

        public virtual float Yaw { get; set; }

        public virtual float Gaz { get; set; }

        public bool CombineYaw { get; set; }

        public float ControllerHeading { get; set; }

        public float ControllerHeadingAccuracy { get; set; }

        public bool AbsoluteControlMode { get; set; }

        #endregion

        #region Feedback properties

        public int Altitude { get; internal set; }

        public uint BatteryPercentage { get; internal set; }

        public float Theta { get; internal set; }

        public float Phi { get; internal set; }

        public float Psi { get; internal set; }

        public float KilometersPerHour { get; internal set; }

        public virtual bool Flying { get; internal set; }

        public virtual bool Connected { get; internal set; }

        public bool CanRecord { get; internal set; }

        public bool UsbKeyIsRecording { get; internal set; }

        public bool CanSendFlatTrimCommand { get; internal set; }

        internal bool CommWatchDog { get; set; }

        #endregion

        #region Commands

        public void Connect()
        {
            try
            {
                DisposeWorkersAndTimer();

                ControlWorker = WorkerFactory.CreateControlWorker();
                ControlWorker.Disconnected += ControlSocketOnDisconnected;

                CommandWorker = WorkerFactory.CreateCommandWorker();

                NavDataWorker = WorkerFactory.CreateNavDataWorker();
                NavDataWorker.NavDataReceived += NavDataWorkerOnNavDataReceived;

                ControlWorker.Run();
                CommandWorker.Run();

                CommandTimer = TimerFactory.CreateTimer();

                NavDataWorker.Run();

                QueueInitialCommands();

                Connected = true;
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        private void QueueInitialCommands()
        {
            CommandWorker.SendConfigCommand(GeneralVideoEnableConfigKey, TrueConfigValue);
            CommandWorker.SendConfigCommand(VideoBitrateCtrlModeConfigKey, "0");
            CommandWorker.SendConfigCommand(VideoVideoChannelConfigKey, "0");
            CommandWorker.SendConfigCommand(GeneralNavdataDemo, TrueConfigValue);
            CommandWorker.SendConfigCommand(GeneralNavdataOptionsConfigKey, NavDataOptions.ToString());
            CommandWorker.ExitBootStrapMode();
        }

        public void Disconnect()
        {
            lock (_threadLock)
            {
                DisposeWorkersAndTimer();
                ResetProperties();
            }
        }

        public virtual void TakeOff()
        {
            CommandWorker.SendRefCommand(CommandWorker.RefCommands.TakeOff);
        }

        public void CailbrateCompass()
        {
            CommandWorker.SendCalibrateCompassCommand();
        }

        public void FlatTrim()
        {
            CommandWorker.SendFlatTrimCommand();
        }

        public virtual void Land()
        {
            CommandWorker.SendRefCommand(CommandWorker.RefCommands.LandOrReset);
        }

        public virtual void Emergency()
        {
            CommandWorker.SendRefCommand(CommandWorker.RefCommands.Emergency);
        }

        public void SendLedAnimationCommand(ILedAnimation ledAnimation)
        {
            CommandWorker.SendLedAnimationCommand(ledAnimation);
        }

        public void SendFlightAnimationCommand(IFlightAnimation animation)
        {
            CommandWorker.SendFlightAnimationCommand(animation);
        }

        public void StartRecording()
        {
            DisposeVideoWorker();

            CommandWorker.SendConfigCommand(VideoOnUsbConfigKey, TrueConfigValue);
            CommandWorker.SendConfigCommand(VideoCodecConfigKey, H264Codec720PConfigValue);

            VideoWorker = WorkerFactory.CreateVideoWorker();
            VideoWorker.Run();
        }

        public void StopRecording()
        {
            CommandWorker.SendConfigCommand(VideoOnUsbConfigKey, FalseConfigValue);
            DisposeVideoWorker();
        }

        #endregion

        internal void DoWork(object state)
        {
            lock (_threadLock)
            {
                if (CommWatchDog)
                {
                    CommandWorker.SendResetWatchDogCommand();
                }
                else if (Flying)
                {
                    CommandWorker.SendProgressiveCommand(this);
                }

                CommandWorker.Flush();
            }
        }

        private void NavDataWorkerOnNavDataReceived(object sender, NavDataReceivedEventArgs e)
        {
            lock (_threadLock)
            {
                if (Connected)
                {
                    Dispatcher.BeginInvoke(() => UpdateProperties(e));
                }
            }
        }

        private void UpdateProperties(NavDataReceivedEventArgs e)
        {
            Altitude = e.NavData.Demo.Altitude;
            BatteryPercentage = e.NavData.Demo.BatteryPercentage;
            CanRecord = e.NavData.HdVideoStream.CanRecord;
            CanSendFlatTrimCommand = !e.NavData.Flying;
            Flying = e.NavData.Flying;
            UsbKeyIsRecording = e.NavData.HdVideoStream.UsbKeyIsRecording;
            Psi = e.NavData.Demo.Psi;
            Phi = e.NavData.Demo.Phi;
            Theta = e.NavData.Demo.Theta;
            KilometersPerHour = e.NavData.Demo.KilometersPerHour;
            CommWatchDog = e.NavData.CommWatchDog;
        }

        private void DisposeWorkersAndTimer()
        {
            DisposeCommandTimer();
            DisposeControlWorker();
            DisposeCommandWorker();
            DisposeNavDataWorker();
            DisposeVideoWorker();
        }

        private void DisposeVideoWorker()
        {
            if (VideoWorker != null)
            {
                VideoWorker.Dispose();
                VideoWorker = null;
            }
        }

        private void DisposeNavDataWorker()
        {
            if (NavDataWorker != null)
            {
                NavDataWorker.Dispose();
                NavDataWorker = null;
            }
        }

        private void DisposeCommandWorker()
        {
            if (CommandWorker != null)
            {
                CommandWorker.Dispose();
                CommandWorker = null;
            }
        }

        private void DisposeControlWorker()
        {
            if (ControlWorker != null)
            {
                ControlWorker.Dispose();
                ControlWorker = null;
            }
        }

        private void DisposeCommandTimer()
        {
            if (CommandTimer != null)
            {
                CommandTimer.Dispose();
                CommandTimer = null;
            }
        }

        private void ControlSocketOnDisconnected(object sender, EventArgs eventArgs)
        {
            Disconnect();
        }

        private void ResetProperties()
        {
            Altitude = 0;
            BatteryPercentage = 0;
            Theta = 0;
            Psi = 0;
            Phi = 0;
            KilometersPerHour = 0;
            Connected = false;
            CanSendFlatTrimCommand = false;
            Flying = false;
            CanRecord = false;
            UsbKeyIsRecording = false;
            CommWatchDog = false;
        }

        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        //private const int defaultDronePort = 23;
        //private const String droneConfigurationCommand = "cat /data/config.ini";

        //private void NavDataWorkerOnNavDataReceived(object sender, NavDataReceivedEventArgs e)
        //{
        //    NavData = e.NavData;
        //    if ((e.NavData.HdVideoStream == null || e.NavData.Demo == null) && CommandWorker != null)
        //    {
        //        const uint navDataOptions = (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.Demo) +
        //                                    (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.HdVideoStream);
        //        CommandWorker.CreateConfigCommand("general:navdata_demo", "TRUE");
        //        CommandWorker.CreateConfigCommand("general:navdata_options", navDataOptions.ToString());
        //    }
        //}

        //public void GetConfig()
        //{
        //    CommandWorker.SendCtrlCommand(CommandWorker.ControlMode.CfgGetControlMode);
        //}

        internal void SetLocation(double latitude, double longitude, double altitude)
        {
            Int64 convertedLatitude = DoubleToInt64Converter.Convert(latitude);
            Int64 convertedLongitude = DoubleToInt64Converter.Convert(longitude);
            Int64 convertedAltitude = DoubleToInt64Converter.Convert(altitude);

            CommandWorker.SendConfigCommand(GpsLatitudeConfigKey, convertedLatitude.ToString());
            CommandWorker.SendConfigCommand(GpsLongitudeConfigCommand, convertedLongitude.ToString());

            CommandWorker.SendConfigCommand(GpsAltitudeConfigCommand, convertedAltitude.ToString());
        }

        internal void UserBoxStart()
        {
            string now = DateTimeFactory.Now.ToString(UserBoxCommandDateFormat);
            var value = string.Format("{0},{1}", (int)UserBoxCommands.Start, now);
            CommandWorker.SendConfigCommand(UserboxConfigKey, value);
        }

        internal void UserBoxStop()
        {
            CommandWorker.SendConfigCommand(UserboxConfigKey, ((int)UserBoxCommands.Stop).ToString());
        }

        internal void UserBoxCancel()
        {
            CommandWorker.SendConfigCommand(UserboxConfigKey, ((int)UserBoxCommands.Cancel).ToString());
        }

        internal void UserBoxScreenShot(uint delayInSecondsBetweenScreenshots, uint numberOfScreenshotsToTake)
        {
            string now = DateTimeFactory.Now.ToString(UserBoxCommandDateFormat);
            var value = string.Format("{0},{1},{2},{3}", (int)UserBoxCommands.ScreenShot, delayInSecondsBetweenScreenshots,
                numberOfScreenshotsToTake, now);
            CommandWorker.SendConfigCommand(UserboxConfigKey, value);
        }
    }
}
