using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Prefabs;
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
		try
		{
			SceneBuilder sceneBuilder = GlbLevelBuilder.Build(assets, isScene);
			using Stream fileStream = fileSystem.File.Create(path);
			sceneBuilder
				.ToGltf2(SceneBuilderSchema2Settings.Default with { MergeBuffers = false }) // Setting MergeBuffers here to false is actually meaningful.
				.WriteGLB(fileStream, new WriteSettings() { MergeBuffers = false }); // Setting MergeBuffers here to false doesn't actually do anything because SharpGLTF changes it back to true.
			return true;
		}
		catch (InvalidOperationException ex) when (ex.Message == "Can't merge a buffer larger than 2Gb")
		{
			Logger.Error(LogCategory.Export, $"Model was too large to export as GLB.");
			return false;
		}
		catch (OutOfMemoryException)
		{
			Logger.Error(LogCategory.Export, $"Could not allocate enough contiguous memory to export the model as GLB.");
			return false;
		}
	}
}
