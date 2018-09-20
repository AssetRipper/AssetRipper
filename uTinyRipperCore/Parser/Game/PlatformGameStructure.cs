using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using uTinyRipper.AssetExporters;
using uTinyRipper.AssetExporters.Mono;

namespace uTinyRipper
{
	public abstract class PlatformGameStructure
	{
		public PlatformGameStructure(FileCollection collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			m_fileCollection = collection;
		}
		
		public bool RequestDependency(string dependency)
		{
			if(Files.TryGetValue(dependency, out string dependencyPath))
			{
				LoadDependency(dependency, dependencyPath);
				return true;
			}

			foreach (string dataPath in DataPathes)
			{
				string filePath = Path.Combine(dataPath, dependency);
				if (FileMultiStream.Exists(filePath))
				{
					LoadDependency(dependency, filePath);
					return true;
				}

				if (FilenameUtils.IsDefaultResource(dependency))
				{
					if (LoadEngineDependency(dataPath, FilenameUtils.DefaultResourceName1))
					{
						return true;
					}
					if (LoadEngineDependency(dataPath, FilenameUtils.DefaultResourceName2))
					{
						return true;
					}
				}
				else if (FilenameUtils.IsBuiltinExtra(dependency))
				{
					if (LoadEngineDependency(dataPath, FilenameUtils.BuiltinExtraName1))
					{
						return true;
					}
					if (LoadEngineDependency(dataPath, FilenameUtils.BuiltinExtraName2))
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool RequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			if(Assemblies.TryGetValue(assemblyName, out string assemblyPath))
			{
				m_fileCollection.LoadAssembly(assemblyPath);
				Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
				return true;
			}
			return false;
		}

		public bool RequestResource(string resource, out string path)
		{
			foreach (string dataPath in DataPathes)
			{
				path = Path.Combine(dataPath, resource);
				if(FileMultiStream.Exists(path))
				{
					return true;
				}
			}
			path = null;
			return false;
		}

		protected void SetScriptingBackend()
		{
			if(Assemblies.Count == 0)
			{
				return;
			}

			string assemblyPath = Assemblies.First().Value;
			if(MonoManager.IsMonoAssembly(assemblyPath))
			{
				m_fileCollection.AssemblyManager.ScriptingBackEnd = ScriptingBackEnd.Mono;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		protected void CollectGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			string filePath = Path.Combine(root.FullName, GlobalGameManagerName);
			if (FileMultiStream.Exists(filePath))
			{
				AddFile(files, GlobalGameManagerName, filePath);
			}
			else
			{
				filePath = Path.Combine(root.FullName, MainDataName);
				if (FileMultiStream.Exists(filePath))
				{
					AddFile(files, MainDataName, filePath);
				}
			}

			foreach (FileInfo levelFile in root.EnumerateFiles())
			{
				if (s_levelName.IsMatch(levelFile.Name))
				{
					string levelName = FileMultiStream.GetFileName(levelFile.Name);
					AddFile(files, levelName, levelFile.FullName);
				}
			}
		}

		protected void CollectStreamingAssets(DirectoryInfo root, IDictionary<string, string> files)
		{
			string streamingPath = Path.Combine(root.FullName, StreamingName);
			DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
			if (streamingDirectory.Exists)
			{
				CollectAssetBundlesRecursivly(root, files);
			}
		}
		
		protected void CollectAssetBundles(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach(FileInfo file in root.EnumerateFiles())
			{
				if(file.Extension == AssetBundleExtension)
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
					AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		protected void CollectAssetBundlesRecursivly(DirectoryInfo root, IDictionary<string, string> files)
		{
			CollectAssetBundles(root, files);
			foreach (DirectoryInfo directory in root.EnumerateDirectories())
			{
				CollectAssetBundlesRecursivly(directory, files);
			}
		}

		protected void CollectAssemblies(DirectoryInfo root, IDictionary<string, string> assemblies)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if(AssemblyManager.IsAssembly(file.Name))
				{
					assemblies.Add(file.Name, file.FullName);
				}
			}
		}

		protected void CollectMainAssemblies(DirectoryInfo root, IDictionary<string, string> assemblies)
		{
			string managedPath = Path.Combine(root.FullName, ManagedName);
			if (Directory.Exists(managedPath))
			{
				DirectoryInfo managedDirectory = new DirectoryInfo(managedPath);
				CollectAssemblies(managedDirectory, assemblies);
			}
			else
			{
				string libPath = Path.Combine(root.FullName, LibName);
				if (Directory.Exists(libPath))
				{
					CollectAssemblies(root, assemblies);
					DirectoryInfo libDirectory = new DirectoryInfo(libPath);
					CollectAssemblies(libDirectory, assemblies);
				}
			}
		}

		private bool LoadEngineDependency(string path, string dependency)
		{
			string filePath = Path.Combine(path, dependency);
			if (FileUtils.Exists(filePath))
			{
				LoadDependency(dependency, filePath);
				return true;
			}

			string resourcePath = Path.Combine(path, ResourceName);
			filePath = Path.Combine(resourcePath, dependency);
			if (FileUtils.Exists(filePath))
			{
				LoadDependency(dependency, filePath);
				return true;
			}

			// really old versions contains file in this directory
			string unityPath = Path.Combine(path, UnityName);
			filePath = Path.Combine(unityPath, dependency);
			if (FileUtils.Exists(filePath))
			{
				LoadDependency(dependency, filePath);
				return true;
			}
			return false;
		}

		protected void AddFile(IDictionary<string, string> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Game file '{name}' has been found");
		}

		protected void AddAssetBundle(IDictionary<string, string> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Asset bundle '{name}' has been found");
		}

		private void LoadDependency(string name, string path)
		{
			m_fileCollection.Load(path);
			Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Dependency '{name}' has been loaded");
		}

		public abstract string Name { get; }
		public abstract IReadOnlyList<string> DataPathes { get; }

		public abstract IReadOnlyDictionary<string, string> Files { get; }
		public abstract IReadOnlyDictionary<string, string> Assemblies { get; }

		protected static readonly Regex s_levelName = new Regex($@"^level[0-9][0-9]*({FileMultiStream.MultifileRegPostfix}0)?$", RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected const string ManagedName = "Managed";
		protected const string LibName = "lib";
		protected const string ResourceName = "Resources";
		protected const string UnityName = "unity";
		protected const string StreamingName = "StreamingAssets";

		protected const string MainDataName = "mainData";
		protected const string GlobalGameManagerName = "globalgamemanagers";
		protected const string LevelPrefix = "level";

		protected const string AssetBundleExtension = ".unity3d";

		protected readonly FileCollection m_fileCollection;
	}
}
