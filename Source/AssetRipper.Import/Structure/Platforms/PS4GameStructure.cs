using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class PS4GameStructure : PlatformGameStructure
{
	private const string PS4ExecutableName = "eboot.bin";
	private const string PS4DataFolderName = "Media";
	private const string ModulesName = "Modules";
	private const string PS4IL2CppGameAssemblyName = "Il2CppUserAssemblies.prx";

	private string ModulesPath { get; set; }

	public PS4GameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		if (!GetDataDirectory(rootPath, fileSystem, out string? dataPath))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = FileSystem.Path.GetFileName(rootPath);
		GameDataPath = dataPath;
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(GameDataPath, ManagedName);
		ModulesPath = FileSystem.Path.Join(GameDataPath, ModulesName);
		UnityPlayerPath = null;
		string globalGameManagersPath = FileSystem.Path.Join(GameDataPath, GlobalGameManagersName);
		Version = GetUnityVersionFromDataDirectory(globalGameManagersPath);
		Il2CppGameAssemblyPath = FileSystem.Path.Join(ModulesPath, PS4IL2CppGameAssemblyName);
		Il2CppMetaDataPath = FileSystem.Path.Join(GameDataPath, MetadataName, DefaultGlobalMetadataName);

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
		return fileSystem.Directory.Exists(path) && IsRootPS4Directory(path, fileSystem);
	}

	private static bool IsRootPS4Directory(string rootDirectory, FileSystem fileSystem)
	{
		return GetDataDirectory(rootDirectory, fileSystem, out _);
	}

	private static bool GetDataDirectory(string rootDirectory, FileSystem fileSystem, [NotNullWhen(true)] out string? dataPath)
	{
		foreach (string file in fileSystem.Directory.EnumerateFiles(rootDirectory))
		{
			if (fileSystem.Path.GetFileName(file) == PS4ExecutableName)
			{
				dataPath = fileSystem.Path.Join(rootDirectory, PS4DataFolderName);
				if (fileSystem.Directory.Exists(dataPath))
				{
					return true;
				}
			}
		}

		dataPath = null;
		return false;
	}
}
