using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_141;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class BuildSettingsExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IBuildSettings;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new BuildSettingsExportCollection(this, virtualFile, asset);
		}
	}
}
