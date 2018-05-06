using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct OffsetPtr<T> : IAssetReadable, IYAMLExportable
		where T: struct, IAssetReadable, IYAMLExportable
	{
		public OffsetPtr(T instance)
		{
			Instance = instance;
		}

		public void Read(AssetStream stream)
		{
			Instance.Read(stream);
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
