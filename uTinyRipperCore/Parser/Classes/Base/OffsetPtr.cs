using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct OffsetPtr<T> : IAssetReadable, IYAMLExportable
		where T: struct, IAssetReadable, IYAMLExportable
	{
		public OffsetPtr(T instance)
		{
			Instance = instance;
		}

		public void Read(AssetReader reader)
		{
			Instance.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("data", Instance.ExportYAML(container));
			return node;
		}

		public T Instance;
	}
}
