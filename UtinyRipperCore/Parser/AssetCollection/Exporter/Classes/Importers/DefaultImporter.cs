using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class DefaultImporter: IAssetImporter
	{
		public DefaultImporter():
			this(true)
		{
		}

		public DefaultImporter(bool isExportExternalObjects)
		{
			m_isExportExternalObjects = isExportExternalObjects;
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if(m_isExportExternalObjects)
			{
				node.Add("externalObjects", YAMLMappingNode.Empty);
			}
			ExportYAMLInner(container, node);
			node.Add("userData", string.Empty);
			node.Add("assetBundleName", string.Empty);
			node.Add("assetBundleVariant", string.Empty);
			return node;
		}

		protected virtual void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
		}

		public virtual string Name => nameof(DefaultImporter);

		private readonly bool m_isExportExternalObjects;
	}
}
