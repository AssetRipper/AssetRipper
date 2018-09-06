namespace UtinyRipper.Classes.Meshes
{
	public struct Face : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			V1 = reader.ReadUInt16();
			V2 = reader.ReadUInt16();
			V3 = reader.ReadUInt16();
		}

		public ushort V1 { get; private set; }
		public ushort V2 { get; private set; }
		public ushort V3 { get; private set; }
	}
}
