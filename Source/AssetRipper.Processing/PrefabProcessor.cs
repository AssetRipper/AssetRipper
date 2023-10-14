using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using System.Diagnostics;

namespace AssetRipper.Processing;

public sealed class PrefabProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Generated Prefab Assets");

		HashSet<IGameObject> gameObjectsAlreadyProcessed = new();
		List<IGameObject> gameObjectsWithNoTransform = new();
		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			switch (asset)
			{
				case IGameObject gameObject:
					if (!gameObject.TryGetComponent<ITransform>(out _))
					{
						gameObjectsWithNoTransform.Add(gameObject);
					}
					break;
				case IPrefabInstance prefab:
					if (prefab.RootGameObjectP is { } root)
					{
						gameObjectsAlreadyProcessed.Add(root);
					}
					break;
			}
		}

		foreach (IGameObject gameObject in gameObjectsWithNoTransform)
		{
			Logger.Warning(LogCategory.Processing, $"GameObject {gameObject.Name} has no Transform. Adding one.");

			ITransform transform = processedCollection.CreateAsset((int)ClassIDType.Transform, Transform.Create);

			transform.InitializeDefault();

			transform.GameObject_C4P = gameObject;
			gameObject.AddComponent(ClassIDType.Transform, transform);
		}

		foreach (IGameObject asset in gameData.GameBundle.FetchAssets().OfType<IGameObject>())
		{
			if (gameObjectsAlreadyProcessed.Contains(asset))
			{
				continue;
			}

			IGameObject root = asset.GetRoot();
			if (gameObjectsAlreadyProcessed.Add(root))
			{
				gameObjectsAlreadyProcessed.AddRange(root.FetchHierarchy().OfType<IGameObject>());

				if (!root.Collection.IsScene && processedCollection.Version.IsLess(2018, 3))
				{
					IPrefabInstance prefab = CreatePrefab(processedCollection, root);
					IPrefabMarker? prefabMarker = prefab as IPrefabMarker;
					Debug.Assert(prefab is not IPrefabInstanceMarker);

					foreach (IEditorExtension editorExtension in root.FetchHierarchy())
					{
						editorExtension.PrefabInternal_C18P = prefabMarker;
					}
				}
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

		root.MainAsset = prefab;

		return prefab;
	}
}
