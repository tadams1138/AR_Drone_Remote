using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_8
{
    class SocketFactory : ISocketFactory
    {
        ITcpSocket ISocketFactory.GetTcpSocket(string address, int port)
        {
            return new TcpSocket(address, port);
        }

        public IUdpSocket GetUdpSocket(string address, int port)
        {
            return new UdpSocket(address, port);
        }
    }
}
