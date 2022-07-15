namespace AssetRipper.Core.Classes.SpriteRenderer
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/2D/Common/ScriptBindings/SpriteRenderer.bindings.cs"/>
	/// </summary>
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