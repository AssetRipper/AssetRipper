namespace AssetRipper.IO.Files.BundleFiles
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
		/// <summary>
		/// True if UnityRaw or UnityWeb
		/// </summary>
		public static bool IsRawWeb(this BundleType _this)
		{
			return _this == BundleType.UnityRaw || _this == BundleType.UnityWeb;
		}
	}
}
