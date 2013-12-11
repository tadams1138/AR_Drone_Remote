using System;

namespace AR_Drone_Controller
{
    class DateTimeFactory
    {
        public virtual DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
