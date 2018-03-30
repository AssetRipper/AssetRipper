namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShaderVectorValue : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			X.Read(stream);
			Y.Read(stream);
			Z.Read(stream);
			W.Read(stream);
			Name = stream.ReadStringAligned();
		}

		public bool IsZero => X.IsZero && Y.IsZero && Z.IsZero && W.IsZero;

		public string Name { get; private set; }

		public SerializedShaderFloatValue X;
		public SerializedShaderFloatValue Y;
		public SerializedShaderFloatValue Z;
		public SerializedShaderFloatValue W;
	}
}
