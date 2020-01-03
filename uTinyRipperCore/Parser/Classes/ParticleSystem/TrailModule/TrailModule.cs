using uTinyRipper.Converters;
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
		public static bool HasMode(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasRibbonCount(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasShadowBias(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2017.1.0b2
		/// </summary>
		public static bool HasGenerateLightingData(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSplitSubEmitterRibbons(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasAttachRibbonsToTransform(Version version) => version.IsGreaterEqual(2018, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasMode(reader.Version))
			{
				Mode = (ParticleSystemTrailMode)reader.ReadInt32();
			}
			Ratio = reader.ReadSingle();
			Lifetime.Read(reader);
			MinVertexDistance = reader.ReadSingle();
			TextureMode = (ParticleSystemTrailTextureMode)reader.ReadInt32();
			if (HasRibbonCount(reader.Version))
			{
				RibbonCount = reader.ReadInt32();
			}
			if (HasShadowBias(reader.Version))
			{
				ShadowBias = reader.ReadSingle();
			}
			WorldSpace = reader.ReadBoolean();
			DieWithParticles = reader.ReadBoolean();
			SizeAffectsWidth = reader.ReadBoolean();
			SizeAffectsLifetime = reader.ReadBoolean();
			InheritParticleColor = reader.ReadBoolean();
			if (HasGenerateLightingData(reader.Version))
			{
				GenerateLightingData = reader.ReadBoolean();
			}
			if (HasSplitSubEmitterRibbons(reader.Version))
			{
				SplitSubEmitterRibbons = reader.ReadBoolean();
			}
			if (HasAttachRibbonsToTransform(reader.Version))
			{
				AttachRibbonsToTransform = reader.ReadBoolean();
			}
			reader.AlignStream();
			
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
			if (HasShadowBias(container.ExportVersion))
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
			if (HasAttachRibbonsToTransform(container.ExportVersion))
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
			return HasRibbonCount(version) ? RibbonCount : 1;
		}

		public ParticleSystemTrailMode Mode { get; set; }
		public float Ratio { get; set; }
		public float MinVertexDistance { get; set; }
		public ParticleSystemTrailTextureMode TextureMode { get; set; }
		public int RibbonCount { get; set; }
		public float ShadowBias { get; set; }
		public bool WorldSpace { get; set; }
		public bool DieWithParticles { get; set; }
		public bool SizeAffectsWidth { get; set; }
		public bool SizeAffectsLifetime { get; set; }
		public bool InheritParticleColor { get; set; }
		public bool GenerateLightingData { get; set; }
		public bool SplitSubEmitterRibbons { get; set; }
		public bool AttachRibbonsToTransform { get; set; }

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
