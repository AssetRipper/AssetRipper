using System;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateKey : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			StateID = stream.ReadUInt32();
			LayerIndex = stream.ReadInt32();
		}

		public override int GetHashCode()
		{
			return Tuple.Create(StateID, LayerIndex).GetHashCode();
		}

		public uint StateID { get; private set; }
		public int LayerIndex { get; private set; }
	}
}
