using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct ParserBindChannels : IAssetReadable
	{
		public ParserBindChannels(ShaderBindChannel[] channels, int sourceMap)
		{
			m_channels = channels;
			SourceMap = sourceMap;
		}

		public void Read(AssetReader reader)
		{
			m_channels = reader.ReadAssetArray<ShaderBindChannel>();
			reader.AlignStream(AlignType.Align4);
			SourceMap = reader.ReadInt32();
		}

		public IReadOnlyList<ShaderBindChannel> Channels => m_channels;
		public int SourceMap { get; private set; }

		private ShaderBindChannel[] m_channels;
	}
}
