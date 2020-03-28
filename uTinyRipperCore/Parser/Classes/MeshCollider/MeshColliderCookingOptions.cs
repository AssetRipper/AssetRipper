using System;

namespace uTinyRipper.Classes.MeshColliders
{
	/// <summary>
	/// Cooking options that are available with MeshCollider
	/// </summary>
	[Flags]
	public enum MeshColliderCookingOptions
	{
		/// <summary>
		/// No optional cooking steps will be run.
		/// </summary>
		None					= 0x0,
		/// <summary>
		/// Allow the physics engine to increase the volume of the input mesh in attempt to generate a valid convex mesh.
		/// </summary>
		InflateConvexMesh		= 0x1,
		/// <summary>
		/// Toggle between cooking for faster simulation or faster cooking time.
		/// </summary>
		CookForFasterSimulation = 0x2,
		/// <summary>
		/// Toggle cleaning of the mesh.
		/// </summary>
		EnableMeshCleaning		= 0x4,
		/// <summary>
		/// Toggle the removal of equal vertices.
		/// </summary>
		WeldColocatedVertices	= 0x8,
		/// <summary>
		/// Determines whether to use the fast midphase structure that doesn't require R-trees (only available on Desktop targets).
		/// </summary>
		UseFastMidphase			= 0x10,
	}
}
