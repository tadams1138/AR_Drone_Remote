using System;
using System.Threading;

namespace AR_Drone_Controller.NavData
{
    class TimerFactory
    {
        public TimerCallback TimerCallback { get; set; }

        public int Period { get; set; }

        public virtual IDisposable CreateTimer()
        {
            return new Timer(TimerCallback, null, Period, Period);
        }
    }
}
