namespace AssetRipper.Core.Classes.QualitySettings
{
	/// <summary>
	/// Anisotropic filtering mode.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum AnisotropicFiltering
	{
		/// <summary>
		/// Disable anisotropic filtering for all textures.
		/// </summary>
		Disable = 0,
		/// <summary>
		/// Enable anisotropic filtering, as set for each texture.
		/// </summary>
		Enable = 1,
		/// <summary>
		/// Enable anisotropic filtering for all textures.
		/// </summary>
		ForceEnable = 2,
	}
}
