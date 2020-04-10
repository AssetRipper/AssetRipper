namespace uTinyRipper.BundleFiles
{
	public enum BundleType
	{
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
