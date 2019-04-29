using System;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// Options to control the optimization of mesh data during asset import
	/// </summary>
	[Flags]
	public enum MeshOptimizationFlags
	{
		/// <summary>
		/// Optimize the order of polygons in the mesh to make better use of the GPUs internal caches to improve rendering performance
		/// </summary>
		PolygonOrder	= 1,
		/// <summary>
		/// Optimize the order of vertices in the mesh to make better use of the GPUs internal caches to improve rendering performance
		/// </summary>
		VertexOrder		= 2,
		/// <summary>
		/// Perform maximum optimization of the mesh data, enables all optimization options
		/// </summary>
		Everything		= -1,
	}
}
