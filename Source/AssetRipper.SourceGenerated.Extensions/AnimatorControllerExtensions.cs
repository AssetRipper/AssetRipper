using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorState;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimationClip;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorState;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateTransition;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour;
using AssetRipper.SourceGenerated.Subclasses.StateBehavioursPair;
using AssetRipper.SourceGenerated.Subclasses.StateKey;
using AssetRipper.SourceGenerated.Subclasses.StateMachineBehaviourVectorDescription;
using AssetRipper.SourceGenerated.Subclasses.StateMotionPair;
using AssetRipper.SourceGenerated.Subclasses.StateRange;

namespace AssetRipper.SourceGenerated.Extensions;

public static class AnimatorControllerExtensions
{
	public static bool ContainsAnimationClip(this IAnimatorController controller, IAnimationClip clip)
	{
		foreach (IPPtr_AnimationClip clipPtr in controller.AnimationClips)
		{
			if (clipPtr.IsAsset(controller.Collection, clip))
			{
				return true;
			}
		}
		return false;
	}

	public static IMonoBehaviour?[] GetStateBehaviours(this IAnimatorController controller, int layerIndex, uint stateID)
	{
		if (controller.Has_StateMachineBehaviourVectorDescription())
		{
			StateKey key = new();
			key.SetValues(layerIndex, stateID);
			if (controller.StateMachineBehaviourVectorDescription.StateMachineBehaviourRanges.TryGetValue(key, out StateRange? range))
			{
				return GetStateBehaviours(controller.StateMachineBehaviourVectorDescription, controller.StateMachineBehavioursP, range);
			}
		}
		return Array.Empty<IMonoBehaviour>();
	}

	private static IMonoBehaviour?[] GetStateBehaviours(
		IStateMachineBehaviourVectorDescription controllerStateMachineBehaviourVectorDescription,
		PPtrAccessList<PPtr_MonoBehaviour_5, IMonoBehaviour> controllerStateMachineBehaviours,
		StateRange range)
	{
		IMonoBehaviour?[] stateMachineBehaviours = new IMonoBehaviour?[range.Count];
		for (int i = 0; i < range.Count; i++)
		{
			int index = (int)controllerStateMachineBehaviourVectorDescription.StateMachineBehaviourIndices[(int)range.StartIndex + i];
			IMonoBehaviour? stateMachineBehaviour = controllerStateMachineBehaviours[index];
			if (stateMachineBehaviour != null)
			{
				stateMachineBehaviour.HideFlagsE = HideFlags.HideInHierarchy;
			}
			stateMachineBehaviours[i] = stateMachineBehaviour;
		}
		return stateMachineBehaviours;
	}

	public static IEnumerable<IUnityObjectBase?> FetchEditorHierarchy(this IAnimatorController animatorController)
	{
		yield return animatorController;

		foreach (IAnimatorControllerLayer layer in animatorController.AnimatorLayers)
		{
			//Ignoring layer.Controller, layer.Mask, and layer.SkeletonMask
			if (layer.Has_Behaviours())
			{
				foreach (IStateBehavioursPair pair in layer.Behaviours)
				{
					IAnimatorState? state = pair.State.TryGetAsset(animatorController.Collection);
					if (state is not null)
					{
						foreach (IUnityObjectBase? reference in state.FetchHierarchy())
						{
							yield return reference;
						}
					}
					foreach (PPtr_MonoBehaviour_5 stateMachineBehaviour in pair.StateMachineBehaviours)
					{
						yield return stateMachineBehaviour.TryGetAsset(animatorController.Collection);
					}
				}
				foreach (IStateMotionPair pair in layer.Motions)
				{
					IAnimatorState? state = pair.State.TryGetAsset(animatorController.Collection);
					if (state is not null)
					{
						foreach (IUnityObjectBase? reference in state.FetchHierarchy())
						{
							yield return reference;
						}
					}

					//AnimationClip also inherits from Motion, but we don't want to include that.
					IBlendTree? blendTree = pair.Motion.TryGetAsset(animatorController.Collection) as IBlendTree;
					if (blendTree is not null)
					{
						foreach (IUnityObjectBase? reference in blendTree.FetchHierarchy())
						{
							yield return reference;
						}
					}
				}
			}
			IAnimatorStateMachine? stateMachine = layer.StateMachine.TryGetAsset(animatorController.Collection);
			if (stateMachine is not null)
			{
				foreach (IUnityObjectBase? reference in stateMachine.FetchEditorHierarchy())
				{
					yield return reference;
				}
			}
		}
		//Ignoring animatorController.AnimatorParameters.Controller
		//It has no other PPtr's.
	}

	private static IEnumerable<IUnityObjectBase?> FetchEditorHierarchy(this IAnimatorStateMachine stateMachine)
	{
		yield return stateMachine;
		if (stateMachine.Has_ChildStateMachines())
		{
			foreach (IAnimatorStateTransition? anyStateTransition in stateMachine.AnyStateTransitionsP)
			{
				yield return anyStateTransition;
			}
			foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachine.ChildStateMachines)
			{
				IAnimatorStateMachine? childStateMachine = childAnimatorStateMachine.StateMachine.TryGetAsset(stateMachine.Collection);
				if (childStateMachine is not null)
				{
					foreach (IUnityObjectBase? reference in childStateMachine.FetchEditorHierarchy())
					{
						yield return reference;
					}
				}
			}
			foreach (ChildAnimatorState childState in stateMachine.ChildStates)
			{
				IAnimatorState? state = childState.State.TryGetAsset(stateMachine.Collection);
				if (state is not null)
				{
					foreach (IUnityObjectBase? reference in state.FetchHierarchy())
					{
						yield return reference;
					}
				}
			}
			foreach (IAnimatorTransition? entryTransition in stateMachine.EntryTransitionsP)
			{
				yield return entryTransition;
			}
			foreach (IMonoBehaviour? behaviour in stateMachine.StateMachineBehavioursP)
			{
				yield return behaviour;
			}
			foreach (AssetList<PPtr_AnimatorTransition> list in stateMachine.StateMachineTransitions.Values)
			{
				//Skipping keys because they're IAnimatorStateMachine
				foreach (PPtr_AnimatorTransition transition in list)
				{
					yield return transition.TryGetAsset(stateMachine.Collection);
				}
			}
		}
		else
		{
			foreach (IAnimatorStateMachine? childStateMachine in stateMachine.ChildStateMachineP)
			{
				if (childStateMachine is not null)
				{
					foreach (IUnityObjectBase? reference in childStateMachine.FetchEditorHierarchy())
					{
						yield return reference;
					}
				}
			}
			if (stateMachine.Has_LocalTransitions())
			{
				foreach ((PPtr_AnimatorState_4 statePPtr, AssetList<PPtr_AnimatorStateTransition_4> list) in stateMachine.LocalTransitions)
				{
					IAnimatorState? state = statePPtr.TryGetAsset(stateMachine.Collection);
					if (state is not null)
					{
						foreach (IUnityObjectBase? reference in state.FetchHierarchy())
						{
							yield return reference;
						}
					}
					foreach (PPtr_AnimatorStateTransition_4 transition in list)
					{
						yield return transition.TryGetAsset(stateMachine.Collection);
					}
				}
			}
			foreach ((PPtr_AnimatorState_4 statePPtr, AssetList<PPtr_AnimatorStateTransition_4> list) in stateMachine.OrderedTransitions)
			{
				IAnimatorState? state = statePPtr.TryGetAsset(stateMachine.Collection);
				if (state is not null)
				{
					foreach (IUnityObjectBase? reference in state.FetchHierarchy())
					{
						yield return reference;
					}
				}
				foreach (PPtr_AnimatorStateTransition_4 transition in list)
				{
					yield return transition.TryGetAsset(stateMachine.Collection);
				}
			}
			foreach (IAnimatorState? state in stateMachine.StatesP)
			{
				if (state is not null)
				{
					foreach (IUnityObjectBase? reference in state.FetchHierarchy())
					{
						yield return reference;
					}
				}
			}
		}
		//Ignoring DefaultState because redundant
	}

	private static IEnumerable<IUnityObjectBase?> FetchHierarchy(this IBlendTree blendTree)
	{
		yield return blendTree;
		foreach (IChildMotion childMotion in blendTree.Childs)
		{
			//AnimationClips are excluded from the hierarchy
			IBlendTree? child = childMotion.Motion.TryGetAsset(blendTree.Collection) as IBlendTree;
			if (child is not null)
			{
				foreach (IUnityObjectBase? reference in child.FetchHierarchy())
				{
					yield return reference;
				}
			}
		}
	}

	private static IEnumerable<IUnityObjectBase?> FetchHierarchy(this IAnimatorState state)
	{
		yield return state;

		if (state.Has_Motions())
		{
			foreach (IMotion? motion in state.MotionsP)
			{
				if (motion is IBlendTree blendTree)
				{
					foreach (IUnityObjectBase? reference in blendTree.FetchHierarchy())
					{
						yield return reference;
					}
				}
			}
		}
		else
		{
			if (state.MotionP is IBlendTree blendTree)
			{
				foreach (IUnityObjectBase? reference in blendTree.FetchHierarchy())
				{
					yield return reference;
				}
			}
		}
		//Ignoring ParentStateMachine
		if (state.Has_StateMachineBehaviours())
		{
			foreach (IMonoBehaviour? behaviour in state.StateMachineBehavioursP)
			{
				yield return behaviour;
			}
		}
		if (state.Has_Transitions())
		{
			foreach (IAnimatorStateTransition? transition in state.TransitionsP)
			{
				yield return transition;
			}
		}
	}
}
