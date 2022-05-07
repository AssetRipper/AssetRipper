using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ConstantForce
{
	public sealed class ConstantForce : Behaviour
	{
		public ConstantForce(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Force.Read(reader);
			RelativeForce.Read(reader);
			Torque.Read(reader);
			RelativeTorque.Read(reader);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Force", Force.ExportYaml(container));
			node.Add("m_RelativeForce", RelativeForce.ExportYaml(container));
			node.Add("m_Torque", Torque.ExportYaml(container));
			node.Add("m_RelativeTorque", RelativeTorque.ExportYaml(container));
			return node;
		}

		public Vector3f Force = new();
		public Vector3f RelativeForce = new();
		public Vector3f Torque = new();
		public Vector3f RelativeTorque = new();
	}
}
