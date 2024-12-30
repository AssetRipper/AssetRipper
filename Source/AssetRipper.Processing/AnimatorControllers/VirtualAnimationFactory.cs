using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
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
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.ConditionConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AssetRipper.SourceGenerated.Subclasses.LeafInfoConstant;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorState;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateTransition;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition;
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

		private static IAnimatorState CreateAnimatorState(ProcessedAssetCollection virtualFile, IAnimatorController controller, AssetDictionary<uint, Utf8String> tos, int layerIndex, IStateConstant state)
		{
			IAnimatorState generatedState = virtualFile.CreateAsset((int)ClassIDType.AnimatorState, AnimatorState.Create);
			generatedState.HideFlagsE = HideFlags.HideInHierarchy;

			if (state.Has_NameID())
			{
				generatedState.Name = tos[state.NameID];
			}
			else
			{
				string statePath = tos[state.ID].String; // ParentStateMachineName.StateName
				int pathDelimiterPos = statePath.IndexOf('.');
				if (pathDelimiterPos != -1 && pathDelimiterPos + 1 < statePath.Length)
				{
					generatedState.Name = statePath[(pathDelimiterPos + 1)..];
				}
				else
				{
					generatedState.Name = statePath;
				}
			}

			generatedState.Speed = state.Speed;
			generatedState.CycleOffset = state.CycleOffset;

			// skip Transitions because not all States exist at this moment

			if (generatedState.Has_StateMachineBehaviours())
			{
				uint stateID = state.GetId();
				IMonoBehaviour?[] stateBehaviours = controller.GetStateBehaviours(layerIndex, stateID);
				generatedState.StateMachineBehavioursP.AddRange(stateBehaviours);
			}

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

			// https://github.com/AssetRipper/AssetRipper/issues/1566
			// Strangely, some BlendTree nodes have the same index as the child node index.
			// In the case of the above issue, both indices were 0.
			IMotion? motion = nodeIndex != childNodeIndex
				? state.CreateMotion(file, controller, childNodeIndex)
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

		public static IAnimatorStateMachine CreateAnimatorStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			StateMachineContext stateMachineContext = new(virtualFile, controller, stateMachineIndex);
			stateMachineContext.Process();
			return stateMachineContext.RootStateMachine;
		}

		private class StateContext
		{
			public readonly int StateCount;
			[MemberNotNullWhen(true, nameof(states), nameof(stateConstants), nameof(stateMachinePathIdxs),
				nameof(uniqueStateMachinePaths), nameof(uniqueStateMachinePathIDs))]
			public bool HasStates() => StateCount > 0;

			public readonly int DefaultStateIdx;

			readonly IAnimatorState[]? states;
			readonly IStateConstant[]? stateConstants;
			readonly int[]? stateMachinePathIdxs;

			readonly string[]? uniqueStateMachinePaths; // Bidirectional Dictionary and grouping States for StateMachines
			readonly uint[]? uniqueStateMachinePathIDs;   // Bidirectional Dictionary and grouping States for StateMachines

			public IStateConstant GetStateConstant(int index)
			{
				return stateConstants![index];
			}

			public IAnimatorState GetState(int index)
			{
				return states![index];
			}

			public int GetStateIdx(IAnimatorState? state)
			{
				if (state == null)
				{
					return -1;
				}
				int stateIdx = states!.IndexOf(state);
				return stateIdx;
			}

			public string GetStateMachinePath(int stateIndex)
			{
				int pathIdx = stateMachinePathIdxs![stateIndex];
				return uniqueStateMachinePaths![pathIdx];
			}

			public bool TryGetStateMachinePath(uint pathID, out string path) // for Bidirectional Dictionary
			{
				if (pathID == 0)
				{
					path = string.Empty;
					return false;
				}
				int pathIdx = uniqueStateMachinePathIDs!.IndexOf(pathID);
				if (pathIdx == -1)
				{
					path = string.Empty;
					return false;
				}
				path = uniqueStateMachinePaths![pathIdx];
				return true;
			}

			public bool TryGetStateMachinePathID(string path, out uint pathID) // for Bidirectional Dictionary
			{
				if (string.IsNullOrEmpty(path))
				{
					pathID = 0;
					return false;
				}
				int pathIdx = uniqueStateMachinePaths!.IndexOf(path);
				if (pathIdx == -1)
				{
					pathID = 0;
					return false;
				}
				pathID = uniqueStateMachinePathIDs![pathIdx];
				return true;
			}

			public IReadOnlyList<string> GetUniqueSMPaths()
			{
				return uniqueStateMachinePaths!;
			}

			public IEnumerable<int> StateIdxsForStateMachine(uint pathID) // for grouping States
			{
				int pathIdx = uniqueStateMachinePathIDs!.IndexOf(pathID);
				if (pathIdx != -1)
				{
					for (int i = 0; i < stateMachinePathIdxs!.Length; i++)
					{
						if (stateMachinePathIdxs[i] == pathIdx)
						{
							yield return i;
						}
					}
				}
			}

			public IEnumerable<int> StateIdxsForStateMachine(string path) // for grouping States
			{
				int pathIdx = uniqueStateMachinePaths!.IndexOf(path);
				if (pathIdx != -1)
				{
					for (int i = 0; i < stateMachinePathIdxs!.Length; i++)
					{
						if (stateMachinePathIdxs[i] == pathIdx)
						{
							yield return i;
						}
					}
				}
			}

			public StateContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
			{
				if (!controller.TOS.ContainsKey(0))
				{
					controller.TOS[0] = Utf8String.Empty;
				}
				DefaultStateIdx = stateMachineConstant.DefaultState != uint.MaxValue ? (int)stateMachineConstant.DefaultState : 0;

				StateCount = stateMachineConstant.StateConstantArray.Count;
				if (!HasStates())
				{
					return;
				}

				stateConstants = new IStateConstant[StateCount];
				states = new IAnimatorState[StateCount];
				stateMachinePathIdxs = new int[StateCount];
				List<string> uniqueSMPaths = new();
				for (int i = 0; i < StateCount; i++)
				{
					IStateConstant stateConstant = stateMachineConstant.StateConstantArray[i].Data;
					IAnimatorState state = CreateAnimatorState(virtualFile, controller, controller.TOS, layerIndex, stateConstant);

					string stateMachinePath = MakeStateMachinePath(controller.TOS, stateConstant.GetId(), state.Name.String);
					int SMPathIdx = uniqueSMPaths.FindIndex(x => x == stateMachinePath);
					if (SMPathIdx == -1)
					{
						SMPathIdx = uniqueSMPaths.Count;
						uniqueSMPaths.Add(stateMachinePath);
					}
					stateConstants[i] = stateConstant;
					states[i] = state;
					stateMachinePathIdxs[i] = SMPathIdx;
				}

				int uniqueSMPathsCount = uniqueSMPaths.Count;
				if (stateMachineConstant.StateMachineCount() > uniqueSMPathsCount) // can only happen on Unity 5+
				{
					// there are StateMachines with no States
					// try generate more possible StateMachine paths to locate them
					// *not useful when these StateMachines come last in hierachy (don't have child StateMachines with States)
					for (int i = 0; i < uniqueSMPathsCount; i++)
					{
						string stateMachinePath = uniqueSMPaths[i];
						int pathDelimiterPos = stateMachinePath.LastIndexOf('.');
						while (pathDelimiterPos != -1)
						{
							stateMachinePath = stateMachinePath[..pathDelimiterPos];
							if (uniqueSMPaths.Contains(stateMachinePath))
							{
								break;
							}
							else
							{
								uniqueSMPaths.Add(stateMachinePath);
							}
							pathDelimiterPos = stateMachinePath.LastIndexOf('.');
						}
					}
				}

				uniqueStateMachinePaths = uniqueSMPaths.ToArray();
				uniqueStateMachinePathIDs = new uint[uniqueStateMachinePaths.Length];
				for (int i = 0; i < uniqueStateMachinePaths.Length; i++)
				{
					string uniqueSMPath = uniqueStateMachinePaths[i];
					uint PathID = Checksum.Crc32Algorithm.HashUTF8(uniqueSMPath);
					uniqueStateMachinePathIDs[i] = PathID;
				}
			}

			private static string MakeStateMachinePath(AssetDictionary<uint, Utf8String> TOS, uint statePathID, string stateName)
			{
				string path = TOS[statePathID];
				string stateMachinePath = path[..(path.Length - stateName.Length - 1)];
				return stateMachinePath;
			}
		}

		private class StateMachineContext
		{
			const uint StateMachineTransitionFlag = 30000;

			readonly ProcessedAssetCollection VirtualFile;
			readonly IAnimatorController Controller;

			readonly IStateMachineConstant StateMachineConstant;
			readonly int LayerIndex;
			readonly ILayerConstant Layer;
			readonly StateContext StateContext;

			IAnimatorStateMachine[] StateMachines;
			uint[]? StateMachineFullPathIDs;
			uint[]? StateMachinePathIDs;
			SelectorTransitionConstant[]?[]? EntryTransitions;
			SelectorTransitionConstant[]?[]? ExitTransitions;

			int UnknownFullPaths = 0;
			bool _HasSelectorStateConstant = false;

			[MemberNotNullWhen(true, nameof(StateMachineFullPathIDs), nameof(StateMachinePathIDs), nameof(EntryTransitions), nameof(ExitTransitions))]
			private bool HasSelectorStateConstant() => _HasSelectorStateConstant;

			public IAnimatorStateMachine RootStateMachine => StateMachines[0];

			public StateMachineContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
			{
				VirtualFile = virtualFile;
				Controller = controller;
				StateMachineConstant = controller.Controller.StateMachineArray[stateMachineIndex].Data;
				LayerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex, out Layer);
				StateContext = new(virtualFile, controller, StateMachineConstant, LayerIndex); // setting State Transitions later
				InitializeStateMachines();
			}

			[MemberNotNull(nameof(StateMachines))]
			private void InitializeStateMachines()
			{
				if (StateMachineConstant.Has_SelectorStateConstantArray())
				{
					// Unity 5+
					_HasSelectorStateConstant = true;

					int stateMachineCount = StateMachineConstant.StateMachineCount();
					StateMachines = new IAnimatorStateMachine[stateMachineCount];
					StateMachineFullPathIDs = new uint[stateMachineCount];
					StateMachinePathIDs = new uint[stateMachineCount];
					EntryTransitions = new SelectorTransitionConstant[stateMachineCount][];
					ExitTransitions = new SelectorTransitionConstant[stateMachineCount][];

					// assuming SelectorStateConstantArray follows the sequence: [Entry1, Exit1, Entry2, Exit2, ...]
					// just in case, next code can handle StateMachines missing Entry or Exit SelectorStateConstant
					int stateMachineIdx = 0;
					uint lastFullPathID = 0;
					foreach (SelectorStateConstant ssc in StateMachineConstant.SelectorStateConstantArray)
					{
						SelectorTransitionConstant[]? transitions = ssc.TransitionConstantArray.Count == 0 ? null :
							ssc.TransitionConstantArray.Select(ptr => ptr.Data).ToArray();
						uint sscFullPathID = ssc.FullPathID;
						if (lastFullPathID != sscFullPathID)
						{
							StateMachines[stateMachineIdx] = CreateStateMachine(sscFullPathID);
							StateMachineFullPathIDs[stateMachineIdx] = ssc.FullPathID;
							if (ssc.IsEntry)
							{
								EntryTransitions[stateMachineIdx] = transitions;
							}
							else
							{
								ExitTransitions[stateMachineIdx] = transitions;
							}

							lastFullPathID = ssc.FullPathID;
							stateMachineIdx++;
						}
						else
						{
							if (ssc.IsEntry)
							{
								EntryTransitions[stateMachineIdx-1] = transitions;
							}
							else
							{
								ExitTransitions[stateMachineIdx-1] = transitions;
							}
						}
					}

					if (!StateContext.HasStates())
					{
						// No States to help resolve StateMachine Names
						// still can give Name to Root StateMachine using Layer
						IAnimatorStateMachine stateMachine = StateMachines[0];
						stateMachine.Name = Controller.TOS[Layer.Binding].String.Replace('.', '_');
					}
				}
				else
				{
					// Unity 4.x-
					// StateMachines don't have FullPaths.
					// can set Names, Child States (with their Transitions),
					// Default State and Child StateMachines now
					if (!StateContext.HasStates())
					{
						// empty Main StateMachine. its better to resolve Name now
						IAnimatorStateMachine stateMachine = CreateStateMachine();
						stateMachine.Name = Controller.TOS[Layer.Binding].String.Replace('.', '_');
						StateMachines = [stateMachine];
					}
					else
					{
						IReadOnlyList<string> stateMachinePaths = StateContext.GetUniqueSMPaths();
						StateMachines = new IAnimatorStateMachine[stateMachinePaths.Count];

						// set Root StateMachine Name from DefaultState Path
						IAnimatorStateMachine mainStateMachine = CreateStateMachine();
						string mainStateMachineName = StateContext.GetStateMachinePath(StateContext.DefaultStateIdx);
						mainStateMachine.Name = mainStateMachineName;
						mainStateMachine.DefaultStateP = StateContext.GetState(StateContext.DefaultStateIdx);
						mainStateMachine.SetChildStateMachineCapacity(StateMachines.Length - 1);

						// ensure Root StateMachine will be at index 0
						StateMachines[0] = mainStateMachine;
						for (int i = 0, j = 1; i < StateMachines.Length; i++, j++)
						{
							string stateMachineName = stateMachinePaths[i];
							if (stateMachineName != mainStateMachineName)
							{
								IAnimatorStateMachine stateMachine = CreateStateMachine();
								stateMachine.Name = stateMachineName;
								StateMachines[j] = stateMachine;
								// set Child StateMachines
								mainStateMachine.ChildStateMachineP.Add(stateMachine);
							}
							else
							{
								j--;
							}
						}

						// set Child States with their Transitions
						for (int i = 0; i < StateMachines.Length; i++)
						{
							int childStateCount = StateContext.StateIdxsForStateMachine(StateMachines[i].Name).Count();
							StateMachines[i].SetChildStateCapacity(childStateCount);
							foreach (int stateIdx in StateContext.StateIdxsForStateMachine(StateMachines[i].Name))
							{
								AddStateAndTransitionsToStateMachine(i, stateIdx);
							}
						}
					}
				}

				IAnimatorStateMachine CreateStateMachine(uint fullPathID = 0)
				{
					IAnimatorStateMachine stateMachine = VirtualFile.CreateAsset((int)ClassIDType.AnimatorStateMachine, AnimatorStateMachine.Create);
					stateMachine.HideFlagsE = HideFlags.HideInHierarchy;
					// can add StateMachineBehaviours now
					if (stateMachine.Has_StateMachineBehaviours())
					{
						IMonoBehaviour?[] stateBehaviours = Controller.GetStateBehaviours(LayerIndex, fullPathID);
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
			}

			private void AddStateAndTransitionsToStateMachine(int parentStateMachineIdx, int stateIdx)
			{
				// -- add child State to stateMachine --
				IAnimatorState state = StateContext.GetState(stateIdx);
				IStateConstant stateConstant = StateContext.GetStateConstant(stateIdx);
				IAnimatorStateMachine parentStateMachine = StateMachines[parentStateMachineIdx];
				if (parentStateMachine.Has_ChildStates())
				{
					ChildAnimatorState childState = parentStateMachine.ChildStates.AddNew();
					// set childState.Position later, when having all Children set
					childState.State.SetAsset(parentStateMachine.Collection, state);
				}
				else
				{
					parentStateMachine.StatesP.Add(state);
				}
				// set state.Position later, when having all Children set

				// -- add State Transitions --
				AssetList<PPtr_AnimatorStateTransition_4>? transitionList = null;
				if (state.Has_Transitions())
				{
					state.Transitions.Capacity = stateConstant.TransitionConstantArray.Count;
				}
				else if (parentStateMachine.Has_OrderedTransitions())
				{
					//I'm not sure if this is correct, but it seems to be the only logical way to store the transitions before Unity 5.
					//IAnimatorStateMachine.LocalTransitions only exists until Unity 4.2.0, so by process of elimination, this is the only option.

					AssetPair<PPtr_AnimatorState_4, AssetList<PPtr_AnimatorStateTransition_4>> pair = parentStateMachine.OrderedTransitions.AddNew();
					pair.Key.SetAsset(parentStateMachine.Collection, state);
					transitionList = pair.Value;
				}
				for (int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
				{
					ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Data;
					IAnimatorStateTransition? transition = CreateAnimatorStateTransition(parentStateMachineIdx, transitionConstant);
					if (transition != null)
					{
						if (state.Has_Transitions())
						{
							state.TransitionsP.Add(transition);
						}
						else
						{
							transitionList?.AddNew().SetAsset(parentStateMachine.Collection, transition);
						}
					}
				}
			}

			private IAnimatorStateTransition? CreateAnimatorStateTransition(int parentStateMachineIdx, ITransitionConstant Transition)
			{
				if (!TryGetDestinationState(Transition.DestinationState, out IAnimatorState? stateDst, out int stateMachineDstIdx, out bool isEntryDst))
				{
					return null;
				}

				IAnimatorStateTransition animatorStateTransition = VirtualFile.CreateAsset((int)ClassIDType.AnimatorStateTransition, AnimatorStateTransition.Create);
				animatorStateTransition.HideFlags = (uint)HideFlags.HideInHierarchy;
				animatorStateTransition.Name = Controller.TOS[Transition.UserID];
				animatorStateTransition.Atomic = Transition.Atomic;
				animatorStateTransition.TransitionDuration = Transition.TransitionDuration;
				animatorStateTransition.TransitionOffset = Transition.TransitionOffset;
				animatorStateTransition.ExitTime = Transition.GetExitTime();
				animatorStateTransition.HasExitTime = Transition.GetHasExitTime();
				animatorStateTransition.HasFixedDuration = Transition.GetHasFixedDuration();
				animatorStateTransition.InterruptionSourceE = Transition.GetInterruptionSource();
				animatorStateTransition.OrderedInterruption = Transition.OrderedInterruption;
				animatorStateTransition.CanTransitionToSelf = Transition.CanTransitionToSelf;

				animatorStateTransition.Conditions.Capacity = Transition.ConditionConstantArray.Count;
				for (int i = 0; i < Transition.ConditionConstantArray.Count; i++)
				{
					ConditionConstant conditionConstant = Transition.ConditionConstantArray[i].Data;
					if (!animatorStateTransition.Has_ExitTime() || conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
					{
						IAnimatorCondition condition = animatorStateTransition.Conditions.AddNew();
						condition.ConditionMode = (int)conditionConstant.ConditionModeE;
						condition.ConditionEvent = Controller.TOS[conditionConstant.EventID];
						condition.EventTreshold = conditionConstant.EventThreshold;
						condition.ExitTime = conditionConstant.ExitTime;
					}
				}

				if (stateDst != null)
				{
					animatorStateTransition.DstStateP = stateDst;
				}
				else if (HasSelectorStateConstant() && stateMachineDstIdx != -1)
				{
					IAnimatorStateMachine stateMachineDst = StateMachines[stateMachineDstIdx];
					if (isEntryDst)
					{
						animatorStateTransition.DstStateMachineP = stateMachineDst;

						if (stateMachineDst.Name.IsEmpty) // try locate StateMachine of unknown FullPath
						{
							uint stateMachineDstPathID = StateMachinePathIDs[stateMachineDstIdx];
							uint stateMachineSrcFullPathID = StateMachineFullPathIDs[parentStateMachineIdx];

							if (!StateContext.TryGetStateMachinePath(stateMachineDstPathID, out string stateMachineDstPath) ||
								(StateContext.TryGetStateMachinePath(stateMachineSrcFullPathID, out string parentPath) &&
								IsDeeperHierarchy(stateMachineDstPath, parentPath)))
							{
								StateMachinePathIDs[stateMachineDstIdx] = stateMachineSrcFullPathID;
							}
						}
					}
					else
					{
						animatorStateTransition.IsExit = true;
					}
				}

				return animatorStateTransition;
			}

			private bool TryGetDestinationState(uint destinationState,
			out IAnimatorState? stateDst, out int stateMachineDstIdx, out bool isEntryDst)
			{
				stateDst = null; stateMachineDstIdx = -1; isEntryDst = false;
				if (destinationState == uint.MaxValue)
				{
					return false;
				}
				if (destinationState >= StateMachineTransitionFlag)
				{
					// Entry and Exit from StateMachines
					if (StateMachineConstant.Has_SelectorStateConstantArray())
					{
						uint stateIndex = destinationState % StateMachineTransitionFlag;
						SelectorStateConstant selectorState = StateMachineConstant.SelectorStateConstantArray[(int)stateIndex].Data;
						stateMachineDstIdx = GetStateMachineIdxForId(selectorState.FullPathID);
						isEntryDst = selectorState.IsEntry;
						return true;
					}
					return false;
				}
				else
				{
					// State
					stateDst = StateContext.GetState((int)destinationState);
					return true;
				}

				int GetStateMachineIdxForId(uint id)
				{
					if (StateMachineFullPathIDs != null)
					{
						return StateMachineFullPathIDs.IndexOf(id);
					}
					return -1;
				}
			}

			private static bool IsDeeperHierarchy(string currentPath, string newPath)
			{
				int currentDepth = currentPath.Count(ch => ch == '.');
				int newDepth = newPath.Count(ch => ch == '.');
				return newDepth > currentDepth;
			}

			public void Process()
			{
				// create AnyStateTransitions for Root StateMachine
				CreateAnyStateTransitions();

				// Unity 5+
				{
					// Assign States to StateMachines and Create State Transitions
					AssignChildStates();

					// Create Entry Transitions and set Default States
					CreateEntryTransitions();

					if (UnknownFullPaths != 0)
					{
						// try set unknown FullPaths from default Exit Transitions and Default States
						FindParentStateMachineForUnknownFullPaths();
					}

					// Assign Child StateMachines and Create StateMachine Transitions
					AssignChildStateMachines();
				}

				// Set StateMachine Children Positions for Editor
				SetChildrenPositions();
			}

			private void CreateAnyStateTransitions()
			{
				int anyStateTransitionCount = StateMachineConstant.AnyStateTransitionConstantArray.Count;
				if (anyStateTransitionCount > 0 && StateContext.HasStates())
				{
					if (RootStateMachine.Has_AnyStateTransitions())
					{
						RootStateMachine.AnyStateTransitions.Capacity = anyStateTransitionCount;
						RootStateMachine.AnyStateTransitionsP.AddRange(GetAnyStateTransitions());
					}
					else
					{
						AssetList<PPtr_AnimatorStateTransition_4> newList = RootStateMachine.OrderedTransitions.AddNew().Value;
						newList.Capacity = anyStateTransitionCount;
						PPtrAccessList<PPtr_AnimatorStateTransition_4, IAnimatorStateTransition> anyStateTransitions2 = new(newList, RootStateMachine.Collection);
						anyStateTransitions2.AddRange(GetAnyStateTransitions());
					}
				}

				IEnumerable<IAnimatorStateTransition> GetAnyStateTransitions()
				{
					for (int i = 0; i < anyStateTransitionCount; i++)
					{
						ITransitionConstant transitionConstant = StateMachineConstant.AnyStateTransitionConstantArray[i].Data;
						IAnimatorStateTransition? transition = CreateAnimatorStateTransition(0, transitionConstant);
						if (transition != null)
						{
							yield return transition;
						}
					}
				}
			}

			private void AssignChildStates()
			{
				if (!HasSelectorStateConstant() || !StateContext.HasStates())
				{
					return;
				}

				// assign Name to StateMachines
				for (int i = 0; i < StateMachines.Length; i++)
				{
					uint fullPathID = StateMachineFullPathIDs[i];
					if (StateContext.TryGetStateMachinePath(fullPathID, out string fullPath))
					{
						IAnimatorStateMachine stateMachine = StateMachines[i];
						int pathDelimiterPos = fullPath.LastIndexOf('.');
						if (pathDelimiterPos != -1)
						{
							string stateMachinePath = fullPath[..pathDelimiterPos];
							if (StateContext.TryGetStateMachinePathID(stateMachinePath, out uint stateMachinePathID))
							{
								StateMachinePathIDs[i] = stateMachinePathID;
							}
							stateMachine.Name = fullPath[(pathDelimiterPos + 1)..];
						}
						else
						{
							// if FullPath doesn't have delimiter '.' , it should be Root StateMachine
							stateMachine.Name = fullPath;
						}
					}
					else
					{
						UnknownFullPaths++;
						// StateMachine with non recoverable FullPath, because doesn't contain States.
						// keep StateMachine Name empty as signal to keep looking for a FullPath/StateMachine Parent
						// through Transition connections
					}
				}

				// assign Child States and State Transitions.
				// has to be after assigning StateMachine Names,
				// to apply extra analysis for locating unknown StateMachine FullPaths
				for (int i = 0; i < StateMachines.Length; i++)
				{
					if (StateMachines[i].Name.IsEmpty)
					{
						continue;
					}
					uint fullPathID = StateMachineFullPathIDs[i];
					int childStateCount = StateContext.StateIdxsForStateMachine(fullPathID).Count();
					StateMachines[i].SetChildStateCapacity(childStateCount);
					foreach (int stateIdx in StateContext.StateIdxsForStateMachine(fullPathID))
					{
						AddStateAndTransitionsToStateMachine(i, stateIdx);
					}
				}
			}

			private void CreateEntryTransitions()
			{
				if (!HasSelectorStateConstant())
				{
					return;
				}
				for (int stateMachineIdx = 0; stateMachineIdx < StateMachines.Length; stateMachineIdx++)
				{
					IAnimatorStateMachine stateMachine = StateMachines[stateMachineIdx];
					SelectorTransitionConstant[]? entryTransitions = EntryTransitions[stateMachineIdx];
					if (entryTransitions == null)
					{
						continue;
					}

					// Entry Transitions for StateMachine
					stateMachine.SetEntryTransitionsCapacity(entryTransitions.Length - 1);
					for (int j = 0; j < entryTransitions.Length - 1; j++)
					{
						SelectorTransitionConstant selectorTransition = entryTransitions[j];
						IAnimatorTransition? transition = CreateAnimatorTransition(stateMachineIdx, true, selectorTransition);
						if (transition != null)
						{
							stateMachine.EntryTransitionsP.Add(transition);
						}
					}

					// Default State for StateMachine
					uint destination = entryTransitions[^1].Destination;
					if (destination != uint.MaxValue)
					{
						int defaultStateIdx = (int)destination;
						IAnimatorState defaultState = StateContext.GetState(defaultStateIdx);
						stateMachine.DefaultStateP = defaultState;
					}
				}
			}

			private IAnimatorTransition? CreateAnimatorTransition(int stateMachineSrcIdx, bool isEntrySrc, SelectorTransitionConstant transition)
			{
				if (!TryGetDestinationState(transition.Destination, out IAnimatorState? stateDst, out int stateMachineDstIdx, out bool isEntryDst))
				{
					return null;
				}

				IAnimatorTransition animatorTransition = VirtualFile.CreateAsset((int)ClassIDType.AnimatorTransition, AnimatorTransition.Create);
				animatorTransition.HideFlagsE = HideFlags.HideInHierarchy;

				animatorTransition.Conditions.Capacity = transition.ConditionConstantArray.Count;
				for (int i = 0; i < transition.ConditionConstantArray.Count; i++)
				{
					ConditionConstant conditionConstant = transition.ConditionConstantArray[i].Data;
					if (conditionConstant.ConditionMode != (int)AnimatorConditionMode.ExitTime)
					{
						IAnimatorCondition condition = animatorTransition.Conditions.AddNew();
						condition.ConditionMode = (int)conditionConstant.ConditionModeE;
						condition.ConditionEvent = Controller.TOS[conditionConstant.EventID];
						condition.EventTreshold = conditionConstant.EventThreshold;
					}
				}

				if (stateDst != null)
				{
					animatorTransition.DstStateP = stateDst;
				}
				else if (HasSelectorStateConstant() && stateMachineDstIdx != -1)
				{
					if (isEntryDst)
					{
						IAnimatorStateMachine stateMachineDst = StateMachines[stateMachineDstIdx];
						animatorTransition.DstStateMachineP = stateMachineDst;

						// try locate StateMachine of unknown FullPath, with Entry Transitions
						if (isEntrySrc && stateMachineDst.Name.IsEmpty)
						{
							uint stateMachineDstPathID = StateMachinePathIDs[stateMachineDstIdx];
							uint stateMachineSrcFullPathID = StateMachineFullPathIDs[stateMachineSrcIdx];

							if (!StateContext.TryGetStateMachinePath(stateMachineDstPathID, out string stateMachineDstPath) ||
								(StateContext.TryGetStateMachinePath(stateMachineSrcFullPathID, out string parentPath) &&
								IsDeeperHierarchy(stateMachineDstPath, parentPath)))
							{
								StateMachinePathIDs[stateMachineDstIdx] = stateMachineSrcFullPathID;
							}
						}
					}
					else
					{
						animatorTransition.IsExit = true;



					}
				}

				return animatorTransition;
			}

			private void AssignChildStateMachines()
			{
				if (!HasSelectorStateConstant())
				{
					return;
				}
				for (int childIdx = 1; childIdx < StateMachines.Length; childIdx++) // skipping 0 because its the Root StateMachine
				{
					IAnimatorStateMachine? parentStateMachine = null;
					IAnimatorStateMachine childStateMachine = StateMachines[childIdx];
					uint stateMachinePathID = StateMachinePathIDs[childIdx];
					if (stateMachinePathID == 0)
					{
						// set unknown stateMachines parent to Root StateMachine
						parentStateMachine = RootStateMachine;
					}
					else
					{
						// find parent
						for (int parentIdx = 0; parentIdx < StateMachines.Length; parentIdx++)
						{
							if (childIdx == parentIdx)
							{
								continue;
							}
							if (StateMachinePathIDs[childIdx] == StateMachineFullPathIDs[parentIdx])
							{
								parentStateMachine = StateMachines[parentIdx];
								break;
							}
						}
					}
					if (parentStateMachine == null)
					{
						// ensure parent for stateMachine
						parentStateMachine = RootStateMachine;
					}
					// ensure all StateMachines have Name
					if (childStateMachine.Name.IsEmpty)
					{
						childStateMachine.Name = $"Empty_{StateMachineFullPathIDs[childIdx]}";
					}

					// set Child StateMachine for its found Parent
					if (parentStateMachine.Has_ChildStateMachines())
					{
						ChildAnimatorStateMachine child = parentStateMachine.ChildStateMachines.AddNew();
						child.StateMachine.SetAsset(parentStateMachine.Collection, childStateMachine);
					}
					else
					{
						PPtr_AnimatorStateMachine_4 child = parentStateMachine.ChildStateMachine.AddNew();
						child.SetAsset(parentStateMachine.Collection, childStateMachine);
					}

					// add StateMachine Transitions (Transitions from Child are stored in Parent)
					SelectorTransitionConstant[]? exitTransitions = ExitTransitions[childIdx];
					if (exitTransitions != null)
					{
						// check if its the default exit transition, to not add it
						if (exitTransitions.Length == 1 && exitTransitions[0].ConditionConstantArray.Count == 0)
						{
							uint stateDstId = exitTransitions[0].Destination;
							if (stateDstId < StateMachineTransitionFlag)
							{
								int stateDstIdx = (int)stateDstId;
								int defaultStateIdx = StateContext.GetStateIdx(parentStateMachine.DefaultStateP);
								if (defaultStateIdx == stateDstIdx)
								{
									continue;
								}
							}
						}

						AssetPair<PPtr_AnimatorStateMachine_5, AssetList<PPtr_AnimatorTransition>> newPair =
								parentStateMachine.StateMachineTransitions!.AddNew();
						newPair.Key.SetAsset(parentStateMachine.Collection, childStateMachine);
						PPtrAccessList<PPtr_AnimatorTransition, IAnimatorTransition> transitions = new(newPair.Value, parentStateMachine.Collection);
						for (int j = 0; j < exitTransitions.Length; j++)
						{
							SelectorTransitionConstant selectorTransition = exitTransitions[j];
							IAnimatorTransition? transition = CreateAnimatorTransition(childIdx, false, selectorTransition);
							if (transition != null)
							{
								transitions.Add(transition);
							}
						}

						if (transitions.Count != 0)
						{
							newPair.Value.Capacity = newPair.Value.Count;
						}
						else
						{
							parentStateMachine.StateMachineTransitions.RemoveAt(parentStateMachine.StateMachineTransitions.Count-1);
						}
					}
				}

				// fix Child List Capacity
				foreach (IAnimatorStateMachine parent in StateMachines)
				{
					parent.TrimChildStateMachines();
				}
			}

			private void SetChildrenPositions()
			{
				const float StateOffset = 250.0f;
				foreach (IAnimatorStateMachine stateMachine in StateMachines)
				{
					int stateCount = stateMachine.ChildStatesCount();
					int stateMachineCount = stateMachine.ChildStateMachinesCount();
					int totalChildrenCount = stateCount + stateMachineCount;
					int side = (int)Math.Ceiling(Math.Sqrt(totalChildrenCount));

					for (int y = 0, i = 0; y < side && i < totalChildrenCount; y++)
					{
						for (int x = 0; x < side && i < totalChildrenCount; x++, i++)
						{
							Vector3f position = new() { X = x * StateOffset, Y = y * StateOffset };
							// Position all Child States first
							if (i < stateCount)
							{
								IAnimatorState? state;
								if (stateMachine.Has_ChildStates())
								{
									ChildAnimatorState childState = stateMachine.ChildStates[i];
									childState.Position.CopyValues(position);
									childState.State.TryGetAsset(stateMachine.Collection, out state);
								}
								else
								{
									state = stateMachine.StatesP[i];
								}
								state?.Position.CopyValues(position);
							}
							// Position all Child StateMachines second 
							else if (stateMachine.Has_ChildStateMachines())
							{
								// remember to handle Unity 5- SubStateMachines too
								ChildAnimatorStateMachine csm = stateMachine.ChildStateMachines[i - stateCount];
								csm.Position.CopyValues(position);
							}
							else
							{
								stateMachine.ChildStateMachinePosition.Add(position);
							}
						}
					}

					stateMachine.AnyStatePosition.SetValues(0.0f, -StateOffset, 0.0f);
					stateMachine.EntryPosition?.SetValues(StateOffset, -StateOffset, 0.0f);
					stateMachine.ExitPosition?.SetValues(2.0f * StateOffset, -StateOffset, 0.0f);
					stateMachine.ParentStateMachinePosition.SetValues(0.0f, -2.0f * StateOffset, 0.0f);
				}
			}

			private void FindParentStateMachineForUnknownFullPaths()
			{
				// Try set FullPaths from default Exit Transitions and Default States
				if (!HasSelectorStateConstant())
				{
					return;
				}
				for (int stateMachineIdx = 0; stateMachineIdx < StateMachines.Length; stateMachineIdx++)
				{
					// run only for StateMachine with unknown FullPaths; StateMachine Name not set yet
					if (!StateMachines[stateMachineIdx].Name.IsEmpty)
					{
						continue;
					}
					SelectorTransitionConstant[]? exitTransitions = ExitTransitions[stateMachineIdx];
					if (exitTransitions == null)
					{
						continue;
					}
					// check if its the default exit transition
					if (exitTransitions.Length != 1 ||
						exitTransitions[0].ConditionConstantArray.Count != 0 ||
						exitTransitions[0].Destination >= StateMachineTransitionFlag)
					{
						continue;
					}
					int exitStateIdx = (int)exitTransitions[0].Destination;

					uint selectedParentFullPathID = 0;
					for (int stateMachineParentIdx = 0; stateMachineParentIdx < StateMachines.Length; stateMachineParentIdx++)
					{
						if (stateMachineParentIdx == stateMachineIdx) // can't be its own Parent
						{
							continue;
						}
						IAnimatorState? defaultState = StateMachines[stateMachineParentIdx].DefaultStateP;
						if (defaultState == null)
						{
							continue;
						}
						int defaultStateIdx = StateContext.GetStateIdx(defaultState);
						if (defaultStateIdx == exitStateIdx)
						{
							// stateMachine can be Child of stateMachineParent.
							uint newParentFullPathID = StateMachineFullPathIDs[stateMachineParentIdx];
							// let the loop find more than one possible Parent, but keep the one with deeper hierarchy
							if (selectedParentFullPathID == 0 ||
								!StateContext.TryGetStateMachinePath(selectedParentFullPathID, out string selectedParentPath) ||
								(StateContext.TryGetStateMachinePath(newParentFullPathID, out string newParentPath) &&
								IsDeeperHierarchy(selectedParentPath, newParentPath)))
							{
								selectedParentFullPathID = newParentFullPathID;
							}
						}
					}
					if (selectedParentFullPathID != 0)
					{
						StateMachines[stateMachineIdx].Name = $"Empty_{StateMachineFullPathIDs[stateMachineIdx]}";
						StateMachinePathIDs[stateMachineIdx] = selectedParentFullPathID;
						UnknownFullPaths--;
					}
				}
			}
		}
	}
}
