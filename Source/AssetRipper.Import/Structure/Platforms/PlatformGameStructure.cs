using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Streams;
using System.Text.RegularExpressions;

namespace AssetRipper.Import.Structure.Platforms;

public abstract partial class PlatformGameStructure
{
	public FileSystem FileSystem { get; }
	public string? Name { get; protected set; }
	public string? RootPath { get; }
	public string? GameDataPath { get; protected set; }
	public string? StreamingAssetsPath { get; protected set; }
	public string? ResourcesPath { get; protected set; }
	public ScriptingBackend Backend { get; protected set; } = ScriptingBackend.Unknown;
	public string? ManagedPath { get; protected set; }
	public string? Il2CppGameAssemblyPath { get; protected set; }
	public string? Il2CppMetaDataPath { get; protected set; }
	public string? UnityPlayerPath { get; protected set; }
	public UnityVersion? Version { get; protected set; }

	public IReadOnlyList<string> DataPaths { get; protected set; } = [];

	/// <summary>Name : FullName</summary>
	public List<KeyValuePair<string, string>> Files { get; } = [];
	/// <summary>AssemblyName : AssemblyPath</summary>
	public Dictionary<string, string> Assemblies { get; } = [];

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

	public PlatformGameStructure(FileSystem fileSystem)
	{
		ArgumentNullException.ThrowIfNull(fileSystem);
		FileSystem = fileSystem;
	}

	public PlatformGameStructure(string rootPath, FileSystem fileSystem) : this(fileSystem)
	{
		ArgumentException.ThrowIfNullOrEmpty(rootPath);
		if (!FileSystem.Directory.Exists(rootPath))
		{
			throw new DirectoryNotFoundException($"Root directory '{rootPath}' doesn't exist");
		}
		RootPath = rootPath;
	}

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
			string filePath = FileSystem.Path.Join(dataPath, dependency);
			if (MultiFileStream.Exists(filePath, FileSystem))
			{
				return filePath;
			}

			if (SpecialFileNames.IsDefaultResource(dependency))
			{
				return FindEngineDependency(dataPath, SpecialFileNames.DefaultResourceName1) ??
					FindEngineDependency(dataPath, SpecialFileNames.DefaultResourceName2);
			}
			else if (SpecialFileNames.IsBuiltinExtra(dependency))
			{
				return FindEngineDependency(dataPath, SpecialFileNames.BuiltinExtraName1) ??
					FindEngineDependency(dataPath, SpecialFileNames.BuiltinExtraName2);
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
			string path = FileSystem.Path.Join(dataPath, resource);
			if (MultiFileStream.Exists(path, FileSystem))
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
			CollectGameFiles(dataPath, Files);
		}
		CollectMainAssemblies();
		if (!skipStreamingAssets)
		{
			CollectStreamingAssets();
		}
	}

	protected void CollectGameFiles(string root, List<KeyValuePair<string, string>> files)
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
	protected void CollectCompressedGameFiles(string root, List<KeyValuePair<string, string>> files)
	{
		string dataBundlePath = FileSystem.Path.Join(root, DataBundleName);
		if (MultiFileStream.Exists(dataBundlePath, FileSystem))
		{
			AddAssetBundle(files, DataBundleName, dataBundlePath);
		}

		string dataPackBundlePath = FileSystem.Path.Join(root, DataPackBundleName);
		if (MultiFileStream.Exists(dataPackBundlePath, FileSystem))
		{
			AddAssetBundle(files, DataPackBundleName, dataPackBundlePath);
		}
	}

	/// <summary>
	/// Collects global game managers and all the level files
	/// </summary>
	protected void CollectSerializedGameFiles(string root, List<KeyValuePair<string, string>> files)
	{
		string filePath = FileSystem.Path.Join(root, GlobalGameManagersName);
		if (MultiFileStream.Exists(filePath, FileSystem))
		{
			AddFile(files, GlobalGameManagersName, filePath);
		}
		else
		{
			filePath = FileSystem.Path.Join(root, MainDataName);
			if (MultiFileStream.Exists(filePath, FileSystem))
			{
				AddFile(files, MainDataName, filePath);
			}
		}

		foreach (string levelFile in FileSystem.Directory.EnumerateFiles(root))
		{
			string name = FileSystem.Path.GetFileName(levelFile);
			if (s_levelTemplate.IsMatch(name))
			{
				string levelName = MultiFileStream.GetFileName(name);
				AddFile(files, levelName, levelFile);
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
		if (FileSystem.Directory.Exists(StreamingAssetsPath))
		{
			CollectAssetBundlesRecursively(StreamingAssetsPath, Files);
		}
	}

	/// <summary>
	/// Collect asset bundles only from this directory
	/// </summary>
	protected void CollectAssetBundles(string root, List<KeyValuePair<string, string>> files)
	{
		foreach (string file in FileSystem.Directory.EnumerateFiles(root))
		{
			//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
			if (BundleHeader.IsBundleHeader(file, FileSystem))
			{
				string name = FileSystem.Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
				AddAssetBundle(files, name, file);
			}
		}
	}

	/// <summary>
	/// Collect asset bundles from this directory and all subdirectories
	/// </summary>
	protected void CollectAssetBundlesRecursively(string root, List<KeyValuePair<string, string>> files)
	{
		CollectAssetBundles(root, files);
		foreach (string directory in FileSystem.Directory.EnumerateDirectories(root))
		{
			CollectAssetBundlesRecursively(directory, files);
		}
	}

	protected void CollectAssemblies(string root, Dictionary<string, string> assemblies)
	{
		foreach (string file in FileSystem.Directory.EnumerateFiles(root))
		{
			string name = FileSystem.Path.GetFileName(file);
			if (MonoManager.IsMonoAssembly(name))
			{
				assemblies.TryAdd(name, file);
			}
		}
	}

	protected void CollectMainAssemblies()
	{
		if (Backend != ScriptingBackend.Mono)
		{
			return;//Only needed for Mono
		}
		else if (!string.IsNullOrWhiteSpace(ManagedPath) && FileSystem.Directory.Exists(ManagedPath))
		{
			CollectAssemblies(ManagedPath, Assemblies);
		}
		else if (!string.IsNullOrEmpty(GameDataPath))
		{
			string libPath = FileSystem.Path.Join(FileSystem.Path.GetFullPath(GameDataPath), LibName);
			if (FileSystem.Directory.Exists(libPath))
			{
				CollectAssemblies(GameDataPath, Assemblies);
				CollectAssemblies(libPath, Assemblies);
			}
		}
	}

	private string? FindEngineDependency(string path, string dependency)
	{
		string filePath = FileSystem.Path.Join(path, dependency);
		if (FileSystem.File.Exists(filePath))
		{
			return filePath;
		}

		string resourcePath = FileSystem.Path.Join(path, ResourcesName);
		filePath = FileSystem.Path.Join(resourcePath, dependency);
		if (FileSystem.File.Exists(filePath))
		{
			return filePath;
		}

		// really old versions contains file in this directory
		string unityPath = FileSystem.Path.Join(path, UnityName);
		filePath = FileSystem.Path.Join(unityPath, dependency);
		if (FileSystem.File.Exists(filePath))
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

	protected UnityVersion GetUnityVersionFromSerializedFile(string filePath)
	{
		return SerializedFile.FromFile(filePath, FileSystem).Version;
	}

	protected UnityVersion GetUnityVersionFromBundleFile(string filePath)
	{
		string version = new FileStreamBundleFile(filePath, FileSystem).Header.UnityWebMinimumRevision ?? "";
		return UnityVersion.Parse(version);
	}

	protected UnityVersion? GetUnityVersionFromDataDirectory(string dataDirectoryPath)
	{
		string globalGameManagersPath = FileSystem.Path.Join(dataDirectoryPath, GlobalGameManagersName);
		if (FileSystem.File.Exists(globalGameManagersPath))
		{
			return GetUnityVersionFromSerializedFile(globalGameManagersPath);
		}
		string dataBundlePath = FileSystem.Path.Join(dataDirectoryPath, DataBundleName);
		if (FileSystem.File.Exists(dataBundlePath))
		{
			return GetUnityVersionFromBundleFile(dataBundlePath);
		}
		return null;
	}

	protected bool HasMonoAssemblies(string managedDirectory)
	{
		if (string.IsNullOrEmpty(managedDirectory) || !FileSystem.Directory.Exists(managedDirectory))
		{
			return false;
		}

		return FileSystem.Directory.GetFiles(managedDirectory, "*.dll").Length > 0;
	}

	protected bool HasIl2CppFiles()
	{
		return Il2CppGameAssemblyPath != null &&
			Il2CppMetaDataPath != null &&
			FileSystem.File.Exists(Il2CppGameAssemblyPath) &&
			FileSystem.File.Exists(Il2CppMetaDataPath);
	}

	[GeneratedRegex("^level(0|[1-9][0-9]*)(\\.split0)?$", RegexOptions.Compiled)]
	private static partial Regex LevelTemplateRegex();

	[GeneratedRegex("^sharedassets[0-9]+\\.assets", RegexOptions.Compiled)]
	private static partial Regex SharedAssetTemplateRegex();
}
