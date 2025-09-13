using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class AndroidGameStructure : PlatformGameStructure
{
	public AndroidGameStructure(string rootPath, FileSystem fileSystem) : this(rootPath, string.Empty, fileSystem)
	{
	}

	public AndroidGameStructure(string rootPath, string? obbPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		string apkDataPath = FileSystem.Path.Join(rootPath, AssetName, BinName, DataFolderName);
		if (!FileSystem.Directory.Exists(apkDataPath))
		{
			throw new Exception($"Data directory hasn't been found");
		}
		List<string> dataPaths = [apkDataPath];

		GameDataPath = apkDataPath;
		StreamingAssetsPath = null;
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(GameDataPath, ManagedName);
		LibPath = FileSystem.Path.Join(RootPath, LibName);
		Il2CppGameAssemblyPath = GetIl2CppGameAssemblyPath(LibPath);
		Il2CppMetaDataPath = FileSystem.Path.Join(ManagedPath, MetadataName, DefaultGlobalMetadataName);
		UnityPlayerPath = null;
		Version = GetUnityVersionFromDataDirectory(GameDataPath);

		if (HasIl2CppFiles())
		{
			Backend = Assembly.ScriptingBackend.IL2Cpp;
		}
		else if (IsMono(ManagedPath))
		{
			Backend = Assembly.ScriptingBackend.Mono;
		}
		else
		{
			Backend = Assembly.ScriptingBackend.Unknown;
		}

		if (obbPath != null)
		{
			m_obbRoot = obbPath;
			if (!FileSystem.Directory.Exists(obbPath))
			{
				throw new Exception($"Obb directory '{obbPath}' doesn't exist");
			}

			string obbDataPath = FileSystem.Path.Join(obbPath, AssetName, BinName, DataFolderName);
			if (!FileSystem.Directory.Exists(obbDataPath))
			{
				throw new Exception($"Obb data directory '{obbDataPath}' wasn't found");
			}
			dataPaths.Add(obbDataPath);
		}
		DataPaths = dataPaths.ToArray();
	}

	public override void CollectFiles(bool skipStreamingAssets)
	{
		base.CollectFiles(skipStreamingAssets);
		CollectApkAssetBundles(Files);
	}

	public static bool IsAndroidStructure(string path, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(path))
		{
			return false;
		}

		int match = GetRootAndroidDirectoryMatch(path, fileSystem);
		if (match <= 8)
		{
			return false;
		}

		string dataPath = fileSystem.Path.Join(path, AssetName, BinName, DataFolderName);

		return fileSystem.Directory.Exists(dataPath);
	}

	public static bool IsAndroidObbStructure(string path, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(path))
		{
			return false;
		}

		int match = GetRootAndroidDirectoryMatch(path, fileSystem);
		if (match != 8)
		{
			return false;
		}

		string dataPath = fileSystem.Path.Join(path, AssetName, BinName, DataFolderName);

		return fileSystem.Directory.Exists(dataPath);
	}

	private static int GetRootAndroidDirectoryMatch(string directory, FileSystem fileSystem)
	{
		int matches = 0;
		foreach (string subDirectory in fileSystem.Directory.EnumerateDirectories(directory))
		{
			switch (fileSystem.Path.GetFileName(subDirectory))
			{
				case AssetName:
					matches |= 8;
					break;

				case MetaName:
					matches |= 4;
					break;

				case LibName:
					matches |= 2;
					break;
			}
		}
		return matches;
	}

	private void CollectApkAssetBundles(List<KeyValuePair<string, string>> files)
	{
		string assetPath = FileSystem.Path.Join(RootPath, AssetName);

		CollectAssetBundles(assetPath, files);
		foreach (string subDirectory in FileSystem.Directory.EnumerateDirectories(assetPath))
		{
			if (FileSystem.Path.GetFileName(subDirectory) == BinName)
			{
				continue;
			}
			CollectAssetBundlesRecursively(subDirectory, files);
		}
	}

	private string? GetIl2CppGameAssemblyPath(string libDirectory)
	{
		if (string.IsNullOrEmpty(libDirectory) || !FileSystem.Directory.Exists(libDirectory))
		{
			return null;
		}

		return FileSystem.Directory.EnumerateFiles(libDirectory, Il2CppGameAssemblyName, SearchOption.AllDirectories).FirstOrDefault();
	}

	private string? GetAndroidUnityAssemblyPath(string libDirectory)
	{
		if (string.IsNullOrEmpty(libDirectory) || !FileSystem.Directory.Exists(libDirectory))
		{
			return null;
		}

		return FileSystem.Directory.EnumerateFiles(libDirectory, AndroidUnityAssemblyName, SearchOption.AllDirectories).FirstOrDefault();
	}

	private bool IsMono(string managedDirectory)
	{
		if (string.IsNullOrEmpty(managedDirectory) || !FileSystem.Directory.Exists(managedDirectory))
		{
			return false;
		}

		return FileSystem.Directory.EnumerateFiles(managedDirectory, "*.dll").Any();
	}

	public string LibPath { get; private set; }

	private const string AssetName = "assets";
	private const string MetaName = "META-INF";
	private const string BinName = "bin";
	private const string Il2CppGameAssemblyName = "libil2cpp.so";
	private const string AndroidUnityAssemblyName = "libunity.so";

	private readonly string? m_obbRoot;
}
