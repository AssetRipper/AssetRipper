using AssetRipper.Assets;
using AssetRipper.Processing.Prefabs;

namespace AssetRipper.Export.UnityProjects.Project;

public class SceneYamlExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		switch (asset.MainAsset)
		{
			case SceneHierarchyObject sceneHierarchyObject:
				exportCollection = new SceneExportCollection(this, sceneHierarchyObject);
				return true;
			case PrefabHierarchyObject prefabHierarchyObject:
				exportCollection = new PrefabExportCollection(this, prefabHierarchyObject);
				return true;
			default:
				exportCollection = null;
				return false;
		}
	}
}
