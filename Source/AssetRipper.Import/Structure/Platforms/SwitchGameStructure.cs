using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class SwitchGameStructure : PlatformGameStructure
{
	private const string ExefsName = "exefs";
	private const string RomName = "romfs";
	private const string MainName = "main";

	public SwitchGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		if (!GetDataSwitchDirectory(rootPath, FileSystem, out string? dataPath))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = FileSystem.Path.GetFileName(rootPath);
		GameDataPath = dataPath;
		StreamingAssetsPath = FileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(GameDataPath, ManagedName);
		UnityPlayerPath = null;
		Version = GetUnityVersionFromDataDirectory(GameDataPath);
		Il2CppGameAssemblyPath = FileSystem.Path.Join(rootPath, ExefsName, MainName);
		Il2CppMetaDataPath = FileSystem.Path.Join(ManagedPath, MetadataName, DefaultGlobalMetadataName);
		Backend = HasIl2CppFiles() ? Assembly.ScriptingBackend.IL2Cpp : Assembly.ScriptingBackend.Unknown;

		DataPaths = [GameDataPath];
	}

	public static bool Exists(string path, FileSystem fileSystem)
	{
		return fileSystem.Directory.Exists(path)
			&& fileSystem.Directory.Exists(fileSystem.Path.Join(path, ExefsName))
			&& GetDataSwitchDirectory(path, fileSystem, out _);
	}

	private static bool GetDataSwitchDirectory(string rootDirectory, FileSystem fileSystem, [NotNullWhen(true)] out string? dataPath)
	{
		dataPath = null;
		string romPath = fileSystem.Path.Join(rootDirectory, RomName);
		if (!fileSystem.Directory.Exists(romPath))
		{
			return false;
		}

		string ldataPath = fileSystem.Path.Join(romPath, DataFolderName);
		if (!fileSystem.Directory.Exists(ldataPath))
		{
			return false;
		}

		dataPath = ldataPath;
		return true;
	}
}
