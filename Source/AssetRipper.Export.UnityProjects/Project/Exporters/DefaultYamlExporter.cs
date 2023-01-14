using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class DefaultYamlExporter : YamlExporterBase
	{
		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new AssetExportCollection(this, asset);
		}
	}
}
