using AssetRipper.Core.IO.MultiFile;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Utils;
using AssetRipper.Core.Structure.Assembly;
using AssetRipper.Core.Structure.Assembly.Managers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	public abstract class PlatformGameStructure
	{
		public string? Name { get; protected set; }
		public string? RootPath { get; protected set; }
		public string? GameDataPath { get; protected set; }
		public string? StreamingAssetsPath { get; protected set; }
		public string? ResourcesPath { get; protected set; }
		public ScriptingBackend Backend { get; protected set; } = ScriptingBackend.Unknown;
		public string? ManagedPath { get; protected set; }
		public string? Il2CppGameAssemblyPath { get; protected set; }
		public string? Il2CppMetaDataPath { get; protected set; }
		public string? UnityPlayerPath { get; protected set; }
		public int[]? UnityVersion { get; protected set; }

		public IReadOnlyList<string> DataPaths { get; protected set; }

		/// <summary>Name : FullName</summary>
		public Dictionary<string, string> Files { get; } = new Dictionary<string, string>();
		/// <summary>AssemblyName : AssemblyPath</summary>
		public Dictionary<string, string> Assemblies { get; } = new Dictionary<string, string>();

		protected DirectoryInfo m_root { get; set; }

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
		protected const string DataBundleName = DataName + AssetBundleExtension;
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
			if (fileName == MainDataName ||
				fileName == GlobalGameManagersName ||
				fileName == GlobalGameManagerAssetsName ||
				fileName == ResourcesAssetsName ||
				s_levelTemplate.IsMatch(fileName) ||
				s_sharedAssetTemplate.IsMatch(fileName))
			{
				return true;
			}
			return false;
		}

		/// <summary>Attempts to find the path for the dependency with that name.</summary>
		public string? RequestDependency(string dependency)
		{
			if (Files.TryGetValue(dependency, out string? dependencyPath))
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

		public string? RequestAssembly(string assembly)
		{
			string assemblyName = $"{assembly}{MonoManager.AssemblyExtension}";
			if (Assemblies.TryGetValue(assemblyName, out string? assemblyPath))
			{
				return assemblyPath;
			}
			return null;
		}

		public string? RequestResource(string resource)
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

		public virtual void CollectFiles(bool skipStreamingAssets)
		{
			if (this is MixedGameStructure)
			{
				return;
			}

			foreach (string dataPath in DataPaths)
			{
				DirectoryInfo dataDirectory = new DirectoryInfo(dataPath);
				CollectGameFiles(dataDirectory, Files);
			}
			CollectMainAssemblies();
			if (!skipStreamingAssets)
			{
				CollectStreamingAssets(Files);
			}
		}

		protected void CollectGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			Logger.Info(LogCategory.Import, "Collecting game files...");
			CollectCompressedGameFiles(root, files);
			CollectSerializedGameFiles(root, files);
		}

		/// <summary>
		/// Finds data.unity3d when Lz4 compressed
		/// </summary>
		protected void CollectCompressedGameFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			string dataBundlePath = Path.Combine(root.FullName, DataBundleName);
			if (MultiFileStream.Exists(dataBundlePath))
			{
				AddAssetBundle(files, DataBundleName, dataBundlePath);
			}
		}

		/// <summary>
		/// Collects global game managers and all the level files
		/// </summary>
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

		/// <summary>
		/// Collect bundles from the Streaming Assets folder
		/// </summary>
		protected void CollectStreamingAssets(IDictionary<string, string> files)
		{
			if (string.IsNullOrWhiteSpace(StreamingAssetsPath))
			{
				return;
			}

			Logger.Info(LogCategory.Import, "Collecting Streaming Assets...");
			DirectoryInfo streamingDirectory = new DirectoryInfo(StreamingAssetsPath);
			if (streamingDirectory.Exists)
			{
				CollectAssetBundlesRecursively(streamingDirectory, files);
			}
		}

		/// <summary>
		/// Collect asset bundles only from this directory
		/// </summary>
		protected void CollectAssetBundles(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
				if (BundleFile.IsBundleFile(file.FullName))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
					AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		/// <summary>
		/// Collect asset bundles from this directory and all subdirectories
		/// </summary>
		protected void CollectAssetBundlesRecursively(DirectoryInfo root, IDictionary<string, string> files)
		{
			CollectAssetBundles(root, files);
			foreach (DirectoryInfo directory in root.EnumerateDirectories())
			{
				CollectAssetBundlesRecursively(directory, files);
			}
		}

		protected static void CollectAssemblies(DirectoryInfo root, IDictionary<string, string> assemblies)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if (MonoManager.IsMonoAssembly(file.Name))
				{
					assemblies.Add(file.Name, file.FullName);
				}
			}
		}

		protected void CollectMainAssemblies()
		{
			if (Backend != ScriptingBackend.Mono)
			{
				return;//Only needed for Mono
			}
			else if (!string.IsNullOrWhiteSpace(ManagedPath) && Directory.Exists(ManagedPath))
			{
				DirectoryInfo managedDirectory = new DirectoryInfo(ManagedPath);
				CollectAssemblies(managedDirectory, Assemblies);
			}
			else if (!string.IsNullOrEmpty(GameDataPath))
			{
				string libPath = Path.Combine(Path.GetFullPath(GameDataPath), LibName);
				if (Directory.Exists(libPath))
				{
					CollectAssemblies(new DirectoryInfo(GameDataPath), Assemblies);
					DirectoryInfo libDirectory = new DirectoryInfo(libPath);
					CollectAssemblies(libDirectory, Assemblies);
				}
			}
		}

		private string? FindEngineDependency(string path, string dependency)
		{
			string filePath = Path.Combine(path, dependency);
			if (File.Exists(filePath))
			{
				return filePath;
			}

			string resourcePath = Path.Combine(path, ResourcesName);
			filePath = Path.Combine(resourcePath, dependency);
			if (File.Exists(filePath))
			{
				return filePath;
			}

			// really old versions contains file in this directory
			string unityPath = Path.Combine(path, UnityName);
			filePath = Path.Combine(unityPath, dependency);
			if (File.Exists(filePath))
			{
				return filePath;
			}
			return null;
		}

		/// <summary>
		/// Add game file
		/// </summary>
		protected void AddFile(IDictionary<string, string> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Info(LogCategory.Import, $"Game file '{name}' has been found");
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
			Logger.Info(LogCategory.Import, $"Asset bundle '{name}' has been found");
		}

		protected static int[] GetUnityVersionFromSerializedFile(string filePath)
		{
			return ToArray(SerializedFile.LoadScheme(filePath).Metadata.UnityVersion);
		}

		protected static int[] GetUnityVersionFromBundleFile(string filePath)
		{
			return ToArray(BundleFile.LoadScheme(filePath).Header.UnityWebMinimumRevision);
		}

		private static int[] ToArray(UnityVersion version)
		{
			return new int[] { version.Major, version.Minor, version.Build };
		}

		protected static int[]? GetUnityVersionFromDataDirectory(string dataDirectoryPath)
		{
			string globalGameManagersPath = Path.Combine(dataDirectoryPath, GlobalGameManagersName);
			if (File.Exists(globalGameManagersPath))
			{
				return GetUnityVersionFromSerializedFile(globalGameManagersPath);
			}
			string dataBundlePath = Path.Combine(dataDirectoryPath, DataBundleName);
			if (File.Exists(dataBundlePath))
			{
				return GetUnityVersionFromBundleFile(dataBundlePath);
			}
			return null;
		}

		protected static bool HasMonoAssemblies(string managedDirectory)
		{
			if (string.IsNullOrEmpty(managedDirectory) || !Directory.Exists(managedDirectory))
			{
				return false;
			}

			return Directory.GetFiles(managedDirectory, "*.dll").Length > 0;
		}

		protected bool HasIl2CppFiles()
		{
			return Il2CppGameAssemblyPath != null &&
				Il2CppMetaDataPath != null &&
				File.Exists(Il2CppGameAssemblyPath) &&
				File.Exists(Il2CppMetaDataPath);
		}
	}
}
