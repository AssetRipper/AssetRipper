using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.OcclusionCullingDatas
{
	public struct SceneObjectIdentifier : IAssetReadable, IYAMLExportable
	{
		public SceneObjectIdentifier(long targetObject, long targetPrefab)
		{
			TargetObject = targetObject;
			TargetPrefab = targetPrefab;
		}

		public void Read(AssetStream stream)
		{
			TargetObject = stream.ReadInt64();
			TargetPrefab = stream.ReadInt64();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("targetObject", TargetObject);
			node.Add("targetPrefab", TargetPrefab);
			return node;
		}

		public long TargetObject { get; private set; }
		public long TargetPrefab { get; private set; }
	}
}
