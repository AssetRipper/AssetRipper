namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedTextureProperty : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			DefaultName = reader.ReadString();
			TexDim = reader.ReadInt32();
		}

		public string DefaultName { get; set; }
		public int TexDim { get; set; }
	}
}
