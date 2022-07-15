using AssetRipper.Core.Classes.PhysicsManager;
using AssetRipper.SourceGenerated.Classes.ClassID_55;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PhysicsManagerExtensions
	{
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
