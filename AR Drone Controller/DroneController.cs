using System.Threading;
using System.Threading.Tasks;
using AR_Drone_Controller.NavData;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AR_Drone_Controller
{
    public class DroneController : INotifyPropertyChanged, IDisposable
    {
        private const string DefaultDroneIp = "192.168.1.1";
        private const int VideoPort = 5555;
        private const int ControlPort = 5559;

        private const string _ipAddress = DefaultDroneIp;
        public ISocketFactory SocketFactory { get; set; }

        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        private ITcpSocket _controlSocket;

        private const int defaultDronePort = 23;
        private const String droneConfigurationCommand = "cat /data/config.ini";

        private NavDataWorker _navDataWorker;
        private NavData.NavData _navData = new NavData.NavData();

        private Task _worker;     

        public NavData.NavData NavData
        {
            get { return _navData; }
            set
            {
                _navData = value;
                NotifyPropertyChanged();
            }
        }

        private bool _connected;

        public bool Connected
        {
            get { return _connected; } 
            private set
            {
                _connected = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private CommandWorker _commandWorker;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                Dispatcher.BeginInvoke(() => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
            }
        }

        public void Connect()
        {
            string ardroneSessionId = ("T&I AR Drone Remote SessionId").GetHashCode().ToString("X").ToLowerInvariant();     // SessionID
            string ardroneProfileId = ("T&I AR Drone Remote ProfileId").GetHashCode().ToString("X").ToLowerInvariant();    // Profile ID
            string ardroneApplicationId = ("T&I AR Drone Remote ApplicationId").GetHashCode().ToString("X").ToLowerInvariant();      // Application ID

            try
            {
                _controlSocket = SocketFactory.GetTcpSocket(_ipAddress, ControlPort);
                if (_controlSocket.Connected)
                {
                    var clientDone = new ManualResetEvent(false);

                    _navDataWorker = new NavDataWorker
                    {
                        RemoteIpAddress = _ipAddress,
                        SocketFactory = SocketFactory
                    };
                    _navDataWorker.NavDataReceived += NavDataWorkerOnNavDataReceived;
                    _navDataWorker.NavDataUnhandledException += NavDataWorkerOnNavDataUnhandledException;
                    _navDataWorker.Run();

                    clientDone.WaitOne(100);

                    _commandWorker = new CommandWorker
                        {
                            RemoteIpAddress = _ipAddress,
                            SocketFactory = SocketFactory,
                            ApplocationId = ardroneApplicationId,
                            ProfileId = ardroneProfileId,
                            SessionId = ardroneSessionId
                        };
                    _commandWorker.Run();

                    clientDone.WaitOne(100);

                    _commandWorker.SendPModeCommand(2);
                    _commandWorker.SendMiscellaneousCommand(string.Format("{0},{1},{2},{3}", 2, 20, 2000, 3000));
                    _commandWorker.SendConfigCommand("custom:session_id", ardroneSessionId);
                    _commandWorker.SendConfigCommand("custom:profile_id", ardroneProfileId);
                    _commandWorker.SendConfigCommand("custom:application_id", ardroneApplicationId);

                    _commandWorker.SendConfigCommand("general:video_enable", "FALSE");
                    //_commandWorker.SendConfigCommand("video:bitrate_ctrl_mode", "0");
                    //_commandWorker.SendConfigCommand("video:video_codec", string.Format("{0}", 0x81)); // H264_360P_CODEC

                    //_commandWorker.SendConfigCommand("video:video_channel", "0");

                    _commandWorker.SendFtrimCommand();

                    // _commandWorker.SendConfigCommand("general:navdata_options", "105971713");
                    _commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");

                    //_commandWorker.SendAck();

                    _worker = Task.Factory.StartNew(DoWork);

                    Connected = true;
                }
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        private void DoWork()
        {
            using (var manualResetEvent = new ManualResetEvent(false))
            {
                do
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

                    manualResetEvent.WaitOne(30);
                } while (_controlSocket != null);
            }
        }

        public void Disconnect()
        {
            if (_controlSocket != null)
            {
                _controlSocket.Dispose();
                _controlSocket = null;
            }

            if (_worker != null)
            {
                _worker.Wait(1000);
                _worker = null;
            }

            if (_commandWorker != null)
            {
                _commandWorker.Stop();
                _commandWorker = null;
            }

            if (_navDataWorker != null)
            {
                _navDataWorker.Stop();
                _navDataWorker = null;
            }

            Connected = false;
        }

        private void NavDataWorkerOnNavDataUnhandledException(object sender, NavDataUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NavDataWorkerOnNavDataReceived(object sender, NavDataReceivedEventArgs e)
        {
            NavData = e.NavData;
            if (e.NavData.Demo == null && _commandWorker != null)
            {
                _commandWorker.SendConfigCommand("general:navdata_demo", "FALSE");
                _commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");
            }
        }

        public void Dispose()
        {
        }

        public void TakeOff()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.TakeOff);
           // _commandWorker.SendCalibrationCommand();
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

        public void Blink()
        {
            _commandWorker.SendLedCommand(CommandWorker.LedAnimation.BlinkRed, 0.5f, 5);
        }

        public bool CombineYaw { get; set; }
    }
}
