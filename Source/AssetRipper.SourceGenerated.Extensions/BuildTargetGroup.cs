using AssetRipper.IO.Files;

namespace AssetRipper.SourceGenerated.Extensions;

public enum BuildTargetGroup
{
	/// <summary>
	/// Unknown target.
	/// </summary>
	Unknown = 0,
	/// <summary>
	/// Mac/PC standalone target.
	/// </summary>
	Standalone = 1,
	/// <summary>
	/// Mac/PC webplayer target.
	/// </summary>
	WebPlayer = 2,
	Wii = 3,
	/// <summary>
	/// Apple iOS target.
	/// </summary>
	iPhone = 4,
	/// <summary>
	/// Apple iOS target.
	/// </summary>
	iOS = 4,
	PS3 = 5,
	XBOX360 = 6,
	/// <summary>
	/// Android target.
	/// </summary>
	Android = 7,
	GLESEmu = 9,
	/// <summary>
	/// Google Native Client
	/// </summary>
	NaCl = 11,
	FlashPlayer = 12,
	/// <summary>
	/// WebGL.
	/// </summary>
	WebGL = 13,
	/// <summary>
	/// Windows Store Apps target.
	/// </summary>
	WSA = 14,
	Metro = 14,
	WP8 = 15,
	BlackBerry = 16,
	/// <summary>
	/// Samsung Tizen target.
	/// </summary>
	Tizen = 17,
	/// <summary>
	/// Sony PS Vita target.
	/// </summary>
	PSP2 = 18,
	/// <summary>
	/// Sony Playstation 4 target.
	/// </summary>
	PS4 = 19,
	PSM = 20,
	/// <summary>
	/// Microsoft Xbox One target.
	/// </summary>
	XboxOne = 21,
	SamsungTV = 22,
	/// <summary>
	/// Nintendo 3DS target.
	/// </summary>
	N3DS = 23,
	/// <summary>
	/// Nintendo Wii U target.
	/// </summary>
	WiiU = 24,
	/// <summary>
	/// Apple's tvOS target.
	/// </summary>
	tvOS = 25,
	/// <summary>
	/// Facebook target.
	/// </summary>
	Facebook = 26,
	/// <summary>
	/// Nintendo Switch target.
	/// </summary>
	Switch = 27,
}

public static class BuildTargetGroupExtensions
{
	public static BuildTargetGroup PlatformToBuildGroup(this BuildTarget _this)
	{
		switch (_this)
		{
			case BuildTarget.StandaloneOSXUniversal:
			case BuildTarget.StandaloneOSXPPC:
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneWinPlayer:
			case BuildTarget.StandaloneLinux:
			case BuildTarget.StandaloneWin64Player:
			case BuildTarget.StandaloneLinux64:
			case BuildTarget.StandaloneLinuxUniversal:
			case BuildTarget.StandaloneOSXIntel64:
				return BuildTargetGroup.Standalone;

			case BuildTarget.WebPlayerLZMA:
			case BuildTarget.WebPlayerLZMAStreamed:
				return BuildTargetGroup.WebPlayer;

			case BuildTarget.Wii:
				return BuildTargetGroup.Wii;

			case BuildTarget.iOS:
				return BuildTargetGroup.iOS;

			case BuildTarget.PS3:
				return BuildTargetGroup.PS3;

			case BuildTarget.XBox360:
				return BuildTargetGroup.XBOX360;

			case BuildTarget.Android:
				return BuildTargetGroup.Android;

			case BuildTarget.WinGLESEmu:
			case BuildTarget.WinGLES20Emu:
				return BuildTargetGroup.GLESEmu;

			case BuildTarget.GoogleNaCl:
				return BuildTargetGroup.NaCl;

			case BuildTarget.Flash:
				return BuildTargetGroup.FlashPlayer;

			case BuildTarget.WebGL:
				return BuildTargetGroup.WebGL;

			case BuildTarget.MetroPlayerX86:
			case BuildTarget.MetroPlayerX64:
			case BuildTarget.MetroPlayerARM:
				return BuildTargetGroup.WSA;

			case BuildTarget.WP8Player:
				return BuildTargetGroup.WP8;

			case BuildTarget.BB10:
				return BuildTargetGroup.BlackBerry;

			case BuildTarget.Tizen:
				return BuildTargetGroup.Tizen;

			case BuildTarget.PSP2:
				return BuildTargetGroup.PSP2;

			case BuildTarget.PS4:
				return BuildTargetGroup.PS4;

			case BuildTarget.PSM:
				return BuildTargetGroup.PSM;

			case BuildTarget.XboxOne:
				return BuildTargetGroup.XboxOne;

			case BuildTarget.SamsungTV:
				return BuildTargetGroup.SamsungTV;

			case BuildTarget.N3DS:
				return BuildTargetGroup.N3DS;

			case BuildTarget.WiiU:
				return BuildTargetGroup.WiiU;

			case BuildTarget.tvOS:
				return BuildTargetGroup.tvOS;

			case BuildTarget.Switch:
				return BuildTargetGroup.Switch;

			default:
				throw new NotSupportedException($"Platform {_this} is not supported.");
		}
	}

	public static string ToExportString(this BuildTargetGroup _this)
	{
		return _this switch
		{
			BuildTargetGroup.Unknown => "Unknown",
			BuildTargetGroup.Standalone => "Standalone",
			BuildTargetGroup.WebPlayer => "WebPlayer",
			BuildTargetGroup.Wii => "Wii",
			BuildTargetGroup.iOS => "iPhone",
			BuildTargetGroup.PS3 => "PS3",
			BuildTargetGroup.XBOX360 => "XBOX360",
			BuildTargetGroup.Android => "Android",
			BuildTargetGroup.GLESEmu => "GLESEmu",
			BuildTargetGroup.NaCl => "NaCl",
			BuildTargetGroup.FlashPlayer => "FlashPlayer",
			BuildTargetGroup.WebGL => "WebGL",
			BuildTargetGroup.Metro => "Windows Store Apps",
			BuildTargetGroup.WP8 => "WP8",
			BuildTargetGroup.BlackBerry => "BlackBerry",
			BuildTargetGroup.Tizen => "Tizen",
			BuildTargetGroup.PSP2 => "PSP2",
			BuildTargetGroup.PS4 => "PS4",
			BuildTargetGroup.PSM => "PSM",
			BuildTargetGroup.XboxOne => "XboxOne",
			BuildTargetGroup.SamsungTV => "SamsungTV",
			BuildTargetGroup.N3DS => "Nintendo 3DS",
			BuildTargetGroup.WiiU => "WiiU",
			BuildTargetGroup.tvOS => "tvOS",
			BuildTargetGroup.Facebook => "Facebook",
			BuildTargetGroup.Switch => "Nintendo Switch",
			_ => throw new NotSupportedException($"Value: {_this}"),
		};
	}
}
