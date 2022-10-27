using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_29;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Subclasses.AssetInfo;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AssetRipper.Library.Processors
{
	public sealed class SceneGuidProcessor : IAssetProcessor
	{
		public void Process(GameBundle gameBundle, UnityVersion projectVersion)
		{
			Logger.Info(LogCategory.Processing, "Scene GUID Assignment");
			IBuildSettings? buildSettings = null;
			List<AssetCollection> scenes = new();
			List<IAssetBundle> sceneAssetBundles = new();
			foreach (AssetCollection collection in gameBundle.FetchAssetCollections())
			{
				foreach (IUnityObjectBase asset in collection)
				{
					if (asset is ILevelGameManager)
					{
						scenes.Add(collection);
						if (asset is IOcclusionCullingSettings sceneSettings && sceneSettings.Has_SceneGUID_C29())
						{
							collection.GUID = sceneSettings.SceneGUID_C29;
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
				if (collection.GUID.IsZero)
				{
					collection.GUID = UnityGUID.NewGuid();
				}
			}
			foreach (AssetCollection scene in scenes)
			{
				if (SceneExportHelpers.TryGetScenePath(scene, buildSettings, out string? scenePath))
				{
					scene.ScenePath = scenePath;
				}
			}
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
						
						string name = Path.ChangeExtension(pair.Key.String, null);
						Debug.Assert(name.StartsWith("Assets/", StringComparison.Ordinal), "Scene path is not relative to the project directory.");
						int index = IndexOf(bundle.Collections, scenes, startingIndex);
						if (index < 0)
						{
							throw new Exception($"Scene collection not found in {bundle.Name} at or after index {startingIndex}");
						}
						bundle.Collections[index].ScenePath = name;
						startingIndex = index + 1;
					}
				}
			}
		}

		private static int IndexOf(IReadOnlyList<AssetCollection> list, List<AssetCollection> containingSet, int startingIndex)
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
