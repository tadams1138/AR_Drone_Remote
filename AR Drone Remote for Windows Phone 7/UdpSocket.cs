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
        private int _exceptionsSending;

        public UdpSocket(int localPort, string remoteIp, int remotePort)
        {
            _localPort = localPort;
            _remoteIp = remoteIp;
            _remotePort = remotePort;
        }

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            CreateReceiveSocketEventArg();
            CreateSendSocketAsyncEventArgs();
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
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        public void Write(string s)
        {
            byte[] payload = Encoding.UTF8.GetBytes(s);
            SafeSend(payload);
        }

        public void Write(int i)
        {
            byte[] payload = BitConverter.GetBytes(i);
            SafeSend(payload);
        }

        private void InitializeSocket()
        {
            Write(1);
        }

        private void CreateSendSocketAsyncEventArgs()
        {
            _sendSocketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(_remoteIp, _remotePort) };
            _sendSocketEventArg.Completed += (s, e) => _clientDone.Set();
        }

        private void CreateReceiveSocketEventArg()
        {
            _receiveSocketEventArg = new SocketAsyncEventArgs
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Any, _localPort)
            };
            _receiveSocketEventArg.SetBuffer(_buffer, 0, _buffer.Length);
            _receiveSocketEventArg.Completed += ReceiveSocketEventArgOnCompleted;
        }

        private void SafeSend(byte[] payload)
        {
            lock (_syncLock)
            {
                if (!_disposed)
                {
                    try
                    {
                        Send(payload);
                        _exceptionsSending = 0;
                    }
                    catch (InvalidOperationException)
                    {
                        if (_exceptionsSending > 2)
                        {
                            throw;
                        }

                        _exceptionsSending++;
                        CreateNewSocketAsyncEventArgsAndResend(payload);
                    }
                }
            }
        }

        private void CreateNewSocketAsyncEventArgsAndResend(byte[] payload)
        {
            DisposeSendSocketEventArg();
            CreateSendSocketAsyncEventArgs();
            Send(payload);
        }

        private void Send(byte[] payload)
        {
            _sendSocketEventArg.SetBuffer(payload, 0, payload.Length);
            _clientDone.Reset();
            _socket.SendToAsync(_sendSocketEventArg);
            _clientDone.WaitOne(TimeoutMilliseconds);
        }

        private void DisposeSendSocketEventArg()
        {
            _sendSocketEventArg.Dispose();
            _sendSocketEventArg = null;
        }

        private void ListenForIncomingData()
        {
            bool shouldProcessSocketEvent;

            lock (_syncLock)
            {
                shouldProcessSocketEvent = !_disposed && !_socket.ReceiveFromAsync(_receiveSocketEventArg);
            }

            if (shouldProcessSocketEvent)
            {
                ProcessSocketEvent(_receiveSocketEventArg);
            }
        }

        private void ReceiveSocketEventArgOnCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                ProcessSocketEvent(e);
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
            byte[] buffer = null;

            lock (_syncLock)
            {
                if (e.BytesTransferred > 0)
                {
                    buffer = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, buffer, 0, e.BytesTransferred);
                }
                else if (e.SocketError != SocketError.Success)
                {
                    throw new SocketException((int) e.SocketError);
                }
            }

            if (buffer != null)
            {
                DataReceived(this, new DataReceivedEventArgs(buffer));
            }

            ListenForIncomingData();
        }
    }
}
