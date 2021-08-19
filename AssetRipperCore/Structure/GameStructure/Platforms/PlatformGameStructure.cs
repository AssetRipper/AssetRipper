using AssetRipper.Core.IO.FileReading;
using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	public abstract class PlatformGameStructure
	{
		public string Name { get; protected set; }
		public string RootPath { get; protected set; }
		public string GameDataPath { get; protected set; }
		public ScriptingBackend Backend { get; protected set; } = ScriptingBackend.Unknown;
		public abstract PlatformType Platform { get; }
		public string ManagedPath { get; protected set; }
		public string Il2CppGameAssemblyPath { get; protected set; }
		public string Il2CppMetaDataPath { get; protected set; }
		public string UnityPlayerPath { get; protected set; }
		public int[] UnityVersion { get; protected set; }
		
		public IReadOnlyList<string> DataPaths { get; protected set; }

		/// <summary>Name : FullName</summary>
		public Dictionary<string, string> Files { get; } = new Dictionary<string, string>();
		/// <summary>AssemblyName : AssemblyPath</summary>
		public Dictionary<string, string> Assemblies { get; } = new Dictionary<string, string>();

		protected static readonly Regex s_levelTemplate = new Regex($@"^level(0|[1-9][0-9]*)({MultiFileStream.MultifileRegPostfix}0)?$", RegexOptions.Compiled);
		protected static readonly Regex s_sharedAssetTemplate = new Regex(@"^sharedassets[0-9]+\.assets", RegexOptions.Compiled);

		protected const string DataFolderName = "Data";
		protected const string ManagedName = "Managed";
		protected const string LibName = "lib";
		protected const string ResourcesName = "Resources";
		protected const string UnityName = "unity";
		protected const string StreamingName = "StreamingAssets";
		protected const string MetadataName = "Metadata";
		protected const string DefaultUnityPlayerName = "UnityPlayer.dll";
		protected const string DefaultGameAssemblyName = "GameAssembly.dll";
		protected const string DefaultGlobalMetadataName = "global-metadata.dat";

		protected const string DataName = "data";
		protected const string MainDataName = "mainData";
		protected const string GlobalGameManagersName = "globalgamemanagers";
		protected const string GlobalGameManagerAssetsName = "globalgamemanagers.assets";
		protected const string ResourcesAssetsName = "resources.assets";
		protected const string LevelPrefix = "level";

		protected const string AssetBundleExtension = ".unity3d";
		protected const string AlternateBundleExtension = ".bundle";
		protected const string Lz4BundleName = DataName + AssetBundleExtension;

		public static bool IsPrimaryEngineFile(string fileName)
		{
			if (fileName == MainDataName)
			{
				return true;
			}
			if (fileName == GlobalGameManagersName || fileName == GlobalGameManagerAssetsName)
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

		/// <summary>Attempts to find the path for the dependency with that name.</summary>
		public string RequestDependency(string dependency)
		{
			if (Files.TryGetValue(dependency, out string dependencyPath))
			{
				return dependencyPath;
			}

			foreach (string dataPath in DataPaths)
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
			foreach (string dataPath in DataPaths)
			{
				string path = Path.Combine(dataPath, resource);
				if (MultiFileStream.Exists(path))
				{
					return path;
				}
			}
			return null;
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
			string filePath = Path.Combine(root.FullName, GlobalGameManagersName);
			if (MultiFileStream.Exists(filePath))
			{
				AddFile(files, GlobalGameManagersName, filePath);
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
				if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
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
				if (MonoManager.IsMonoAssembly(file.Name))
				{
					assemblies.Add(file.Name, file.FullName);
				}
			}
		}

		protected void CollectMainAssemblies(DirectoryInfo dataDirectory, IDictionary<string, string> assemblies)
		{
			string managedPath = ManagedPath ?? Path.Combine(dataDirectory.FullName, ManagedName);
			if (Backend == ScriptingBackend.Il2Cpp)
			{
				return;//If Il2Cpp, don't look for any other assemblies
			}
			else if (Directory.Exists(ManagedPath))
			{
				DirectoryInfo managedDirectory = new DirectoryInfo(ManagedPath);
				CollectAssemblies(managedDirectory, assemblies);
			}
			else
			{
				string libPath = Path.Combine(dataDirectory.FullName, LibName);
				if (Directory.Exists(libPath))
				{
					CollectAssemblies(dataDirectory, assemblies);
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

		protected static int[] GetUnityVersionFromSerializedFile(string filePath)
		{
			string unityVersion = (new AssetRipper.Core.SerializedFiles.SerializedFile(new FileReader(filePath), null)).unityVersion;
			return AssetRipper.Core.Parser.Files.UnityVersion.Parse(unityVersion).ToArray();
		}

		protected static int[] GetUnityVersionFromBundleFile(string filePath)
		{
			string unityVersion = (new AssetRipper.Core.Reading.BundleFile(new FileReader(filePath))).m_Header.unityRevision;
			return AssetRipper.Core.Parser.Files.UnityVersion.Parse(unityVersion).ToArray();
		}

		protected static bool HasMonoAssemblies(string managedDirectory)
		{
			if (string.IsNullOrEmpty(managedDirectory) || !Directory.Exists(managedDirectory)) return false;

			return Directory.GetFiles(managedDirectory, "*.dll").Length > 0;
		}

		protected bool HasIl2CppFiles()
		{
			return Il2CppGameAssemblyPath != null &&
				Il2CppMetaDataPath != null &&
				FileUtils.Exists(Il2CppGameAssemblyPath) &&
				FileUtils.Exists(Il2CppMetaDataPath);
		}
	}
}
