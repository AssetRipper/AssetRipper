using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.UnityConnectSettings
{
	public sealed class UnityConnectSettings : GlobalGameManager
	{
		public UnityConnectSettings(AssetInfo assetInfo) : base(assetInfo)
		{
			TestEventUrl = string.Empty;
			TestConfigUrl = string.Empty;
			DashboardUrl = string.Empty;
		}

		public static UnityConnectSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new UnityConnectSettings(assetInfo));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 1;
			}
			return 0;
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasUnityConnectSettings(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasEnabled(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasOldEventUrl(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasEventUrl(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTestConfigUrl(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2020.3.5 and greater and 2021.1 full release and greater
		/// </summary>
		public static bool HasDashboardUrl(UnityVersion version)
		{
			if (version.IsGreaterEqual(2021))
				return version.IsGreaterEqual(2021, 1, 0, UnityVersionType.Final);

			return version.IsGreaterEqual(2020, 3, 5);
		}

		/// <summary>
		/// 5.6.0b6 and greater
		/// </summary>
		public static bool HasTestInitMode(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 6);
		/// <summary>
		/// 5.4.0 and greater and (Not Release or IsSupported)
		/// </summary>
		public static bool HasCrashReportingSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 4))
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
		public static bool HasUnityPurchasingSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
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
		public static bool HasUnityAnalyticsSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
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
		public static bool HasUnityAdsSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
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
		public static bool HasPerformanceReportingSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				TestMode = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasOldEventUrl(reader.Version))
			{
				TestEventUrl = reader.ReadString();
			}
			if (HasEventUrl(reader.Version))
			{
				EventUrl = reader.ReadString();
			}
			if (HasTestConfigUrl(reader.Version))
			{
				TestConfigUrl = reader.ReadString();
			}

			if (HasDashboardUrl(reader.Version))
			{
				DashboardUrl = reader.ReadString();
			}
			if (HasTestInitMode(reader.Version))
			{
				TestInitMode = reader.ReadInt32();
			}
			if (HasEnabled(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCrashReportingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				CrashReportingSettings.Read(reader);
			}
			if (HasUnityPurchasingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityPurchasingSettings.Read(reader);
			}
			if (HasUnityAnalyticsSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityAnalyticsSettings.Read(reader);
			}
			if (HasUnityAdsSettings(reader.Version, reader.Platform, reader.Flags))
			{
				UnityAdsSettings.Read(reader);
			}
			if (HasPerformanceReportingSettings(reader.Version, reader.Platform, reader.Flags))
			{
				PerformanceReportingSettings.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.ForceAddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(EnabledName, Enabled);
			node.Add(TestModeName, TestMode);
			node.Add(TestEventUrlName, GetTestEventUrl(container.Version));
			node.Add(TestConfigUrlName, GetTestConfigUrl(container.Version));
			if (HasDashboardUrl(container.ExportVersion))
			{
				node.Add(DashboardUrlName, DashboardUrl);
			}
			node.Add(TestInitModeName, TestInitMode);
			node.Add(CrashReportingSettingsName, GetCrashReportingSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(UnityPurchasingSettingsName, UnityPurchasingSettings.ExportYAML(container));
			node.Add(UnityAnalyticsSettingsName, GetUnityAnalyticsSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(UnityAdsSettingsName, GetUnityAdsSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(PerformanceReportingSettingsName, PerformanceReportingSettings.ExportYAML(container));
			return node;
		}

		private string GetTestEventUrl(UnityVersion version)
		{
			return HasEnabled(version) ? TestEventUrl : "https://api.uca.cloud.unity3d.com/v1/events";
		}
		private string GetTestConfigUrl(UnityVersion version)
		{
			return HasEnabled(version) ? TestConfigUrl : "https://config.uca.cloud.unity3d.com";
		}
		private CrashReportingSettings GetCrashReportingSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			return HasCrashReportingSettings(version, platform, flags) ? CrashReportingSettings : new CrashReportingSettings();
		}
		private UnityAnalyticsSettings GetUnityAnalyticsSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			return HasUnityAnalyticsSettings(version, platform, flags) ? UnityAnalyticsSettings : new UnityAnalyticsSettings();
		}
		private UnityAdsSettings GetUnityAdsSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			return HasUnityAdsSettings(version, platform, flags) ? UnityAdsSettings : new UnityAdsSettings();
		}

		public bool Enabled { get; set; }
		public bool TestMode { get; set; }
		/// <summary>
		/// OldEventUrl since 2018.3
		/// </summary>
		public string TestEventUrl { get; set; }
		public string EventUrl { get; set; }
		/// <summary>
		/// ConfigUrl since 2018.3 
		/// </summary>
		public string TestConfigUrl { get; set; }
		public string DashboardUrl { get; set; }
		public int TestInitMode { get; set; }

		public const string EnabledName = "m_Enabled";
		public const string TestModeName = "m_TestMode";
		public const string TestEventUrlName = "m_TestEventUrl";
		public const string TestConfigUrlName = "m_TestConfigUrl";
		public const string DashboardUrlName = "m_DashboardUrl";
		public const string TestInitModeName = "m_TestInitMode";
		public const string CrashReportingSettingsName = "CrashReportingSettings";
		public const string UnityPurchasingSettingsName = "UnityPurchasingSettings";
		public const string UnityAnalyticsSettingsName = "UnityAnalyticsSettings";
		public const string UnityAdsSettingsName = "UnityAdsSettings";
		public const string PerformanceReportingSettingsName = "PerformanceReportingSettings";

		public CrashReportingSettings CrashReportingSettings = new();
		public UnityPurchasingSettings UnityPurchasingSettings = new();
		public UnityAnalyticsSettings UnityAnalyticsSettings = new();
		public UnityAdsSettings UnityAdsSettings = new();
		public PerformanceReportingSettings PerformanceReportingSettings = new();
	}
}
