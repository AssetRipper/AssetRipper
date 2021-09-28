using AssetRipper.Core.Configuration;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Structure.GameStructure
{
	public sealed class GameStructure : IDisposable
	{
		public GameCollection FileCollection { get; private set; }
		public PlatformGameStructure PlatformStructure { get; private set; }
		public PlatformGameStructure MixedStructure { get; private set; }

		private GameStructure() { }

		public bool IsValid => FileCollection != null;

		public string Name => PlatformStructure?.Name ?? MixedStructure?.Name;

		public static GameStructure Load(IEnumerable<string> paths, CoreConfiguration configuration) => Load(paths, configuration, null);
		public static GameStructure Load(IEnumerable<string> paths, CoreConfiguration configuration, LayoutInfo layinfo)
		{
			List<string> toProcess = Preprocessor.Process(paths);
			if (toProcess.Count == 0)
				throw new ArgumentException("Game files not found", nameof(paths));

			GameStructure structure = new GameStructure();//an empty constructor
			structure.Load(toProcess, configuration, layinfo);
			return structure;
		}

		private void Load(List<string> paths, CoreConfiguration configuration, LayoutInfo layinfo)
		{
			Logger.SendStatusChange("Collecting files and detecting game structure");
			PlatformChecker.CheckPlatform(paths, out PlatformGameStructure platformStructure, out MixedGameStructure mixedStructure);
			PlatformStructure = platformStructure;
			PlatformStructure?.CollectFiles(configuration.IgnoreStreamingAssets);
			MixedStructure = mixedStructure;
			//The PlatformGameStructure constructor adds all the paths to the Assemblies and Files dictionaries
			//No bundles or assemblies have been loaded yet

			using (GameStructureProcessor processor = new GameStructureProcessor())
			{
				//This block adds all the files to the processor
				//It determines each of their file types, but still no extraction
				if (PlatformStructure != null)
					ProcessPlatformStructure(processor, PlatformStructure);
				if (MixedStructure != null)
					ProcessPlatformStructure(processor, MixedStructure);
				processor.AddDependencySchemes(RequestDependency);

				if (!processor.IsValid)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "The game structure processor could not find any valid assets.");
				}
				else
				{
					Logger.SendStatusChange("Initializing asset layout...");
					
					//Assigns a layout if one wasn't already provided
					layinfo ??= processor.GetLayoutInfo();

					//Initializes all the component layouts
					AssetLayout layout = new AssetLayout(layinfo);

					//Setting the parameters for exporting
					GameCollection.Parameters pars = new GameCollection.Parameters(layout);
					pars.ScriptBackend = GetScriptingBackend(configuration.DisableScriptImport);
					pars.PlatformStructure = PlatformStructure;
					Logger.Info(LogCategory.Import, $"Files use the '{pars.ScriptBackend}' scripting backend.");
					pars.RequestAssemblyCallback = OnRequestAssembly;
					pars.RequestResourceCallback = OnRequestResource;
					
					Logger.SendStatusChange("Creating file collection");

					//Sets its fields and creates the Project Exporter
					FileCollection = new GameCollection(pars, configuration);

					Logger.SendStatusChange("Starting scheme processing");
					
					//Creates new objects for each scheme in the collection
					processor.ProcessSchemes(FileCollection);
				}
			}
		}

		public void Export(CoreConfiguration options)
		{
			UnityVersion maxFileVersion = FileCollection.GameFiles.Values.Max(t => t.Version);
			UnityVersion version = UnityVersion.Max(maxFileVersion, UnityVersion.DefaultVersion);
			Logger.Info(LogCategory.Export, $"Exporting to Unity version {version}");
			options.SetProjectSettings(version, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			FileCollection.Export(options);
		}

		/// <summary>Attempts to find the path for the dependency with that name.</summary>
		public string RequestDependency(string dependency)
		{
			if (PlatformStructure != null)
			{
				string path = PlatformStructure.RequestDependency(dependency);
				if (path != null) return path;
			}
			if (MixedStructure != null)
			{
				string path = MixedStructure.RequestDependency(dependency);
				if (path != null) return path;
			}
			return null;
		}

		/// <summary>Processes all files, gets their file type, and adds it to one big list.</summary>
		private void ProcessPlatformStructure(GameStructureProcessor processor, PlatformGameStructure structure)
		{
			foreach (KeyValuePair<string, string> file in structure.Files)
			{
				processor.AddScheme(file.Value, file.Key);
			}
		}

		private ScriptingBackend GetScriptingBackend(bool disableScriptImport)
		{
			if(disableScriptImport)
			{
				Logger.Info(LogCategory.Import, "Script import disabled by settings.");
				return ScriptingBackend.Unknown;
			}
			if (PlatformStructure != null)
			{
				ScriptingBackend backend = PlatformStructure.Backend;
				if (backend != ScriptingBackend.Unknown) return backend;
			}
			if (MixedStructure != null)
			{
				ScriptingBackend backend = MixedStructure.Backend;
				if (backend != ScriptingBackend.Unknown) return backend;
			}
			return ScriptingBackend.Unknown;
		}

		private string OnRequestAssembly(string assembly) => RequestAssembly(assembly);

		private string OnRequestResource(string resource) => RequestResource(resource);

		public string RequestAssembly(string assembly)
		{
			if (PlatformStructure != null)
			{
				string assemblyPath = PlatformStructure.RequestAssembly(assembly);
				if (assemblyPath != null) return assemblyPath;
			}
			if (MixedStructure != null)
			{
				string assemblyPath = MixedStructure.RequestAssembly(assembly);
				if (assemblyPath != null) return assemblyPath;
			}
			return null;
		}

		public string RequestResource(string resource)
		{
			if (PlatformStructure != null)
			{
				string path = PlatformStructure.RequestResource(resource);
				if (path != null) return path;
			}
			if (MixedStructure != null)
			{
				string path = MixedStructure.RequestResource(resource);
				if (path != null) return path;
			}
			return null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool _) => FileCollection?.Dispose();
		~GameStructure() => Dispose(false);
	}
}
