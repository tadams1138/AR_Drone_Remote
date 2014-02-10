namespace AR_Drone_Remote_for_Windows_8
{
    using AR_Drone_Controller;
    using System;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    internal class TcpSocket : ITcpSocket
    {
        private const int TimeoutMilliseconds = 500;
        private const int BufferSize = 2 << 16;

        private readonly byte[] _receiveBuffer = new byte[BufferSize];
        private StreamSocket _socket;
        private DataWriter _writer;
        private DataReader _reader;
        private bool _connected;
        readonly string _ipAddress;
        readonly int _port;

        public TcpSocket(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public void Connect()
        {
            _socket = new StreamSocket();
            var result = _socket.ConnectAsync(new HostName(_ipAddress), _port.ToString());
            result.AsTask().Wait(TimeoutMilliseconds);
            if (result.ErrorCode != null)
            {
                throw result.ErrorCode;
            }

            _writer = new DataWriter(_socket.OutputStream);
            _reader = new DataReader(_socket.InputStream);

            ListenForIncomingData();
            _connected = true;
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
        public event EventHandler Disconnected;

        public void Dispose()
        {
            _connected = false;
            _socket.Dispose();
            _writer.Dispose();
            _reader.Dispose();
        }

        public async void Write(int s)
        {
            _writer.WriteInt32(s);
            await _writer.FlushAsync();
        }

        private static bool IsDisconnected(uint data)
        {
            return data == 0;
        }

        private async void ListenForIncomingData()
        {
            try
            {
                var byteCount = await _reader.LoadAsync(BufferSize);

                if (!IsDisconnected(byteCount))
                {
                    _reader.ReadBytes(_receiveBuffer);
                    DataReceived(this, new DataReceivedEventArgs(_receiveBuffer));

                    if (_connected)
                    {
                        ListenForIncomingData();
                    }
                }
                else if (_connected)
                {
                    throw new UnexpectedDisconnectException();
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

        internal class UnexpectedDisconnectException : Exception
        {
            public UnexpectedDisconnectException()
                : base("The socket closed unexpectedly.")
            {
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