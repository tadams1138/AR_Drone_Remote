using AR_Drone_Controller;

namespace AR_Drone_Remote_for_Windows_Phone
{
    internal class TcpSocket : ITcpSocket
    {
        // TODO: TCP sockets for Windows phone
        public TcpSocket(string ipAddress, int port)
        {
            // throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public bool Connected { get { return true; } private set { } }

        public void Write(string s)
        {
            // throw new NotImplementedException();
        }

        public string Read()
        {
            return null;
            // throw new NotImplementedException();
        }
    }
}
