using AssetRipper.Core.Classes.NavMeshObstacle;
using AssetRipper.SourceGenerated.Classes.ClassID_208;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class NavMeshObstacleExtensions
	{
		public static NavMeshObstacleShape GetShape(this INavMeshObstacle obstacle)
		{
			return obstacle.Has_Shape_C208()
				? (NavMeshObstacleShape)obstacle.Shape_C208
				: NavMeshObstacleShape.Capsule;
		}
	}
}
