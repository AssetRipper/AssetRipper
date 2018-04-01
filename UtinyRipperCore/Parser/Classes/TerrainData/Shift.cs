namespace UtinyRipper.Classes.TerrainDatas
{
	public struct Shift : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			X = stream.ReadUInt16();
			Y = stream.ReadUInt16();
			Flags = stream.ReadUInt16();
		}

		public ushort X { get; private set; }
		public ushort Y { get; private set; }
		public ushort Flags { get; private set; }
	}
}
