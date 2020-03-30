namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// Topology of Mesh faces.
	/// </summary>
	public enum MeshTopology
	{
		/// <summary>
		/// Mesh is made from triangles.
		/// </summary>
		Triangles		= 0,
		/// <summary>
		/// Mesh is a triangle strip.
		/// </summary>
		TriangleStrip = 1,
		/// <summary>
		/// Mesh is made from quads.
		/// </summary>
		Quads			= 2,
		/// <summary>
		/// Mesh is made from lines.
		/// </summary>
		Lines			= 3,
		/// <summary>
		/// Mesh is a line strip.
		/// </summary>
		LineStrip		= 4,
		/// <summary>
		/// Mesh is made from points.
		/// </summary>
		Points			= 5,
	}
}
