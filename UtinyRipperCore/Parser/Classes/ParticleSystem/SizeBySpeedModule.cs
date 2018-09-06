using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class SizeBySpeedModule : ParticleSystemModule
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Curve.Read(reader);
			if (IsReadAxes(reader.Version))
			{
				Y.Read(reader);
				Z.Read(reader);
			}
			Range.Read(reader);
			if (IsReadAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("y", GetExportY(container.Version).ExportYAML(container));
			node.Add("z", GetExportZ(container.Version).ExportYAML(container));
			node.Add("range", Range.ExportYAML(container));
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public Vector2f Range;
	}
}
