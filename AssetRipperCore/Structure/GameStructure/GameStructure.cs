using AssetRipper.Converters.Project;
using AssetRipper.Layout;
using AssetRipper.Logging;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.Structure.Assembly;
using AssetRipper.Structure.GameStructure.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = AssetRipper.Parser.Classes.Object.Object;
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

			GameStructure structure = new GameStructure();
			structure.Load(toProcess, layinfo);
			return structure;
		}

		private static bool DefaultAssetFilter(Object asset) => true;

		public void Export(string exportPath) => Export(exportPath, null);
		public void Export(string exportPath, Func<Object, bool> filter)
		{
			Version defaultVersion = new Version(2017, 3, 0, VersionType.Final, 3);
			Version maxVersion = FileCollection.GameFiles.Values.Max(t => t.Version);
			Version version = defaultVersion < maxVersion ? maxVersion : defaultVersion;
			ExportOptions options = new ExportOptions(version, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			options.Filter = filter ?? new Func<Object, bool>(DefaultAssetFilter);
			FileCollection.Exporter.Export(exportPath, FileCollection, FileCollection.FetchSerializedFiles(), options);
		}

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

		private void Load(List<string> paths, LayoutInfo layinfo)
		{
			PlatformChecker.CheckPlatform(paths, out PlatformGameStructure platformStructure, out MixedGameStructure mixedStructure);
			PlatformStructure = platformStructure;
			MixedStructure = mixedStructure;

			using (GameStructureProcessor processor = new GameStructureProcessor())
			{
				if (PlatformStructure != null)
				{
					ProcessPlatformStructure(processor, PlatformStructure);
				}
				if (MixedStructure != null)
				{
					ProcessPlatformStructure(processor, MixedStructure);
				}
				processor.AddDependencySchemes(RequestDependency);

				if (processor.IsValid)
				{
					layinfo = layinfo ?? processor.GetLayoutInfo();
					AssetLayout layout = new AssetLayout(layinfo);
					GameCollection.Parameters pars = new GameCollection.Parameters(layout);
					pars.ScriptBackend = GetScriptingBackend();
					Logger.Log(LogType.Info, LogCategory.Import, $"Files use the '{pars.ScriptBackend}' scripting backend.");
					pars.RequestAssemblyCallback = OnRequestAssembly;
					pars.RequestResourceCallback = OnRequestResource;
					FileCollection = new GameCollection(pars);
					FileCollection.AssemblyManager.Initialize(this.PlatformStructure?.GameDataPath);
					processor.ProcessSchemes(FileCollection);
				}
			}
		}

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
				ScriptingBackend backend = PlatformStructure.GetScriptingBackend();
				if (backend != ScriptingBackend.Unknown)
				{
					return backend;
				}
			}
			if (MixedStructure != null)
			{
				ScriptingBackend backend = MixedStructure.GetScriptingBackend();
				if (backend != ScriptingBackend.Unknown)
				{
					return backend;
				}
			}
			return ScriptingBackend.Unknown;
		}

		private string OnRequestAssembly(string assembly)
		{
			return RequestAssembly(assembly);
		}

		private string OnRequestResource(string resource)
		{
			return RequestResource(resource);
		}

		public string Name
		{
			get
			{
				if (PlatformStructure == null)
				{
					return MixedStructure.Name;
				}
				else
				{
					return PlatformStructure.Name;
				}
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
