using System;

namespace AR_Drone_Controller
{
    public interface ITcpSocket : IDisposable
    {
        void Connect();
        
        void Write(int i);

        event EventHandler<DataReceivedEventArgs> DataReceived;

        event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        event EventHandler Disconnected;
    }
}
