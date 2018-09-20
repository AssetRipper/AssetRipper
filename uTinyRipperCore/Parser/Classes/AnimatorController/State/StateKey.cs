namespace uTinyRipper.Classes.AnimatorControllers
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
			int hash = 103;
			unchecked
			{
				hash = hash + 29 * StateID.GetHashCode();
				hash = hash * 97 + LayerIndex.GetHashCode();
			}
			return hash;
		}

		public uint StateID { get; private set; }
		public int LayerIndex { get; private set; }
	}
}
