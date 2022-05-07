using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class ColorModule : ParticleSystemModule
	{
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Gradient.Read(reader);
		}

		public override YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = (YamlMappingNode)base.ExportYaml(container);
			node.Add(GradientName, Gradient.ExportYaml(container));
			return node;
		}

		public const string GradientName = "gradient";

		public MinMaxGradient.MinMaxGradient Gradient = new();
	}
}
