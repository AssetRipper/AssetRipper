namespace AssetRipper.Core.Classes.RenderSettings
{
	/// <summary>
	/// Fog mode to use.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum FogMode
	{
		/// <summary>
		/// Unknown fog.
		/// </summary>
		Unknown = -1,
		/// <summary>
		/// Disabled fog.
		/// </summary>
		Disabled = 0,
		/// <summary>
		/// Linear fog.
		/// </summary>
		Linear = 1,
		/// <summary>
		/// Exponential fog.
		/// </summary>
		Exponential = 2,
		/// <summary>
		/// Exponential squared fog (default).
		/// </summary>
		ExponentialSquared = 3
	}
}
