namespace AR_Drone_Remote_for_Windows_Phone_7
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using AR_Drone_Controller;
    using UnhandledExceptionEventArgs = AR_Drone_Controller.UnhandledExceptionEventArgs;

    internal class TcpSocket : ITcpSocket
    {
        private const int TimeoutMilliseconds = 5000;
        private const int BufferSize = 2 << 16;

        private Socket _socket;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private readonly byte[] _receiveBuffer = new byte[BufferSize];
        private SocketAsyncEventArgs _responseListener;
        private bool _connected;
        private readonly string _ipAddress;
        private readonly int _port;

        public TcpSocket(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketEventArg = new SocketAsyncEventArgs {RemoteEndPoint = new DnsEndPoint(_ipAddress, _port)};
            socketEventArg.Completed += (s, e) =>
                {
                    _connected = true;
                    _manualResetEvent.Set();
                };
            _manualResetEvent.Reset();
            _socket.ConnectAsync(socketEventArg);
            _manualResetEvent.WaitOne(TimeoutMilliseconds);
            _responseListener = CreateResponseListenerSocketAsyncEventArgs();

            if (_connected)
            {
                ListenForIncomingData();
            }
            else
            {
                throw new TcpSocketConnectTimeoutException(_ipAddress, _port, TimeoutMilliseconds);
            }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
        public event EventHandler Disconnected;

        public void Dispose()
        {
            _connected = false;
            _socket.Dispose();
        }

        public void Write(int s)
        {
            // throw new NotImplementedException();
        }

        private SocketAsyncEventArgs CreateResponseListenerSocketAsyncEventArgs()
        {
            var responseListener = new SocketAsyncEventArgs();
            responseListener.Completed += (sender, args) => ProcessSocketEvent(args);
            return responseListener;
        }

        private void ListenForIncomingData()
        {
            _responseListener.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
            if (_connected && !_socket.ReceiveAsync(_responseListener))
            {
                ProcessSocketEvent(_responseListener);
            }
        }

        private void ProcessSocketEvent(SocketAsyncEventArgs e)
        {
            try
            {
                if (!_connected)
                {
                    return;
                }

                if (e.BytesTransferred > 0)
                {
                    var buffer = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesTransferred);
                    DataReceived(this, new DataReceivedEventArgs(buffer));
                }
                
                if (e.SocketError == SocketError.Success)
                {
                    ListenForIncomingData();
                }
                else
                {
                    throw new SocketException((int)e.SocketError);
                }
            }
            catch (Exception ex)
            {
                if (UnhandledException != null)
                {
                    UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                }

                Dispose();
                if (Disconnected != null)
                {
                    Disconnected(this, null);
                }
            }
        }

        internal class TcpSocketConnectTimeoutException : Exception
        {
            private const string MessageFormat = "Time exceeded {2} milliseconds waiting to connect to {0}:{1}.";

            public TcpSocketConnectTimeoutException(string ipAddress, int port, int timeoutMilliseconds)
                : base(string.Format(MessageFormat, ipAddress, port, timeoutMilliseconds))
            {
            }
        }
    }
}
