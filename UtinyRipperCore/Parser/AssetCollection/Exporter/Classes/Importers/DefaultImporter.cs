using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class DefaultImporter: IYAMLExportable
	{
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("externalObjects", YAMLMappingNode.Empty);
			ExportYAMLInner(exporter, node);
			node.Add("userData", string.Empty);
			node.Add("assetBundleName", string.Empty);
			node.Add("assetBundleVariant", string.Empty);
			return node;
		}

		protected virtual void ExportYAMLInner(IAssetsExporter exporter, YAMLMappingNode node)
		{
		}
	}
}
