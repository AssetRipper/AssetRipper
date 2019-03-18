using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class RotationBySpeedModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadAxes(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadAxes(reader.Version))
			{
				X.Read(reader);
				Y.Read(reader);
			}
			Curve.Read(reader);
			if (IsReadAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			
			Range.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("x", GetExportX(container.Version).ExportYAML(container));
			node.Add("y", GetExportY(container.Version).ExportYAML(container));
			node.Add("curve", Curve.ExportYAML(container));
			node.Add("separateAxes", SeparateAxes);
			node.Add("range", Range.ExportYAML(container));
			return node;
		}

		private MinMaxCurve GetExportX(Version version)
		{
			return IsReadAxes(version) ? X : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportY(Version version)
		{
			return IsReadAxes(version) ? Y : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Curve;
		public Vector2f Range;
	}
}
