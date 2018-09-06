namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedTextureProperty : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			DefaultName = reader.ReadStringAligned();
			TexDim = reader.ReadInt32();
		}

		public string DefaultName { get; private set; }
		public int TexDim { get; private set; }
	}
}
