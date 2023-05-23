namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class MacGameStructure : PlatformGameStructure
	{
		public MacGameStructure(string rootPath)
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

			string resourcePath = Path.Combine(m_root.FullName, ContentsName, ResourcesName);
			if (!Directory.Exists(resourcePath))
			{
				throw new Exception("Resources directory wasn't found");
			}
			string dataPath = Path.Combine(resourcePath, DataFolderName);
			if (!Directory.Exists(dataPath))
			{
				throw new Exception("Data directory wasn't found");
			}
			DataPaths = new string[] { dataPath, resourcePath };


			Name = m_root.Name.Substring(0, m_root.Name.Length - AppExtension.Length);
			RootPath = rootPath;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Combine(GameDataPath, StreamingName);
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = Path.Combine(RootPath, ContentsName, FrameworksName, MacUnityPlayerName);
			Version = null;

			Il2CppGameAssemblyPath = Path.Combine(RootPath, ContentsName, FrameworksName, "GameAssembly.dylib");
			Il2CppMetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

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

		public static bool IsMacStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(path);
			if (!dinfo.Exists)
			{
				return false;
			}
			if (!dinfo.Name.EndsWith(AppExtension, StringComparison.Ordinal))
			{
				return false;
			}

			string dataPath = Path.Combine(dinfo.FullName, ContentsName, ResourcesName, DataFolderName);
			if (!Directory.Exists(dataPath))
			{
				return false;
			}
			string resourcePath = Path.Combine(dinfo.FullName, ContentsName, ResourcesName);
			if (!Directory.Exists(resourcePath))
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
}
