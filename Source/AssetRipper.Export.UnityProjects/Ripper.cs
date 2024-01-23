using AssetRipper.Assets.Bundles;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.PathIdMapping;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
using AssetRipper.Processing.AnimatorControllers;
using AssetRipper.Processing.Assemblies;
using AssetRipper.Processing.AudioMixers;
using AssetRipper.Processing.Editor;
using AssetRipper.Processing.PrefabOutlining;
using AssetRipper.Processing.Scenes;
using AssetRipper.Processing.Textures;

namespace AssetRipper.Export.UnityProjects
{
	public static class Ripper
	{
		public static GameData Load(IReadOnlyList<string> paths, CoreConfiguration settings)
		{
			if (paths.Count == 1)
			{
				Logger.Info(LogCategory.Import, $"Attempting to read files from {paths[0]}");
			}
			else
			{
				Logger.Info(LogCategory.Import, $"Attempting to read files from {paths.Count} paths...");
			}

			GameStructure gameStructure = GameStructure.Load(paths, settings);
			GameData gameData = GameData.FromGameStructure(gameStructure);
			Logger.Info(LogCategory.Import, "Finished reading files");
			return gameData;
		}

		public static void Process(GameData gameData, IEnumerable<IAssetProcessor> processors)
		{
			Logger.Info(LogCategory.Processing, "Processing loaded assets...");
			foreach (IAssetProcessor processor in processors)
			{
				processor.Process(gameData);
			}
			Logger.Info(LogCategory.Processing, "Finished processing assets");
		}

		public static IEnumerable<IAssetProcessor> GetDefaultProcessors(LibraryConfiguration settings)
		{
			if (settings.ScriptContentLevel == ScriptContentLevel.Level1)
			{
				yield return new MethodStubbingProcessor();
			}
			yield return new SceneDefinitionProcessor();
			yield return new MainAssetProcessor();
			yield return new LightingDataProcessor();
			yield return new AnimatorControllerProcessor();
			yield return new AudioMixerProcessor();
			yield return new EditorFormatProcessor(settings.BundledAssetsExportMode);
			//Static mesh separation goes here
			if (settings.EnablePrefabOutlining)
			{
				yield return new PrefabOutliningProcessor();
			}
			yield return new PrefabProcessor();
			yield return new SpriteProcessor();
		}

		public static void ExportProject(GameData gameData, LibraryConfiguration settings, string exportPath, IEnumerable<IPostExporter>? postExporters = null, Action<ProjectExporter>? beforeExport = null)
		{
			Logger.Info(LogCategory.Export, "Starting export");
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameData.GameBundle)}");
			Logger.Info(LogCategory.Export, $"Exporting to Unity version {gameData.ProjectVersion}");

			settings.ExportRootPath = exportPath;
			settings.SetProjectSettings(gameData.ProjectVersion, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

			ProjectExporter projectExporter = new(settings, gameData.AssemblyManager);
			beforeExport?.Invoke(projectExporter);
			projectExporter.DoFinalOverrides(settings);
			projectExporter.Export(gameData.GameBundle, settings);

			Logger.Info(LogCategory.Export, "Finished exporting assets");

			foreach (IPostExporter postExporter in postExporters ?? GetDefaultPostExporters())
			{
				postExporter.DoPostExport(gameData, settings);
			}
			Logger.Info(LogCategory.Export, "Finished post-export");

			static string GetListOfVersions(GameBundle gameBundle)
			{
				return string.Join(' ', gameBundle
					.FetchAssetCollections()
					.Select(c => c.Version)
					.Distinct()
					.Select(v => v.ToString()));
			}
		}

		public static IEnumerable<IPostExporter> GetDefaultPostExporters()
		{
			yield return new ProjectVersionPostExporter();
			yield return new PackageManifestPostExporter();
			yield return new StreamingAssetsPostExporter();
			yield return new DllPostExporter();
			yield return new PathIdMapExporter();
		}
	}
}
