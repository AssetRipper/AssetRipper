using AssetRipper.SourceGenerated.Classes.ClassID_1107;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimatorStateMachineExtensions
{
	public static void SetChildStateCapacity(this IAnimatorStateMachine stateMachine, int c)
	{
		if (stateMachine.Has_ChildStates())
		{
			stateMachine.ChildStates.Capacity = c;
		}
		else
		{
			stateMachine.States.Capacity = c;
		}
	}

	public static void SetChildStateMachineCapacity(this IAnimatorStateMachine stateMachine, int c)
	{
		if (stateMachine.Has_ChildStateMachines())
		{
			stateMachine.ChildStateMachines.Capacity = c;
		}
		else
		{
			stateMachine.ChildStateMachine.Capacity = c;
			stateMachine.ChildStateMachinePosition.Capacity = c;
		}
	}

	public static void TrimChildStateMachines(this IAnimatorStateMachine stateMachine)
	{
		stateMachine.SetChildStateMachineCapacity(stateMachine.ChildStateMachinesCount());
	}

	public static void SetEntryTransitionsCapacity(this IAnimatorStateMachine stateMachine, int c)
	{
		if (stateMachine.Has_EntryTransitions())
		{
			stateMachine.EntryTransitions.Capacity = c;
		}
	}

	public static int ChildStatesCount(this IAnimatorStateMachine stateMachine)
	{
		if (stateMachine.Has_ChildStates())
		{
			return stateMachine.ChildStates.Count;
		}
		return stateMachine.StatesP.Count;
	}

	public static int ChildStateMachinesCount(this IAnimatorStateMachine stateMachine)
	{
		if (stateMachine.Has_ChildStateMachines())
		{
			return stateMachine.ChildStateMachines.Count;
		}
		return stateMachine.ChildStateMachine.Count;
	}
}
