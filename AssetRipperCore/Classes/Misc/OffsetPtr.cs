using AssetRipper.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Classes.Misc
{
	public class OffsetPtr<T> : IAssetReadable, IYAMLExportable where T : IAssetReadable, IYAMLExportable, new()
	{
		public OffsetPtr()
		{
			Instance = new();
		}

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
			node.Add(DataName, Instance.ExportYAML(container));
			return node;
		}

		public const string DataName = "data";

		public T Instance;
	}
}
