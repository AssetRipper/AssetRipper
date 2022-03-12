using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShaderVectorValue : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			X.Read(reader);
			Y.Read(reader);
			Z.Read(reader);
			W.Read(reader);
			Name = reader.ReadString();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("x", X.ExportYAML(container));
			node.Add("y", Y.ExportYAML(container));
			node.Add("z", Z.ExportYAML(container));
			node.Add("w", W.ExportYAML(container));
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
