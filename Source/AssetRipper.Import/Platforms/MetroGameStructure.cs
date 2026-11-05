using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;
using System.Diagnostics;

namespace AssetRipper.Import.Platforms;

/// <summary>
/// A dumped Unity "Windows Store" (Metro / UWP) package.
/// </summary>
/// <remarks>
/// <para>
/// Unlike a standard Windows player (<c>Game_Data/Managed/*.dll</c>), a Metro package keeps the
/// managed assemblies (<c>Assembly-CSharp.dll</c>, <c>UnityEngine.dll</c>, ...) directly in the package root,
/// next to the executable, <c>UnityPlayer.dll</c> and <c>AppxManifest.xml</c>.
/// The serialized files live in a plain <c>Data</c> folder.
/// </para>
/// <code>
/// Package/
///   AppxManifest.xml
///   Game.exe
///   UnityPlayer.dll
///   Assembly-CSharp.dll      &lt;- used by this structure
///   UnityEngine.dll          &lt;- used by this structure
///   Data/
///     globalgamemanagers
///     level0
///     ...
/// </code>
/// </remarks>
internal sealed class MetroGameStructure : PlatformGameStructure
{
	public MetroGameStructure(string rootPath, FileSystem fileSystem) : base(GetActualRootPath(rootPath, fileSystem), fileSystem)
	{
		Debug.Assert(RootPath is not null);
		if (rootPath != RootPath)
		{
			Logger.Info(LogCategory.Import, "An executable file or data directory was found, so the parent directory is being used instead.");
		}

		GameDataPath = FileSystem.Path.Join(RootPath, DataFolderName);
		if (!FileSystem.Directory.Exists(GameDataPath))
		{
			throw new DirectoryNotFoundException($"Data directory wasn't found");
		}

		Name = GetName(RootPath, FileSystem);
		StreamingAssetsPath = FileSystem.Path.Join(GameDataPath, StreamingName);
		ResourcesPath = FileSystem.Path.Join(GameDataPath, ResourcesName);
		// Metro packages keep their managed assemblies in the root, not in Data/Managed.
		ManagedPath = RootPath;
		UnityPlayerPath = FileSystem.Path.Join(RootPath, DefaultUnityPlayerName);
		Version = GetUnityVersionFromDataDirectory(GameDataPath);
		Il2CppGameAssemblyPath = FileSystem.Path.Join(RootPath, DefaultGameAssemblyName);
		Il2CppMetaDataPath = FileSystem.Path.Join(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

		if (HasIl2CppFiles())
		{
			Backend = ScriptingBackend.IL2Cpp;
		}
		else if (HasManagedUnityAssemblies(RootPath, FileSystem))
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
		return TryGetRootDirectory(path, fileSystem, out _);
	}

	private static string GetActualRootPath(string path, FileSystem fileSystem)
	{
		ArgumentException.ThrowIfNullOrEmpty(path);
		return TryGetRootDirectory(path, fileSystem, out string? root) ? root : path;
	}

	/// <summary>
	/// Accepts the package root, an executable inside it, or its <c>Data</c> folder.
	/// </summary>
	private static bool TryGetRootDirectory(string path, FileSystem fileSystem, [NotNullWhen(true)] out string? rootDirectory)
	{
		rootDirectory = null;
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}

		if (fileSystem.File.Exists(path))
		{
			if (!path.EndsWith(ExeExtension, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			string? directory = fileSystem.Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory) && IsMetroRoot(directory, fileSystem))
			{
				rootDirectory = directory;
				return true;
			}
			return false;
		}

		if (!fileSystem.Directory.Exists(path))
		{
			return false;
		}

		if (IsMetroRoot(path, fileSystem))
		{
			rootDirectory = path;
			return true;
		}

		if (string.Equals(fileSystem.Path.GetFileName(path.TrimEnd('/', '\\')), DataFolderName, StringComparison.OrdinalIgnoreCase))
		{
			string? parent = fileSystem.Path.GetDirectoryName(path.TrimEnd('/', '\\'));
			if (!string.IsNullOrEmpty(parent) && IsMetroRoot(parent, fileSystem))
			{
				rootDirectory = parent;
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// A Metro root has a <c>Data</c> folder and either an <c>AppxManifest.xml</c>
	/// or Unity's managed assemblies sitting in the root while <c>Data/Managed</c> doesn't exist.
	/// </summary>
	private static bool IsMetroRoot(string directory, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(fileSystem.Path.Join(directory, DataFolderName)))
		{
			return false;
		}

		if (HasAppxManifest(directory, fileSystem))
		{
			return true;
		}

		return HasManagedUnityAssemblies(directory, fileSystem)
			&& !fileSystem.Directory.Exists(fileSystem.Path.Join(directory, DataFolderName, ManagedName));
	}

	private static bool HasAppxManifest(string directory, FileSystem fileSystem)
	{
		foreach (string file in fileSystem.Directory.EnumerateFiles(directory))
		{
			if (string.Equals(fileSystem.Path.GetFileName(file), AppxManifestName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasManagedUnityAssemblies(string directory, FileSystem fileSystem)
	{
		foreach (string file in fileSystem.Directory.EnumerateFiles(directory))
		{
			string name = fileSystem.Path.GetFileName(file);
			if (name.Equals(UnityEngineName, StringComparison.OrdinalIgnoreCase) ||
				(name.StartsWith(AssemblyPrefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetName(string rootDirectory, FileSystem fileSystem)
	{
		string? executableName = null;
		foreach (string file in fileSystem.Directory.EnumerateFiles(rootDirectory))
		{
			if (file.EndsWith(ExeExtension, StringComparison.OrdinalIgnoreCase))
			{
				if (executableName is not null)
				{
					// More than one executable, so the folder name is less ambiguous.
					return fileSystem.Path.GetFileName(rootDirectory.TrimEnd('/', '\\'));
				}
				executableName = fileSystem.Path.GetFileNameWithoutExtension(file);
			}
		}
		return executableName ?? fileSystem.Path.GetFileName(rootDirectory.TrimEnd('/', '\\'));
	}

	private const string ExeExtension = ".exe";
	private const string AppxManifestName = "AppxManifest.xml";
	private const string UnityEngineName = "UnityEngine.dll";
	private const string AssemblyPrefix = "Assembly-";
}
