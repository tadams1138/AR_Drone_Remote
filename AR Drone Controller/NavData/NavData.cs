using System.Diagnostics;

namespace AR_Drone_Controller.NavData
{
    using System;
    using System.IO;

    public class NavData
    {
        internal enum NavDataTag : ushort
        {
            Demo = 0,
            Time = 1,
            RawMeasures = 2,
            PhysMeasures = 3,
            GyrosOffsets = 4,
            EulerAngles = 5,
            References = 6,
            Trims = 7,
            RcReferences = 8,
            Pwm = 9,
            Altitude = 10,
            VisionRaw = 11,
            VisionOf = 12,
            VISION = 13,
            VISIONPerf = 14,
            TrackersSend = 15,
            VisionDetect = 16,
            Watchdog = 17,
            AdcDataFrame = 18,
            VideoStream = 19,
            Games = 20,
            PressureRaw = 21,
            Magneto = 22,
            Wind = 23,
            KalmanPressure = 24,
            HdVideoStream = 25,
            Wifi = 26,
            Zimmu3000 = 27,
            Checksum = 65535
        }

        [Flags]
        internal enum ArdroneStateMask
        {
            Flying = 1 << 0,
            VideoEnabled = 1 << 1,
            VisionEnabled = 1 << 2,
            ControlMask = 1 << 3, // CONTROL ALGO              : (0) Euler angles control, (1) Angular speed control
            AltitudeControl = 1 << 4, // ALTITUDE CONTROL ALGO     : (0) Altitude control inactive (1) Altitude control active
            UserFeedbackStart = 1 << 5, // USER feedback             :     Start button state 
            CommandMask = 1 << 6, // Control command ACK       : (0) None, (1) One received
            CameraReady = 1 << 7,
            Travelling = 1 << 8,
            UsbKeyReady = 1 << 9,
            DemoDataOnly = 1 << 10, // Navdata demo              : (0) All navdata, (1) Only navdata demo
            NavdataBootstrap = 1 << 11, // Navdata bootstrap         : (0) Options sent in all or demo mode, (1) No navdata options sent
            MotorProblem = 1 << 12,
            CommLost = 1 << 13,
            SoftwareFault = 1 << 14,
            BatteryTooLow = 1 << 15,
            EmergencyLanding = 1 << 16, // User Emergency Landing    : (1) User EL is ON, (0) User EL is OFF
            TimerElapsed = 1 << 17,
            MagnetometerNeedsCalibration = 1 << 18,
            AnglesOutOfRange = 1 << 19,
            TooMuchWind = 1 << 20,
            UltrasoundDeaf = 1 << 21,
            CutoutDetected = 1 << 22,
            PicVersionNumberOk = 1 << 23,
            AtCodecThreadOn = 1 << 24,
            NavDataThreadOn = 1 << 25,
            VideoThreadOn = 1 << 26,
            AcquisitionThreadOn = 1 << 27,
            ControlWatchdog = 1 << 28, // CTRL watchdog             : (1) Delay in control execution (> 5ms), (0) Control is well scheduled
            AdcWatchdog = 1 << 29, // ADC Watchdog              : (1) Delay in uart2 dsr (> 5ms), (0) Uart2 is good
            CommWatchdog = 1 << 30, // Communication Watchdog    : (1) Com problem, (0) Com is ok
            Emergency = 1 << 31 // Emergency landing         : (0) No emergency, (1) Emergency
        }

        private const uint Header = 1432778632;

        private static uint _previousSequence;

        private uint _header;
        private int _state;
        private uint _sequence;
        private uint _vision;

        public static void ResetSequence()
        {
            _previousSequence = 0;
        }

        private bool IsStateBitOne(ArdroneStateMask bitmask)
        {
            return (_state & (int)bitmask) > 0;
        }

        public bool ArdroneAcqThreadState
        {
            get { return IsStateBitOne(ArdroneStateMask.AcquisitionThreadOn); }
        }

        public bool AdcWatchdog { get { return IsStateBitOne(ArdroneStateMask.AdcWatchdog); } }

        public bool AltitudeControl { get { return IsStateBitOne(ArdroneStateMask.AltitudeControl); } }

        public bool AnglesOutOfRange { get { return IsStateBitOne(ArdroneStateMask.AnglesOutOfRange); } }

        public bool AtCodecThreadOn { get { return IsStateBitOne(ArdroneStateMask.AtCodecThreadOn); } }

        public bool CommLost { get { return IsStateBitOne(ArdroneStateMask.CommLost); } }

        public bool CommWatchDog { get { return IsStateBitOne(ArdroneStateMask.CommWatchdog); } }

        public bool CommandState { get { return IsStateBitOne(ArdroneStateMask.CommandMask); } }

        public bool ControlState { get { return IsStateBitOne(ArdroneStateMask.ControlMask); } }

        public bool ControlWatchdog { get { return IsStateBitOne(ArdroneStateMask.ControlWatchdog); } }

        public bool CutoutDetected { get { return IsStateBitOne(ArdroneStateMask.CutoutDetected); } }

        public bool Emergency { get { return IsStateBitOne(ArdroneStateMask.Emergency); } }

        public bool Flying { get { return IsStateBitOne(ArdroneStateMask.Flying); } }

        public bool Landed { get { return !IsStateBitOne(ArdroneStateMask.Flying); } }

        public bool MotorsProblem { get { return IsStateBitOne(ArdroneStateMask.MotorProblem); } }

        public bool NavdataBootstrap { get { return IsStateBitOne(ArdroneStateMask.NavdataBootstrap); } }

        public bool DemoDataOnly { get { return IsStateBitOne(ArdroneStateMask.DemoDataOnly); } }

        public bool NavDataThreadOn { get { return IsStateBitOne(ArdroneStateMask.NavDataThreadOn); } }

        public bool PicVersionNumberOk { get { return IsStateBitOne(ArdroneStateMask.PicVersionNumberOk); } }

        public bool TimerElapsed { get { return IsStateBitOne(ArdroneStateMask.TimerElapsed); } }

        public bool UltrasoundDeaf { get { return IsStateBitOne(ArdroneStateMask.UltrasoundDeaf); } }

        public bool UserEmergencyLanding { get { return IsStateBitOne(ArdroneStateMask.EmergencyLanding); } }

        public bool UserFeedbackStart { get { return IsStateBitOne(ArdroneStateMask.UserFeedbackStart); } }

        public bool VisionEnabled { get { return IsStateBitOne(ArdroneStateMask.VisionEnabled); } }

        public bool BatteryTooLow { get { return IsStateBitOne(ArdroneStateMask.BatteryTooLow); } }

        public bool VideoEnabled { get { return IsStateBitOne(ArdroneStateMask.VideoEnabled); } }

        public bool VideoThreadOn { get { return IsStateBitOne(ArdroneStateMask.VideoThreadOn); } }

        public bool CameraReady { get { return IsStateBitOne(ArdroneStateMask.CameraReady); } }

        public bool Travelling { get { return IsStateBitOne(ArdroneStateMask.VideoThreadOn); } }

        public bool UsbReady { get { return IsStateBitOne(ArdroneStateMask.VideoThreadOn); } }

        public bool MagnetometerNeedsCalibration { get { return IsStateBitOne(ArdroneStateMask.MagnetometerNeedsCalibration); } }

        public bool TooMuchWind { get { return IsStateBitOne(ArdroneStateMask.TooMuchWind); } }

        public bool SoftwareFault { get { return IsStateBitOne(ArdroneStateMask.SoftwareFault); } }

        public DemoOption Demo { get; set; }

        internal uint CheckSum { get; set; }

        public VisionDetectOption VisionDetect { get; internal set; }

        public WifiOption Wifi { get; internal set; }

        public HdVideoStreamOption HdVideoStream { get; set; }

        public static NavData FromBytes(byte[] buffer)
        {
            var result = new NavData();
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                ReadHeaderInformation(result, reader);

                NavDataTag optionId;
                do
                {
                    optionId = (NavDataTag)reader.ReadUInt16();
                    ushort size = reader.ReadUInt16();
                    result.AddOption(optionId, size, reader);
                } while (optionId != NavDataTag.Checksum);
            }

            result.ValidateChecksum(buffer);

            return result;
        }

        private static void ReadHeaderInformation(NavData result, BinaryReader reader)
        {
            result._header = reader.ReadUInt32();
            result._state = reader.ReadInt32();
            result._sequence = reader.ReadUInt32();
            result._vision = reader.ReadUInt32();

            result.ValidateHeader();
            result.ValidateSequence();
        }

        private void ValidateSequence()
        {
            if (_sequence <= _previousSequence)
            {
                throw new OutOfSequenceException(_sequence);
            }

            _previousSequence = _sequence;
        }

        private void ValidateHeader()
        {
            if (_header != Header)
            {
                throw new InvalidHeaderException(_header);
            }
        }

        private void ValidateChecksum(byte[] buffer)
        {
            uint checkSum = 0;
            int length = buffer.Length - ChecksumOption.OptionSize;
            for (int i = 0; i < length; i++)
            {
                checkSum += buffer[i];
            }

            if (CheckSum != checkSum)
            {
                throw new InvalidCheckSumException(CheckSum, checkSum);
            }
        }

        private void AddOption(NavDataTag optionId, ushort size, BinaryReader reader)
        {
            switch (optionId)
            {
                case NavDataTag.Demo:
                    Demo = DemoOption.FromReader(size, reader);
                    break;

                case NavDataTag.Checksum:
                    CheckSum = ChecksumOption.FromBytes(size, reader);
                    break;

                case NavDataTag.VisionDetect:
                    VisionDetect = VisionDetectOption.FromReader(size, reader);
                    break;

                case NavDataTag.Wifi:
                    Wifi = WifiOption.FromReader(size, reader);
                    break;

                case NavDataTag.HdVideoStream:
                    HdVideoStream = HdVideoStreamOption.FromReader(size, reader);
                    break;

                default:
                    if (false && Debugger.IsAttached)
                    {
                        // An unhandled navdata option was detected; break into the debugger
                        Debugger.Break();
                    }
                    else
                    {
                        for (int i = 0; i < size - 4; i++)
                        {
                            reader.ReadByte();
                        }
                    }

                    break;
            }
        }

        public class InvalidHeaderException : InvalidNavDataException
        {
            internal InvalidHeaderException(uint header)
                : base(string.Format("Expected NavData to start with {0}. Instead it started with {1}.",
                                     Header, header))
            {
            }
        }

        public class OutOfSequenceException : InvalidNavDataException
        {
            internal OutOfSequenceException(uint sequence)
                : base(string.Format("Expected sequence greater than {0}. Instead received {1}.",
                                     _previousSequence, sequence))
            {
            }
        }

        public class InvalidCheckSumException : InvalidNavDataException
        {
            public InvalidCheckSumException(uint receivedCheckSum, uint calculatedCheckSum) :
                base(
                string.Format("Navdata checksum did not match. Received {0} but calculated {1}.", receivedCheckSum,
                              calculatedCheckSum))
            {
            }
        }

        public class OptionNotImplementedException : InvalidNavDataException
        {
            internal OptionNotImplementedException(NavDataTag optionId)
                : base(string.Format("NavData option with tage {0} is not implemented",
                                     optionId))
            {
            }
        }

        public class InvalidNavDataException : Exception
        {
            public InvalidNavDataException(string message)
                : base(message)
            {
            }
        }
    }
}