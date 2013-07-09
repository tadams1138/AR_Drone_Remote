using System;
using System.Threading;
using AR_Drone_Controller;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AR_Drone_Remote_for_Windows_8
{
    internal class TcpSocket : ITcpSocket
    {
        private readonly StreamSocket _socket;
        private readonly DataWriter _writer;
        private readonly ManualResetEvent _clientDone = new ManualResetEvent(false);
        private readonly int _timeoutMilliseconds = 500;

        // TODO: TCP sockets for Windows phone
        public TcpSocket(string ipAddress, int port)
        {
            _socket = new StreamSocket();
            var result = _socket.ConnectAsync(new HostName(ipAddress), port.ToString());
            result.AsTask().Wait(_timeoutMilliseconds);
            if (result.ErrorCode != null)
            {
                throw result.ErrorCode;
            }

            Connected = true;
            _writer = new DataWriter(_socket.OutputStream);
        }

        public void Dispose()
        {
            Connected = false;
            _socket.Dispose();
        }

        public bool Connected { get; private set; }

        public void Write(string s)
        {
            // throw new NotImplementedException();
        }

        public string Read()
        {
            return null;
            // throw new NotImplementedException();
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
