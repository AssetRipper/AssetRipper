using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Layout;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
{
	public sealed class GameStructure : IDisposable
	{
		private GameStructure()
		{
		}

		~GameStructure()
		{
			Dispose(false);
		}

		public static GameStructure Load(IEnumerable<string> pathes)
		{
			return Load(pathes, null);
		}

		public static GameStructure Load(IEnumerable<string> pathes, LayoutInfo layinfo)
		{
			List<string> toProcess = new List<string>();
			toProcess.AddRange(pathes);
			if (toProcess.Count == 0)
			{
				throw new ArgumentException("Game files not found", nameof(pathes));
			}

			GameStructure structure = new GameStructure();
			structure.Load(toProcess, layinfo);
			return structure;
		}

		public void Export(string exportPath, Func<Object, bool> filter)
		{
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

		private void Load(List<string> pathes, LayoutInfo layinfo)
		{
			if (CheckPC(pathes)) {}
			else if (CheckLinux(pathes)) {}
			else if (CheckMac(pathes)) {}
			else if (CheckAndroid(pathes)) {}
			else if (CheckiOS(pathes)) {}
			else if (CheckSwitch(pathes)) {}
			else if (CheckWebGL(pathes)) {}
			else if (CheckWebPlayer(pathes)) {}
			CheckMixed(pathes);

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
					pars.RequestAssemblyCallback = OnRequestAssembly;
					pars.RequestResourceCallback = OnRequestResource;
					FileCollection = new GameCollection(pars);
					processor.ProcessSchemes(FileCollection);
				}
			}
		}

		private bool CheckPC(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (PCGameStructure.IsPCStructure(path))
				{
					PlatformStructure = new PCGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"PC game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckLinux(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (LinuxGameStructure.IsLinuxStructure(path))
				{
					PlatformStructure = new LinuxGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Linux game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckMac(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (MacGameStructure.IsMacStructure(path))
				{
					PlatformStructure = new MacGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Mac game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckAndroid(List<string> pathes)
		{
			string androidStructure = null;
			string obbStructure = null;
			foreach (string path in pathes)
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
				pathes.Remove(androidStructure);
				Logger.Log(LogType.Info, LogCategory.Import, $"Android game structure has been found at '{androidStructure}'");
				if (obbStructure != null)
				{
					pathes.Remove(obbStructure);
					Logger.Log(LogType.Info, LogCategory.Import, $"Android obb game structure has been found at '{obbStructure}'");
				}
				return true;
			}

			return false;
		}

		private bool CheckiOS(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (iOSGameStructure.IsiOSStructure(path))
				{
					PlatformStructure = new iOSGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"iOS game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckSwitch(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (SwitchGameStructure.IsSwitchStructure(path))
				{
					PlatformStructure = new SwitchGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Switch game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckWebGL(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (WebGLGameStructure.IsWebGLStructure(path))
				{
					PlatformStructure = new WebGLGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private bool CheckWebPlayer(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (WebPlayerGameStructure.IsWebPlayerStructure(path))
				{
					PlatformStructure = new WebPlayerGameStructure(path);
					pathes.Remove(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"WebPlayer game structure has been found at '{path}'");
					return true;
				}
			}
			return false;
		}

		private void CheckMixed(List<string> pathes)
		{
			if (pathes.Count > 0)
			{
				MixedStructure = new MixedGameStructure(pathes);
				pathes.Clear();
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
