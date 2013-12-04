namespace AR_Drone_Controller
{
    public interface IProgressiveCommand
    {
        float Pitch { get; set; }
        float Roll { get; set; }
        float Yaw { get; set; }
        float Gaz { get; set; }
        float ControllerHeading { get; set; }
        float ControllerHeadingAccuracy { get; set; }
        bool AbsoluteControlMode { get; set; }
        bool CombineYaw { get; set; }
    }
}