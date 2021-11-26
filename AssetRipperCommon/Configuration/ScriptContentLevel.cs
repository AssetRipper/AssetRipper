namespace AssetRipper.Core.Configuration
{
	public enum ScriptContentLevel
	{
		/// <summary>
		/// Scripts are not exported.
		/// </summary>
		Level0,
		/// <summary>
		/// Methods are stripped from decompiled export.
		/// </summary>
		Level1,
		/// <summary>
		/// This level is the default. It exports full methods for Mono games and dummy methods for IL2Cpp games.
		/// </summary>
		Level2,
		/// <summary>
		/// IL2Cpp methods are safely recovered where possible.
		/// </summary>
		Level3,
		/// <summary>
		/// IL2Cpp methods are recovered without regard to safety.
		/// </summary>
		Level4,
	}
}
