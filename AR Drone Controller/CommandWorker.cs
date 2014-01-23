using System;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace AR_Drone_Controller
{
    internal class CommandWorker : IDisposable
    {
        public const int MinMillisecondsSinceLastTransmission = 30;
        public const int MaxMillisecondsOfInactivity = 200;
        public const string FtrimCommand = "FTRIM";
        public const string PmodeCommand = "PMODE";
        public const string MiscCommand = "MISC";
        public const string RefCommand = "REF";
        public const string GeneralNavdataDemoConfigKey = "general:navdata_demo";
        public const string TrueConfigValue = "TRUE";
        public const string CalibCommand = "CALIB";
        public const string ComwdgCommand = "COMWDG";
        public const string ConfigCommand = "CONFIG";
        public const string ConfigIdsCommand = "CONFIG_IDS";
        public const string CtrlCommand = "CTRL";
        public const string SessionIdConfigKey = "custom:session_id";
        public const string ProfileIdConfigKey = "custom:profile_id";
        public const string ApplicationIdConfigKey = "custom:application_id";
        public const string ApplicationDescConfigKey = "custom:application_desc";
        public const string ProﬁleDescConfigKey = "custom:proﬁle_desc";
        public const string SessionDescConfigKey = "custom:session_desc";
        public const string LedCommand = "LED";
        public const string AnimCommand = "ANIM";
        public const string PcmdMagCommand = "PCMD_MAG";
        public const string PcmdCommand = "PCMD";
        
        public readonly string SessionId =
            ("T&I AR Drone Remote SessionId").GetHashCode().ToString("X").ToLowerInvariant();

        public readonly string ProfileId =
            ("T&I AR Drone Remote ProfileId").GetHashCode().ToString("X").ToLowerInvariant();

        public readonly string ApplicationId =
            ("T&I AR Drone Remote ApplicationId").GetHashCode().ToString("X").ToLowerInvariant();
        
        public enum RefCommands
        {
            LandOrReset = 290717696,
            Emergency = 290717952,
            TakeOff = 290718208
        }

        public enum ControlMode
        {
            NoControlMode = 0, /*<! Doing nothing */
            ArdroneUpdateControlMode, /*<! Not used */
            PicUpdateControlMode, /*<! Not used */
            LogsGetControlMode, /*<! Not used */
            CfgGetControlMode, /*<! Send active configuration file to a client through the 'control' socket UDP 5559 */
            AckControlMode, /*<! Reset command mask in navdata */
            CustomCfgGetControlMode /*<! Requests the list of custom configuration IDs */
        }

        public CommandWorker()
        {
            TimeOfLastTransmission = DateTime.UtcNow;
            ThreadSleeper = new ThreadSleeper();
        }

        internal CommandQueue CommandQueue { get; set; }
        internal CommandFormatter CommandFormatter { get; set; }
        internal FloatToInt32Converter FloatToInt32Converter { get; set; }
        public ProgressiveCommandFormatter ProgressiveCommandFormatter { get; set; }
        internal IUdpSocket Socket { get; set; }

        internal ThreadSleeper ThreadSleeper { get; set; }

        public DateTime TimeOfLastTransmission { get; set; }

        public virtual void Run()
        {
            Socket.Connect();
            SendPModeCommand(2);
            SendMiscellaneousCommand("2,20,2000,3000");
            SendSetConfigurationCommand();
        }

        public virtual void SendFlatTrimCommand()
        {
            string command = CommandFormatter.CreateCommand(FtrimCommand);
            CommandQueue.Enqueue(command);
        }

        public virtual void SendConfigCommand(string key, string value)
        {
            var command = CreateConfigCommand(key, value);
            CommandQueue.Enqueue(command);
        }

        private string CreateConfigCommand(string key, string value)
        {
            string configIdsMessage = String.Format("\"{0}\",\"{1}\",\"{2}\"", SessionId, ProfileId, ApplicationId);
            string configIdsCommand = CommandFormatter.CreateCommand(ConfigIdsCommand, configIdsMessage);
            string configMessage = String.Format("\"{0}\",\"{1}\"", key, value);
            string configCommand = CommandFormatter.CreateCommand(ConfigCommand, configMessage);
            string command = configIdsCommand + configCommand;
            return command;
        }

        public virtual void SendRefCommand(RefCommands refCommand)
        {
            string command = CommandFormatter.CreateCommand(RefCommand, ((int)refCommand).ToString());
            CommandQueue.Enqueue(command);
        }

        public virtual void SendLedAnimationCommand(ILedAnimation ledAnimatoin)
        {
            int freq = FloatToInt32Converter.Convert(ledAnimatoin.FrequencyInHz);
            string message = string.Format("{0},{1},{2}", (int) ledAnimatoin.Animation, freq, ledAnimatoin.DurationInSeconds);
            string command = CommandFormatter.CreateCommand(LedCommand, message);
            CommandQueue.Enqueue(command);
        }

        public virtual void SendFlightAnimationCommand(IFlightAnimation flightAnimation)
        {
            string message = string.Format("{0},{1}", (int) flightAnimation.Animation,
                flightAnimation.MaydayTimeoutInMilliseconds);
            string command = CommandFormatter.CreateCommand(AnimCommand, message);
            CommandQueue.Enqueue(command);
        }

        public virtual void ExitBootStrapMode()
        {
            SendConfigCommand(GeneralNavdataDemoConfigKey, TrueConfigValue);
            var command = CreateAck();
            CommandQueue.Enqueue(command);
        }

        private string CreateAck()
        {
            string ackMessage = String.Format("{0},0", (int) ControlMode.NoControlMode);
            string command = CommandFormatter.CreateCommand(CtrlCommand, ackMessage);
            return command;
        }

        internal virtual void SendProgressiveCommand(IProgressiveCommand args)
        {
            string command;
            ProgressiveCommandFormatter.Load(args);

            if (args.AbsoluteControlMode)
            {
                string message = string.Format("{0},{1},{2},{3},{4},{5},{6}", (int)ProgressiveCommandFormatter.Mode,
                    ProgressiveCommandFormatter.Roll, ProgressiveCommandFormatter.Pitch, ProgressiveCommandFormatter.Gaz,
                    ProgressiveCommandFormatter.Yaw, ProgressiveCommandFormatter.MagnetoPsi,
                    ProgressiveCommandFormatter.MagnetoPsiAccuracy);
                command = CommandFormatter.CreateCommand(PcmdMagCommand, message);
            }
            else
            {
                string message = string.Format("{0},{1},{2},{3},{4}", (int)ProgressiveCommandFormatter.Mode,
                    ProgressiveCommandFormatter.Roll, ProgressiveCommandFormatter.Pitch, ProgressiveCommandFormatter.Gaz,
                    ProgressiveCommandFormatter.Yaw);
                 command = CommandFormatter.CreateCommand(PcmdCommand, message);
            }

            CommandQueue.Enqueue(command);
        }

        internal virtual void SendCalibrateCompassCommand()
        {
            string command = CommandFormatter.CreateCommand(CalibCommand, "0");
            CommandQueue.Enqueue(command);
        }

        internal virtual void SendResetWatchDogCommand()
        {
            string command = CommandFormatter.CreateCommand(ComwdgCommand);
            CommandQueue.Enqueue(command);
        }

        public virtual void Dispose()
        {
            SendRemainingCommands();

            if (MillisecondsSinceLastTransmition() < MinMillisecondsSinceLastTransmission)
            {
                ThreadSleeper.Sleep(MinMillisecondsSinceLastTransmission);
            }

            Socket.Dispose();
        }

        private void SendRemainingCommands()
        {
            string message = CommandQueue.Flush();
            while (!string.IsNullOrWhiteSpace(message))
            {
                TransmitCommand(message);
                message = CommandQueue.Flush();
            }
        }

        internal virtual void Flush()
        {
            string message = CommandQueue.Flush();
            if (!String.IsNullOrWhiteSpace(message))
            {
                TransmitCommand(message);
            }
            else if (MillisecondsSinceLastTransmition() > MaxMillisecondsOfInactivity)
            {
                string ack = CreateAck();
                TransmitCommand(ack);
            }
        }

        private double MillisecondsSinceLastTransmition()
        {
            return (DateTime.UtcNow - TimeOfLastTransmission).TotalMilliseconds;
        }

        private void TransmitCommand(string message)
        {
            Socket.Write(message);
            Debug.WriteLine(message);
            TimeOfLastTransmission = DateTime.UtcNow;
        }

        private void SendMiscellaneousCommand(string message)
        {
            string command = CommandFormatter.CreateCommand(MiscCommand, message);
            CommandQueue.Enqueue(command);
        }

        private void SendPModeCommand(int mode)
        {
            string command = CommandFormatter.CreateCommand(PmodeCommand, mode.ToString());
            CommandQueue.Enqueue(command);
        }

        private void SendSetConfigurationCommand()
        {
            var command = new StringBuilder();
            command.Append(CreateConfigCommand(SessionIdConfigKey, SessionId));
            command.Append(CreateConfigCommand(ProfileIdConfigKey, ProfileId));
            command.Append(CreateConfigCommand(ApplicationIdConfigKey, ApplicationId));
            command.Append(CreateConfigCommand(ApplicationDescConfigKey, "AR Drone Remote"));
            command.Append(CreateConfigCommand(ProﬁleDescConfigKey, ".Primary Profile"));
            command.Append(CreateConfigCommand(SessionDescConfigKey, "Session " + SessionId));
            CommandQueue.Enqueue(command.ToString());
        }
    }
}
