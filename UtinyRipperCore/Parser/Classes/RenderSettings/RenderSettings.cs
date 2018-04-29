using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.RenderSettingss;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		public static bool IsReadAmbientSkyColor(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAmbientIntensity(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool IsReadAmbientSkyboxLight(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAmbientMode(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool IsReadCreateAmbientLight(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
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
#warning unknown
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
			return version.IsGreaterEqual(5) && flags.IsSerializeGameRelease();
		}
		/// <summary>
		/// 5.0.0f1 to 5.3.0 exclusive and Release
		/// </summary>
		public static bool IsReadAmbientProbeInGamma(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final) && version.IsLess(5, 3) && flags.IsSerializeGameRelease();
		}
		/// <summary>
		/// 5.0.0 and (Release or ResourcesFile)
		/// </summary>
		public static bool IsReadGeneratedSkyboxReflection(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) &&  (flags.IsSerializeGameRelease() || flags.IsBuiltinResourcesFile());
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadSun(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadIndirectSpecularColor(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		/// <summary>
		/// Less than 5.0.0b2
		/// </summary>
		private static bool IsReadAmbientProbeFirst(Version version)
		{
#warning unknown
			return version.IsLess(5, 0, 0, VersionType.Beta, 2);
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 8;
			}
			
			if (version.IsGreaterEqual(5, 6))
			{
				return 8;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 7;
			}
#warning unknown
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 6;
			}
			/* unknown
			if (version.IsGreaterEqual())
			{
				return 5;
			}
			if (version.IsGreaterEqual())
			{
				return 4;
			}
			if (version.IsGreaterEqual())
			{
				return 3;
			}*/
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Fog = stream.ReadBoolean();
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			FogColor.Read(stream);
			if (IsReadFogMode(stream.Version))
			{
				FogMode = stream.ReadInt32();
			}
			FogDensity = stream.ReadSingle();
			if (IsReadLinearFogStart(stream.Version))
			{
				LinearFogStart = stream.ReadSingle();
				LinearFogEnd = stream.ReadSingle();
			}
			if (IsReadAmbientSkyColor(stream.Version))
			{
				AmbientSkyColor.Read(stream);
				AmbientEquatorColor.Read(stream);
			}
			AmbientGroundColor.Read(stream);
			if (IsReadAmbientIntensity(stream.Version))
			{
				AmbientIntensity = stream.ReadSingle();
			}
			if (IsReadAmbientProbe(stream.Version, stream.Flags))
			{
				if (IsReadAmbientProbeFirst(stream.Version))
				{
					AmbientProbe.Read(stream);
				}
			}
			if (IsReadAmbientSkyboxLight(stream.Version))
			{
				AmbientSkyboxLight.Read(stream);
			}
			if (IsReadAmbientMode(stream.Version))
			{
				AmbientMode = stream.ReadInt32();
				if (IsReadCreateAmbientLight(stream.Version))
				{
					CreateAmbientLight = stream.ReadBoolean();
				}
				stream.AlignStream(AlignType.Align4);
			}
			if (IsReadSubtractiveShadowColor(stream.Version))
			{
				SubtractiveShadowColor.Read(stream);
			}
			
			SkyboxMaterial.Read(stream);
			HaloStrength = stream.ReadSingle();
			FlareStrength = stream.ReadSingle();
			if (IsReadFlareFadeSpeed(stream.Version))
			{
				FlareFadeSpeed = stream.ReadSingle();
			}
			if (IsReadPixelLightCount(stream.Version))
			{
				PixelLightCount = stream.ReadInt32();
			}
			HaloTexture.Read(stream);
			if (IsReadAmbientLightScale(stream.Version))
			{
				AmbientLightScale = stream.ReadSingle();
			}
			if (IsReadSpecularTex(stream.Version))
			{
				SpecularTexture.Read(stream);
			}
			SpotCookie.Read(stream);
			if (IsReadDefaultReflectionMode(stream.Version))
			{
				DefaultReflectionMode = stream.ReadInt32();
			}
			if (IsReadDefaultReflectionResolution(stream.Version))
			{
				DefaultReflectionResolution = stream.ReadInt32();
				ReflectionBounces = stream.ReadInt32();
				ReflectionIntensity = stream.ReadSingle();
			}
			if (IsReadCustomReflection(stream.Version))
			{
				CustomReflection.Read(stream);
			}
			if (IsReadAmbientProbe(stream.Version, stream.Flags))
			{
				if (!IsReadAmbientProbeFirst(stream.Version))
				{
					AmbientProbe.Read(stream);
				}
			}
			if (IsReadAmbientProbeInGamma(stream.Version, stream.Flags))
			{
				AmbientProbeInGamma.Read(stream);
			}
			if (IsReadGeneratedSkyboxReflection(stream.Version, stream.Flags))
			{
				GeneratedSkyboxReflection.Read(stream);
			}
			if (IsReadSun(stream.Version))
			{
				Sun.Read(stream);
			}
			if (IsReadIndirectSpecularColor(stream.Version))
			{
				IndirectSpecularColor.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return SkyboxMaterial.FetchDependency(file, isLog, ToLogString, "m_SkyboxMaterial");
			yield return HaloTexture.FetchDependency(file, isLog, ToLogString, "m_HaloTexture");
			yield return SpotCookie.FetchDependency(file, isLog, ToLogString, "m_SpotCookie");
			yield return CustomReflection.FetchDependency(file, isLog, ToLogString, "m_CustomReflection");
			if (IsReadGeneratedSkyboxReflection(file.Version, file.Flags))
			{
				yield return GeneratedSkyboxReflection.FetchDependency(file, isLog, ToLogString, "m_GeneratedSkyboxReflection");
			}
			yield return Sun.FetchDependency(file, isLog, ToLogString, "m_Sun");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Fog", Fog);
			node.Add("m_FogColor", FogColor.ExportYAML(exporter));
			node.Add("m_FogMode", FogMode);
			node.Add("m_FogDensity", FogDensity);
			node.Add("m_LinearFogStart", LinearFogStart);
			node.Add("m_LinearFogEnd", LinearFogEnd);
			node.Add("m_AmbientSkyColor", AmbientSkyColor.ExportYAML(exporter));
			node.Add("m_AmbientEquatorColor", AmbientEquatorColor.ExportYAML(exporter));
			node.Add("m_AmbientGroundColor", AmbientGroundColor.ExportYAML(exporter));
			node.Add("m_AmbientIntensity", AmbientIntensity);
			node.Add("m_AmbientMode", AmbientMode);
			node.Add("m_SubtractiveShadowColor", SubtractiveShadowColor.ExportYAML(exporter));
			node.Add("m_SkyboxMaterial", SkyboxMaterial.ExportYAML(exporter));
			node.Add("m_HaloStrength", HaloStrength);
			node.Add("m_FlareStrength", FlareStrength);
			node.Add("m_FlareFadeSpeed", FlareFadeSpeed);
			node.Add("m_HaloTexture", HaloTexture.ExportYAML(exporter));
			node.Add("m_SpotCookie", SpotCookie.ExportYAML(exporter));
			node.Add("m_DefaultReflectionMode", DefaultReflectionMode);
			node.Add("m_DefaultReflectionResolution", DefaultReflectionResolution);
			node.Add("m_ReflectionBounces", ReflectionBounces);
			node.Add("m_ReflectionIntensity", ReflectionIntensity);
			node.Add("m_CustomReflection", CustomReflection.ExportYAML(exporter));
			node.Add("m_AmbientProbe", AmbientProbe.ExportYAML(exporter));
			node.Add("m_GeneratedSkyboxReflection", GeneratedSkyboxReflection.ExportYAML(exporter));
			node.Add("m_Sun", Sun.ExportYAML(exporter));
			node.Add("m_IndirectSpecularColor", IndirectSpecularColor.ExportYAML(exporter));
			return node;
		}

		public bool Fog { get; private set; }
		public int FogMode { get; private set; }
		public float FogDensity { get; private set; }
		public float LinearFogStart { get; private set; }
		public float LinearFogEnd { get; private set; }
		/// <summary>
		/// AmbientSkyboxExposure previously
		/// </summary>
		public float AmbientIntensity { get; private set; }
		public int AmbientMode { get; private set; }
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

		public ColorRGBAf FogColor;
		public ColorRGBAf AmbientSkyColor;
		public ColorRGBAf AmbientEquatorColor;
		/// <summary>
		/// Ambient/AmbientLight previously
		/// </summary>
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
