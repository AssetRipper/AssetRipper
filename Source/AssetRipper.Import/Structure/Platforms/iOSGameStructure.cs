using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class iOSGameStructure : PlatformGameStructure
{
	public iOSGameStructure(string rootPath)
	{
		if (string.IsNullOrEmpty(rootPath))
		{
			throw new ArgumentNullException(nameof(rootPath));
		}
		m_root = new DirectoryInfo(rootPath);
		if (!m_root.Exists)
		{
			throw new Exception($"Root directory '{rootPath}' doesn't exist");
		}

		if (!GetDataiOSDirectory(m_root, out string? dataPath, out string? appPath, out string? name))
		{
			throw new Exception($"Data directory wasn't found");
		}

		Name = name;
		RootPath = rootPath;
		GameDataPath = dataPath;
		StreamingAssetsPath = Path.Join(m_root.FullName, iOSStreamingName);
		ResourcesPath = Path.Join(dataPath, ResourcesName);
		ManagedPath = Path.Join(dataPath, ManagedName);
		UnityPlayerPath = null;
		Version = GetUnityVersionFromDataDirectory(GameDataPath);
		Il2CppGameAssemblyPath = Path.Join(appPath, name);
		Il2CppMetaDataPath = Path.Join(ManagedPath, MetadataName, DefaultGlobalMetadataName);

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

		DataPaths = new string[] { dataPath };
	}

	public static bool IsiOSStructure(string path)
	{
		DirectoryInfo root = new DirectoryInfo(path);
		if (!root.Exists)
		{
			return false;
		}

		return GetDataiOSDirectory(root, out _, out _, out _);
	}

	private static bool GetDataiOSDirectory(DirectoryInfo rootDirectory, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? appPath, [NotNullWhen(true)] out string? appName)
	{
		string payloadPath = Path.Join(rootDirectory.FullName, PayloadName);
		DirectoryInfo payloadDirectory = new DirectoryInfo(payloadPath);
		if (!payloadDirectory.Exists)
		{
			dataPath = null;
			appPath = null;
			appName = null;
			return false;
		}

		foreach (DirectoryInfo dinfo in payloadDirectory.EnumerateDirectories())
		{
			if (dinfo.Name.EndsWith(AppExtension, StringComparison.Ordinal))
			{
				appPath = dinfo.FullName;
				appName = dinfo.Name[..^AppExtension.Length];
				dataPath = Path.Join(dinfo.FullName, DataFolderName);
				if (Directory.Exists(dataPath))
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

	private static void CollectiOSStreamingAssets(DirectoryInfo root, List<KeyValuePair<string, string>> files)
	{
		string streamingPath = Path.Join(root.FullName, iOSStreamingName);
		DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
		if (streamingDirectory.Exists)
		{
			CollectAssetBundlesRecursively(root, files);
		}
	}


	private const string iOSStreamingName = "Raw";

	private const string PayloadName = "Payload";
	private const string AppExtension = ".app";
}
