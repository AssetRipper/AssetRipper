using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_18;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using AssetRipper.SourceGenerated.Subclasses.PPtr_EditorExtension;
using System.Diagnostics;

namespace AssetRipper.Processing;

public sealed class PrefabProcessor : IAssetProcessor
{
	public void Process(GameBundle gameBundle, UnityVersion projectVersion)
	{
		ProcessedAssetCollection processedCollection = gameBundle.AddNewProcessedCollection("Generated Prefab Assets", projectVersion);

		HashSet<IGameObject> gameObjects = new();
		foreach (IPrefabInstance prefab in gameBundle.FetchAssets().OfType<IPrefabInstance>())
		{
			IGameObject? root = prefab.RootGameObject_C1001P;
			if (root is not null)
			{
				gameObjects.Add(root);
			}
		}

		foreach (IGameObject asset in gameBundle.FetchAssets().OfType<IGameObject>())
		{
			if (gameObjects.Contains(asset))
			{
				continue;
			}

			IGameObject root = asset.GetRoot();
			if (gameObjects.Add(root))
			{
				gameObjects.AddRange(root.FetchHierarchy().OfType<IGameObject>());

				if (!root.Collection.IsScene && processedCollection.Version.IsLess(2018, 3))
				{
					IPrefabInstance prefab = CreatePrefab(processedCollection, root);
					IPrefabMarker? prefabMarker = prefab as IPrefabMarker;
					Debug.Assert(prefab is not IPrefabInstanceMarker);

					PPtrAccessList<PPtr_EditorExtension_3_4_0, IEditorExtension>? objects = prefab.Has_Objects_C1001() ? prefab.Objects_C1001P : null;

					foreach (IEditorExtension editorExtension in root.FetchHierarchy())
					{
						objects?.Add(editorExtension);
						editorExtension.PrefabInternal_C18P = prefabMarker;
					}
				}
			}
		}
	}

	private static IPrefabInstance CreatePrefab(ProcessedAssetCollection virtualFile, IGameObject root)
	{
		IPrefabInstance prefab = virtualFile.CreateAsset((int)ClassIDType.PrefabInstance, PrefabInstanceFactory.CreateAsset);

		prefab.RootGameObject_C1001P = root;
		prefab.IsPrefabAsset_C1001 = true;
		prefab.Name_C1001?.CopyValues(root.Name_C1);
		prefab.AssetBundleName = root.AssetBundleName;
		prefab.OriginalDirectory = root.OriginalDirectory;
		prefab.OriginalName = root.OriginalName;
		prefab.OriginalExtension = root.OriginalExtension;

		root.MainAsset = prefab;

		return prefab;
	}
}
