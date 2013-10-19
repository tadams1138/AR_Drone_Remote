namespace AR_Drone_Controller
{
    public class ConnectParams
    {
        public const int DefaultCommandPort =5556;
        public const int DefaultControlPort = 5559;
        public const int DefaultVideoPort = 5555;
        public const int DefaultNavDataPort = 5554;
        public const string DefaultNetworkAddress = "192.168.1.1";

        public ConnectParams()
        {
            NetworkAddress = DefaultNetworkAddress;
            ControlPort = DefaultControlPort;
            CommandPort = DefaultCommandPort;
            VideoPort = DefaultVideoPort;
            NavDataPort = DefaultNavDataPort;
        }

        public string NetworkAddress { get; set; }

        public int ControlPort { get; set; }

        public int CommandPort { get; set; }

        public int VideoPort { get; set; }

        public int NavDataPort { get; set; }
    }
}