using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour;
using AssetRipper.SourceGenerated.Subclasses.StateBehavioursPair;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StateBehavioursPairExtensions
{
	public static void SetValues(this IStateBehavioursPair pair, IAnimatorController controller, IAnimatorState state, IMonoBehaviour?[] behaviours)
	{
		if (state == null)
		{
			throw new ArgumentNullException(nameof(state));
		}
		if (behaviours == null || behaviours.Length == 0)
		{
			throw new ArgumentNullException(nameof(behaviours));
		}

		pair.State.SetAsset(controller.Collection, state);

		pair.StateMachineBehaviours.Clear();
		pair.StateMachineBehaviours.Capacity = behaviours.Length;
		new PPtrAccessList<PPtr_MonoBehaviour_5, IMonoBehaviour>(pair.StateMachineBehaviours, controller.Collection).AddRange(behaviours);
	}
}
