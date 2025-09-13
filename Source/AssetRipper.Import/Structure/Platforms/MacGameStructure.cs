using AssetRipper.IO.Files;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class MacGameStructure : PlatformGameStructure
{
	public MacGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		string resourcePath = FileSystem.Path.Join(rootPath, ContentsName, ResourcesName);
		if (!FileSystem.Directory.Exists(resourcePath))
		{
			throw new DirectoryNotFoundException("Resources directory wasn't found");
		}
		string dataPath = FileSystem.Path.Join(resourcePath, DataFolderName);
		if (!FileSystem.Directory.Exists(dataPath))
		{
			throw new DirectoryNotFoundException("Data directory wasn't found");
		}
		DataPaths = [dataPath, resourcePath];

		Debug.Assert(rootPath.EndsWith(AppExtension, StringComparison.Ordinal));
		Name = FileSystem.Path.GetFileNameWithoutExtension(rootPath);
		GameDataPath = dataPath;
		StreamingAssetsPath = FileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = FileSystem.Path.Join(GameDataPath, ManagedName);
		UnityPlayerPath = FileSystem.Path.Join(RootPath, ContentsName, FrameworksName, MacUnityPlayerName);
		Version = null;

		Il2CppGameAssemblyPath = FileSystem.Path.Join(RootPath, ContentsName, FrameworksName, "GameAssembly.dylib");
		Il2CppMetaDataPath = FileSystem.Path.Join(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

		if (HasIl2CppFiles())
		{
			Backend = Assembly.ScriptingBackend.IL2Cpp;
		}
		else if (HasMonoAssemblies(ManagedPath))
		{
			Backend = Assembly.ScriptingBackend.Mono;
		}
		else
		{
			Backend = Assembly.ScriptingBackend.Unknown;
		}
	}

	public static bool Exists(string path, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(path))
		{
			return false;
		}
		if (fileSystem.Path.GetExtension(path) != AppExtension)
		{
			return false;
		}

		string dataPath = fileSystem.Path.Join(path, ContentsName, ResourcesName, DataFolderName);
		if (!fileSystem.Directory.Exists(dataPath))
		{
			return false;
		}
		string resourcePath = fileSystem.Path.Join(path, ContentsName, ResourcesName);
		if (!fileSystem.Directory.Exists(resourcePath))
		{
			return false;
		}
		return true;
	}


	private const string ContentsName = "Contents";
	private const string FrameworksName = "Frameworks";
	private const string MacUnityPlayerName = "UnityPlayer.dylib";
	private const string AppExtension = ".app";
}
