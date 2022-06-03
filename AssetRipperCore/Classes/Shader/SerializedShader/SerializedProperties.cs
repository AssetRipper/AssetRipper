using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProperties : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			m_Props = reader.ReadAssetArray<SerializedProperty>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_Props", m_Props.ExportYaml(container));
			return node;
		}

		private SerializedProperty[] m_Props;

		public SerializedProperty[] Props => m_Props;
	}
}
