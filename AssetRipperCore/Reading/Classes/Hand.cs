using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
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
