using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Streams.MultiFile;
using AssetRipper.IO.Files.Utils;
using System.Text.RegularExpressions;

namespace AssetRipper.Import.Structure.Platforms
{
	public abstract partial class PlatformGameStructure
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
		public UnityVersion? Version { get; protected set; }

		public IReadOnlyList<string> DataPaths { get; protected set; } = Array.Empty<string>();

		/// <summary>Name : FullName</summary>
		public List<KeyValuePair<string, string>> Files { get; } = [];
		/// <summary>AssemblyName : AssemblyPath</summary>
		public Dictionary<string, string> Assemblies { get; } = [];

		protected DirectoryInfo m_root { get; set; }

		protected static readonly Regex s_levelTemplate = LevelTemplateRegex();
		protected static readonly Regex s_sharedAssetTemplate = SharedAssetTemplateRegex();

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
		protected const string DataPackBundleName = DataName + "pack" + AssetBundleExtension;
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
			string? dependencyPath = Files.FirstOrDefault(t => t.Key == dependency).Value;
			if (!string.IsNullOrEmpty(dependencyPath))
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
				CollectStreamingAssets();
			}
		}

		protected static void CollectGameFiles(DirectoryInfo root, List<KeyValuePair<string, string>> files)
		{
			Logger.Info(LogCategory.Import, "Collecting game files...");
			CollectCompressedGameFiles(root, files);
			CollectSerializedGameFiles(root, files);
		}

		/// <summary>
		/// Finds data.unity3d and datapack.unity3d when Lz4 compressed
		/// 
		/// Accoding to comments in Unity source file in the function at
		/// PlatformDependent/AndroidPlayer/Source/ApkFile.cpp:268,
		/// the datapack asset is only present if Gradle built an AAB with Unity
		/// data asset pack inside and then bundletool converted AAB into universal APK.
		/// </summary>
		protected static void CollectCompressedGameFiles(DirectoryInfo root, List<KeyValuePair<string, string>> files)
		{
			string dataBundlePath = Path.Combine(root.FullName, DataBundleName);
			if (MultiFileStream.Exists(dataBundlePath))
			{
				AddAssetBundle(files, DataBundleName, dataBundlePath);
			}
			
			string dataPackBundlePath = Path.Combine(root.FullName, DataPackBundleName);
			if (MultiFileStream.Exists(dataPackBundlePath))
			{
				AddAssetBundle(files, DataPackBundleName, dataPackBundlePath);
			}
		}
		
		/// <summary>
		/// Collects global game managers and all the level files
		/// </summary>
		protected static void CollectSerializedGameFiles(DirectoryInfo root, List<KeyValuePair<string, string>> files)
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
		protected void CollectStreamingAssets()
		{
			if (string.IsNullOrWhiteSpace(StreamingAssetsPath))
			{
				return;
			}

			Logger.Info(LogCategory.Import, "Collecting Streaming Assets...");
			DirectoryInfo streamingDirectory = new DirectoryInfo(StreamingAssetsPath);
			if (streamingDirectory.Exists)
			{
				CollectAssetBundlesRecursively(streamingDirectory, Files);
			}
		}

		/// <summary>
		/// Collect asset bundles only from this directory
		/// </summary>
		protected static void CollectAssetBundles(DirectoryInfo root, List<KeyValuePair<string, string>> files)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
				if (BundleHeader.IsBundleHeader(file.FullName))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
					AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		/// <summary>
		/// Collect asset bundles from this directory and all subdirectories
		/// </summary>
		protected static void CollectAssetBundlesRecursively(DirectoryInfo root, List<KeyValuePair<string, string>> files)
		{
			CollectAssetBundles(root, files);
			foreach (DirectoryInfo directory in root.EnumerateDirectories())
			{
				CollectAssetBundlesRecursively(directory, files);
			}
		}

		protected static void CollectAssemblies(DirectoryInfo root, Dictionary<string, string> assemblies)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if (MonoManager.IsMonoAssembly(file.Name))
				{
					assemblies.TryAdd(file.Name, file.FullName);
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

		private static string? FindEngineDependency(string path, string dependency)
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
		protected static void AddFile(List<KeyValuePair<string, string>> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Info(LogCategory.Import, $"Game file '{name}' has been found");
		}

		protected static void AddAssetBundle(List<KeyValuePair<string, string>> files, string name, string path)
		{
			files.Add(name, path);
			Logger.Info(LogCategory.Import, $"Asset bundle '{name}' has been found");
		}

		protected static UnityVersion GetUnityVersionFromSerializedFile(string filePath)
		{
			return SerializedFile.FromFile(filePath).Version;
		}

		protected static UnityVersion GetUnityVersionFromBundleFile(string filePath)
		{
			string version = new FileStreamBundleFile(filePath).Header.UnityWebMinimumRevision ?? "";
			return UnityVersion.Parse(version);
		}

		protected static UnityVersion? GetUnityVersionFromDataDirectory(string dataDirectoryPath)
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

		[GeneratedRegex("^level(0|[1-9][0-9]*)(\\.split0)?$", RegexOptions.Compiled)]
		private static partial Regex LevelTemplateRegex();

		[GeneratedRegex("^sharedassets[0-9]+\\.assets", RegexOptions.Compiled)]
		private static partial Regex SharedAssetTemplateRegex();
	}
}
