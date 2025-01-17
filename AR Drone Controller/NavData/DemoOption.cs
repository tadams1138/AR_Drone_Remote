﻿using System;

namespace AR_Drone_Controller.NavData
{
    using Common;
    using System.IO;

    public class DemoOption
    {
        public enum States : uint
        {
            Default,
            Init,
            Landed,
            Flying,
            Hovering,
            Test,
            TransTakeoff,
            TransGotofix,
            TransLanding,
            TransLooping,
            NumStates
        }

        private Matrix33 _detectionCameraRotation;

        private Vector _detectionCameraTrans;

        private Matrix33 _droneCameraRotation;

        private Vector _droneCameraTrans;

        public States State { get; internal set; }

        public virtual uint BatteryPercentage { get; internal set; }

        public virtual float Theta { get; internal set; }

        public virtual float Phi { get; internal set; }

        public virtual float Psi { get; internal set; }
        
        public virtual int Altitude { get; set; }

        public float Vx { get; internal set; }

        public float Vy { get; internal set; }

        public float Vz { get; internal set; }

        public virtual float KilometersPerHour
        {
            get { return (float)Math.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz) * 3.6f; }
        }

        public uint FrameNumber { get; internal set; }

        public uint DetectionTagIndex { get; internal set; }

        public uint DetectionCameraType { get; internal set; }

        internal static DemoOption FromReader(ushort size, BinaryReader reader)
        {
            Validate(size);
            var result = new DemoOption
            {
                State = (States)reader.ReadUInt32(),
                BatteryPercentage = reader.ReadUInt32(),
                Theta = reader.ReadSingle() / 1000f,
                Phi = reader.ReadSingle() / 1000f,
                Psi = reader.ReadSingle() / 1000f,
                Altitude = reader.ReadInt32() / 1000,
                Vx = reader.ReadSingle() / 1000f,
                Vy = reader.ReadSingle() / 1000f,
                Vz = reader.ReadSingle() / 1000f,
                FrameNumber = reader.ReadUInt32(),
                _detectionCameraRotation = Matrix33.FromReader(reader),
                _detectionCameraTrans = Vector.FromReader(reader),
                DetectionTagIndex = reader.ReadUInt32(),
                DetectionCameraType = reader.ReadUInt32(),
                _droneCameraRotation = Matrix33.FromReader(reader),
                _droneCameraTrans = Vector.FromReader(reader)
            };
            return result;
        }

        private static void Validate(ushort size)
        {
            // TODO: validate the size
        }
    }
}
