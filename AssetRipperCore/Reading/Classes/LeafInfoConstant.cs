using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class LeafInfoConstant
    {
        public uint[] m_IDArray;
        public uint m_IndexOffset;

        public LeafInfoConstant(ObjectReader reader)
        {
            m_IDArray = reader.ReadUInt32Array();
            m_IndexOffset = reader.ReadUInt32();
        }
    }
}
