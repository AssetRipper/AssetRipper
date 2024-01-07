using AssetRipper.Assets;
using AssetRipper.Processing.Scenes;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class SceneYamlExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (SceneHelpers.IsSceneCompatible(asset))
			{
				if (asset.Collection.IsScene)
				{
					exportCollection = new SceneExportCollection(this, asset.Collection.Scene);
				}
				else if (PrefabExportCollection.IsValidAsset(asset))
				{
					exportCollection = new PrefabExportCollection(this, asset);
				}
				else
				{
					exportCollection = new FailExportCollection(this, asset);
				}
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}
	}
}
