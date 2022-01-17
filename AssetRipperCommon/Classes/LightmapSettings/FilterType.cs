namespace AssetRipper.Core.Classes.LightmapSettings
{
	/// <summary>
	/// Which path tracer filter is used.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/GI/LightingSettings.bindings.cs"/>
	/// </summary>
	public enum FilterType
	{
		/// <summary>
		/// A Gaussian filter is applied.
		/// </summary>
		Gaussian = 0,
		/// <summary>
		/// An A-Trous filter is applied.
		/// </summary>
		ATrous = 1,
		/// <summary>
		/// No filter
		/// </summary>
		None = 2,
	}
}
