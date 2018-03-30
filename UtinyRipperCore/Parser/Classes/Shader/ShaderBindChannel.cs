namespace UtinyRipper.Classes.Shaders
{
	public struct ShaderBindChannel : IAssetReadable
	{
		public ShaderBindChannel(ShaderChannel source, VertexComponent target)
		{
			Source = source;
			Target = target;
		}

		public void Read(AssetStream stream)
		{
			Source = (ShaderChannel)stream.ReadByte();
			Target = (VertexComponent)stream.ReadByte();
		}

		public ShaderChannel Source { get; private set; }
		public VertexComponent Target { get; private set; }
	}
}
