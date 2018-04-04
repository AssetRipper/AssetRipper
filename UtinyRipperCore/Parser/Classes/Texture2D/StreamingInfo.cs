namespace UtinyRipper.Classes.Textures
{
	public struct StreamingInfo : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Offset = stream.ReadUInt32();
			Size = stream.ReadUInt32();
			Path = stream.ReadStringAligned();
		}

		public void Read(AssetStream stream, string path)
		{
			Size = stream.ReadUInt32();
			Offset = stream.ReadUInt32();
			Path = path;
		}

		public uint Offset { get; private set; }
		public uint Size { get; private set; }
		public string Path { get; private set; }
	}
}
