using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.QualitySettingss;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class QualitySettings : GlobalGameManager
	{
		public QualitySettings(AssetInfo assetInfo):
			base(assetInfo)
		{

		}

		public QualitySettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new QualitySettings(assetInfo));
		}

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadQualitySettings(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}

		/// <summary>
		/// 1.6.0 to 3.5.0 exclusive
		/// </summary>
		public static bool IsReadDefaultStandaloneQuality(Version version)
		{
			return version.IsLess(3, 5) && version.IsGreaterEqual(1, 6);
		}
		/// <summary>
		/// 3.2.0 to 3.5.0 exclusive
		/// </summary>
		public static bool IsReadDefaultMobileQuality(Version version)
		{
			return version.IsLess(3, 5) && version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadQualitySettingArray(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadWebPlayer(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadPerPlatformDefaultQuality(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool IsReadStrippedMaximumLODLevel(Version version, TransferInstructionFlags flags)
		{
			return flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			// static PlatformDefaultQuality has been replaced by dictionary
			if (Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(3, 5))
			{
				return 5;
			}
			// static QualitySetting has been replaced by array
			/*if (version.IsGreaterEqual(3, 5, 0, some alpha or beta))
			{
				return 4;
			}*/
			// some QualitySetting's default inner values has been changed from 0 to another value
			if (version.IsGreaterEqual(3, 4))
			{
				return 3;
			}
			// unknown
			if (version.IsGreaterEqual(1, 6))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadDefaultStandaloneQuality(reader.Version))
			{
				QualityLevel defaultStandaloneQuality = (QualityLevel)reader.ReadInt32();
				QualityLevel defaultWebPlayerQuality = (QualityLevel)reader.ReadInt32();
				m_perPlatformDefaultQuality = new Dictionary<string, int>();
				SetDefaultPlatformQuality(m_perPlatformDefaultQuality);
				m_perPlatformDefaultQuality[BuildTargetGroup.Standalone.ToExportString()] = (int)defaultStandaloneQuality;
				m_perPlatformDefaultQuality[BuildTargetGroup.WebPlayer.ToExportString()] = (int)defaultStandaloneQuality;
			}
			if (IsReadDefaultMobileQuality(reader.Version))
			{
				QualityLevel defaultMobileQuality = (QualityLevel)reader.ReadInt32();
				m_perPlatformDefaultQuality[BuildTargetGroup.Android.ToExportString()] = (int)defaultMobileQuality;
				m_perPlatformDefaultQuality[BuildTargetGroup.iOS.ToExportString()] = (int)defaultMobileQuality;
			}
			CurrentQuality = reader.ReadInt32();
			if (IsReadQualitySettingArray(reader.Version))
			{
				m_qualitySettings = reader.ReadAssetArray<QualitySetting>();
			}
			else
			{
				m_qualitySettings = new QualitySetting[6];
				QualitySetting fastest = reader.ReadAsset<QualitySetting>();
				fastest.Name = nameof(QualityLevel.Fastest);
				m_qualitySettings[(int)QualityLevel.Fastest] = fastest;

				QualitySetting fast = reader.ReadAsset<QualitySetting>();
				fast.Name = nameof(QualityLevel.Fast);
				m_qualitySettings[(int)QualityLevel.Fast] = fast;

				QualitySetting simple = reader.ReadAsset<QualitySetting>();
				simple.Name = nameof(QualityLevel.Simple);
				m_qualitySettings[(int)QualityLevel.Simple] = simple;

				QualitySetting good = reader.ReadAsset<QualitySetting>();
				good.Name = nameof(QualityLevel.Good);
				m_qualitySettings[(int)QualityLevel.Good] = good;

				QualitySetting beautiful = reader.ReadAsset<QualitySetting>();
				beautiful.Name = nameof(QualityLevel.Beautiful);
				m_qualitySettings[(int)QualityLevel.Beautiful] = beautiful;

				QualitySetting fantastic = reader.ReadAsset<QualitySetting>();
				fantastic.Name = nameof(QualityLevel.Fantastic);
				m_qualitySettings[(int)QualityLevel.Fantastic] = fantastic;
			}
			foreach (QualitySetting setting in m_qualitySettings)
			{
				switch (setting.Name)
				{
					case nameof(QualityLevel.Fastest):
					case "Very Low":
						QualitySetting fastest = CreateFastestSettings();
						setting.Merge(fastest, reader.Version, reader.Flags);
						break;

					case nameof(QualityLevel.Fast):
					case "Low":
						QualitySetting fast = CreateFastSettings();
						setting.Merge(fast, reader.Version, reader.Flags);
						break;

					case nameof(QualityLevel.Simple):
					case "Medium":
						QualitySetting simple = CreateSimpleSettings();
						setting.Merge(simple, reader.Version, reader.Flags);
						break;

					case nameof(QualityLevel.Good):
					case "High":
						QualitySetting good = CreateGoodSettings();
						setting.Merge(good, reader.Version, reader.Flags);
						break;

					case nameof(QualityLevel.Beautiful):
					case "Very High":
						QualitySetting beautiful = CreateBeautifulSettings();
						setting.Merge(beautiful, reader.Version, reader.Flags);
						break;

					case nameof(QualityLevel.Fantastic):
					case "Ultra":
					default:
						QualitySetting fantastic = CreateFantasticSettings();
						setting.Merge(fantastic, reader.Version, reader.Flags);
						break;
				}
			}

			if (IsReadWebPlayer(reader.Version))
			{
				QualitySetting webPlayer = reader.ReadAsset<QualitySetting>();
				webPlayer.Name = "WebPlayer";
			}

#if UNIVERSAL
			if (IsReadPerPlatformDefaultQuality(reader.Version, reader.Flags))
			{
				m_perPlatformDefaultQuality = new Dictionary<string, int>();
				m_perPlatformDefaultQuality.Read(reader);
			}
#endif
			if (IsReadStrippedMaximumLODLevel(reader.Version, reader.Flags))
			{
				StrippedMaximumLODLevel = reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(CurrentQualityName, CurrentQuality);
			node.Add(QualitySettingsName, QualitySettingss.ExportYAML(container));
			node.Add(PerPlatformDefaultQualityName, GetPerPlatformDefaultQuality(container.Version, container.Flags).ExportYAML());
			return node;
		}

		private IReadOnlyDictionary<string, int> GetPerPlatformDefaultQuality(Version version, TransferInstructionFlags flags)
		{
			if (IsReadDefaultStandaloneQuality(version))
			{
				return PerPlatformDefaultQuality;
			}
#if UNIVERSAL
			if (IsReadPerPlatformDefaultQuality(version, flags))
			{
				return PerPlatformDefaultQuality;
			}
#endif
			Dictionary<string, int> quality = new Dictionary<string, int>();
			SetDefaultPlatformQuality(quality);
			return quality;
		}

		private void SetDefaultPlatformQuality(Dictionary<string, int> perPlatformDefaultQuality)
		{
			perPlatformDefaultQuality.Add(BuildTargetGroup.Android.ToExportString(), (int)QualityLevel.Simple);
			perPlatformDefaultQuality.Add(BuildTargetGroup.N3DS.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.Switch.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.PS4.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.PSM.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.PSP2.ToExportString(), (int)QualityLevel.Simple);
			perPlatformDefaultQuality.Add(BuildTargetGroup.Standalone.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.Tizen.ToExportString(), (int)QualityLevel.Simple);
			perPlatformDefaultQuality.Add(BuildTargetGroup.WebGL.ToExportString(), (int)QualityLevel.Good);
			perPlatformDefaultQuality.Add(BuildTargetGroup.WiiU.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.Metro.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.XboxOne.ToExportString(), (int)QualityLevel.Fantastic);
			perPlatformDefaultQuality.Add(BuildTargetGroup.iOS.ToExportString(), (int)QualityLevel.Simple);
			perPlatformDefaultQuality.Add(BuildTargetGroup.tvOS.ToExportString(), (int)QualityLevel.Simple);
		}

		private QualitySetting CreateFastestSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "Very Low";
			setting.ShadowCascades = ShadowCascades.NoCascades;
			setting.ShadowDistance = 15;
			setting.SkinWeights = SkinWeights.OneBone;
			setting.TextureQuality = TextureQuality.HalfRes;
			setting.LodBias = 0.3f;
			setting.ParticleRaycastBudget = 4;
			return setting;
		}

		private QualitySetting CreateFastSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "Low";
			setting.ShadowCascades = ShadowCascades.NoCascades;
			setting.ShadowDistance = 20;
			setting.SkinWeights = SkinWeights.TwoBones;
			setting.LodBias = 0.4f;
			setting.ParticleRaycastBudget = 16;
			return setting;
		}

		private QualitySetting CreateSimpleSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "Medium";
			setting.PixelLightCount = 1;
			setting.Shadows = ShadowQuality.HardOnly;
			setting.ShadowCascades = ShadowCascades.NoCascades;
			setting.ShadowDistance = 20;
			setting.SkinWeights = SkinWeights.TwoBones;
			setting.AnisotropicTextures = AnisotropicFiltering.Enable;
			setting.VSyncCount = VSyncCount.EveryVBlank;
			setting.LodBias = 0.7f;
			setting.ParticleRaycastBudget = 64;
			return setting;
		}

		private QualitySetting CreateGoodSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "High";
			setting.PixelLightCount = 2;
			setting.Shadows = ShadowQuality.All;
			setting.ShadowResolution = ShadowResolution.Medium;
			setting.ShadowCascades = ShadowCascades.TwoCascades;
			setting.ShadowDistance = 40;
			setting.SkinWeights = SkinWeights.TwoBones;
			setting.AnisotropicTextures = AnisotropicFiltering.Enable;
			setting.AntiAliasing = AntiAliasing._2X;
			setting.SoftVegetation = true;
			setting.VSyncCount = VSyncCount.EveryVBlank;
			setting.LodBias = 1.0f;
			setting.ParticleRaycastBudget = 256;
			return setting;
		}

		private QualitySetting CreateBeautifulSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "Very High";
			setting.PixelLightCount = 3;
			setting.Shadows = ShadowQuality.All;
			setting.ShadowResolution = ShadowResolution.High;
			setting.ShadowCascades = ShadowCascades.TwoCascades;
			setting.ShadowDistance = 40;
			setting.ShadowmaskMode = ShadowmaskMode.DistanceShadowmask;
			setting.SkinWeights = SkinWeights.FourBones;
			setting.AnisotropicTextures = AnisotropicFiltering.Enable;
			setting.AntiAliasing = AntiAliasing._4X;
			setting.SoftParticles = true;
			setting.SoftVegetation = true;
			setting.RealtimeReflectionProbes = true;
			setting.BillboardsFaceCameraPosition = true;
			setting.VSyncCount = VSyncCount.EveryVBlank;
			setting.LodBias = 1.5f;
			setting.ParticleRaycastBudget = 1024;
			return setting;
		}

		private QualitySetting CreateFantasticSettings()
		{
			QualitySetting setting = new QualitySetting(true);
			setting.Name = "Ultra";
			setting.PixelLightCount = 4;
			setting.Shadows = ShadowQuality.All;
			setting.ShadowResolution = ShadowResolution.High;
			setting.ShadowCascades = ShadowCascades.FourCascades;
			setting.ShadowDistance = 150;
			setting.ShadowmaskMode = ShadowmaskMode.DistanceShadowmask;
			setting.SkinWeights = SkinWeights.FourBones;
			setting.AnisotropicTextures = AnisotropicFiltering.Enable;
			setting.AntiAliasing = AntiAliasing._4X;
			setting.SoftParticles = true;
			setting.SoftVegetation = true;
			setting.RealtimeReflectionProbes = true;
			setting.BillboardsFaceCameraPosition = true;
			setting.VSyncCount = VSyncCount.EveryVBlank;
			setting.LodBias = 2.0f;
			setting.ParticleRaycastBudget = 4096;
			return setting;
		}

		/// <summary>
		/// EditorQuality previously
		/// </summary>
		public int CurrentQuality { get; private set; }
		public IReadOnlyList<QualitySetting> QualitySettingss => m_qualitySettings;
		public IReadOnlyDictionary<string, int> PerPlatformDefaultQuality => m_perPlatformDefaultQuality;
		public int StrippedMaximumLODLevel { get; private set; }

		public const string CurrentQualityName = "m_CurrentQuality";
		public const string QualitySettingsName = "m_QualitySettings";
		public const string PerPlatformDefaultQualityName = "m_PerPlatformDefaultQuality";

		private QualitySetting[] m_qualitySettings;
		private Dictionary<string, int> m_perPlatformDefaultQuality;
	}
}
