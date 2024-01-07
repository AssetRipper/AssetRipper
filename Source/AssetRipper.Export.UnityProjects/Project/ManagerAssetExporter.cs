using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_1045;
using AssetRipper.SourceGenerated.Classes.ClassID_159;
using AssetRipper.SourceGenerated.Classes.ClassID_6;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class ManagerAssetExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset is IGlobalGameManager or IEditorBuildSettings or IEditorSettings)
			{
				exportCollection = asset is IEditorBuildSettings editorBuildSettings
					? new EditorBuildSettingsExportCollection(this, editorBuildSettings)
					: new ManagerExportCollection(this, asset);
				return true;
			}
			exportCollection = null;
			return false;
		}
	}
}
