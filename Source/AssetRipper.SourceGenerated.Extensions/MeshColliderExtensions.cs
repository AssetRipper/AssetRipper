using AssetRipper.SourceGenerated.Classes.ClassID_64;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MeshColliderExtensions
{
	public static MeshColliderCookingOptions GetCookingOptions(this IMeshCollider collider)
	{
		if (collider.Has_CookingOptions())
		{
			return (MeshColliderCookingOptions)collider.CookingOptions;
		}
		else
		{
			MeshColliderCookingOptions options =
				MeshColliderCookingOptions.CookForFasterSimulation |
				MeshColliderCookingOptions.EnableMeshCleaning |
				MeshColliderCookingOptions.WeldColocatedVertices;

			if (collider.InflateMesh)
			{
				options |= MeshColliderCookingOptions.InflateConvexMesh;
			}
			return options;
		}
	}
}
