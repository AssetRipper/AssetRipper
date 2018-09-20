namespace uTinyRipper.Classes.Textures
{
	public struct StreamingInfo : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			Offset = reader.ReadUInt32();
			Size = reader.ReadUInt32();
			Path = reader.ReadStringAligned();
		}

		public void Read(AssetReader reader, string path)
		{
			Size = reader.ReadUInt32();
			Offset = reader.ReadUInt32();
			Path = path;
		}

		public uint Offset { get; private set; }
		public uint Size { get; private set; }
		public string Path { get; private set; }
	}
}
