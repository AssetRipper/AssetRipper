using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderDependency : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadString();
			To = reader.ReadString();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("from", From);
			node.Add("to", To);
			return node;
		}

		public string From { get; set; }
		public string To { get; set; }
	}
}
