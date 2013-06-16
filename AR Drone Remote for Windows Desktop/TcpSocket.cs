using System.Text;
using System.Threading;

namespace AR_Drone_Remote_for_Windows_Desktop
{
    using AR_Drone_Controller;

    class TcpSocket : ITcpSocket
    {
        private const int TimeOutMs = 100;
        private readonly System.Net.Sockets.TcpClient _tcpClient;

        public TcpSocket(string ipAddress, int port)
        {
            _tcpClient = new System.Net.Sockets.TcpClient(ipAddress, port);
        }

        public bool Connected
        {
            get { return _tcpClient.Connected; }
        }

        public void Write(string s)
        {
            byte[] buf = Encoding.ASCII.GetBytes(s);
            _tcpClient.GetStream().Write(buf, 0, buf.Length);
            Thread.Sleep(100);
        }

        public void Dispose()
        {
            _tcpClient.Close();
        }
        
        public string Read()
        {
            if (!_tcpClient.Connected) return null;
            var sb = new StringBuilder();
            do
            {
                ParseTelnet(sb);
                System.Threading.Thread.Sleep(TimeOutMs);
            } while (_tcpClient.Available > 0);
            return sb.ToString();
        }

        void ParseTelnet(StringBuilder sb)
        {
            while (_tcpClient.Available > 0)
            {
                int input = _tcpClient.GetStream().ReadByte();
                switch (input)
                {
                    case -1:
                    case 255:
                        break;
                    default:
                        sb.Append((char)input);
                        break;
                }
            }
        }
    }
}
