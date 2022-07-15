using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.PPtr_AnimationClip_;
using AssetRipper.SourceGenerated.Subclasses.PPtr_MonoBehaviour_;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;
using AssetRipper.SourceGenerated.Subclasses.StateKey;
using AssetRipper.SourceGenerated.Subclasses.StateMachineConstant;
using AssetRipper.SourceGenerated.Subclasses.StateRange;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorControllerExtensions
	{
		public static bool IsContainsAnimationClip(this IAnimatorController controller, IAnimationClip clip)
		{
			foreach (IPPtr_AnimationClip_ clipPtr in controller.AnimationClips_C91)
			{
				if (clipPtr.IsAsset(controller.SerializedFile, clip))
				{
					return true;
				}
			}
			return false;
		}

		public static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(this IAnimatorController controller, int layerIndex)
		{
			if (controller.Has_StateMachineBehaviourVectorDescription_C91())
			{
				uint layerID = controller.Controller_C91.LayerArray[layerIndex].Data.Binding;
				StateKey key = new();
				key.SetValues(layerIndex, layerID);
				if (controller.StateMachineBehaviourVectorDescription_C91.StateMachineBehaviourRanges.TryGetValue(key, out StateRange? range))
				{
					return controller.GetStateBehaviours(range);
				}
			}
			return Array.Empty<PPtr_MonoBehaviour__5_0_0_f4>();
		}

		public static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(this IAnimatorController controller, int stateMachineIndex, int stateIndex)
		{
			if (controller.Has_StateMachineBehaviourVectorDescription_C91())
			{
				int layerIndex = controller.Controller_C91.GetLayerIndexByStateMachineIndex(stateMachineIndex);
				IStateMachineConstant stateMachine = controller.Controller_C91.StateMachineArray[stateMachineIndex].Data;
				IStateConstant state = stateMachine.StateConstantArray[stateIndex].Data;
				uint stateID = GetIdForStateConstant(state);
				StateKey key = new();
				key.SetValues(layerIndex, stateID);
				if (controller.StateMachineBehaviourVectorDescription_C91.StateMachineBehaviourRanges.TryGetValue(key, out StateRange? range))
				{
					return controller.GetStateBehaviours(range);
				}
			}
			return Array.Empty<PPtr_MonoBehaviour__5_0_0_f4>();
		}

		private static PPtr_MonoBehaviour__5_0_0_f4[] GetStateBehaviours(this IAnimatorController controller, StateRange range)
		{
			PPtr_MonoBehaviour__5_0_0_f4[] stateMachineBehaviours = new PPtr_MonoBehaviour__5_0_0_f4[range.Count];
			for (int i = 0; i < range.Count; i++)
			{
				int index = (int)controller.StateMachineBehaviourVectorDescription_C91.StateMachineBehaviourIndices[range.StartIndex + i];
				stateMachineBehaviours[i] = controller.StateMachineBehaviours_C91[index];
			}
			return stateMachineBehaviours;
		}

		private static uint GetIdForStateConstant(IStateConstant stateConstant)
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
}
