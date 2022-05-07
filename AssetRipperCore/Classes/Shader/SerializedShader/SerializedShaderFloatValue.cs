using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderFloatValue : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Val = reader.ReadSingle();
			Name = reader.ReadString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("val", Val);
			node.Add("name", Name);
			return node;
		}

		public bool IsZero => Val == 0.0f;
		public bool IsMax => Val == 255.0f;

		public float Val { get; set; }
		public string Name { get; set; }
	}
}
