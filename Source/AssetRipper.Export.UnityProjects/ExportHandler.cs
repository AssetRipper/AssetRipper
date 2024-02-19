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

namespace AssetRipper.Export.UnityProjects;

public class ExportHandler
{
	protected LibraryConfiguration Settings { get; }

	public ExportHandler(LibraryConfiguration settings)
	{
		Settings = settings;
	}

	public GameData Load(IReadOnlyList<string> paths)
	{
		if (paths.Count == 1)
		{
			Logger.Info(LogCategory.Import, $"Attempting to read files from {paths[0]}");
		}
		else
		{
			Logger.Info(LogCategory.Import, $"Attempting to read files from {paths.Count} paths...");
		}

		GameStructure gameStructure = GameStructure.Load(paths, Settings);
		GameData gameData = GameData.FromGameStructure(gameStructure);
		Logger.Info(LogCategory.Import, "Finished reading files");
		return gameData;
	}

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Processing loaded assets...");
		foreach (IAssetProcessor processor in GetProcessors())
		{
			processor.Process(gameData);
		}
		Logger.Info(LogCategory.Processing, "Finished processing assets");
	}

	protected virtual IEnumerable<IAssetProcessor> GetProcessors()
	{
		if (Settings.ImportSettings.ScriptContentLevel == ScriptContentLevel.Level1)
		{
			yield return new MethodStubbingProcessor();
		}
		yield return new SceneDefinitionProcessor();
		yield return new MainAssetProcessor();
		yield return new AnimatorControllerProcessor();
		yield return new AudioMixerProcessor();
		yield return new EditorFormatProcessor(Settings.ImportSettings.BundledAssetsExportMode);
		//Static mesh separation goes here
		if (Settings.ProcessingSettings.EnablePrefabOutlining)
		{
			yield return new PrefabOutliningProcessor();
		}
		yield return new LightingDataProcessor();//Needs to be after static mesh separation
		yield return new PrefabProcessor();
		yield return new SpriteProcessor();
	}

	public void Export(GameData gameData, string outputPath)
	{
		Logger.Info(LogCategory.Export, "Starting export");
		Logger.Info(LogCategory.Export, $"Attempting to export assets to {outputPath}...");
		Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameData.GameBundle)}");
		Logger.Info(LogCategory.Export, $"Exporting to Unity version {gameData.ProjectVersion}");

		Settings.ExportRootPath = outputPath;
		Settings.SetProjectSettings(gameData.ProjectVersion, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

		ProjectExporter projectExporter = new(Settings, gameData.AssemblyManager);
		BeforeExport(projectExporter);
		projectExporter.DoFinalOverrides(Settings);
		projectExporter.Export(gameData.GameBundle, Settings);

		Logger.Info(LogCategory.Export, "Finished exporting assets");

		foreach (IPostExporter postExporter in GetPostExporters())
		{
			postExporter.DoPostExport(gameData, Settings);
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

	protected virtual void BeforeExport(ProjectExporter projectExporter)
	{
	}

	protected virtual IEnumerable<IPostExporter> GetPostExporters()
	{
		yield return new ProjectVersionPostExporter();
		yield return new PackageManifestPostExporter();
		yield return new StreamingAssetsPostExporter();
		yield return new DllPostExporter();
		yield return new PathIdMapExporter();
	}

	public GameData LoadAndProcess(IReadOnlyList<string> paths)
	{
		GameData gameData = Load(paths);
		Process(gameData);
		return gameData;
	}

	public void LoadProcessAndExport(IReadOnlyList<string> inputPaths, string outputPath)
	{
		GameData gameData = LoadAndProcess(inputPaths);
		Export(gameData, outputPath);
	}

	public void ThrowIfSettingsDontMatch(LibraryConfiguration settings)
	{
		if (Settings != settings)
		{
			throw new ArgumentException("Settings don't match");
		}
	}
}
