using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class PS4GameStructure : PlatformGameStructure
	{
		private const string PS4ExecutableName = "eboot.bin";
		private const string PS4DataFolderName = "Media";
		private const string ModulesName = "Modules";
		private const string PS4IL2CppGameAssemblyName = "Il2CppUserAssemblies.prx";

		private string ModulesPath { get; set; }

		public PS4GameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			m_root = new DirectoryInfo(rootPath);
			if (!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			if (!GetDataDirectory(m_root, out string? dataPath))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = m_root.Name;
			RootPath = rootPath;
			GameDataPath = dataPath;
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			ModulesPath = Path.Combine(GameDataPath, ModulesName);
			UnityPlayerPath = null;
			string globalGameManagersPath = Path.Combine(GameDataPath, GlobalGameManagersName);
			Version = GetUnityVersionFromDataDirectory(globalGameManagersPath);
			Il2CppGameAssemblyPath = Path.Combine(ModulesPath, PS4IL2CppGameAssemblyName);
			Il2CppMetaDataPath = Path.Combine(GameDataPath, MetadataName, DefaultGlobalMetadataName);

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

		public static bool IsPS4Structure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(path);
			if (!dinfo.Exists)
			{
				return false;
			}
			return IsRootPS4Directory(dinfo);
		}

		private static bool IsRootPS4Directory(DirectoryInfo rootDiectory)
		{
			return GetDataDirectory(rootDiectory, out _);
		}

		private static bool GetDataDirectory(DirectoryInfo rootDiectory, [NotNullWhen(true)] out string? dataPath)
		{
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Name == PS4ExecutableName)
				{
					dataPath = Path.Combine(rootDiectory.FullName, PS4DataFolderName);
					if (Directory.Exists(dataPath))
					{
						return true;
					}
				}
			}

			dataPath = null;
			return false;
		}
	}
}
