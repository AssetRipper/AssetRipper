using System.IO;

namespace AssetRipper.Reading.Classes
{
	public class UAVParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_OriginalIndex;

        public UAVParameter(BinaryReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_OriginalIndex = reader.ReadInt32();
        }
    }
}
