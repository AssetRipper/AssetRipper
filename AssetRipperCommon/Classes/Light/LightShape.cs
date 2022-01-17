namespace AssetRipper.Core.Classes.Light
{
	/// <summary>
	/// Describes the shape of a spot light.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Graphics/GraphicsEnums.cs"/>
	/// </summary>
	public enum LightShape
	{
		/// <summary>
		/// The shape of the spot light resembles a cone. This is the default shape for spot lights.
		/// </summary>
		Cone = 0,
		/// <summary>
		/// The shape of the spotlight resembles a pyramid or frustum. You can use this to simulate a screening or barn door effect on a normal spotlight.
		/// </summary>
		Pyramid = 1,
		/// <summary>
		/// The shape of the spot light resembles a box oriented along the ray direction.
		/// </summary>
		Box = 2,
	}
}
