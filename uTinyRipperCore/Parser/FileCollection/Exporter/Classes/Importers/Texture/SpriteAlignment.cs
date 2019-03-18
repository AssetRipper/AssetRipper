namespace uTinyRipper.AssetExporters.Classes
{
	/// <summary>
	/// How a Sprite's graphic rectangle is aligned with its pivot point.
	/// </summary>
	public enum SpriteAlignment
	{
		/// <summary>
		/// Pivot is at the center of the graphic rectangle.
		/// </summary>
		Center = 0,
		/// <summary>
		/// Pivot is at the top left corner of the graphic rectangle.
		/// </summary>
		TopLeft = 1,
		/// <summary>
		/// Pivot is at the center of the top edge of the graphic rectangle.
		/// </summary>
		TopCenter = 2,
		/// <summary>
		/// Pivot is at the top right corner of the graphic rectangle.
		/// </summary>
		TopRight = 3,
		/// <summary>
		/// Pivot is at the center of the left edge of the graphic rectangle.
		/// </summary>
		LeftCenter = 4,
		/// <summary>
		/// Pivot is at the center of the right edge of the graphic rectangle.
		/// </summary>
		RightCenter = 5,
		/// <summary>
		/// Pivot is at the bottom left corner of the graphic rectangle.
		/// </summary>
		BottomLeft = 6,
		/// <summary>
		/// Pivot is at the center of the bottom edge of the graphic rectangle.
		/// </summary>
		BottomCenter = 7,
		/// <summary>
		/// Pivot is at the bottom right corner of the graphic rectangle.
		/// </summary>
		BottomRight = 8,
		/// <summary>
		/// Pivot is at a custom position within the graphic rectangle.
		/// </summary>
		Custom = 9,
	}
}
