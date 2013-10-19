namespace AR_Drone_Controller
{
    public interface ISocketFactory
    {
        ITcpSocket GetTcpSocket(GetTcpSocketParams getTcpSocketParams);

        IUdpSocket GetUdpSocket(GetUdpSocketParams getUdpSocketParams);
    }
}
