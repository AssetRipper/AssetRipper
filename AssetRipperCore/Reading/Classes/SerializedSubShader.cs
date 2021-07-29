using AssetRipper.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedSubShader
    {
        public SerializedPass[] m_Passes;
        public SerializedTagMap m_Tags;
        public int m_LOD;

        public SerializedSubShader(ObjectReader reader)
        {
            int numPasses = reader.ReadInt32();
            m_Passes = new SerializedPass[numPasses];
            for (int i = 0; i < numPasses; i++)
            {
                m_Passes[i] = new SerializedPass(reader);
            }

            m_Tags = new SerializedTagMap(reader);
            m_LOD = reader.ReadInt32();
        }
    }
}
