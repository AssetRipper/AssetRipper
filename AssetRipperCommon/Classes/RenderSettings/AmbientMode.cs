namespace AssetRipper.Core.Classes.RenderSettings
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum AmbientMode
	{
		/// <summary>
		/// Skybox-based or custom ambient lighting.
		/// </summary>
		Skybox = 0,
		/// <summary>
		/// Trilight ambient lighting.
		/// </summary>
		Trilight = 1,
		/// <summary>
		/// Flat ambient lighting.
		/// </summary>
		Flat = 3,
		/// <summary>
		/// Ambient lighting is defined by a custom cubemap.
		/// </summary>
		Custom = 4,
	}
}
