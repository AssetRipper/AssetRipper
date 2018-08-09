using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Mono;

namespace UtinyRipper
{
	internal class MixedGameStructure : PlatformGameStructure
	{
		public MixedGameStructure(FileCollection collection):
			base(collection)
		{
		}

		public void AddFiles(IEnumerable<string> pathes)
		{
			foreach (string path in pathes)
			{
				if (FileMultiStream.Exists(path))
				{
					AddFile(path);
				}
				else if (Directory.Exists(path))
				{
					DirectoryInfo directory = new DirectoryInfo(path);
					AddDirectory(directory);
				}
				else
				{
					throw new Exception($"Neither file nor directory with path '{path}' exists");
				}
			}
		}

		public override IEnumerable<string> FetchFiles()
		{
			return m_files.Values;
		}

		public override IEnumerable<string> FetchAssemblies()
		{
			return m_assemblies;
		}

		public override bool RequestDependency(string dependency)
		{
			if(m_files.ContainsKey(dependency))
			{
				return true;
			}

			return base.RequestDependency(dependency);
		}

		private void AddFile(string path)
		{
			string name = FileMultiStream.GetFileName(path);
			m_files.Add(name, path);
			string directory = Path.GetDirectoryName(path);
			m_knownDirectories.Add(directory);
		}

		private void AddDirectory(DirectoryInfo directory)
		{
			foreach (FileInfo file in directory.EnumerateFiles())
			{
				if (file.Extension == AssetBundleExtension)
				{
					string onlyName = Path.GetFileNameWithoutExtension(file.Name);
					m_files.Add(onlyName, file.FullName);
					Logger.Log(LogType.Info, LogCategory.Import, $"Asset bundle '{onlyName}' has been found at '{directory.FullName}'");
				}
				else if (AssemblyManager.IsAssembly(file.Name))
				{
					if (MonoManager.IsMonoAssembly(file.Name))
					{
						m_fileCollection.AssemblyManager.ScriptingBackEnd = ScriptingBackEnd.Mono;
					}
					else
					{
						m_fileCollection.AssemblyManager.ScriptingBackEnd = ScriptingBackEnd.Il2Cpp;
					}

					m_assemblies.Add(file.FullName);

					Logger.Log(LogType.Info, LogCategory.Import, $"Assembly '{file.Name}' has been found at '{directory.FullName}'");
				}
			}

			CheckGameDirectory(directory);

			foreach(DirectoryInfo subDirectory in directory.EnumerateDirectories())
			{
				AddDirectory(subDirectory);
			}
		}

		private void CheckGameDirectory(DirectoryInfo directory)
		{
			string filePath = Path.Combine(directory.FullName, MainDataName);
			if (File.Exists(filePath))
			{
				AddFile(filePath);
				Logger.Log(LogType.Info, LogCategory.Import, $"'{MainDataName}' has been found at '{directory.FullName}'");
			}

			filePath = Path.Combine(directory.FullName, GlobalGameManagerName);
			if (File.Exists(filePath))
			{
				AddFile(filePath);
				Logger.Log(LogType.Info, LogCategory.Import, $"'{GlobalGameManagerName}' has been found at '{directory.FullName}'");
			}
			
			foreach (FileInfo levelFile in directory.EnumerateFiles())
			{
				if (m_levelName.IsMatch(levelFile.Name))
				{
					AddFile(levelFile.FullName);
					Logger.Log(LogType.Info, LogCategory.Import, $"'{levelFile.Name}' has been found at '{directory.FullName}'");
				}
			}

			string streamingPath = Path.Combine(directory.FullName, StreamingName);
			DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
			if (streamingDirectory.Exists)
			{
				foreach (string path in FetchAssetBundles(streamingDirectory))
				{
					AddFile(filePath);
					string name = Path.GetFileNameWithoutExtension(path);
					Logger.Log(LogType.Info, LogCategory.Import, $"Asset bundle '{name}' has been found at '{streamingDirectory.FullName}'");
				}
			}
		}

		public override string Name => m_files.First().Key;
		public override string MainDataPath => throw new NotSupportedException();
		public override IEnumerable<string> DataPathes => m_knownDirectories;

		private readonly Dictionary<string, string> m_files = new Dictionary<string, string>();
		private readonly HashSet<string> m_assemblies = new HashSet<string>();
		private readonly HashSet<string> m_knownDirectories = new HashSet<string>();
	}
}
