using AR_Drone_Controller;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AR_Drone_Remote_for_Windows_Phone
{
    internal class UdpSocket : IUdpSocket
    {
        private const int MaxBufferSize = 8192;

        private readonly ManualResetEvent _clientDone = new ManualResetEvent(false);
        private readonly Socket _socket;
        private readonly string _remoteIp;
        private readonly int _localPort;
        private readonly int _remotePort;
        private readonly int _timeoutMilliseconds;
        private readonly byte[] _buffer = new byte[MaxBufferSize];

        public UdpSocket(int localPort, string remoteIp, int remotePort, int timeout)
        {
            _timeoutMilliseconds = timeout;
            _localPort = localPort;
            _remoteIp = remoteIp;
            _remotePort = remotePort;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Write(string s)
        {
            byte[] payload = Encoding.UTF8.GetBytes(s);
            Send(_remoteIp, _remotePort, payload);
        }

        public void Write(int i)
        {
            byte[] payload = BitConverter.GetBytes(i);
            Send(_remoteIp, _remotePort, payload);
        }

        public void Send(string serverName, int portNumber, byte[] payload)
        {
            var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(serverName, portNumber) };
            socketEventArg.Completed += (s, e) => _clientDone.Set();
            socketEventArg.SetBuffer(payload, 0, payload.Length);
            _clientDone.Reset();
            _socket.SendToAsync(socketEventArg);
            _clientDone.WaitOne(_timeoutMilliseconds);
        }

        public byte[] Receive()
        {
            byte[] response = null;

            var socketEventArg = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, _localPort)
                };

            socketEventArg.SetBuffer(_buffer, 0, MaxBufferSize);
            socketEventArg.Completed += delegate(object s, SocketAsyncEventArgs e)
                {
                    if (e.BytesTransferred > 0)
                    {
                        response = new byte[e.BytesTransferred];
                        Buffer.BlockCopy(e.Buffer, 0, response, 0, e.BytesTransferred);
                    }

                    _clientDone.Set();
                };
            
            _clientDone.Reset();
            _socket.ReceiveFromAsync(socketEventArg);
            _clientDone.WaitOne(_timeoutMilliseconds);

            if (response == null)
            {
                throw new UdpSocketReceiveTimeoutException(_localPort, _timeoutMilliseconds);
            }

            return response;
        }

        class UdpSocketReceiveTimeoutException : Exception
        {
            private const string MessageFormat = "Time exceeded {0} milliseconds waiting to receive on port {1}.";

            public UdpSocketReceiveTimeoutException(int portNumber, int timeoutMilliseconds)
                : base(string.Format(MessageFormat, portNumber, timeoutMilliseconds))
            {
            }
        }
    }
}
