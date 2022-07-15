namespace AssetRipper.Core.Classes.SpriteRenderer
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/2D/Common/ScriptBindings/SpriteRenderer.bindings.cs"/>
	/// </summary>
	public enum SpriteDrawMode
	{
		/// <summary>
		/// Displays the full sprite.
		/// </summary>
		Simple = 0,
		/// <summary>
		/// The SpriteRenderer will render the sprite as a 9-slice image where the corners
		/// will remain constant and the other sections will scale.
		/// </summary>
		Sliced = 1,
		/// <summary> 
		/// The SpriteRenderer will render the sprite as a 9-slice image where the corners
		/// will remain constant and the other sections will tile.
		/// </summary>
		Tiled = 2
	}
}
