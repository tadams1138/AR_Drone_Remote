using AR_Drone_Controller;
using System;
using System.Text;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AR_Drone_Remote_for_Windows_8
{
    internal class UdpSocket : IUdpSocket
    {
        private readonly DatagramSocket _socket;
        private readonly DataWriter _writer;

        public UdpSocket(string remoteIp, int remotePort, int timeout)
        {
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

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        public void Dispose()
        {
            _writer.Dispose();
            _socket.Dispose();
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
            if (DataReceived != null)
            {
                using (var dataReader = args.GetDataReader())
                {
                    var bufferLength = dataReader.UnconsumedBufferLength;
                    var buffer = new byte[bufferLength];
                    dataReader.ReadBytes(buffer);
                    DataReceived(this, new DataReceivedEventArgs(buffer));
                }
            }
        }
    }
}