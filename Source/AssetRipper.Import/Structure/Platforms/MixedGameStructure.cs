using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files.Streams.MultiFile;

namespace AssetRipper.Import.Structure.Platforms
{
	public sealed class MixedGameStructure : PlatformGameStructure
	{
		public MixedGameStructure(IEnumerable<string> paths)
		{
			HashSet<string> dataPaths = new HashSet<string>();
			foreach (string path in SelectUniquePaths(paths))
			{
				if (MultiFileStream.Exists(path))
				{
					string name = MultiFileStream.GetFileName(path);
					AddFile(Files, name, path);
					string directory = Path.GetDirectoryName(path) ?? throw new Exception("Could not get directory name");
					dataPaths.Add(directory);
				}
				else if (Directory.Exists(path))
				{
					DirectoryInfo directory = new DirectoryInfo(path);
					CollectFromDirectory(directory, Files, Assemblies, dataPaths);
				}
				else
				{
					throw new Exception($"Neither file nor directory at '{path}' exists");
				}
			}

			DataPaths = dataPaths.ToArray();
			Name = Files.Count == 0 ? string.Empty : Files.First().Key;
			RootPath = null;
			GameDataPath = null;
			ManagedPath = null;
			UnityPlayerPath = null;
			Version = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;
			Backend = Assemblies.Count > 0 ? ScriptingBackend.Mono : ScriptingBackend.Unknown;
		}

		private IEnumerable<string> SelectUniquePaths(IEnumerable<string> paths)
		{
			return paths.Select(t => MultiFileStream.GetFilePath(t)).Distinct();
		}

		private void CollectFromDirectory(DirectoryInfo root, IDictionary<string, string> files, IDictionary<string, string> assemblies, ISet<string> dataPaths)
		{
			int count = files.Count;
			CollectSerializedGameFiles(root, files);
			CollectWebFiles(root, files);
			CollectAssetBundles(root, files);
			CollectAssembliesSafe(root, assemblies);
			if (files.Count != count)
			{
				dataPaths.Add(root.FullName);
			}

			foreach (DirectoryInfo subDirectory in root.EnumerateDirectories())
			{
				CollectFromDirectory(subDirectory, files, assemblies, dataPaths);
			}
		}

		private void CollectWebFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach (FileInfo levelFile in root.EnumerateFiles())
			{
				string extension = Path.GetExtension(levelFile.Name);
				switch (extension)
				{
					case WebGLGameStructure.DataExtension:
					case WebGLGameStructure.DataGzExtension:
						{
							string name = Path.GetFileNameWithoutExtension(levelFile.Name);
							AddFile(files, name, levelFile.FullName);
						}
						break;

					case WebGLGameStructure.UnityWebExtension:
						{
							if (levelFile.Name.EndsWith(WebGLGameStructure.DataWebExtension, StringComparison.Ordinal))
							{
								string name = levelFile.Name.Substring(0, levelFile.Name.Length - WebGLGameStructure.DataWebExtension.Length);
								AddFile(files, name, levelFile.FullName);
							}
						}
						break;
				}
			}
		}

		private void CollectAssembliesSafe(DirectoryInfo root, IDictionary<string, string> assemblies)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if (MonoManager.IsMonoAssembly(file.Name))
				{
					if (assemblies.ContainsKey(file.Name))
					{
						Logger.Log(LogType.Warning, LogCategory.Import, $"Duplicate assemblies found: '{assemblies[file.Name]}' & '{file.FullName}'");
					}
					else
					{
						assemblies.Add(file.Name, file.FullName);
					}
				}
			}
		}
	}
}
