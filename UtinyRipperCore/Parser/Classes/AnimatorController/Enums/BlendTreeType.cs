namespace UtinyRipper.Classes.AnimatorControllers
{
	/// <summary>
	/// The type of blending algorithm that the blend tree uses.
	/// </summary>
	public enum BlendTreeType
	{
		/// <summary>
		/// Basic blending using a single parameter.
		/// </summary>
		Simple1D,
		/// <summary>
		/// Best used when your motions represent different directions, such as "walk forward", "walk backward", "walk left", and "walk right", or "aim up", "aim down", "aim left", and "aim right".
		/// </summary>
		SimpleDirectional2D,
		/// <summary>
		/// This blend type is used when your motions represent different directions, however you can have multiple motions in the same direction, for example "walk forward" and "run forward".
		/// </summary>
		FreeformDirectional2D,
		/// <summary>
		/// Best used when your motions do not represent different directions.
		/// </summary>
		FreeformCartesian2D,
		/// <summary>
		/// Direct control of blending weight for each node.
		/// </summary>
		Direct
	}
}
