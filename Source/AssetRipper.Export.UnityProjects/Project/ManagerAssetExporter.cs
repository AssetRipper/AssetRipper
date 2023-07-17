using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_6;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class ManagerAssetExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IGlobalGameManager manager && manager is not IBuildSettings)
			{
				exportCollection = new ManagerExportCollection(this, manager);
				return true;
			}
			exportCollection = null;
			return false;
		}
	}
}
