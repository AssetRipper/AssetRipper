using System;

namespace uTinyRipper.Classes.MeshColliders
{
	/// <summary>
	/// Cooking options that are available with MeshCollider
	/// </summary>
	[Flags]
	public enum MeshColliderCookingOptions
	{
		None					= 0,
		InflateConvexMesh		= 1,
		CookForFasterSimulation = 2,
		EnableMeshCleaning		= 4,
		WeldColocatedVertices	= 8,
	}
}
