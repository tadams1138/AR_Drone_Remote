namespace AR_Drone_Controller.Common
{
    using System.IO;

    public class Matrix33
    {
        private Vector[] _values = new Vector[3];

        internal static Matrix33 FromReader(BinaryReader reader)
        {
            var result = new Matrix33()
                {
                    _values = Vector.GetVectorArray(reader, 3)
                };

            return result;
        }

        internal static Matrix33[] GetMatrixArray(BinaryReader reader, int elementCount)
        {
            var results = new Matrix33[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                results[i] = FromReader(reader);
            }

            return results;
        }
    }
}
