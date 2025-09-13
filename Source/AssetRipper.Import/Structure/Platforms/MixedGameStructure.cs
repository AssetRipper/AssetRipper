using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.Streams;

namespace AssetRipper.Import.Structure.Platforms;

public sealed class MixedGameStructure : PlatformGameStructure
{
	public MixedGameStructure(IEnumerable<string> paths, FileSystem fileSystem) : base(fileSystem)
	{
		HashSet<string> dataPaths = [];
		foreach (string path in SelectUniquePaths(paths))
		{
			if (MultiFileStream.Exists(path, FileSystem))
			{
				string name = MultiFileStream.GetFileName(path);
				AddFile(Files, name, path);
				string directory = FileSystem.Path.GetDirectoryName(path) ?? throw new Exception("Could not get directory name");
				dataPaths.Add(directory);
			}
			else if (FileSystem.Directory.Exists(path))
			{
				CollectFromDirectory(path, Files, Assemblies, dataPaths);
			}
			else
			{
				throw new Exception($"Neither file nor directory at '{path}' exists");
			}
		}

		DataPaths = dataPaths.ToArray();
		Name = Files.Count == 0 ? string.Empty : Files.First().Key;
		GameDataPath = null;
		ManagedPath = null;
		UnityPlayerPath = null;
		Version = null;
		Il2CppGameAssemblyPath = null;
		Il2CppMetaDataPath = null;
		Backend = Assemblies.Count > 0 ? ScriptingBackend.Mono : ScriptingBackend.Unknown;
	}

	private static IEnumerable<string> SelectUniquePaths(IEnumerable<string> paths)
	{
		return paths.Select(t => MultiFileStream.GetFilePath(t)).Distinct();
	}

	private void CollectFromDirectory(string root, List<KeyValuePair<string, string>> files, Dictionary<string, string> assemblies, ISet<string> dataPaths)
	{
		int count = files.Count;
		CollectSerializedGameFiles(root, files);
		CollectWebFiles(root, files);
		CollectAssetBundles(root, files);
		CollectAssembliesSafe(root, assemblies);
		if (files.Count != count)
		{
			dataPaths.Add(root);
		}

		foreach (string subDirectory in FileSystem.Directory.EnumerateDirectories(root))
		{
			CollectFromDirectory(subDirectory, files, assemblies, dataPaths);
		}
	}

	private void CollectWebFiles(string root, List<KeyValuePair<string, string>> files)
	{
		foreach (string levelFile in FileSystem.Directory.EnumerateFiles(root))
		{
			string extension = FileSystem.Path.GetExtension(levelFile);
			switch (extension)
			{
				case WebGLGameStructure.DataExtension:
				case WebGLGameStructure.DataGzExtension:
					{
						string name = FileSystem.Path.GetFileNameWithoutExtension(levelFile);
						AddFile(files, name, levelFile);
					}
					break;

				case WebGLGameStructure.UnityWebExtension:
					{
						string fileName = FileSystem.Path.GetFileName(levelFile);
						if (fileName.EndsWith(WebGLGameStructure.DataWebExtension, StringComparison.Ordinal))
						{
							string name = fileName.Substring(0, fileName.Length - WebGLGameStructure.DataWebExtension.Length);
							AddFile(files, name, levelFile);
						}
					}
					break;
			}
		}
	}

	private void CollectAssembliesSafe(string root, Dictionary<string, string> assemblies)
	{
		foreach (string file in FileSystem.Directory.EnumerateFiles(root))
		{
			string name = FileSystem.Path.GetFileName(file);
			if (MonoManager.IsMonoAssembly(name))
			{
				if (assemblies.TryGetValue(name, out string? value))
				{
					Logger.Log(LogType.Warning, LogCategory.Import, $"Duplicate assemblies found: '{value}' & '{file}'");
				}
				else
				{
					assemblies.Add(name, file);
				}
			}
		}
	}
}
