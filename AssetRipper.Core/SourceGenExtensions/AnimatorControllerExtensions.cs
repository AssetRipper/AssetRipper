using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorState;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimationClip_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_State_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Transition_;
using AssetRipper.SourceGenerated.Subclasses.StateBehavioursPair;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateKey;
using AssetRipper.SourceGenerated.Subclasses.StateMachineBehaviourVectorDescription;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMotionPair;
using AssetRipper.SourceGenerated.Subclasses.StateRange;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorControllerExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimatorController controller, IAnimationClip clip)
		{
			foreach (IPPtr_AnimationClip_ clipPtr in controller.AnimationClips_C91)
			{
				if (clipPtr.IsAsset(controller.Collection, clip))
				{
					return true;
				}
			}
			return false;
		}

		public static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(this IAnimatorController controller, int layerIndex)
		{
			if (controller.Has_StateMachineBehaviourVectorDescription_C91())
			{
				uint layerID = controller.Controller_C91.LayerArray[layerIndex].Data.Binding;
				StateKey key = new();
				key.SetValues(layerIndex, layerID);
				if (controller.StateMachineBehaviourVectorDescription_C91.StateMachineBehaviourRanges.TryGetValue(key, out StateRange? range))
				{
					return GetStateBehaviours(controller.StateMachineBehaviourVectorDescription_C91, controller.StateMachineBehaviours_C91!, range);
				}
			}
			return Array.Empty<PPtr_MonoBehaviour__5_0_0_f4>();
		}

		public static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(this IAnimatorController controller, int stateMachineIndex, int stateIndex)
		{
			if (controller.Has_StateMachineBehaviourVectorDescription_C91())
			{
				int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;
				IStateConstant state = stateMachine.StateConstantArray[stateIndex].Data;
				uint stateID = GetIdForStateConstant(state);
				StateKey key = new();
				key.SetValues(layerIndex, stateID);
				if (controller.StateMachineBehaviourVectorDescription_C91.StateMachineBehaviourRanges.TryGetValue(key, out StateRange? range))
				{
					return GetStateBehaviours(controller.StateMachineBehaviourVectorDescription_C91, controller.StateMachineBehaviours_C91!, range);
				}
			}
			return Array.Empty<PPtr_MonoBehaviour__5_0_0_f4>();
		}

		private static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(
			IStateMachineBehaviourVectorDescription controllerStateMachineBehaviourVectorDescription,
			AssetList<PPtr_MonoBehaviour__5_0_0_f4> controllerStateMachineBehaviours,
			StateRange range)
		{
			PPtr_MonoBehaviour__5_0_0_f4[] stateMachineBehaviours = new PPtr_MonoBehaviour__5_0_0_f4[range.Count];
			for (int i = 0; i < range.Count; i++)
			{
				int index = (int)controllerStateMachineBehaviourVectorDescription.StateMachineBehaviourIndices[range.StartIndex + i];
				stateMachineBehaviours[i] = controllerStateMachineBehaviours[index];
			}
			return stateMachineBehaviours;
		}

		private static uint GetIdForStateConstant(IStateConstant stateConstant)
		{
			if (stateConstant.Has_FullPathID())
			{
				return stateConstant.FullPathID;
			}
			else if (stateConstant.Has_NameID())
			{
				return stateConstant.NameID;
			}
			else
			{
				return stateConstant.ID;
			}
		}

		public static IEnumerable<IUnityObjectBase?> FetchEditorHierarchy(this IAnimatorController animatorController)
		{
			yield return animatorController;

			foreach (IAnimatorControllerLayer layer in animatorController.AnimatorLayers_C91)
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
						foreach (PPtr_MonoBehaviour__5_0_0_f4 stateMachineBehaviour in pair.StateMachineBehaviours)
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
				IAnimatorStateMachine? stateMachine = layer.StateMachine_PPtr_StateMachine_?.TryGetAsset(animatorController.Collection)
					?? layer.StateMachine_PPtr_AnimatorStateMachine_?.TryGetAsset(animatorController.Collection);
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
			if (stateMachine.Has_ChildStateMachines_C1107())
			{
				foreach (IAnimatorStateTransition? anyStateTransition in stateMachine.AnyStateTransitions_C1107P)
				{
					yield return anyStateTransition;
				}
				foreach (ChildAnimatorStateMachine childAnimatorStateMachine in stateMachine.ChildStateMachines_C1107)
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
				foreach (ChildAnimatorState childState in stateMachine.ChildStates_C1107)
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
				foreach (IAnimatorTransition? entryTransition in stateMachine.EntryTransitions_C1107P)
				{
					yield return entryTransition;
				}
				foreach (IMonoBehaviour? behaviour in stateMachine.StateMachineBehaviours_C1107P)
				{
					yield return behaviour;
				}
				foreach (AssetList<PPtr_AnimatorTransition_> list in stateMachine.StateMachineTransitions_C1107.Values)
				{
					//Skipping keys because they're IAnimatorStateMachine
					foreach (PPtr_AnimatorTransition_ transition in list)
					{
						yield return transition.TryGetAsset(stateMachine.Collection);
					}
				}
			}
			else
			{
				foreach (IAnimatorStateMachine? childStateMachine in stateMachine.ChildStateMachine_C1107P)
				{
					if (childStateMachine is not null)
					{
						foreach (IUnityObjectBase? reference in childStateMachine.FetchEditorHierarchy())
						{
							yield return reference;
						}
					}
				}
				if (stateMachine.Has_LocalTransitions_C1107())
				{
					foreach ((PPtr_State_ statePPtr, AssetList<PPtr_Transition_> list) in stateMachine.LocalTransitions_C1107)
					{
						IAnimatorState? state = statePPtr.TryGetAsset(stateMachine.Collection);
						if (state is not null)
						{
							foreach (IUnityObjectBase? reference in state.FetchHierarchy())
							{
								yield return reference;
							}
						}
						foreach (PPtr_Transition_ transition in list)
						{
							yield return transition.TryGetAsset(stateMachine.Collection);
						}
					}
				}
				foreach ((PPtr_State_ statePPtr, AssetList<PPtr_Transition_> list) in stateMachine.OrderedTransitions_C1107)
				{
					IAnimatorState? state = statePPtr.TryGetAsset(stateMachine.Collection);
					if (state is not null)
					{
						foreach (IUnityObjectBase? reference in state.FetchHierarchy())
						{
							yield return reference;
						}
					}
					foreach (PPtr_Transition_ transition in list)
					{
						yield return transition.TryGetAsset(stateMachine.Collection);
					}
				}
				foreach (IAnimatorState? state in stateMachine.States_C1107P)
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
			foreach (IChildMotion childMotion in blendTree.Childs_C206)
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

			if (state.Has_Motions_C1102())
			{
				foreach (Motion? motion in state.Motions_C1102P)
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
				if (state.Motion_C1102P is IBlendTree blendTree)
				{
					foreach (IUnityObjectBase? reference in blendTree.FetchHierarchy())
					{
						yield return reference;
					}
				}
			}
			//Ignoring ParentStateMachine
			if (state.Has_StateMachineBehaviours_C1102())
			{
				foreach (IMonoBehaviour? behaviour in state.StateMachineBehaviours_C1102P)
				{
					yield return behaviour;
				}
			}
			if (state.Has_Transitions_C1102())
			{
				foreach (IAnimatorStateTransition? transition in state.Transitions_C1102P)
				{
					yield return transition;
				}
			}
		}
	}
}
