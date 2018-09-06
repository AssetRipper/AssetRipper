namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShaderVectorValue : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			W.Read(reader);
			Name = reader.ReadStringAligned();
		}

		public bool IsZero => X.IsZero && Y.IsZero && Z.IsZero && W.IsZero;

		public string Name { get; private set; }

		public SerializedShaderFloatValue X;
		public SerializedShaderFloatValue Y;
		public SerializedShaderFloatValue Z;
		public SerializedShaderFloatValue W;
	}
}
