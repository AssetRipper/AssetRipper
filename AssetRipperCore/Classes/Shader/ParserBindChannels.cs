using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class ParserBindChannels : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_Channels", Channels.ExportYaml(container));
			node.Add("m_SourceMap", SourceMap);
			return node;
		}

		public ShaderBindChannel[] Channels { get; set; }
		public int SourceMap { get; set; }
	}
}
