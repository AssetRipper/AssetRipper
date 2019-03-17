using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class SizeModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadAxes(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Curve.Read(reader);
			if (IsReadAxes(reader.Version))
			{
				Y.Read(reader);
				Z.Read(reader);
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
			node.Add("separateAxes", SeparateAxes);
			return node;
		}

		private MinMaxCurve GetExportY(Version version)
		{
			return IsReadAxes(version) ? Y : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}
		private MinMaxCurve GetExportZ(Version version)
		{
			return IsReadAxes(version) ? Z : new MinMaxCurve(1.0f, 1.0f, 1.0f, 0.0f, 1.0f);
		}

		public bool SeparateAxes { get; private set; }

		public MinMaxCurve Curve;
		public MinMaxCurve Y;
		public MinMaxCurve Z;
	}
}
