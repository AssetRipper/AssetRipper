using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class LifetimeByEmitterSpeedModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Curve.Read(reader);
			Range.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(RangeName, Range.ExportYAML(container));
			return node;
		}

		public const string CurveName = "m_Curve";
		public const string RangeName = "m_Range";

		public MinMaxCurve Curve = new();
		public Vector2f Range = new();
	}
}
