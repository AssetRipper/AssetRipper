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
		private GameStructure() { }

		~GameStructure()
		{
			Dispose(false);
		}

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
			if (filter == null) filter = new Func<Object, bool>(DefaultAssetFilter);

			Version defaultVersion = new Version(2017, 3, 0, VersionType.Final, 3);
			Version maxVersion = FileCollection.GameFiles.Values.Max(t => t.Version);
			Version version = defaultVersion < maxVersion ? maxVersion : defaultVersion;
			ExportOptions options = new ExportOptions(version, Platform.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);
			options.Filter = filter;
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

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool _)
		{
			FileCollection?.Dispose();
		}

		private void Load(List<string> paths, LayoutInfo layinfo)
		{
			if (CheckPC(paths)) { }
			else if (CheckLinux(paths)) { }
			else if (CheckMac(paths)) { }
			else if (CheckAndroid(paths)) { }
			else if (CheckiOS(paths)) { }
			else if (CheckSwitch(paths)) { }
			else if (CheckWebGL(paths)) { }
			else if (CheckWebPlayer(paths)) { }
			CheckMixed(paths);

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
					processor.ProcessSchemes(FileCollection);
				}
			}
		}

		private bool CheckPC(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (PCGameStructure.IsPCStructure(path))
				{
					PlatformStructure = new PCGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"PC game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckLinux(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (LinuxGameStructure.IsLinuxStructure(path))
				{
					PlatformStructure = new LinuxGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Linux game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckMac(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (MacGameStructure.IsMacStructure(path))
				{
					PlatformStructure = new MacGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Mac game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckAndroid(List<string> paths)
		{
			string androidStructure = null;
			string obbStructure = null;
			foreach (string path in paths)
			{
				if (AndroidGameStructure.IsAndroidStructure(path))
				{
					if (androidStructure == null)
					{
						androidStructure = path;
					}
					else
					{
						throw new Exception("2 Android game stuctures has been found");
					}
				}
				else if (AndroidGameStructure.IsAndroidObbStructure(path))
				{
					if (obbStructure == null)
					{
						obbStructure = path;
					}
					else
					{
						throw new Exception("2 Android obb game stuctures has been found");
					}
				}
			}

			if (androidStructure != null)
			{
				PlatformStructure = new AndroidGameStructure(androidStructure, obbStructure);
				paths.Remove(androidStructure);
				Logger.Log(LogType.Info, LogCategory.Import, $"Android game structure has been found at '{androidStructure}'");
				if (obbStructure != null)
				{
					paths.Remove(obbStructure);
					Logger.Log(LogType.Info, LogCategory.Import, $"Android obb game structure has been found at '{obbStructure}'");
				}
				return true;
			}

			return false;
		}

		private bool CheckiOS(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (iOSGameStructure.IsiOSStructure(path))
				{
					PlatformStructure = new iOSGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"iOS game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckSwitch(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (SwitchGameStructure.IsSwitchStructure(path))
				{
					PlatformStructure = new SwitchGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Switch game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckWebGL(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (WebGLGameStructure.IsWebGLStructure(path))
				{
					PlatformStructure = new WebGLGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckWebPlayer(List<string> paths)
		{
			foreach (string path in paths)
			{
				if (WebPlayerGameStructure.IsWebPlayerStructure(path))
				{
					PlatformStructure = new WebPlayerGameStructure(path);
					paths.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private void CheckMixed(List<string> paths)
		{
			if (paths.Count > 0)
			{
				MixedStructure = new MixedGameStructure(paths);
				paths.Clear();
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
		public bool IsValid => FileCollection != null;

		public GameCollection FileCollection { get; private set; }
		public PlatformGameStructure PlatformStructure { get; private set; }
		public PlatformGameStructure MixedStructure { get; private set; }
	}
}
