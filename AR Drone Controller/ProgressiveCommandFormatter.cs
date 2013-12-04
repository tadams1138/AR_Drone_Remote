using System;

namespace AR_Drone_Controller
{
    class ProgressiveCommandFormatter
    {
        public const float Threshold = 0.001f;

        [Flags]
        public enum Modes
        {
            EnableProgressiveCommands = 1,
            CombineYaw = 2,
            AbsoluteControl = 4
        }

        public ProgressiveCommandFormatter()
        {
            FloatToInt32Converter = new FloatToInt32Converter();
        }

        public virtual Modes Mode { get; set; }

        public virtual int Roll { get; set; }

        public virtual int Pitch { get; set; }

        public virtual int Gaz { get; set; }

        public virtual int Yaw { get; set; }

        public virtual int MagnetoPsi { get; set; }

        public virtual int MagnetoPsiAccuracy { get; set; }

        public FloatToInt32Converter FloatToInt32Converter { get; set; }

        internal virtual void Load(IProgressiveCommand progressiveCommand)
        {
            Gaz = FloatToInt32Converter.Convert(progressiveCommand.Gaz);
            Yaw = FloatToInt32Converter.Convert(progressiveCommand.Yaw);

            if (Math.Abs(progressiveCommand.Pitch) < Threshold && Math.Abs(progressiveCommand.Roll) < Threshold)
            {
                Pitch = 0;
                Roll = 0;
                Mode = 0;
            }
            else
            {
                Pitch = FloatToInt32Converter.Convert(progressiveCommand.Pitch);
                Roll = FloatToInt32Converter.Convert(progressiveCommand.Roll);
                Mode = Modes.EnableProgressiveCommands;
            }

            if (progressiveCommand.CombineYaw)
            {
                Mode |= Modes.CombineYaw;
            }

            if (progressiveCommand.AbsoluteControlMode)
            {
                float normalizedMagnetoPsi = NormalizeMagnetoPsiDegrees(progressiveCommand.ControllerHeading);
                MagnetoPsi = FloatToInt32Converter.Convert(normalizedMagnetoPsi);
                float normalizedMagnetoPsiAccuracy =
                    NormalizedMagnetoPsiAccuracy(progressiveCommand.ControllerHeadingAccuracy);
                MagnetoPsiAccuracy = FloatToInt32Converter.Convert(normalizedMagnetoPsiAccuracy);
                Mode |= Modes.AbsoluteControl;
            }
            else
            {
                MagnetoPsi = 0;
                MagnetoPsiAccuracy = 0;
            }
        }

        private float NormalizedMagnetoPsiAccuracy(float accuracy)
        {
            return Math.Abs(accuracy / 360f);
        }

        private float NormalizeMagnetoPsiDegrees(float magnetoPsi)
        {
            float degrees = magnetoPsi % 360f;
            if (degrees < 0)
            {
                degrees += 360;
            }

            if (degrees <= 180)
            {
                return degrees / 180;
            }

            return degrees / 180 - 2;
        }
    }
}
