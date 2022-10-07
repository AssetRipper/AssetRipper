namespace AssetRipper.IO.Files
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/BuildTarget.cs"/>
	/// </summary>
	public enum BuildTarget : uint
	{
		ValidPlayer = 1,
		/// <summary>
		/// Build a universal macOS standalone
		/// </summary>
		StandaloneOSXUniversal = 2,
		/// <summary>
		/// Build a macOS standalone (PowerPC only)
		/// </summary>
		StandaloneOSXPPC = 3,
		/// <summary>
		/// Build a macOS standalone (Intel only)
		/// </summary>
		StandaloneOSXIntel = 4,
		/// <summary>
		/// Build a Windows standalone
		/// </summary>
		StandaloneWinPlayer = 5,
		/// <summary>
		/// Build a web player.
		/// </summary>
		WebPlayerLZMA = 6,
		/// <summary>
		/// Build a streamed web player
		/// </summary>
		WebPlayerLZMAStreamed = 7,
		Wii = 8,
		/// <summary>
		/// Build an iOS player
		/// </summary>
		iOS = 9,
		PS3 = 10,
		XBox360 = 11,
		Broadcom = 12,
		/// <summary>
		/// Build an Android .apk standalone app
		/// </summary>
		Android = 13,
		WinGLESEmu = 14,
		WinGLES20Emu = 15,
		/// <summary>
		/// Google Native Client
		/// </summary>
		GoogleNaCl = 16,
		/// <summary>
		/// Build a Linux standalone
		/// </summary>
		StandaloneLinux = 17,
		Flash = 18,
		/// <summary>
		/// Build a Windows 64-bit standalone
		/// </summary>
		StandaloneWin64Player = 19,
		/// <summary>
		/// WebGL
		/// </summary>
		WebGL = 20,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerX86 = 21,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerX64 = 22,
		/// <summary>
		/// Build an Windows Store Apps player
		/// </summary>
		MetroPlayerARM = 23,
		/// <summary>
		/// Build a Linux 64-bit standalone
		/// </summary>
		StandaloneLinux64 = 24,
		/// <summary>
		/// Build a Linux universal standalone
		/// </summary>
		StandaloneLinuxUniversal = 25,
		WP8Player = 26,
		/// <summary>
		/// Build a macOS Intel 64-bit standalone
		/// </summary>
		StandaloneOSXIntel64 = 27,
		/// <summary>
		/// BlackBerry
		/// </summary>
		BB10 = 28,
		/// <summary>
		/// Build a Tizen player
		/// </summary>
		Tizen = 29,
		/// <summary>
		/// Build a PS Vita Standalone
		/// </summary>
		PSP2 = 30,
		/// <summary>
		/// Build a PS4 Standalone
		/// </summary>
		PS4 = 31,
		PSM = 32,
		/// <summary>
		/// Build a Xbox One Standalone
		/// </summary>
		XboxOne = 33,
		/// <summary>
		/// Build to Samsung Smart TV platform
		/// </summary>
		SamsungTV = 34,
		/// <summary>
		/// Build to Nintendo 3DS platform
		/// </summary>
		N3DS = 35,
		/// <summary>
		/// Build a Wii U standalone
		/// </summary>
		WiiU = 36,
		/// <summary>
		/// Build to Apple's tvOS platform
		/// </summary>
		tvOS = 37,
		/// <summary>
		/// Build a Nintendo Switch player
		/// </summary>
		Switch = 38,
		Lumin = 39,
		Stadia = 40,
		CloudRendering = 41,
		GameCoreXboxSeries = 42,
		GameCoreXboxOne = 43,
		/// <summary>
		/// Build a PS5 Standalone
		/// </summary>
		PS5 = 44,
		EmbeddedLinux = 45,
		QNX = 46,

		/// <summary>
		/// Editor
		/// </summary>
		NoTarget = 0xFFFFFFFE,
		AnyPlayer = 0xFFFFFFFF,
	}

	public static class PlatformExtensions
	{
		public static bool IsCompatible(this BuildTarget _this, BuildTarget comp)
		{
			return _this == comp || (_this.IsStandalone() && comp.IsStandalone());
		}

		public static bool IsStandalone(this BuildTarget _this)
		{
			switch (_this)
			{
				case BuildTarget.StandaloneWinPlayer:
				case BuildTarget.StandaloneWin64Player:
				case BuildTarget.StandaloneLinux:
				case BuildTarget.StandaloneLinux64:
				case BuildTarget.StandaloneLinuxUniversal:
				case BuildTarget.StandaloneOSXIntel:
				case BuildTarget.StandaloneOSXIntel64:
				case BuildTarget.StandaloneOSXPPC:
				case BuildTarget.StandaloneOSXUniversal:
					return true;
			}
			return false;
		}
	}
}
