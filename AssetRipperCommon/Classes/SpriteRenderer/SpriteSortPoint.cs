namespace AssetRipper.Core.Classes.SpriteRenderer
{
	/// <summary>
	/// Determines the position of the Sprite used for sorting the Renderer.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/2D/Common/ScriptBindings/SpriteRenderer.bindings.cs"/>
	/// </summary>
	public enum SpriteSortPoint
	{
		/// <summary>
		/// The center of the Sprite is used as the point for sorting the Renderer.
		/// </summary>
		Center = 0,
		/// <summary>
		/// The pivot of the Sprite is used as the point for sorting the Renderer.
		/// </summary>
		Pivot = 1
	}
}
