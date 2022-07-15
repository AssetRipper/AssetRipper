namespace AssetRipper.Core.Classes.Light
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum LightType
	{
		Spot = 0,
		Directional = 1,
		Point = 2,
		/// <summary>
		/// Replaced with Rectangle
		/// </summary>
		Area = 3,
		Rectangle = 3,
		Disc = 4,
	}
}
