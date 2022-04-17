using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderVectorValue : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			W.Read(reader);
			Name = reader.ReadString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("x", X.ExportYaml(container));
			node.Add("y", Y.ExportYaml(container));
			node.Add("z", Z.ExportYaml(container));
			node.Add("w", W.ExportYaml(container));
			node.Add("name", Name);
			return node;
		}

		public bool IsZero => X.IsZero && Y.IsZero && Z.IsZero && W.IsZero;

		public string Name { get; set; }

		public SerializedShaderFloatValue X = new();
		public SerializedShaderFloatValue Y = new();
		public SerializedShaderFloatValue Z = new();
		public SerializedShaderFloatValue W = new();
	}
}
