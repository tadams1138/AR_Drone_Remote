namespace AR_Drone_Controller
{
    using System;

    class VideoWorker : IDisposable
    {
        internal ITcpSocket Socket { get; set; }
        
        internal virtual void Run()
        {
            // TODO: debug these events to determine if we need to react to them
            Socket.Disconnected += SocketOnDisconnected;
            Socket.DataReceived += SocketOnDataReceived;
            Socket.Connect();
        }

        private void SocketOnDisconnected(object sender, EventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void SocketOnDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            Socket.Dispose();
        }
    }
}