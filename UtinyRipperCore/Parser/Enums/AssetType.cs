namespace UtinyRipper
{
	public enum AssetType
	{
		Internal = 0,
		/// <summary>
		/// AssetPath has the format "library/cache/[first Hash byte as hex]/[Hash as hex]"
		/// </summary>
		Cached = 1,
		Serialized = 2,
		Meta = 3,
	}
}
