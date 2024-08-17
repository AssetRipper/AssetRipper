using AssetRipper.Assets.Bundles;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.Import.Structure
{
	public sealed class GameStructure : IDisposable
	{
		public GameBundle FileCollection { get; private set; }
		public PlatformGameStructure? PlatformStructure { get; private set; }
		public PlatformGameStructure? MixedStructure { get; private set; }
		public IAssemblyManager AssemblyManager { get; set; }

		private GameStructure(List<string> paths, CoreConfiguration configuration)
		{
			Logger.SendStatusChange("loading_step_detect_platform");
			PlatformChecker.CheckPlatform(paths, out PlatformGameStructure? platformStructure, out MixedGameStructure? mixedStructure);
			PlatformStructure = platformStructure;
			PlatformStructure?.CollectFiles(configuration.ImportSettings.IgnoreStreamingAssets);
			MixedStructure = mixedStructure;
			//MixedStructure?.CollectFiles(configuration.IgnoreStreamingAssets);
			//The PlatformGameStructure constructor adds all the paths to the Assemblies and Files dictionaries
			//No bundles or assemblies have been loaded yet

			Logger.SendStatusChange("loading_step_initialize_layout");

			InitializeAssemblyManager(configuration);

			Logger.SendStatusChange("loading_step_begin_scheme_processing");

			InitializeGameCollection(configuration.ImportSettings.DefaultVersion, configuration.ImportSettings.TargetVersion);

			if (!FileCollection.HasAnyAssetCollections())
			{
				Logger.Log(LogType.Warning, LogCategory.Import, "The game structure processor could not find any valid assets.");
			}
		}

		public bool IsValid => FileCollection.HasAnyAssetCollections();

		public string? Name => PlatformStructure?.Name ?? MixedStructure?.Name;

		public static GameStructure Load(IEnumerable<string> paths, CoreConfiguration configuration)
		{
			List<string> toProcess = ZipExtractor.Process(paths);
			if (toProcess.Count == 0)
			{
				throw new ArgumentException("Game files not found", nameof(paths));
			}

			return new GameStructure(toProcess, configuration);
		}

		[MemberNotNull(nameof(FileCollection))]
		private void InitializeGameCollection(UnityVersion defaultVersion, UnityVersion targetVersion)
		{
			Logger.SendStatusChange("loading_step_create_file_collection");

			GameAssetFactory assetFactory = new GameAssetFactory(AssemblyManager);

			IEnumerable<string> filePaths;
			if (PlatformStructure is null || MixedStructure is null)
			{
				filePaths = (PlatformStructure ?? MixedStructure)?.Files.Values() ?? [];
			}
			else
			{
				filePaths = PlatformStructure.Files.Union(MixedStructure.Files).Select(pair => pair.Value);
			}

			FileCollection = GameBundle.FromPaths(
				filePaths,
				assetFactory,
				new GameInitializer(PlatformStructure, MixedStructure, defaultVersion, targetVersion));
		}

		[MemberNotNull(nameof(AssemblyManager))]
		private void InitializeAssemblyManager(CoreConfiguration configuration)
		{
			ScriptingBackend scriptBackend = GetScriptingBackend(configuration.DisableScriptImport);
			Logger.Info(LogCategory.Import, $"Files use the '{scriptBackend}' scripting backend.");

			AssemblyManager = scriptBackend switch
			{
				ScriptingBackend.Mono => new MonoManager(OnRequestAssembly),
				ScriptingBackend.IL2Cpp => new IL2CppManager(OnRequestAssembly, configuration.ImportSettings.ScriptContentLevel),
				_ => new BaseManager(OnRequestAssembly),
			};

			Logger.SendStatusChange("loading_step_load_assemblies");

			try
			{
				//Loads any Mono or IL2Cpp assemblies
				AssemblyManager.Initialize(PlatformStructure ?? MixedStructure ?? throw new Exception("No platform structure"));
			}
			catch (Exception ex)
			{
				Logger.Error(LogCategory.Import, "Could not initialize assembly manager. Switching to the 'Unknown' scripting backend.");
				Logger.Error(ex);
				AssemblyManager = new BaseManager(OnRequestAssembly);
			}
		}

		private ScriptingBackend GetScriptingBackend(bool disableScriptImport)
		{
			if (disableScriptImport)
			{
				Logger.Info(LogCategory.Import, "Script import disabled by settings.");
				return ScriptingBackend.Unknown;
			}
			if (PlatformStructure != null)
			{
				ScriptingBackend backend = PlatformStructure.Backend;
				if (backend != ScriptingBackend.Unknown)
				{
					return backend;
				}
			}
			if (MixedStructure != null)
			{
				ScriptingBackend backend = MixedStructure.Backend;
				if (backend != ScriptingBackend.Unknown)
				{
					return backend;
				}
			}
			return ScriptingBackend.Unknown;
		}

		private void OnRequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}.dll";
			ResourceFile? resFile = FileCollection.ResolveResource(assemblyName);
			if (resFile is not null)
			{
				resFile.Stream.Position = 0;
				AssemblyManager.Read(resFile.Stream, assemblyName);
			}
			else
			{
				string? path = RequestAssembly(assembly);
				if (path is null)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
					return;
				}
				AssemblyManager.Load(path);
			}
			Logger.Info(LogCategory.Import, $"Assembly '{assembly}' has been loaded");
		}

		public string? RequestAssembly(string assembly)
		{
			return PlatformStructure?.RequestAssembly(assembly) ?? MixedStructure?.RequestAssembly(assembly);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool _)
		{
			AssemblyManager?.Dispose();
			FileCollection?.Dispose();
		}
	}
}
