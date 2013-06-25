using AR_Drone_Controller;
using System;
using System.Windows.Threading;

namespace AR_Drone_Remote_for_Windows_Desktop
{
    class DispatcherWrapper : IDispatcher
    {
        public DispatcherWrapper(Dispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        protected Dispatcher Dispatcher { get; private set; }

        public void BeginInvoke(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }
    }
}
