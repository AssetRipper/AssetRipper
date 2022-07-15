namespace AssetRipper.Core.Classes.Renderer
{
	/// <summary>
	/// How shadows are cast from this object.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ShadowCastingMode : byte
	{
		/// <summary>
		/// No shadows are cast from this object.
		/// </summary>
		Off = 0,
		/// <summary>
		/// Shadows are cast from this object.
		/// </summary>
		On = 1,
		/// <summary>
		/// Shadows are cast from this object, treating it as two-sided.
		/// </summary>
		TwoSided = 2,
		/// <summary>
		/// Object casts shadows, but is otherwise invisible in the scene.
		/// </summary>
		ShadowsOnly = 3,
	}
}
