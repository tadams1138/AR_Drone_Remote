namespace AR_Drone_Controller.NavData
{
    class NavDataFactory
    {
        internal virtual NavData Create(byte[] bytes)
        {
            NavData.ResetSequence();
            var result = NavData.FromBytes(bytes);

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
