using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public class Hand
    {
        public int[] m_HandBoneIndex;

        public Hand(ObjectReader reader)
        {
            m_HandBoneIndex = reader.ReadInt32Array();
        }
    }
}
