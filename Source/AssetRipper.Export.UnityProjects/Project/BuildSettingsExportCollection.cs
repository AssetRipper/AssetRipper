using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1045;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_159;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.Scene;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class BuildSettingsExportCollection : ManagerExportCollection
	{
		public BuildSettingsExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, IBuildSettings asset) : base(assetExporter, asset)
		{
			EditorBuildSettingsAsset = CreateVirtualEditorBuildSettings(virtualFile);
			EditorSettingsAsset = CreateVirtualEditorSettings(virtualFile);
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath = Path.Combine(projectDirectory, ProjectSettingsName);
			string fileName = "EditorBuildSettings.asset";
			string filePath = Path.Combine(subPath, fileName);

			Directory.CreateDirectory(subPath);

			IBuildSettings buildSettings = (IBuildSettings)Asset;
			InitializeEditorBuildSettings(EditorBuildSettingsAsset, buildSettings, container);
			AssetExporter.Export(container, EditorBuildSettingsAsset, filePath);

			fileName = "EditorSettings.asset";
			filePath = Path.Combine(subPath, fileName);

			AssetExporter.Export(container, EditorSettingsAsset, filePath);

			return true;
		}

		public static IEditorSettings CreateVirtualEditorSettings(TemporaryAssetCollection virtualFile)
		{
			IEditorSettings result = virtualFile.CreateAsset((int)ClassIDType.EditorSettings, EditorSettings.Create);
			result.SetToDefaults();
			return result;
		}

		public static IEditorBuildSettings CreateVirtualEditorBuildSettings(TemporaryAssetCollection virtualFile)
		{
			return virtualFile.CreateAsset((int)ClassIDType.EditorBuildSettings, EditorBuildSettings.Create);
		}

		public static void InitializeEditorBuildSettings(IEditorBuildSettings editorBuildSettings, IBuildSettings buildSettings, IExportContainer container)
		{
			int numScenes = buildSettings.Scenes.Count;
			editorBuildSettings.Scenes.Capacity = numScenes;
			for (int i = 0; i < numScenes; i++)
			{
				string scenePath = buildSettings.Scenes[i].String;
				IScene scene = editorBuildSettings.Scenes.AddNew();
				scene.Enabled = true;
				scene.Path = scenePath;
				if (scene.Has_Guid())
				{
					scene.Guid.CopyValues(container.SceneNameToGUID(scenePath));
				}
			}
		}

		public override bool IsContains(IUnityObjectBase asset)
		{
			if (asset is IEditorBuildSettings)
			{
				return asset == EditorBuildSettingsAsset;
			}
			else if (asset is IEditorSettings)
			{
				return asset == EditorSettingsAsset;
			}
			else
			{
				return base.IsContains(asset);
			}
		}

		public override long GetExportID(IUnityObjectBase asset)
		{
			return 1;
		}

		public override IEnumerable<IUnityObjectBase> Assets
		{
			get
			{
				yield return Asset;
				yield return EditorBuildSettingsAsset;
				yield return EditorSettingsAsset;
			}
		}

		public IEditorBuildSettings EditorBuildSettingsAsset { get; }
		public IEditorSettings EditorSettingsAsset { get; }
	}
}
