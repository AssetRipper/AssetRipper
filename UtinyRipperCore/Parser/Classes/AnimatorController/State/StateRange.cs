namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateRange : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			StartIndex = (int)stream.ReadUInt32();
			Count = (int)stream.ReadUInt32();
		}

		public int StartIndex { get; private set; }
		public int Count { get; private set; }
	}
}
