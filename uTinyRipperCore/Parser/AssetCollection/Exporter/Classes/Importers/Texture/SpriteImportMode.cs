namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// Texture importer modes for Sprite import.
	/// </summary>
	public enum SpriteImportMode
	{
		/// <summary>
		/// Graphic is not a Sprite.
		/// </summary>
		None = 0,
		/// <summary>
		/// Sprite is a single image section extracted automatically from the texture.
		/// </summary>
		Single = 1,
		/// <summary>
		/// Sprites are multiple image sections extracted from the texture.
		/// </summary>
		Multiple = 2,
		/// <summary>
		/// Sprite has it own mesh outline defined.
		/// </summary>
		Polygon = 3,
	}
}
