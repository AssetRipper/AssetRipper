using AssetRipper.SourceGenerated.Subclasses.PhysicsJobOptions2D;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PhysicsJobOptions2DExtensions
{
	public static void SetToDefault(this IPhysicsJobOptions2D options)
	{
		options.UseMultithreading = false;
		options.UseConsistencySorting = false;
		options.InterpolationPosesPerJob = 100;
		options.NewContactsPerJob = 30;
		options.CollideContactsPerJob = 100;
		options.ClearFlagsPerJob = 200;
		options.ClearBodyForcesPerJob = 200;
		options.SyncDiscreteFixturesPerJob = 50;
		options.SyncContinuousFixturesPerJob = 50;
		options.FindNearestContactsPerJob = 100;
		options.UpdateTriggerContactsPerJob = 100;
		options.IslandSolverCostThreshold = 100;
		options.IslandSolverBodyCostScale = 1;
		options.IslandSolverContactCostScale = 10;
		options.IslandSolverJointCostScale = 10;
		options.IslandSolverBodiesPerJob = 50;
		options.IslandSolverContactsPerJob = 50;
	}
}
