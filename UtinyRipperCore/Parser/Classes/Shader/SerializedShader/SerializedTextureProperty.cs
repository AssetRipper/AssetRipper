namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedTextureProperty : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			DefaultName = stream.ReadStringAligned();
			TexDim = stream.ReadInt32();
		}

		public string DefaultName { get; private set; }
		public int TexDim { get; private set; }
	}
}
