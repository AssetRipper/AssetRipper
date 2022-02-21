using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Classes.Misc
{
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
		public static BuildTargetGroup PlatformToBuildGroup(this Platform _this)
		{
			switch (_this)
			{
				case Platform.StandaloneOSXUniversal:
				case Platform.StandaloneOSXPPC:
				case Platform.StandaloneOSXIntel:
				case Platform.StandaloneWinPlayer:
				case Platform.StandaloneLinux:
				case Platform.StandaloneWin64Player:
				case Platform.StandaloneLinux64:
				case Platform.StandaloneLinuxUniversal:
				case Platform.StandaloneOSXIntel64:
					return BuildTargetGroup.Standalone;

				case Platform.WebPlayerLZMA:
				case Platform.WebPlayerLZMAStreamed:
					return BuildTargetGroup.WebPlayer;

				case Platform.Wii:
					return BuildTargetGroup.Wii;

				case Platform.iOS:
					return BuildTargetGroup.iOS;

				case Platform.PS3:
					return BuildTargetGroup.PS3;

				case Platform.XBox360:
					return BuildTargetGroup.XBOX360;

				case Platform.Android:
					return BuildTargetGroup.Android;

				case Platform.WinGLESEmu:
				case Platform.WinGLES20Emu:
					return BuildTargetGroup.GLESEmu;

				case Platform.GoogleNaCl:
					return BuildTargetGroup.NaCl;

				case Platform.Flash:
					return BuildTargetGroup.FlashPlayer;

				case Platform.WebGL:
					return BuildTargetGroup.WebGL;

				case Platform.MetroPlayerX86:
				case Platform.MetroPlayerX64:
				case Platform.MetroPlayerARM:
					return BuildTargetGroup.WSA;

				case Platform.WP8Player:
					return BuildTargetGroup.WP8;

				case Platform.BB10:
					return BuildTargetGroup.BlackBerry;

				case Platform.Tizen:
					return BuildTargetGroup.Tizen;

				case Platform.PSP2:
					return BuildTargetGroup.PSP2;

				case Platform.PS4:
					return BuildTargetGroup.PS4;

				case Platform.PSM:
					return BuildTargetGroup.PSM;

				case Platform.XboxOne:
					return BuildTargetGroup.XboxOne;

				case Platform.SamsungTV:
					return BuildTargetGroup.SamsungTV;

				case Platform.N3DS:
					return BuildTargetGroup.N3DS;

				case Platform.WiiU:
					return BuildTargetGroup.WiiU;

				case Platform.tvOS:
					return BuildTargetGroup.tvOS;

				case Platform.Switch:
					return BuildTargetGroup.Switch;

				default:
					throw new NotSupportedException($"Platform {_this} is not supported.");
			}
		}

		public static string ToExportString(this BuildTargetGroup _this)
		{
			return _this switch
			{
				BuildTargetGroup.N3DS => "Nintendo 3DS",
				BuildTargetGroup.Switch => "Nintendo Switch",
				BuildTargetGroup.Metro => "Windows Store Apps",
				BuildTargetGroup.iOS => "iPhone",
				_ => _this.ToString(),
			};
		}
	}
}
