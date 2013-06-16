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
            HdvideoStream = 25,
            Wifi = 26,
            Zimmu3000 = 27,
            Checksum = 65535
        }

        [Flags]
        internal enum ARDRONE_STATE_MASK
        {
            ARDRONE_FLY_MASK = 1 << 0, // FLY MASK                  : (0) Ardrone is landed, (1) Ardrone is flying
            ARDRONE_VIDEO_MASK = 1 << 1, // VIDEO MASK                : (0) Video disable, (1) Video enable
            ARDRONE_VISION_MASK = 1 << 2, // VISION MASK               : (0) Vision disable, (1) Vision enable
            ARDRONE_CONTROL_MASK = 1 << 3,
            // CONTROL ALGO              : (0) Euler angles control, (1) Angular speed control
            ARDRONE_ALTITUDE_MASK = 1 << 4,
            // ALTITUDE CONTROL ALGO     : (0) Altitude control inactive (1) Altitude control active
            ARDRONE_USER_FEEDBACK_START = 1 << 5, // USER feedback             :     Start button state 
            ARDRONE_COMMAND_MASK = 1 << 6, // Control command ACK       : (0) None, (1) One received
//  ARDRONE_FW_FILE_MASK        = 1 <<  7, //                           : (1) Firmware file is good
//  ARDRONE_FW_VER_MASK         = 1 <<  8, //                           : (1) Firmware update is newer
//  ARDRONE_FW_UPD_MASK         = 1 <<  9, //                           : (1) Firmware update is ongoing
            ARDRONE_NAVDATA_DEMO_MASK = 1 << 10, // Navdata demo              : (0) All navdata, (1) Only navdata demo
            ARDRONE_NAVDATA_BOOTSTRAP = 1 << 11,
            // Navdata bootstrap         : (0) Options sent in all or demo mode, (1) No navdata options sent
            ARDRONE_MOTORS_MASK = 1 << 12, // Motors status             : (0) Ok, (1) Motors problem
            ARDRONE_COM_LOST_MASK = 1 << 13, // Communication Lost        : (1) Com problem, (0) Com is ok
            ARDRONE_VBAT_LOW = 1 << 15, // VBat low                  : (1) Too low, (0) Ok
            ARDRONE_USER_EL = 1 << 16, // User Emergency Landing    : (1) User EL is ON, (0) User EL is OFF
            ARDRONE_TIMER_ELAPSED = 1 << 17, // Timer elapsed             : (1) Elapsed, (0) Not elapsed
            ARDRONE_ANGLES_OUT_OF_RANGE = 1 << 19, // Angles                    : (0) Ok, (1) Out of range
            ARDRONE_ULTRASOUND_MASK = 1 << 21, // Ultrasonic sensor         : (0) Ok, (1) Deaf
            ARDRONE_CUTOUT_MASK = 1 << 22, // Cutout system detection   : (0) Not detected, (1) Detected
            ARDRONE_PIC_VERSION_MASK = 1 << 23,
            // PIC Version number OK     : (0) A bad version number, (1) Version number is OK */
            ARDRONE_ATCODEC_THREAD_ON = 1 << 24, // ATCodec thread ON         : (0) Thread OFF (1) thread ON
            ARDRONE_NAVDATA_THREAD_ON = 1 << 25, // Navdata thread ON         : (0) Thread OFF (1) thread ON
            ARDRONE_VIDEO_THREAD_ON = 1 << 26, // Video thread ON           : (0) Thread OFF (1) thread ON
            ARDRONE_ACQ_THREAD_ON = 1 << 27, // Acquisition thread ON     : (0) Thread OFF (1) thread ON
            ARDRONE_CTRL_WATCHDOG_MASK = 1 << 28,
            // CTRL watchdog             : (1) Delay in control execution (> 5ms), (0) Control is well scheduled
            ARDRONE_ADC_WATCHDOG_MASK = 1 << 29,
            // ADC Watchdog              : (1) Delay in uart2 dsr (> 5ms), (0) Uart2 is good
            ARDRONE_COM_WATCHDOG_MASK = 1 << 30, // Communication Watchdog    : (1) Com problem, (0) Com is ok
            ARDRONE_EMERGENCY_MASK = 1 << 31 // Emergency landing         : (0) No emergency, (1) Emergency
        }

        private const uint Header = 1432778632;

        private static uint _previousSequence;

        private uint _header;
        private int _state;
        private uint _sequence;
        private uint _vision;

        public bool CommunicationWatchDogState
        {
            get { return (_state & (int)ARDRONE_STATE_MASK.ARDRONE_COM_WATCHDOG_MASK) > 0; }
        }

        public bool FlyingState { get { return (_state & (int)ARDRONE_STATE_MASK.ARDRONE_FLY_MASK) > 0; } }

        public DemoOption Demo { get; internal set; }

        internal uint CheckSum { get; set; }

        public VisionDetectOption VisionDetect { get; internal set; }

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

                default:
                    throw new OptionNotImplementedException(optionId);
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