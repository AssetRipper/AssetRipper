using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.OffsetPtr_StateMachineConstant;

namespace AssetRipper.Processing.AnimatorControllers
{
	public sealed class AnimatorControllerProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Reconstruct AnimatorController Assets");
			ProcessedAssetCollection processedCollection = gameBundle.AddNewProcessedCollection(
				"Generated AnimatorController Dependencies",
				projectVersion);
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections().Where(c => c.Flags.IsRelease()))
			{
				foreach (IAnimatorController asset in collection.OfType<IAnimatorController>())
				{
					Process(asset, processedCollection);
				}
			}
		}

		private static void Process(IAnimatorController controller, ProcessedAssetCollection processedCollection)
		{
			IControllerConstant controllerConstant = controller.Controller_C91;
			AccessListBase<IOffsetPtr_StateMachineConstant> stateMachinesConst = controllerConstant.StateMachineArray;
			IAnimatorStateMachine[] StateMachines = new IAnimatorStateMachine[stateMachinesConst.Count];
			for (int i = 0; i < stateMachinesConst.Count; i++)
			{
				IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateAnimatorStateMachine(processedCollection, controller, i);
				StateMachines[i] = stateMachine;
			}

			controller.AnimatorParameters_C91.Clear();
			controller.AnimatorParameters_C91.Capacity = controllerConstant.Values.Data.ValueArray.Count;
			for (int i = 0; i < controllerConstant.Values.Data.ValueArray.Count; i++)
			{
				controller.AnimatorParameters_C91.AddNew().Initialize(controller, i);
			}

			controller.AnimatorLayers_C91.Clear();
			controller.AnimatorLayers_C91.Capacity = controllerConstant.LayerArray.Count;
			for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
			{
				uint stateMachineIndex = controllerConstant.LayerArray[i].Data.StateMachineIndex;
				IAnimatorStateMachine stateMachine = StateMachines[stateMachineIndex];
				controller.AnimatorLayers_C91.AddNew().Initialize(stateMachine, controller, i);
			}

			foreach (IUnityObjectBase? dependency in controller.FetchEditorHierarchy())
			{
				if (dependency is not null)
				{
					dependency.MainAsset = controller;
				}
			}
		}
	}
}
