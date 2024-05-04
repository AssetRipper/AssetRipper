using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_1660057539;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;

namespace AssetRipper.Processing;

public sealed class PrefabProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		ProcessedBundle processedBundle = gameData.GameBundle.AddNewProcessedBundle("Generated Hierarchy Assets");
		ProcessedAssetCollection sceneHierarchyCollection = processedBundle.AddNewProcessedCollection("Scene Hierarchies", gameData.ProjectVersion);
		ProcessedAssetCollection prefabHierarchyCollection = processedBundle.AddNewProcessedCollection("Prefab Hierarchies", gameData.ProjectVersion);
		ProcessedAssetCollection prefabInstanceCollection = processedBundle.AddNewProcessedCollection("Generated Prefabs", gameData.ProjectVersion);
		ProcessedAssetCollection missingPrefabTransformCollection = processedBundle.AddNewProcessedCollection("Missing Prefab Transforms", gameData.ProjectVersion);
		Dictionary<SceneDefinition, ProcessedAssetCollection> sceneCollectionDictionary = new();

		//Add missing Transforms
		foreach (IGameObject gameObject in gameData.GameBundle.FetchAssets().OfType<IGameObject>().Where<IGameObject>(HasNoTransform))
		{
			Logger.Warning(LogCategory.Processing, $"GameObject {gameObject.Name} has no Transform. Adding one.");

			ProcessedAssetCollection collection;
			if (gameObject.Collection.IsScene)
			{
				SceneDefinition scene = gameObject.Collection.Scene;
				if (sceneCollectionDictionary.TryGetValue(scene, out ProcessedAssetCollection? sceneCollection))
				{
					collection = sceneCollection;
				}
				else
				{
					collection = processedBundle.AddNewProcessedCollection(scene.Name, gameData.ProjectVersion);
					scene.AddCollection(collection);
					sceneCollectionDictionary.Add(scene, collection);
				}
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

		HashSet<IGameObject> gameObjectsAlreadyProcessed = new();

		//Create scene hierarchies
		foreach (SceneDefinition scene in gameData.GameBundle.Scenes)
		{
			SceneHierarchyObject sceneHierarchy = CreateSceneHierarchyObject(sceneHierarchyCollection, scene);
			gameObjectsAlreadyProcessed.AddRange(sceneHierarchy.GameObjects);
		}

		//Create hierarchies for prefabs with an existing PrefabInstance
		foreach (IPrefabInstance prefab in gameData.GameBundle.FetchAssets().OfType<IPrefabInstance>())
		{
			if (prefab.RootGameObjectP is { } root && !gameObjectsAlreadyProcessed.Contains(root))
			{
				//Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
				if (prefab is IPrefabMarker prefabMarker)
				{
					foreach (IEditorExtension editorExtension in root.FetchHierarchy())
					{
						editorExtension.PrefabInternal_C18P = prefabMarker;
					}
				}

				PrefabHierarchyObject prefabHierarchy = CreatePrefabHierarchyObject(prefabHierarchyCollection, root, prefab);
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
				IPrefabInstance prefab = CreatePrefab(prefabInstanceCollection, root);

				//Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
				if (prefab is IPrefabMarker prefabMarker)
				{
					foreach (IEditorExtension editorExtension in root.FetchHierarchy())
					{
						editorExtension.PrefabInternal_C18P = prefabMarker;
					}
				}

				PrefabHierarchyObject prefabHierarchy = CreatePrefabHierarchyObject(prefabHierarchyCollection, root, prefab);
				gameObjectsAlreadyProcessed.AddRange(prefabHierarchy.GameObjects);
			}
		}
	}

	private static IPrefabInstance CreatePrefab(ProcessedAssetCollection virtualFile, IGameObject root)
	{
		IPrefabInstance prefab = virtualFile.CreateAsset((int)ClassIDType.PrefabInstance, PrefabInstance.Create);

		prefab.HideFlagsE = HideFlags.HideInHierarchy;
		prefab.RootGameObjectP = root;
		prefab.IsPrefabAsset = true;
		prefab.AssetBundleName = root.AssetBundleName;
		prefab.OriginalDirectory = root.OriginalDirectory;
		prefab.OriginalName = root.OriginalName;
		prefab.OriginalExtension = root.OriginalExtension;

		return prefab;
	}

	private static SceneHierarchyObject CreateSceneHierarchyObject(ProcessedAssetCollection virtualFile, SceneDefinition scene)
	{
		SceneHierarchyObject sceneHierarchy = virtualFile.CreateAsset((int)ClassIDType.SceneAsset, (assetInfo) => new SceneHierarchyObject(assetInfo, scene));

		foreach (IUnityObjectBase asset in scene.Assets)
		{
			switch (asset)
			{
				case IGameObject gameObject:
					sceneHierarchy.GameObjects.Add(gameObject);
					break;
				case IMonoBehaviour monoBehaviour:
					if (monoBehaviour.IsSceneObject())
					{
						sceneHierarchy.Components.Add(monoBehaviour);
					}
					break;
				case IComponent component:
					sceneHierarchy.Components.Add(component);
					break;
				case ILevelGameManager manager:
					sceneHierarchy.Managers.Add(manager);
					break;
				case IPrefabInstance prefabInstance:
					sceneHierarchy.PrefabInstances.Add(prefabInstance);
					break;
				case ISceneRoots sceneRoots:
					sceneHierarchy.SceneRoots = sceneRoots;
					break;
			}
		}

		sceneHierarchy.SetMainAssets();

		return sceneHierarchy;
	}

	private static PrefabHierarchyObject CreatePrefabHierarchyObject(ProcessedAssetCollection virtualFile, IGameObject root, IPrefabInstance prefab)
	{
		PrefabHierarchyObject prefabHierarchy = virtualFile.CreateAsset((int)ClassIDType.PrefabInstance, (assetInfo) => new PrefabHierarchyObject(assetInfo, root, prefab));

		foreach (IEditorExtension asset in root.FetchHierarchy())
		{
			switch (asset)
			{
				case IGameObject gameObject:
					prefabHierarchy.GameObjects.Add(gameObject);
					break;
				case IComponent component:
					prefabHierarchy.Components.Add(component);
					break;
			}
		}

		prefabHierarchy.SetMainAssets();

		return prefabHierarchy;
	}

	private static bool HasNoTransform(IGameObject gameObject)
	{
		return !gameObject.TryGetComponent<ITransform>(out _);
	}
}
