using System;

namespace AR_Drone_Controller
{
    public interface ITcpSocket : IDisposable
    {
        bool Connected { get; }

        void Write(int i);

        event EventHandler<DataReceivedEventArgs> DataReceived;

        event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        event EventHandler Disconnected;
    }
}
