using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.PhysicMaterials
{
	public struct JointSpring : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Spring = reader.ReadSingle();
			Damper = reader.ReadSingle();
			TargetPosition = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(SpringName, Spring);
			node.Add(DamperName, Damper);
			node.Add(TargetPositionName, TargetPosition);
			return node;
		}

		public float Spring { get; set; }
		public float Damper { get; set; }
		public float TargetPosition { get; set; }

		public const string SpringName = "spring";
		public const string DamperName = "damper";
		public const string TargetPositionName = "targetPosition";
	}
}
