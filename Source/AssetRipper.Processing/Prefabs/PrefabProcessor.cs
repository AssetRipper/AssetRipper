using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics;

namespace AssetRipper.Processing.Prefabs;

public sealed class PrefabProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		ProcessedBundle processedBundle = gameData.GameBundle.AddNewProcessedBundle("Generated Hierarchy Assets");
		ProcessedAssetCollection prefabHierarchyCollection = processedBundle.AddNewProcessedCollection("Prefab Hierarchies", gameData.ProjectVersion);
		ProcessedAssetCollection prefabInstanceCollection = processedBundle.AddNewProcessedCollection("Generated Prefabs", gameData.ProjectVersion);
		Dictionary<SceneDefinition, ProcessedAssetCollection> sceneCollectionDictionary = new();

		AddMissingTransforms(gameData, processedBundle, sceneCollectionDictionary);

		HashSet<IGameObject> gameObjectsAlreadyProcessed = new();

		//Create scene hierarchies
		foreach (SceneDefinition scene in gameData.GameBundle.Scenes.ToList())
		{
			ProcessedAssetCollection sceneCollection = GetOrCreateSceneCollection(gameData, processedBundle, sceneCollectionDictionary, scene);
			SceneHierarchyObject sceneHierarchy = SceneHierarchyObject.Create(sceneCollection, scene);
			gameObjectsAlreadyProcessed.AddRange(sceneHierarchy.GameObjects);

			Bundle? bundle = scene.Collections.Select(c => c.Bundle).FirstOrDefault(b => b is SerializedBundle);
			if (bundle is not null)
			{
				IAssetBundle? assetBundleAsset = bundle.FetchAssets().OfType<IAssetBundle>().FirstOrDefault();
				if (assetBundleAsset is not null)
				{
					Debug.Assert(!assetBundleAsset.Has_IsStreamedSceneAssetBundle() || assetBundleAsset.IsStreamedSceneAssetBundle);
					sceneHierarchy.AssetBundleName = assetBundleAsset.GetAssetBundleName();
				}
				else
				{
					sceneHierarchy.AssetBundleName = bundle.Name;
				}
			}
		}

		//Create hierarchies for prefabs with an existing PrefabInstance
		foreach (IPrefabInstance prefab in gameData.GameBundle.FetchAssets().OfType<IPrefabInstance>())
		{
			if (prefab.RootGameObjectP is { } root && !gameObjectsAlreadyProcessed.Contains(root))
			{
				prefab.SetPrefabInternal();

				PrefabHierarchyObject prefabHierarchy = PrefabHierarchyObject.Create(prefabHierarchyCollection, root, prefab);
				gameObjectsAlreadyProcessed.AddRange(prefabHierarchy.GameObjects);
			}
		}

		//Create hierarchies for prefabs without an existing PrefabInstance
		foreach (IGameObject asset in gameData.GameBundle.FetchAssets().OfType<IGameObject>())
		{
			if (gameObjectsAlreadyProcessed.Contains(asset))
			{
				continue;
			}

			IGameObject root = asset.GetRoot();
			if (gameObjectsAlreadyProcessed.Add(root))
			{
				IPrefabInstance prefab = root.CreatePrefabForRoot(prefabInstanceCollection);

				PrefabHierarchyObject prefabHierarchy = PrefabHierarchyObject.Create(prefabHierarchyCollection, root, prefab);
				gameObjectsAlreadyProcessed.AddRange(prefabHierarchy.GameObjects);
			}
		}
	}

	private static void AddMissingTransforms(GameData gameData, ProcessedBundle processedBundle, Dictionary<SceneDefinition, ProcessedAssetCollection> sceneCollectionDictionary)
	{
		ProcessedAssetCollection missingPrefabTransformCollection = processedBundle.AddNewProcessedCollection("Missing Prefab Transforms", gameData.ProjectVersion);
		foreach (IGameObject gameObject in gameData.GameBundle.FetchAssets().OfType<IGameObject>().Where(HasNoTransform))
		{
			Logger.Warning(LogCategory.Processing, $"GameObject {gameObject.Name} has no Transform. Adding one.");

			ProcessedAssetCollection collection;
			if (gameObject.Collection.IsScene)
			{
				SceneDefinition scene = gameObject.Collection.Scene;
				collection = GetOrCreateSceneCollection(gameData, processedBundle, sceneCollectionDictionary, scene);
			}
			else
			{
				collection = missingPrefabTransformCollection;
			}

			ITransform transform = collection.CreateAsset((int)ClassIDType.Transform, Transform.Create);

			transform.InitializeDefault();

			transform.GameObject_C4P = gameObject;
			gameObject.AddComponent(ClassIDType.Transform, transform);
		}
	}

	private static ProcessedAssetCollection GetOrCreateSceneCollection(GameData gameData, ProcessedBundle processedBundle, Dictionary<SceneDefinition, ProcessedAssetCollection> sceneCollectionDictionary, SceneDefinition scene)
	{
		ProcessedAssetCollection collection;
		if (sceneCollectionDictionary.TryGetValue(scene, out ProcessedAssetCollection? sceneCollection))
		{
			collection = sceneCollection;
		}
		else
		{
			collection = processedBundle.AddNewProcessedCollection(scene.Name + " (Generated Assets)", gameData.ProjectVersion);
			scene.AddCollection(collection);
			sceneCollectionDictionary.Add(scene, collection);
		}

		return collection;
	}

	private static bool HasNoTransform(IGameObject gameObject)
	{
		return !gameObject.TryGetComponent<ITransform>(out _);
	}
}
