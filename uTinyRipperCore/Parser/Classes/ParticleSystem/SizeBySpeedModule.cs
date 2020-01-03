using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class SizeBySpeedModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasAxes(Version version) => version.IsGreaterEqual(5, 4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Curve.Read(reader);
			if (HasAxes(reader.Version))
			{
				Y.Read(reader);
				Z.Read(reader);
			}
			Range.Read(reader);
			if (HasAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(YName, GetExportY(container.Version).ExportYAML(container));
			node.Add(ZName, GetExportZ(container.Version).ExportYAML(container));
			node.Add(RangeName, Range.ExportYAML(container));
			node.Add(SeparateAxesName, SeparateAxes);
			return node;
		}

		private MinMaxCurve GetExportY(Version version)
		{
			return HasAxes(version) ? Y : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}
		private MinMaxCurve GetExportZ(Version version)
		{
			return HasAxes(version) ? Z : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}

		public bool SeparateAxes { get; set; }

		public const string CurveName = "curve";
		public const string YName = "y";
		public const string ZName = "z";
		public const string RangeName = "range";
		public const string SeparateAxesName = "separateAxes";

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
		public Vector2f Range;
	}
}
