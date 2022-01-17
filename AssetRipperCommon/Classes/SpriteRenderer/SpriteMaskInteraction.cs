namespace AssetRipper.Core.Classes.SpriteRenderer
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/2D/Common/ScriptBindings/SpriteRenderer.bindings.cs"/>
	/// </summary>
	public enum SpriteMaskInteraction
	{
		/// <summary>
		/// The sprite will not interact with the masking system.
		/// </summary>
		None = 0,
		/// <summary>
		/// The sprite will be visible only in areas where a mask is present.
		/// </summary>
		VisibleInsideMask = 1,
		/// <summary>
		/// The sprite will be visible only in areas where no mask is present.
		/// </summary>
		VisibleOutsideMask = 2
	}
}
