using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class LinuxGameStructure : PlatformGameStructure
	{
		public LinuxGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			if (IsExecutableFile(rootPath))
			{
				Logger.Info(LogCategory.Import, "Linux executable found. Setting root to parent directory");
				m_root = new FileInfo(rootPath).Directory ?? throw new Exception("Could not get file directory");
			}
			else if (IsUnityDataDirectory(rootPath))
			{
				Logger.Info(LogCategory.Import, "Linux data directory found. Setting root to parent directory");
				m_root = new DirectoryInfo(rootPath).Parent ?? throw new Exception("Could not get parent directory");
			}
			else
			{
				m_root = new DirectoryInfo(rootPath);
				if (!m_root.Exists)
				{
					throw new Exception($"Directory '{rootPath}' doesn't exist");
				}
			}

			if (!GetDataLinuxDirectory(m_root, out string? dataPath, out string? name))
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

		public static bool IsLinuxStructure(string path)
		{
			DirectoryInfo dinfo;
			if (IsExecutableFile(path))
			{
				dinfo = new FileInfo(path).Directory ?? throw new Exception("Could not get file directory");
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
				return IsRootLinuxDirectory(dinfo);
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
			string x86Path = Path.Combine(rootPath, gameName + x86Extension);
			string x64Path = Path.Combine(rootPath, gameName + x64Extension);
			string x86_64Path = Path.Combine(rootPath, gameName + x86_64Extension);
			if (File.Exists(x86Path) || File.Exists(x64Path) || File.Exists(x86_64Path))
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
			return !string.IsNullOrEmpty(filePath)
				&& (filePath.EndsWith(x86Extension) || filePath.EndsWith(x64Extension) || filePath.EndsWith(x86_64Extension))
				&& File.Exists(filePath);
		}

		private static bool IsRootLinuxDirectory(DirectoryInfo rootDiectory)
		{
			return GetDataLinuxDirectory(rootDiectory, out _, out _);
		}

		private static bool GetDataLinuxDirectory(DirectoryInfo rootDiectory, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? name)
		{
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Extension == x86Extension || finfo.Extension == x64Extension || finfo.Extension == x86_64Extension)
				{
					name = Path.GetFileNameWithoutExtension(finfo.Name);
					string dataFolder = $"{name}_{DataFolderName}";
					dataPath = Path.Combine(rootDiectory.FullName, dataFolder);
					if (Directory.Exists(dataPath))
					{
						return true;
					}
				}
			}

			name = null;
			dataPath = null;
			return false;
		}


		private const string x86Extension = ".x86";
		private const string x64Extension = ".x64";
		private const string x86_64Extension = ".x86_64";
	}
}
