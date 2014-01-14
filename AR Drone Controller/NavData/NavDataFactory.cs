namespace AR_Drone_Controller.NavData
{
    class NavDataFactory
    {
        internal virtual NavData Create(byte[] bytes)
        {
            return NavData.FromBytes(bytes);
        }
    }
}
