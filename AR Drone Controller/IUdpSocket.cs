using System;

namespace AR_Drone_Controller
{
    public interface IUdpSocket : IDisposable
    {
        void Write(string s);

        void Write(int i);

        event EventHandler<DataReceivedEventArgs> DataReceived;

        event EventHandler<UnhandledExceptionEventArgs> UnhandledException;
    }
}
