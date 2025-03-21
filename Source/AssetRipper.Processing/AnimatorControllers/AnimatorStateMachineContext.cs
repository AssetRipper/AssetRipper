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

namespace AssetRipper.Processing.AnimatorControllers;

internal sealed class AnimatorStateMachineContext
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
	/// <summary>
	/// Hashed paths of StateMachines, including their Names
	/// </summary>
	private uint[]? StateMachineFullPathIDs;
	/// <summary>
	/// Hashed paths of StateMachines, excluding their Names
	/// </summary>
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
			int stateMachineIndex = 0;
			uint lastFullPathID = 0;
			foreach (SelectorStateConstant ssc in StateMachineConstant.SelectorStateConstantArray)
			{
				SelectorTransitionConstant[]? transitions = ssc.TransitionConstantArray.Count == 0 ? null :
					ssc.TransitionConstantArray.Select(ptr => ptr.Data).ToArray();
				uint sscFullPathID = ssc.FullPathID;
				if (lastFullPathID != sscFullPathID)
				{
					StateMachines[stateMachineIndex] = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex, sscFullPathID);
					StateMachineFullPathIDs[stateMachineIndex] = ssc.FullPathID;
					if (ssc.IsEntry)
					{
						EntryTransitions[stateMachineIndex] = transitions;
					}
					else
					{
						ExitTransitions[stateMachineIndex] = transitions;
					}

					lastFullPathID = ssc.FullPathID;
					stateMachineIndex++;
				}
				else
				{
					if (ssc.IsEntry)
					{
						EntryTransitions[stateMachineIndex - 1] = transitions;
					}
					else
					{
						ExitTransitions[stateMachineIndex - 1] = transitions;
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
				StateMachines = new IAnimatorStateMachine[StateContext.GetUniqueStateMachinePathsCount()];
				// set Root StateMachine Name from DefaultState Path
				IAnimatorStateMachine mainStateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
				string mainStateMachineName = StateContext.GetStateMachinePath(StateContext.DefaultStateIndex);
				mainStateMachine.Name = mainStateMachineName;
				mainStateMachine.DefaultStateP = StateContext.GetState(StateContext.DefaultStateIndex);
				mainStateMachine.SetChildStateMachineCapacity(StateMachines.Length - 1);

				// ensure Root StateMachine will be at index 0
				StateMachines[0] = mainStateMachine;
				// initialize the rest of StateMachines
				int j = 1;
				foreach (string stateMachineName in StateContext.GetUniqueStateMachinePaths())
				{
					if (stateMachineName != mainStateMachineName)
					{
						IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
						stateMachine.Name = stateMachineName;
						StateMachines[j] = stateMachine;
						j++;
						// set Child StateMachines
						mainStateMachine.ChildStateMachineP.Add(stateMachine);
					}
				}

				// set Child States with their Transitions
				for (int i = 0; i < StateMachines.Length; i++)
				{
					int childStateCount = StateContext.StateIndicesForStateMachine(StateMachines[i].Name).Count();
					StateMachines[i].SetChildStateCapacity(childStateCount);
					foreach (int stateIndex in StateContext.StateIndicesForStateMachine(StateMachines[i].Name))
					{
						AddStateAndTransitionsToStateMachine(i, stateIndex);
					}
				}
			}
		}
	}

	private void AddStateAndTransitionsToStateMachine(int parentStateMachineIndex, int stateIndex)
	{
		// -- add child State to stateMachine --
		IAnimatorState state = StateContext.GetState(stateIndex);
		IStateConstant stateConstant = StateContext.GetStateConstant(stateIndex);
		IAnimatorStateMachine parentStateMachine = StateMachines[parentStateMachineIndex];
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
			IAnimatorStateTransition? transition = CreateAnimatorStateTransition(parentStateMachineIndex, transitionConstant);
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

	private IAnimatorStateTransition? CreateAnimatorStateTransition(int parentStateMachineIndex, ITransitionConstant Transition)
	{
		if (!TryGetDestinationState(Transition.DestinationState, out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination))
		{
			return null;
		}

		IAnimatorStateTransition animatorStateTransition = VirtualAnimationFactory.CreateAnimatorStateTransition(VirtualFile, Controller.TOS, Transition);

		if (stateDestination != null)
		{
			animatorStateTransition.DstStateP = stateDestination;
		}
		else if (HasSelectorStateConstant() && stateMachineDestinationIndex != -1)
		{
			IAnimatorStateMachine stateMachineDestination = StateMachines[stateMachineDestinationIndex];
			if (isEntryDestination)
			{
				animatorStateTransition.DstStateMachineP = stateMachineDestination;

				if (stateMachineDestination.Name.IsEmpty) // try locate StateMachine of unknown FullPath
				{
					uint stateMachineDestinationPathID = StateMachinePathIDs[stateMachineDestinationIndex];
					uint parentStateMachineFullPathID = StateMachineFullPathIDs[parentStateMachineIndex];

					if (!StateContext.TryGetStateMachinePath(stateMachineDestinationPathID, out string destinationPath) ||
						(StateContext.TryGetStateMachinePath(parentStateMachineFullPathID, out string parentPath) &&
						IsDeeperHierarchy(destinationPath, parentPath)))
					{
						StateMachinePathIDs[stateMachineDestinationIndex] = parentStateMachineFullPathID;
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
	out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination)
	{
		stateDestination = null;
		stateMachineDestinationIndex = -1;
		isEntryDestination = false;
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
				stateMachineDestinationIndex = GetStateMachineIndexForId(selectorState.FullPathID);
				isEntryDestination = selectorState.IsEntry;
				return true;
			}
			return false;
		}
		else
		{
			// State
			stateDestination = StateContext.GetState((int)destinationState);
			return true;
		}

		int GetStateMachineIndexForId(uint id)
		{
			if (StateMachineFullPathIDs == null)
				return -1;

			return StateMachineFullPathIDs.IndexOf(id);
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
			int childStateCount = StateContext.StateIndicesForStateMachine(fullPathID).Count();
			StateMachines[i].SetChildStateCapacity(childStateCount);
			foreach (int stateIndex in StateContext.StateIndicesForStateMachine(fullPathID))
			{
				AddStateAndTransitionsToStateMachine(i, stateIndex);
			}
		}
	}

	private void CreateEntryTransitions()
	{
		if (!HasSelectorStateConstant())
		{
			return;
		}
		for (int stateMachineIndex = 0; stateMachineIndex < StateMachines.Length; stateMachineIndex++)
		{
			IAnimatorStateMachine stateMachine = StateMachines[stateMachineIndex];
			SelectorTransitionConstant[]? entryTransitions = EntryTransitions[stateMachineIndex];
			if (entryTransitions == null)
			{
				continue;
			}

			// Entry Transitions for StateMachine
			stateMachine.SetEntryTransitionsCapacity(entryTransitions.Length - 1);
			for (int j = 0; j < entryTransitions.Length - 1; j++)
			{
				SelectorTransitionConstant selectorTransition = entryTransitions[j];
				IAnimatorTransition? transition = CreateAnimatorTransition(stateMachineIndex, true, selectorTransition);
				if (transition != null)
				{
					stateMachine.EntryTransitionsP.Add(transition);
				}
			}

			// Default State for StateMachine
			uint destination = entryTransitions[^1].Destination;
			if (destination != uint.MaxValue)
			{
				int defaultStateIndex = (int)destination;
				IAnimatorState defaultState = StateContext.GetState(defaultStateIndex);
				stateMachine.DefaultStateP = defaultState;
			}
		}
	}

	private IAnimatorTransition? CreateAnimatorTransition(int stateMachineSourceIndex, bool isEntrySource, SelectorTransitionConstant transition)
	{
		if (!TryGetDestinationState(transition.Destination, out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination))
		{
			return null;
		}

		IAnimatorTransition animatorTransition = VirtualAnimationFactory.CreateAnimatorTransition(VirtualFile, Controller.TOS, transition);

		if (stateDestination != null)
		{
			animatorTransition.DstStateP = stateDestination;
		}
		else if (HasSelectorStateConstant() && stateMachineDestinationIndex != -1)
		{
			if (isEntryDestination)
			{
				IAnimatorStateMachine stateMachineDestination = StateMachines[stateMachineDestinationIndex];
				animatorTransition.DstStateMachineP = stateMachineDestination;

				// try locate StateMachine of unknown FullPath, with Entry Transitions
				if (isEntrySource && stateMachineDestination.Name.IsEmpty)
				{
					uint stateMachineDestinationPathID = StateMachinePathIDs[stateMachineDestinationIndex];
					uint stateMachineSourceFullPathID = StateMachineFullPathIDs[stateMachineSourceIndex];

					if (!StateContext.TryGetStateMachinePath(stateMachineDestinationPathID, out string destinationPath) ||
						(StateContext.TryGetStateMachinePath(stateMachineSourceFullPathID, out string parentPath) &&
						IsDeeperHierarchy(destinationPath, parentPath)))
					{
						StateMachinePathIDs[stateMachineDestinationIndex] = stateMachineSourceFullPathID;
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
		for (int childIndex = 1; childIndex < StateMachines.Length; childIndex++) // skipping 0 because its the Root StateMachine
		{
			IAnimatorStateMachine? parentStateMachine = null;
			IAnimatorStateMachine childStateMachine = StateMachines[childIndex];
			uint stateMachinePathID = StateMachinePathIDs[childIndex];
			if (stateMachinePathID == 0)
			{
				// set unknown stateMachines parent to Root StateMachine
				parentStateMachine = RootStateMachine;
			}
			else
			{
				// find parent StateMachine
				for (int parentIndex = 0; parentIndex < StateMachines.Length; parentIndex++)
				{
					if (childIndex == parentIndex)
					{
						continue;
					}
					if (StateMachinePathIDs[childIndex] == StateMachineFullPathIDs[parentIndex])
					{
						parentStateMachine = StateMachines[parentIndex];
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
			if (childStateMachine.Name.IsEmpty) // || childStateMachine.Name == FlagName
			{
				// for recompiling the AnimatorController back to Release and getting the original StateMachine path hashes:
				//   stateMachine_FullPath = Crc32Algorithm.ReverseAscii(StateMachineFullPathIDs[childIndex], Parent_StateMachine_FullPath_Name + ".");
				//   stateMachine.Name = stateMachine_FullPath[(Parent_StateMachine_FullPath_Name.Length+1)..];
				// may need to check and set parent StateMachine Name recursively, if its still Empty or FlagName
				childStateMachine.Name = $"Empty_{StateMachineFullPathIDs[childIndex]}";
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
			SelectorTransitionConstant[]? exitTransitions = ExitTransitions[childIndex];
			if (exitTransitions != null)
			{
				// check if its the default exit transition, to not add it
				if (exitTransitions.Length == 1 && exitTransitions[0].ConditionConstantArray.Count == 0)
				{
					uint stateDestinationId = exitTransitions[0].Destination;
					if (stateDestinationId < StateMachineTransitionFlag)
					{
						int stateDestinationIndex = (int)stateDestinationId;
						int defaultStateIndex = StateContext.GetStateIndex(parentStateMachine.DefaultStateP);
						if (defaultStateIndex == stateDestinationIndex)
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
					IAnimatorTransition? transition = CreateAnimatorTransition(childIndex, false, selectorTransition);
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
					parentStateMachine.StateMachineTransitions.RemoveAt(^1);
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
		for (int stateMachineIndex = 0; stateMachineIndex < StateMachines.Length; stateMachineIndex++)
		{
			// run only for StateMachine with unknown FullPaths; StateMachine Name not set yet
			if (!StateMachines[stateMachineIndex].Name.IsEmpty)
			{
				continue;
			}
			SelectorTransitionConstant[]? exitTransitions = ExitTransitions[stateMachineIndex];
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
			int exitStateIndex = (int)exitTransitions[0].Destination;

			uint selectedParentFullPathID = 0;
			for (int stateMachineParentIndex = 0; stateMachineParentIndex < StateMachines.Length; stateMachineParentIndex++)
			{
				if (stateMachineParentIndex == stateMachineIndex) // can't be its own Parent
				{
					continue;
				}
				IAnimatorState? defaultState = StateMachines[stateMachineParentIndex].DefaultStateP;
				if (defaultState == null)
				{
					continue;
				}
				int defaultStateIndex = StateContext.GetStateIndex(defaultState);
				if (defaultStateIndex == exitStateIndex)
				{
					// stateMachine can be Child of stateMachineParent.
					uint newParentFullPathID = StateMachineFullPathIDs[stateMachineParentIndex];
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
				// for recompiling the AnimatorController back to Release and getting the original StateMachine path hashes:
				//   stateMachine_FullPath = Crc32Algorithm.ReverseAscii(StateMachineFullPathIDs[stateMachineIndex], Parent_StateMachine_FullPath_Name + ".");
				//   stateMachine.Name = stateMachine_FullPath[(Parent_StateMachine_FullPath_Name.Length+1)..];
				// but don't do it here, not all StateMachine parents have been set yet; set a temporary flag Name to fix later
				// example FlagName = "." (period is not allowed in StateMachine Name)
				StateMachines[stateMachineIndex].Name = $"Empty_{StateMachineFullPathIDs[stateMachineIndex]}";
				StateMachinePathIDs[stateMachineIndex] = selectedParentFullPathID;
				UnknownFullPaths--;
			}
		}
	}
}
