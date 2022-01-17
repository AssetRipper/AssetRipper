namespace AssetRipper.Core.Classes.QualitySettings
{
	/// <summary>
	/// Determines which type of shadows should be used.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ShadowQuality
	{
		/// <summary>
		/// Disable Shadows.
		/// </summary>
		Disable = 0,
		/// <summary>
		/// Hard Shadows Only.
		/// </summary>
		HardOnly = 1,
		/// <summary>
		/// Hard and Soft Shadows.
		/// </summary>
		All = 2,
	}
}
