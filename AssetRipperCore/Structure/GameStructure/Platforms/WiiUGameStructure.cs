namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	public class WiiUGameStructure : PlatformGameStructure
	{
		public WiiUGameStructure(string path)
		{
		}

		public static bool IsWiiUStructure(string rootPath)
		{
			return false;
		}

		public override PlatformType Platform => PlatformType.WiiU;
	}
}
