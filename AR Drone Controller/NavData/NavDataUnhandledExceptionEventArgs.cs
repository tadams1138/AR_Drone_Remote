using System;

namespace AR_Drone_Controller.NavData
{
    class NavDataUnhandledExceptionEventArgs : EventArgs
    {
        public NavDataUnhandledExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}
