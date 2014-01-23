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
        internal static readonly string H264_360P_CodecConfigValue = String.Format("{0}", 0x81);
        internal static readonly string Mp4_360p_H264_720p_CodecConfigValue = String.Format("{0}", 0x82);

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
        private bool _canSendLocationInformation;
        private long _convertedLatitude;
        private long _convertedLongitude;
        private long _convertedAltitude;
        private bool _recordFlightData;

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

            RecordScreenshotDelayInSeconds = 1;
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

        public bool CanSendLocationInformation
        {
            get { return _canSendLocationInformation; }
            set
            {
                if (_canSendLocationInformation != value)
                {
                    _canSendLocationInformation = value;
                    if (value && Connected)
                    {
                        SendLocationInformation();
                    }
                }
            }
        }

        public bool RecordFlightData
        {
            get { return _recordFlightData; }
            set
            {
                if (_recordFlightData != value)
                {
                    _recordFlightData = value;

                    if (Connected)
                    {
                        if (value)
                        {
                            SendUserBoxStartAndScreenShotCommands();
                        }
                        else
                        {
                            SendUserBoxStopCommand();
                        }
                    }
                }
            }
        }

        public int RecordScreenshotDelayInSeconds { get; set; }

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
            RequestNavData();
            CommandWorker.ExitBootStrapMode();
            if (CanSendLocationInformation)
            {
                SendLocationInformation();
            }

            if (RecordFlightData)
            {
                SendUserBoxStartAndScreenShotCommands();
            }
            else
            {
                SendUserBoxStopCommand();
            }
        }

        private void SendUserBoxStartAndScreenShotCommands()
        {
            UserBoxStart();
            SendUserBoxScreenShot(RecordScreenshotDelayInSeconds, 86400);
        }

        private void RequestNavData()
        {
            CommandWorker.SendConfigCommand(GeneralNavdataDemo, TrueConfigValue);
            CommandWorker.SendConfigCommand(GeneralNavdataOptionsConfigKey, NavDataOptions.ToString());
        }

        public void Disconnect()
        {
            lock (_threadLock)
            {
                if (RecordFlightData && Connected)
                {
                    SendUserBoxStopCommand();
                }

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
            CommandWorker.SendConfigCommand(VideoCodecConfigKey, Mp4_360p_H264_720p_CodecConfigValue);

            VideoWorker = WorkerFactory.CreateVideoWorker();
            VideoWorker.Run();
        }

        public void StopRecording()
        {
            CommandWorker.SendConfigCommand(VideoOnUsbConfigKey, FalseConfigValue);
            CommandWorker.SendConfigCommand(VideoCodecConfigKey, H264_360P_CodecConfigValue);
            DisposeVideoWorker();
        }

        #endregion

        internal void DoWork(object state)
        {
            lock (_threadLock)
            {
                if (Connected)
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
            CanSendFlatTrimCommand = !e.NavData.Flying;
            Flying = e.NavData.Flying;
            CommWatchDog = e.NavData.CommWatchDog;

            if (e.NavData.Demo != null)
            {
                Altitude = e.NavData.Demo.Altitude;
                BatteryPercentage = e.NavData.Demo.BatteryPercentage;
                Psi = e.NavData.Demo.Psi;
                Phi = e.NavData.Demo.Phi;
                Theta = e.NavData.Demo.Theta;
                KilometersPerHour = e.NavData.Demo.KilometersPerHour;
            }

            if (e.NavData.HdVideoStream != null)
            {
                CanRecord = e.NavData.HdVideoStream.CanRecord;
                UsbKeyIsRecording = e.NavData.HdVideoStream.UsbKeyIsRecording;
            }

            if (e.NavData.Demo == null || e.NavData.HdVideoStream == null)
            {
                RequestNavData();
            }
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

        public void SetLocation(double latitude, double longitude, double altitude)
        {
            _convertedLatitude = DoubleToInt64Converter.Convert(latitude);
            _convertedLongitude = DoubleToInt64Converter.Convert(longitude);
            _convertedAltitude = DoubleToInt64Converter.Convert(altitude);
            if (CanSendLocationInformation && Connected)
            {
                SendLocationInformation();
            }
        }

        private void SendLocationInformation()
        {
            CommandWorker.SendConfigCommand(GpsLatitudeConfigKey, _convertedLatitude.ToString());
            CommandWorker.SendConfigCommand(GpsLongitudeConfigCommand, _convertedLongitude.ToString());
            CommandWorker.SendConfigCommand(GpsAltitudeConfigCommand, _convertedAltitude.ToString());
        }

        private void UserBoxStart()
        {
            string now = DateTimeFactory.Now.ToString(UserBoxCommandDateFormat);
            var value = string.Format("{0},{1}", (int)UserBoxCommands.Start, now);
            CommandWorker.SendConfigCommand(UserboxConfigKey, value);
        }

        private void SendUserBoxStopCommand()
        {
            CommandWorker.SendConfigCommand(UserboxConfigKey, ((int)UserBoxCommands.Stop).ToString());
        }

        private void SendUserBoxScreenShot(int delayInSecondsBetweenScreenshots, uint numberOfScreenshotsToTake)
        {
            string now = DateTimeFactory.Now.ToString(UserBoxCommandDateFormat);
            var value = string.Format("{0},{1},{2},{3}", (int)UserBoxCommands.ScreenShot, delayInSecondsBetweenScreenshots,
                numberOfScreenshotsToTake, now);
            CommandWorker.SendConfigCommand(UserboxConfigKey, value);
        }



        // Academy FTP "parrot01.nyx.emencia.net", port 21
        // URL to signup: http://ardrone2.parrot.com/ar-drone-academy/

        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        //private const int defaultDronePort = 23;
        //private const String droneConfigurationCommand = "cat /data/config.ini";

        //public void GetConfig()
        //{
        //    CommandWorker.SendCtrlCommand(CommandWorker.ControlMode.CfgGetControlMode);
        //}
    }
}
