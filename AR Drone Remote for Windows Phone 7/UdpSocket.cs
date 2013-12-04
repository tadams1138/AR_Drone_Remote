using AR_Drone_Controller;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnhandledExceptionEventArgs = AR_Drone_Controller.UnhandledExceptionEventArgs;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    internal class UdpSocket : IUdpSocket
    {
        private const int MaxBufferSize = 8192;

        private readonly ManualResetEvent _clientDone = new ManualResetEvent(false);
        private Socket _socket;
        private const int TimeoutMilliseconds = 500;
        private readonly byte[] _buffer = new byte[MaxBufferSize];
        private SocketAsyncEventArgs _receiveSocketEventArg;
        private SocketAsyncEventArgs _sendSocketEventArg;
        private readonly object _syncLock = new object();
        private bool _disposed;
        private readonly int _localPort;
        private readonly string _remoteIp;
        private readonly int _remotePort;

        public UdpSocket(int localPort, string remoteIp, int remotePort)
        {
            _localPort = localPort;
            _remoteIp = remoteIp;
            _remotePort = remotePort;
        }

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _receiveSocketEventArg = CreateReceiveSocketEventArg(_localPort);
            _sendSocketEventArg = CreateSendSocketAsyncEventArgs(_remoteIp, _remotePort);
            InitializeSocket();
            ListenForIncomingData();
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        public void Dispose()
        {
            lock (_syncLock)
            {
                _disposed = true;
                _socket.Dispose();
            }
        }

        public void Write(string s)
        {
            byte[] payload = Encoding.UTF8.GetBytes(s);
            Send(payload);
        }

        public void Write(int i)
        {
            byte[] payload = BitConverter.GetBytes(i);
            Send(payload);
        }

        private void InitializeSocket()
        {
            Write(1);
        }

        private SocketAsyncEventArgs CreateSendSocketAsyncEventArgs(string remoteIp, int remotePort)
        {
            var sendSocketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(remoteIp, remotePort) };
            sendSocketEventArg.Completed += (s, e) => _clientDone.Set();
            return sendSocketEventArg;
        }

        private SocketAsyncEventArgs CreateReceiveSocketEventArg(int localPort)
        {
            var receiveSocketEventArg = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, localPort)
            };
            receiveSocketEventArg.SetBuffer(_buffer, 0, _buffer.Length);
            receiveSocketEventArg.Completed += ReceiveSocketEventArgOnCompleted;
            return receiveSocketEventArg;
        }

        private void Send(byte[] payload)
        {
            lock (_syncLock)
            {
                if (_disposed)
                {
                    return;
                }

                _sendSocketEventArg.SetBuffer(payload, 0, payload.Length);
                _clientDone.Reset();
                _socket.SendToAsync(_sendSocketEventArg);
                _clientDone.WaitOne(TimeoutMilliseconds);
            }
        }

        private void ListenForIncomingData()
        {
            if (!_disposed && !_socket.ReceiveFromAsync(_receiveSocketEventArg))
            {
                ProcessSocketEvent(_receiveSocketEventArg);
            }
        }

        private void ReceiveSocketEventArgOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                lock (_syncLock)
                {
                    ProcessSocketEvent(e);
                }
            }
            catch (Exception ex)
            {
                if (UnhandledException != null)
                {
                    UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                }
            }
        }

        private void ProcessSocketEvent(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0)
            {
                var buffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesTransferred);
                DataReceived(this, new DataReceivedEventArgs(buffer));
            }
            else if (e.SocketError != SocketError.Success)
            {
                throw new SocketException((int)e.SocketError);
            }

            ListenForIncomingData();
        }
    }
}
