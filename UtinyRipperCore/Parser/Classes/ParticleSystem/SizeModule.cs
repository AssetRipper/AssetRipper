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

		private MinMaxCurve GetExportY(Version version)
		{
			return IsReadAxes(version) ? Y : Curve;
		}
		private MinMaxCurve GetExportZ(Version version)
		{
			return IsReadAxes(version) ? Z : Curve;
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
			node.Add("y", GetExportY(exporter.Version).ExportYAML(exporter));
			node.Add("z", GetExportZ(exporter.Version).ExportYAML(exporter));
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
	}
}
