using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_310;

namespace AssetRipper.SourceGenerated.Extensions;

public static class UnityConnectSettingsExtensions
{
	public static void ConvertToEditorFormat(this IUnityConnectSettings settings)
	{
		settings.CrashReportingSettings?.ConvertToEditorFormat();
	}

	/// <summary>
	/// 5.4.0 and greater and (Not Release or IsSupported)
	/// </summary>
	public static bool HasCrashReportingSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		if (version.LessThan(5, 4))
		{
			return false;
		}
		if (!flags.IsRelease())
		{
			return true;
		}
		switch (platform)
		{
			case BuildTarget.NoTarget:
			case BuildTarget.Android:
			case BuildTarget.iOS:
			case BuildTarget.tvOS:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerARM:
			case BuildTarget.WebPlayerLZMA:
			case BuildTarget.WebPlayerLZMAStreamed:
			case BuildTarget.WebGL:
				return true;

			case BuildTarget.Tizen:
				return version.GreaterThanOrEquals(5, 6);

			default:
				return false;
		}
	}
	/// <summary>
	/// Less than 5.4.0 or Not Release or IsSupported
	/// </summary>
	public static bool HasUnityPurchasingSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		if (version.LessThan(5, 4))
		{
			return true;
		}
		if (!flags.IsRelease())
		{
			return true;
		}
		switch (platform)
		{
			case BuildTarget.NoTarget:
			case BuildTarget.Android:
			case BuildTarget.iOS:
			case BuildTarget.tvOS:
			case BuildTarget.Tizen:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerARM:
			case BuildTarget.WebGL:
				return true;

			default:
				return false;
		}
	}
	/// <summary>
	/// Less than 5.4.0 or Not Release or IsSupported
	/// </summary>
	public static bool HasUnityAnalyticsSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		if (version.LessThan(5, 4))
		{
			return true;
		}
		if (!flags.IsRelease())
		{
			return true;
		}
		switch (platform)
		{
			case BuildTarget.NoTarget:
			case BuildTarget.Android:
			case BuildTarget.iOS:
			case BuildTarget.tvOS:
			case BuildTarget.Tizen:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerARM:
			case BuildTarget.WebGL:
				return true;

			default:
				return false;
		}
	}
	/// <summary>
	/// 5.5.0 and greater and (Not Release or IsSupported)
	/// </summary>
	public static bool HasUnityAdsSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		if (version.LessThan(5, 5))
		{
			return false;
		}
		if (!flags.IsRelease())
		{
			return true;
		}
		switch (platform)
		{
			case BuildTarget.NoTarget:
			case BuildTarget.Android:
			case BuildTarget.iOS:
			case BuildTarget.tvOS:
			case BuildTarget.Tizen:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerARM:
			case BuildTarget.WebGL:
				return true;

			default:
				return false;
		}
	}
	/// <summary>
	/// 5.6.0 and greater and (Not Release or IsSupported)
	/// </summary>
	public static bool HasPerformanceReportingSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		if (version.LessThan(5, 6))
		{
			return false;
		}
		if (!flags.IsRelease())
		{
			return true;
		}
		switch (platform)
		{
			case BuildTarget.NoTarget:
			case BuildTarget.Android:
			case BuildTarget.iOS:
			case BuildTarget.tvOS:
			case BuildTarget.Tizen:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerARM:
			case BuildTarget.WebGL:
				return true;

			default:
				return false;
		}
	}
}
