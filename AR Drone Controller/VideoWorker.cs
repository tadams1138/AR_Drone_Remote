namespace AR_Drone_Controller
{
    using System;
    using System.Threading;


    class VideoWorker : IDisposable
    {
        private const int VideoPort = 5555;
        private const int TimeoutValue = 500;

        private ITcpSocket _videoSocket;
        private bool _run;
        private Timer _keepAliveTimer;
        private DateTime _timeSinceLastReception;

        public string RemoteIpAddress { get; set; }
        public ISocketFactory SocketFactory { get; set; }

        //public event EventHandler<VideoReceivedEventArgs> VideoReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        internal void Run()
        {
            if (!_run)
            {
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

            if (_videoSocket != null)
            {
                _videoSocket.Dispose();
                _videoSocket = null;
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

        private void VideoSocketOnDataReceived(object sender, DataReceivedEventArgs e)
        {
            ResetTimeout();

            // TODO: turn packet into image
        }

        private void VideoSocketOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
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
            var getTcpSocketParams = new GetTcpSocketParams {IpAddress = RemoteIpAddress, Port = VideoPort};
            _videoSocket = SocketFactory.GetTcpSocket(getTcpSocketParams);
            _videoSocket.DataReceived += VideoSocketOnDataReceived;
            _videoSocket.UnhandledException += VideoSocketOnUnhandledException;
            _videoSocket.Connect();
        }

        private void InitiateCommunication()
        {
            _videoSocket.Write(1);
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}