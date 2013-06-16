namespace AR_Drone_Controller
{
    public interface ISocketFactory
    {
        ITcpSocket GetTcpSocket(string ipAddress, int port);

        IUdpSocket GetUdpSocket(string localIp, int localPort, string remoteIp, int remotePort, int timeout);
    }
}
