using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AnimatorControllerLayerExtensions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="animatorControllerLayer">Must be a child of <paramref name="controller"/></param>
		/// <param name="stateMachine"></param>
		/// <param name="controller"></param>
		/// <param name="layerIndex"></param>
		public static void Initialize(this IAnimatorControllerLayer animatorControllerLayer, IAnimatorStateMachine stateMachine, IAnimatorController controller, int layerIndex)
		{
			ILayerConstant layer = controller.Controller_C91.LayerArray[layerIndex].Data;

			stateMachine.ParentStateMachinePosition_C1107.SetValues(800.0f, 20.0f, 0.0f);//not sure why this happens here

			animatorControllerLayer.Name.CopyValues(controller.TOS_C91[layer.Binding]);


			if (animatorControllerLayer.Has_StateMachine_PPtr_AnimatorStateMachine())
			{
				animatorControllerLayer.StateMachine_PPtr_AnimatorStateMachine.CopyValues(controller.Collection.ForceCreatePPtr(stateMachine));
			}
			else if (animatorControllerLayer.Has_StateMachine_PPtr_StateMachine())
			{
				animatorControllerLayer.StateMachine_PPtr_StateMachine.CopyValues(controller.Collection.ForceCreatePPtr(stateMachine));
			}

#warning TODO: animator
			//Mask = new();

			animatorControllerLayer.BlendingMode = layer.LayerBlendingMode;
			animatorControllerLayer.SyncedLayerIndex = layer.StateMachineSynchronizedLayerIndex == 0 ? -1 : (int)layer.StateMachineIndex;
			animatorControllerLayer.DefaultWeight = layer.DefaultWeight;
			animatorControllerLayer.IKPass = layer.IKPass;
			animatorControllerLayer.SyncedLayerAffectsTiming = layer.SyncedLayerAffectsTiming;
			animatorControllerLayer.Controller?.CopyValues(controller.Collection.CreatePPtr(controller));
		}
	}
}
