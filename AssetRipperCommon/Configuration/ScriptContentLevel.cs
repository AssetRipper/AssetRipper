namespace AssetRipper.Core.Configuration
{
	public enum ScriptContentLevel
	{
		/// <summary>
		/// MonoBehaviours are not exported.
		/// </summary>
		Level0,
		/// <summary>
		/// Scripts are not exported.
		/// </summary>
		Level1,
		/// <summary>
		/// Only fields and attributes are exported.
		/// </summary>
		Level2,
		/// <summary>
		/// Method bodies are stubbed.
		/// </summary>
		Level3,
		/// <summary>
		/// Full methods for Mono games and dummy methods for IL2Cpp games.
		/// </summary>
		Level4,
		/// <summary>
		/// IL2Cpp methods are safely recovered where possible.
		/// </summary>
		Level5,
		/// <summary>
		/// IL2Cpp methods are recovered without regard to safety.
		/// </summary>
		Level6,
	}
}
