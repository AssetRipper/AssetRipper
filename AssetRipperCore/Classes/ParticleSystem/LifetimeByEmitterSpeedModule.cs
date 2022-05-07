using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

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

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(CurveName, Curve.ExportYaml(container));
			node.Add(RangeName, Range.ExportYaml(container));
			return node;
		}

		public const string CurveName = "m_Curve";
		public const string RangeName = "m_Range";

		public MinMaxCurve Curve = new();
		public Vector2f Range = new();
	}
}
