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
        private const int VideoPort = 5555;
        private const int ControlPort = 5559;

        private static int seq = 1;

        public string IpAddress { get; set; }
        public ISocketFactory SocketFactory { get; set; }
        
        // videoSocket udp for AR Drone 1 and tcp for AR Drone 2
        private ITcpSocket _controlSocket;

        private const int timeoutValue = 500;

        private const int defaultDronePort = 23;
        private const String droneConfigurationCommand = "cat /data/config.ini";

        private NavDataWorker _navDataWorker;
        private NavData.NavData _navData = new NavData.NavData();

        public NavData.NavData NavData
        {
            get { return _navData; }
             set
            {
                _navData = value;
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
            const string ardroneSessionId = "d2e081a3";     // SessionID
            const string ardroneProfileId = "be27e2e4";    // Profile ID
            const string ardroneApplicationId = "d87f7e0c";      // Application ID
            
            _controlSocket = SocketFactory.GetTcpSocket(IpAddress, ControlPort);
            if (_controlSocket.Connected)
            {
                _commandWorker = new CommandWorker
                {
                    RemoteIpAddress = IpAddress,
                    SocketFactory = SocketFactory,
                    ApplocationId = ardroneApplicationId,
                    ProfileId = ardroneProfileId,
                    SessionId = ardroneSessionId
                };
                _commandWorker.Run();

                _commandWorker.SendPModeCommand(2);
                _commandWorker.SendMiscellaneousCommand(string.Format("{0},{1},{2},{3}", 2, 20, 2000, 3000));
                _commandWorker.SendConfigCommand("custom:session_id", ardroneSessionId);
                _commandWorker.SendConfigCommand("custom:profile_id", ardroneProfileId);
                _commandWorker.SendConfigCommand("custom:application_id", ardroneApplicationId);

                _commandWorker.SendConfigCommand("general:video_enable", "TRUE");
                _commandWorker.SendConfigCommand("video:bitrate_ctrl_mode", "0");
                _commandWorker.SendConfigCommand("video:video_codec", string.Format("{0}", 0x81)); // H264_360P_CODEC

                _commandWorker.SendConfigCommand("video:video_channel", "0");

                _commandWorker.SendFtrimCommand();

                _commandWorker.SendConfigCommand("general:navdata_demo", "TRUE");

                _commandWorker.SendAck();

                _navDataWorker = new NavDataWorker
                    {
                        RemoteIpAddress = IpAddress,
                        SocketFactory = SocketFactory
                    };
                _navDataWorker.NavDataReceived += NavDataWorkerOnNavDataReceived;
                _navDataWorker.NavDataUnhandledException += NavDataWorkerOnNavDataUnhandledException;
                _navDataWorker.Run();

                Task.Factory.StartNew(DoWork);
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
                        _commandWorker.SendProgressiveCommand(Pitch, Roll, Gaz, Yaw);
                    }

                    manualResetEvent.WaitOne(30);
                } while (true);
            }
        }
        
        private void NavDataWorkerOnNavDataUnhandledException(object sender, NavDataUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void NavDataWorkerOnNavDataReceived(object sender, NavDataReceivedEventArgs e)
        {
            NavData = e.NavData;
        }

        public void Dispose()
        {
        }

        public void TakeOff()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.TakeOff);
        }

        public void Land()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.LandOrReset);
        }

        public float Pitch { get; set; }

        public float Roll { get; set; }

        public float Yaw { get; set; }

        public float Gaz { get; set; }

        public void Emergency()
        {
            _commandWorker.SendRefCommand(CommandWorker.RefCommands.Emergency);
        }

        public void Blink()
        {
            _commandWorker.SendLedCommand(CommandWorker.LedAnimation.BlinkRed, 0.5f, 5);
        }

        public IDispatcher Dispatcher { get; set; }
    }
}
