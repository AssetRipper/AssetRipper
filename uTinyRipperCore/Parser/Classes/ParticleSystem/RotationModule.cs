using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class RotationModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasAxes(Version version) => version.IsGreaterEqual(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasAxes(reader.Version))
			{
				X.Read(reader);
				Y.Read(reader);
			}
			Curve.Read(reader);

			if (HasAxes(reader.Version))
			{
				SeparateAxes = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(XName, GetExportX(container.Version).ExportYAML(container));
			node.Add(YName, GetExportY(container.Version).ExportYAML(container));
			node.Add(CurveName, Curve.ExportYAML(container));
			node.Add(SeparateAxesName, SeparateAxes);
			return node;
		}

		private MinMaxCurve GetExportX(Version version)
		{
			return HasAxes(version) ? X : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetExportY(Version version)
		{
			return HasAxes(version) ? Y : new MinMaxCurve(0.0f);
		}

		public bool SeparateAxes { get; set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string CurveName = "curve";
		public const string SeparateAxesName = "separateAxes";

		public MinMaxCurve X;
		public MinMaxCurve Y;
		public MinMaxCurve Curve;
	}
}
