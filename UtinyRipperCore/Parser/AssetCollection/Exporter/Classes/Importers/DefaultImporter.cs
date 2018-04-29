using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class DefaultImporter: IYAMLExportable
	{
		public DefaultImporter():
			this(true)
		{
		}

		public DefaultImporter(bool isExportExternalObjects)
		{
			m_isExportExternalObjects = isExportExternalObjects;
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if(m_isExportExternalObjects)
			{
				node.Add("externalObjects", YAMLMappingNode.Empty);
			}
			ExportYAMLInner(exporter, node);
			node.Add("userData", string.Empty);
			node.Add("assetBundleName", string.Empty);
			node.Add("assetBundleVariant", string.Empty);
			return node;
		}

		protected virtual void ExportYAMLInner(IAssetsExporter exporter, YAMLMappingNode node)
		{
		}

		private readonly bool m_isExportExternalObjects;
	}
}
