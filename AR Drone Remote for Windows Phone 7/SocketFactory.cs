using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    class SocketFactory : ISocketFactory
    {
        public ITcpSocket GetTcpSocket(GetTcpSocketParams getTcpSocketParams)
        {
            return new TcpSocket(getTcpSocketParams.IpAddress, getTcpSocketParams.Port);
        }

        public IUdpSocket GetUdpSocket(GetUdpSocketParams getUdpSocketParams)
        {
            return new UdpSocket(getUdpSocketParams.LocalPort, getUdpSocketParams.RemoteIp, getUdpSocketParams.RemotePort, getUdpSocketParams.Timeout);
        }
    }
}
