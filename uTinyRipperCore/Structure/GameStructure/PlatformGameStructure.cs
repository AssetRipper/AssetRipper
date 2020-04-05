using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using uTinyRipper.Game;
using uTinyRipper.Game.Assembly;
using uTinyRipper.Game.Assembly.Mono;

namespace uTinyRipper
{
	public abstract class PlatformGameStructure
	{
		public static bool IsPrimaryEngineFile(string fileName)
		{
			if (fileName == MainDataName)
			{
				return true;
			}
			if (fileName == GlobalGameManagerName || fileName == GlobalGameManagerAssetsName)
			{
				return true;
			}
			if (fileName == ResourcesAssetsName)
			{
				return true;
			}
			if (s_levelTemplate.IsMatch(fileName))
			{
				return true;
			}
			if (s_sharedAssetTemplate.IsMatch(fileName))
			{
				return true;
			}
			return false;
		}

		public string RequestDependency(string dependency)
		{
			if (Files.TryGetValue(dependency, out string dependencyPath))
			{
				return dependencyPath;
			}

			foreach (string dataPath in DataPathes)
			{
				string filePath = Path.Combine(dataPath, dependency);
				if (MultiFileStream.Exists(filePath))
				{
					return filePath;
				}

				if (FilenameUtils.IsDefaultResource(dependency))
				{
					return FindEngineDependency(dataPath, FilenameUtils.DefaultResourceName1) ??
						FindEngineDependency(dataPath, FilenameUtils.DefaultResourceName2);
				}
				else if (FilenameUtils.IsBuiltinExtra(dependency))
				{
					return FindEngineDependency(dataPath, FilenameUtils.BuiltinExtraName1) ??
						FindEngineDependency(dataPath, FilenameUtils.BuiltinExtraName2);
				}
			}
			return null;
		}

		public string RequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			if (Assemblies.TryGetValue(assemblyName, out string assemblyPath))
			{
				return assemblyPath;
			}
			return null;
		}

		public string RequestResource(string resource)
		{
			foreach (string dataPath in DataPathes)
			{
				string path = Path.Combine(dataPath, resource);
				if (MultiFileStream.Exists(path))
				{
					return path;
				}
			}
			return null;
		}

		public virtual ScriptingBackend GetScriptingBackend()
		{
			if (Assemblies.Count == 0)
			{
				return ScriptingBackend.Unknown;
			}

			string assemblyPath = Assemblies.First().Value;
			if (MonoManager.IsMonoAssembly(assemblyPath))
			{
				return ScriptingBackend.Mono;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		protected void CollectGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			CollectBundleGameFiles(root, files);
			CollectSerializedGameFiles(root, files);
		}

		protected void CollectBundleGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			const string DataBundleName = DataName + AssetBundleExtension;
			string dataBundlePath = Path.Combine(root.FullName, DataBundleName);
			if (MultiFileStream.Exists(dataBundlePath))
			{
				AddAssetBundle(files, DataBundleName, dataBundlePath);
			}
		}

		protected void CollectSerializedGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			string filePath = Path.Combine(root.FullName, GlobalGameManagerName);
			if (MultiFileStream.Exists(filePath))
			{
				AddFile(files, GlobalGameManagerName, filePath);
			}
			else
			{
				filePath = Path.Combine(root.FullName, MainDataName);
				if (MultiFileStream.Exists(filePath))
				{
					AddFile(files, MainDataName, filePath);
				}
			}

			foreach (FileInfo levelFile in root.EnumerateFiles())
			{
				if (s_levelTemplate.IsMatch(levelFile.Name))
				{
					string levelName = MultiFileStream.GetFileName(levelFile.Name);
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
				CollectAssetBundlesRecursivly(streamingDirectory, files);
			}
		}

		protected void CollectAssetBundles(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if (file.Extension == AssetBundleExtension)
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
				if (AssemblyManager.IsAssembly(file.Name))
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

		private string FindEngineDependency(string path, string dependency)
		{
			string filePath = Path.Combine(path, dependency);
			if (FileUtils.Exists(filePath))
			{
				return filePath;
			}

			string resourcePath = Path.Combine(path, ResourcesName);
			filePath = Path.Combine(resourcePath, dependency);
			if (FileUtils.Exists(filePath))
			{
				return filePath;
			}

			// really old versions contains file in this directory
			string unityPath = Path.Combine(path, UnityName);
			filePath = Path.Combine(unityPath, dependency);
			if (FileUtils.Exists(filePath))
			{
				return filePath;
			}
			return null;
		}

		protected void AddFile(IDictionary<string, string> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Log(LogType.Info, LogCategory.Import, $"Game file '{name}' has been found");
		}

		protected void AddAssetBundle(IDictionary<string, string> files, string name, string path)
		{
#warning TEMP HACK:
			int i = 0;
			string uniqueName = name;
			while (files.ContainsKey(uniqueName))
			{
				uniqueName = name + i++;
			}
			files.Add(uniqueName, path);
			Logger.Log(LogType.Info, LogCategory.Import, $"Asset bundle '{name}' has been found");
		}

		public abstract string Name { get; }
		public abstract IReadOnlyList<string> DataPathes { get; }

		public abstract IReadOnlyDictionary<string, string> Files { get; }
		public abstract IReadOnlyDictionary<string, string> Assemblies { get; }

		protected static readonly Regex s_levelTemplate = new Regex($@"^level(0|[1-9][0-9]*)({MultiFileStream.MultifileRegPostfix}0)?$", RegexOptions.Compiled);
		protected static readonly Regex s_sharedAssetTemplate = new Regex(@"^sharedassets[0-9]+\.assets", RegexOptions.Compiled);

		protected const string DataFolderName = "Data";
		protected const string ManagedName = "Managed";
		protected const string LibName = "lib";
		protected const string ResourcesName = "Resources";
		protected const string UnityName = "unity";
		protected const string StreamingName = "StreamingAssets";

		protected const string DataName = "data";
		protected const string MainDataName = "mainData";
		protected const string GlobalGameManagerName = "globalgamemanagers";
		protected const string GlobalGameManagerAssetsName = "globalgamemanagers.assets";
		protected const string ResourcesAssetsName = "resources.assets";
		protected const string LevelPrefix = "level";

		protected const string AssetBundleExtension = ".unity3d";
	}
}
