namespace AR_Drone_Controller
{
    public interface ISocketFactory
    {
        ITcpSocket GetTcpSocket(string ipAddress, int port);

        IUdpSocket GetUdpSocket(int localPort, string remoteIp, int remotePort, int timeout);
    }
}
