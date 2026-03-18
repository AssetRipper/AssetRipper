using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Platforms;

internal sealed class WindowsPhoneGameStructure : PlatformGameStructure
{
	public WindowsPhoneGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		if (!GetDataDirectory(rootPath, FileSystem, out string? dataPath, out string? name))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = name;
		GameDataPath = dataPath;
		StreamingAssetsPath = FileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(RootPath);
		UnityPlayerPath = FileSystem.Path.Join(RootPath, DefaultUnityPlayerName);
		Version = GetUnityVersionFromDataDirectory(GameDataPath);
		Il2CppGameAssemblyPath = FileSystem.Path.Join(RootPath, DefaultGameAssemblyName);
		Il2CppMetaDataPath = FileSystem.Path.Join(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

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

		return GetDataDirectory(path, fileSystem, out _, out _);
	}

	private static bool GetDataDirectory(string rootDirectory, FileSystem fileSystem, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? appName)
	{
		string assetsPath = fileSystem.Path.Join(rootDirectory, "Assets");
		if (!fileSystem.Directory.Exists(assetsPath))
		{
			dataPath = null;
			appName = null;
			return false;
		}

		dataPath = fileSystem.Path.Join(rootDirectory, DataFolderName);
		if (!fileSystem.Directory.Exists(dataPath))
		{
			dataPath = null;
			appName = null;
			return false;
		}

		string? executablePath = null;
		foreach (string file in fileSystem.Directory.EnumerateFiles(rootDirectory, "*.exe"))
		{
			if (executablePath == null)
			{
				executablePath = file;
			}
			else
			{
				executablePath = null;
				break;
			}
		}
		if (executablePath != null)
		{
			appName = fileSystem.Path.GetFileNameWithoutExtension(executablePath);
			return true;
		}
		else
		{
			appName = null;
			return false;
		}
	}
}
