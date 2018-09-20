namespace uTinyRipper.Classes.EditorSettingss
{
	/// <summary>
	/// Sprite Packer mode for the current project.
	/// </summary>
	public enum SpritePackerMode
	{
		/// <summary>
		/// Doesn't pack sprites.
		/// </summary>
		Disabled			= 0,
		/// <summary>
		/// Updates the sprite atlas cache when the Player or bundles builds containing Sprite with the legacy packing tag.
		/// </summary>
		BuildTimeOnly		= 1,
		/// <summary>
		/// Always maintain an up-to-date sprite atlas cache for Sprite with packing tag (legacy).
		/// </summary>
		AlwaysOn			= 2,
		/// <summary>
		/// Pack all the SpriteAtlas when building player/bundles.
		/// </summary>
		BuildTimeOnlyAtlas	= 3,
		/// <summary>
		/// Always pack all the SpriteAtlas.
		/// </summary>
		AlwaysOnAtlas		= 4,
	}
}
