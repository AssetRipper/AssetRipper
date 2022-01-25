namespace AssetRipper.Core.Classes.QualitySettings
{
	/// <summary>
	/// Shadow projection type for Quality Settings.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum ShadowProjection
	{
		/// <summary>
		/// Close fit shadow maps with linear fadeout.
		/// </summary>
		CloseFit = 0,
		/// <summary>
		/// Stable shadow maps with spherical fadeout.
		/// </summary>
		StableFit = 1,
	}
}
