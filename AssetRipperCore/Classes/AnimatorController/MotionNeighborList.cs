using AssetRipper.IO;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Classes.AnimatorController
{
	public struct MotionNeighborList : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			NeighborArray = reader.ReadUInt32Array();
		}

		public MotionNeighborList(ObjectReader reader)
		{
			NeighborArray = reader.ReadUInt32Array();
		}

		public uint[] NeighborArray { get; set; }
	}
}
