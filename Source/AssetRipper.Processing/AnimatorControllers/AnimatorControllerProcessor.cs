using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1107;
using AssetRipper.SourceGenerated.Classes.ClassID_91;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.NativeEnums.Animation;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerLayer;
using AssetRipper.SourceGenerated.Subclasses.AnimatorControllerParameter;
using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using AssetRipper.SourceGenerated.Subclasses.ValueConstant;
using System.Diagnostics;

namespace AssetRipper.Processing.AnimatorControllers;

public sealed class AnimatorControllerProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Reconstruct AnimatorController Assets");

		List<IAnimatorController> animatorControllers = gameData.GameBundle
			.FetchAssetCollections()
			.Where(c => c.Flags.IsRelease())
			.SelectMany(c => c.OfType<IAnimatorController>())
			.ToList();

		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Generated AnimatorController Dependencies");
		foreach (IAnimatorController controller in animatorControllers)
		{
			Process(controller, processedCollection);
		}

		List<HashSet<IUnityObjectBase>> assetSets = [];
		Dictionary<IUnityObjectBase, List<int>> assetToSetIndices = new();
		foreach (IAnimatorController controller in animatorControllers)
		{
			int setIndex = assetSets.Count;
			HashSet<IUnityObjectBase> assetSet = [];
			assetSets.Add(assetSet);

			foreach (IUnityObjectBase dependency in controller.FetchEditorHierarchy().WhereNotNull())
			{
				assetSet.Add(dependency);
				if (!assetToSetIndices.TryGetValue(dependency, out List<int>? setIndices))
				{
					setIndices = [setIndex];
					assetToSetIndices.Add(dependency, setIndices);
				}
				else if (!setIndices.Contains(setIndex))
				{
					setIndices.Add(setIndex);
				}
			}
		}

		foreach ((IUnityObjectBase asset, List<int> setIndices) in assetToSetIndices)
		{
			if (setIndices.Count <= 1)
			{
				continue;
			}

			for (int i = 1; i < setIndices.Count; i++)
			{
				int currentSetIndex = setIndices[i];
				HashSet<IUnityObjectBase> currentSet = assetSets[currentSetIndex];
				currentSet.Remove(asset);

				IUnityObjectBase clonedAsset = processedCollection.CreateAsset(asset.ClassID, AssetFactory.Create);
				SingleReplacementAssetResolver resolver = new(asset, clonedAsset);
				clonedAsset.CopyValues(asset, new PPtrConverter(asset.Collection, clonedAsset.Collection, resolver));

				foreach (IUnityObjectBase otherAsset in currentSet)
				{
					otherAsset.CopyValues(otherAsset, new PPtrConverter(otherAsset.Collection, otherAsset.Collection, resolver));
				}

				Debug.Assert(!currentSet.OfType<IAnimatorController>().First().FetchEditorHierarchy().Contains(asset));
				Debug.Assert(currentSet.OfType<IAnimatorController>().First().FetchEditorHierarchy().Contains(clonedAsset));

				currentSet.Add(clonedAsset);
			}
		}
		assetToSetIndices.Clear();

		Debug.Assert(assetSets.Count == animatorControllers.Count);
		for (int i = 0; i < animatorControllers.Count; i++)
		{
			IAnimatorController controller = animatorControllers[i];
			HashSet<IUnityObjectBase> assetSet = assetSets[i];
			Debug.Assert(controller.FetchEditorHierarchy().WhereNotNull().Distinct().Count() == assetSet.Count);
			Debug.Assert(controller.FetchEditorHierarchy().WhereNotNull().All(assetSet.Contains));
			foreach (IUnityObjectBase asset in assetSet)
			{
				Debug.Assert(asset.MainAsset is null);
				asset.MainAsset = controller;
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
