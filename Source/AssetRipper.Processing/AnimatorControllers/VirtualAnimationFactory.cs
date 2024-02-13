using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Logging;
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
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateTransition;
using AssetRipper.SourceGenerated.Subclasses.SelectorStateConstant;
using AssetRipper.SourceGenerated.Subclasses.SelectorTransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;

namespace AssetRipper.Processing.AnimatorControllers
{
	public static class VirtualAnimationFactory
	{
		// Example of default BlendTree Name:
		// https://github.com/ds5678/Binoculars/blob/d6702ed3a1db39b1a2788956ff195b2590c3d08b/Unity/Assets/Models/binoculars_animator.controller#L106
		private static Utf8String BlendTreeName { get; } = new Utf8String("Blend Tree");

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
						return controller.AnimationClipsP[clipIndex] as IMotion;//AnimationClip has inherited from Motion since Unity 4.
					}
				}
			}
		}

		private static IBlendTree CreateBlendTree(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateConstant state, int nodeIndex)
		{
			IBlendTree blendTree = virtualFile.CreateAsset((int)ClassIDType.BlendTree, BlendTree.Create);
			blendTree.HideFlagsE = HideFlags.HideInHierarchy;

			IBlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Data;

			blendTree.Name = BlendTreeName;

			blendTree.Childs.Capacity = node.ChildIndices.Count;
			for (int i = 0; i < node.ChildIndices.Count; i++)
			{
				blendTree.AddAndInitializeNewChild(virtualFile, controller, state, nodeIndex, i);
			}

			if (node.BlendEventID != uint.MaxValue)
			{
				blendTree.BlendParameter = controller.TOS[node.BlendEventID];
			}
			if (node.BlendEventYID != uint.MaxValue)
			{
				blendTree.BlendParameterY = controller.TOS[node.BlendEventYID];
			}
			blendTree.MinThreshold = node.GetMinThreshold();
			blendTree.MaxThreshold = node.GetMaxThreshold();
			blendTree.UseAutomaticThresholds = false;
			blendTree.NormalizedBlendValues = node.BlendDirectData?.Data.NormalizedBlendValues ?? false;
			if (blendTree.Has_BlendType_Int32())
			{
				blendTree.BlendType_Int32 = (int)node.BlendType;
			}
			else
			{
				blendTree.BlendType_UInt32 = node.BlendType;
			}
			return blendTree;
		}

		private static IChildMotion AddAndInitializeNewChild(this IBlendTree tree, ProcessedAssetCollection file, IAnimatorController controller, IStateConstant state, int nodeIndex, int childIndex)
		{
			IChildMotion childMotion = tree.Childs.AddNew();
			IBlendTreeConstant treeConstant = state.GetBlendTree();
			IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			IMotion? motion = state.CreateMotion(file, controller, childNodeIndex);
			childMotion.Motion.SetAsset(tree.Collection, motion);

			IBlendTreeNodeConstant childNode = treeConstant.NodeArray[childNodeIndex].Data;
			if (childNode.IsBlendTree())
			{
				// BlendTree ChildMotions are not allowed to use TimeScale or Mirror
				// https://github.com/Unity-Technologies/UnityCsReference/blob/4e215c07ca8e9a32a589043202fd919bdfc0a26d/Editor/Mono/Inspector/BlendTreeInspector.cs#L1469
				// https://github.com/Unity-Technologies/UnityCsReference/blob/4e215c07ca8e9a32a589043202fd919bdfc0a26d/Editor/Mono/Inspector/BlendTreeInspector.cs#L1488
				childMotion.TimeScale = 1;
				childMotion.Mirror = false;
			}
			else
			{
				childMotion.TimeScale = 1 / childNode.Duration;
				childMotion.Mirror = childNode.Mirror;
			}
			childMotion.CycleOffset = childNode.CycleOffset;

			childMotion.Threshold = node.GetThreshold(childIndex);
			childMotion.Position?.CopyValues(node.GetPosition(childIndex));
			if (node.TryGetDirectBlendParameter(childIndex, out uint directID))
			{
				childMotion.DirectBlendParameter = controller.TOS[directID];
			}

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
			if (generatedStateMachine.Has_EntryTransitions() && stateMachineConstant.Has_SelectorStateConstantArray())
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
							generatedStateMachine.EntryTransitionsP.Add(transition);
						}
					}
				}
			}
		}

		public static IAnimatorStateMachine CreateAnimatorStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			const float StateOffset = 250.0f;

			IAnimatorStateMachine generatedStateMachine = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateMachine, AnimatorStateMachine.Create);
			generatedStateMachine.HideFlagsE = HideFlags.HideInHierarchy;

			int layerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
			ILayerConstant layer = controller.Controller.LayerArray[layerIndex].Data;
			generatedStateMachine.Name = controller.TOS[layer.Binding];

			IStateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Data;

			int stateCount = stateMachine.StateConstantArray.Count;
			int stateMachineCount = 0;
			int stateAndStateMachineCount = stateCount + stateMachineCount;
			int side = (int)Math.Ceiling(Math.Sqrt(stateAndStateMachineCount));

			List<IAnimatorState> states = new();
			if (generatedStateMachine.Has_ChildStates())
			{
				generatedStateMachine.ChildStates.Clear();
				generatedStateMachine.ChildStates.Capacity = stateCount;
			}
			else if (generatedStateMachine.Has_States())
			{
				generatedStateMachine.States.Clear();
				generatedStateMachine.States.Capacity = stateCount;
			}
			for (int y = 0, stateIndex = 0; y < side && stateIndex < stateCount; y++)
			{
				for (int x = 0; x < side && stateIndex < stateCount; x++, stateIndex++)
				{
					Vector3f position = new() { X = x * StateOffset, Y = y * StateOffset };
					IAnimatorState state = CreateAnimatorState(virtualFile, controller, stateMachineIndex, stateIndex, position);

					if (generatedStateMachine.Has_ChildStates())
					{
						ChildAnimatorState childState = generatedStateMachine.ChildStates.AddNew();
						childState.Position.CopyValues(position);
						childState.State.SetAsset(generatedStateMachine.Collection, state);
					}
					else if (generatedStateMachine.Has_States())
					{
						generatedStateMachine.StatesP.Add(state);
					}

					states.Add(state);
				}
			}

#warning TODO: child StateMachines
			//generatedStateMachine.ChildStateMachines = new ChildAnimatorStateMachine[stateMachineCount];

			// set destination state for transitions here because all states have only become valid now
			for (int i = 0; i < stateMachine.StateConstantArray.Count; i++)
			{
				IAnimatorState state = states[i];
				IStateConstant stateConstant = stateMachine.StateConstantArray[i].Data;

				AssetList<PPtr_AnimatorStateTransition_4>? transitionList;
				if (state.Has_Transitions())
				{
					state.Transitions.EnsureCapacity(state.Transitions.Count + stateConstant.TransitionConstantArray.Count);
					transitionList = null;
				}
				else if (generatedStateMachine.Has_OrderedTransitions())
				{
					//I'm not sure if this is correct, but it seems to be the only logical way to store the transitions before Unity 5.
					//IAnimatorStateMachine.LocalTransitions only exists until Unity 4.2.0, so by process of elimination, this is the only option.

					AssetPair<PPtr_AnimatorState_4, AssetList<PPtr_AnimatorStateTransition_4>> pair = generatedStateMachine.OrderedTransitions.AddNew();
					pair.Key.SetAsset(generatedStateMachine.Collection, state);
					transitionList = pair.Value;
				}
				else
				{
					//This should never happen.
					Logger.Error(LogCategory.Processing, "Loose Transitions will be created. This can only happen when the AnimatorState and AnimatorStateMachine have different Unity versions, specifically on opposite sides of Unity 5.");
					transitionList = null;
				}

				for (int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
				{
					ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Data;
					IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachine, states, controller.TOS, transitionConstant);
					if (state.Has_Transitions())
					{
						state.TransitionsP.Add(transition);
					}
					else
					{
						transitionList?.AddNew().SetAsset(generatedStateMachine.Collection, transition);
					}
				}
			}

			//AnyStateTransitions
			{
				int count = stateMachine.AnyStateTransitionConstantArray.Count;
				if (generatedStateMachine.Has_AnyStateTransitions())
				{
					generatedStateMachine.AnyStateTransitions.Capacity = count;
					for (int i = 0; i < count; i++)
					{
						ITransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Data;
						IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachine, states, controller.TOS, transitionConstant);
						generatedStateMachine.AnyStateTransitionsP.Add(transition);
					}
				}
				else
				{
					//https://github.com/AssetRipper/AssetRipper/issues/1028
					AssetList<PPtr_AnimatorStateTransition_4> newList = generatedStateMachine.OrderedTransitions.AddNew().Value;
					newList.Capacity = count;
					PPtrAccessList<PPtr_AnimatorStateTransition_4, IAnimatorStateTransition> anyStateTransitions = new(newList, generatedStateMachine.Collection);
					for (int i = 0; i < count; i++)
					{
						ITransitionConstant transitionConstant = stateMachine.AnyStateTransitionConstantArray[i].Data;
						IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachine, states, controller.TOS, transitionConstant);
						anyStateTransitions.Add(transition);
					}
				}
			}

			CreateEntryTransitions(generatedStateMachine, stateMachine, virtualFile, layer.Binding, states, controller.TOS);

			generatedStateMachine.StateMachineBehaviours?.Clear();
#warning TEMP: enable when AnimatorStateMachine's child StateMachines has been implemented
			if (false)
			{
				generatedStateMachine.StateMachineBehavioursP.AddRange(controller.GetStateBehaviours(layerIndex));
			}

			generatedStateMachine.AnyStatePosition.SetValues(0.0f, -StateOffset, 0.0f);
			generatedStateMachine.EntryPosition?.SetValues(StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ExitPosition?.SetValues(2.0f * StateOffset, -StateOffset, 0.0f);
			generatedStateMachine.ParentStateMachinePosition.SetValues(0.0f, -2.0f * StateOffset, 0.0f);

			if (generatedStateMachine.Has_ChildStates() && generatedStateMachine.ChildStates.Count > 0)
			{
				PPtr_AnimatorState_5 defaultStatePPtr = generatedStateMachine.ChildStates[(int)stateMachine.DefaultState].State;

				generatedStateMachine.DefaultState.CopyValues(defaultStatePPtr, new PPtrConverter(generatedStateMachine));
			}
			else if (generatedStateMachine.Has_States() && generatedStateMachine.States.Count > 0)
			{
				PPtr_AnimatorState_4 defaultStatePPtr = generatedStateMachine.States[(int)stateMachine.DefaultState];

				generatedStateMachine.DefaultState.CopyValues(defaultStatePPtr, new PPtrConverter(generatedStateMachine));
			}

			return generatedStateMachine;
		}

		private static IAnimatorState CreateAnimatorState(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex, int stateIndex, Vector3f position)
		{
			IAnimatorState generatedState = virtualFile.CreateAsset((int)ClassIDType.AnimatorState, AnimatorState.Create);
			generatedState.HideFlagsE = HideFlags.HideInHierarchy;

			AssetDictionary<uint, Utf8String> tos;
			if (controller.TOS.ContainsKey(0))
			{
				tos = controller.TOS;
			}
			else
			{
				tos = new AssetDictionary<uint, Utf8String>() { { 0, Utf8String.Empty } };
				foreach ((uint hash, Utf8String str) in controller.TOS)
				{
					tos.Add(hash, str);
				}
			}
			IStateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Data;
			IStateConstant state = stateMachine.StateConstantArray[stateIndex].Data;

			generatedState.Name = tos[state.NameID];

			generatedState.Speed = state.Speed;
			generatedState.CycleOffset = state.CycleOffset;

			// skip Transitions because not all state exists at this moment

			if (generatedState.Has_StateMachineBehaviours())
			{
				// exclude StateMachine's behaviours
				int layerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);
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

				generatedState.StateMachineBehavioursP.AddRange(behaviours);
			}

			generatedState.Position.CopyValues(position);
			generatedState.IKOnFeet = state.IKOnFeet;
			generatedState.WriteDefaultValues = state.GetWriteDefaultValues();
			generatedState.Mirror = state.Mirror;
			generatedState.SpeedParameterActive = state.SpeedParamID > 0;
			generatedState.MirrorParameterActive = state.MirrorParamID > 0;
			generatedState.CycleOffsetParameterActive = state.CycleOffsetParamID > 0;
			generatedState.TimeParameterActive = state.TimeParamID > 0;

			IMotion? motion = state.CreateMotion(virtualFile, controller, 0);
			if (generatedState.Has_Motion())
			{
				generatedState.MotionP = motion;
			}
			else
			{
				generatedState.MotionsP.Add(motion);
			}

			generatedState.Tag = tos[state.TagID];
			generatedState.SpeedParameter = tos[state.SpeedParamID];
			generatedState.MirrorParameter = tos[state.MirrorParamID];
			generatedState.CycleOffsetParameter = tos[state.CycleOffsetParamID];
			generatedState.TimeParameter = tos[state.TimeParamID];

			return generatedState;
		}

		private static IAnimatorStateTransition CreateAnimatorStateTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachine,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS,
			ITransitionConstant Transition)
		{
			IAnimatorStateTransition animatorStateTransition = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateTransition, AnimatorStateTransition.Create);
			animatorStateTransition.HideFlags = (uint)HideFlags.HideInHierarchy;

			animatorStateTransition.Conditions.Capacity = Transition.ConditionConstantArray.Count;
			for (int i = 0; i < Transition.ConditionConstantArray.Count; i++)
			{
				ConditionConstant conditionConstant = Transition.ConditionConstantArray[i].Data;
				if (!animatorStateTransition.Has_ExitTime() || conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
				{
					IAnimatorCondition condition = animatorStateTransition.Conditions.AddNew();
					condition.ConditionMode = (int)conditionConstant.ConditionModeE;
					condition.ConditionEvent = TOS[conditionConstant.EventID];
					condition.EventTreshold = conditionConstant.EventThreshold;
					condition.ExitTime = conditionConstant.ExitTime;
				}
			}

			animatorStateTransition.DstStateP = GetDestinationState(Transition.DestinationState, StateMachine, States);

			animatorStateTransition.Name = TOS[Transition.UserID];
			animatorStateTransition.IsExit = Transition.IsExit();

			animatorStateTransition.Atomic = Transition.Atomic;
			animatorStateTransition.TransitionDuration = Transition.TransitionDuration;
			animatorStateTransition.TransitionOffset = Transition.TransitionOffset;
			animatorStateTransition.ExitTime = Transition.GetExitTime();
			animatorStateTransition.HasExitTime = Transition.GetHasExitTime();
			animatorStateTransition.HasFixedDuration = Transition.GetHasFixedDuration();
			animatorStateTransition.InterruptionSourceE = Transition.GetInterruptionSource();
			animatorStateTransition.OrderedInterruption = Transition.OrderedInterruption;
			animatorStateTransition.CanTransitionToSelf = Transition.CanTransitionToSelf;

			return animatorStateTransition;
		}

		private static IAnimatorTransition CreateAnimatorTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachine,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS,
			SelectorTransitionConstant Transition)
		{
			IAnimatorTransition animatorTransition = virtualFile.CreateAsset((int)ClassIDType.AnimatorTransition, AnimatorTransition.Create);
			animatorTransition.HideFlagsE = HideFlags.HideInHierarchy;

			animatorTransition.Conditions.Capacity = Transition.ConditionConstantArray.Count;
			for (int i = 0; i < Transition.ConditionConstantArray.Count; i++)
			{
				ConditionConstant conditionConstant = Transition.ConditionConstantArray[i].Data;
				if (conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
				{
					IAnimatorCondition condition = animatorTransition.Conditions.AddNew();
					condition.ConditionMode = (int)conditionConstant.ConditionModeE;
					condition.ConditionEvent = TOS[conditionConstant.EventID];
					condition.EventTreshold = conditionConstant.EventThreshold;
				}
			}

			animatorTransition.DstStateP = GetDestinationState(Transition.Destination, StateMachine, States);

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
