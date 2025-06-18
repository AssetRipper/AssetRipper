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
		if (!HasStates())
		{
			return;
		}

		Controller.TOS.TryAdd(0, Utf8String.Empty);

		for (int i = 0; i < IndexedStates.Length; i++)
		{
			IStateConstant stateConstant = StateMachineConstant.StateConstantArray[i].Data;
			IAnimatorState state = VirtualAnimationFactory.CreateAnimatorState(VirtualFile, Controller, Controller.TOS, LayerIndex, stateConstant);

			string stateMachineFullPath = MakeStateMachinePath(Controller.TOS, stateConstant.GetId(), state.Name.String); // [stateMachinePath].StateName
			if (!stateMachinePathNamesAndIDs.TryGetValue(stateMachineFullPath, out uint stateMachineFullPathID))
			{
				stateMachineFullPathID = Checksum.Crc32Algorithm.HashUTF8(stateMachineFullPath);
				stateMachinePathNamesAndIDs.Add(stateMachineFullPath, stateMachineFullPathID);
			}

			IndexedStates[i] = new(state, stateConstant, stateMachineFullPathID);
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
				// periods are used for concatenating State and StateMachine Names to get their paths.
				// Unity Editor doesn't allow periods in State and StateMachine Names.
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

	public uint GetParentForState(int index)
	{
		return IndexedStates[index].ParentStateMachineID;
	}

	public string GetStateMachinePath(int stateIndex)
	{
		uint stateMachinePathID = IndexedStates[stateIndex].ParentStateMachineID;
		return stateMachinePathNamesAndIDs[stateMachinePathID];
	}

	public void AddStateMachineFullPath(string fullPath, uint fullPathID)
	{
		stateMachinePathNamesAndIDs.Add(fullPath, fullPathID);
	}

	public bool TryGetStateMachinePath(uint fullPathID, out string fullPath)
	{
		if (stateMachinePathNamesAndIDs.TryGetValue(fullPathID, out string? pathName))
		{
			fullPath = pathName;
			return true;
		}

		fullPath = string.Empty;
		return false;
	}

	public bool TryGetStateMachinePathID(string fullPath, out uint fullPathID)
	{
		if (!string.IsNullOrEmpty(fullPath) && stateMachinePathNamesAndIDs.TryGetValue(fullPath, out uint fullPathId))
		{
			fullPathID = fullPathId;
			return true;
		}

		fullPathID = 0;
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

	public IEnumerable<int> StateIndicesForStateMachine(uint fullPathID) // yield AnimatorStates from provided StateMachine FullPath
	{
		for (int i = 0; i < IndexedStates.Length; i++)
		{
			if (IndexedStates[i].ParentStateMachineID == fullPathID)
			{
				yield return i;
			}
		}
	}

	public IEnumerable<int> StateIndicesForStateMachine(string fullPath) // yield AnimatorStates from provided StateMachine FullPath
	{
		if (TryGetStateMachinePathID(fullPath, out uint fullPathID))
		{
			return StateIndicesForStateMachine(fullPathID);
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
