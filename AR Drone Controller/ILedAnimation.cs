namespace AR_Drone_Controller
{
    public interface ILedAnimation
    {
        LedAnimations Animation { get; set; }
        float FrequencyInHz { get; set; }
        int DurationInSeconds { get; set; }
    }
}