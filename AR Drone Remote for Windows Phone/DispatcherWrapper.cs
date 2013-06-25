using AR_Drone_Controller;
using System;
using System.Windows.Threading;

namespace AR_Drone_Remote_for_Windows_Phone
{
    class DispatcherWrapper : IDispatcher
    {
        public DispatcherWrapper(Dispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
        }

        protected Dispatcher Dispatcher { get; private set; }

        public void BeginInvoke(Action action)
        {
            this.Dispatcher.BeginInvoke(action);
        }
    }
}
