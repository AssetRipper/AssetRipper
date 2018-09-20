namespace uTinyRipper.Classes.TerrainDatas
{
	public struct Shift : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadUInt16();
			Y = reader.ReadUInt16();
			Flags = reader.ReadUInt16();
		}

		public ushort X { get; private set; }
		public ushort Y { get; private set; }
		public ushort Flags { get; private set; }
	}
}
