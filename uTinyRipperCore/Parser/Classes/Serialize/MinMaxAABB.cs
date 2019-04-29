using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public struct MinMaxAABB : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MinName, Min.ExportYAML(container));
			node.Add(MaxName, Max.ExportYAML(container));
			return node;
		}

		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";

		public Vector3f Min;
		public Vector3f Max;
	}
}
