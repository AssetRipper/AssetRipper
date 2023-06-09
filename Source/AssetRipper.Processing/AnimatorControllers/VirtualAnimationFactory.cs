using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.AnimatorCondition;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorState;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.ConditionConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AssetRipper.SourceGenerated.Subclasses.LeafInfoConstant;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_SelectorStateConstant;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorState;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour;
using AssetRipper.SourceGenerated.Subclasses.SelectorStateConstant;
using AssetRipper.SourceGenerated.Subclasses.SelectorTransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;

namespace AssetRipper.Processing.AnimatorControllers
{
	public static class VirtualAnimationFactory
	{
		private static IMotion? CreateMotion(this IStateConstant stateConstant, ProcessedAssetCollection file, IAnimatorController controller, int nodeIndex)
		{
			if (stateConstant.BlendTreeConstantArray.Count == 0)
			{
				return default;
			}
			else
			{
				IBlendTreeNodeConstant node = stateConstant.GetBlendTree().NodeArray[nodeIndex].Data;
				if (node.IsBlendTree())
				{
					return CreateBlendTree(file, controller, stateConstant, nodeIndex);
				}
				else
				{
					int clipIndex = -1;
					if (stateConstant.Has_LeafInfoArray())
					{
						for (int i = 0; i < stateConstant.LeafInfoArray.Count; i++)
						{
							LeafInfoConstant leafInfo = stateConstant.LeafInfoArray[i];
							int index = leafInfo.IDArray.IndexOf(node.ClipID);
							if (index >= 0)
							{
								clipIndex = (int)leafInfo.IndexOffset + index;
								break;
							}
						}
					}
					else
					{
						clipIndex = unchecked((int)node.ClipID);
					}

					if (clipIndex == -1)
					{
						return default;
					}
					else
					{
						return controller.AnimationClips_C91P[clipIndex] as IMotion;//AnimationClip has inherited from Motion since Unity 4.
					}
				}
			}
		}

		private static IBlendTree CreateBlendTree(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateConstant state, int nodeIndex)
		{
			IBlendTree blendTree = virtualFile.CreateAsset((int)ClassIDType.BlendTree, BlendTreeFactory.CreateAsset);
			blendTree.ObjectHideFlags = HideFlags.HideInHierarchy;

			IBlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Data;

			blendTree.NameString = "BlendTree";

			blendTree.Childs_C206.Capacity = node.ChildIndices.Length;
			for (int i = 0; i < node.ChildIndices.Length; i++)
			{
				blendTree.AddAndInitializeNewChild(virtualFile, controller, state, nodeIndex, i);
			}

			if (node.BlendEventID != uint.MaxValue)
			{
				blendTree.BlendParameter_C206.CopyValues(controller.TOS_C91[node.BlendEventID]);
			}
			if (node.BlendEventYID != uint.MaxValue)
			{
				blendTree.BlendParameterY_C206?.CopyValues(controller.TOS_C91[node.BlendEventYID]);
			}
			blendTree.MinThreshold_C206 = node.GetMinThreshold();
			blendTree.MaxThreshold_C206 = node.GetMaxThreshold();
			blendTree.UseAutomaticThresholds_C206 = false;
			blendTree.NormalizedBlendValues_C206 = node.BlendDirectData?.Data.NormalizedBlendValues ?? false;
			if (blendTree.Has_BlendType_C206_Int32())
			{
				blendTree.BlendType_C206_Int32 = (int)node.BlendType;
			}
			else
			{
				blendTree.BlendType_C206_UInt32 = node.BlendType;
			}
			return blendTree;
		}

		private static IChildMotion AddAndInitializeNewChild(this IBlendTree tree, ProcessedAssetCollection file, IAnimatorController controller, IStateConstant state, int nodeIndex, int childIndex)
		{
			IChildMotion childMotion = tree.Childs_C206.AddNew();
			IBlendTreeConstant treeConstant = state.GetBlendTree();
			IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			IMotion? motion = state.CreateMotion(file, controller, childNodeIndex);
			childMotion.Motion.CopyValues(tree.Collection.ForceCreatePPtr(motion));

			childMotion.Threshold = node.GetThreshold(childIndex);
			childMotion.Position?.CopyValues(node.GetPosition(childIndex));
			childMotion.TimeScale = 1.0f;
			childMotion.CycleOffset = node.CycleOffset;

			if (node.TryGetDirectBlendParameter(childIndex, out uint directID))
			{
				childMotion.DirectBlendParameter?.CopyValues(controller.TOS_C91[directID]);
			}

			childMotion.Mirror = node.Mirror;

			return childMotion;
		}

		private static void CreateEntryTransitions(
			IAnimatorStateMachine generatedStateMachine,
			IStateMachineConstant stateMachineConstant,
			ProcessedAssetCollection file,
			uint ID,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS)
		{
			AssetList<PPtr_AnimatorTransition>? entryTransitionList = generatedStateMachine.EntryTransitions_C1107;
			if (entryTransitionList is not null && stateMachineConstant.Has_SelectorStateConstantArray())
			{
				foreach (OffsetPtr_SelectorStateConstant selectorPtr in stateMachineConstant.SelectorStateConstantArray)
				{
					SelectorStateConstant selector = selectorPtr;
					if (selector.FullPathID == ID && selector.IsEntry)
					{
						for (int i = 0; i < selector.TransitionConstantArray.Count - 1; i++)
						{
							SelectorTransitionConstant selectorTrans = selector.TransitionConstantArray[i].Data;
							IAnimatorTransition transition = CreateAnimatorTransition(file, stateMachineConstant, States, TOS, selectorTrans);
							entryTransitionList.AddNew().CopyValues(generatedStateMachine.Collection.ForceCreatePPtr(transition));
						}
					}
				}
			}
		}

		public static IAnimatorStateMachine CreateAnimatorStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			const float StateOffset = 250.0f;

			IAnimatorStateMachine generatedStateMachine = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateMachine, AnimatorStateMachineFactory.CreateAsset);
			generatedStateMachine.ObjectHideFlags = HideFlags.HideInHierarchy;

			int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			ILayerConstant layer = controller.Controller_C91.LayerArray[layerIndex].Data;
			generatedStateMachine.Name.CopyValues(controller.TOS_C91[layer.Binding]);

			IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;

			int stateCount = stateMachine.StateConstantArray.Count;
			int stateMachineCount = 0;
			int count = stateCount + stateMachineCount;
			int side = (int)Math.Ceiling(Math.Sqrt(count));

			List<IAnimatorState> states = new();
			if (generatedStateMachine.Has_ChildStates_C1107())
			{
				generatedStateMachine.ChildStates_C1107.Clear();
				generatedStateMachine.ChildStates_C1107.Capacity = stateCount;
			}
			else if (generatedStateMachine.Has_States_C1107())
			{
				generatedStateMachine.States_C1107.Clear();
				generatedStateMachine.States_C1107.Capacity = stateCount;
			}
			for (int y = 0, stateIndex = 0; y < side && stateIndex < stateCount; y++)
			{
				for (int x = 0; x < side && stateIndex < stateCount; x++, stateIndex++)
				{
					Vector3f position = new() { X = x * StateOffset, Y = y * StateOffset };
					IAnimatorState state = CreateAnimatorState(virtualFile, controller, stateMachineIndex, stateIndex, position);

					if (generatedStateMachine.Has_ChildStates_C1107())
					{
						ChildAnimatorState childState = generatedStateMachine.ChildStates_C1107.AddNew();
						childState.Position.CopyValues(position);
						childState.State.CopyValues(generatedStateMachine.Collection.ForceCreatePPtr(state));
					}
					else if (generatedStateMachine.Has_States_C1107())
					{
						generatedStateMachine.States_C1107.AddNew().CopyValues(generatedStateMachine.Collection.ForceCreatePPtr(state));
					}

					states.Add(state);
				}
			}

#warning TODO: child StateMachines
			//generatedStateMachine.ChildStateMachines_C1107 = new ChildAnimatorStateMachine[stateMachineCount];

			// set destination state for transitions here because all states has become valid only now
			for (int i = 0; i < stateMachine.StateConstantArray.Count; i++)
			{
				IAnimatorState state = states[i];
				IStateConstant stateConstant = stateMachine.StateConstantArray[i].Data;

				if (state.Has_Transitions_C1102())
				{
					state.Transitions_C1102.EnsureCapacity(state.Transitions_C1102.Count + stateConstant.TransitionConstantArray.Count);
				}

				for (int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
				{
					ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Data;
					IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachine, states, controller.TOS_C91, transitionConstant);
					if (state.Has_Transitions_C1102())
					{
						state.Transitions_C1102.AddNew().CopyValues(state.Collection.ForceCreatePPtr(transition));
					}
				}
			}

			if (generatedStateMachine.Has_AnyStateTransitions_C1107())
			{
				generatedStateMachine.AnyStateTransitions_C1107.Clear();
				generatedStateMachine.AnyStateTransitions_C1107.Capacity = stateMachine.AnyStateTransitionConstantArray.Count;
				for (int i = 0; i < stateMachine.AnyStateTransitionConstantArray.Count; i++)
				{
					ITransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Data;
					IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachine, states, controller.TOS_C91, transitionConstant);
					generatedStateMachine.AnyStateTransitions_C1107.AddNew().CopyValues(generatedStateMachine.Collection.ForceCreatePPtr(transition));
				}
			}

			CreateEntryTransitions(generatedStateMachine, stateMachine, virtualFile, layer.Binding, states, controller.TOS_C91);

			generatedStateMachine.StateMachineBehaviours_C1107?.Clear();
#warning TEMP: enable when AnimatorStateMachine's child StateMachines has been implemented
			if (false)
			{
				generatedStateMachine.StateMachineBehaviours_C1107P.AddRange(controller.GetStateBehaviours(layerIndex));
			}

			generatedStateMachine.AnyStatePosition_C1107.SetValues(0.0f, -StateOffset, 0.0f);
			generatedStateMachine.EntryPosition_C1107?.SetValues(StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ExitPosition_C1107?.SetValues(2.0f * StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ParentStateMachinePosition_C1107.SetValues(0.0f, -2.0f * StateOffset, 0.0f);

			if (generatedStateMachine.Has_ChildStates_C1107() && generatedStateMachine.ChildStates_C1107.Count > 0)
			{
				PPtr_AnimatorState_5_0_0 defaultStatePPtr = generatedStateMachine.ChildStates_C1107[(int)stateMachine.DefaultState].State;

				generatedStateMachine.DefaultState_C1107.CopyValues((PPtr<IAnimatorState>)defaultStatePPtr);
			}

			return generatedStateMachine;
		}

		private static IAnimatorState CreateAnimatorState(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position)
		{
			IAnimatorState generatedState = virtualFile.CreateAsset((int)ClassIDType.AnimatorState, AnimatorStateFactory.CreateAsset);
			generatedState.ObjectHideFlags = HideFlags.HideInHierarchy;

			AssetDictionary<uint, Utf8String> TOS = controller.TOS_C91;
			if (!TOS.ContainsKey(0))
			{
				AssetDictionary<uint, Utf8String> tos = new AssetDictionary<uint, Utf8String>() { { 0, new Utf8String() } };
				tos.AddRange(controller.TOS_C91);
				TOS = tos;
			}
			IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;
			IStateConstant state = stateMachine.StateConstantArray[stateIndex].Data;

			generatedState.Name.CopyValues(TOS[state.NameID]);

			generatedState.Speed_C1102 = state.Speed;
			generatedState.CycleOffset_C1102 = state.CycleOffset;

			// skip Transitions because not all state exists at this moment

			if (generatedState.Has_StateMachineBehaviours_C1102())
			{
				// exclude StateMachine's behaviours
				int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				IMonoBehaviour?[] machineBehaviours = controller.GetStateBehaviours(layerIndex);
				IMonoBehaviour?[] stateBehaviours = controller.GetStateBehaviours(stateMachineIndex, stateIndex);
				IMonoBehaviour?[] behaviours = stateBehaviours;
#warning TEMP: remove comment when AnimatorStateMachine's child StateMachines has been implemented
				//List<IMonoBehaviour?> behaviours = new List<IMonoBehaviour?>(stateBehaviours.Length);
				//foreach (IMonoBehaviour? behaviour in stateBehaviours)
				//{
				//	if (!machineBehaviours.Contains(behaviour))
				//	{
				//		behaviours.Add(behaviour);
				//	}
				//}

				generatedState.StateMachineBehaviours_C1102P.AddRange(behaviours);
			}

			generatedState.Position_C1102.CopyValues(position);
			generatedState.IKOnFeet_C1102 = state.IKOnFeet;
			generatedState.WriteDefaultValues_C1102 = state.GetWriteDefaultValues();
			generatedState.Mirror_C1102 = state.Mirror;
			generatedState.SpeedParameterActive_C1102 = state.SpeedParamID > 0;
			generatedState.MirrorParameterActive_C1102 = state.MirrorParamID > 0;
			generatedState.CycleOffsetParameterActive_C1102 = state.CycleOffsetParamID > 0;
			generatedState.TimeParameterActive_C1102 = state.TimeParamID > 0;

			generatedState.Motion_C1102P = state.CreateMotion(virtualFile, controller, 0);

			generatedState.Tag_C1102.CopyValues(TOS[state.TagID]);
			generatedState.SpeedParameter_C1102?.CopyValues(TOS[state.SpeedParamID]);
			generatedState.MirrorParameter_C1102?.CopyValues(TOS[state.MirrorParamID]);
			generatedState.CycleOffsetParameter_C1102?.CopyValues(TOS[state.CycleOffsetParamID]);
			generatedState.TimeParameter_C1102?.CopyValues(TOS[state.TimeParamID]);

			return generatedState;
		}

		private static IAnimatorStateTransition CreateAnimatorStateTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachine,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS,
			ITransitionConstant Transition)
		{
			IAnimatorStateTransition animatorStateTransition = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateTransition, AnimatorStateTransitionFactory.CreateAsset);
			animatorStateTransition.HideFlags_C1101 = (uint)HideFlags.HideInHierarchy;

			animatorStateTransition.Conditions_C1101.Capacity = Transition.ConditionConstantArray.Count;
			for (int i = 0; i < Transition.ConditionConstantArray.Count; i++)
			{
				ConditionConstant conditionConstant = Transition.ConditionConstantArray[i].Data;
				if (conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
				{
					IAnimatorCondition condition = animatorStateTransition.Conditions_C1101.AddNew();
					condition.ConditionMode = (int)conditionConstant.ConditionModeE;
					condition.ConditionEvent.CopyValues(TOS[conditionConstant.EventID]);
					condition.EventTreshold = conditionConstant.EventThreshold;
				}
			}

			animatorStateTransition.DstState_C1101P = GetDestinationState(Transition.DestinationState, StateMachine, States);

			animatorStateTransition.Name.CopyValues(TOS[Transition.UserID]);
			animatorStateTransition.IsExit_C1101 = Transition.IsExit();

			animatorStateTransition.TransitionDuration_C1101 = Transition.TransitionDuration;
			animatorStateTransition.TransitionOffset_C1101 = Transition.TransitionOffset;
			animatorStateTransition.ExitTime_C1101 = Transition.GetExitTime();
			animatorStateTransition.HasExitTime_C1101 = Transition.GetHasExitTime();
			animatorStateTransition.HasFixedDuration_C1101 = Transition.GetHasFixedDuration();
			animatorStateTransition.InterruptionSource_C1101E = Transition.GetInterruptionSource();
			animatorStateTransition.OrderedInterruption_C1101 = Transition.OrderedInterruption;
			animatorStateTransition.CanTransitionToSelf_C1101 = Transition.CanTransitionToSelf;

			return animatorStateTransition;
		}

		private static IAnimatorTransition CreateAnimatorTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachine,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS,
			SelectorTransitionConstant Transition)
		{
			IAnimatorTransition animatorTransition = virtualFile.CreateAsset((int)ClassIDType.AnimatorTransition, AnimatorTransitionFactory.CreateAsset);
			animatorTransition.HideFlags_C1109 = (uint)HideFlags.HideInHierarchy;

			animatorTransition.Conditions_C1109.Capacity = Transition.ConditionConstantArray.Count;
			for (int i = 0; i < Transition.ConditionConstantArray.Count; i++)
			{
				ConditionConstant conditionConstant = Transition.ConditionConstantArray[i].Data;
				if (conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
				{
					IAnimatorCondition condition = animatorTransition.Conditions_C1109.AddNew();
					condition.ConditionMode = (int)conditionConstant.ConditionModeE;
					condition.ConditionEvent.CopyValues(TOS[conditionConstant.EventID]);
					condition.EventTreshold = conditionConstant.EventThreshold;
				}
			}

			animatorTransition.DstState_C1109P = GetDestinationState(Transition.Destination, StateMachine, States);

			return animatorTransition;
		}

		private static IAnimatorState? GetDestinationState(uint destinationState, IStateMachineConstant stateMachine, IReadOnlyList<IAnimatorState> states)
		{
			if (destinationState == uint.MaxValue)
			{
				return null;
			}
			else if (destinationState >= 30000)
			{
				// Entry and Exit states
				uint stateIndex = destinationState % 30000;
				if (stateIndex == 0 || stateIndex == 1)
				{
					// base layer node. Default value is valid
					return null;
				}
				else if (stateMachine.Has_SelectorStateConstantArray())
				{
					SelectorStateConstant selectorState = stateMachine.SelectorStateConstantArray[(int)stateIndex].Data;
#warning		HACK: take default Entry destination. TODO: child StateMachines
					SelectorTransitionConstant selectorTransition = selectorState.TransitionConstantArray[^1].Data;
					return GetDestinationState(selectorTransition.Destination, stateMachine, states);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return states[(int)destinationState];
			}
		}
	}
}
