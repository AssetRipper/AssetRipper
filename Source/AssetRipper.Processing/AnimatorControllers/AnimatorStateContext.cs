using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1102;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_206;
using AssetRipper.SourceGenerated.Classes.ClassID_207;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeNodeConstant;
using AssetRipper.SourceGenerated.Subclasses.ChildMotion;
using AssetRipper.SourceGenerated.Subclasses.LeafInfoConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;

namespace AssetRipper.Processing.AnimatorControllers
{
	public sealed class AnimatorStateContext
	{
		// Example of default BlendTree Name:
		// https://github.com/ds5678/Binoculars/blob/d6702ed3a1db39b1a2788956ff195b2590c3d08b/Unity/Assets/Models/binoculars_animator.controller#L106
		private static Utf8String BlendTreeName { get; } = new Utf8String("Blend Tree");

		public readonly int StateCount;
		public bool HasStates() => StateCount > 0;

		public readonly int DefaultStateIdx;

		readonly IAnimatorState[] states;
		readonly IStateConstant[] stateConstants;
		readonly uint[] stateIdxsToStateMachinePathIDs;

		readonly BidirectionalDictionary<string,uint> stateMachinePathNamesAndIDs; // Bidirectional Dictionary and grouping States for StateMachines

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

			IMotion? motion = CreateMotion(virtualFile, controller, state, 0);
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

		public static IMotion? CreateMotion(ProcessedAssetCollection file, IAnimatorController controller, IStateConstant stateConstant, int nodeIndex)
		{
			if (stateConstant.BlendTreeConstantArray.Count == 0)
			{
				return default; // null Motion
			}

			IBlendTreeNodeConstant node = stateConstant.GetBlendTree().NodeArray[nodeIndex].Data;
			if (node.IsBlendTree())
			{
				return CreateBlendTree(file, controller, stateConstant, nodeIndex); // BlendTree Motion
			}

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
				return controller.AnimationClipsP[clipIndex] as IMotion; // AnimationClip Motion
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

		private static IChildMotion AddAndInitializeNewChild(ProcessedAssetCollection file, IAnimatorController controller, IStateConstant state, IBlendTree tree, int nodeIndex, int childIndex)
		{
			IChildMotion childMotion = tree.Childs.AddNew();
			IBlendTreeConstant treeConstant = state.GetBlendTree();
			IBlendTreeNodeConstant node = treeConstant.NodeArray[nodeIndex].Data;
			int childNodeIndex = (int)node.ChildIndices[childIndex];
			IMotion? motion = CreateMotion(file, controller, state, childNodeIndex);
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

		private static string MakeStateMachinePath(AssetDictionary<uint, Utf8String> TOS, uint statePathID, string stateName)
		{
			string fullPath = TOS[statePathID];
			string stateMachinePath = fullPath[..(fullPath.Length - stateName.Length - 1)];
			return stateMachinePath;
		}

		/// <summary>
		/// Create all AnimatorStates, and find names for StateMachines
		/// </summary>
		public AnimatorStateContext(ProcessedAssetCollection virtualFile, IAnimatorController controller, IStateMachineConstant stateMachineConstant, int layerIndex)
		{
			if (!controller.TOS.ContainsKey(0))
			{
				controller.TOS[0] = Utf8String.Empty;
			}

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

			for (int i = 0; i < StateCount; i++)
			{
				IStateConstant stateConstant = stateMachineConstant.StateConstantArray[i].Data;
				IAnimatorState state = CreateAnimatorState(virtualFile, controller, controller.TOS, layerIndex, stateConstant);

				string stateMachinePath = MakeStateMachinePath(controller.TOS, stateConstant.GetId(), state.Name.String); // [stateMachinePath].StateName
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
			if (stateMachineConstant.StateMachineCount() > uniqueSMPathsCount) // can only happen on Unity 5+
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
	}
}
