using System.Collections.Generic;
using uTinyRipper.Classes.RenderSettingss;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// LightManager previously
	/// </summary>
	public sealed class RenderSettings : LevelGameManager
	{
		public RenderSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool IsReadFogMode(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool IsReadLinearFogStart(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAmbientEquatorColor(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b
		/// </summary>
		public static bool IsReadAmbientSkyboxLight(Version version)
		{
			// unknown version
			return version.IsEqual(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAmbientMode(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b
		/// </summary>
		public static bool IsReadCreateAmbientLight(Version version)
		{
			// unknown version
			return version.IsEqual(5, 0, 0, VersionType.Beta);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadSubtractiveShadowColor(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadFlareFadeSpeed(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public static bool IsReadPixelLightCount(Version version)
		{
			return version.IsLess(1, 5);
		}
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public static bool IsReadAmbientLightScale(Version version)
		{
			return version.IsLess(1, 5);
		}
		/// <summary>
		/// Less than 1.6.0
		/// </summary>
		public static bool IsReadSpecularTex(Version version)
		{
			return version.IsLess(1, 6);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDefaultReflectionMode(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadDefaultReflectionResolution(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadCustomReflection(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 and Release
		/// </summary>
		public static bool IsReadAmbientProbe(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0f1 to 5.3.0 exclusive and Release
		/// </summary>
		public static bool IsReadAmbientProbeInGamma(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 3) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and (Release or ResourcesFile)
		/// </summary>
		public static bool IsReadGeneratedSkyboxReflection(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) &&  (flags.IsRelease() || flags.IsBuiltinResources());
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadSun(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadIndirectSpecularColor(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadUseRadianceAmbientProbe(Version version)
		{
			return version.IsGreaterEqual(2018);
		}

		/// <summary>
		/// Less than 5.0.0f1
		/// </summary>
		private static bool IsReadAmbientProbeFirst(Version version)
		{
			// unknown version
			return version.IsLess(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO:
			/*if (version.IsGreaterEqual(2018))
			{
				return 9;
			}*/
			if (version.IsGreaterEqual(5, 6))
			{
				return 8;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 7;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 6;
			}
			// unknown (beta) version
			// return 5;
			// unknown (beta) version
			// return 4;
			// unknown (beta) version
			// return 3;
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Fog = reader.ReadBoolean();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			
			FogColor.Read(reader);
			if (IsReadFogMode(reader.Version))
			{
				FogMode = (FogMode)reader.ReadInt32();
			}
			FogDensity = reader.ReadSingle();
			if (IsReadLinearFogStart(reader.Version))
			{
				LinearFogStart = reader.ReadSingle();
				LinearFogEnd = reader.ReadSingle();
			}
			AmbientSkyColor.Read(reader);
			if (IsReadAmbientEquatorColor(reader.Version))
			{
				AmbientEquatorColor.Read(reader);
				AmbientGroundColor.Read(reader);
				AmbientIntensity = reader.ReadSingle();
			}
			if (IsReadAmbientProbe(reader.Version, reader.Flags))
			{
				if (IsReadAmbientProbeFirst(reader.Version))
				{
					AmbientProbe.Read(reader);
				}
			}
			if (IsReadAmbientSkyboxLight(reader.Version))
			{
				AmbientSkyboxLight.Read(reader);
			}
			if (IsReadAmbientMode(reader.Version))
			{
				AmbientMode = (AmbientMode)reader.ReadInt32();
				if (IsReadCreateAmbientLight(reader.Version))
				{
					CreateAmbientLight = reader.ReadBoolean();
				}
				reader.AlignStream();
			}
			if (IsReadSubtractiveShadowColor(reader.Version))
			{
				SubtractiveShadowColor.Read(reader);
			}
			
			SkyboxMaterial.Read(reader);
			HaloStrength = reader.ReadSingle();
			FlareStrength = reader.ReadSingle();
			if (IsReadFlareFadeSpeed(reader.Version))
			{
				FlareFadeSpeed = reader.ReadSingle();
			}
			if (IsReadPixelLightCount(reader.Version))
			{
				PixelLightCount = reader.ReadInt32();
			}
			HaloTexture.Read(reader);
			if (IsReadAmbientLightScale(reader.Version))
			{
				AmbientLightScale = reader.ReadSingle();
			}
			if (IsReadSpecularTex(reader.Version))
			{
				SpecularTexture.Read(reader);
			}
			SpotCookie.Read(reader);
			if (IsReadDefaultReflectionMode(reader.Version))
			{
				DefaultReflectionMode = reader.ReadInt32();
			}
			if (IsReadDefaultReflectionResolution(reader.Version))
			{
				DefaultReflectionResolution = reader.ReadInt32();
				ReflectionBounces = reader.ReadInt32();
				ReflectionIntensity = reader.ReadSingle();
			}
			if (IsReadCustomReflection(reader.Version))
			{
				CustomReflection.Read(reader);
			}
			if (IsReadAmbientProbe(reader.Version, reader.Flags))
			{
				if (!IsReadAmbientProbeFirst(reader.Version))
				{
					AmbientProbe.Read(reader);
				}
			}
			if (IsReadAmbientProbeInGamma(reader.Version, reader.Flags))
			{
				AmbientProbeInGamma.Read(reader);
			}
			if (IsReadGeneratedSkyboxReflection(reader.Version, reader.Flags))
			{
				GeneratedSkyboxReflection.Read(reader);
			}
			if (IsReadSun(reader.Version))
			{
				Sun.Read(reader);
			}
			if (IsReadIndirectSpecularColor(reader.Version))
			{
				IndirectSpecularColor.Read(reader);
			}
			if(IsReadUseRadianceAmbientProbe(reader.Version))
			{
				UseRadianceAmbientProbe = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(SkyboxMaterial, SkyboxMaterialName);
			yield return context.FetchDependency(HaloTexture, HaloTextureName);
			yield return context.FetchDependency(SpotCookie, SpotCookieName);
			yield return context.FetchDependency(CustomReflection, CustomReflectionName);
			if (IsReadGeneratedSkyboxReflection(context.Version, context.Flags))
			{
				yield return context.FetchDependency(GeneratedSkyboxReflection, GeneratedSkyboxReflectionName);
			}
			yield return context.FetchDependency(Sun, SunName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(FogName, Fog);
			node.Add(FogColorName, FogColor.ExportYAML(container));
			node.Add(FogModeName, (int)GetExportFogMode(container.Version));
			node.Add(FogDensityName, FogDensity);
			node.Add(LinearFogStartName, LinearFogStart);
			node.Add(LinearFogEndName, GetExportLinearFogEnd(container.Version));
			node.Add(AmbientSkyColorName, AmbientSkyColor.ExportYAML(container));
			node.Add(AmbientEquatorColorName, GetExportAmbientEquatorColor(container.Version).ExportYAML(container));
			node.Add(AmbientGroundColorName, GetExportAmbientGroundColor(container.Version).ExportYAML(container));
			node.Add(AmbientIntensityName, GetExportAmbientIntensity(container.Version));
			node.Add(AmbientModeName, (int)AmbientMode);
			node.Add(SubtractiveShadowColorName, GetExportSubtractiveShadowColor(container.Version).ExportYAML(container));
			node.Add(SkyboxMaterialName, SkyboxMaterial.ExportYAML(container));
			node.Add(HaloStrengthName, HaloStrength);
			node.Add(FlareStrengthName, FlareStrength);
			node.Add(FlareFadeSpeedName, GetExportFlareFadeSpeed(container.Version));
			node.Add(HaloTextureName, HaloTexture.ExportYAML(container));
			node.Add(SpotCookieName, SpotCookie.ExportYAML(container));
			node.Add(DefaultReflectionModeName, DefaultReflectionMode);
			node.Add(DefaultReflectionResolutionName, GetExportDefaultReflectionResolution(container.Version));
			node.Add(ReflectionBouncesName, GetExportReflectionBounces(container.Version));
			node.Add(ReflectionIntensityName, GetExportReflectionIntensity(container.Version));
			node.Add(CustomReflectionName, CustomReflection.ExportYAML(container));
			node.Add(SunName, Sun.ExportYAML(container));
			node.Add(IndirectSpecularColorName, GetExportIndirectSpecularColor(container.Version).ExportYAML(container));
			return node;
		}

		private FogMode GetExportFogMode(Version version)
		{
			return IsReadFogMode(version) ? FogMode : FogMode.ExponentialSquared;
		}
		private float GetExportLinearFogEnd(Version version)
		{
			return IsReadLinearFogStart(version) ? LinearFogEnd : 300.0f;
		}
		private ColorRGBAf GetExportAmbientEquatorColor(Version version)
		{
			return IsReadAmbientEquatorColor(version) ? AmbientEquatorColor : new ColorRGBAf(0.114f, 0.125f, 0.133f, 1.0f);
		}
		private ColorRGBAf GetExportAmbientGroundColor(Version version)
		{
			return IsReadAmbientEquatorColor(version) ? AmbientGroundColor : new ColorRGBAf(0.047f, 0.043f, 0.035f, 1.0f);
		}
		private float GetExportAmbientIntensity(Version version)
		{
			return IsReadAmbientEquatorColor(version) ? AmbientIntensity : 1.0f;
		}
		private ColorRGBAf GetExportSubtractiveShadowColor(Version version)
		{
			return IsReadSubtractiveShadowColor(version) ? SubtractiveShadowColor : new ColorRGBAf(0.42f, 0.478f, 0.627f, 1.0f);
		}
		private float GetExportFlareFadeSpeed(Version version)
		{
			return IsReadFlareFadeSpeed(version) ? FlareFadeSpeed : 3.0f;
		}
		private int GetExportDefaultReflectionResolution(Version version)
		{
			return IsReadDefaultReflectionResolution(version) ? DefaultReflectionResolution : 128;
		}
		private int GetExportReflectionBounces(Version version)
		{
			return IsReadDefaultReflectionResolution(version) ? ReflectionBounces : 1;
		}
		private float GetExportReflectionIntensity(Version version)
		{
			return IsReadDefaultReflectionResolution(version) ? ReflectionIntensity : 1.0f;
		}
		private ColorRGBAf GetExportIndirectSpecularColor(Version version)
		{
			return IsReadIndirectSpecularColor(version) ? IndirectSpecularColor : new ColorRGBAf(0.44657898f, 0.4964133f, 0.5748178f, 1.0f);
		}

		public bool Fog { get; private set; }
		public FogMode FogMode { get; private set; }
		public float FogDensity { get; private set; }
		public float LinearFogStart { get; private set; }
		public float LinearFogEnd { get; private set; }
		/// <summary>
		/// AmbientSkyboxExposure previously
		/// </summary>
		public float AmbientIntensity { get; private set; }
		public AmbientMode AmbientMode { get; private set; }
		public bool CreateAmbientLight { get; private set; }
		public float HaloStrength { get; private set; }
		public float FlareStrength { get; private set; }
		public int PixelLightCount { get; private set; }
		public float FlareFadeSpeed { get; private set; }
		public int DefaultReflectionMode { get; private set; }
		public int DefaultReflectionResolution { get; private set; }
		public int ReflectionBounces { get; private set; }
		public float ReflectionIntensity { get; private set; }
		public float AmbientLightScale { get; private set; }
		public bool UseRadianceAmbientProbe { get; private set; }

		public const string FogName = "m_Fog";
		public const string FogColorName = "m_FogColor";
		public const string FogModeName = "m_FogMode";
		public const string FogDensityName = "m_FogDensity";
		public const string LinearFogStartName = "m_LinearFogStart";
		public const string LinearFogEndName = "m_LinearFogEnd";
		public const string AmbientSkyColorName = "m_AmbientSkyColor";
		public const string AmbientEquatorColorName = "m_AmbientEquatorColor";
		public const string AmbientGroundColorName = "m_AmbientGroundColor";
		public const string AmbientIntensityName = "m_AmbientIntensity";
		public const string AmbientModeName = "m_AmbientMode";
		public const string SubtractiveShadowColorName = "m_SubtractiveShadowColor";
		public const string SkyboxMaterialName = "m_SkyboxMaterial";
		public const string HaloStrengthName = "m_HaloStrength";
		public const string FlareStrengthName = "m_FlareStrength";
		public const string FlareFadeSpeedName = "m_FlareFadeSpeed";
		public const string HaloTextureName = "m_HaloTexture";
		public const string SpotCookieName = "m_SpotCookie";
		public const string DefaultReflectionModeName = "m_DefaultReflectionMode";
		public const string DefaultReflectionResolutionName = "m_DefaultReflectionResolution";
		public const string ReflectionBouncesName = "m_ReflectionBounces";
		public const string ReflectionIntensityName = "m_ReflectionIntensity";
		public const string CustomReflectionName = "m_CustomReflection";
		public const string GeneratedSkyboxReflectionName = "m_GeneratedSkyboxReflection";
		public const string SunName = "m_Sun";
		public const string IndirectSpecularColorName = "m_IndirectSpecularColor";

		public ColorRGBAf FogColor;
		/// <summary>
		/// Ambient/AmbientLight previously
		/// </summary>
		public ColorRGBAf AmbientSkyColor;
		public ColorRGBAf AmbientEquatorColor;
		public ColorRGBAf AmbientGroundColor;
		public PPtr<Light> AmbientSkyboxLight;
		public ColorRGBAf SubtractiveShadowColor;
		public PPtr<Material> SkyboxMaterial;
		public PPtr<Texture2D> HaloTexture;
		/// <summary>
		/// SpecularTex previously
		/// </summary>
		public PPtr<Cubemap> SpecularTexture;
		public PPtr<Texture2D> SpotCookie;
		public PPtr<Cubemap> CustomReflection;
		public SphericalHarmonicsL2 AmbientProbe;
		public SphericalHarmonicsL2 AmbientProbeInGamma;
		public PPtr<Cubemap> GeneratedSkyboxReflection;
		public PPtr<Light> Sun;
		public ColorRGBAf IndirectSpecularColor;
	}
}
