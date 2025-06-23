using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
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
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.ConditionConstant;
using AssetRipper.SourceGenerated.Subclasses.LeafInfoConstant;
using AssetRipper.SourceGenerated.Subclasses.SelectorTransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;

namespace AssetRipper.Processing.AnimatorControllers;

public static class VirtualAnimationFactory
{
	// Example of default BlendTree Name:
	// https://github.com/ds5678/Binoculars/blob/d6702ed3a1db39b1a2788956ff195b2590c3d08b/Unity/Assets/Models/binoculars_animator.controller#L106
	private static Utf8String BlendTreeName { get; } = new Utf8String("Blend Tree");

	private static Utf8String AnimatorStateName { get; } = new Utf8String("New State");

	private static IMotion? CreateMotion(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateConstant stateConstant, int nodeIndex)
	{
		if (stateConstant.BlendTreeConstantArray.Count == 0)
		{
			return default; // null Motion
		}
		else
		{
			IBlendTreeNodeConstant node = stateConstant.GetBlendTree().NodeArray[nodeIndex].Data;
			if (node.IsBlendTree())
			{
				return CreateBlendTree(virtualFile, controller, stateConstant, nodeIndex); // BlendTree Motion
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
					return default; // null Motion
				}
				else
				{
					return controller.AnimationClipsP[clipIndex] as IMotion; // AnimationClip Motion (inherited since Unity 4)
				}
			}
		}
	}

	private static IBlendTree CreateBlendTree(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateConstant state, int nodeIndex)
	{
		IBlendTree blendTree = virtualFile.CreateBlendTree();
		blendTree.HideFlagsE = HideFlags.HideInHierarchy;

		IBlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Data;

		blendTree.Name = BlendTreeName;

		blendTree.Childs.Capacity = node.ChildIndices.Count;
		for (int i = 0; i < node.ChildIndices.Count; i++)
		{
			AddAndInitializeNewChild(virtualFile, controller, state, blendTree, nodeIndex, i);
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

	private static IChildMotion AddAndInitializeNewChild(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateConstant state, IBlendTree tree, int nodeIndex, int childIndex)
	{
		IChildMotion childMotion = tree.Childs.AddNew();
		IBlendTreeConstant treeConstant = state.GetBlendTree();
		IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
		int childNodeIndex = (int)node.ChildIndices[childIndex];
		// https://github.com/AssetRipper/AssetRipper/issues/1566
		// Strangely, some BlendTree nodes have the same index as the child node index.
		// In the case of the above issue, both indices were 0.
		IMotion? motion = nodeIndex != childNodeIndex
			? CreateMotion(virtualFile, controller, state, childNodeIndex)
			: null; // tree might be more accurate here since the indices are the same, but it doesn't make sense for a BlendTree to be a child of itself.

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

	/// <summary>
	/// Create a fully solved Root AnimatorStateMachine for the corresponding index
	/// </summary>
	public static IAnimatorStateMachine CreateRootAnimatorStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
	{
		AnimatorStateMachineContext stateMachineContext = new(virtualFile, controller, stateMachineIndex);
		stateMachineContext.Process();
		return stateMachineContext.RootStateMachine;
	}

	public static IAnimatorStateMachine CreateStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int layerIndex, uint fullPathID = 0)
	{
		IAnimatorStateMachine stateMachine = virtualFile.CreateAnimatorStateMachine();
		stateMachine.HideFlagsE = HideFlags.HideInHierarchy;

		// can add StateMachineBehaviours now
		if (stateMachine.Has_StateMachineBehaviours())
		{
			IMonoBehaviour?[] stateBehaviours = controller.GetStateBehaviours(layerIndex, fullPathID);
			foreach (IMonoBehaviour? stateBehaviour in stateBehaviours)
			{
				if (stateBehaviour != null)
				{
					stateBehaviour.HideFlagsE = HideFlags.HideInHierarchy;
					stateMachine.StateMachineBehavioursP.Add(stateBehaviour);
				}
			}
		}

		return stateMachine;
	}

	public static IAnimatorState CreateAnimatorState(ProcessedAssetCollection virtualFile, IAnimatorController controller, AssetDictionary<uint, Utf8String> tos, int layerIndex, IStateConstant stateConstant)
	{
		IAnimatorState animatorState = virtualFile.CreateAnimatorState();
		animatorState.HideFlagsE = HideFlags.HideInHierarchy;

		animatorState.Name = stateConstant.GetName(tos);
		animatorState.Speed = stateConstant.Speed;
		animatorState.CycleOffset = stateConstant.CycleOffset;

		if (animatorState.Has_StateMachineBehaviours())
		{
			uint stateID = stateConstant.GetId();
			IMonoBehaviour?[] stateBehaviours = controller.GetStateBehaviours(layerIndex, stateID);
			animatorState.StateMachineBehavioursP.AddRange(stateBehaviours);
		}

		animatorState.IKOnFeet = stateConstant.IKOnFeet;
		animatorState.WriteDefaultValues = stateConstant.GetWriteDefaultValues();
		animatorState.Mirror = stateConstant.Mirror;
		animatorState.SpeedParameterActive = stateConstant.SpeedParamID > 0;
		animatorState.MirrorParameterActive = stateConstant.MirrorParamID > 0;
		animatorState.CycleOffsetParameterActive = stateConstant.CycleOffsetParamID > 0;
		animatorState.TimeParameterActive = stateConstant.TimeParamID > 0;

		IMotion? motion = CreateMotion(virtualFile, controller, stateConstant, 0);
		if (animatorState.Has_Motion())
		{
			animatorState.MotionP = motion;
		}
		else
		{
			animatorState.MotionsP.Add(motion);
		}

		animatorState.Tag = tos[stateConstant.TagID];
		animatorState.SpeedParameter = tos[stateConstant.SpeedParamID];
		animatorState.MirrorParameter = tos[stateConstant.MirrorParamID];
		animatorState.CycleOffsetParameter = tos[stateConstant.CycleOffsetParamID];
		animatorState.TimeParameter = tos[stateConstant.TimeParamID];

		return animatorState;
	}

	public static IAnimatorState CreateDefaultAnimatorState(ProcessedAssetCollection virtualFile)
	{
		IAnimatorState animatorState = virtualFile.CreateAnimatorState();
		animatorState.HideFlagsE = HideFlags.HideInHierarchy;

		animatorState.Name = AnimatorStateName;
		animatorState.Speed = 1;
		animatorState.WriteDefaultValues = true;

		return animatorState;
	}

	public static IAnimatorStateTransition CreateAnimatorStateTransition(ProcessedAssetCollection virtualFile, AssetDictionary<uint, Utf8String> TOS, ITransitionConstant Transition)
	{
		IAnimatorStateTransition animatorStateTransition = virtualFile.CreateAnimatorStateTransition();
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

		animatorStateTransition.Name = TOS[Transition.UserID];

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

	public static IAnimatorTransition CreateAnimatorTransition(ProcessedAssetCollection virtualFile, AssetDictionary<uint, Utf8String> TOS, SelectorTransitionConstant transition)
	{
		IAnimatorTransition animatorTransition = virtualFile.CreateAnimatorTransition();
		animatorTransition.HideFlagsE = HideFlags.HideInHierarchy;

		animatorTransition.Conditions.Capacity = transition.ConditionConstantArray.Count;
		for (int i = 0; i < transition.ConditionConstantArray.Count; i++)
		{
			ConditionConstant conditionConstant = transition.ConditionConstantArray[i].Data;
			if (conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
			{
				IAnimatorCondition condition = animatorTransition.Conditions.AddNew();
				condition.ConditionMode = (int)conditionConstant.ConditionModeE;
				condition.ConditionEvent = TOS[conditionConstant.EventID];
				condition.EventTreshold = conditionConstant.EventThreshold;
			}
		}

		return animatorTransition;
	}
}
