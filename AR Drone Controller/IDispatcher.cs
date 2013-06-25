using System;

namespace AR_Drone_Controller
{
    public interface IDispatcher
    {
        void BeginInvoke(Action action);
    }
}
