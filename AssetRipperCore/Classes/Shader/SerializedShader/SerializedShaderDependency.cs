using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderDependency : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadString();
			To = reader.ReadString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("from", From);
			node.Add("to", To);
			return node;
		}

		public string From { get; set; }
		public string To { get; set; }
	}
}
