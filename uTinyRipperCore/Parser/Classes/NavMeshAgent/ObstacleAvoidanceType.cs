namespace uTinyRipper.Classes.NavMeshAgents
{
	/// <summary>
	/// Level of obstacle avoidance.
	/// </summary>
	public enum ObstacleAvoidanceType
	{
		/// <summary>
		/// Disable avoidance.
		/// </summary>
		NoObstacleAvoidance				= 0,
		/// <summary>
		/// Enable simple avoidance. Low performance impact.
		/// </summary>
		LowQualityObstacleAvoidance		= 1,
		/// <summary>
		/// Medium avoidance. Medium performance impact.
		/// </summary>
		MedQualityObstacleAvoidance		= 2,
		/// <summary>
		/// Good avoidance. High performance impact.
		/// </summary>
		GoodQualityObstacleAvoidance	= 3,
		/// <summary>
		/// Enable highest precision. Highest performance impact.
		/// </summary>
		HighQualityObstacleAvoidance	= 4
	}
}
