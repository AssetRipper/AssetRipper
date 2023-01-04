using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Project.Collections;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_29;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Diagnostics;

namespace AssetRipper.Processing
{
	public sealed class SceneDefinitionProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Creating Scene Definitions");
			IBuildSettings? buildSettings = null;
			HashSet<AssetCollection> sceneCollections = new();
			Dictionary<AssetCollection, string> scenePaths = new();
			Dictionary<AssetCollection, UnityGUID> sceneGuids = new();
			List<IAssetBundle> sceneAssetBundles = new();

			//Find the relevant assets in this single pass over all the assets.
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				foreach (IUnityObjectBase asset in collection)
				{
					if (asset is ILevelGameManager)
					{
						sceneCollections.Add(collection);
						if (asset is IOcclusionCullingSettings sceneSettings && sceneSettings.Has_SceneGUID_C29())
						{
							sceneGuids[collection] = sceneSettings.SceneGUID_C29;
						}
					}
					else if (asset is IBuildSettings buildSettings1)
					{
						buildSettings = buildSettings1;
					}
					else if (asset is IAssetBundle assetBundle && assetBundle.IsStreamedSceneAssetBundle_C142)
					{
						sceneAssetBundles.Add(assetBundle);
					}
				}
			}

			//Currently, these paths are treated as lower precedent than paths defined in asset bundles, but they should never conflict.
			foreach (AssetCollection sceneCollection in sceneCollections)
			{
				if (SceneExportHelpers.TryGetScenePath(sceneCollection, buildSettings, out string? scenePath))
				{
					scenePaths[sceneCollection] = scenePath;
				}
			}

			//Extract scene paths from asset bundles.
			foreach (IAssetBundle assetBundleAsset in sceneAssetBundles)
			{
				Bundle bundle = assetBundleAsset.Collection.Bundle;
				if (bundle is not SerializedBundle)
				{
					Logger.Log(LogType.Warning, LogCategory.Processing, $"Scene name recovery is not supported for bundles of type {bundle.GetType().Name}");
				}
				else
				{
					int startingIndex = 0;
					foreach (AccessPairBase<Utf8String, IAssetInfo> pair in assetBundleAsset.Container_C142)
					{
						Debug.Assert(pair.Value.Asset.IsNull(), "Scene pointer is not null");

						string path = Path.ChangeExtension(pair.Key.String, null);
						Debug.Assert(path.StartsWith("Assets/", StringComparison.Ordinal), "Scene path is not relative to the project directory.");
						int index = IndexOf(bundle.Collections, sceneCollections, startingIndex);
						if (index < 0)
						{
							throw new Exception($"Scene collection not found in {bundle.Name} at or after index {startingIndex}");
						}

						AssetCollection sceneCollection = bundle.Collections[index];
						sceneCollections.Add(sceneCollection);//Just to be safe
						scenePaths[sceneCollection] = path;
						startingIndex = index + 1;
					}
				}
			}

			//Make the scene definitions
			foreach (AssetCollection sceneCollection in sceneCollections)
			{
				SceneDefinition sceneDefinition;
				UnityGUID guid = sceneGuids.TryGetValue(sceneCollection, out UnityGUID sceneGuid) ? sceneGuid : default;
				if (scenePaths.TryGetValue(sceneCollection, out string? path))
				{
					sceneDefinition = SceneDefinition.FromPath(path, guid);
				}
				else
				{
					sceneDefinition = SceneDefinition.FromName(sceneCollection.Name, guid);
				}
				sceneDefinition.AddCollection(sceneCollection);
			}
		}

		private static int IndexOf(IReadOnlyList<AssetCollection> list, HashSet<AssetCollection> containingSet, int startingIndex)
		{
			for (int i = startingIndex; i < list.Count; i++)
			{
				if (containingSet.Contains(list[i]))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
