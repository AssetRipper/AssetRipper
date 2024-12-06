using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Processing;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;

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

	public bool Export(IEnumerable<IUnityObjectBase> assets, string path, FileSystem fileSystem)
	{
		return ExportModel(assets, path, false, fileSystem); //Called by the prefab exporter
	}

	public static bool ExportModel(IEnumerable<IUnityObjectBase> assets, string path, bool isScene, FileSystem fileSystem)
	{
		SceneBuilder sceneBuilder = GlbLevelBuilder.Build(assets, isScene);
		using Stream fileStream = fileSystem.File.Create(path);
		sceneBuilder.ToGltf2().WriteGLB(fileStream, new WriteSettings() { MergeBuffers = false });
		return true;
	}
}
