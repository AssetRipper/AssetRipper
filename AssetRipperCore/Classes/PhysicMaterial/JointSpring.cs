using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.PhysicMaterial
{
	public sealed class JointSpring : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Spring = reader.ReadSingle();
			Damper = reader.ReadSingle();
			TargetPosition = reader.ReadSingle();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
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
