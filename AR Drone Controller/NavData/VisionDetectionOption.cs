namespace AR_Drone_Controller.NavData
{
    using Common;
    using System.IO;

    public class VisionDetectOption
    {
        private const int DetectionResults = 4;

        public uint[] CameraSource { get; internal set; }

        public Vector[] Translation { get; internal set; }

        public Matrix33[] Rotation { get; internal set; }

        public float[] OrientationAngle { get; internal set; }

        public uint[] Distance { get; internal set; }

        public uint[] Height { get; internal set; }

        public uint[] Width { get; internal set; }

        public uint Detected { get; internal set; }

        public uint[] Type { get; internal set; }

        public uint[] Xc { get; internal set; }

        public uint[] Yc { get; internal set; }

        internal static VisionDetectOption FromReader(ushort size, BinaryReader reader)
        {
            Validate(size);
            var result = new VisionDetectOption
            {
                Detected = reader.ReadUInt32(),
                Type = GetUInt32Array(reader, DetectionResults),
                Xc = GetUInt32Array(reader, DetectionResults),
                Yc = GetUInt32Array(reader, DetectionResults),
                Width = GetUInt32Array(reader, DetectionResults),
                Height = GetUInt32Array(reader, DetectionResults),
                Distance = GetUInt32Array(reader, DetectionResults),
                OrientationAngle = GetSingleArray(reader, DetectionResults),
                Rotation = Matrix33.GetMatrixArray(reader, DetectionResults),
                Translation = Vector.GetVectorArray(reader, DetectionResults),
                CameraSource = GetUInt32Array(reader, DetectionResults)
            };

            return result;
        }

        private static uint[] GetUInt32Array(BinaryReader reader, int elementCount)
        {
            var results = new uint[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                results[i] = reader.ReadUInt32();
            }

            return results;
        }

        private static float[] GetSingleArray(BinaryReader reader, int elementCount)
        {
            var results = new float[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                results[i] = reader.ReadSingle();
            }

            return results;
        }

        private static void Validate(ushort size)
        {
            // TODO: Validate size
        }
    }
}