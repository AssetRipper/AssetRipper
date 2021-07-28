using AssetRipper.Project;
using AssetRipper.Layout;
using AssetRipper.Logging;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.Structure.Assembly;
using AssetRipper.Structure.GameStructure.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityObject = AssetRipper.Classes.Object.UnityObject;
using Version = AssetRipper.Parser.Files.Version;

namespace AssetRipper.Structure.GameStructure
{
	public sealed class GameStructure : IDisposable
	{
		public bool IsValid => FileCollection != null;

		public GameCollection FileCollection { get; private set; }
		public PlatformGameStructure PlatformStructure { get; private set; }
		public PlatformGameStructure MixedStructure { get; private set; }

		private GameStructure() { }

		public static GameStructure Load(IEnumerable<string> paths) => Load(paths, null);
		public static GameStructure Load(IEnumerable<string> paths, LayoutInfo layinfo)
		{
			List<string> toProcess = new List<string>();
			toProcess.AddRange(paths);
			if (toProcess.Count == 0)
			{
				throw new ArgumentException("Game files not found", nameof(paths));
			}

			GameStructure structure = new GameStructure();//an empty constructor
			structure.Load(toProcess, layinfo);
			return structure;
		}

		private void Load(List<string> paths, LayoutInfo layinfo)
		{
			PlatformChecker.CheckPlatform(paths, out PlatformGameStructure platformStructure, out MixedGameStructure mixedStructure);
			PlatformStructure = platformStructure;
			MixedStructure = mixedStructure;
			//The PlatformGameStructure constructor adds all the paths to the Assemblies and Files dictionaries
			//No bundles or assemblies have been loaded yet

			using (GameStructureProcessor processor = new GameStructureProcessor())
			{
				//This block adds all the files to the processor
				//It determines each of their file types, but still no extraction
				if (PlatformStructure != null)
				{
					ProcessPlatformStructure(processor, PlatformStructure);
				}
				if (MixedStructure != null)
				{
					ProcessPlatformStructure(processor, MixedStructure);
				}
				processor.AddDependencySchemes(RequestDependency);

				if (!processor.IsValid)
				{
					Logger.Log(LogType.Warning, LogCategory.Import, "The game structure processor could not find any valid assets.");
				}
				else
				{
					//Assigns a layout if one wasn't already provided
					layinfo = layinfo ?? processor.GetLayoutInfo();

					//Initializes all the component layouts
					AssetLayout layout = new AssetLayout(layinfo);

					//Setting the parameters for exporting
					GameCollection.Parameters pars = new GameCollection.Parameters(layout);
					pars.ScriptBackend = GetScriptingBackend();
					Logger.Log(LogType.Info, LogCategory.Import, $"Files use the '{pars.ScriptBackend}' scripting backend.");
					pars.RequestAssemblyCallback = OnRequestAssembly;
					pars.RequestResourceCallback = OnRequestResource;

					//Sets its fields and creates the Project Exporter
					FileCollection = new GameCollection(pars);

					//Loads any Mono or IL2Cpp assemblies
					FileCollection.AssemblyManager.Initialize(PlatformStructure);

					//Creates new objects for each scheme in the collection
					processor.ProcessSchemes(FileCollection);
				}
			}
		}

		public void Export(string exportPath) => Export(exportPath, null);
		public void Export(string exportPath, Func<UnityObject, bool> filter)
		{
			Version defaultVersion = new Version(2017, 3, 0, VersionType.Final, 3);
			Version maxVersion = FileCollection.GameFiles.Values.Max(t => t.Version);
			Version version = defaultVersion < maxVersion ? maxVersion : defaultVersion;
			Logger.Log(LogType.Info, LogCategory.Export, $"Exporting to Unity version {version.ToString()}");
			ExportOptions options = new ExportOptions(version, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			options.Filter = filter ?? new Func<UnityObject, bool>((UnityObject obj) => true);
			FileCollection.Exporter.Export(exportPath, FileCollection, FileCollection.FetchSerializedFiles(), options);
		}

		/// <summary>Attempts to find the path for the dependency with that name.</summary>
		public string RequestDependency(string dependency)
		{
			if (PlatformStructure != null)
			{
				string path = PlatformStructure.RequestDependency(dependency);
				if (path != null)
				{
					return path;
				}
			}
			if (MixedStructure != null)
			{
				string path = MixedStructure.RequestDependency(dependency);
				if (path != null)
				{
					return path;
				}
			}

			return null;
		}

		public string RequestAssembly(string assembly)
		{
			if (PlatformStructure != null)
			{
				string assemblyPath = PlatformStructure.RequestAssembly(assembly);
				if (assemblyPath != null)
				{
					return assemblyPath;
				}
			}
			if (MixedStructure != null)
			{
				string assemblyPath = MixedStructure.RequestAssembly(assembly);
				if (assemblyPath != null)
				{
					return assemblyPath;
				}
			}

			return null;
		}

		public string RequestResource(string resource)
		{
			if (PlatformStructure != null)
			{
				string path = PlatformStructure.RequestResource(resource);
				if (path != null)
				{
					return path;
				}
			}
			if (MixedStructure != null)
			{
				string path = MixedStructure.RequestResource(resource);
				if (path != null)
				{
					return path;
				}
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

		private ScriptingBackend GetScriptingBackend()
		{
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

		private string OnRequestAssembly(string assembly) => RequestAssembly(assembly);

		private string OnRequestResource(string resource) => RequestResource(resource);

		public string Name
		{
			get
			{
				if (PlatformStructure == null)
					return MixedStructure.Name;
				else
					return PlatformStructure.Name;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool _)
		{
			FileCollection?.Dispose();
		}

		~GameStructure()
		{
			Dispose(false);
		}

	}
}
