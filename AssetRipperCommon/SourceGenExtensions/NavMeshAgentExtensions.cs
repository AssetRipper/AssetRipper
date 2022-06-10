using AssetRipper.Core.Classes.NavMeshAgent;
using AssetRipper.SourceGenerated.Classes.ClassID_195;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class NavMeshAgentExtensions
	{
		public static ObstacleAvoidanceType GetObstacleAvoidanceType(this INavMeshAgent agent)
		{
			return (ObstacleAvoidanceType)agent.ObstacleAvoidanceType_C195;
		}
	}
}
