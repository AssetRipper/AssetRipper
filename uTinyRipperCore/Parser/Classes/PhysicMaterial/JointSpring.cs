using uTinyRipper.AssetExporters;
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
			node.Add("spring", Spring);
			node.Add("damper", Damper);
			node.Add("targetPosition", TargetPosition);
			return node;
		}

		public float Spring { get; private set; }
		public float Damper { get; private set; }
		public float TargetPosition { get; private set; }
	}
}
