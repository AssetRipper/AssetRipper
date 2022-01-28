using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class ColorBySpeedModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gradient.Read(reader);
			Range.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(GradientName, Gradient.ExportYAML(container));
			node.Add(RangeName, Range.ExportYAML(container));
			return node;
		}

		public const string GradientName = "gradient";
		public const string RangeName = "range";

		public MinMaxGradient.MinMaxGradient Gradient = new();
		public Vector2f Range = new();
	}
}
