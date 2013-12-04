using AR_Drone_Controller.NavData;

namespace AR_Drone_Controller
{
    internal class WorkerFactory
    {
        public ConnectParams ConnectParams { get; set; }

        public ISocketFactory SocketFactory { get; set; }


        public virtual CommandWorker CreateCommandWorker()
        {
            var socket = SocketFactory.GetUdpSocket(ConnectParams.NetworkAddress, ConnectParams.CommandPort);

            var worker = new CommandWorker
            {
                Socket = socket,
                CommandFormatter = new CommandFormatter(),
                CommandQueue = new CommandQueue(),
                FloatToInt32Converter = new FloatToInt32Converter(),
                ProgressiveCommandFormatter = new ProgressiveCommandFormatter()
            };

            return worker;
        }

        public virtual VideoWorker CreateVideoWorker()
        {
            var socket = SocketFactory.GetTcpSocket(ConnectParams.NetworkAddress, ConnectParams.VideoPort);
            var worker = new VideoWorker { Socket = socket };
            return worker;
        }

        public virtual NavDataWorker CreateNavDataWorker()
        {
            var socket = SocketFactory.GetUdpSocket(ConnectParams.NetworkAddress, ConnectParams.NavDataPort);
            var timerFactory = new TimerFactory();
            var worker = new NavDataWorker
            {
                Socket = socket,
                NavDataFactory = new NavDataFactory(),
                TimerFactory = timerFactory
            };
            timerFactory.TimerCallback = worker.CheckTimeout;
            timerFactory.Period = NavDataWorker.Timeout;
            return worker;
        }

        public virtual ControlWorker CreateControlWorker()
        {
            var socket = SocketFactory.GetTcpSocket(ConnectParams.NetworkAddress, ConnectParams.ControlPort);
            var worker = new ControlWorker { Socket = socket };
            return worker;
        }
    }
}