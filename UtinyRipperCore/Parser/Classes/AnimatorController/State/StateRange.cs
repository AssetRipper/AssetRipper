namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateRange : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			StartIndex = stream.ReadUInt32();
			Count = stream.ReadUInt32();
		}

		public uint StartIndex { get; private set; }
		public uint Count { get; private set; }
	}
}
