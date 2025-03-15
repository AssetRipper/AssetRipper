using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;

namespace AssetRipper.Processing.AnimatorControllers;

internal sealed class AnimatorStateContext
{
	public readonly int DefaultStateIndex;
	public bool HasStates() => indexedStates.StateCount > 0;

	private readonly ProcessedAssetCollection VirtualFile;
	private readonly IAnimatorController Controller;
	private readonly IStateMachineConstant StateMachineConstant;
	private readonly int LayerIndex;

	private readonly IndexedStates indexedStates;
	private readonly BidirectionalDictionary<string, uint> stateMachinePathNamesAndIDs;

	public AnimatorStateContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
	{
		VirtualFile = virtualFile;
		Controller = controller;
		StateMachineConstant = stateMachineConstant;
		LayerIndex = layerIndex;

		DefaultStateIndex = stateMachineConstant.DefaultState != uint.MaxValue ? (int)stateMachineConstant.DefaultState : 0;

		indexedStates = new(stateMachineConstant.StateConstantArray.Count);
		stateMachinePathNamesAndIDs = new();
	}

	/// <summary>
	/// Create all AnimatorStates, and group them by StateMachine paths
	/// </summary>
	public void Process()
	{
		if (!HasStates()) { return; }

		Controller.TOS.TryAdd(0, Utf8String.Empty);

		for (int i = 0; i < indexedStates.StateCount; i++)
		{
			IStateConstant stateConstant = StateMachineConstant.StateConstantArray[i].Data;
			IAnimatorState state = VirtualAnimationFactory.CreateAnimatorState(VirtualFile, Controller, Controller.TOS, LayerIndex, stateConstant);

			string stateMachinePath = MakeStateMachinePath(Controller.TOS, stateConstant.GetId(), state.Name.String); // [stateMachinePath].StateName
			if (!stateMachinePathNamesAndIDs.TryGetValue(stateMachinePath, out uint stateMachinePathID))
			{
				stateMachinePathID = Checksum.Crc32Algorithm.HashUTF8(stateMachinePath);
				stateMachinePathNamesAndIDs.Add(stateMachinePath, stateMachinePathID);
			}

			indexedStates.SetAtIndex(i, state, stateConstant, stateMachinePathID);
		}

		if (StateMachineConstant.StateMachineCount() > stateMachinePathNamesAndIDs.Count) // can only happen on Unity 5+
		{
			// there are StateMachines that don't contain States
			// generate more possible StateMachine paths to locate this StateMachines
			// *not useful when these StateMachines come last in hierachy (don't have child StateMachines with States)
			string[] originalStateMachinePathNames = stateMachinePathNamesAndIDs.Keys.ToArray();
			foreach (string originalStateMachinePathName in originalStateMachinePathNames)
			{
				string stateMachinePath = originalStateMachinePathName;
				int pathDelimiterPos = stateMachinePath.LastIndexOf('.');
				// loop and trim StateMachine names from end of path
				while (pathDelimiterPos != -1)
				{
					stateMachinePath = stateMachinePath[..pathDelimiterPos];
					if (stateMachinePathNamesAndIDs.ContainsKey(stateMachinePath))
					{
						break;
					}
					else
					{
						uint stateMachinePathID = Checksum.Crc32Algorithm.HashUTF8(stateMachinePath);
						stateMachinePathNamesAndIDs.Add(stateMachinePath, stateMachinePathID);
					}
					pathDelimiterPos = stateMachinePath.LastIndexOf('.');
				}
			}
		}
	}

	public IStateConstant GetStateConstant(int index)
	{
		return indexedStates.GetStateConstant(index);
	}

	public IAnimatorState GetState(int index)
	{
		return indexedStates.GetState(index);
	}

	public int GetStateIndex(IAnimatorState? state)
	{
		if (state == null)
			return -1;

		return indexedStates.IndexOf(state);
	}

	public string GetStateMachinePath(int stateIndex)
	{
		uint stateMachinePathID = indexedStates.GetParentStateMachineID(stateIndex);
		return stateMachinePathNamesAndIDs[stateMachinePathID];
	}

	public bool TryGetStateMachinePath(uint pathID, out string path)
	{
		if (pathID != 0 && stateMachinePathNamesAndIDs.TryGetValue(pathID, out string? pathName))
		{
			path = pathName;
			return true;
		}

		path = string.Empty;
		return false;
	}

	public bool TryGetStateMachinePathID(string path, out uint pathID)
	{
		if (!string.IsNullOrEmpty(path) && stateMachinePathNamesAndIDs.TryGetValue(path, out uint pathId))
		{
			pathID = pathId;
			return true;
		}

		pathID = 0;
		return false;
	}

	public IEnumerable<string> GetUniqueStateMachinePaths()
	{
		return stateMachinePathNamesAndIDs.Keys;
	}

	public int GetUniqueStateMachinePathsCount()
	{
		return stateMachinePathNamesAndIDs.Count;
	}

	public IEnumerable<int> StateIndicesForStateMachine(uint pathID) // yield AnimatorStates from provided StateMachine path
	{
		for (int i = 0; i < indexedStates.StateCount; i++)
		{
			if (indexedStates.GetParentStateMachineID(i) == pathID)
			{
				yield return i;
			}
		}
	}

	public IEnumerable<int> StateIndicesForStateMachine(string path) // yield AnimatorStates from provided StateMachine path
	{
		if (TryGetStateMachinePathID(path, out uint pathID))
		{
			return StateIndicesForStateMachine(pathID);
		}
		return Array.Empty<int>();
	}

	private static string MakeStateMachinePath(AssetDictionary<uint, Utf8String> TOS, uint statePathID, string stateName)
	{
		string fullPath = TOS[statePathID];
		string stateMachinePath = fullPath[..(fullPath.Length - stateName.Length - 1)];
		return stateMachinePath;
	}

	/// <summary>
	/// Stores AnimatorState related data in arrays, retrieves by index
	/// </summary>
	/// <param name="StateCount">Total number of AnimatorStates to process.</param>
	private readonly record struct IndexedStates(int StateCount)
	{
		private IAnimatorState[] States { get; } = new IAnimatorState[StateCount];
		private IStateConstant[] StateConstants { get; } = new IStateConstant[StateCount];
		/// <summary>
		/// Used for grouping States into their Parent StateMachines
		/// </summary>
		private uint[] ParentStateMachineIDs { get; } = new uint[StateCount];

		public void SetAtIndex(int index, IAnimatorState state, IStateConstant stateConstant, uint parentStateMachineID)
		{
			States[index] = state;
			StateConstants[index] = stateConstant;
			ParentStateMachineIDs[index] = parentStateMachineID;
		}

		public IAnimatorState GetState(int index)
		{
			return States[index];
		}

		public IStateConstant GetStateConstant(int index)
		{
			return StateConstants[index];
		}

		public uint GetParentStateMachineID(int index)
		{
			return ParentStateMachineIDs[index];
		}

		public int IndexOf(IAnimatorState state)
		{
			return States.IndexOf(state);
		}
	}
}
