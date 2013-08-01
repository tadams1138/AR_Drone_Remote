using System.IO;

namespace AR_Drone_Controller.NavData
{
    public class WifiOption
    {
        public uint LinkQuality { get; internal set; }

        public uint WifiStrength { get { return 100 - LinkQuality/5; } }

        internal static WifiOption FromReader(ushort size, BinaryReader reader)
        {
            Validate(size);
            var result = new WifiOption
            {
                LinkQuality = reader.ReadUInt32()
            };

            return result;
        }

        private static void Validate(ushort size)
        {
            // TODO:
        }
    }
}
