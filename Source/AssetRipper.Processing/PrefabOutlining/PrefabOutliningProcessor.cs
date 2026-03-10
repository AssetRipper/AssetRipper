using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.PrefabOutlining;

public sealed class PrefabOutliningProcessor : IAssetProcessor
{
	private const int MinimumRepetitionCount = 2;

	// Documentation note: prefab variants were introduced in 2018.3.
	// That does not affect this processor currently, but it may have an impact on future improvements.
	// https://blog.unity.com/technology/introducing-unity-2018-3

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Prefab Outlining");
		ProcessedAssetCollection processedCollection = gameData.AddNewProcessedCollection("Outlined Prefabs");

		MakeDictionaries(
			gameData.GameBundle,
			out Dictionary<IGameObject, GameObjectInfo> infoDictionary,
			out Dictionary<AssetCollection, bool> sceneInfo);

		MakeBoxes(
			infoDictionary,
			sceneInfo,
			out Dictionary<string, Dictionary<GameObjectInfo, List<IGameObject>>> boxes,
			out HashSet<IGameObject> prefabRoots);

		HashSet<string> generatedNames = new(StringComparer.Ordinal);
		int createdCount = 0;
		int skippedNoRepetitionCount = 0;
		int skippedExistingPrefabCount = 0;
		int skippedNoSceneCandidateCount = 0;

		foreach ((string cleanedName, Dictionary<GameObjectInfo, List<IGameObject>> variants) in boxes.OrderBy(pair => pair.Key, StringComparer.Ordinal))
		{
			bool hasMultipleVariants = variants.Count > 1;
			int variantIndex = 0;
			foreach (List<IGameObject> candidates in variants.Values
				.OrderByDescending(static c => c.Count)
				.ThenBy(GetMinimumPathId))
			{
				variantIndex++;
				if (candidates.Count < MinimumRepetitionCount)
				{
					skippedNoRepetitionCount++;
					continue;
				}
				if (candidates.Any(g => prefabRoots.Contains(g)))
				{
					skippedExistingPrefabCount++;
					continue;
				}
				if (!candidates.Any(g => sceneInfo[g.Collection]))
				{
					skippedNoSceneCandidateCount++;
					continue;
				}

				string prefabName = GetUniquePrefabName(cleanedName, hasMultipleVariants, variantIndex, generatedNames);
				IGameObject source = SelectSource(candidates, sceneInfo);
				Logger.Info(LogCategory.Processing, $"Recreating prefab for {prefabName} from {candidates.Count} matches");
				AddCopyToCollection(prefabName, source, processedCollection);
				createdCount++;
			}
		}

		Logger.Info(LogCategory.Processing,
			$"Prefab outlining summary: created={createdCount}, " +
			$"skipped_not_repeated={skippedNoRepetitionCount}, " +
			$"skipped_existing_prefab={skippedExistingPrefabCount}, " +
			$"skipped_without_scene_candidates={skippedNoSceneCandidateCount}");
	}

	private static void AddCopyToCollection(string name, IGameObject source, ProcessedAssetCollection collection)
	{
		// AddPrefabPlaceHolder(name, collection);
		AddNewPrefab(name, source, collection);
	}

	private static void AddNewPrefab(string name, IGameObject source, ProcessedAssetCollection collection)
	{
		IGameObject root = GameObjectCloner.Clone(source, collection);
		root.Name = name;
		root.SetIsActive(true);

		ITransform transform = root.GetTransform();
		transform.LocalPosition_C4.Reset();
		// Keep the original scale and rotation, as the prefab root may have intended non-identity values.
		// transform.LocalRotation_C4.Reset();
		// transform.LocalScale_C4.SetValues(1, 1, 1);
		// transform.LocalEulerAnglesHint_C4?.Reset();
		transform.RootOrder_C4 = 0;
		transform.Father_C4.Reset();
	}

	private static void AddPrefabPlaceHolder(string name, ProcessedAssetCollection collection)
	{
		// Place holder code until source gen improves

		IGameObject root = CreateNewGameObject(collection);
		root.Name = name;
		root.SetIsActive(true);
		root.TagString = TagManagerConstants.UntaggedTag;

		ITransform rootTransform = CreateNewTransform(collection);
		rootTransform.GameObject_C4P = root;
		rootTransform.RootOrder_C4 = 0;
		// Since this Transform has no Father, its RootOrder is zero.

		root.AddComponent(ClassIDType.Transform, rootTransform);

		IGameObject child = CreateNewGameObject(collection);
		child.Name = "This prefab is a placeholder until AssetRipper improves.";
		child.SetIsActive(true);
		child.TagString = TagManagerConstants.UntaggedTag;

		ITransform childTransform = CreateNewTransform(collection);
		childTransform.GameObject_C4P = child;
		childTransform.RootOrder_C4 = 0;
		// Since this Transform is the only child, its RootOrder is zero.

		childTransform.Father_C4P = rootTransform;
		rootTransform.Children_C4P.Add(childTransform);

		child.AddComponent(ClassIDType.Transform, childTransform);
	}

	private static IGameObject CreateNewGameObject(ProcessedAssetCollection collection)
	{
		return collection.CreateAsset((int)ClassIDType.GameObject, GameObject.Create);
	}

	private static ITransform CreateNewTransform(ProcessedAssetCollection collection)
	{
		return collection.CreateAsset((int)ClassIDType.Transform, Transform.Create);
	}

	private static void MakeBoxes(Dictionary<IGameObject, GameObjectInfo> infoDictionary, Dictionary<AssetCollection, bool> sceneInfo, out Dictionary<string, Dictionary<GameObjectInfo, List<IGameObject>>> boxes, out HashSet<IGameObject> prefabRoots)
	{
		boxes = [];
		prefabRoots = [];
		foreach ((IGameObject gameObject, GameObjectInfo info) in infoDictionary)
		{
			string name = GameObjectNameCleaner.CleanName(gameObject.Name);
			if (!boxes.TryGetValue(name, out Dictionary<GameObjectInfo, List<IGameObject>>? variants))
			{
				variants = [];
				boxes.Add(name, variants);
			}
			if (!variants.TryGetValue(info, out List<IGameObject>? candidates))
			{
				candidates = [];
				variants.Add(info, candidates);
			}
			candidates.Add(gameObject);

			if (!sceneInfo[gameObject.Collection] && gameObject.IsRoot())
			{
				prefabRoots.Add(gameObject);
			}
		}
	}

	private static IGameObject SelectSource(List<IGameObject> candidates, Dictionary<AssetCollection, bool> sceneInfo)
	{
		foreach (IGameObject candidate in candidates)
		{
			if (sceneInfo[candidate.Collection] && candidate.IsRoot())
			{
				return candidate;
			}
		}
		foreach (IGameObject candidate in candidates)
		{
			if (candidate.IsRoot())
			{
				return candidate;
			}
		}
		return candidates[0];
	}

	private static long GetMinimumPathId(List<IGameObject> candidates)
	{
		long minimum = long.MaxValue;
		foreach (IGameObject candidate in candidates)
		{
			minimum = Math.Min(minimum, candidate.PathID);
		}
		return minimum;
	}

	private static string GetUniquePrefabName(string cleanedName, bool hasMultipleVariants, int variantIndex, HashSet<string> generatedNames)
	{
		string baseName = hasMultipleVariants
			? $"{cleanedName}_Outlined_{variantIndex}"
			: cleanedName;

		if (generatedNames.Add(baseName))
		{
			return baseName;
		}

		int duplicateIndex = 2;
		while (true)
		{
			string candidate = $"{baseName}_{duplicateIndex}";
			if (generatedNames.Add(candidate))
			{
				return candidate;
			}
			duplicateIndex++;
		}
	}

	private static void MakeDictionaries(GameBundle gameBundle, out Dictionary<IGameObject, GameObjectInfo> infoDictionary, out Dictionary<AssetCollection, bool> sceneInfo)
	{
		infoDictionary = [];
		sceneInfo = [];
		foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
		{
			sceneInfo.Add(collection, collection.IsScene);
			GameObjectInfo.AddCollectionToDictionary(collection, infoDictionary);
		}
	}
}
