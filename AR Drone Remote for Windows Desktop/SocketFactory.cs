namespace AR_Drone_Remote_for_Windows_Desktop
{
    using AR_Drone_Controller;

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
}
