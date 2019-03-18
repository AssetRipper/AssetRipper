using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public class DefaultImporter: IAssetImporter
	{
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			ExportYAMLPreInner(container, node);
			if (IsExportExternalObjects)
			{
				node.Add("externalObjects", YAMLMappingNode.Empty);
			}
			ExportYAMLInner(container, node);
			node.Add("userData", string.Empty);
			node.Add("assetBundleName", string.Empty);
			node.Add("assetBundleVariant", string.Empty);
			return node;
		}

		protected virtual void ExportYAMLPreInner(IExportContainer container, YAMLMappingNode node)
		{
		}

		protected virtual void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
		}

		public virtual string Name => nameof(DefaultImporter);

		protected virtual bool IsExportExternalObjects => true;
	}
}
