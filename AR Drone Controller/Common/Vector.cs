namespace AR_Drone_Controller.Common
{
    using System.IO;

    public class Vector
    {
        private readonly float[] _values = new float[3];

        internal static Vector FromReader(BinaryReader reader)
        {
            var result = new Vector();
            for (int i = 0; i < 3; i++)
            {
                result._values[i] = reader.ReadSingle();
            }

            return result;
        }

        internal static Vector[] GetVectorArray(BinaryReader reader, int elementCount)
        {
            var results = new Vector[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                results[i] = FromReader(reader);
            }

            return results;
        }
    }
}
