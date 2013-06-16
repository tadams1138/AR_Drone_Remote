namespace AR_Drone_Controller
{
    public interface IUdpSocket
    {
        void Write(string s);

        void Write(int i);

        byte[] Receive();
    }
}
