using System;

namespace AR_Drone_Controller
{
    public interface ITcpSocket : IDisposable
    {
        bool Connected { get; }

        void Write(string s);

        string Read();
    }
}
