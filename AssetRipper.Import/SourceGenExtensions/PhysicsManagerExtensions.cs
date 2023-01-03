using AssetRipper.SourceGenerated.Classes.ClassID_55;

namespace AssetRipper.Core.SourceGenExtensions
{
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
			if (manager.Has_ContactsGeneration_C55())
			{
				return (ContactsGeneration)manager.ContactsGeneration_C55;
			}
			else
			{
				return manager.EnablePCM_C55 ? ContactsGeneration.PersistentContactManifold : ContactsGeneration.LegacyContactsGeneration;
			}
		}

		public static ContactPairsMode GetContactPairsMode(this IPhysicsManager manager)
		{
			return (ContactPairsMode)manager.ContactPairsMode_C55;
		}

		public static BroadphaseType GetBroadphaseType(this IPhysicsManager manager)
		{
			return (BroadphaseType)manager.BroadphaseType_C55;
		}

		public static FrictionType GetFrictionType(this IPhysicsManager manager)
		{
			return (FrictionType)manager.FrictionType_C55;
		}

		public static SolverType GetSolverType(this IPhysicsManager manager)
		{
			return (SolverType)manager.SolverType_C55;
		}
	}
}
