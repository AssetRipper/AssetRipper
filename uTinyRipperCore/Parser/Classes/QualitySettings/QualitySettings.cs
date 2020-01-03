using System.Collections.Generic;
using uTinyRipper.Classes.QualitySettingss;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;
using uTinyRipper;

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

		public static int ToSerializedVersion(Version version)
		{
			// static PlatformDefaultQuality has been replaced by dictionary
			if (version.IsGreaterEqual(3, 5))
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

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasQualitySettings(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 1.6.0 to 3.5.0 exclusive
		/// </summary>
		public static bool HasDefaultStandaloneQuality(Version version) => version.IsLess(3, 5) && version.IsGreaterEqual(1, 6);
		/// <summary>
		/// 3.2.0 to 3.5.0 exclusive
		/// </summary>
		public static bool HasDefaultMobileQuality(Version version) => version.IsLess(3, 5) && version.IsGreaterEqual(3, 2);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasQualitySettingArray(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasWebPlayer(Version version) => version.IsLess(3);
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasPerPlatformDefaultQuality(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool HasStrippedMaximumLODLevel(Version version, TransferInstructionFlags flags)
		{
			return flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasDefaultStandaloneQuality(reader.Version))
			{
				QualityLevel defaultStandaloneQuality = (QualityLevel)reader.ReadInt32();
				QualityLevel defaultWebPlayerQuality = (QualityLevel)reader.ReadInt32();
				PerPlatformDefaultQuality = new Dictionary<string, int>();
				SetDefaultPlatformQuality(PerPlatformDefaultQuality);
				PerPlatformDefaultQuality[BuildTargetGroup.Standalone.ToExportString()] = (int)defaultStandaloneQuality;
				PerPlatformDefaultQuality[BuildTargetGroup.WebPlayer.ToExportString()] = (int)defaultStandaloneQuality;
			}
			if (HasDefaultMobileQuality(reader.Version))
			{
				QualityLevel defaultMobileQuality = (QualityLevel)reader.ReadInt32();
				PerPlatformDefaultQuality[BuildTargetGroup.Android.ToExportString()] = (int)defaultMobileQuality;
				PerPlatformDefaultQuality[BuildTargetGroup.iOS.ToExportString()] = (int)defaultMobileQuality;
			}
			CurrentQuality = reader.ReadInt32();
			if (HasQualitySettingArray(reader.Version))
			{
				QualitySettingss = reader.ReadAssetArray<QualitySetting>();
			}
			else
			{
				QualitySettingss = new QualitySetting[6];
				QualitySetting fastest = reader.ReadAsset<QualitySetting>();
				fastest.Name = nameof(QualityLevel.Fastest);
				QualitySettingss[(int)QualityLevel.Fastest] = fastest;

				QualitySetting fast = reader.ReadAsset<QualitySetting>();
				fast.Name = nameof(QualityLevel.Fast);
				QualitySettingss[(int)QualityLevel.Fast] = fast;

				QualitySetting simple = reader.ReadAsset<QualitySetting>();
				simple.Name = nameof(QualityLevel.Simple);
				QualitySettingss[(int)QualityLevel.Simple] = simple;

				QualitySetting good = reader.ReadAsset<QualitySetting>();
				good.Name = nameof(QualityLevel.Good);
				QualitySettingss[(int)QualityLevel.Good] = good;

				QualitySetting beautiful = reader.ReadAsset<QualitySetting>();
				beautiful.Name = nameof(QualityLevel.Beautiful);
				QualitySettingss[(int)QualityLevel.Beautiful] = beautiful;

				QualitySetting fantastic = reader.ReadAsset<QualitySetting>();
				fantastic.Name = nameof(QualityLevel.Fantastic);
				QualitySettingss[(int)QualityLevel.Fantastic] = fantastic;
			}
			foreach (QualitySetting setting in QualitySettingss)
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

			if (HasWebPlayer(reader.Version))
			{
				QualitySetting webPlayer = reader.ReadAsset<QualitySetting>();
				webPlayer.Name = "WebPlayer";
			}

#if UNIVERSAL
			if (HasPerPlatformDefaultQuality(reader.Version, reader.Flags))
			{
				PerPlatformDefaultQuality = new Dictionary<string, int>();
				PerPlatformDefaultQuality.Read(reader);
			}
#endif
			if (HasStrippedMaximumLODLevel(reader.Version, reader.Flags))
			{
				StrippedMaximumLODLevel = reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(CurrentQualityName, CurrentQuality);
			node.Add(QualitySettingsName, QualitySettingss.ExportYAML(container));
			node.Add(PerPlatformDefaultQualityName, GetPerPlatformDefaultQuality(container.Version, container.Flags).ExportYAML());
			return node;
		}

		private IReadOnlyDictionary<string, int> GetPerPlatformDefaultQuality(Version version, TransferInstructionFlags flags)
		{
			if (HasDefaultStandaloneQuality(version))
			{
				return PerPlatformDefaultQuality;
			}
#if UNIVERSAL
			if (HasPerPlatformDefaultQuality(version, flags))
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
		public int CurrentQuality { get; set; }
		public QualitySetting[] QualitySettingss { get; set; }
		public Dictionary<string, int> PerPlatformDefaultQuality { get; set; }
		public int StrippedMaximumLODLevel { get; set; }

		public const string CurrentQualityName = "m_CurrentQuality";
		public const string QualitySettingsName = "m_QualitySettings";
		public const string PerPlatformDefaultQualityName = "m_PerPlatformDefaultQuality";
	}
}
