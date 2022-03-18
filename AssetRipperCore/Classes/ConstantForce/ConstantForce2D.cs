using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ConstantForce
{
	public sealed class ConstantForce2D : Behaviour
	{
		public ConstantForce2D(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Force.Read(reader);
			RelativeForce.Read(reader);
			Torque = reader.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Force", Force.ExportYAML(container));
			node.Add("m_RelativeForce", RelativeForce.ExportYAML(container));
			node.Add("m_Torque", Torque);
			return node;
		}

		public Vector2f Force = new();
		public Vector2f RelativeForce = new();
		public float Torque { get; set; }
	}
}
