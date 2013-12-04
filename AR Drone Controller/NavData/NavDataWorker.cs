namespace AR_Drone_Controller.NavData
{
    using System;

    internal class NavDataWorker : IDisposable
    {
        public const int Timeout = 100;

        private readonly object _timeoutSynclock = new object();

        private IDisposable _timeoutTimer;
        private bool _dataReceived;

        internal NavDataFactory NavDataFactory { get; set; }

        internal TimerFactory TimerFactory { get; set; }

        internal IUdpSocket Socket { get; set; }

        public virtual event EventHandler<NavDataReceivedEventArgs> NavDataReceived;
        
       public virtual void Run()
        {
            Socket.DataReceived += SocketOnDataReceived;
            Socket.Connect();
            InitiateCommunication();
            _timeoutTimer = TimerFactory.CreateTimer();
        }

        public virtual void Dispose()
        {
            Socket.Dispose();

            if (_timeoutTimer != null)
            {
                _timeoutTimer.Dispose();
            }
        }

        private void InitiateCommunication()
        {
            Socket.Write(1);
        }

        private void SocketOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            lock (_timeoutSynclock)
            {
                _dataReceived = true;
            }

            if (e.Data != null && NavDataReceived != null)
            {
                try
                {
                    var navData = NavDataFactory.Create(e.Data);
                    var navDataReceivedEventArgs = new NavDataReceivedEventArgs(navData);
                    NavDataReceived(this, navDataReceivedEventArgs);
                }
                catch
                {
                    // ignore... seriously
                }
            }
        }

        internal void CheckTimeout(object state)
        {
            bool needToInitiateCommunication;

            lock (_timeoutSynclock)
            {
                needToInitiateCommunication = !_dataReceived;
            }

            if (needToInitiateCommunication)
            {
                InitiateCommunication();
            }

            lock (_timeoutSynclock)
            {
                _dataReceived = false;
            }
        }
    }
}