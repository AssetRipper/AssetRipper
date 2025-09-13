using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class WiiUGameStructure : PlatformGameStructure
{
	public WiiUGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		Name = fileSystem.Path.GetFileName(rootPath);
		GameDataPath = fileSystem.Path.Join(RootPath, ContentName, DataFolderName);
		if (!fileSystem.Directory.Exists(GameDataPath))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}
		StreamingAssetsPath = fileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = fileSystem.Path.Join(GameDataPath, ResourcesName);
		ManagedPath = fileSystem.Path.Join(GameDataPath, ManagedName);
		UnityPlayerPath = null;
		Version = null;
		Il2CppGameAssemblyPath = null;
		Il2CppMetaDataPath = null;
		//WiiU doesn't support IL2Cpp
		//See https://docs.unity3d.com/2017.4/Documentation/Manual/ScriptingRestrictions.html

		if (HasMonoAssemblies(ManagedPath))
		{
			Backend = ScriptingBackend.Mono;
		}
		else
		{
			Backend = ScriptingBackend.Unknown;
		}

		DataPaths = [GameDataPath];
	}

	public static bool Exists(string rootPath, FileSystem fileSystem)
	{
		string gameDataPath = fileSystem.Path.Join(rootPath, ContentName, DataFolderName);
		return fileSystem.Directory.Exists(gameDataPath);
	}

	private const string ContentName = "content";
}
