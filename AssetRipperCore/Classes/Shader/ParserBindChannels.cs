using AssetRipper.IO.Asset;

namespace AssetRipper.Classes.Shader
{
	public struct ParserBindChannels : IAssetReadable
	{
		public ParserBindChannels(ShaderBindChannel[] channels, int sourceMap)
		{
			Channels = channels;
			SourceMap = sourceMap;
		}

		public void Read(AssetReader reader)
		{
			Channels = reader.ReadAssetArray<ShaderBindChannel>();
			reader.AlignStream();
			SourceMap = reader.ReadInt32();
		}

		public ShaderBindChannel[] Channels { get; set; }
		public int SourceMap { get; set; }
	}
}
