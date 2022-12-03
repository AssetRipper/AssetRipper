using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class SceneYamlExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return SceneExportHelpers.IsSceneCompatible(asset);
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			if (asset.Collection.IsScene)
			{
				return new SceneExportCollection(this, asset.Collection);
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
