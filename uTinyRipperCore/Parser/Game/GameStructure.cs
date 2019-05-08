using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper
{
	public sealed class GameStructure : IDisposable
	{
		private class Processor : IDisposable
		{
			public Processor(FileCollection fileCollection, Func<string, string> dependencyCallback)
			{
				m_fileCollection = fileCollection ?? throw new ArgumentNullException(nameof(fileCollection));
				m_dependencyCallback = dependencyCallback ?? throw new ArgumentNullException(nameof(dependencyCallback));
			}

			~Processor()
			{
				Dispose(false);
			}

			public void ProcessFile(string fileName, string filePath)
			{
				if (m_loadedFiles.Contains(fileName))
				{
					return;
				}

				FileScheme scheme = FileCollection.LoadScheme(filePath, fileName);
				OnSchemeLoaded(scheme);

				if (LoadDependencies(scheme))
				{
					m_fileCollection.AddFile(scheme, m_fileCollection, m_fileCollection.AssemblyManager);
					scheme.Dispose();
				}
				else
				{
					m_delayedSchemes.Add(fileName, scheme);
				}
			}

			public void PostProcess()
			{
#warning TODO:
				foreach (FileScheme scheme in m_delayedSchemes.Values)
				{
					m_fileCollection.AddFile(scheme, m_fileCollection, m_fileCollection.AssemblyManager);
				}
			}

			public void Dispose()
			{
				GC.SuppressFinalize(this);
				Dispose(true);
			}

			private void Dispose(bool _)
			{
				foreach (FileScheme scheme in m_delayedSchemes.Values)
				{
					scheme.Dispose();
				}
			}

			private bool LoadDependencies(FileScheme scheme)
			{
				bool loaded = true;
#warning TODO: fetch unresolved (external) dependencies
				foreach (FileIdentifier dependency in scheme.Dependencies)
				{
					if (m_loadedFiles.Contains(dependency.FilePath))
					{
						continue;
					}

					string fileSystemPath = m_dependencyCallback.Invoke(dependency.FilePath);
					if (fileSystemPath == null)
					{
						if (m_knownFiles.Add(dependency.FilePath))
						{
							Logger.Log(LogType.Warning, LogCategory.Import, $"Dependency '{dependency}' hasn't been found");
						}
						loaded = false;
					}
					else
					{
						ProcessFile(dependency.FilePath, fileSystemPath);
					}
				}
				return loaded;
			}

			private void OnSchemeLoaded(FileScheme scheme)
			{
				m_loadedFiles.Add(scheme.Name);
				m_knownFiles.Add(scheme.Name);

				if (scheme is FileSchemeList list)
				{
					foreach (FileScheme nestedScheme in list.Schemes)
					{
						OnSchemeLoaded(nestedScheme);
					}
				}
			}

			private readonly HashSet<string> m_loadedFiles = new HashSet<string>();
			private readonly HashSet<string> m_knownFiles = new HashSet<string>();
			private readonly Dictionary<string, FileScheme> m_delayedSchemes = new Dictionary<string, FileScheme>();

			private readonly FileCollection m_fileCollection;
			private readonly Func<string, string> m_dependencyCallback;
		}

		private GameStructure()
		{
			FileCollection.Parameters pars = new FileCollection.Parameters()
			{
				RequestAssemblyCallback = OnRequestAssembly,
				RequestResourceCallback = OnRequestResource,
			};
			FileCollection = new FileCollection(pars);
		}

		~GameStructure()
		{
			Dispose(false);
		}

		public static GameStructure Load(IEnumerable<string> pathes)
		{
			List<string> toProcess = new List<string>();
			toProcess.AddRange(pathes);
			if (toProcess.Count == 0)
			{
				throw new ArgumentException("No pathes found", nameof(pathes));
			}

			GameStructure structure = new GameStructure();
			structure.Load(toProcess);
			return structure;
		}

		public void Export(string exportPath, Func<Object, bool> filter)
		{
			ExportOptions options = new ExportOptions()
			{
				Version = new Version(2017, 3, 0, VersionType.Final, 3),
				Platform = Platform.NoTarget,
				Flags = TransferInstructionFlags.NoTransferInstructionFlags,
			};
			FileCollection.Exporter.Export(exportPath, FileCollection, FileCollection.FetchAssets().Where(t => filter(t)), options);
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
			if (m_knownAssemblies.Add(assembly))
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

				Logger.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
				return null;
			}
			else
			{
				return null;
			}
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
			FileCollection.Dispose();
		}

		private void Load(List<string> pathes)
		{
			if (CheckPC(pathes)) {}
			else if (CheckLinux(pathes)) {}
			else if (CheckMac(pathes)) {}
			else if (CheckAndroid(pathes)) {}
			else if (CheckiOS(pathes)) {}
			else if (CheckWebGL(pathes)) {}
			else if (CheckWebPlayer(pathes)) {}
			CheckMixed(pathes);

			Processor processor = new Processor(FileCollection, RequestDependency);
			if (PlatformStructure != null)
			{
				ProcessGameStructure(processor, PlatformStructure);
			}
			if (MixedStructure != null)
			{
				ProcessGameStructure(processor, MixedStructure);
			}
			processor.PostProcess();
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

		private bool CheckWebGL(List<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (WebGLStructure.IsWebGLStructure(path))
				{
					PlatformStructure = new WebGLStructure(path);
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
				if (WebPlayerStructure.IsWebPlayerStructure(path))
				{
					PlatformStructure = new WebPlayerStructure(path);
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

		private void ProcessGameStructure(Processor processor, PlatformGameStructure structure)
		{
			SetScriptingBackend(structure);
			foreach (KeyValuePair<string, string> file in structure.Files)
			{
				processor.ProcessFile(file.Key, file.Value);
			}
		}

		private void SetScriptingBackend(PlatformGameStructure structure)
		{
			ScriptingBackEnd backend = structure.GetScriptingBackend();
			if (backend == ScriptingBackEnd.Unknown)
			{
				return;
			}
			if (FileCollection.AssemblyManager.ScriptingBackEnd == backend)
			{
				return;
			}

			if (FileCollection.AssemblyManager.ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				FileCollection.AssemblyManager.ScriptingBackEnd = backend;
			}
			else
			{
				throw new Exception("Scripting backend is already set");
			}
		}

		private void OnRequestAssembly(string assembly)
		{
			string assemblyPath = RequestAssembly(assembly);
			if (assemblyPath !=	null)
			{
				FileCollection.LoadAssembly(assemblyPath);
				Logger.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
			}
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

		public FileCollection FileCollection { get; }
		public PlatformGameStructure PlatformStructure { get; private set; }
		public PlatformGameStructure MixedStructure { get; private set; }

		private readonly HashSet<string> m_knownAssemblies = new HashSet<string>();
	}
}
