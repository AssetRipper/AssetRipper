using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.OcclusionCullingDatas
{
	public struct SceneObjectIdentifier : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			TargetObject = stream.ReadInt64();
			TargetPrefab = stream.ReadInt64();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("targetObject", TargetObject);
			node.Add("targetPrefab", TargetObject);
			return node;
		}

		public long TargetObject { get; private set; }
		public long TargetPrefab { get; private set; }
	}
}
