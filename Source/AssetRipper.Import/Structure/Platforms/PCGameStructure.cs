using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			if (IsExecutableFile(rootPath))
			{
				Logger.Info(LogCategory.Import, "PC executable found. Setting root to parent directory");
				m_root = new FileInfo(rootPath).Directory ?? throw new Exception("File has no directory");
			}
			else if (IsUnityDataDirectory(rootPath))
			{
				Logger.Info(LogCategory.Import, "PC data directory found. Setting root to parent directory");
				m_root = new DirectoryInfo(rootPath).Parent ?? throw new Exception("Directory has no parent");
			}
			else
			{
				m_root = new DirectoryInfo(rootPath);
				if (!m_root.Exists)
				{
					throw new Exception($"Directory '{rootPath}' doesn't exist");
				}
			}

			if (!GetDataPCDirectory(m_root, out string? dataPath, out string? name))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = name;
			RootPath = m_root.FullName;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Combine(GameDataPath, StreamingName);
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = Path.Combine(RootPath, DefaultUnityPlayerName);
			Version = null;
			Il2CppGameAssemblyPath = Path.Combine(RootPath, DefaultGameAssemblyName);
			Il2CppMetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

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

			DataPaths = new string[] { dataPath };
		}

		public static bool IsPCStructure(string path)
		{
			DirectoryInfo dinfo;
			if (IsExecutableFile(path))
			{
				dinfo = new FileInfo(path).Directory ?? throw new Exception("File has no directory");
			}
			else if (IsUnityDataDirectory(path))
			{
				return true;
			}
			else
			{
				dinfo = new DirectoryInfo(path);
			}

			if (!dinfo.Exists)
			{
				return false;
			}
			else
			{
				return IsRootPCDirectory(dinfo);
			}
		}

		private static bool IsUnityDataDirectory(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath) || !folderPath.EndsWith($"_{DataFolderName}"))
			{
				return false;
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			if (!directoryInfo.Exists || directoryInfo.Parent == null)
			{
				return false;
			}

			string folderName = directoryInfo.Name;
			string gameName = folderName.Substring(0, folderName.IndexOf($"_{DataFolderName}"));
			string rootPath = directoryInfo.Parent.FullName;
			if (File.Exists(Path.Combine(rootPath, gameName + ExeExtension)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool IsExecutableFile(string filePath)
		{
			return !string.IsNullOrEmpty(filePath) && filePath.EndsWith(ExeExtension) && File.Exists(filePath);
		}

		private static bool IsRootPCDirectory(DirectoryInfo rootDirectory)
		{
			return GetDataPCDirectory(rootDirectory, out string? _, out string? _);
		}

		private static bool GetDataPCDirectory(DirectoryInfo rootDirectory, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? name)
		{
			name = "";
			int exeCount = 0;
			foreach (FileInfo fileInfo in rootDirectory.EnumerateFiles())
			{
				if (fileInfo.Extension == ExeExtension)
				{
					exeCount++;
					name = Path.GetFileNameWithoutExtension(fileInfo.Name);
					string dataFolder = $"{name}_{DataFolderName}";
					dataPath = Path.Combine(rootDirectory.FullName, dataFolder);
					if (Directory.Exists(dataPath))
					{
						return true;
					}
				}
			}

			if (exeCount > 0)
			{
				name = exeCount == 1 ? name : rootDirectory.Name;
				dataPath = Path.Combine(rootDirectory.FullName, DataFolderName);
				if (Directory.Exists(dataPath))
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
}
