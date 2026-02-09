namespace AssetRipper.Import.Configuration;

public enum ScriptContentLevel
{
	/// <summary>
	/// Scripts are not loaded.
	/// </summary>
	Level0,
	/// <summary>
	/// Methods are stubbed during processing.
	/// </summary>
	Level1,
	/// <summary>
	/// This level is the default. It has full methods for Mono games and empty methods for IL2Cpp games.
	/// </summary>
	Level2,
	/// <summary>
	/// IL2Cpp methods are safely recovered where possible.
	/// </summary>
	Level3,
	/// <summary>
	/// IL2Cpp methods are recovered without regard to safety. Currently the same as <see cref="Level2"/>
	/// </summary>
	//Level4,
}
