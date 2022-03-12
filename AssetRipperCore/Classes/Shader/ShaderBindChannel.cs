using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader
{
	public sealed class ShaderBindChannel : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
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
