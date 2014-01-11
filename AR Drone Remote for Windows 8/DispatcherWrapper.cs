using AR_Drone_Controller;
using System;
using Windows.UI.Core;

namespace AR_Drone_Remote_for_Windows_8
{
    class DispatcherWrapper : IDispatcher
    {
        public DispatcherWrapper(CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        protected CoreDispatcher Dispatcher { get; private set; }

        public void BeginInvoke(Action action)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action.Invoke);
        }
    }
}
