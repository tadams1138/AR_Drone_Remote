using AR_Drone_Controller.NavData;
using System;
using System.Collections.Generic;
using System.Threading;
using PropertyChanged;

namespace AR_Drone_Controller
{
    [ImplementPropertyChanged]
    public class DroneController : IDisposable
    {
        public const string DefaultDroneIp = "192.168.1.1";


        private string _ipAddress = DefaultDroneIp;

        public ISocketFactory SocketFactory { get; set; }

        internal WorkerFactory WorkerFactory;

        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        private ITcpSocket _controlSocket;

        //private const int defaultDronePort = 23;
        //private const String droneConfigurationCommand = "cat /data/config.ini";

        internal CommandWorker _commandWorker;
        internal VideoWorker _videoWorker;
        internal NavDataWorker _navDataWorker;
        internal ControlWorker _controlWorker;

        private Timer _commandTimer;

        public static Uri HelpUri { get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote_4115.html"); } }

        public static Uri PrivacyUri { get { return new Uri("http://tadams1138.blogspot.com/p/ar-drone-remote-privacy-policy.html"); } }

        public DroneController()
        {
            WorkerFactory = new WorkerFactory();
            ResetNavData();
        }

        public int Altitude { get; internal set; }

        public uint BatteryPercentage { get; internal set; }

        public float Theta { get; internal set; }

        public float Phi { get; internal set; }

        public float Psi { get; internal set; }

        public float KilometersPerHour { get; internal set; }

        public bool Flying { get; internal set; }

        public NavData.NavData NavData { get; internal set; }

        public bool Connected { get; internal set; }

        public bool CanRecord { get; internal set; }
        
        public bool UsbKeyIsRecording { get; internal set; }

        public bool CanSendFlatTrimCommand { get; internal set; }
        // get { return Connected && NavData != null && !NavData.Flying; }

        public void Connect(ConnectParams connectParams)
        {
            try
            {
                DisposeWorkers();

                _controlWorker = WorkerFactory.CreateControlWorker(SocketFactory, connectParams);
                _commandWorker = WorkerFactory.CreateCommandWorker(SocketFactory, connectParams);
                _navDataWorker = WorkerFactory.CreateNavDataWorker(SocketFactory, connectParams);

                //var getTcpSocketParams = new GetTcpSocketParams
                //    {
                //        IpAddress = connectParams.NetworkAddress,
                //        Port = connectParams.ControlPort
                //    };
                //_controlSocket = SocketFactory.GetTcpSocket(getTcpSocketParams);
                //_controlSocket.Disconnected += ControlSocketOnDisconnected;
                //_controlSocket.UnhandledException += UnhandledException;
                //_controlSocket.Connect();

                using (var manualResetEvent = new ManualResetEvent(false))
                {
                    //InitializeCommandWorker();
                    //manualResetEvent.WaitOne(100);
                    //InitializeNavDataWorker();
                    //manualResetEvent.WaitOne(100);

                    //_commandWorker.SendConfigCommand("general:video_enable", "TRUE");
                    //_commandWorker.SendConfigCommand("video:bitrate_ctrl_mode", "0");
                    //_commandWorker.SendConfigCommand("video:video_channel", "0");

                    //const uint navDataOptions = (1 << (ushort) AR_Drone_Controller.NavData.NavData.NavDataTag.Demo) +
                    //                            (1 <<
                    //                             (ushort) AR_Drone_Controller.NavData.NavData.NavDataTag.HdVideoStream);

                    //_commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");
                    //_commandWorker.SendConfigCommand("general:navdata_options", navDataOptions.ToString());

                    //_commandWorker.ExitBootStrapMode();

                    //manualResetEvent.WaitOne(100);

                    //_commandTimer = new Timer(DoWork, null, 30, 30);

                    Connected = true;
                }
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        private void DisposeWorkers()
        {
            if (_controlWorker != null)
            {
                _controlWorker.Dispose();
                _controlWorker = null;
            }

            if (_commandWorker != null)
            {
                _commandWorker.Dispose();
                _commandWorker = null;
            }

            if (_navDataWorker != null)
            {
                _navDataWorker.Dispose();
                _navDataWorker = null;
            }

            if (_videoWorker != null)
            {
                _videoWorker.Dispose();
                _videoWorker = null;
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
            if (NavData.CommWatchDog)
            {
                _commandWorker.SendResetWatchDogCommand();
            }
            else if (NavData.Flying)
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
            //ShutdownControlSocket();
            //ShutdownCommandTimer();
            //ShutdownCommandWorker();
            //ShutdownNavDataWorker();
            //ShutdownVideoWorker();

            DisposeWorkers();
            ResetProperties();
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
        }

        private void ResetNavData()
        {
            NavData = new NavData.NavData { Demo = new DemoOption(), HdVideoStream = new HdVideoStreamOption() };
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

        public virtual float Pitch { get; set; }

        public virtual float Roll { get; set; }

        public virtual float Yaw { get; set; }

        public virtual float Gaz { get; set; }

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
