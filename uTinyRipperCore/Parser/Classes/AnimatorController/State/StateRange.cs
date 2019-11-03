namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateRange : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			StartIndex = (int)reader.ReadUInt32();
			Count = (int)reader.ReadUInt32();
		}

		public int StartIndex { get; set; }
		public int Count { get; set; }
	}
}
