namespace AR_Drone_Controller
{
    public class LedCommand
    {
        private const float DefaultFrequencyInHz = 0.5f;
        private const int DefautlDurationInSeconds = 5;

        internal string Title { get; set; }
        internal CommandWorker.LedAnimation Animation { get; set; }
        internal float FrequencyInHz { get; set; }
        internal int DurationInSeconds { get; set; }
        internal DroneController DroneController { get; set; }

        public LedCommand()
        {
            FrequencyInHz = DefaultFrequencyInHz;
            DurationInSeconds = DefautlDurationInSeconds;
        }

        public void Execute()
        {
            DroneController.SendLedCommand(Animation, FrequencyInHz, DurationInSeconds);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
