using System;

namespace AR_Drone_Controller
{
    class FloatToInt32Converter
    {
        internal virtual Int32 Convert(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            int result = BitConverter.ToInt32(bytes, 0);
            return result;
        }
    }
}
