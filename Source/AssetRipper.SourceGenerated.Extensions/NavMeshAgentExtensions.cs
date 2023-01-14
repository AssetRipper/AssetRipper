using AssetRipper.SourceGenerated.Classes.ClassID_195;
using ObstacleAvoidanceType = AssetRipper.SourceGenerated.Enums.ObstacleAvoidanceType_1;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class NavMeshAgentExtensions
	{
		public static ObstacleAvoidanceType GetObstacleAvoidanceType(this INavMeshAgent agent)
		{
			return (ObstacleAvoidanceType)agent.ObstacleAvoidanceType_C195;
		}
	}
}
