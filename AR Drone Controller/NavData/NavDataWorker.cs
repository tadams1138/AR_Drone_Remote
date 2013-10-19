namespace AR_Drone_Controller.NavData
{
    using System;

    using System.Threading;

    class NavDataWorker : IDisposable
    {
        private const int NavDataPort = 5554;
        private const int TimeoutValue = 500;

        private IUdpSocket _navDataSocket;
        private bool _run;
        private Timer _keepAliveTimer;
        private DateTime _timeSinceLastReception;

        public string RemoteIpAddress { get; set; }
        public ISocketFactory SocketFactory { get; set; }

        public event EventHandler<NavDataReceivedEventArgs> NavDataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
        
        internal void Run()
        {
            if (!_run)
            {
                NavData.ResetSequence();
                CreateSocket();
                _keepAliveTimer = new Timer(KeepAlive, null, 0, TimeoutValue);
                _run = true;
            }
        }

        internal void Stop()
        {
            if (_keepAliveTimer != null)
            {
                _keepAliveTimer.Dispose();
                _keepAliveTimer = null;
            }
            
            if (_navDataSocket != null)
            {
                _navDataSocket.Dispose();
                _navDataSocket = null;
            }

            _run = false;
        }

        private void KeepAlive(object state)
        {
            if (TimeoutExceeded())
            {
                try
                {
                    InitiateCommunication();
                }
                catch (Exception ex)
                {
                    if (UnhandledException != null)
                    {
                        UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                    }
                }
            }
        }

        private void NavDataSocketOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            ResetTimeout();

            if (e.Data != null && NavDataReceived != null)
            {
                try
                {
                    var navData = NavData.FromBytes(e.Data);
                    var navDataReceivedEventArgs = new NavDataReceivedEventArgs(navData);
                    NavDataReceived(this, navDataReceivedEventArgs);
                }
                catch (NavData.OutOfSequenceException)
                {
                    // ignore, we'll get the next one
                }
                catch (Exception ex)
                {
                    if (UnhandledException != null)
                    {
                        UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                    }
                }
            }
        }

        private void NavDataSocketOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (UnhandledException != null)
            {
                UnhandledException(this, e);
            }
        }

        private bool TimeoutExceeded()
        {
            return (DateTime.UtcNow - _timeSinceLastReception).TotalMilliseconds > TimeoutValue;
        }

        private void ResetTimeout()
        {
            _timeSinceLastReception = DateTime.UtcNow;
        }

        private void CreateSocket()
        {
            var getUdpSocketParams = new GetUdpSocketParams
                {
                    LocalPort = NavDataPort,
                    RemoteIp = RemoteIpAddress,
                    RemotePort = NavDataPort,
                    Timeout = TimeoutValue
                };
            _navDataSocket = SocketFactory.GetUdpSocket(getUdpSocketParams);
            _navDataSocket.DataReceived += NavDataSocketOnDataReceived;
            _navDataSocket.UnhandledException += NavDataSocketOnUnhandledException;
            _navDataSocket.Connect();
        }

        private void InitiateCommunication()
        {
            _navDataSocket.Write(1);
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}