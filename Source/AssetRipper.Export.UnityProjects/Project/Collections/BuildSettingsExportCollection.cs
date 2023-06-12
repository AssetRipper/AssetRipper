using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1045;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_159;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.Scene;

namespace AssetRipper.Export.UnityProjects.Project.Collections
{
	public sealed class BuildSettingsExportCollection : ManagerExportCollection
	{
		public BuildSettingsExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection file, IUnityObjectBase asset) : this(assetExporter, file, (IBuildSettings)asset) { }

		public BuildSettingsExportCollection(IAssetExporter assetExporter, TemporaryAssetCollection virtualFile, IBuildSettings asset) : base(assetExporter, asset)
		{
			EditorBuildSettings = CreateVirtualEditorBuildSettings(virtualFile);
			EditorSettings = CreateVirtualEditorSettings(virtualFile);
		}

		public override bool Export(IExportContainer container, string projectDirectory)
		{
			string subPath = Path.Combine(projectDirectory, ProjectSettingsName);
			string fileName = "EditorBuildSettings.asset";
			string filePath = Path.Combine(subPath, fileName);

			Directory.CreateDirectory(subPath);

			IBuildSettings buildSettings = (IBuildSettings)Asset;
			InitializeEditorBuildSettings(EditorBuildSettings, buildSettings, container);
			AssetExporter.Export(container, EditorBuildSettings, filePath);

			fileName = "EditorSettings.asset";
			filePath = Path.Combine(subPath, fileName);

			AssetExporter.Export(container, EditorSettings, filePath);

			return true;
		}

		public static IEditorSettings CreateVirtualEditorSettings(TemporaryAssetCollection virtualFile)
		{
			IEditorSettings result = virtualFile.CreateAsset((int)ClassIDType.EditorSettings, EditorSettingsFactory.CreateAsset);
			result.SetToDefaults();
			return result;
		}

		public static IEditorBuildSettings CreateVirtualEditorBuildSettings(TemporaryAssetCollection virtualFile)
		{
			return virtualFile.CreateAsset((int)ClassIDType.EditorBuildSettings, EditorBuildSettingsFactory.CreateAsset);
		}

		public static void InitializeEditorBuildSettings(IEditorBuildSettings editorBuildSettings, IBuildSettings buildSettings, IExportContainer container)
		{
			int numScenes = buildSettings.Scenes_C141.Count;
			editorBuildSettings.Scenes_C1045.Capacity = numScenes;
			for (int i = 0; i < numScenes; i++)
			{
				string scenePath = buildSettings.Scenes_C141[i].String;
				IScene scene = editorBuildSettings.Scenes_C1045.AddNew();
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
				return asset == EditorBuildSettings;
			}
			else if (asset is IEditorSettings)
			{
				return asset == EditorSettings;
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
				yield return EditorBuildSettings;
				yield return EditorSettings;
			}
		}

		public IEditorBuildSettings EditorBuildSettings { get; }
		public IEditorSettings EditorSettings { get; }
	}
}
