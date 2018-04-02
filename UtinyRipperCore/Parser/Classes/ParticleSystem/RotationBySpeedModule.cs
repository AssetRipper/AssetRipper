using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class RotationBySpeedModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadAxes(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadAxes(stream.Version))
			{
				X.Read(stream);
				Y.Read(stream);
			}
			Curve.Read(stream);
			if (IsReadAxes(stream.Version))
			{
				SeparateAxes = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			
			Range.Read(stream);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("x", X.ExportYAML(exporter));
			node.Add("y", Y.ExportYAML(exporter));
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			node.Add("range", Range.ExportYAML(exporter));
			return node;
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Curve;
		public Vector2f Range;
	}
}
