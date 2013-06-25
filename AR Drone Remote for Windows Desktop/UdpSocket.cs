using AR_Drone_Controller;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AR_Drone_Remote_for_Windows_Desktop
{
    class UdpSocket : IUdpSocket
    {
        private readonly UdpClient _socket;
        private IPEndPoint _remoteEndpoint;

        public UdpSocket(int localPort, string remoteIp, int remotePort, int timeout)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, localPort);
            _socket = new UdpClient(endpoint) { Client = { ReceiveTimeout = timeout, SendTimeout = timeout } };
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse(remoteIp), remotePort);
        }

        public void Write(string s)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(s);
            _socket.Send(buffer, buffer.Length, _remoteEndpoint);
        }
        
        public void Write(int i)
        {
            byte[] buffer = BitConverter.GetBytes(i);
            _socket.Send(buffer, buffer.Length, _remoteEndpoint);
            Thread.Sleep(100);
        }
        
        public byte[] Receive()
        {
            return _socket.Receive(ref _remoteEndpoint);
        }
    }
}
