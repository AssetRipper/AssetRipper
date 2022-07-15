namespace AssetRipper.Core.Classes.Light
{
	/// <summary>
	/// Enum describing what lighting mode to be used with Mixed lights.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum MixedLightingMode
	{
		IndirectOnly = 0,
		Subtractive = 1,
		Shadowmask = 2,
	}
}
