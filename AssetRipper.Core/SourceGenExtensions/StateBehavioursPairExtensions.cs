using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Subclasses.StateBehavioursPair;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StateBehavioursPairExtensions
	{
		public static void SetValues(this IStateBehavioursPair pair, IAnimatorState state, IMonoBehaviour[] behaviours)
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}
			if (behaviours == null || behaviours.Length == 0)
			{
				throw new ArgumentNullException(nameof(behaviours));
			}

			pair.State.CopyValues(state.SerializedFile.CreatePPtr(state));

			pair.StateMachineBehaviours.Clear();
			pair.StateMachineBehaviours.Capacity = behaviours.Length;
			for (int i = 0; i < behaviours.Length; i++)
			{
				IMonoBehaviour behaviour = behaviours[i];
				pair.StateMachineBehaviours.AddNew().CopyValues(behaviour.SerializedFile.CreatePPtr(behaviour));
			}
		}
	}
}
