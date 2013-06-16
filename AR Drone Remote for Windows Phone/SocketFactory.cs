using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone
{
    class SocketFactory : ISocketFactory
    {
        ITcpSocket ISocketFactory.GetTcpSocket(string ipAddress, int port)
        {
            return new TcpSocket(ipAddress, port);
        }

        public IUdpSocket GetUdpSocket(string localIp, int localPort, string remoteIp, int remotePort, int timeout)
        {
            return new UdpSocket(localIp, localPort, remoteIp, remotePort, timeout);
        }
    }

    internal class UdpSocket : IUdpSocket
    {
        static ManualResetEvent _clientDone = new ManualResetEvent(false);

        private readonly System.Net.Sockets.Socket _socket;
        private string _remoteIp;
        private int _remotePort;
        private int TIMEOUT_MILLISECONDS;


        public UdpSocket(string localIp, int localPort, string remoteIp, int remotePort, int timeout)
        {
            TIMEOUT_MILLISECONDS = timeout * 10;
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
            // Create SocketAsyncEventArgs context object
            var socketEventArg = new SocketAsyncEventArgs { RemoteEndPoint = new DnsEndPoint(serverName, portNumber) };
            // Set properties on context object
            // Inline event handler for the Completed event.
            // Note: This event handler was implemented inline in order to make this method self-contained.
            socketEventArg.Completed += (s, e) => _clientDone.Set();
            // Add the data to be sent into the buffer

            socketEventArg.SetBuffer(payload, 0, payload.Length);
            // Sets the state of the event to nonsignaled, causing threads to block
            _clientDone.Reset();
            // Make an asynchronous Send request over the socket
            _socket.SendToAsync(socketEventArg);
            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
        }


        public byte[] Receive()
        {
            return Receive(_remotePort);
        }

        const int MAX_BUFFER_SIZE = 2048 * 1024;


        public byte[] Receive(int portNumber)
        {
            byte[] response = null;

            // Create SocketAsyncEventArgs context object
            var socketEventArg = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Any, portNumber)
                };
            // Setup the buffer to receive the data
            socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);
            // Inline event handler for the Completed event.
            // Note: This even handler was implemented inline in order to make this method self-contained.
            socketEventArg.Completed += delegate(object s, SocketAsyncEventArgs e)
                {

                    response = e.Buffer;

                    _clientDone.Set();
                };
            // Sets the state of the event to nonsignaled, causing threads to block
            _clientDone.Reset();
            // Make an asynchronous Receive request over the socket
            _socket.ReceiveFromAsync(socketEventArg);
            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            return response;
        }

    }

    internal class TcpSocket : ITcpSocket
    {
        public TcpSocket(string ipAddress, int port)
        {
            // throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
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
}
