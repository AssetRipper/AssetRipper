namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderBindChannel : IAssetReadable
	{
		public ShaderBindChannel(ShaderChannel source, VertexComponent target)
		{
			Source = source;
			Target = target;
		}

		public void Read(AssetReader reader)
		{
			Source = (ShaderChannel)reader.ReadByte();
			Target = (VertexComponent)reader.ReadByte();
		}

		public ShaderChannel Source { get; set; }
		public VertexComponent Target { get; set; }
	}
}
