using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AnimatorController
{
	public sealed class MotionNeighborList : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			NeighborArray = reader.ReadUInt32Array();
		}

		public uint[] NeighborArray { get; set; }
	}
}
