using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AR_Drone_Controller
{
    public class CommandWorker
    {
        private const int CommandPort = 5556;
        private const int TimeoutValue = 500;

        private int _seq;

        private IUdpSocket _cmdSocket;
        private bool _run;
        private readonly Queue<string> _commands = new Queue<string>();
        private readonly object _syncLock = new object();
        private DateTime _timeSinceLastSend;

        public string RemoteIpAddress { get; set; }

        public ISocketFactory SocketFactory { get; set; }

        public string SessionId { get; set; }
        public string ProfileId { get; set; }
        public string ApplocationId { get; set; }

        internal void Run()
        {
            if (!_run)
            {
                _run = true;
                _seq = 1;
                Task.Factory.StartNew(DoWork);
            }
        }

        internal void Stop()
        {
            _run = false;
        }

        const int MaxCommandLength = 1024;

        private void DoWork()
        {
            var message = new StringBuilder();
            CreateSocket();

            using (var manualResetEvent = new ManualResetEvent(false))
            {
                do
                {
                    bool anyCommands;

                    lock (_syncLock)
                    {
                        anyCommands = _commands.Any();
                    }

                    if (anyCommands)
                    {
                        AppendCommands(message);
                        _cmdSocket.Write(message.ToString());
                        _timeSinceLastSend = DateTime.UtcNow;
                    }
                    else if ((DateTime.UtcNow - _timeSinceLastSend).Milliseconds > 200)
                    {
                        SendAck();
                    }

                    // Sleep
                    manualResetEvent.WaitOne(30);
                } while (_run);
            }
        }

        private void AppendCommands(StringBuilder message)
        {
            message.Clear();
            lock (_syncLock)
            {
                while (_commands.Any() && message.Length + _commands.Peek().Length <= MaxCommandLength)
                {
                    message.Append(_commands.Dequeue());
                }
            }
        }

        private void CreateSocket()
        {
            _cmdSocket = SocketFactory.GetUdpSocket(CommandPort, RemoteIpAddress, CommandPort,
                                                    TimeoutValue);
        }

        public void SendFtrimCommand()
        {
            EnqueCommand("FTRIM", string.Empty);
        }

        public void SendMiscellaneousCommand(string message)
        {
            EnqueCommand("MISC", message);
        }

        public void SendPModeCommand(int mode)
        {
            EnqueCommand("PMODE", mode.ToString());
        }

        public void SendConfigCommand(string key, string value)
        {
            string configId = string.Format("\"{0}\",\"{1}\",\"{2}\"", SessionId, ProfileId, ApplocationId);
            string config = string.Format("\"{0}\",\"{1}\"", key, value);
            EnqueCommand("CONFIG_IDS", configId);
            EnqueCommand("CONFIG", config);
        }

        public enum RefCommands
        {
            LandOrReset = 290717696,
            Emergency = 290717952,
            TakeOff = 290718208
        }

        public void SendRefCommand(RefCommands command)
        {
            EnqueCommand("REF", ((int)command).ToString());
        }

        public enum LedAnimation
        {
            BlinkGreenRed = 0,
            BlinkGreen = 1,
            BlinkRed = 2,
            BlinkOrange = 3,
            SnakeGreenRed = 4,
            Fire = 5,
            Standard = 6,
            Red = 7,
            Green = 8,
            RedSnake = 9,
            Blank = 10,
            RightMissile = 11,
            LeftMissile = 12,
            DoubleMissile = 13,
            FrontLeftGreenOthersRed = 14,
            FrontRightGreenOthersRed = 15,
            RearRightGreenOthersRed = 16,
            RearLeftGreenOthersRed = 17,
            LeftGreenRightRed = 18,
            LeftRedRightGreen = 19,
            BlinkStandard = 20
        }

        public void SendLedCommand(LedAnimation command, float frequencyInHz, int durationInSeconds)
        {
            int frequency = ConvertFloatToInt32(frequencyInHz);
            string message = string.Format("{0},{1},{2}", (int)command, frequency,
                                           durationInSeconds);
            EnqueCommand("LED", message);
        }

        public void SendAck()
        {
            EnqueCommand("CTRL", "0");
        }

        private void EnqueCommand(string type, string message)
        {
            string command = string.Format("AT*{0}={1},{2}\r", type, _seq++, message);
            lock (_syncLock)
            {
                _commands.Enqueue(command);
            }
        }

        internal void SendProgressiveCommand(ProgressiveCommandArguments args)
        {
            EnqueCommand(args.GetCommand(), args.GetMessage());
        }

        public class ProgressiveCommandArguments
        {
            const float Threshold = 0.001f;

            public float Pitch { get; set; }
            public float Roll { get; set; }
            public float Gaz { get; set; }
            public float Yaw { get; set; }
            public float MagnetoPsi { get; set; }
            public float MagnetoPsiAccuracy { get; set; }
            public bool AbsoluteControl { get; set; }
            public bool CombineYaw { get; set; }

            public string GetCommand()
            {
                return AbsoluteControl ? "PCMD_MAG" : "PCMD";
            }

            public string GetMessage()
            {
                int pitchAsInt;
                int rollAsInt;
                int gazAsInt = ConvertFloatToInt32(Gaz);
                int yawAsInt = ConvertFloatToInt32(Yaw);
                int mode = 0;

                if (Math.Abs(Pitch) < Threshold && Math.Abs(Roll) < Threshold)
                {
                    pitchAsInt = 0;
                    rollAsInt = 0;
                }
                else
                {
                    pitchAsInt = ConvertFloatToInt32(Pitch);
                    rollAsInt = ConvertFloatToInt32(Roll);
                    mode = (int)Modes.EnableProgressiveCommands;
                }

                if (CombineYaw)
                {
                    mode |= (int)Modes.CombineYaw;
                }

                string message;
                if (AbsoluteControl)
                {
                    float normalizedMagnetoPsi = NormalizeMagnetoPsiDegrees();
                    float normalizedMagnetoPsiAccuracy = NormalizedMagnetoPsiAccuracy();
                    int magnetoPsiAsInt = ConvertFloatToInt32(normalizedMagnetoPsi);
                    int magnetoPsiAccuracyAsInt = ConvertFloatToInt32(normalizedMagnetoPsiAccuracy);
                    mode |= (int)Modes.AbsoluteControl;
                    message = string.Format("{0},{1},{2},{3},{4},{5},{6}", mode, rollAsInt, pitchAsInt, gazAsInt,
                                            yawAsInt, magnetoPsiAsInt, magnetoPsiAccuracyAsInt);
                }
                else
                {
                    message = string.Format("{0},{1},{2},{3},{4}", mode, rollAsInt, pitchAsInt, gazAsInt,
                                            yawAsInt);
                }

                return message;
            }

            [Flags]
            private enum Modes
            {
                EnableProgressiveCommands = 1,
                CombineYaw = 2,
                AbsoluteControl = 4
            }

            private float NormalizedMagnetoPsiAccuracy()
            {
                return Math.Abs(MagnetoPsiAccuracy / 360f);
            }

            private float NormalizeMagnetoPsiDegrees()
            {
                float degrees = MagnetoPsi % 360f;
                if (degrees <= 180)
                {
                    return degrees / 180;
                }
                else
                {
                    return degrees / 180 - 2;
                }
            }

            private int ConvertFloatToInt32(float value)
            {
                var bytes = BitConverter.GetBytes(value);
                int result = BitConverter.ToInt32(bytes, 0);
                return result;
            }
        }

        private int ConvertFloatToInt32(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            int result = BitConverter.ToInt32(bytes, 0);
            return result;
        }

        internal void SendCalibrationCommand()
        {
            EnqueCommand("CALIB", "0");
        }

        internal void SendResetWatchDogCommand()
        {
            string command = string.Format("AT*COMWDG={0}\r", _seq++);
            lock (_syncLock)
            {
                _commands.Enqueue(command);
            }
        }
    }
}
