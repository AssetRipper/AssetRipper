namespace uTinyRipper.Classes.SpriteRenderers
{
	public enum SpriteTileMode
	{
		/// <summary>
		/// Sprite Renderer tiles the sprite continuously when is set to SpriteRenderer.tileMode.
		/// </summary>
		Continuous = 0,
		/// <summary>
		/// Sprite Renderer tiles the sprite once the Sprite Renderer size is above SpriteRenderer.adaptiveModeThreshold.
		/// </summary>
		Adaptive = 1
	}
}