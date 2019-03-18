using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.UnityConnectSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class UnityConnectSettings : GlobalGameManager
	{
		public UnityConnectSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public UnityConnectSettings(AssetInfo assetInfo, bool _) :
			base(assetInfo)
		{
			TestEventUrl = string.Empty;
			TestConfigUrl = string.Empty;
			CrashReportingSettings = new CrashReportingSettings(true);
			UnityAnalyticsSettings = new UnityAnalyticsSettings(true);
			UnityAdsSettings = new UnityAdsSettings(true);
		}

		public static UnityConnectSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new UnityConnectSettings(assetInfo, true));
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadUnityConnectSettings(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadEnabled(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadOldEventUrl(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadEventUrl(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadTestConfigUrl(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadTestInitMode(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.4.0 and greater and (Not Release or IsSupported)
		/// </summary>
		public static bool IsReadCrashReportingSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if(version.IsLess(5, 4))
			{
				return false;
			}
			if (!flags.IsRelease())
			{
				return true;
			}
			switch (platform)
			{
				case Platform.NoTarget:
				case Platform.Android:
				case Platform.iOS:
				case Platform.tvOS:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerARM:
				case Platform.WebPlayerLZMA:
				case Platform.WebPlayerLZMAStreamed:
				case Platform.WebGL:
					return true;

				case Platform.Tizen:
					return version.IsGreaterEqual(5, 6);

				default:
					return false;
			}
		}
		/// <summary>
		/// Less than 5.4.0 or Not Release or IsSupported
		/// </summary>
		public static bool IsReadUnityPurchasingSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 4))
			{
				return true;
			}
			if (!flags.IsRelease())
			{
				return true;
			}
			switch (platform)
			{
				case Platform.NoTarget:
				case Platform.Android:
				case Platform.iOS:
				case Platform.tvOS:
				case Platform.Tizen:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerARM:
				case Platform.WebGL:
					return true;

				default:
					return false;
			}
		}
		/// <summary>
		/// Less than 5.4.0 or Not Release or IsSupported
		/// </summary>
		public static bool IsReadUnityAnalyticsSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 4))
			{
				return true;
			}
			if (!flags.IsRelease())
			{
				return true;
			}
			switch (platform)
			{
				case Platform.NoTarget:
				case Platform.Android:
				case Platform.iOS:
				case Platform.tvOS:
				case Platform.Tizen:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerARM:
				case Platform.WebGL:
					return true;

				default:
					return false;
			}
		}
		/// <summary>
		/// 5.5.0 and greater and (Not Release or IsSupported)
		/// </summary>
		public static bool IsReadUnityAdsSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 5))
			{
				return false;
			}
			if (!flags.IsRelease())
			{
				return true;
			}
			switch (platform)
			{
				case Platform.NoTarget:
				case Platform.Android:
				case Platform.iOS:
				case Platform.tvOS:
				case Platform.Tizen:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerARM:
				case Platform.WebGL:
					return true;

				default:
					return false;
			}
		}
		/// <summary>
		/// 5.6.0 and greater and (Not Release or IsSupported)
		/// </summary>
		public static bool IsReadPerformanceReportingSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 6))
			{
				return false;
			}
			if (!flags.IsRelease())
			{
				return true;
			}
			switch (platform)
			{
				case Platform.NoTarget:
				case Platform.Android:
				case Platform.iOS:
				case Platform.tvOS:
				case Platform.Tizen:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerARM:
				case Platform.WebGL:
					return true;

				default:
					return false;
			}
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 1;
			}
			return 0;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				TestMode = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadOldEventUrl(reader.Version))
			{
				TestEventUrl = reader.ReadString();
			}
			if (IsReadEventUrl(reader.Version))
			{
				EventUrl = reader.ReadString();
			}
			if (IsReadTestConfigUrl(reader.Version))
			{
				TestConfigUrl = reader.ReadString();
			}
			if (IsReadTestInitMode(reader.Version))
			{
				TestInitMode = reader.ReadInt32();
			}
			if(IsReadEnabled(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadCrashReportingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				CrashReportingSettings.Read(reader);
			}
			if (IsReadUnityPurchasingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityPurchasingSettings.Read(reader);
			}
			if (IsReadUnityAnalyticsSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityAnalyticsSettings.Read(reader);
			}
			if (IsReadUnityAdsSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityAdsSettings.Read(reader);
			}
			if (IsReadPerformanceReportingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				PerformanceReportingSettings.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.ForceAddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(EnabledName, Enabled);
			node.Add(TestModeName, TestMode);
			node.Add(TestEventUrlName, GetTestEventUrl(container.Version));
			node.Add(TestConfigUrlName, GetTestConfigUrl(container.Version));
			node.Add(TestInitModeName, TestInitMode);
			node.Add(CrashReportingSettingsName, GetCrashReportingSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(UnityPurchasingSettingsName, UnityPurchasingSettings.ExportYAML(container));
			node.Add(UnityAnalyticsSettingsName, GetUnityAnalyticsSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(UnityAdsSettingsName, GetUnityAdsSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(PerformanceReportingSettingsName, PerformanceReportingSettings.ExportYAML(container));
			return node;
		}

		private string GetTestEventUrl(Version version)
		{
			return IsReadEnabled(version) ? TestEventUrl : "https://api.uca.cloud.unity3d.com/v1/events";
		}
		private string GetTestConfigUrl(Version version)
		{
			return IsReadEnabled(version) ? TestConfigUrl : "https://config.uca.cloud.unity3d.com";
		}
		private CrashReportingSettings GetCrashReportingSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			return IsReadCrashReportingSettings(version, platform, flags) ? CrashReportingSettings : new CrashReportingSettings(true);
		}
		private UnityAnalyticsSettings GetUnityAnalyticsSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			return IsReadUnityAnalyticsSettings(version, platform, flags) ? UnityAnalyticsSettings : new UnityAnalyticsSettings(true);
		}
		private UnityAdsSettings GetUnityAdsSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			return IsReadUnityAdsSettings(version, platform, flags) ? UnityAdsSettings : new UnityAdsSettings(true);
		}

		public bool Enabled { get; private set; }
		public bool TestMode { get; private set; }
		/// <summary>
		/// OldEventUrl since 2018.3
		/// </summary>
		public string TestEventUrl { get; private set; }
		public string EventUrl { get; private set; }
		/// <summary>
		/// ConfigUrl since 2018.3 
		/// </summary>
		public string TestConfigUrl { get; private set; }
		public int TestInitMode { get; private set; }

		public const string EnabledName = "m_Enabled";
		public const string TestModeName = "m_TestMode";
		public const string TestEventUrlName = "m_TestEventUrl";
		public const string TestConfigUrlName = "m_TestConfigUrl";
		public const string TestInitModeName = "m_TestInitMode";
		public const string CrashReportingSettingsName = "CrashReportingSettings";
		public const string UnityPurchasingSettingsName = "UnityPurchasingSettings";
		public const string UnityAnalyticsSettingsName = "UnityAnalyticsSettings";
		public const string UnityAdsSettingsName = "UnityAdsSettings";
		public const string PerformanceReportingSettingsName = "PerformanceReportingSettings";

		public CrashReportingSettings CrashReportingSettings;
		public UnityPurchasingSettings UnityPurchasingSettings;
		public UnityAnalyticsSettings UnityAnalyticsSettings;
		public UnityAdsSettings UnityAdsSettings;
		public PerformanceReportingSettings PerformanceReportingSettings;
	}
}
