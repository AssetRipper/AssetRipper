using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct TreePrototype : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Prefab.Read(stream);
			BendFactor = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("prefab", Prefab.ExportYAML(exporter));
			node.Add("bendFactor", BendFactor);
			return node;
		}

		public float BendFactor { get; private set; }

		public PPtr<GameObject> Prefab;
	}
}
