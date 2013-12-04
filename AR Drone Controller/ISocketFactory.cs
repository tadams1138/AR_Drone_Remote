namespace AR_Drone_Controller
{
    public interface ISocketFactory
    {
        ITcpSocket GetTcpSocket(string address, int port);

        IUdpSocket GetUdpSocket(string address, int port);
    }
}
