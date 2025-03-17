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
	public bool HasStates() => IndexedStates.Length > 0;

	private readonly ProcessedAssetCollection VirtualFile;
	private readonly IAnimatorController Controller;
	private readonly IStateMachineConstant StateMachineConstant;
	private readonly int LayerIndex;
	/// <summary>
	/// Stores all AnimatorState data, also linking each AnimatorState with its index from StateConstantArray
	/// </summary>
	private readonly StateData[] IndexedStates;
	private readonly BidirectionalDictionary<string, uint> stateMachinePathNamesAndIDs;

	public AnimatorStateContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
	{
		VirtualFile = virtualFile;
		Controller = controller;
		StateMachineConstant = stateMachineConstant;
		LayerIndex = layerIndex;

		DefaultStateIndex = stateMachineConstant.DefaultState != uint.MaxValue ? (int)stateMachineConstant.DefaultState : 0;

		IndexedStates = new StateData[stateMachineConstant.StateConstantArray.Count];
		stateMachinePathNamesAndIDs = new();
	}

	/// <summary>
	/// Create all AnimatorStates, and group them by StateMachine paths
	/// </summary>
	public void Process()
	{
		if (!HasStates()) { return; }

		Controller.TOS.TryAdd(0, Utf8String.Empty);

		for (int i = 0; i < IndexedStates.Length; i++)
		{
			IStateConstant stateConstant = StateMachineConstant.StateConstantArray[i].Data;
			IAnimatorState state = VirtualAnimationFactory.CreateAnimatorState(VirtualFile, Controller, Controller.TOS, LayerIndex, stateConstant);

			string stateMachinePath = MakeStateMachinePath(Controller.TOS, stateConstant.GetId(), state.Name.String); // [stateMachinePath].StateName
			if (!stateMachinePathNamesAndIDs.TryGetValue(stateMachinePath, out uint stateMachinePathID))
			{
				stateMachinePathID = Checksum.Crc32Algorithm.HashUTF8(stateMachinePath);
				stateMachinePathNamesAndIDs.Add(stateMachinePath, stateMachinePathID);
			}

			IndexedStates[i] = new(state, stateConstant, stateMachinePathID);
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
		return IndexedStates[index].StateConstant;
	}

	public IAnimatorState GetState(int index)
	{
		return IndexedStates[index].State;
	}

	public int GetStateIndex(IAnimatorState? state)
	{
		if (state == null)
			return -1;

		return IndexedStates.IndexOf(s => s.State == state);
	}

	public string GetStateMachinePath(int stateIndex)
	{
		uint stateMachinePathID = IndexedStates[stateIndex].ParentStateMachineID;
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
		for (int i = 0; i < IndexedStates.Length; i++)
		{
			if (IndexedStates[i].ParentStateMachineID == pathID)
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
	/// Stores AnimatorState related data together.
	/// </summary>
	private readonly record struct StateData(IAnimatorState State, IStateConstant StateConstant, uint ParentStateMachineID);
}
