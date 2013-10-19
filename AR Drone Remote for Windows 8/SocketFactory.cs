using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_8
{
    class SocketFactory : ISocketFactory
    {
        ITcpSocket ISocketFactory.GetTcpSocket(GetTcpSocketParams getTcpSocketParams)
        {
            return new TcpSocket(getTcpSocketParams.IpAddress, getTcpSocketParams.Port);
        }

        public IUdpSocket GetUdpSocket(GetUdpSocketParams getUdpSocketParams)
        {
            return new UdpSocket(getUdpSocketParams.RemoteIp, getUdpSocketParams.RemotePort, getUdpSocketParams.Timeout);
        }
    }
}
