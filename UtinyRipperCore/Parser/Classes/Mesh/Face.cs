namespace UtinyRipper.Classes.Meshes
{
	public struct Face : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			V1 = stream.ReadUInt16();
			V2 = stream.ReadUInt16();
			V3 = stream.ReadUInt16();
		}

		public ushort V1 { get; private set; }
		public ushort V2 { get; private set; }
		public ushort V3 { get; private set; }
	}
}
