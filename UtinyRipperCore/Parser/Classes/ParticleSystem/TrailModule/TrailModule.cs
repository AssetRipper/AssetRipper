using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class TrailModule : ParticleSystemModule
	{
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadMode(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadRibbonCount(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.1.0b2
		/// </summary>
		public static bool IsReadGenerateLightingData(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadSplitSubEmitterRibbons(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		private int GetExportRibbonCount(Version version)
		{
			return IsReadRibbonCount(version) ? RibbonCount : 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadMode(stream.Version))
			{
				Mode = (ParticleSystemTrailMode)stream.ReadInt32();
			}
			Ratio = stream.ReadSingle();
			Lifetime.Read(stream);
			MinVertexDistance = stream.ReadSingle();
			TextureMode = stream.ReadInt32();
			if (IsReadRibbonCount(stream.Version))
			{
				RibbonCount = stream.ReadInt32();
			}
			WorldSpace = stream.ReadBoolean();
			DieWithParticles = stream.ReadBoolean();
			SizeAffectsWidth = stream.ReadBoolean();
			SizeAffectsLifetime = stream.ReadBoolean();
			InheritParticleColor = stream.ReadBoolean();
			if (IsReadGenerateLightingData(stream.Version))
			{
				GenerateLightingData = stream.ReadBoolean();
			}
			if (IsReadSplitSubEmitterRibbons(stream.Version))
			{
				SplitSubEmitterRibbons = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
			
			ColorOverLifetime.Read(stream);
			WidthOverTrail.Read(stream);
			ColorOverTrail.Read(stream);
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("mode", (int)Mode);
			node.Add("ratio", Ratio);
			node.Add("lifetime", Lifetime.ExportYAML(exporter));
			node.Add("minVertexDistance", MinVertexDistance);
			node.Add("textureMode", TextureMode);
			node.Add("ribbonCount", GetExportRibbonCount(exporter.Version));
			node.Add("worldSpace", WorldSpace);
			node.Add("dieWithParticles", DieWithParticles);
			node.Add("sizeAffectsWidth", SizeAffectsWidth);
			node.Add("sizeAffectsLifetime", SizeAffectsLifetime);
			node.Add("inheritParticleColor", InheritParticleColor);
			node.Add("generateLightingData", GenerateLightingData);
			node.Add("splitSubEmitterRibbons", SplitSubEmitterRibbons);
			node.Add("colorOverLifetime", ColorOverLifetime.ExportYAML(exporter));
			node.Add("widthOverTrail", WidthOverTrail.ExportYAML(exporter));
			node.Add("colorOverTrail", ColorOverTrail.ExportYAML(exporter));
			return node;
		}

		public ParticleSystemTrailMode Mode { get; private set; }
		public float Ratio { get; private set; }
		public float MinVertexDistance { get; private set; }
		public int TextureMode { get; private set; }
		public int RibbonCount { get; private set; }
		public bool WorldSpace { get; private set; }
		public bool DieWithParticles { get; private set; }
		public bool SizeAffectsWidth { get; private set; }
		public bool SizeAffectsLifetime { get; private set; }
		public bool InheritParticleColor { get; private set; }
		public bool GenerateLightingData { get; private set; }
		public bool SplitSubEmitterRibbons { get; private set; }

		public MinMaxCurve Lifetime;
		public MinMaxGradient ColorOverLifetime;
		public MinMaxCurve WidthOverTrail;
		public MinMaxGradient ColorOverTrail;
	}
}
