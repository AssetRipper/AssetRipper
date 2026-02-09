using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class WindowsGameStructure : PlatformGameStructure
{
	public WindowsGameStructure(string rootPath, FileSystem fileSystem) : base(GetActualRootPath(rootPath, fileSystem), fileSystem)
	{
		Debug.Assert(RootPath is not null);
		if (rootPath != RootPath)
		{
			Logger.Info(LogCategory.Import, "An executable file or data directory was found, so the parent directory is being used instead.");
		}

		if (!GetDataDirectory(RootPath, fileSystem, out string? dataPath, out string? name))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = name;
		GameDataPath = dataPath;
		StreamingAssetsPath = FileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(GameDataPath, ManagedName);
		UnityPlayerPath = FileSystem.Path.Join(RootPath, DefaultUnityPlayerName);
		Version = null;
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
		string directory;
		if (IsExecutableFile(path, fileSystem))
		{
			directory = fileSystem.Path.GetDirectoryName(path) ?? throw new Exception("Could not get file directory");
		}
		else if (IsUnityDataDirectory(path, fileSystem))
		{
			return true;
		}
		else
		{
			directory = path;
		}

		return fileSystem.Directory.Exists(directory) && IsRootDirectory(directory, fileSystem);
	}

	private static bool IsUnityDataDirectory(string folderPath, FileSystem fileSystem)
	{
		const string Suffix = $"_{DataFolderName}";
		if (string.IsNullOrEmpty(folderPath) || !folderPath.EndsWith(Suffix, StringComparison.Ordinal))
		{
			return false;
		}

		if (!fileSystem.Directory.Exists(folderPath))
		{
			return false;
		}

		string folderName = fileSystem.Path.GetFileName(folderPath);
		string gameName = folderName[..^Suffix.Length];
		string rootPath = fileSystem.Path.GetDirectoryName(folderPath);
		string exePath = fileSystem.Path.Join(rootPath, gameName + ExeExtension);
		if (fileSystem.File.Exists(exePath))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private static bool IsExecutableFile(string filePath, FileSystem fileSystem)
	{
		return !string.IsNullOrEmpty(filePath) && filePath.EndsWith(ExeExtension, StringComparison.OrdinalIgnoreCase) && fileSystem.File.Exists(filePath);
	}

	private static string GetActualRootPath(string rootPath, FileSystem fileSystem)
	{
		ArgumentException.ThrowIfNullOrEmpty(rootPath);
		if (IsExecutableFile(rootPath, fileSystem))
		{
			return fileSystem.Path.GetDirectoryName(rootPath) ?? throw new Exception("Could not get file directory");
		}
		else if (IsUnityDataDirectory(rootPath, fileSystem))
		{
			return fileSystem.Path.GetDirectoryName(rootPath) ?? throw new Exception("Could not get parent directory");
		}
		else
		{
			return rootPath;
		}
	}

	private static bool IsRootDirectory(string rootDirectory, FileSystem fileSystem)
	{
		return GetDataDirectory(rootDirectory, fileSystem, out string? _, out string? _);
	}

	private static bool GetDataDirectory(string rootDirectory, FileSystem fileSystem, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? name)
	{
		name = "";
		int exeCount = 0;
		foreach (string file in fileSystem.Directory.EnumerateFiles(rootDirectory))
		{
			if (file.EndsWith(ExeExtension, StringComparison.OrdinalIgnoreCase))
			{
				exeCount++;
				name = fileSystem.Path.GetFileNameWithoutExtension(file);
				string dataFolder = $"{name}_{DataFolderName}";
				dataPath = fileSystem.Path.Join(rootDirectory, dataFolder);
				if (fileSystem.Directory.Exists(dataPath))
				{
					return true;
				}
			}
		}

		if (exeCount > 0)
		{
			name = exeCount == 1 ? name : fileSystem.Path.GetFileName(rootDirectory);
			dataPath = fileSystem.Path.Join(rootDirectory, DataFolderName);
			if (fileSystem.Directory.Exists(dataPath))
			{
				return true;
			}
		}

		name = null;
		dataPath = null;
		return false;
	}


	private const string ExeExtension = ".exe";
}
