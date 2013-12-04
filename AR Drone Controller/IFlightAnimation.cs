namespace AR_Drone_Controller
{
    public interface IFlightAnimation
    {
        FlightAnimations Animation { get; set; }
        int MaydayTimeoutInMilliseconds { get; set; }
    }
}