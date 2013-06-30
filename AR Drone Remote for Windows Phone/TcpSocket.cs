using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone
{
    internal class TcpSocket : ITcpSocket
    {
        private readonly Socket _socket;
        private readonly ManualResetEvent _clientDone = new ManualResetEvent(false);
        private readonly int _timeoutMilliseconds = 1000;

        // TODO: TCP sockets for Windows phone
        public TcpSocket(string ipAddress, int port)
        {
            bool connected = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(ipAddress, port) };
            socketEventArg.Completed += (s, e) =>
                {
                    connected = true;
                    _clientDone.Set();
                };
            _clientDone.Reset();
            _socket.ConnectAsync(socketEventArg);
            _clientDone.WaitOne(_timeoutMilliseconds);

            if (!connected)
            {
                throw new TcpSocketConnectTimeoutException(ipAddress, port, _timeoutMilliseconds);
            }
        }

        public void Dispose()
        {
            _socket.Dispose();
        }

        public bool Connected { get { return true; } private set { } }

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
