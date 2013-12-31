using System;

namespace AR_Drone_Controller
{
    class DoubleToInt64Converter
    {
        internal virtual Int64 Convert(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Int64 result = BitConverter.ToInt64(bytes, 0);
            return result;
        }
    }
}
