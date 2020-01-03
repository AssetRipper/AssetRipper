namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateKey : IAssetReadable
	{
		public StateKey(int layerIndex, uint stateID)
		{
			StateID = stateID;
			LayerIndex = layerIndex;
		}

		public static bool operator ==(StateKey left, StateKey right)
		{
			if (left.StateID != right.StateID)
			{
				return false;
			}
			if (left.LayerIndex != right.LayerIndex)
			{
				return false;
			}
			return true;
		}

		public static bool operator !=(StateKey left, StateKey right)
		{
			if (left.StateID != right.StateID)
			{
				return true;
			}
			if (left.LayerIndex != right.LayerIndex)
			{
				return true;
			}
			return false;
		}

		public void Read(AssetReader reader)
		{
			StateID = reader.ReadUInt32();
			LayerIndex = reader.ReadInt32();
		}

		public override bool Equals(object obj)
		{
			if (obj is StateKey stateKey)
			{
				return stateKey == this;
			}
			return false;
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

		public uint StateID { get; set; }
		public int LayerIndex { get; set; }
	}
}
