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
			ProcessControllerState(controller, processedCollection);
		}

		// Tracks which assets have already been mapped to an earlier AnimatorController
		HashSet<IUnityObjectBase> globallyClaimedAssets = new();

		foreach (IAnimatorController controller in animatorControllers)
		{
			// Unique assets belonging to this controller's graph. 
			// Distinct() prevents a controller from mistakenly cloning an asset from itself.
			List<IUnityObjectBase> hierarchy = controller.FetchEditorHierarchy()
				.WhereNotNull()
				.Distinct()
				.ToList();
			
			hierarchy.Remove(controller);

			Dictionary<IUnityObjectBase, IUnityObjectBase> cloneMap = new();

			// 1. Identify assets that belong to this controller but are ALREADY claimed by an earlier one
			foreach (IUnityObjectBase asset in hierarchy)
			{
				if (!globallyClaimedAssets.Add(asset))
				{
					IUnityObjectBase clonedAsset = processedCollection.CreateAsset(asset.ClassID, AssetFactory.Create);
					cloneMap.Add(asset, clonedAsset);
				}
			}

			// 2. Isolate subgraph if there are shared components
			if (cloneMap.Count > 0)
			{
				MultiReplacementAssetResolver resolver = new(cloneMap);

				// Remap the controller's immediate pointers
				controller.CopyValues(controller, new PPtrConverter(controller.Collection, controller.Collection, resolver));

				foreach (IUnityObjectBase asset in hierarchy)
				{
					if (cloneMap.TryGetValue(asset, out IUnityObjectBase? clonedAsset))
					{
						// Copy the original data into the clone, rewriting pointers mapping to the new subgraph
						clonedAsset.CopyValues(asset, new PPtrConverter(asset.Collection, clonedAsset.Collection, resolver));
					}
					else
					{
						// It's exclusive to this controller. Rewrite any pointers aiming at old shared assets
						asset.CopyValues(asset, new PPtrConverter(asset.Collection, asset.Collection, resolver));
					}
				}
			}

			// 3. Assign MainAsset cleanly using the dynamically updated hierarchy
			foreach (IUnityObjectBase asset in controller.FetchEditorHierarchy().WhereNotNull().Distinct())
			{
				if (asset != controller)
				{
					Debug.Assert(asset.MainAsset is null || asset.MainAsset == controller);
					asset.MainAsset = controller;
				}
			}
		}
	}

	private static void ProcessControllerState(IAnimatorController controller, ProcessedAssetCollection processedCollection)
	{
		IControllerConstant controllerConstant = controller.Controller;

		IAnimatorStateMachine[] stateMachines = new IAnimatorStateMachine[controllerConstant.StateMachineArray.Count];
		for (int i = 0; i < controllerConstant.StateMachineArray.Count; i++)
		{
			stateMachines[i] = VirtualAnimationFactory.CreateRootAnimatorStateMachine(processedCollection, controller, i);
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
			
			// Safe bounds check against obfuscated/corrupted AssetBundles
			IAnimatorStateMachine? stateMachine = stateMachineIndex < stateMachines.Length 
				? stateMachines[stateMachineIndex] 
				: null;

			IAnimatorControllerLayer newLayer = controller.AnimatorLayers.AddNew();
			InitializeLayer(newLayer, stateMachine, controller, i);
		}
	}

	private static void InitializeLayer(IAnimatorControllerLayer animatorControllerLayer, IAnimatorStateMachine? stateMachine, IAnimatorController controller, int layerIndex)
	{
		ILayerConstant layer = controller.Controller.LayerArray[layerIndex].Data;

		animatorControllerLayer.Name = controller.TOS[layer.Binding];

		if (stateMachine is not null)
		{
			animatorControllerLayer.StateMachine.SetAsset(controller.Collection, stateMachine);
		}

#warning TODO: animator Mask
		// animatorControllerLayer.Mask = new();

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
		int index = (int)value.Index;

		// Implemented safety bounds checks to prevent IndexOutOfRange exceptions in corrupted bundles
		switch (type)
		{
			case AnimatorControllerParameterType.Trigger:
			case AnimatorControllerParameterType.Bool:
				if (index >= 0 && index < controller.Controller.DefaultValues.Data.BoolValues.Count)
				{
					parameter.DefaultBool = controller.Controller.DefaultValues.Data.BoolValues[index];
				}
				break;

			case AnimatorControllerParameterType.Int:
				if (index >= 0 && index < controller.Controller.DefaultValues.Data.IntValues.Count)
				{
					parameter.DefaultInt = controller.Controller.DefaultValues.Data.IntValues[index];
				}
				break;

			case AnimatorControllerParameterType.Float:
				if (index >= 0 && index < controller.Controller.DefaultValues.Data.FloatValues.Count)
				{
					parameter.DefaultFloat = controller.Controller.DefaultValues.Data.FloatValues[index];
				}
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

	/// <summary>
	/// Optimized implementation mapping multiple replaced instances simultaneously rather than iterating one by one.
	/// </summary>
	private sealed class MultiReplacementAssetResolver : IAssetResolver
	{
		private readonly Dictionary<IUnityObjectBase, IUnityObjectBase> _replacements;

		public MultiReplacementAssetResolver(Dictionary<IUnityObjectBase, IUnityObjectBase> replacements)
		{
			_replacements = replacements;
		}

		public IUnityObjectBase? Resolve(IUnityObjectBase? asset)
		{
			if (asset is not null && _replacements.TryGetValue(asset, out IUnityObjectBase? replacement))
			{
				return replacement;
			}
			return asset;
		}
	}
}
