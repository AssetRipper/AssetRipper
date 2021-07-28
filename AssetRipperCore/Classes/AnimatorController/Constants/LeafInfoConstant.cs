using AssetRipper.IO.Asset;

namespace AssetRipper.Classes.AnimatorController.Constants
{
	public struct LeafInfoConstant : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			IDArray = reader.ReadUInt32Array();
			IndexOffset = (int)reader.ReadUInt32();
		}

		public uint[] IDArray { get; set; }
		public int IndexOffset { get; set; }
	}
}
