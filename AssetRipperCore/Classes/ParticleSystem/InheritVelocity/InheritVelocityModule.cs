using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem.InheritVelocity
{
	public sealed class InheritVelocityModule : ParticleSystemModule
	{
		public InheritVelocityModule() { }

		public InheritVelocityModule(float value) : base(value != 0.0f)
		{
			Curve = new MinMaxCurve(value);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Mode = (InheritVelocityMode)reader.ReadInt32();
			Curve.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(ModeName, (int)Mode);
			node.Add(CurveName, Curve.ExportYAML(container));
			return node;
		}

		public InheritVelocityMode Mode { get; set; }

		public const string ModeName = "m_Mode";
		public const string CurveName = "m_Curve";

		public MinMaxCurve Curve = new();
	}
}
