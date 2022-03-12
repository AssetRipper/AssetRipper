using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class ParserBindChannels : IAssetReadable, IYAMLExportable
	{
		public ParserBindChannels() { }

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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Channels", Channels.ExportYAML(container));
			node.Add("m_SourceMap", SourceMap);
			return node;
		}

		public ShaderBindChannel[] Channels { get; set; }
		public int SourceMap { get; set; }
	}
}
