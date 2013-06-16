namespace AR_Drone_Controller.NavData
{
    using System.IO;

    internal class ChecksumOption
    {
        public const int OptionSize = 8;

        internal static uint FromBytes(ushort size, BinaryReader reader)
        {
            Validate(size);
            return reader.ReadUInt32();
        }

        private static void Validate(ushort size)
        {
            if (size != OptionSize)
            {
                string message = string.Format("Checksum size mismatch. Size specified {0} should have been {1}.",
                                               size, OptionSize);
                throw new NavData.InvalidNavDataException(message);
            }
        }
    }
}
