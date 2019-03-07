namespace uTinyRipper
{
	public enum Platform : uint
	{
		ValidPlayer				= 1,
		/// <summary>
		/// Build a universal macOS standalone
		/// </summary>
		StandaloneOSXUniversal	= 2,
		/// <summary>
		/// Build a macOS standalone (PowerPC only)
		/// </summary>
		StandaloneOSXPPC		= 3,
		/// <summary>
		/// Build a macOS standalone (Intel only)
		/// </summary>
		StandaloneOSXIntel		= 4,
		/// <summary>
		/// Build a Windows standalone
		/// </summary>
		StandaloneWinPlayer		= 5,
		/// <summary>
		/// Build a web player.
		/// </summary>
		WebPlayerLZMA			= 6,
		/// <summary>
		/// Build a streamed web player
		/// </summary>
		WebPlayerLZMAStreamed	= 7,
		Wii						= 8,
		/// <summary>
		/// Build an iOS player
		/// </summary>
		iOS						= 9,
		PS3						= 10,
		XBox360					= 11,
		Broadcom				= 12,
		/// <summary>
		/// Build an Android .apk standalone app
		/// </summary>
		Android					= 13,
		WinGLESEmu				= 14,
		WinGLES20Emu			= 15,
		/// <summary>
		/// Google Native Client
		/// </summary>
		GoogleNaCl				= 16,
		/// <summary>
		/// Build a Linux standalone
		/// </summary>
		StandaloneLinux			= 17,
		Flash					= 18,
		/// <summary>
		/// Build a Windows 64-bit standalone
		/// </summary>
		StandaloneWin64Player	= 19,
		/// <summary>
		/// WebGL
		/// </summary>
		WebGL					= 20,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerX86			= 21,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerX64			= 22,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerARM			= 23,
		/// <summary>
		/// Build a Linux 64-bit standalone
		/// </summary>
		StandaloneLinux64		= 24,
		/// <summary>
		/// Build a Linux universal standalone
		/// </summary>
		StandaloneLinuxUniversal = 25,
		WP8Player				= 26,
		/// <summary>
		/// Build a macOS Intel 64-bit standalone
		/// </summary>
		StandaloneOSXIntel64	= 27,
		/// <summary>
		/// BlackBerry
		/// </summary>
		BB10					= 28,
		/// <summary>
		/// Build a Tizen player
		/// </summary>
		Tizen					= 29,
		/// <summary>
		/// Build a PS Vita Standalone
		/// </summary>
		PSP2					= 30,
		/// <summary>
		/// Build a PS4 Standalone
		/// </summary>
		PS4						= 31,
		PSM						= 32,
		/// <summary>
		/// Build a Xbox One Standalone
		/// </summary>
		XboxOne					= 33,
		/// <summary>
		/// Build to Samsung Smart TV platform
		/// </summary>
		SamsungTV				= 34,
		/// <summary>
		/// Build to Nintendo 3DS platform
		/// </summary>
		N3DS					= 35,
		/// <summary>
		/// Build a Wii U standalone
		/// </summary>
		WiiU					= 36,
		/// <summary>
		/// Build to Apple's tvOS platform
		/// </summary>
		tvOS					= 37,
		/// <summary>
		/// Build a Nintendo Switch player
		/// </summary>
		Switch					= 38,
		PlayerTypeCount,
		
		/// <summary>
		/// Editor
		/// </summary>
		NoTarget				= 0xFFFFFFFE,
		AnyPlayer				= 0xFFFFFFFF,
	}

	public static class PlatformExtensions
	{
		public static bool IsCompatible(this Platform _this, Platform comp)
		{
			if (_this == comp)
			{
				return true;
			}

			if (_this.IsStandalone())
			{
				if (comp.IsStandalone())
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsStandalone(this Platform _this)
		{
			switch(_this)
			{
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneOSXIntel64:
				case Platform.StandaloneOSXPPC:
				case Platform.StandaloneOSXUniversal:
					return true;
			}
			return false;
		}
	}
}
