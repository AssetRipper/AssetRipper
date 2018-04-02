using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class SizeModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadAxes(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Curve.Read(stream);
			if (IsReadAxes(stream.Version))
			{
				Y.Read(stream);
				Z.Read(stream);
				SeparateAxes = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("z", Z.ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
	}
}
