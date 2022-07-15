namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// Selects the cache server configuration mode.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorSettings.bindings.cs"/>
	/// </summary>
	public enum CacheServerMode
	{
		/// <summary>
		/// Use this if you want to use the global cache server settings.
		/// </summary>
		AsPreferences = 0,
		/// <summary>
		/// Use this if you want to enable use of the project specific cache server settings.
		/// </summary>
		Enabled = 1,
		/// <summary>
		/// Use this if you want to disable the use of cache server for the project.
		/// </summary>
		Disabled = 2,
	}
}
