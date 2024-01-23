using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_1045;
using AssetRipper.SourceGenerated.Classes.ClassID_6;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class ManagerAssetExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IGlobalGameManager manager)
			{
				exportCollection = manager is IEditorBuildSettings editorBuildSettings
					? new EditorBuildSettingsExportCollection(this, editorBuildSettings)
					: new ManagerExportCollection(this, manager);
				return true;
			}
			exportCollection = null;
			return false;
		}
	}
}
