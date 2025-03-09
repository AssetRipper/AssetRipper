using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;

namespace AssetRipper.Processing.AnimatorControllers
{
	public sealed class AnimatorStateContext
	{
		public readonly int StateCount;
		public bool HasStates() => StateCount > 0;
		public readonly int DefaultStateIdx;

		private readonly ProcessedAssetCollection VirtualFile;
		private readonly IAnimatorController Controller;
		private readonly IStateMachineConstant StateMachineConstant;
		private readonly int LayerIndex;
		
		private readonly IAnimatorState[] states;
		private readonly IStateConstant[] stateConstants;
		private readonly uint[] stateIdxsToStateMachinePathIDs; // for grouping States into StateMachines
		private readonly BidirectionalDictionary<string,uint> stateMachinePathNamesAndIDs;

		public AnimatorStateContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
		{
			VirtualFile = virtualFile;
			Controller = controller;
			StateMachineConstant = stateMachineConstant;
			LayerIndex = layerIndex;

			DefaultStateIdx = stateMachineConstant.DefaultState != uint.MaxValue ? (int)stateMachineConstant.DefaultState : 0;
			StateCount = stateMachineConstant.StateConstantArray.Count;

			stateMachinePathNamesAndIDs = new();
			if (!HasStates())
			{
				stateConstants = [];
				states = [];
				stateIdxsToStateMachinePathIDs = [];
				return;
			}
			stateConstants = new IStateConstant[StateCount];
			states = new IAnimatorState[StateCount];
			stateIdxsToStateMachinePathIDs = new uint[StateCount];
		}

		/// <summary>
		/// Create all AnimatorStates, and group them by StateMachine paths
		/// </summary>
		public void Process()
		{
			if (!HasStates()) { return; }

			if (!Controller.TOS.ContainsKey(0))
			{
				Controller.TOS[0] = Utf8String.Empty;
			}

			for (int i = 0; i < StateCount; i++)
			{
				IStateConstant stateConstant = StateMachineConstant.StateConstantArray[i].Data;
				IAnimatorState state = VirtualAnimationFactory.CreateAnimatorState(VirtualFile, Controller, Controller.TOS, LayerIndex, stateConstant);

				string stateMachinePath = MakeStateMachinePath(Controller.TOS, stateConstant.GetId(), state.Name.String); // [stateMachinePath].StateName
				if (!stateMachinePathNamesAndIDs.TryGetValue(stateMachinePath, out uint stateMachinePathID))
				{
					stateMachinePathID = Checksum.Crc32Algorithm.HashUTF8(stateMachinePath);
					stateMachinePathNamesAndIDs.Add(stateMachinePath, stateMachinePathID);
				}

				stateConstants[i] = stateConstant;
				states[i] = state;
				stateIdxsToStateMachinePathIDs[i] = stateMachinePathID; // this is later used to group AnimatorStates by common StateMachine path
			}

			int uniqueSMPathsCount = stateMachinePathNamesAndIDs.Count;
			if (StateMachineConstant.StateMachineCount() > uniqueSMPathsCount) // can only happen on Unity 5+
			{
				// there are StateMachines that don't contain States
				// generate more possible StateMachine paths to locate this StateMachines
				// *not useful when these StateMachines come last in hierachy (don't have child StateMachines with States)
				string[] originalSMPathNames = stateMachinePathNamesAndIDs.Keys.ToArray();
				foreach (string originalSMPathName in originalSMPathNames)
				{
					string stateMachinePath = originalSMPathName;
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
			return stateConstants[index];
		}

		public IAnimatorState GetState(int index)
		{
			return states[index];
		}

		public int GetStateIdx(IAnimatorState? state)
		{
			if (state == null)
			{
				return -1;
			}
			int stateIdx = states.IndexOf(state);
			return stateIdx;
		}

		public string GetStateMachinePath(int stateIndex)
		{
			uint stateMachinePathID = stateIdxsToStateMachinePathIDs[stateIndex];
			return stateMachinePathNamesAndIDs[stateMachinePathID];
		}

		public bool TryGetStateMachinePath(uint pathID, out string path)
		{
			if (pathID != 0 && stateMachinePathNamesAndIDs.TryGetValue(pathID, out string pathName))
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

		public IReadOnlyList<string> GetUniqueSMPaths()
		{
			return stateMachinePathNamesAndIDs.Keys.ToArray();
		}

		public IEnumerable<int> StateIdxsForStateMachine(uint pathID) // yield AnimatorStates from provided StateMachine path
		{
			for (int i = 0; i < stateIdxsToStateMachinePathIDs.Length; i++)
			{
				if (stateIdxsToStateMachinePathIDs[i] == pathID)
				{
					yield return i;
				}
			}
		}

		public IEnumerable<int> StateIdxsForStateMachine(string path) // yield AnimatorStates from provided StateMachine path
		{
			if (TryGetStateMachinePathID(path, out uint pathID))
			{
				return StateIdxsForStateMachine(pathID);
			}
			return Array.Empty<int>();
		}

		private static string MakeStateMachinePath(AssetDictionary<uint, Utf8String> TOS, uint statePathID, string stateName)
		{
			string fullPath = TOS[statePathID];
			string stateMachinePath = fullPath[..(fullPath.Length - stateName.Length - 1)];
			return stateMachinePath;
		}
	}
}
