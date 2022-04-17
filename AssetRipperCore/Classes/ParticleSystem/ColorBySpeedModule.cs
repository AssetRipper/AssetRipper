using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

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

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(GradientName, Gradient.ExportYaml(container));
			node.Add(RangeName, Range.ExportYaml(container));
			return node;
		}

		public const string GradientName = "gradient";
		public const string RangeName = "range";

		public MinMaxGradient.MinMaxGradient Gradient = new();
		public Vector2f Range = new();
	}
}
