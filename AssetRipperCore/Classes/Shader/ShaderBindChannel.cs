using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class ShaderBindChannel : IAssetReadable, IYamlExportable
	{
		public ShaderBindChannel() { }

		public ShaderBindChannel(uint source, VertexComponent target)
		{
			Source = (byte)source;
			Target = target;
		}

		public void Read(AssetReader reader)
		{
			Source = reader.ReadByte();
			Target = (VertexComponent)reader.ReadByte();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("source", Source);
			node.Add("target", (byte)Target);
			return node;
		}

		/// <summary>
		/// ShaderChannel enum
		/// </summary>
		public byte Source { get; set; }
		public VertexComponent Target { get; set; }
	}
}
