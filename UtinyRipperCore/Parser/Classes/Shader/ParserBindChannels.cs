using System.Collections.Generic;

namespace UtinyRipper.Classes.Shaders
{
	public struct ParserBindChannels : IAssetReadable
	{
		public ParserBindChannels(ShaderBindChannel[] channels, int sourceMap)
		{
			m_channels = channels;
			SourceMap = sourceMap;
		}

		public void Read(AssetStream stream)
		{
			m_channels = stream.ReadArray<ShaderBindChannel>();
			stream.AlignStream(AlignType.Align4);
			SourceMap = stream.ReadInt32();
		}

		public IReadOnlyList<ShaderBindChannel> Channels => m_channels;
		public int SourceMap { get; private set; }

		private ShaderBindChannel[] m_channels;
	}
}
