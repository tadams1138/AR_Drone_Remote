﻿namespace AR_Drone_Controller.NavData
{
    class NavDataFactory
    {
        internal virtual NavData Create(byte[] bytes)
        {
            var result = NavData.FromBytes(bytes);

            result.ReceivedDemoOption = result.Demo != null;
            result.ReceivedHdVideoStreamOption = result.HdVideoStream != null;

            if (result.Demo == null)
            {
                result.Demo = new DemoOption();
            }

            if (result.HdVideoStream == null)
            {
                result.HdVideoStream = new HdVideoStreamOption();
            }

            return result;
        }
    }
}
