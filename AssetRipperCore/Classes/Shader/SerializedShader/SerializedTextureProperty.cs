using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedTextureProperty : IAssetReadable, ISerializedTextureProperty, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			DefaultName = reader.ReadString();
			TexDim = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_DefaultName", DefaultName);
			node.Add("m_TexDim", TexDim);
			return node;
		}

		public string DefaultName { get; set; }
		public int TexDim { get; set; }
	}
}
