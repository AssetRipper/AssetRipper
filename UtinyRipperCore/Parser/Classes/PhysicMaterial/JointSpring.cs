using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.PhysicMaterials
{
	public struct JointSpring : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Spring = stream.ReadSingle();
			Damper = stream.ReadSingle();
			TargetPosition = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
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
