namespace uTinyRipper.Classes.Sprites
{
	/// <summary>
	/// Defines the type of mesh generated for a sprite.
	/// </summary>
	public enum SpriteMeshType
	{
		/// <summary>
		/// Rectangle mesh equal to the user specified sprite size.
		/// </summary>
		FullRect = 0,
		/// <summary>
		/// Tight mesh based on pixel alpha values. As many excess pixels are cropped as possible.
		/// </summary>
		Tight = 1,
	}
}
