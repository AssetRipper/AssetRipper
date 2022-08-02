using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorControllerLayerExtensions
	{
		public static void Initialize(this IAnimatorControllerLayer animatorControllerLayer, IAnimatorStateMachine stateMachine, IAnimatorController controller, int layerIndex)
		{
			ILayerConstant layer = controller.Controller_C91.LayerArray[layerIndex].Data;

			stateMachine.ParentStateMachinePosition_C1107.SetValues(800.0f, 20.0f, 0.0f);//not sure why this happens here

			animatorControllerLayer.Name.CopyValues(controller.TOS_C91[layer.Binding]);


			if (animatorControllerLayer.Has_StateMachine_PPtr_AnimatorStateMachine_())
			{
				animatorControllerLayer.StateMachine_PPtr_AnimatorStateMachine_.CopyValues(stateMachine.SerializedFile.CreatePPtr(stateMachine));
			}
			else if (animatorControllerLayer.Has_StateMachine_PPtr_StateMachine_())
			{
				animatorControllerLayer.StateMachine_PPtr_StateMachine_.CopyValues(stateMachine.SerializedFile.CreatePPtr(stateMachine));
			}

#warning TODO: animator
			//Mask = new();

			animatorControllerLayer.BlendingMode = layer.LayerBlendingMode;
			animatorControllerLayer.SyncedLayerIndex = layer.StateMachineSynchronizedLayerIndex == 0 ? -1 : (int)layer.StateMachineIndex;
			animatorControllerLayer.DefaultWeight = layer.DefaultWeight;
			animatorControllerLayer.IKPass = layer.IKPass;
			animatorControllerLayer.SyncedLayerAffectsTiming = layer.SyncedLayerAffectsTiming;
			animatorControllerLayer.Controller?.CopyValues(controller.SerializedFile.CreatePPtr(controller));
		}

		public static AnimatorLayerBlendingMode GetBlendingMode(this IAnimatorControllerLayer animatorControllerLayer)
		{
			return (AnimatorLayerBlendingMode)animatorControllerLayer.BlendingMode;
		}
	}
}
