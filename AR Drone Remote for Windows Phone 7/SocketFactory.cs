using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    class SocketFactory : ISocketFactory
    {
        public ITcpSocket GetTcpSocket(string address, int port)
        {
            return new TcpSocket(address, port);
        }

        public IUdpSocket GetUdpSocket(string address, int port)
        {
            return new UdpSocket(port, address, port);
        }
    }
}
