using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Processing.Scenes;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class SceneYamlExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return SceneHelpers.IsSceneCompatible(asset);
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			if (asset.Collection.IsScene)
			{
				return new SceneExportCollection(this, asset.Collection.Scene);
			}
			else if (PrefabExportCollection.IsValidAsset(asset))
			{
				return new PrefabExportCollection(this, virtualFile, asset);
			}
			else
			{
				return new FailExportCollection(this, asset);
			}
		}
	}
}
