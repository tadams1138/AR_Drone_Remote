using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace AR_Drone_Controller
{
    public class CommandWorker : IDisposable
    {
        private const int CommandPort = 5556;
        private const int TimeoutValue = 500;

        private readonly Queue<string> _commands = new Queue<string>();
        private readonly object _syncLock = new object();
        
        private int _seq;
        private IUdpSocket _cmdSocket;
        private bool _run;
        private DateTime _timeSinceLastSend;
        private Timer _workerTimer;

        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        public string RemoteIpAddress { get; set; }
        public ISocketFactory SocketFactory { get; set; }
        public string SessionId { get; set; }
        public string ProfileId { get; set; }
        public string ApplicationId { get; set; }
        
        internal void Run()
        {
            if (!_run)
            {
                _run = true;
                _seq = 1;
                CreateSocket();
                _workerTimer = new Timer(DoWork, null, 30, 30);
            }
        }

        internal void Stop()
        {
            if (_workerTimer != null)
            {
                _workerTimer.Dispose();
                _workerTimer = null;
            }

            if (_cmdSocket != null)
            {
                _cmdSocket.Dispose();
                _cmdSocket = null;
            }

            _run = false;
        }

        const int MaxCommandLength = 1024;

        private void DoWork(object state)
        {
            try
            {
                string message = AppendCommands();

                if (!string.IsNullOrEmpty(message))
                {
                    _cmdSocket.Write(message);
                    _timeSinceLastSend = DateTime.UtcNow;
                }
                else if ((DateTime.UtcNow - _timeSinceLastSend).Milliseconds > 200)
                {
                    SendAck();
                }
            }
            catch (Exception ex)
            {
                if (UnhandledException != null)
                {
                    UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                }
            }
        }

        private string AppendCommands()
        {
            var message = new StringBuilder();
            lock (_syncLock)
            {
                while (_commands.Any() && message.Length + _commands.Peek().Length <= MaxCommandLength)
                {
                    message.Append(_commands.Dequeue());
                }
            }

            return message.ToString();
        }

        private void CreateSocket()
        {
            var getUdpSocketParams = new GetUdpSocketParams
                {
                    LocalPort = CommandPort,
                    RemoteIp = RemoteIpAddress,
                    RemotePort = CommandPort,
                    Timeout = TimeoutValue
                };
            _cmdSocket = SocketFactory.GetUdpSocket(getUdpSocketParams);
            _cmdSocket.Connect();
        }

        public void SendFlatTrimCommand()
        {
            EnqueCommand("FTRIM");
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
            string configId = string.Format("\"{0}\",\"{1}\",\"{2}\"", SessionId, ProfileId, ApplicationId);
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

        public void SendLedAnimationCommand(LedAnimation command, float frequencyInHz, int durationInSeconds)
        {
            int frequency = ConvertFloatToInt32(frequencyInHz);
            string message = string.Format("{0},{1},{2}", (int)command, frequency,
                                           durationInSeconds);
            EnqueCommand("LED", message);
        }

        public enum FlightAnimation
        {
            PhiMinus30Degrees = 0,
            PhiPlus30Degrees,
            ThetaMinus30Degrees,
            ThetaPlus30Degrees,
            Theta20DegYaw200Degrees,
            Theta20DegYawM200Degrees,
            Turnaround,
            TurnaroundAndGoDown,
            YawShake,
            YawDance,
            PhiDance,
            ThetaDance,
            VzDance,
            Wave,
            PhiThetaMixed,
            DoublePhiThetaMixed,
            FlipAhead,
            FlipBehind,
            FlipLeft,
            FlipRight
        }

        public void SendFlightAnimationCommand(FlightAnimation command, int maydayTimeoutInMilliseconds)
        {
            string message = string.Format("{0},{1}", (int)command, maydayTimeoutInMilliseconds);
            EnqueCommand("ANIM", message);
        }

        public void ExitBootStrapMode()
        {
            SendConfigCommand("general:navdata_demo", "TRUE");
            SendAck();
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

        public void SendCtrlCommand(ControlMode mode)
        {
            string message = ((int)mode).ToString() + ",0";
            EnqueCommand("CTRL", message);
        }

        private void SendAck()
        {
            SendCtrlCommand(0);
        }

        private void EnqueCommand(string type)
        {
            string command = string.Format("AT*{0}={1}\r", type, _seq++);
            lock (_syncLock)
            {
                _commands.Enqueue(command);
            }
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
                
                return degrees / 180 - 2;
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

        internal void SendCalibrateCompassCommand()
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

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
