using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class iOSGameStructure : PlatformGameStructure
{
	public iOSGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		if (!GetDataiOSDirectory(rootPath, FileSystem, out string? dataPath, out string? appPath, out string? name))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = name;
		GameDataPath = dataPath;
		StreamingAssetsPath = FileSystem.Path.Join(rootPath, iOSStreamingName);
		ResourcesPath = FileSystem.Path.Join(dataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(dataPath, ManagedName);
		UnityPlayerPath = null;
		Version = GetUnityVersionFromDataDirectory(GameDataPath);
		Il2CppGameAssemblyPath = FileSystem.Path.Join(appPath, name);
		Il2CppMetaDataPath = FileSystem.Path.Join(ManagedPath, MetadataName, DefaultGlobalMetadataName);

		if (HasIl2CppFiles())
		{
			Backend = ScriptingBackend.IL2Cpp;
		}
		else if (HasMonoAssemblies(ManagedPath))
		{
			Backend = ScriptingBackend.Mono;
		}
		else
		{
			Backend = ScriptingBackend.Unknown;
		}

		DataPaths = [GameDataPath];
	}

	public static bool Exists(string path, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(path))
		{
			return false;
		}

		return GetDataiOSDirectory(path, fileSystem, out _, out _, out _);
	}

	private static bool GetDataiOSDirectory(string rootDirectory, FileSystem fileSystem, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? appPath, [NotNullWhen(true)] out string? appName)
	{
		string payloadPath = fileSystem.Path.Join(rootDirectory, PayloadName);
		if (!fileSystem.Directory.Exists(payloadPath))
		{
			dataPath = null;
			appPath = null;
			appName = null;
			return false;
		}

		foreach (string directory in fileSystem.Directory.EnumerateDirectories(payloadPath))
		{
			string name = fileSystem.Path.GetFileName(directory);
			if (name.EndsWith(AppExtension, StringComparison.Ordinal))
			{
				appPath = directory;
				appName = name[..^AppExtension.Length];
				dataPath = fileSystem.Path.Join(directory, DataFolderName);
				if (fileSystem.Directory.Exists(dataPath))
				{
					return true;
				}
			}
		}

		dataPath = null;
		appPath = null;
		appName = null;
		return false;
	}

	private void CollectiOSStreamingAssets(string root, List<KeyValuePair<string, string>> files)
	{
		string streamingPath = FileSystem.Path.Join(root, iOSStreamingName);
		if (FileSystem.Directory.Exists(streamingPath))
		{
			CollectAssetBundlesRecursively(streamingPath, files);
		}
	}


	private const string iOSStreamingName = "Raw";

	private const string PayloadName = "Payload";
	private const string AppExtension = ".app";
}
