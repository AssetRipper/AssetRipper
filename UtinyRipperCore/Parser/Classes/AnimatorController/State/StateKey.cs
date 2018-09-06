using System;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateKey : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			StateID = reader.ReadUInt32();
			LayerIndex = reader.ReadInt32();
		}

		public override int GetHashCode()
		{
			return Tuple.Create(StateID, LayerIndex).GetHashCode();
		}

		public uint StateID { get; private set; }
		public int LayerIndex { get; private set; }
	}
}
