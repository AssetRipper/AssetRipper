using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class SerializedProperties
    {
        public SerializedProperty[] m_Props;

        public SerializedProperties(BinaryReader reader)
        {
            int numProps = reader.ReadInt32();
            m_Props = new SerializedProperty[numProps];
            for (int i = 0; i < numProps; i++)
            {
                m_Props[i] = new SerializedProperty(reader);
            }
        }
    }
}
