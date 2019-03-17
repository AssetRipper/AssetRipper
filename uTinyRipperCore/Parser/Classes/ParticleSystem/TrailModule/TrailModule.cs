using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class TrailModule : ParticleSystemModule
	{
		public TrailModule()
		{
		}

		public TrailModule(bool _)
		{
			Ratio = 1.0f;
			Lifetime = new MinMaxCurve(1.0f);
			MinVertexDistance = 0.2f;
			RibbonCount = 1;
			DieWithParticles = true;
			SizeAffectsWidth = true;
			InheritParticleColor = true;
			ColorOverLifetime = new MinMaxGradient(true);
			WidthOverTrail = new MinMaxCurve(1.0f);
			ColorOverTrail = new MinMaxGradient(true);
		}

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
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadShadowBias(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
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
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadAttachRibbonsToTransform(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadMode(reader.Version))
			{
				Mode = (ParticleSystemTrailMode)reader.ReadInt32();
			}
			Ratio = reader.ReadSingle();
			Lifetime.Read(reader);
			MinVertexDistance = reader.ReadSingle();
			TextureMode = (ParticleSystemTrailTextureMode)reader.ReadInt32();
			if (IsReadRibbonCount(reader.Version))
			{
				RibbonCount = reader.ReadInt32();
			}
			if (IsReadShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			WorldSpace = reader.ReadBoolean();
			DieWithParticles = reader.ReadBoolean();
			SizeAffectsWidth = reader.ReadBoolean();
			SizeAffectsLifetime = reader.ReadBoolean();
			InheritParticleColor = reader.ReadBoolean();
			if (IsReadGenerateLightingData(reader.Version))
			{
				GenerateLightingData = reader.ReadBoolean();
			}
			if (IsReadSplitSubEmitterRibbons(reader.Version))
			{
				SplitSubEmitterRibbons = reader.ReadBoolean();
			}
			if (IsReadAttachRibbonsToTransform(reader.Version))
			{
				AttachRibbonsToTransform = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
			
			ColorOverLifetime.Read(reader);
			WidthOverTrail.Read(reader);
			ColorOverTrail.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(ModeName, (int)Mode);
			node.Add(RatioName, Ratio);
			node.Add(LifetimeName, Lifetime.ExportYAML(container));
			node.Add(MinVertexDistanceName, MinVertexDistance);
			node.Add(TextureModeName, (int)TextureMode);
			node.Add(RibbonCountName, GetExportRibbonCount(container.Version));
			if (IsReadShadowBias(container.ExportVersion))
			{
				node.Add(ShadowBiasName, ShadowBias);
			}
			node.Add(WorldSpaceName, WorldSpace);
			node.Add(DieWithParticlesName, DieWithParticles);
			node.Add(SizeAffectsWidthName, SizeAffectsWidth);
			node.Add(SizeAffectsLifetimeName, SizeAffectsLifetime);
			node.Add(InheritParticleColorName, InheritParticleColor);
			node.Add(GenerateLightingDataName, GenerateLightingData);
			node.Add(SplitSubEmitterRibbonsName, SplitSubEmitterRibbons);
			if (IsReadAttachRibbonsToTransform(container.ExportVersion))
			{
				node.Add(AttachRibbonsToTransformName, AttachRibbonsToTransform);
			}
			node.Add(ColorOverLifetimeName, ColorOverLifetime.ExportYAML(container));
			node.Add(WidthOverTrailName, WidthOverTrail.ExportYAML(container));
			node.Add(ColorOverTrailName, ColorOverTrail.ExportYAML(container));
			return node;

		}

		private int GetExportRibbonCount(Version version)
		{
			return IsReadRibbonCount(version) ? RibbonCount : 1;
		}

		public ParticleSystemTrailMode Mode { get; private set; }
		public float Ratio { get; private set; }
		public float MinVertexDistance { get; private set; }
		public ParticleSystemTrailTextureMode TextureMode { get; private set; }
		public int RibbonCount { get; private set; }
		public float ShadowBias { get; private set; }
		public bool WorldSpace { get; private set; }
		public bool DieWithParticles { get; private set; }
		public bool SizeAffectsWidth { get; private set; }
		public bool SizeAffectsLifetime { get; private set; }
		public bool InheritParticleColor { get; private set; }
		public bool GenerateLightingData { get; private set; }
		public bool SplitSubEmitterRibbons { get; private set; }
		public bool AttachRibbonsToTransform { get; private set; }

		public const string ModeName = "mode";
		public const string RatioName = "ratio";
		public const string LifetimeName = "lifetime";
		public const string MinVertexDistanceName = "minVertexDistance";
		public const string TextureModeName = "textureMode";
		public const string RibbonCountName = "ribbonCount";
		public const string ShadowBiasName = "shadowBias";
		public const string WorldSpaceName = "worldSpace";
		public const string DieWithParticlesName = "dieWithParticles";
		public const string SizeAffectsWidthName = "sizeAffectsWidth";
		public const string SizeAffectsLifetimeName = "sizeAffectsLifetime";
		public const string InheritParticleColorName = "inheritParticleColor";
		public const string GenerateLightingDataName = "generateLightingData";
		public const string SplitSubEmitterRibbonsName = "splitSubEmitterRibbons";
		public const string AttachRibbonsToTransformName = "attachRibbonsToTransform";
		public const string ColorOverLifetimeName = "colorOverLifetime";
		public const string WidthOverTrailName = "widthOverTrail";
		public const string ColorOverTrailName = "colorOverTrail";

		public MinMaxCurve Lifetime;
		public MinMaxGradient ColorOverLifetime;
		public MinMaxCurve WidthOverTrail;
		public MinMaxGradient ColorOverTrail;
	}
}
