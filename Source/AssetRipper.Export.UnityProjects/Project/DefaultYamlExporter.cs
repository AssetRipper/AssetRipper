using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class DefaultYamlExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = new AssetExportCollection<IUnityObjectBase>(this, asset);
			return true;
		}
	}
}
