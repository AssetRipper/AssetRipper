using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.NativeEnums.Animation;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerParameter;
using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;

namespace AssetRipper.Processing.AnimatorControllers;

public sealed class AnimatorControllerProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Reconstruct AnimatorController Assets");
		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Generated AnimatorController Dependencies");
		foreach (AssetCollection collection in gameData.GameBundle.FetchAssetCollections().Where(c => c.Flags.IsRelease()))
		{
			foreach (IAnimatorController asset in collection.OfType<IAnimatorController>())
			{
				Process(asset, processedCollection);
			}
		}
	}

	private static void Process(IAnimatorController controller, ProcessedAssetCollection processedCollection)
	{
		IControllerConstant controllerConstant = controller.Controller;
		IAnimatorStateMachine[] StateMachines = new IAnimatorStateMachine[controllerConstant.StateMachineArray.Count];
		for (int i = 0; i < controllerConstant.StateMachineArray.Count; i++)
		{
			IAnimatorStateMachine stateMachine = VirtualAnimationFactory.CreateRootAnimatorStateMachine(processedCollection, controller, i);
			StateMachines[i] = stateMachine;
		}

		controller.AnimatorParameters.Clear();
		controller.AnimatorParameters.Capacity = controllerConstant.Values.Data.ValueArray.Count;
		for (int i = 0; i < controllerConstant.Values.Data.ValueArray.Count; i++)
		{
			IAnimatorControllerParameter newParameter = controller.AnimatorParameters.AddNew();
			InitializeParameter(newParameter, controller, i);
		}

		controller.AnimatorLayers.Clear();
		controller.AnimatorLayers.Capacity = controllerConstant.LayerArray.Count;
		for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
		{
			uint stateMachineIndex = controllerConstant.LayerArray[i].Data.StateMachineIndex;
			IAnimatorStateMachine stateMachine = StateMachines[stateMachineIndex];
			IAnimatorControllerLayer newLayer = controller.AnimatorLayers.AddNew();
			InitializeLayer(newLayer, stateMachine, controller, i);
		}

		foreach (IUnityObjectBase dependency in controller.FetchEditorHierarchy().WhereNotNull())
		{
			dependency.MainAsset = controller;
		}
	}

	private static void InitializeLayer(IAnimatorControllerLayer animatorControllerLayer, IAnimatorStateMachine stateMachine, IAnimatorController controller, int layerIndex)
	{
		ILayerConstant layer = controller.Controller.LayerArray[layerIndex].Data;

		animatorControllerLayer.Name = controller.TOS[layer.Binding];

		animatorControllerLayer.StateMachine.SetAsset(controller.Collection, stateMachine);

#warning TODO: animator
		//Mask = new();

		animatorControllerLayer.BlendingMode = layer.LayerBlendingMode;
		animatorControllerLayer.SyncedLayerIndex = layer.StateMachineSynchronizedLayerIndex == 0 ? -1 : (int)layer.StateMachineIndex;
		animatorControllerLayer.DefaultWeight = layer.DefaultWeight;
		animatorControllerLayer.IKPass = layer.IKPass;
		animatorControllerLayer.SyncedLayerAffectsTiming = layer.SyncedLayerAffectsTiming;
		if (animatorControllerLayer.Has_Controller())
		{
			animatorControllerLayer.Controller.SetAsset(controller.Collection, controller);
		}
	}

	private static void InitializeParameter(IAnimatorControllerParameter parameter, IAnimatorController controller, int paramIndex)
	{
		IValueConstant value = controller.Controller.Values.Data.ValueArray[paramIndex];
		parameter.Name = controller.TOS[value.ID];
		AnimatorControllerParameterType type = value.GetTypeValue();
		switch (type)
		{
			case AnimatorControllerParameterType.Trigger:
				parameter.DefaultBool = controller.Controller.DefaultValues.Data.BoolValues[(int)value.Index];
				break;

			case AnimatorControllerParameterType.Bool:
				parameter.DefaultBool = controller.Controller.DefaultValues.Data.BoolValues[(int)value.Index];
				break;

			case AnimatorControllerParameterType.Int:
				parameter.DefaultInt = controller.Controller.DefaultValues.Data.IntValues[(int)value.Index];
				break;

			case AnimatorControllerParameterType.Float:
				parameter.DefaultFloat = controller.Controller.DefaultValues.Data.FloatValues[(int)value.Index];
				break;

			default:
				throw new NotSupportedException($"Parameter type '{type}' isn't supported");
		}
		parameter.Type = (int)type;
		if (parameter.Has_Controller())
		{
			parameter.Controller.SetAsset(controller.Collection, controller);
		}
	}
}
