namespace AR_Drone_Controller.NavData
{
    using System;
    using System.Threading.Tasks;

    class NavDataWorker
    {
        private const int NavDataPort = 5554;
        private const int TimeoutValue = 500;

        private IUdpSocket _navDataSocket;
        private bool _run;

        public string RemoteIpAddress { get; set; }
        public ISocketFactory SocketFactory { get; set; }

        public delegate void NavDataReceivedEventHandler(object sender, NavDataReceivedEventArgs e);
        public event NavDataReceivedEventHandler NavDataReceived;

        public delegate void NavDataUnhandledExceptionEventHandler(object sender, NavDataUnhandledExceptionEventArgs e);
        public event NavDataUnhandledExceptionEventHandler NavDataUnhandledException;


        internal void Run()
        {
            if (!_run)
            {
                _run = true;
                Task.Factory.StartNew(DoWork);
            }
        }

        internal void Stop()
        {
            _run = false;
        }

        private void DoWork()
        {
            NavData.ResetSequence();
            CreateSocket(); 
            InitiateCommunication();

            do
            {
                byte[] bytesReceived = null;
                try
                {
                    bytesReceived = _navDataSocket.Receive();
                }
                catch
                {
                    InitiateCommunication();
                }

                if (bytesReceived != null && NavDataReceived != null)
                {
                    try
                    {
                        var navData = NavData.FromBytes(bytesReceived);
                        var navDataReceivedEventArgs = new NavDataReceivedEventArgs(navData);
                        NavDataReceived(this, navDataReceivedEventArgs);
                    }
                    catch (NavData.OutOfSequenceException)
                    {
                        // ignore, we'll get the next one
                    }
                    catch (Exception ex)
                    {
                        var navDataUnhandledExceptionEventArgs = new NavDataUnhandledExceptionEventArgs(ex);
                        NavDataUnhandledException(this, navDataUnhandledExceptionEventArgs);
                    }
                }
            } while (_run);
        }

        private void CreateSocket()
        {
            _navDataSocket = SocketFactory.GetUdpSocket(NavDataPort, RemoteIpAddress, NavDataPort,
                                                        TimeoutValue);
        }

        private void InitiateCommunication()
        {
            _navDataSocket.Write(1);
        }
    }
}