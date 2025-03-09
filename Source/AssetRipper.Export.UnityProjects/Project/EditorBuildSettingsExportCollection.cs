using AssetRipper.SourceGenerated.Classes.ClassID_1045;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.Scene;

namespace AssetRipper.Export.UnityProjects.Project;

public sealed class EditorBuildSettingsExportCollection : ManagerExportCollection
{
	public EditorBuildSettingsExportCollection(IAssetExporter assetExporter, IEditorBuildSettings asset) : base(assetExporter, asset)
	{
	}

	public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		SetSceneGuidValues((IEditorBuildSettings)Asset, container);
		return base.Export(container, projectDirectory, fileSystem);
	}

	private static void SetSceneGuidValues(IEditorBuildSettings editorBuildSettings, IExportContainer container)
	{
		foreach (IScene scene in editorBuildSettings.Scenes)
		{
			if (scene.Has_Guid())
			{
				scene.Guid.CopyValues(container.ScenePathToGUID(Path.ChangeExtension(scene.Path, null)));
			}
		}
	}
}
