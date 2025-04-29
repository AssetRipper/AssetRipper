using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Checksum;
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
using System.Numerics;

namespace AssetRipper.Processing.AnimatorControllers;

internal sealed class AnimatorStateMachineContext
{
	public IAnimatorStateMachine RootStateMachine => IndexedStateMachines[0].StateMachine;

	private const uint StateMachineTransitionFlag = 30000;

	private readonly ProcessedAssetCollection VirtualFile;
	private readonly IAnimatorController Controller;
	private readonly IStateMachineConstant StateMachineConstant;
	private readonly int LayerIndex;
	private readonly ILayerConstant Layer;
	private readonly AnimatorStateContext StateContext;
	private readonly bool IsUnity5;

	[field: MaybeNull]
	private StateMachineData[] IndexedStateMachines
	{
		get => field ?? throw new NullReferenceException(nameof(IndexedStateMachines));
		set;
	}

	/// <summary>
	/// Restrictions for parenting Unknown StateMachines. Dict keys are the StateMachine array indexes
	/// </summary>
	private readonly Dictionary<int, ParentingRestrictions> UnknownStateMachineRestrictions = new();
	private readonly Dictionary<int, List<int>> UnknownStateMachinesRelations = new();

	private int UnknownFullPaths = 0;
	/// <summary>
	/// Used/Created when Unknown StateMachines couldn't be placed in any other FullPath StateMachine
	/// </summary>
	private IAnimatorStateMachine? ExtraStateMachine = null;
	/// <summary>
	/// A temporary invalid name: "."
	/// </summary>
	private static readonly Utf8String StateMachineFlagName = new(".");

	public AnimatorStateMachineContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, int stateMachineIndex)
	{
		VirtualFile = virtualFile;
		Controller = controller;
		StateMachineConstant = controller.Controller.StateMachineArray[stateMachineIndex].Data;
		LayerIndex = controller.Controller.GetLayerIndexByStateMachineIndex(stateMachineIndex, out Layer);
		StateContext = new(virtualFile, controller, StateMachineConstant, LayerIndex);
		IsUnity5 = StateMachineConstant.Has_SelectorStateConstantArray();
	}

	/// <summary>
	/// Create all AnimatorStateMachines (Root and Children)
	/// </summary>
	public void Process()
	{
		StateContext.Process();

		InitializeStateMachines();

		if (IsUnity5) // Unity 5+
		{
			AssignStateMachineNames(); // assign Names and parents (FullPaths) to fully determined StateMachines

			// Goal for Unknown-FullPath StateMachines: place them as deep in hierarchy as possible.
			// Try to keep Unknown StateMachines that have Transitions between each other together,
			//   not nesting Unknown StateMachines into anothers, but having them "side-by-side" under the same fully determined (FullPath) parent.
			// Unknown StateMachines shouldn't contain any other StateMachine, except the following case...
			if (UnknownFullPaths != 0)
			{
				// Only child StateMachines (and child States) have access to their parent StateMachine ExitState
				// Try use this to find the true parents for some Unknown-FullPath StateMachines
				LocateUnknownStateMachinesWithExitStateTransition(); // solve more FullPaths; may leave some StateMachines "flagged"
				// Flagged StateMachines have a set parent, but that parent is Unknown-FullPath, so the Flagged StateMachine can't solve its FullPath yet

				if (UnknownFullPaths != 0)
				{
					// If the previous method didn't solve all Unknown StateMachines,
					//   will need to find restrictions for placing them in hierarchy.
					RestrictUnknownStateMachinePossibleParents();
				}
			}

			AssignStateMachineChildStates(); // assign child States to StateMachines and Create State Transitions
			
			CreateAnyStateTransitions(); // create AnyState Transitions for Root StateMachine
			
			if (UnknownFullPaths != 0)
			{
				// All State Transitions have been used for assigning possible StateMachine parents (parents only with FullPaths)
				//   to StateMachines with Unknown-FullPaths (Unknowns don't have Name neither).
				// There can still be Unknowns with no assigned possible parent.

				// Will try to use Entry StateMachine Transitions to locate them the same way like with State Transitions (deeper hierarchy wins),
				//   also only assigning parents with FullPath.
				LocateUnknownStateMachinesWithEntryTransition(true);

				// Then will try use Exit StateMachine Transitions to locate Unknowns.
				// Transitions from Unknowns to States will also follow "deeper hierarchy" rule.
				LocateUnknownStateMachinesWithEntryTransition(false);

				// During Entry and Exit Transitions scan, when a source Unknown went to a destination Unknown,
				//   a link was saved in UnknownStateMachinesRelations.
				// Use those links to try assign/share parents.
				ShareParentsBetweenUnknownStateMachines();

				// There may be Unknowns without possible parent (no connections/Transitions to FullPaths, or parenting was too restricted),
				//   and there may also be some Flaggeds.
				// Iterate through all FullPath StateMachines again and try find valid parents for Unknowns.
				FindParentsForLastUnknownStateMachines();

				// Finalize parenting for Unknown StateMachines with possible parent (generate FullPaths and Names for them)
				PromoteCurrentParentings();

				// All StateMachines have Name and possible parent now
			}

			CreateEntryTransitions(); // create Entry Transitions and set StateMachine Default States

			AssignChildStateMachines(); // Assign Child StateMachines and Create StateMachine Transitions
		}
		else // Unity 4-
		{
			// create AnyStateTransitions for Root StateMachine
			CreateAnyStateTransitions();
		}

		// Set StateMachine Children Positions for Editor
		SetChildrenPositions();
	}

	private void FindParentsForLastUnknownStateMachines()
	{
		bool allUnknownsReceivedParent = true;
		for (int i = 1; i < IndexedStateMachines.Length; i++) // skipping Root StateMachine at 0
		{
			StateMachineData stateMachineData = IndexedStateMachines[i];
			if (!stateMachineData.Name.IsEmpty || stateMachineData.ParentFullPathID != 0)
				continue; // iterate only through Unknowns without a possible parent

			bool parentAssigned = false;
			foreach (StateMachineData possibleParentData in IndexedStateMachines)
			{
				if (possibleParentData.Name.IsEmpty || possibleParentData.Name == StateMachineFlagName)
					continue; // only use FullPaths as parents

				parentAssigned = TryAssignPossibleParent(i, possibleParentData.FullPathID, false);
				if (parentAssigned)
					break; // if parent was assigned, don't check the rest of possible parents
			}

			allUnknownsReceivedParent = allUnknownsReceivedParent && parentAssigned;
		}

		if (!allUnknownsReceivedParent)
		{
			// If no valid parent found for the last Unknowns,
			//   will have to create an Extra StateMachine under Root, and put them in there (last resort, very unlikely)
			ExtraStateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
			ExtraStateMachine.Name = "Extra StateMachine";
			StateContext.AddStateMachineFullPath($"{RootStateMachine.Name}.{ExtraStateMachine.Name}", 0); // this will link all Unknowns without possible parent to ExtraStateMachine
			
			// give ExtraStateMachine a (Default) State to turn it into a FullPath StateMachine, in case of "re-ripping" this Animator Controller
			IAnimatorState ExtraState = VirtualAnimationFactory.CreateDefaultAnimatorState(VirtualFile);
			ChildAnimatorState childState = ExtraStateMachine.ChildStates!.AddNew();
			childState.State.SetAsset(ExtraStateMachine.Collection, ExtraState);
			ExtraStateMachine.DefaultStateP = ExtraState;
		}
	}

	private void PromoteCurrentParentings()
	{
		List<int> stateMachineIndexes = new();

		for (int i = 1; i < IndexedStateMachines.Length; i++) // skipping Root StateMachine at 0
		{
			StateMachineData stateMachine = IndexedStateMachines[i];
			if (stateMachine.Name.IsEmpty || // include Unknown StateMachines
				stateMachine.Name == StateMachineFlagName) // and Flagged StateMachines, in case its Unknown parent gets resolved
			{
				stateMachineIndexes.Add(i);
			}
		}

		GenerateFullPathsForUnknownStateMachines(stateMachineIndexes);
	}

	private void ShareParentsBetweenUnknownStateMachines()
	{
		bool linksUpdated = true;
		while (linksUpdated)
		{
			linksUpdated = false;

			foreach (KeyValuePair<int, List<int>> kvp in UnknownStateMachinesRelations)
			{
				int stateMachineIndex = kvp.Key;
				uint shareParentFullPathID = IndexedStateMachines[stateMachineIndex].ParentFullPathID;

				if (shareParentFullPathID == 0)
					continue; // skip StateMachines that don't have a possible parent to share

				List<int> linkedStateMachineIndexes = kvp.Value;
				foreach (int linkedStateMachineIndex in linkedStateMachineIndexes)
				{
					if (TryAssignPossibleParent(linkedStateMachineIndex, shareParentFullPathID))
					{
						// Can't have infinite loop, because of "strictly deeper in hierarchy" restriction in TryAssignPossibleParent.
						// There isn't infinite possible parents always deeper than previous ones.
						linksUpdated = true;
					}
				}
			}
		}
	}

	private void InitializeStateMachines()
	{
		if (StateMachineConstant.Has_SelectorStateConstantArray())
		{
			// Unity 5+

			int stateMachineCount =  StateMachineConstant.StateMachineCount();
			IndexedStateMachines = new StateMachineData[stateMachineCount];

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
					IAnimatorStateMachine newStateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex, sscFullPathID);
					IndexedStateMachines[stateMachineIndex] = new(newStateMachine, sscFullPathID);
					if (ssc.IsEntry)
					{
						IndexedStateMachines[stateMachineIndex].EntryTransitions = transitions;
					}
					else
					{
						IndexedStateMachines[stateMachineIndex].ExitTransitions = transitions;
					}

					lastFullPathID = ssc.FullPathID;
					stateMachineIndex++;
				}
				else
				{
					if (ssc.IsEntry)
					{
						IndexedStateMachines[stateMachineIndex - 1].EntryTransitions = transitions;
					}
					else
					{
						IndexedStateMachines[stateMachineIndex - 1].ExitTransitions = transitions;
					}
				}
			}

			if (!StateContext.HasStates())
			{
				// No States to help resolve StateMachine Names
				// still can give Name to Root StateMachine using Layer
				string rootStateMachineName = Controller.TOS[Layer.Binding].String.Replace('.', '_');
				IndexedStateMachines[0].Name = rootStateMachineName;
			}
		}
		else
		{
			// Unity 4.x-

			int stateMachineCount = StateContext.HasStates() ? StateContext.GetUniqueStateMachinePathsCount() : 1;
			IndexedStateMachines = new StateMachineData[stateMachineCount];

			// StateMachines don't have FullPaths.
			// can set Names, Child States (with their Transitions),
			// Default State and Child StateMachines now
			if (!StateContext.HasStates())
			{
				// empty Main StateMachine. its better to resolve Name now
				IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
				stateMachine.Name = Controller.TOS[Layer.Binding].String.Replace('.', '_');
				IndexedStateMachines[0] = new(stateMachine);
			}
			else
			{
				// set Root StateMachine Name from DefaultState Path
				IAnimatorStateMachine mainStateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
				string mainStateMachineName = StateContext.GetStateMachinePath(StateContext.DefaultStateIndex);
				mainStateMachine.Name = mainStateMachineName;
				mainStateMachine.DefaultStateP = StateContext.GetState(StateContext.DefaultStateIndex);
				mainStateMachine.SetChildStateMachineCapacity(IndexedStateMachines.Length - 1);

				// ensure Root StateMachine will be at index 0
				IndexedStateMachines[0] = new (mainStateMachine);
				// initialize the rest of StateMachines
				int j = 1;
				foreach (string stateMachineName in StateContext.GetUniqueStateMachinePaths())
				{
					if (stateMachineName != mainStateMachineName)
					{
						IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateStateMachine(VirtualFile, Controller, LayerIndex);
						stateMachine.Name = stateMachineName;
						IndexedStateMachines[j] = new(stateMachine);
						j++;
						// set Child StateMachines
						mainStateMachine.ChildStateMachineP.Add(stateMachine);
					}
				}

				// set Child States with their Transitions
				for (int i = 0; i < IndexedStateMachines.Length; i++)
				{
					StateMachineData stateMachineData = IndexedStateMachines[i];
					int childStateCount = StateContext.StateIndicesForStateMachine(stateMachineData.FullPathID).Count();
					stateMachineData.StateMachine.SetChildStateCapacity(childStateCount);
					foreach (int stateIndex in StateContext.StateIndicesForStateMachine(stateMachineData.FullPathID))
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
		IAnimatorStateMachine parentStateMachine = IndexedStateMachines[parentStateMachineIndex].StateMachine;
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

		if (stateDestination != null) // destination is State
		{
			animatorStateTransition.DstStateP = stateDestination;
		}
		else if (IsUnity5 && stateMachineDestinationIndex != -1) // destination is StateMachine
		{
			IAnimatorStateMachine stateMachineDestination = IndexedStateMachines[stateMachineDestinationIndex].StateMachine;
			if (isEntryDestination)
			{
				animatorStateTransition.DstStateMachineP = stateMachineDestination;

				if (stateMachineDestination.Name.IsEmpty || stateMachineDestination.Name == StateMachineFlagName) // try locate StateMachine with Unknown FullPath
				{
					if (stateMachineDestination.Name == StateMachineFlagName) // Flagged StateMachine has a set parent already, apply possible parent to its Unknown parent
						stateMachineDestinationIndex = GetUnknownParentForFlaggedStateMachine(stateMachineDestinationIndex);

					uint parentStateMachineFullPathID = IndexedStateMachines[parentStateMachineIndex].FullPathID; // new possible parent. will have FullPath because it contains a State (from this Transition)
					TryAssignPossibleParent(stateMachineDestinationIndex, parentStateMachineFullPathID);
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
	}

	private int GetStateMachineIndexForId(uint fullPathID)
	{
		return IndexedStateMachines.IndexOf(ism => ism.FullPathID == fullPathID);
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

	private void AssignStateMachineNames()
	{
		if (!StateContext.HasStates())
			return;
		
		foreach (StateMachineData stateMachineData in IndexedStateMachines)
		{
			if (StateContext.TryGetStateMachinePath(stateMachineData.FullPathID, out string stateMachineFullPath))
			{
				// periods are used for concatenating State and StateMachine Names to get their paths.
				// Unity Editor doesn't allow periods in State and StateMachine Names.
				int pathDelimiterPos = stateMachineFullPath.LastIndexOf('.');
				if (pathDelimiterPos != -1)
				{
					string parentStateMachineFullPath = stateMachineFullPath[..pathDelimiterPos];
					if (StateContext.TryGetStateMachinePathID(parentStateMachineFullPath, out uint parentStateMachineFullPathID)) // this shouldn't fail because the FullPath was already found 
					{
						// the child StateMachine's PathID will match its parent StateMachine's FullPathID
						stateMachineData.ParentFullPathID = parentStateMachineFullPathID; // this links the i'th stateMachine with its true parent
						stateMachineData.Name = stateMachineFullPath[(pathDelimiterPos + 1)..]; // can set its true Name
					}
				}
				else
				{
					// if FullPath doesn't have delimiter '.' , it should be Root StateMachine
					stateMachineData.Name = stateMachineFullPath;
				}
			}
			else
			{
				UnknownFullPaths++;
				// StateMachine with non recoverable FullPath, because doesn't contain States.
				// Keep StateMachine Name empty as signal to keep looking for a fully determined (FullPath) StateMachine parent
				//   through Transition connections.
			}
		}
	}

	private void AssignStateMachineChildStates()
	{
		if (!StateContext.HasStates())
			return;

		// Assign Child States and State Transitions.
		// Has to be after assigning StateMachine Names/FullPaths and finding parenting restrictions,
		//   to apply extra analysis for locating Unknown-FullPath StateMachines
		for (int i = 0; i < IndexedStateMachines.Length; i++)
		{
			StateMachineData stateMachineData = IndexedStateMachines[i];
			if (stateMachineData.Name.IsEmpty)
				continue; // Unknown StateMachines don't contain States
			uint fullPathID = stateMachineData.FullPathID;
			int childStateCount = StateContext.StateIndicesForStateMachine(fullPathID).Count();
			stateMachineData.StateMachine.SetChildStateCapacity(childStateCount);
			if (childStateCount == 0)
				continue;
			foreach (int stateIndex in StateContext.StateIndicesForStateMachine(fullPathID))
			{
				AddStateAndTransitionsToStateMachine(i, stateIndex);
			}
		}
	}

	private void LocateUnknownStateMachinesWithEntryTransition(bool doEntryTransitions)
	{
		for (int _stateMachineSourceIndex = 0; _stateMachineSourceIndex < IndexedStateMachines.Length; _stateMachineSourceIndex++)
		{
			int stateMachineSourceIndex = _stateMachineSourceIndex;
			StateMachineData stateMachineSource = IndexedStateMachines[stateMachineSourceIndex];
			SelectorTransitionConstant[]? Transitions = doEntryTransitions ? stateMachineSource.EntryTransitions : stateMachineSource.ExitTransitions;
			if (Transitions == null)
				continue;

			if (stateMachineSource.Name == StateMachineFlagName)
			{
				// Flagged StateMachine has a set parent already, but that parent is Unknown-FullPath, so transfer the relations to parent
				stateMachineSourceIndex = GetUnknownParentForFlaggedStateMachine(stateMachineSourceIndex);
				stateMachineSource = IndexedStateMachines[stateMachineSourceIndex];
			}

			foreach (SelectorTransitionConstant selectorTransition in Transitions)
			{
				if (!TryGetDestinationState(selectorTransition.Destination, out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination))
					continue;

				if (stateDestination != null) // destination is State
				{
					if (!stateMachineSource.Name.IsEmpty)
						continue; // try assign possible parent only to Unknown StateMachines
					int stateIndex = (int)selectorTransition.Destination;
					AssignPossibleParentFromState(stateMachineSourceIndex, stateIndex);
				}
				else if ((doEntryTransitions || isEntryDestination) && // skip Exit Transition with ExitState destination
					stateMachineDestinationIndex != -1) // destination is StateMachine
				{
					StateMachineData stateMachineDestination = IndexedStateMachines[stateMachineDestinationIndex];
					if (stateMachineDestination.Name == StateMachineFlagName)
					{
						// Flagged StateMachine has a set parent already, but that parent is Unknown-FullPath, so transfer the relations to parent
						stateMachineDestinationIndex = GetUnknownParentForFlaggedStateMachine(stateMachineDestinationIndex);
						stateMachineDestination = IndexedStateMachines[stateMachineDestinationIndex];
					}

					int decision = (stateMachineSource.Name.IsEmpty ? 1 : 0) + (stateMachineDestination.Name.IsEmpty ? 2 : 0);
					switch (decision) // just want to avoid nesting IFs
					{
						case 0: // both have FullPaths already
							break;

						case 1: // Unknown StateMachine Source goes to FullPath StateMachine Destination
							if (doEntryTransitions)
							{
								TryAssignPossibleParent(stateMachineSourceIndex, stateMachineDestination.FullPathID);
							}
							else // due to the Exit Transition restrictions, will offer parent of Destination as possible parent for Source
							{
								if (stateMachineDestinationIndex != 0) // skip Root StateMachine, doesn't have parent!
									TryAssignPossibleParent(stateMachineSourceIndex, stateMachineDestination.ParentFullPathID);
							}
							break;

						case 2: // FullPath StateMachine Source goes to Unknown StateMachine Destination
							if (doEntryTransitions)
							{
								TryAssignPossibleParent(stateMachineDestinationIndex, stateMachineSource.FullPathID);
							}
							else // due to the Exit Transition restrictions, will offer parent of Source as possible parent for Destination
							{
								if (stateMachineSourceIndex != 0) // skip Root StateMachine, doesn't have parent!
									TryAssignPossibleParent(stateMachineDestinationIndex, stateMachineSource.ParentFullPathID);
							}
							break;

						case 3: // both are Unknown StateMachines, link them together for resolving hierarchy later
							{
								if (!UnknownStateMachinesRelations.TryGetValue(stateMachineSourceIndex, out List<int>? relatedStateMachinesIndexes))
								{
									relatedStateMachinesIndexes = new();
									UnknownStateMachinesRelations[stateMachineSourceIndex] = relatedStateMachinesIndexes;
								}
								if (!relatedStateMachinesIndexes.Contains(stateMachineDestinationIndex))
									relatedStateMachinesIndexes.Add(stateMachineDestinationIndex);
								if (!UnknownStateMachinesRelations.TryGetValue(stateMachineDestinationIndex, out relatedStateMachinesIndexes))
								{
									relatedStateMachinesIndexes = new();
									UnknownStateMachinesRelations[stateMachineDestinationIndex] = relatedStateMachinesIndexes;
								}
								if (!relatedStateMachinesIndexes.Contains(stateMachineSourceIndex))
									relatedStateMachinesIndexes.Add(stateMachineSourceIndex);
							}
							break;
					}
				}
			}
		}
	}

	private int GetUnknownParentForFlaggedStateMachine(int flagStateMachineIndex)
	{
		// There can be Flagged StateMachines nested into other Flaggeds,
		//   but going up its sub-hierarchy there will always be an Unknown-FullPath StateMachine (no Name)

		uint parentFullPathID = IndexedStateMachines[flagStateMachineIndex].ParentFullPathID;
		int parentIndex = GetStateMachineIndexForId(parentFullPathID);
		while (!IndexedStateMachines[parentIndex].Name.IsEmpty)
		{
			parentFullPathID = IndexedStateMachines[parentIndex].ParentFullPathID;
			parentIndex = GetStateMachineIndexForId(parentFullPathID);
		}
		return parentIndex;
	}

	private void CreateEntryTransitions()
	{
		foreach (StateMachineData stateMachineData in IndexedStateMachines)
		{
			IAnimatorStateMachine stateMachine = stateMachineData.StateMachine;
			SelectorTransitionConstant[]? entryTransitions = stateMachineData.EntryTransitions;
			if (entryTransitions == null)
			{
				continue;
			}

			// Entry Transitions for StateMachine
			stateMachine.SetEntryTransitionsCapacity(entryTransitions.Length - 1);
			for (int j = 0; j < entryTransitions.Length - 1; j++)
			{
				SelectorTransitionConstant selectorTransition = entryTransitions[j];
				IAnimatorTransition? transition = CreateAnimatorTransition(selectorTransition);
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

	private IAnimatorTransition? CreateAnimatorTransition(SelectorTransitionConstant transition)
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
		else if (IsUnity5 && stateMachineDestinationIndex != -1)
		{
			if (isEntryDestination)
			{
				IAnimatorStateMachine stateMachineDestination = IndexedStateMachines[stateMachineDestinationIndex].StateMachine;
				animatorTransition.DstStateMachineP = stateMachineDestination;
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
		for (int childIndex = 1; childIndex < IndexedStateMachines.Length; childIndex++) // skipping 0 because its the Root StateMachine
		{
			StateMachineData childStateMachineData = IndexedStateMachines[childIndex];
			IAnimatorStateMachine childStateMachine = childStateMachineData.StateMachine;
			IAnimatorStateMachine parentStateMachine;
			if (childStateMachineData.ParentFullPathID == 0) // Unknown StateMachines without possible parent are assigned to ExtraStateMachine
			{
				parentStateMachine = ExtraStateMachine!;
			}
			else
			{
				int parentIndex = GetStateMachineIndexForId(childStateMachineData.ParentFullPathID);
				parentStateMachine = IndexedStateMachines[parentIndex].StateMachine;
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
			SelectorTransitionConstant[]? exitTransitions = childStateMachineData.ExitTransitions;
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
					IAnimatorTransition? transition = CreateAnimatorTransition(selectorTransition);
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

		// set ExtraStateMachine as child of RootStateMachine
		if (ExtraStateMachine != null)
		{
			ChildAnimatorStateMachine child = RootStateMachine.ChildStateMachines!.AddNew();
			child.StateMachine.SetAsset(RootStateMachine.Collection, ExtraStateMachine);

			ExtraStateMachine.TrimChildStateMachines(); // fix Child List Capacity
		}

		// fix Child List Capacity
		foreach (StateMachineData parent in IndexedStateMachines)
		{
			parent.StateMachine.TrimChildStateMachines();
		}
	}

	private void SetChildrenPositions()
	{
		foreach (StateMachineData stateMachineData in IndexedStateMachines)
		{
			ProcessChildrenPosForStateMachine(stateMachineData.StateMachine);
		}
		if (ExtraStateMachine != null)
		{
			ProcessChildrenPosForStateMachine(ExtraStateMachine);
		}
	}

	private void ProcessChildrenPosForStateMachine(IAnimatorStateMachine stateMachine)
	{
		const int StateOffsetX = 250;
		const int StateOffsetY = 100;

		int stateCount = stateMachine.ChildStatesCount();
		int stateMachineCount = stateMachine.ChildStateMachinesCount();
		int totalChildrenCount = stateCount + stateMachineCount;
		int side = (int)Math.Ceiling(Math.Sqrt(totalChildrenCount));

		for (int y = 0, i = 0; y < side && i < totalChildrenCount; y++)
		{
			for (int x = 0; x < side && i < totalChildrenCount; x++, i++)
			{
				Vector3 position = new() { X = x * StateOffsetX, Y = y * StateOffsetY };
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
					ChildAnimatorStateMachine csm = stateMachine.ChildStateMachines[i - stateCount];
					csm.Position.CopyValues(position);
				}
				else
				{
					stateMachine.ChildStateMachinePosition.AddNew().CopyValues(position);
				}
			}
		}

		stateMachine.AnyStatePosition.SetValues(0.0f, -StateOffsetY, 0.0f);
		stateMachine.EntryPosition?.SetValues(StateOffsetX, -StateOffsetY, 0.0f);
		stateMachine.ExitPosition?.SetValues(2.0f * StateOffsetX, -StateOffsetY, 0.0f);
		stateMachine.ParentStateMachinePosition.SetValues(0.0f, -2.0f * StateOffsetY, 0.0f);
	}

	private void AssignPossibleParentFromState(int stateMachineIndex, int stateIndex) // try assign deeper hierarchy parent for stateMachine
	{
		uint possibleParentStateMachineFullPathID = StateContext.GetParentForState(stateIndex); // get parent StateMachine of State (will have FullPath)
		TryAssignPossibleParent(stateMachineIndex, possibleParentStateMachineFullPathID);
	}

	private bool TryAssignPossibleParent(int stateMachineIndex, uint possibleParentStateMachineFullPathID, bool recursive = true)
	{
		uint currentParentFullPathID = IndexedStateMachines[stateMachineIndex].ParentFullPathID; // current possible parent for Unknown stateMachine
		if (!StateContext.TryGetStateMachinePath(currentParentFullPathID, out string destinationPath)) // if current parent is not set yet (0)
		{
			if (StateContext.TryGetStateMachinePath(possibleParentStateMachineFullPathID, out string parentPath)) // always true for FullPath, getting parentPath here
			{
				if (ParentRestrictionsPassed(stateMachineIndex, possibleParentStateMachineFullPathID, parentPath)) // if passes parent restrictions
				{
					IndexedStateMachines[stateMachineIndex].ParentFullPathID = possibleParentStateMachineFullPathID; // possible parent assigned
					return true;
				}
				if (!recursive)
					return false;
				// if offered parent wasn't valid, try with its parent. Recursive
				int possibleParentIndex = GetStateMachineIndexForId(possibleParentStateMachineFullPathID);
				if (possibleParentIndex != 0) // Root StateMachine doesn't have parent
				{
					return TryAssignPossibleParent(stateMachineIndex, IndexedStateMachines[possibleParentIndex].ParentFullPathID);
				}
			}
		}
		else if (currentParentFullPathID != possibleParentStateMachineFullPathID && // else if possible parent is different that current
			StateContext.TryGetStateMachinePath(possibleParentStateMachineFullPathID, out string parentPath) && // always true for FullPath, getting parentPath here
			IsDeeperHierarchy(destinationPath, parentPath)) // if new possible parent has deeper hierarchy
		{
			if (ParentRestrictionsPassed(stateMachineIndex, possibleParentStateMachineFullPathID, parentPath)) // if passes parent restrictions
			{
				IndexedStateMachines[stateMachineIndex].ParentFullPathID = possibleParentStateMachineFullPathID; // possible parent assigned
				return true;
			}
			if (!recursive)
				return false;
			// if offered parent wasn't valid, try with its parent. Recursive
			int possibleParentIndex = GetStateMachineIndexForId(possibleParentStateMachineFullPathID);
			if (possibleParentIndex != 0) // Root StateMachine doesn't have parent
			{
				return TryAssignPossibleParent(stateMachineIndex, IndexedStateMachines[possibleParentIndex].ParentFullPathID);
			}
		}
		return false;
	}

	private void LocateUnknownStateMachinesWithExitStateTransition()
	{
		List<int> TryAssignNames = new();
		for (int stateMachineIndex = 0; stateMachineIndex < IndexedStateMachines.Length; stateMachineIndex++)
		{
			StateMachineData stateMachine = IndexedStateMachines[stateMachineIndex];
			if (!stateMachine.Name.IsEmpty) // check Unknown FullPaths only
				continue;
			SelectorTransitionConstant[]? exitTransitions = stateMachine.ExitTransitions;
			if (exitTransitions == null)
				continue;
			foreach (SelectorTransitionConstant selectorTransition in exitTransitions)
			{
				if (!TryGetDestinationState(selectorTransition.Destination, out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination)
					|| stateDestination != null // check Transitions with StateMachine destination only
					|| isEntryDestination) // destination has to be the StateMachine's Exit State (not Entry destination)
					continue;

				// set definitive parent for stateMachineIndex
				uint parentFullPathID = IndexedStateMachines[stateMachineDestinationIndex].FullPathID;
				stateMachine.ParentFullPathID = parentFullPathID;
				// change Name to signal it has a definitive parent, but final Name/Path is still to be determined
				stateMachine.Name = StateMachineFlagName;
				TryAssignNames.Add(stateMachineIndex);
				break;
			}
		}

		GenerateFullPathsForUnknownStateMachines(TryAssignNames);

		// Now there can exist some Flagged StateMachines: Unknown-FullPath StateMachines with their true parents already found,
		//   but those parents are still Unknown-FullPath StateMachines themselves.
		// This found "sub hierarchies" are still not fully "anchored" to the complete StateMachine hierarchy.
		// Avoid assigning Flagged StateMachines as parents, only use FullPath StateMachines for that.
	}

	/// <summary>
	/// Generate Names and FullPaths for Unknown StateMachines, that have FullPath parent
	/// </summary>
	private void GenerateFullPathsForUnknownStateMachines(List<int> FlaggedOrUnknownStateMachineIndexes)
	{
		int listCount = FlaggedOrUnknownStateMachineIndexes.Count;
		bool shouldTryAssignNames = listCount > 0;
		while (shouldTryAssignNames)
		{
			shouldTryAssignNames = false; // infinite loop proof. only true if List depletes, and List doesn't get new items here
			for (int i = listCount - 1; i >= 0; i--)
			{
				int stateMachineIndex = FlaggedOrUnknownStateMachineIndexes[i]; // StateMachine to get a FullPath
				StateMachineData stateMachineData = IndexedStateMachines[stateMachineIndex];
				if (StateContext.TryGetStateMachinePath(stateMachineData.ParentFullPathID, out string parentFullPath)) // if its parent has FullPath...
				{
					// ...generate its own FullPath and Name
					uint stateMachineFullPathID = stateMachineData.FullPathID;
					string newFullPath = GetReversedFullPath(parentFullPath, stateMachineFullPathID);
					int pathDelimiterPos = newFullPath.LastIndexOf('.');
					string newName = newFullPath[(pathDelimiterPos + 1)..]; // Name will be "EMPTY_" + something
					stateMachineData.Name = newName;
					StateContext.AddStateMachineFullPath(newFullPath, stateMachineFullPathID); // this "promotes" the Unknown StateMachine to be a FullPath StateMachine
					FlaggedOrUnknownStateMachineIndexes.RemoveAt(i);
					UnknownFullPaths--;
					shouldTryAssignNames = true; // if the stateMachine was promoted, it could now be a valid parent for another StateMachine in the list; scan again
				}
			}
			listCount = FlaggedOrUnknownStateMachineIndexes.Count;
		}
	}

	private void RestrictUnknownStateMachinePossibleParents()
	{
		for (int stateMachineSourceIndex = 0; stateMachineSourceIndex < IndexedStateMachines.Length; stateMachineSourceIndex++)
		{
			StateMachineData sourceStateMachineData = IndexedStateMachines[stateMachineSourceIndex];
			// only Exit Transitions can restrict StateMachine parenting, other Transitions are fully unrestricted
			SelectorTransitionConstant[]? exitTransitions = sourceStateMachineData.ExitTransitions;
			if (exitTransitions == null)
				continue;
			foreach (SelectorTransitionConstant selectorTransition in exitTransitions)
			{
				if (!TryGetDestinationState(selectorTransition.Destination, out IAnimatorState? stateDestination, out int stateMachineDestinationIndex, out bool isEntryDestination)
					|| stateDestination != null // check Transitions with StateMachine destination only
					|| !isEntryDestination) // destination has to be a StateMachine's Entry port
					continue;
				StateMachineData stateMachineDestination = IndexedStateMachines[stateMachineDestinationIndex];

				// Rule 1) destination StateMachine cannot be in/under source StateMachine
				if (StateContext.TryGetStateMachinePath(sourceStateMachineData.FullPathID, out string stateMachineSourceFullPath)) // if source has FullPath...
				{
					// ...it can restrict destination StateMachine
					if (!stateMachineDestination.Name.IsEmpty)
					{
						if (stateMachineDestination.Name != StateMachineFlagName)
							continue; // don't try to restrict a FullPath StateMachine!
						
						// Flagged StateMachine has a set parent already, but that parent is Unknown-FullPath, so transfer the restrictions to parent
						stateMachineDestinationIndex = GetUnknownParentForFlaggedStateMachine(stateMachineDestinationIndex);
					}
					if (!UnknownStateMachineRestrictions.TryGetValue(stateMachineDestinationIndex, out ParentingRestrictions pr))
					{
						pr = new();
						UnknownStateMachineRestrictions[stateMachineDestinationIndex] = pr;
					}
					if (pr.NotUnder.All(fullPath => !stateMachineSourceFullPath.StartsWith(fullPath)))
						pr.NotUnder.Add(stateMachineSourceFullPath);
				}
				// Rule 2) destination StateMachine cannot be parent of source StateMachine
				else if (!stateMachineDestination.Name.IsEmpty && stateMachineDestination.Name != StateMachineFlagName) // source is Unknown or Flagged; if destination has FullPath...
				{
					// ...it can restrict source StateMachine
					if (sourceStateMachineData.Name == StateMachineFlagName)
					{
						// Flagged StateMachine has a set parent already, this restriction isn't needed
						continue;
					}
					if (!UnknownStateMachineRestrictions.TryGetValue(stateMachineSourceIndex, out ParentingRestrictions pr))
					{
						pr = new();
						UnknownStateMachineRestrictions[stateMachineSourceIndex] = pr;
					}
					if (!pr.NoDirect.Contains(stateMachineDestination.FullPathID))
						pr.NoDirect.Add(stateMachineDestination.FullPathID);
				}
			}
		}

		// simplify restrictions
		foreach (ParentingRestrictions pr in UnknownStateMachineRestrictions.Values)
		{
			if (pr.NotUnder.Count > 1) // remove StateMachine FullPaths from NotUnder when contained in another NotUnder FullPath
			{
				string[] temp = pr.NotUnder.Where(fullPath => pr.NotUnder.All(otherFullPath => fullPath == otherFullPath || !fullPath.StartsWith(otherFullPath))).ToArray();
				pr.NotUnder.Clear();
				pr.NotUnder.AddRange(temp); 
			}

			for (int i = pr.NoDirect.Count-1; i >= 0; i--) // remove StateMachine FullPaths from NoDirect when contained in NotUnder 
			{
				if (StateContext.TryGetStateMachinePath(pr.NoDirect[i], out string fullPath)) // always true, getting fullPath here
				{
					if (pr.NotUnder.Any(_fullPath => fullPath.StartsWith(_fullPath)))
						pr.NoDirect.RemoveAt(i);
				}
			}
		}
	}

	private bool ParentRestrictionsPassed(int stateMachineIndex, uint parentFullPathID, string parentFullPath)
	{
		if (!UnknownStateMachineRestrictions.TryGetValue(stateMachineIndex, out ParentingRestrictions pr))
			return true;
		if (pr.NoDirect.Contains(parentFullPathID) ||
			pr.NotUnder.Any(badFullPath => parentFullPath.StartsWith(badFullPath)))
			return false;
		return true;
	}

	private static string GetReversedFullPath(string parentFullPath, uint fullPathID)
	{
		return Crc32Algorithm.ReverseAscii(fullPathID, $"{parentFullPath}.EMPTY_");
	}

	/// <summary>
	/// Stores parenting/hierarchy restrictions for Unknown StateMachines.
	/// </summary>
	/// <param name="NoDirect">FullPathIDs of not allowed parents.</param>
	/// <param name="NotUnder">FullPaths of not allowed parents to be under.</param>
	private readonly record struct ParentingRestrictions()
	{
		public readonly List<string> NotUnder = new();
		public readonly List<uint> NoDirect = new();
	}

	private record class StateMachineData(IAnimatorStateMachine StateMachine, uint FullPathID = 0)
	{
		public uint ParentFullPathID;
		public SelectorTransitionConstant[]? EntryTransitions;
		public SelectorTransitionConstant[]? ExitTransitions;

		public Utf8String Name { get { return StateMachine.Name; } set { StateMachine.Name = value; } }
	}
}
