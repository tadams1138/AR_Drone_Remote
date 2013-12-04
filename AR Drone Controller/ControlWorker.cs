using System;

namespace AR_Drone_Controller
{
    internal class ControlWorker : IDisposable
    {
        public virtual event EventHandler Disconnected;

        internal ITcpSocket Socket { get; set; }

        public virtual void Dispose()
        {
            Socket.Dispose();
        }

        public virtual void Run()
        {
            Socket.Disconnected += (sender, eventArgs) => Disconnected(this, eventArgs);
            Socket.Connect();
        }
    }
}
