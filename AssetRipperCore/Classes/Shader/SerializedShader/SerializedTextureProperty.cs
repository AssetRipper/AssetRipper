using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedTextureProperty : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			DefaultName = reader.ReadString();
			TexDim = reader.ReadInt32();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_DefaultName", DefaultName);
			node.Add("m_TexDim", TexDim);
			return node;
		}

		public string DefaultName { get; set; }
		public int TexDim { get; set; }
	}
}
