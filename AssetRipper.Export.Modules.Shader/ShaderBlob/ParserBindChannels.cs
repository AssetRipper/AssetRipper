namespace ShaderTextRestorer.ShaderBlob
{
	public sealed class ParserBindChannels
	{
		public ParserBindChannels() { }

		public ParserBindChannels(ShaderBindChannel[] channels, int sourceMap)
		{
			Channels = channels;
			SourceMap = sourceMap;
		}

		public ShaderBindChannel[] Channels { get; set; } = System.Array.Empty<ShaderBindChannel>();
		public int SourceMap { get; set; }
	}
}
