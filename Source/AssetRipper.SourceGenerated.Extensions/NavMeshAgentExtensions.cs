using AssetRipper.SourceGenerated.Classes.ClassID_195;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class NavMeshAgentExtensions
{
	public static ObstacleAvoidanceType GetObstacleAvoidanceType(this INavMeshAgent agent)
	{
		return (ObstacleAvoidanceType)agent.ObstacleAvoidanceType;
	}
}
