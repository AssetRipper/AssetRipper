using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class LeafInfoConstant : IAssetReadable
	{
		public uint[] m_IDArray;
		public uint m_IndexOffset;

		public void Read(AssetReader reader)
		{
			m_IDArray = reader.ReadUInt32Array();
			m_IndexOffset = reader.ReadUInt32();
		}
	}
}
