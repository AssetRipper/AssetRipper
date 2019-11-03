using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.OcclusionCullingDatas
{
	public struct SceneObjectIdentifier : IAssetReadable, IYAMLExportable
	{
		public SceneObjectIdentifier(long targetObject, long targetPrefab)
		{
			TargetObject = targetObject;
			TargetPrefab = targetPrefab;
		}

		public void Read(AssetReader reader)
		{
			TargetObject = reader.ReadInt64();
			TargetPrefab = reader.ReadInt64();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TargetObjectName, TargetObject);
			node.Add(TargetPrefabName, TargetPrefab);
			return node;
		}

		public long TargetObject { get; set; }
		public long TargetPrefab { get; set; }

		public const string TargetObjectName = "targetObject";
		public const string TargetPrefabName = "targetPrefab";
	}
}
