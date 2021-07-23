using AssetRipper.IO.Asset;

namespace AssetRipper.Parser.Classes.AnimatorController
{
	public struct MotionNeighborList : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			NeighborArray = reader.ReadUInt32Array();
		}

		public uint[] NeighborArray { get; set; }
	}
}
