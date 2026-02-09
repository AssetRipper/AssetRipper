using AssetRipper.SourceGenerated.Classes.ClassID_55;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PhysicsManagerExtensions
{
	public enum BroadphaseType
	{
		SweepAndPruneBroadphase = 0,
		MultiboxPruningBroadphase = 1,
	}
	public enum ContactPairsMode
	{
		DefaultContactPairs = 0,
		EnableKinematicKinematicPairs = 1,
		EnableKinematicStaticPairs = 2,
		EnableAllContactPairs = 3,
	}
	public enum ContactsGeneration
	{
		LegacyContactsGeneration = 0,
		PersistentContactManifold = 1,
	}
	public enum FrictionType
	{
		Patch = 0,
		OneDirectional = 1,
		TwoDirectional = 2,
	}
	public enum SolverType
	{
		ProjectedGaussSiedel = 0,
		TemporalGaussSiedel = 1,
	}
	public static ContactsGeneration GetContactsGeneration(this IPhysicsManager manager)
	{
		if (manager.Has_ContactsGeneration())
		{
			return (ContactsGeneration)manager.ContactsGeneration;
		}
		else
		{
			return manager.EnablePCM ? ContactsGeneration.PersistentContactManifold : ContactsGeneration.LegacyContactsGeneration;
		}
	}

	public static ContactPairsMode GetContactPairsMode(this IPhysicsManager manager)
	{
		return (ContactPairsMode)manager.ContactPairsMode;
	}

	public static BroadphaseType GetBroadphaseType(this IPhysicsManager manager)
	{
		return (BroadphaseType)manager.BroadphaseType;
	}

	public static FrictionType GetFrictionType(this IPhysicsManager manager)
	{
		return (FrictionType)manager.FrictionType;
	}

	public static SolverType GetSolverType(this IPhysicsManager manager)
	{
		return (SolverType)manager.SolverType;
	}
}
