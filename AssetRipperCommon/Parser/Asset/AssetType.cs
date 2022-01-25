namespace AssetRipper.Core.Parser.Asset
{
	public enum AssetType
	{
		/// <summary>
		/// Used by released game
		/// </summary>
		Internal = 0,
		/// <summary>
		/// Library asset file. It is editor created file, it doesn't exist in Assets directory
		/// It has the format "library/cache/[first Hash byte as hex]/[Hash as hex]"
		/// </summary>
		Cached = 1,
		/// <summary>
		/// Serialized asset file. It contains all parameters inside itself
		/// </summary>
		Serialized = 2,
		/// <summary>
		/// Binary asset file. It contains all parameters inside meta file
		/// </summary>
		Meta = 3,
	}
}
