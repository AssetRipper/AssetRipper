namespace AssetRipper.Parser.Files.BundleFile
{
	public enum BundleType
	{
		UnityArchive,//not supported
		UnityRaw,
		UnityWeb,
		UnityFS,
	}

	public static class BundleTypeExtensions
	{
		public static bool IsRawWeb(this BundleType _this)
		{
			return _this == BundleType.UnityRaw || _this == BundleType.UnityWeb;
		}
	}
}
