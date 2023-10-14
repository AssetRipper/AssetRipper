using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.PrefabOutlining
{
	public sealed class PrefabOutliningProcessor : IAssetProcessor
	{
		//Documentation note: prefab variants were introduced in 2018.3.
		//That does not affect this processor currently, but it may have an impact on future improvements.
		//https://blog.unity.com/technology/introducing-unity-2018-3

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
			foreach ((string name, Dictionary<GameObjectInfo, List<IGameObject>> variants) in boxes)
			{
				if (variants.Count != 1)
				{
					continue;//We want the simplest implementation to start out
				}

				(_, List<IGameObject> box) = variants.First();

				if (box.Any(g => prefabRoots.Contains(g)))
				{
					continue;//Prefab already exists
				}

				Logger.Info(LogCategory.Processing, $"Recreating prefab for {name}");

				AddCopyToCollection(name, box[0], processedCollection);
			}
		}

		private static void AddCopyToCollection(string name, IGameObject source, ProcessedAssetCollection collection)
		{
			//AddPrefabPlaceHolder(name, collection);
			AddNewPrefab(name, source, collection);
		}

		private static void AddNewPrefab(string name, IGameObject source, ProcessedAssetCollection collection)
		{
			IGameObject root = GameObjectCloner.Clone(source, collection);
			root.Name = name;
			root.SetIsActive(true);

			ITransform transform = root.GetTransform();
			transform.LocalPosition_C4.Reset();
			transform.LocalRotation_C4.Reset();
			transform.LocalScale_C4.SetValues(1, 1, 1);
			transform.LocalEulerAnglesHint_C4?.Reset();
			transform.RootOrder_C4 = 0;
			transform.Father_C4.Reset();
		}

		private static void AddPrefabPlaceHolder(string name, ProcessedAssetCollection collection)
		{
			//Place holder code until source gen improves

			IGameObject root = CreateNewGameObject(collection);
			root.Name = name;
			root.SetIsActive(true);
			root.TagString = TagManagerConstants.UntaggedTag;

			ITransform rootTransform = CreateNewTransform(collection);
			rootTransform.GameObject_C4P = root;
			rootTransform.RootOrder_C4 = 0;
			//Since this Transform has no Father, its RootOrder is zero.

			root.AddComponent(ClassIDType.Transform, rootTransform);

			IGameObject child = CreateNewGameObject(collection);
			child.Name = "This prefab is a placeholder until AssetRipper improves.";
			child.SetIsActive(true);
			child.TagString = TagManagerConstants.UntaggedTag;

			ITransform childTransform = CreateNewTransform(collection);
			childTransform.GameObject_C4P = child;
			childTransform.RootOrder_C4 = 0;
			//Since this Transform is the only child, its RootOrder is zero.

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
			boxes = new();
			prefabRoots = new();
			foreach ((IGameObject gameObject, GameObjectInfo info) in infoDictionary)
			{
				string name = GameObjectNameCleaner.CleanName(gameObject.Name);
				boxes.GetOrAdd(name).GetOrAdd(info).Add(gameObject);
				if (!sceneInfo[gameObject.Collection] && gameObject.IsRoot())
				{
					prefabRoots.Add(gameObject);
				}
			}
		}

		private static void MakeDictionaries(GameBundle gameBundle, out Dictionary<IGameObject, GameObjectInfo> infoDictionary, out Dictionary<AssetCollection, bool> sceneInfo)
		{
			infoDictionary = new();
			sceneInfo = new();
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				sceneInfo.Add(collection, collection.IsScene);
				GameObjectInfo.AddCollectionToDictionary(collection, infoDictionary);
			}
		}
	}
}
