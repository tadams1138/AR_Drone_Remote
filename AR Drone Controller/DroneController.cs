using AR_Drone_Controller.NavData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace AR_Drone_Controller
{
    public class DroneController : INotifyPropertyChanged, IDisposable
    {
        private const string DefaultDroneIp = "192.168.1.1";
        private const int ControlPort = 5559;

        private string _ipAddress = DefaultDroneIp;

        public ISocketFactory SocketFactory { get; set; }

        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        private ITcpSocket _controlSocket;

        //private const int defaultDronePort = 23;
        //private const String droneConfigurationCommand = "cat /data/config.ini";

        private CommandWorker _commandWorker;
        private VideoWorker _videoWorker;
        private NavDataWorker _navDataWorker;
        private NavData.NavData _navData = new NavData.NavData();

        private Timer _commandTimer;

        public static Uri HelpUri { get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote_4115.html"); } }

        public static Uri PrivacyUri { get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote-privacy-policy.html"); } }

        public DroneController()
        {
            ResetNavData();
        }

        public NavData.NavData NavData
        {
            get { return _navData; }
            set
            {
                _navData = value;
                NotifyPropertyChanged("NavData");
            }
        }

        private bool _connected;

        public bool Connected
        {
            get { return _connected; }
            private set
            {
                _connected = value;
                NotifyPropertyChanged("Connected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        public void Connect()
        {
            try
            {
                _controlSocket = SocketFactory.GetTcpSocket(_ipAddress, ControlPort);
                _controlSocket.Disconnected += ControlSocketOnDisconnected;
                _controlSocket.UnhandledException += UnhandledException;
                if (_controlSocket.Connected)
                {
                    using (var manualResetEvent = new ManualResetEvent(false))
                    {
                        InitializeCommandWorker();
                        manualResetEvent.WaitOne(100);
                        InitializeNavDataWorker();
                        manualResetEvent.WaitOne(100);

                        _commandWorker.SendConfigCommand("general:video_enable", "TRUE");
                        _commandWorker.SendConfigCommand("video:bitrate_ctrl_mode", "0");
                        _commandWorker.SendConfigCommand("video:video_channel", "0");

                        const uint navDataOptions = (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.Demo) +
                                            (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.HdVideoStream);
                        
                        _commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");
                        _commandWorker.SendConfigCommand("general:navdata_options", navDataOptions.ToString());

                        _commandWorker.SendFlatTrimCommand();

                        _commandWorker.ExitBootStrapMode();

                        manualResetEvent.WaitOne(100);
                        
                        _commandTimer = new Timer(DoWork, null, 30, 30);
                       
                        Connected = true;
                    }
                }
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        private void ControlSocketOnDisconnected(object sender, EventArgs eventArgs)
        {
            Disconnect();
        }

        private void InitializeCommandWorker()
        {
            string ardroneSessionId =
                ("T&I AR Drone Remote SessionId").GetHashCode().ToString("X").ToLowerInvariant();
            string ardroneProfileId =
                ("T&I AR Drone Remote ProfileId").GetHashCode().ToString("X").ToLowerInvariant();
            string ardroneApplicationId =
                ("T&I AR Drone Remote ApplicationId").GetHashCode().ToString("X").ToLowerInvariant();

            _commandWorker = new CommandWorker
                {
                    RemoteIpAddress = _ipAddress,
                    SocketFactory = SocketFactory,
                    ApplicationId = ardroneApplicationId,
                    ProfileId = ardroneProfileId,
                    SessionId = ardroneSessionId
                };
            _commandWorker.Run();

            _commandWorker.SendPModeCommand(2);
            _commandWorker.SendMiscellaneousCommand("2,20,2000,3000");

            _commandWorker.SendConfigCommand("custom:session_id", ardroneSessionId);
            _commandWorker.SendConfigCommand("custom:profile_id", ardroneProfileId);
            _commandWorker.SendConfigCommand("custom:application_id", ardroneApplicationId);
            _commandWorker.SendConfigCommand("custom:application_desc", "AR Drone Remote");
            _commandWorker.SendConfigCommand("custom:proﬁle_desc", ".Primary Profile");
            _commandWorker.SendConfigCommand("custom:session_desc", "Session " + ardroneSessionId);
        }

        private void InitializeNavDataWorker()
        {
            _navDataWorker = new NavDataWorker
                {
                    RemoteIpAddress = _ipAddress,
                    SocketFactory = SocketFactory
                };
            _navDataWorker.NavDataReceived += NavDataWorkerOnNavDataReceived;
            _navDataWorker.UnhandledException += UnhandledException;
            _navDataWorker.Run();
        }

        private void InitializeVideoWorker()
        {
            _videoWorker = new VideoWorker
            {
                RemoteIpAddress = _ipAddress,
                SocketFactory = SocketFactory
            };
            _videoWorker.UnhandledException += UnhandledException;

            _videoWorker.Run();
        }

        private void DoWork(object state)
        {
            if (_navData.CommWatchDog)
            {
                _commandWorker.SendResetWatchDogCommand();
            }
            else if (_navData.Flying)
            {
                var args = new CommandWorker.ProgressiveCommandArguments
                    {
                        AbsoluteControl = AbsoluteControlMode,
                        CombineYaw = CombineYaw,
                        Pitch = Pitch,
                        Roll = Roll,
                        Gaz = Gaz,
                        Yaw = Yaw,
                        MagnetoPsi = ControllerHeading,
                        MagnetoPsiAccuracy = ControllerHeadingAccuracy
                    };
                _commandWorker.SendProgressiveCommand(args);
            }
        }

        public void Disconnect()
        {
            Connected = false;
            ShutdownControlSocket();
            ShutdownCommandTimer();
            ShutdownCommandWorker();
            ShutdownNavDataWorker();
            ShutdownVideoWorker();
        }

        private void ResetNavData()
        {
            NavData = new NavData.NavData {Demo = new DemoOption(), HdVideoStream = new HdVideoStreamOption()};
        }

        private void ShutdownControlSocket()
        {
            if (_controlSocket != null)
            {
                _controlSocket.Dispose();
                _controlSocket = null;
            }
        }

        private void ShutdownCommandTimer()
        {
            if (_commandTimer != null)
            {
                _commandTimer.Dispose();
                _commandTimer = null;
            }
        }

        private void ShutdownCommandWorker()
        {
            if (_commandWorker != null)
            {
                _commandWorker.Stop();
                _commandWorker = null;
            }
        }

        private void ShutdownNavDataWorker()
        {
            if (_navDataWorker != null)
            {
                _navDataWorker.Stop();
                _navDataWorker = null;
            }

            ResetNavData();
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void NavDataWorkerOnNavDataReceived(object sender, NavDataReceivedEventArgs e)
        {
            NavData = e.NavData;
            if ((e.NavData.HdVideoStream == null || e.NavData.Demo == null) && _commandWorker != null)
            {
                const uint navDataOptions = (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.Demo) +
                                            (1 << (ushort)AR_Drone_Controller.NavData.NavData.NavDataTag.HdVideoStream);
                _commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");
                _commandWorker.SendConfigCommand("general:navdata_options", navDataOptions.ToString());
            }
        }

        public void Dispose()
        {
        }

        public void TakeOff()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.TakeOff);
        }

        public void CailbrateCompass()
        {
            _commandWorker.SendCalibrateCompassCommand();
        }

        public void FlatTrim()
        {
            _commandWorker.SendFlatTrimCommand();
        }

        public void Land()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.LandOrReset);
        }

        public float Pitch { get; set; }

        public float Roll { get; set; }

        public float Yaw { get; set; }

        public float Gaz { get; set; }

        public float ControllerHeading { get; set; }

        public float ControllerHeadingAccuracy { get; set; }

        public bool AbsoluteControlMode { get; set; }

        public IDispatcher Dispatcher { get; set; }

        public void Emergency()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.Emergency);
        }

        public void SendLedAnimationCommand(CommandWorker.LedAnimation animation, float frequencyInHz, int durationInSeconds)
        {
            if (_commandWorker == null)
            {
                throw new DroneControllerNotConnectedException();
            }

            _commandWorker.SendLedAnimationCommand(animation, frequencyInHz, durationInSeconds);
        }

        public void SendFlightAnimationCommand(CommandWorker.FlightAnimation animation, int maydayTimeoutInMilliseconds)
        {
            if (_commandWorker == null)
            {
                throw new DroneControllerNotConnectedException();
            }

            _commandWorker.SendFlightAnimationCommand(animation, maydayTimeoutInMilliseconds);
        }

        public bool CombineYaw { get; set; }

        public List<LedAnimation> LedAnimations
        {
            get { return LedAnimation.GenerateLedAnimationList(this); }
        }

        public List<FlightAnimation> FlightAnimations
        {
            get { return FlightAnimation.GenerateFlightAnimationList(this); }
        }

        public class DroneControllerNotConnectedException : Exception
        {
            public DroneControllerNotConnectedException()
                : base("The operation is invalid because the drone controller is not connected.")
            {
            }
        }

        public void StartRecording()
        {
            if (_commandWorker == null)
            {
                throw new DroneControllerNotConnectedException();
            }

            _commandWorker.SendConfigCommand("video:video_on_usb", "TRUE");
            _commandWorker.SendConfigCommand("video:video_codec", string.Format("{0}", 0x82)); // H264_720P_CODEC

            ShutdownVideoWorker();
            InitializeVideoWorker();
        }

        public void StopRecording()
        {
            if (_commandWorker == null)
            {
                throw new DroneControllerNotConnectedException();
            }

            _commandWorker.SendConfigCommand("video:video_on_usb", "FALSE");
            _commandWorker.SendConfigCommand("video:video_codec", string.Format("{0}", 0x81)); // H264_720P_CODEC
            ShutdownVideoWorker();
        }

        private void ShutdownVideoWorker()
        {
            if (_videoWorker != null)
            {
                _videoWorker.Stop();
                _videoWorker = null;
            }
        }

        public void GetConfig()
        {
            _commandWorker.SendCtrlCommand(CommandWorker.ControlMode.CfgGetControlMode);
        }
    }
}
