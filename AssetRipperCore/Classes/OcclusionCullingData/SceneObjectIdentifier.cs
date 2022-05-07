using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.OcclusionCullingData
{
	public sealed class SceneObjectIdentifier : UnityAssetBase, ISceneObjectIdentifier
	{
		public override void Read(AssetReader reader)
		{
			TargetObject = reader.ReadInt64();
			TargetPrefab = reader.ReadInt64();
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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
