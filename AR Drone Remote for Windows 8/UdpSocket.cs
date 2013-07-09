using AR_Drone_Controller;
using System;
using System.Text;
using System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AR_Drone_Remote_for_Windows_8
{
    internal class UdpSocket : IUdpSocket
    {
        private readonly ManualResetEvent _clientDone = new ManualResetEvent(false);
        private readonly DatagramSocket _socket;
        private readonly DataWriter _writer;
        private readonly int _remotePort;
        private readonly int _timeoutMilliseconds;
        private byte[] _buffer;
        private readonly object _syncLock = new object();

        public UdpSocket(int localPort, string remoteIp, int remotePort, int timeout)
        {
            _timeoutMilliseconds = timeout;
            _remotePort = remotePort;
            _socket = new DatagramSocket();
            _socket.MessageReceived += socket_MessageReceived;
            var result = _socket.ConnectAsync(new HostName(remoteIp), remotePort.ToString());
            result.AsTask().Wait(timeout);
            if (result.ErrorCode != null) 
            {
                throw result.ErrorCode;
            }
            _writer = new DataWriter(_socket.OutputStream);
        }

        public void Write(string s)
        {
            byte[] payload = Encoding.UTF8.GetBytes(s);
            Write(payload);
        }

        public void Write(int i)
        {
            byte[] payload = BitConverter.GetBytes(i);
            Write(payload);
        }

        private void Write(byte[] b)
        {
            _writer.WriteBytes(b);
            _writer.StoreAsync();
        }

        private void socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            byte[] buffer;

            using (var dataReader = args.GetDataReader())
            {
                var bufferLength = dataReader.UnconsumedBufferLength;
                buffer = new byte[bufferLength];
                dataReader.ReadBytes(buffer);
            }

            lock (_syncLock)
            {
                _buffer = buffer;
            }

            _clientDone.Set();
        }

        public byte[] Receive()
        {
            bool wait;
            byte[] result;
            lock (_syncLock)
            {
                wait = _buffer == null;
            }

            if (wait)
            {
                _clientDone.Reset();
                _clientDone.WaitOne(_timeoutMilliseconds);
            }

            lock (_syncLock)
            {
                result = _buffer;
                _buffer = null;
            }

            if (result == null)
            {
                throw new UdpSocketReceiveTimeoutException(_remotePort, _timeoutMilliseconds);
            }

            return result;
        }

        class UdpSocketReceiveTimeoutException : Exception
        {
            private const string MessageFormat = "Time exceeded {0} milliseconds waiting to receive on port {1}.";

            public UdpSocketReceiveTimeoutException(int portNumber, int timeoutMilliseconds)
                : base(string.Format(MessageFormat, timeoutMilliseconds, portNumber))
            {
            }
        }
    }
}
