namespace AR_Drone_Remote_for_Windows_Desktop
{
    using AR_Drone_Controller;

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
