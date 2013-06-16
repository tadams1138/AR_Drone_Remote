using System;

namespace AR_Drone_Controller.NavData
{
    class NavDataReceivedEventArgs : EventArgs
    {
        public NavDataReceivedEventArgs(NavData navData)
        {
            NavData = navData;
        }

        public NavData NavData { get; private set; }
    }
}
