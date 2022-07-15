namespace AssetRipper.Core.Classes.LightmapSettings
{
	/// <summary>
	/// Set the path tracer filter mode.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/GI/LightingSettings.bindings.cs"/>
	/// </summary>
	public enum FilterMode
	{
		/// <summary>
		/// Do not filter.
		/// </summary>
		None = 0,
		/// <summary>
		/// Select settings for filtering automatically
		/// </summary>
		Auto = 1,
		/// <summary>
		/// Setup filtering manually
		/// </summary>
		Advanced = 2,
	}
}
