using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class RotationModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadAxes(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		private MinMaxCurve GetExportX(Version version)
		{
			return IsReadAxes(version) ? X : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportY(Version version)
		{
			return IsReadAxes(version) ? Y : new MinMaxCurve(0.0f);
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
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("x", GetExportX(exporter.Version).ExportYAML(exporter));
			node.Add("y", GetExportY(exporter.Version).ExportYAML(exporter));
			node.Add("curve", Curve.ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Curve;
	}
}
