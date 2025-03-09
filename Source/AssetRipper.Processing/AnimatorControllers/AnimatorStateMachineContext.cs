using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1101;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_1109;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorState;
using AssetRipper.SourceGenerated.Subclasses.ChildAnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorState;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateMachine;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorStateTransition;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimatorTransition;
using AssetRipper.SourceGenerated.Subclasses.SelectorStateConstant;
using AssetRipper.SourceGenerated.Subclasses.SelectorTransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.TransitionConstant;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;

namespace AssetRipper.Processing.AnimatorControllers
{
	public sealed class AnimatorStateMachineContext
	{
		public IAnimatorStateMachine RootStateMachine => StateMachines[0];

		private const uint StateMachineTransitionFlag = 30000;

		private readonly ProcessedAssetCollection VirtualFile;
		private readonly IAnimatorController Controller;
		private readonly IStateMachineConstant StateMachineConstant;
		private readonly int LayerIndex;
		private readonly ILayerConstant Layer;
		private readonly AnimatorStateContext StateContext;

		private IAnimatorStateMachine[] StateMachines = [];
		private uint[]? StateMachineFullPathIDs;
		private uint[]? StateMachinePathIDs;
		private SelectorTransitionConstant[]?[]? EntryTransitions;
		private SelectorTransitionConstant[]?[]? ExitTransitions;

		private int UnknownFullPaths = 0;
		private bool _HasSelectorStateConstant = false;

		[MemberNotNullWhen(true, nameof(StateMachineFullPathIDs), nameof(StateMachinePathIDs), nameof(EntryTransitions), nameof(ExitTransitions))]
		private bool HasSelectorStateConstant() => _HasSelectorStateConstant;

		public AnimatorStateMachineContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
		{
			VirtualFile = virtualFile;
			Controller = controller;
			StateMachineConstant = controller.Controller.StateMachineArray[stateMachineIndex].Data;
			LayerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex, out Layer);
			StateContext = new(virtualFile, controller, StateMachineConstant, LayerIndex);
		}

		/// <summary>
		/// Create all AnimatorStateMachines (Root and Children)
		/// </summary>
		public void Process()
		{
			StateContext.Process();

			InitializeStateMachines();

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
						StateMachines[stateMachineIdx] = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex, sscFullPathID);
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
							EntryTransitions[stateMachineIdx - 1] = transitions;
						}
						else
						{
							ExitTransitions[stateMachineIdx - 1] = transitions;
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
					IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
					stateMachine.Name = Controller.TOS[Layer.Binding].String.Replace('.', '_');
					StateMachines = [stateMachine];
				}
				else
				{
					IReadOnlyList<string> stateMachinePaths = StateContext.GetUniqueSMPaths();
					StateMachines = new IAnimatorStateMachine[stateMachinePaths.Count];

					// set Root StateMachine Name from DefaultState Path
					IAnimatorStateMachine mainStateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
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
							IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
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

			IAnimatorStateTransition animatorStateTransition = VirtualAnimationFactory.CreateAnimatorStateTransition(VirtualFile, Controller.TOS, Transition);

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

			IAnimatorTransition animatorTransition = VirtualAnimationFactory.CreateAnimatorTransition(VirtualFile, Controller.TOS, transition);

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
						parentStateMachine.StateMachineTransitions.RemoveAt(parentStateMachine.StateMachineTransitions.Count - 1);
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
