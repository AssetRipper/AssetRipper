using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public class ValueArrayConstant
    {
        public ValueConstant[] m_ValueArray;

        public ValueArrayConstant(ObjectReader reader)
        {
            int numVals = reader.ReadInt32();
            m_ValueArray = new ValueConstant[numVals];
            for (int i = 0; i < numVals; i++)
            {
                m_ValueArray[i] = new ValueConstant(reader);
            }
        }
    }
}
