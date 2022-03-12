using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProperties : IAssetReadable, ISerializedProperties, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_Props = reader.ReadAssetArray<SerializedProperty>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Props", m_Props.ExportYAML(container));
			return node;
		}

		private SerializedProperty[] m_Props;

		public ISerializedProperty[] Props => m_Props;
	}
}
