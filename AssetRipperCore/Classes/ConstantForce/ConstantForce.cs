using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

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

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Force", Force.ExportYAML(container));
			node.Add("m_RelativeForce", RelativeForce.ExportYAML(container));
			node.Add("m_Torque", Torque.ExportYAML(container));
			node.Add("m_RelativeTorque", RelativeTorque.ExportYAML(container));
			return node;
		}

		public Vector3f Force = new();
		public Vector3f RelativeForce = new();
		public Vector3f Torque = new();
		public Vector3f RelativeTorque = new();
	}
}
