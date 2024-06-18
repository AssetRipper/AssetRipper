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
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorStateMachine;
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

		private static void CreateEntryTransitions(
			IAnimatorStateMachine stateMachine,
			IStateMachineConstant stateMachineConstant,
			ProcessedAssetCollection file,
			uint FullPathID,
			IReadOnlyList<IAnimatorState> States,
			AssetDictionary<uint, Utf8String> TOS)
		{
			if (stateMachine.Has_EntryTransitions() && stateMachineConstant.Has_SelectorStateConstantArray())
			{
				foreach (SelectorStateConstant selector in stateMachineConstant.SelectorStateConstantArray)
				{
					if (selector.IsEntry && selector.FullPathID == FullPathID)
					{
						for (int i = 0; i < selector.TransitionConstantArray.Count - 1; i++)
						{
							SelectorTransitionConstant selectorTrans = selector.TransitionConstantArray[i].Data;
							// Entries only point to States; SubStateMachine[] and IAnimatorStateMachine can be null
							IAnimatorTransition transition = CreateAnimatorTransition(file, stateMachineConstant, States, null, null, TOS, selectorTrans);
							stateMachine.EntryTransitionsP.Add(transition);
						}

						// Default State
						int defaultStateIdx = (int)selector.TransitionConstantArray[^1].Data.Destination;
						IAnimatorState defaultState = States[defaultStateIdx];
						stateMachine.DefaultState.CopyValues(defaultState, new PPtrConverter(stateMachine));
						break;
					}
				}
			}
		}

		public static IAnimatorStateMachine CreateAnimatorStateMachine(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			IStateMachineConstant stateMachineConstant = controller.Controller.StateMachineArray[stateMachineIndex].Data;
			int layerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex);

			IAnimatorState[] states = InitializeAnimatorStates(virtualFile, controller, layerIndex, stateMachineIndex); // only missing their Transitions
			SubStateMachine[] stateMachines = InitializeSubStateMachines(virtualFile, controller, stateMachineConstant, layerIndex);
			IAnimatorStateMachine MainStateMachine = stateMachines[0].stateMachine; // assuming first SelectorStateConstant is always the Main/root StateMachine

			//AnyStateTransitions for Main StateMachine
			{
				int count = stateMachineConstant.AnyStateTransitionConstantArray.Count;
				if (MainStateMachine.Has_AnyStateTransitions())
				{
					MainStateMachine.AnyStateTransitions.Capacity = count;
					for (int i = 0; i < count; i++)
					{
						ITransitionConstant transitionConstant = stateMachineConstant.AnyStateTransitionConstantArray[i].Data;
						IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachineConstant, states, stateMachines, stateMachines[0], controller.TOS, transitionConstant);
						MainStateMachine.AnyStateTransitionsP.Add(transition);
					}
				}
				else
				{
					//https://github.com/AssetRipper/AssetRipper/issues/1028
					AssetList<PPtr_AnimatorStateTransition_4> newList = MainStateMachine.OrderedTransitions.AddNew().Value;
					newList.Capacity = count;
					PPtrAccessList<PPtr_AnimatorStateTransition_4, IAnimatorStateTransition> anyStateTransitions = new(newList, MainStateMachine.Collection);
					for (int i = 0; i < count; i++)
					{
						ITransitionConstant transitionConstant = stateMachineConstant.AnyStateTransitionConstantArray[i].Data;
						IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachineConstant, states, stateMachines, stateMachines[0], controller.TOS, transitionConstant);
						anyStateTransitions.Add(transition);
					}
				}
			}

			// Assign States to StateMachines and Create State Transitions
			ILayerConstant layer = controller.Controller.LayerArray[layerIndex].Data;
			AssignStatesToStateMachines(virtualFile, stateMachines, states, controller.TOS, stateMachineConstant, layer);

			// Create Entries and set Default States to StateMachines
			foreach (SubStateMachine stateMachine in stateMachines)
			{
				CreateEntryTransitions(stateMachine.stateMachine, stateMachineConstant, virtualFile, stateMachine.fullPathID, states, controller.TOS);
			}

			// Assign Child StateMachines to StateMachines
			AssignChildStateMachines(virtualFile, stateMachines, controller.TOS, stateMachineConstant);




			// Create StateMachineTransitions for StateMachines with solved FullPath aka with Parent
			// finish solving unknown StateMachines FullPaths aka assign some Parent, then create their StateMachineTransitions
			//




			// Set StateMachine Children Positions for Editor
			const float StateOffset = 250.0f;
			foreach (SubStateMachine ssm in stateMachines)
			{
				IAnimatorStateMachine stateMachine = ssm.stateMachine;
				int stateCount = stateMachine.Has_ChildStates() ? stateMachine.ChildStates.Count : stateMachine.StatesP.Count;
				int stateMachineCount = stateMachine.Has_ChildStateMachines() ? stateMachine.ChildStateMachines.Count : 0; // replace '0' when recovering Unity 5- SubStateMachines
				int totalChildrenCount = stateCount + stateMachineCount;
				int side = (int)Math.Ceiling(Math.Sqrt(totalChildrenCount));

				for (int y = 0, i = 0; y < side && i < totalChildrenCount; y++)
				{
					for (int x = 0; x < side && i < totalChildrenCount; x++, i++)
					{
						Vector3f position = new() { X = x * StateOffset, Y = y * StateOffset };
						
						if (i < stateCount) // Position all Child States first
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
						else if (stateMachine.Has_ChildStateMachines()) // Position all Child StateMachines second 
						{
							// remember to handle Unity 5- SubStateMachines too
							ChildAnimatorStateMachine csm = stateMachine.ChildStateMachines[i - stateCount];
							csm.Position.CopyValues(position);
						}
					}
				}

				stateMachine.AnyStatePosition.SetValues(0.0f, -StateOffset, 0.0f);
				stateMachine.EntryPosition?.SetValues(StateOffset, -StateOffset, 0.0f);
				stateMachine.ExitPosition?.SetValues(2.0f * StateOffset, -StateOffset, 0.0f);
				stateMachine.ParentStateMachinePosition.SetValues(0.0f, -2.0f * StateOffset, 0.0f);
			}

			return MainStateMachine;
		}

		private static IAnimatorState[] InitializeAnimatorStates(ProcessedAssetCollection virtualFile, IAnimatorController controller, int layerIndex, int stateMachineIndex)
		{
			if (!controller.TOS.ContainsKey(0))
			{
				controller.TOS[0] = Utf8String.Empty;
			}

			IStateMachineConstant stateMachine = controller.Controller.StateMachineArray[stateMachineIndex].Data;
			int stateCount = stateMachine.StateConstantArray.Count;

			IAnimatorState[] states = new IAnimatorState[stateCount];
			for (int i = 0; i < stateCount; i++)
			{
				IStateConstant stateConstant = stateMachine.StateConstantArray[i].Data;
				IAnimatorState state = CreateAnimatorState(virtualFile, controller, controller.TOS, layerIndex, stateConstant);
				states[i] = state;
			}
			return states;
		}

		private static SubStateMachine[] InitializeSubStateMachines(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
		{
			if (!stateMachineConstant.Has_SelectorStateConstantArray())
			{
				// not generating SubStateMachines for Unity 5- (Min to 5) yet
				IAnimatorStateMachine stateMachine = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateMachine, AnimatorStateMachine.Create);
				stateMachine.HideFlagsE = HideFlags.HideInHierarchy;
				return [new SubStateMachine(stateMachine, 0)];
			}
			// can have SubStateMachines

			// SelectorStateConstantArray contains Entry and Exit points for StateMachines
			// SelectorStateConstantArray = [Entry1, Exit1, Entry2, Exit2, ...]
			// just in case, next code handles StateMachine missing Entry or Exit SelectorStateConstant
			int stateMachineCount = 0;
			uint lastFullPathID = 0;
			foreach (SelectorStateConstant ssc in stateMachineConstant.SelectorStateConstantArray)
			{
				if (lastFullPathID != ssc.FullPathID)
				{
					lastFullPathID = ssc.FullPathID;
					stateMachineCount++;
				}
			}
			SubStateMachine[] StateMachines = new SubStateMachine[stateMachineCount];

			stateMachineCount = 0;
			lastFullPathID = 0;
			foreach (SelectorStateConstant ssc in stateMachineConstant.SelectorStateConstantArray)
			{
				uint sscFullPathID = ssc.FullPathID;
				if (lastFullPathID != sscFullPathID)
				{
					IAnimatorStateMachine stateMachine = virtualFile.CreateAsset((int)ClassIDType.AnimatorStateMachine, AnimatorStateMachine.Create);
					stateMachine.HideFlagsE = HideFlags.HideInHierarchy;
					// can set StateMachine behaviours now
					IMonoBehaviour?[] stateBehaviours = controller.GetStateBehaviours(layerIndex, sscFullPathID);
					stateMachine.StateMachineBehavioursP.AddRange(stateBehaviours);

					SubStateMachine ssm = new(stateMachine, sscFullPathID); // record class to pair IAnimatorStateMachine with some SelectorStateConstant data
					StateMachines[stateMachineCount] = ssm;

					lastFullPathID = ssc.FullPathID;
					stateMachineCount++;
				}
			}

			return StateMachines;
		}

		private static void SetStateMachineCapacity(IAnimatorStateMachine stateMachine, int c)
		{
			if (stateMachine.Has_ChildStates())
			{
				stateMachine.ChildStates.Capacity = c;
			}
			else if (stateMachine.Has_States())
			{
				stateMachine.States.Capacity = c;
			}
		}

		private static void AddStateToStateMachine(
			ProcessedAssetCollection virtualFile, AssetDictionary<uint, Utf8String> TOS, IStateMachineConstant stateMachineConstant,
			SubStateMachine ssm, SubStateMachine[] stateMachines,
			int stateIdx, IAnimatorState[] states)
		{
			// -- add child States to stateMachine --
			IAnimatorState state = states[stateIdx];
			IStateConstant stateConstant = stateMachineConstant.StateConstantArray[stateIdx].Data;
			IAnimatorStateMachine stateMachine = ssm.stateMachine;

			if (stateMachine.Has_ChildStates())
			{
				ChildAnimatorState childState = stateMachine.ChildStates.AddNew();
				// set childState.Position later, when having all Children set
				childState.State.SetAsset(stateMachine.Collection, state);
			}
			else
			{
				stateMachine.StatesP.Add(state);
			}
			// set state.Position later, when having all Children set

			// -- add State Transitions --
			AssetList<PPtr_AnimatorStateTransition_4>? transitionList = null;
			if (state.Has_Transitions())
			{
				state.Transitions.Capacity = stateConstant.TransitionConstantArray.Count;
			}
			else if (stateMachine.Has_OrderedTransitions())
			{
				//I'm not sure if this is correct, but it seems to be the only logical way to store the transitions before Unity 5.
				//IAnimatorStateMachine.LocalTransitions only exists until Unity 4.2.0, so by process of elimination, this is the only option.

				AssetPair<PPtr_AnimatorState_4, AssetList<PPtr_AnimatorStateTransition_4>> pair = stateMachine.OrderedTransitions.AddNew();
				pair.Key.SetAsset(stateMachine.Collection, state);
				transitionList = pair.Value;
			}
			for (int j = 0; j < stateConstant.TransitionConstantArray.Count; j++)
			{
				ITransitionConstant transitionConstant = stateConstant.TransitionConstantArray[j].Data;
				IAnimatorStateTransition transition = CreateAnimatorStateTransition(virtualFile, stateMachineConstant, states, stateMachines, ssm, TOS, transitionConstant);
				if (state.Has_Transitions())
				{
					state.TransitionsP.Add(transition);
				}
				else
				{
					transitionList?.AddNew().SetAsset(stateMachine.Collection, transition);
				}
			}
		}

		private static void AssignStatesToStateMachines(ProcessedAssetCollection virtualFile, SubStateMachine[] stateMachines, IAnimatorState[] states, AssetDictionary<uint, Utf8String> TOS, IStateMachineConstant stateMachineConstant, ILayerConstant layer)
		{
			// -- without SubStateMachines --
			if (!stateMachineConstant.Has_SelectorStateConstantArray())
			{
				// Unity Min to 5
				IAnimatorStateMachine MainStateMachine = stateMachines[0].stateMachine;
				IStateConstant stateConstant = stateMachineConstant.StateConstantArray[0].Data;
				MainStateMachine.Name = TOS[layer.Binding].String.Replace('.', '_');
				SetStateMachineCapacity(MainStateMachine, states.Length);
				for (int i = 0; i < states.Length; i++)
				{
					AddStateToStateMachine(virtualFile, TOS, stateMachineConstant, stateMachines[0], stateMachines, i, states);
				}
				return;
			}
			// Unity 5 to Max
			if (stateMachines.Length == 1)
			{
				IAnimatorStateMachine MainStateMachine = stateMachines[0].stateMachine;
				IStateConstant stateConstant = stateMachineConstant.StateConstantArray[0].Data;
				string stateFullPath = TOS[stateConstant.FullPathID].String;
				int pathDelimiterPos = stateFullPath.IndexOf('.');
				MainStateMachine.Name = stateFullPath[..pathDelimiterPos];
				SetStateMachineCapacity(MainStateMachine, states.Length);
				for (int i = 0; i < states.Length; i++)
				{
					AddStateToStateMachine(virtualFile, TOS, stateMachineConstant, stateMachines[0], stateMachines, i, states);
				}
				return;
			}

			// -- with SubStateMachines --

			Dictionary<string, List<int>?> GroupedStates = new(capacity: stateMachines.Length); // collect State groups and SubStateMachine FullPaths
			// Going to add possible SubStateMachine FullPaths to TOS, tho it could be a separate Dictionary/lookup
			for (int i = 0; i < states.Length; i++)
			{
				// add State to group
				IStateConstant stateConstant = stateMachineConstant.StateConstantArray[i].Data;
				string stateFullPath = TOS[stateConstant.FullPathID].String;
				int pathDelimiterPos = stateFullPath.LastIndexOf('.');
				string stateMachineFullPath = stateFullPath[..pathDelimiterPos];
				if (!GroupedStates.TryGetValue(stateMachineFullPath, out List<int>? group))
				{
					uint FullPathID = Checksum.Crc32Algorithm.HashUTF8(stateMachineFullPath);
					if (!TOS.ContainsKey(FullPathID))
					{
						TOS.Add(FullPathID, stateMachineFullPath);// FullPath into TOS
					}
				}
				if (group == null)
				{
					group = new();
					GroupedStates[stateMachineFullPath] = group;
				}
				group.Add(i);

				// calculate possible SubStateMachine paths
				pathDelimiterPos = stateMachineFullPath.LastIndexOf('.');
				while (pathDelimiterPos != -1)
				{
					stateMachineFullPath = stateMachineFullPath[..pathDelimiterPos];
					if (GroupedStates.ContainsKey(stateMachineFullPath))
					{
						break;
					}
					else
					{
						GroupedStates[stateMachineFullPath] = null;
						uint FullPathID = Checksum.Crc32Algorithm.HashUTF8(stateMachineFullPath);
						if (!TOS.ContainsKey(FullPathID))
						{
							TOS.Add(FullPathID, stateMachineFullPath);// FullPath into TOS
						}
					}
					pathDelimiterPos = stateMachineFullPath.LastIndexOf('.');
				}
			}
			GroupedStates.RemoveAll(kvp => kvp.Value == null); // remove Keys with null Values. Those were only used to fill TOS
			GroupedStates.TrimExcess(); // fix Capacity 
			TOS.Capacity = GroupedStates.Count; // fix Capacity

			// assign Name and Path to SubStateMachines
			foreach (SubStateMachine ssm in stateMachines)
			{
				uint FullPathID = ssm.fullPathID;
				if (TOS.TryGetValue(FullPathID, out Utf8String? _FullPath))
				{
					string FullPath = _FullPath;
					IAnimatorStateMachine stateMachine = ssm.stateMachine;
					int pathDelimiterPos = FullPath.LastIndexOf('.');
					if (pathDelimiterPos != -1)
					{
						ssm.path = FullPath[..pathDelimiterPos];
						stateMachine.Name = FullPath[(pathDelimiterPos+1)..];
					}
					else
					{
						// if FullPath doesn't have delimiter '.' , it should be Main/root StateMachine. path = ""
						stateMachine.Name = FullPath;
					}
				}
				/*else
				{
					// SubStateMachine with unknown FullPath => because doesn't contain States
				}*/
			}

			// assign States and State Transitions
			// has to be after assigning all Names, to have extra analysis for locating unknown SubStateMachine FullPaths
			foreach (SubStateMachine ssm in stateMachines)
			{
				uint FullPathID = ssm.fullPathID;
				if (TOS.TryGetValue(FullPathID, out Utf8String? _FullPath))
				{
					string FullPath = _FullPath;
					IAnimatorStateMachine stateMachine = ssm.stateMachine;
					if (GroupedStates.TryGetValue(FullPath, out List<int>? stateGroup))
					{
						SetStateMachineCapacity(stateMachine, stateGroup!.Count);
						foreach (int stateIdx in stateGroup!)
						{
							AddStateToStateMachine(virtualFile, TOS, stateMachineConstant, ssm, stateMachines, stateIdx, states);
						}
					}
				}
			}
		}

		private static void AssignChildStateMachines(ProcessedAssetCollection virtualFile, SubStateMachine[] stateMachines, AssetDictionary<uint, Utf8String> TOS, IStateMachineConstant stateMachineConstant)
		{
			for (int i = 1; i < stateMachines.Length; i++)
			{
				SubStateMachine ssm = stateMachines[i];
				if (ssm.stateMachine.Name.IsEmpty) // unknown FullPath yet
				{
					continue;
				}
				foreach (SubStateMachine parent in stateMachines)
				{
					if (TOS[parent.fullPathID] == ssm.path)
					{
						if (parent.stateMachine.Has_ChildStateMachines())
						{
							ChildAnimatorStateMachine child = parent.stateMachine.ChildStateMachines.AddNew();
							child.StateMachine.SetAsset(parent.stateMachine.Collection, ssm.stateMachine);
						}
						/*else if (parent.stateMachine.Has_ChildStateMachine())
						{
							// this may be how SubStateMachines are added on Unity Min to 5
							// but current code only generates SubStateMachines for Unity 5+
							SourceGenerated.Subclasses.PPtr_AnimatorStateMachine.PPtr_AnimatorStateMachine_4 child = parent.stateMachine.ChildStateMachine.AddNew();
							child.SetAsset(parent.stateMachine.Collection, ssm.stateMachine);
						}*/
						break;
					}
				}
			}
		}

		private static IAnimatorState CreateAnimatorState(ProcessedAssetCollection virtualFile, IAnimatorController controller, AssetDictionary<uint, Utf8String> tos, int layerIndex, IStateConstant state)
		{
			IAnimatorState generatedState = virtualFile.CreateAsset((int)ClassIDType.AnimatorState, AnimatorState.Create);
			generatedState.HideFlagsE = HideFlags.HideInHierarchy;

			generatedState.Name = tos[state.NameID];

			generatedState.Speed = state.Speed;
			generatedState.CycleOffset = state.CycleOffset;

			// skip Transitions because not all state exists at this moment

			if (generatedState.Has_StateMachineBehaviours())
			{
				uint stateID = GetIdForStateConstant(state);
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

			static uint GetIdForStateConstant(IStateConstant stateConstant)
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
		}

		private static IAnimatorStateTransition CreateAnimatorStateTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachineConstant,
			IReadOnlyList<IAnimatorState> States,
			SubStateMachine[] stateMachines,
			SubStateMachine parentStateMachine,
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

			if (TryGetDestinationState(Transition.DestinationState, StateMachineConstant, States, stateMachines,
				out IAnimatorState? state, out SubStateMachine? ssm, out bool isEntry))
			{
				if (state != null)
				{
					animatorStateTransition.DstStateP = state;
				}
				else if (ssm != null)
				{
					IAnimatorStateMachine stateM = ssm.stateMachine;
					if (isEntry || stateM != parentStateMachine.stateMachine)
					{
						animatorStateTransition.DstStateMachineP = stateM;
						if (stateM.Name.IsEmpty && isEntry)
						{
							// try locate StateMachine of unknown FullPath
							string parentPath = TOS[parentStateMachine.fullPathID];
							if (string.IsNullOrEmpty(ssm.path) || IsDeeperHierarchy(ssm.path, parentPath))
							{
								ssm.path = parentPath;
							}
						}
					}
					else
					{
						animatorStateTransition.IsExit = true;
					}
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

		private static IAnimatorTransition CreateAnimatorTransition(
			ProcessedAssetCollection virtualFile,
			IStateMachineConstant StateMachineConstant,
			IReadOnlyList<IAnimatorState> States,
			SubStateMachine[] stateMachines,
			IAnimatorStateMachine parentStateMachine,
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

			if (TryGetDestinationState(Transition.Destination, StateMachineConstant, States, stateMachines,
				out IAnimatorState? state, out SubStateMachine? ssm, out bool isEntry))
			{
				if (state != null)
				{
					animatorTransition.DstStateP = state;
				}
				else if (ssm != null)
				{
					IAnimatorStateMachine stateM = ssm.stateMachine;
					if (isEntry || stateM != parentStateMachine)
					{
						animatorTransition.DstStateMachineP = stateM;
					}
					else
					{
						animatorTransition.IsExit = true;
					}
				}
			}

			return animatorTransition;
		}

		private static bool TryGetDestinationState(uint destinationState, IStateMachineConstant stateMachineConstant,
			IReadOnlyList<IAnimatorState> states, SubStateMachine[] stateMachines,
			out IAnimatorState? state, out SubStateMachine? stateMachine, out bool isEntry)
		{
			state = null; stateMachine = null; isEntry = false;
			if (destinationState == uint.MaxValue)
			{
				return false;
			}
			if (destinationState >= 30000)
			{
				// Entry and Exit from StateMachines
				if (stateMachineConstant.Has_SelectorStateConstantArray())
				{
					uint stateIndex = destinationState % 30000;
					SelectorStateConstant selectorState = stateMachineConstant.SelectorStateConstantArray[(int)stateIndex].Data;
					stateMachine = GetStateMachineForId(stateMachines, selectorState.FullPathID);
					isEntry = selectorState.IsEntry;
					return true;
				}
				return false;
			}
			else
			{
				// State
				state = states[(int)destinationState];
				return true;
			}

			static SubStateMachine? GetStateMachineForId(SubStateMachine[] stateMachines, uint id)
			{
				foreach (SubStateMachine ssm in stateMachines)
				{
					if (ssm.fullPathID == id)
					{
						return ssm;
					}
				}
				return null;
			}
		}

		private static bool IsDeeperHierarchy(string currentPath, string newPath)
		{
			int currentDepth = currentPath.Count(ch => ch == '.');
			int newDepth = newPath.Count(ch => ch == '.');
			return newDepth > currentDepth;
		}

		private record SubStateMachine (IAnimatorStateMachine stateMachine, uint fullPathID)
		{
			public string path = "";
		}
	}
}
