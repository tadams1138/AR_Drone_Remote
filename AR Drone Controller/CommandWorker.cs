using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AR_Drone_Controller
{
    class CommandWorker
    {
        private const int CommandPort = 5556;
        private const int TimeoutValue = 500;

        private int _seq;

        private IUdpSocket _cmdSocket;
        private bool _run;
        private readonly Queue<string> _commands = new Queue<string>();
        private readonly object _syncLock = new object();
        private DateTime _timeSinceLastSend;

        public string LocalIpAddress { get; set; }

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

        private void DoWork()
        {
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
                        lock (_syncLock)
                        {
                            while (_commands.Any())
                            {
                                _cmdSocket.Write(_commands.Dequeue());
                            }
                        }

                        _timeSinceLastSend = DateTime.UtcNow;
                    }
                    else
                    {
                        if ((DateTime.UtcNow - _timeSinceLastSend).Seconds > 1)
                        {
                            SendAck();
                        }
                        else
                        {
                            // Sleep
                            manualResetEvent.WaitOne(100);
                        }
                    }
                } while (_run);
            }
        }

        private void CreateSocket()
        {
            _cmdSocket = SocketFactory.GetUdpSocket(LocalIpAddress, CommandPort, RemoteIpAddress, CommandPort,
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

        internal void SendProgressiveCommand(float pitch, float roll, float gaz, float yaw)
        {
            const float gain = 1f;
            pitch *= gain;
            roll *= gain;
            gaz *= gain;
            yaw *= gain;

            int mode = (Math.Abs(pitch) < 0.1 && Math.Abs(roll) < 0.1) ? 0 : 1;
            if (mode == 0)
            {
                pitch = 0;
                roll = 0;
            }

            int pitchAsInt = ConvertFloatToInt32(gain * pitch);
            int rollAsInt = ConvertFloatToInt32(gain * roll);
            int gazAsInt = ConvertFloatToInt32(gain * gaz);
            int yawAsInt = ConvertFloatToInt32(gain * yaw);
            
            string message = string.Format("{0},{1},{2},{3},{4}", mode, rollAsInt, pitchAsInt, gazAsInt, yawAsInt);
            EnqueCommand("PCMD", message);
        }

        private int ConvertFloatToInt32(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            int result = BitConverter.ToInt32(bytes, 0);
            return result;
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
