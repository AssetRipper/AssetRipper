namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorSettings.bindings.cs"/>
	/// </summary>
	[Flags]
	public enum EnterPlayModeOptions
	{
		None = 0,
		DisableDomainReload = 1,
		DisableSceneReload = 2,
	}
}
