using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Processing;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.PrimaryContent.Models;

public class GlbModelExporter : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		switch (asset.MainAsset)
		{
			case SceneHierarchyObject sceneHierarchyObject:
				exportCollection = new GlbSceneModelExportCollection(this, sceneHierarchyObject);
				return true;
			case PrefabHierarchyObject prefabHierarchyObject:
				exportCollection = new GlbPrefabModelExportCollection(this, prefabHierarchyObject);
				return true;
			default:
				exportCollection = null;
				return false;
		}
	}

	public bool Export(IEnumerable<IUnityObjectBase> assets, string path)
	{
		return ExportModel(assets, path, false); //Called by the prefab exporter
	}

	public static bool ExportModel(IEnumerable<IUnityObjectBase> assets, string path, bool isScene)
	{
		SceneBuilder sceneBuilder = GlbLevelBuilder.Build(assets, isScene);
		using FileStream fileStream = File.Create(path);
		sceneBuilder.ToGltf2().WriteGLB(fileStream);
		return true;
	}
}
