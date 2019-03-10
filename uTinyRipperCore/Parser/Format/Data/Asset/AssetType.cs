namespace uTinyRipper
{
	public enum AssetType
	{
		Internal		= 0,
		/// <summary>
		/// AssetPath has the format "library/cache/[first Hash byte as hex]/[Hash as hex]"
		/// </summary>
		Cached			= 1,
		/// <summary>
		/// Serialized asset file. It contains all configs inside itself
		/// </summary>
		Serialized		= 2,
		/// <summary>
		/// Binary asset file. It containt all configs inside meta file
		/// </summary>
		Meta			= 3,
	}
}
