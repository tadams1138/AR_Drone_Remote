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
        private readonly StreamSocket _socket;
        private readonly DataWriter _writer;
        private readonly DataReader _reader;

        public TcpSocket(string ipAddress, int port)
        {
            _socket = new StreamSocket();
            var result = _socket.ConnectAsync(new HostName(ipAddress), port.ToString());
            result.AsTask().Wait(TimeoutMilliseconds);
            if (result.ErrorCode != null)
            {
                throw result.ErrorCode;
            }

            _writer = new DataWriter(_socket.OutputStream);
            _reader = new DataReader(_socket.InputStream);

            ListenForIncomingData();
            Connected = true;
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
        public event EventHandler Disconnected;

        public bool Connected { get; private set; }

        public void Dispose()
        {
            Connected = false;
            _socket.Dispose();
            _writer.Dispose();
            _reader.Dispose();
        }

        public void Write(int s)
        {
            _writer.WriteInt32(s);
            _writer.FlushAsync();
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

                    if (Connected)
                    {
                        ListenForIncomingData();
                    }
                }
                else if (Connected)
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
            public UnexpectedDisconnectException() : base("The socket closed unexpectedly.")
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