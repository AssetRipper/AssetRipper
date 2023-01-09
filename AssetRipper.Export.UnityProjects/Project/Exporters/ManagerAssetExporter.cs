using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_6;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class ManagerAssetExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is GlobalGameManager && asset is not IBuildSettings;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new ManagerExportCollection(this, asset);
		}
	}
}
