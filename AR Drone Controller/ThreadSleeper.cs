using System.Threading;

namespace AR_Drone_Controller
{
    class ThreadSleeper
    {
        internal virtual void Sleep(int millisecondsToSleep)
        {
            var t = new ManualResetEvent(false);
            t.WaitOne(millisecondsToSleep);
        }
    }
}
