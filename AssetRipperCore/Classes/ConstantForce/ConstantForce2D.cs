using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Force", Force.ExportYaml(container));
			node.Add("m_RelativeForce", RelativeForce.ExportYaml(container));
			node.Add("m_Torque", Torque);
			return node;
		}

		public Vector2f Force = new();
		public Vector2f RelativeForce = new();
		public float Torque { get; set; }
	}
}
