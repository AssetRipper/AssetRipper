using AssetRipper.Assets;
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
	public class Ripper
	{
		private GameStructure? gameStructure;

		public Ripper() : this(new()) { }

		public Ripper(LibraryConfiguration configuration)
		{
			Settings = configuration;
		}

		[MemberNotNullWhen(true, nameof(gameStructure))]
		public bool IsLoaded => gameStructure != null;

		public GameStructure GameStructure
		{
			[MemberNotNull(nameof(gameStructure))]
			get
			{
				ThrowIfGameStructureIsNull();
				return gameStructure;
			}
		}

		/// <summary>
		/// Needs to be set before loading assets to ensure predictable behavior
		/// </summary>
		public LibraryConfiguration Settings { get; }

		public void ResetData()
		{
			gameStructure?.Dispose();
			gameStructure = null;
		}

		[MemberNotNull(nameof(gameStructure))]
		public GameStructure Load(IReadOnlyList<string> paths)
		{
			ResetData();
			if (paths.Count == 1)
			{
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths[0]}");
			}
			else
			{
				Logger.Info(LogCategory.General, $"Attempting to read files from {paths.Count} paths...");
			}

			gameStructure = GameStructure.Load(paths, Settings);
			Logger.Info(LogCategory.General, "Finished reading files");

			Logger.Info(LogCategory.General, "Processing loaded assets...");
			GameData gameData = GameData.FromGameStructure(gameStructure);
			foreach (IAssetProcessor processor in GetProcessors())
			{
				processor.Process(gameData);
			}
			Logger.Info(LogCategory.General, "Finished processing assets");

			return GameStructure;

			IEnumerable<IAssetProcessor> GetProcessors()
			{
				if (Settings.ScriptContentLevel == ScriptContentLevel.Level1)
				{
					yield return new MethodStubbingProcessor();
				}
				yield return new SceneDefinitionProcessor();
				yield return new MainAssetProcessor();
				yield return new LightingDataProcessor();
				yield return new AnimatorControllerProcessor();
				yield return new AudioMixerProcessor();
				yield return new EditorFormatProcessor(Settings.BundledAssetsExportMode);
				//Static mesh separation goes here
				if (Settings.EnablePrefabOutlining)
				{
					yield return new PrefabOutliningProcessor();
				}
				yield return new PrefabProcessor();
				yield return new SpriteProcessor();
			}
		}

		public IEnumerable<IUnityObjectBase> FetchLoadedAssets()
		{
			return GameStructure.FileCollection.FetchAssets();
		}

		[MemberNotNull(nameof(gameStructure))]
		private void ThrowIfGameStructureIsNull()
		{
			if (gameStructure == null)
			{
				throw new NullReferenceException("GameStructure cannot be null");
			}
		}

		[MemberNotNull(nameof(gameStructure))]
		public void ExportProject(string exportPath, Action<ProjectExporter>? onBeforeExport = null)
		{
			Logger.Info(LogCategory.Export, $"Attempting to export assets to {exportPath}...");
			ThrowIfGameStructureIsNull();

			Logger.Info(LogCategory.Export, "Starting export");
			Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameStructure.FileCollection)}");
			UnityVersion version = gameStructure.FileCollection.GetMaxUnityVersion();
			Logger.Info(LogCategory.Export, $"Exporting to Unity version {version}");

			Settings.ExportRootPath = exportPath;
			Settings.SetProjectSettings(version, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

			{
				ProjectExporter projectExporter = new(Settings, gameStructure.AssemblyManager);
				onBeforeExport?.Invoke(projectExporter);
				projectExporter.Export(gameStructure.FileCollection, Settings);
			}
			Logger.Info(LogCategory.Export, "Finished exporting assets");

			foreach (IPostExporter postExporter in GetPostExporters())
			{
				postExporter.DoPostExport(this);
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

			static IEnumerable<IPostExporter> GetPostExporters()
			{
				yield return new ProjectVersionPostExporter();
				yield return new PackageManifestPostExporter();
				yield return new StreamingAssetsPostExporter();
				yield return new DllPostExporter();
				yield return new PathIdMapExporter();
			}
		}
	}
}
