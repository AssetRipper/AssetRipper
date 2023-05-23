using AssetRipper.Import.Logging;

namespace AssetRipper.Import.Structure.Platforms
{
	public static class PlatformChecker
	{
		public static bool CheckPlatform(List<string> paths, [NotNullWhen(true)] out PlatformGameStructure? platformStructure, [NotNullWhen(true)] out MixedGameStructure? mixedStructure)
		{
			platformStructure = null;
			mixedStructure = null;

			if (CheckPC(paths, out PCGameStructure? pcGameStructure))
			{
				platformStructure = pcGameStructure;
			}
			else if (CheckLinux(paths, out LinuxGameStructure? linuxGameStructure))
			{
				platformStructure = linuxGameStructure;
			}
			else if (CheckMac(paths, out MacGameStructure? macGameStructure))
			{
				platformStructure = macGameStructure;
			}
			else if (CheckAndroid(paths, out AndroidGameStructure? androidGameStructure))
			{
				platformStructure = androidGameStructure;
			}
			else if (CheckiOS(paths, out iOSGameStructure? iosGameStructure))
			{
				platformStructure = iosGameStructure;
			}
			else if (CheckSwitch(paths, out SwitchGameStructure? switchGameStructure))
			{
				platformStructure = switchGameStructure;
			}
			else if (CheckPS4(paths, out PS4GameStructure? ps4GameStructure))
			{
				platformStructure = ps4GameStructure;
			}
			else if (CheckWebGL(paths, out WebGLGameStructure? webglGameStructure))
			{
				platformStructure = webglGameStructure;
			}
			else if (CheckWebPlayer(paths, out WebPlayerGameStructure? webplayerGameStructure))
			{
				platformStructure = webplayerGameStructure;
			}
			else if (CheckWiiU(paths, out WiiUGameStructure? wiiUGameStructure))
			{
				platformStructure = wiiUGameStructure;
			}

			if (CheckMixed(paths, out MixedGameStructure? mixedGameStructure))
			{
				mixedStructure = mixedGameStructure;
			}

			return platformStructure != null || mixedStructure != null;
		}


		private static bool CheckPC(List<string> paths, [NotNullWhen(true)] out PCGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (PCGameStructure.IsPCStructure(path))
				{
					gameStructure = new PCGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"PC game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckLinux(List<string> paths, [NotNullWhen(true)] out LinuxGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (LinuxGameStructure.IsLinuxStructure(path))
				{
					gameStructure = new LinuxGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"Linux game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckMac(List<string> paths, [NotNullWhen(true)] out MacGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (MacGameStructure.IsMacStructure(path))
				{
					gameStructure = new MacGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"Mac game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckAndroid(List<string> paths, [NotNullWhen(true)] out AndroidGameStructure? gameStructure)
		{
			string? androidStructure = null;
			string? obbStructure = null;
			foreach (string path in paths)
			{
				if (AndroidGameStructure.IsAndroidStructure(path))
				{
					if (androidStructure == null)
					{
						androidStructure = path;
					}
					else
					{
						throw new Exception("2 Android game stuctures has been found");
					}
				}
				else if (AndroidGameStructure.IsAndroidObbStructure(path))
				{
					if (obbStructure == null)
					{
						obbStructure = path;
					}
					else
					{
						throw new Exception("2 Android obb game stuctures has been found");
					}
				}
			}

			if (androidStructure != null)
			{
				gameStructure = new AndroidGameStructure(androidStructure, obbStructure);
				paths.Remove(androidStructure);
				Logger.Info(LogCategory.Import, $"Android game structure has been found at '{androidStructure}'");
				if (obbStructure != null)
				{
					paths.Remove(obbStructure);
					Logger.Info(LogCategory.Import, $"Android obb game structure has been found at '{obbStructure}'");
				}
				return true;
			}

			gameStructure = null;
			return false;
		}

		private static bool CheckiOS(List<string> paths, [NotNullWhen(true)] out iOSGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (iOSGameStructure.IsiOSStructure(path))
				{
					gameStructure = new iOSGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"iOS game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckPS4(List<string> paths, [NotNullWhen(true)] out PS4GameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (PS4GameStructure.IsPS4Structure(path))
				{
					gameStructure = new PS4GameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"PS4 game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckSwitch(List<string> paths, [NotNullWhen(true)] out SwitchGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (SwitchGameStructure.IsSwitchStructure(path))
				{
					gameStructure = new SwitchGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"Switch game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckWebGL(List<string> paths, [NotNullWhen(true)] out WebGLGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (WebGLGameStructure.IsWebGLStructure(path))
				{
					gameStructure = new WebGLGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckWebPlayer(List<string> paths, [NotNullWhen(true)] out WebPlayerGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (WebPlayerGameStructure.IsWebPlayerStructure(path))
				{
					gameStructure = new WebPlayerGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckWiiU(List<string> paths, [NotNullWhen(true)] out WiiUGameStructure? gameStructure)
		{
			foreach (string path in paths)
			{
				if (WiiUGameStructure.IsWiiUStructure(path))
				{
					gameStructure = new WiiUGameStructure(path);
					paths.Remove(path);
					Logger.Info(LogCategory.Import, $"WiiU game structure has been found at '{path}'");
					return true;
				}
			}
			gameStructure = null;
			return false;
		}

		private static bool CheckMixed(List<string> paths, [NotNullWhen(true)] out MixedGameStructure? gameStructure)
		{
			if (paths.Count > 0)
			{
				gameStructure = new MixedGameStructure(paths);
				if (paths.Count == 1)
				{
					Logger.Info(LogCategory.Import, $"Mixed game structure has been found at {paths[0]}");
				}
				else
				{
					Logger.Info(LogCategory.Import, $"Mixed game structure has been found for {paths.Count} paths");
				}

				paths.Clear();
				return true;
			}
			gameStructure = null;
			return false;
		}
	}
}
