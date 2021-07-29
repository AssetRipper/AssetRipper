using AssetRipper.IO;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Classes.AnimatorController.Constants
{
	public struct LeafInfoConstant : IAssetReadable
	{
		public uint[] m_IDArray;
		public uint m_IndexOffset;

		public LeafInfoConstant(ObjectReader reader)
		{
			m_IDArray = reader.ReadUInt32Array();
			m_IndexOffset = reader.ReadUInt32();
		}

		public void Read(AssetReader reader)
		{
			m_IDArray = reader.ReadUInt32Array();
			m_IndexOffset = reader.ReadUInt32();
		}
	}
}
