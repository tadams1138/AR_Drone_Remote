using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    class SocketFactory : ISocketFactory
    {
        ITcpSocket ISocketFactory.GetTcpSocket(string ipAddress, int port)
        {
            return new TcpSocket(ipAddress, port);
        }

        public IUdpSocket GetUdpSocket(int localPort, string remoteIp, int remotePort, int timeout)
        {
            return new UdpSocket(localPort, remoteIp, remotePort, timeout);
        }
    }
}
