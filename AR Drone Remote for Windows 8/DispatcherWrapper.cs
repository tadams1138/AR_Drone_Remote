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

        public async void BeginInvoke(Action action)
        {
           await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action.Invoke);
        }
    }
}
