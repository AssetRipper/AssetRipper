using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.RenderSettings
{
	/// <summary>
	/// LightManager previously
	/// </summary>
	public sealed class RenderSettings : LevelGameManager
	{
		public RenderSettings(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			/*if (version.IsGreaterEqual(2018))
			{
				return 9;
			}*/
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2))
			{
				return 8;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 7;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final))
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

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool HasFogMode(UnityVersion version) => version.IsGreaterEqual(3, 2);
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool HasLinearFogStart(UnityVersion version) => version.IsGreaterEqual(3, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAmbientEquatorColorRGBAf(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasAmbientSkyboxLight(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAmbientMode(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasCreateAmbientLight(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasSubtractiveShadowColorRGBAf(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasFlareFadeSpeed(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public static bool HasPixelLightCount(UnityVersion version) => version.IsLess(1, 5);
		/// <summary>
		/// Less than 1.5.0
		/// </summary>
		public static bool HasAmbientLightScale(UnityVersion version) => version.IsLess(1, 5);
		/// <summary>
		/// Less than 1.6.0
		/// </summary>
		public static bool HasSpecularTex(UnityVersion version) => version.IsLess(1, 6);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasDefaultReflectionMode(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasDefaultReflectionResolution(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCustomReflection(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 and Release
		/// </summary>
		public static bool HasAmbientProbe(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(5) && flags.IsRelease();
		/// <summary>
		/// 5.0.0f1 to 5.3.0 exclusive and Release
		/// </summary>
		public static bool HasAmbientProbeInGamma(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final) && version.IsLess(5, 3) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and (Release or ResourcesFile)
		/// </summary>
		public static bool HasGeneratedSkyboxReflection(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && (flags.IsRelease() || flags.IsBuiltinResources());
		}
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasSun(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasIndirectSpecularColorRGBAf(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasUseRadianceAmbientProbe(UnityVersion version) => version.IsGreaterEqual(2018);

		/// <summary>
		/// Less than 5.0.0f1 (NOTE: unknown version)
		/// </summary>
		private static bool HasAmbientProbeFirst(UnityVersion version) => version.IsLess(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Fog = reader.ReadBoolean();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			FogColor.Read(reader);
			if (HasFogMode(reader.Version))
			{
				FogMode = (FogMode)reader.ReadInt32();
			}
			FogDensity = reader.ReadSingle();
			if (HasLinearFogStart(reader.Version))
			{
				LinearFogStart = reader.ReadSingle();
				LinearFogEnd = reader.ReadSingle();
			}
			AmbientSkyColor.Read(reader);
			if (HasAmbientEquatorColorRGBAf(reader.Version))
			{
				AmbientEquatorColor.Read(reader);
				AmbientGroundColor.Read(reader);
				AmbientIntensity = reader.ReadSingle();
			}
			if (HasAmbientProbe(reader.Version, reader.Flags))
			{
				if (HasAmbientProbeFirst(reader.Version))
				{
					AmbientProbe.Read(reader);
				}
			}
			if (HasAmbientSkyboxLight(reader.Version))
			{
				AmbientSkyboxLight.Read(reader);
			}
			if (HasAmbientMode(reader.Version))
			{
				AmbientMode = (AmbientMode)reader.ReadInt32();
				if (HasCreateAmbientLight(reader.Version))
				{
					CreateAmbientLight = reader.ReadBoolean();
				}
				reader.AlignStream();
			}
			if (HasSubtractiveShadowColorRGBAf(reader.Version))
			{
				SubtractiveShadowColor.Read(reader);
			}

			SkyboxMaterial.Read(reader);
			HaloStrength = reader.ReadSingle();
			FlareStrength = reader.ReadSingle();
			if (HasFlareFadeSpeed(reader.Version))
			{
				FlareFadeSpeed = reader.ReadSingle();
			}
			if (HasPixelLightCount(reader.Version))
			{
				PixelLightCount = reader.ReadInt32();
			}
			HaloTexture.Read(reader);
			if (HasAmbientLightScale(reader.Version))
			{
				AmbientLightScale = reader.ReadSingle();
			}
			if (HasSpecularTex(reader.Version))
			{
				SpecularTexture.Read(reader);
			}
			SpotCookie.Read(reader);
			if (HasDefaultReflectionMode(reader.Version))
			{
				DefaultReflectionMode = reader.ReadInt32();
			}
			if (HasDefaultReflectionResolution(reader.Version))
			{
				DefaultReflectionResolution = reader.ReadInt32();
				ReflectionBounces = reader.ReadInt32();
				ReflectionIntensity = reader.ReadSingle();
			}
			if (HasCustomReflection(reader.Version))
			{
				CustomReflection.Read(reader);
			}
			if (HasAmbientProbe(reader.Version, reader.Flags))
			{
				if (!HasAmbientProbeFirst(reader.Version))
				{
					AmbientProbe.Read(reader);
				}
			}
			if (HasAmbientProbeInGamma(reader.Version, reader.Flags))
			{
				AmbientProbeInGamma.Read(reader);
			}
			if (HasGeneratedSkyboxReflection(reader.Version, reader.Flags))
			{
				GeneratedSkyboxReflection.Read(reader);
			}
			if (HasSun(reader.Version))
			{
				Sun.Read(reader);
			}
			if (HasIndirectSpecularColorRGBAf(reader.Version))
			{
				IndirectSpecularColor.Read(reader);
			}
			if (HasUseRadianceAmbientProbe(reader.Version))
			{
				UseRadianceAmbientProbe = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(SkyboxMaterial, SkyboxMaterialName);
			yield return context.FetchDependency(HaloTexture, HaloTextureName);
			yield return context.FetchDependency(SpotCookie, SpotCookieName);
			yield return context.FetchDependency(CustomReflection, CustomReflectionName);
			if (HasGeneratedSkyboxReflection(context.Version, context.Flags))
			{
				yield return context.FetchDependency(GeneratedSkyboxReflection, GeneratedSkyboxReflectionName);
			}
			yield return context.FetchDependency(Sun, SunName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.Version));
			node.Add(FogName, Fog);
			node.Add(FogColorName, FogColor.ExportYAML(container));
			node.Add(FogModeName, (int)GetExportFogMode(container.Version));
			node.Add(FogDensityName, FogDensity);
			node.Add(LinearFogStartName, LinearFogStart);
			node.Add(LinearFogEndName, GetExportLinearFogEnd(container.Version));
			node.Add(AmbientSkyColorName, AmbientSkyColor.ExportYAML(container));
			node.Add(AmbientEquatorColorName, GetExportAmbientEquatorColorRGBAf(container.Version).ExportYAML(container));
			node.Add(AmbientGroundColorName, GetExportAmbientGroundColorRGBAf(container.Version).ExportYAML(container));
			node.Add(AmbientIntensityName, GetExportAmbientIntensity(container.Version));
			node.Add(AmbientModeName, (int)AmbientMode);
			node.Add(SubtractiveShadowColorName, GetExportSubtractiveShadowColorRGBAf(container.Version).ExportYAML(container));
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
			node.Add(IndirectSpecularColorName, GetExportIndirectSpecularColorRGBAf(container.Version).ExportYAML(container));
			return node;
		}

		private FogMode GetExportFogMode(UnityVersion version)
		{
			return HasFogMode(version) ? FogMode : FogMode.ExponentialSquared;
		}
		private float GetExportLinearFogEnd(UnityVersion version)
		{
			return HasLinearFogStart(version) ? LinearFogEnd : 300.0f;
		}
		private ColorRGBAf GetExportAmbientEquatorColorRGBAf(UnityVersion version)
		{
			return HasAmbientEquatorColorRGBAf(version) ? AmbientEquatorColor : new ColorRGBAf(0.114f, 0.125f, 0.133f, 1.0f);
		}
		private ColorRGBAf GetExportAmbientGroundColorRGBAf(UnityVersion version)
		{
			return HasAmbientEquatorColorRGBAf(version) ? AmbientGroundColor : new ColorRGBAf(0.047f, 0.043f, 0.035f, 1.0f);
		}
		private float GetExportAmbientIntensity(UnityVersion version)
		{
			return HasAmbientEquatorColorRGBAf(version) ? AmbientIntensity : 1.0f;
		}
		private ColorRGBAf GetExportSubtractiveShadowColorRGBAf(UnityVersion version)
		{
			return HasSubtractiveShadowColorRGBAf(version) ? SubtractiveShadowColor : new ColorRGBAf(0.42f, 0.478f, 0.627f, 1.0f);
		}
		private float GetExportFlareFadeSpeed(UnityVersion version)
		{
			return HasFlareFadeSpeed(version) ? FlareFadeSpeed : 3.0f;
		}
		private int GetExportDefaultReflectionResolution(UnityVersion version)
		{
			return HasDefaultReflectionResolution(version) ? DefaultReflectionResolution : 128;
		}
		private int GetExportReflectionBounces(UnityVersion version)
		{
			return HasDefaultReflectionResolution(version) ? ReflectionBounces : 1;
		}
		private float GetExportReflectionIntensity(UnityVersion version)
		{
			return HasDefaultReflectionResolution(version) ? ReflectionIntensity : 1.0f;
		}
		private ColorRGBAf GetExportIndirectSpecularColorRGBAf(UnityVersion version)
		{
			return HasIndirectSpecularColorRGBAf(version) ? IndirectSpecularColor : new ColorRGBAf(0.44657898f, 0.4964133f, 0.5748178f, 1.0f);
		}

		public bool Fog { get; set; }
		public FogMode FogMode { get; set; }
		public float FogDensity { get; set; }
		public float LinearFogStart { get; set; }
		public float LinearFogEnd { get; set; }
		/// <summary>
		/// AmbientSkyboxExposure previously
		/// </summary>
		public float AmbientIntensity { get; set; }
		public AmbientMode AmbientMode { get; set; }
		public bool CreateAmbientLight { get; set; }
		public float HaloStrength { get; set; }
		public float FlareStrength { get; set; }
		public int PixelLightCount { get; set; }
		public float FlareFadeSpeed { get; set; }
		public int DefaultReflectionMode { get; set; }
		public int DefaultReflectionResolution { get; set; }
		public int ReflectionBounces { get; set; }
		public float ReflectionIntensity { get; set; }
		public float AmbientLightScale { get; set; }
		public bool UseRadianceAmbientProbe { get; set; }

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

		public ColorRGBAf FogColor = new();
		/// <summary>
		/// Ambient/AmbientLight previously
		/// </summary>
		public ColorRGBAf AmbientSkyColor = new();
		public ColorRGBAf AmbientEquatorColor = new();
		public ColorRGBAf AmbientGroundColor = new();
		public PPtr<Light.Light> AmbientSkyboxLight = new();
		public ColorRGBAf SubtractiveShadowColor = new();
		public PPtr<Material.Material> SkyboxMaterial = new();
		public PPtr<Texture2D.Texture2D> HaloTexture = new();
		/// <summary>
		/// SpecularTex previously
		/// </summary>
		public PPtr<Cubemap> SpecularTexture = new();
		public PPtr<Texture2D.Texture2D> SpotCookie = new();
		public PPtr<Cubemap> CustomReflection = new();
		public SphericalHarmonicsL2 AmbientProbe = new();
		public SphericalHarmonicsL2 AmbientProbeInGamma = new();
		public PPtr<Cubemap> GeneratedSkyboxReflection = new();
		public PPtr<Light.Light> Sun = new();
		public ColorRGBAf IndirectSpecularColor = new();
	}
}
