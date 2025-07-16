namespace AssetRipper.IO.Files;

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
	/// <remarks>
	/// On Unity 3, this is only used for 3D models. Images and audio use <see cref="Cached"/> instead.
	/// </remarks>
	Meta = 3,
}
