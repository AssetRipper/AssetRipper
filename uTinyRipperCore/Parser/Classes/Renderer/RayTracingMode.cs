namespace uTinyRipper.Classes.Renderers
{
	/// <summary>
	/// Indicates how a Renderer is updated
	/// </summary>
	public enum RayTracingMode
	{
		/// <summary>
		/// Renderers with this mode are not ray traced
		/// </summary>
		Off					= 0,
		/// <summary>
		/// Renderers with this mode never update
		/// </summary>
		Static				= 1,
		/// <summary>
		/// Renderers with this mode update their Transform, but not their Mesh
		/// </summary>
		DynamicTransform	= 2,
		/// <summary>
		/// Renderers with this mode have animated geometry and update their Mesh and Transform
		/// </summary>
		DynamicGeometry		= 3,
	}
}
