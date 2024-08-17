namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class AndroidGameStructure : PlatformGameStructure
	{
		public AndroidGameStructure(string rootPath) : this(rootPath, string.Empty) { }

		public AndroidGameStructure(string rootPath, string? obbPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			m_root = new DirectoryInfo(rootPath);
			if (!m_root.Exists)
			{
				throw new Exception($"Root directory '{rootPath}' doesn't exist");
			}

			string apkDataPath = Path.Combine(rootPath, AssetName, BinName, DataFolderName);
			DirectoryInfo apkDataDirectory = new DirectoryInfo(apkDataPath);
			if (!apkDataDirectory.Exists)
			{
				throw new Exception($"Data directory hasn't been found");
			}
			List<string> dataPaths = new List<string>() { apkDataPath };

			RootPath = rootPath;
			GameDataPath = apkDataPath;
			StreamingAssetsPath = null;
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			LibPath = Path.Combine(RootPath, LibName);
			Il2CppGameAssemblyPath = GetIl2CppGameAssemblyPath(LibPath);
			Il2CppMetaDataPath = Path.Combine(ManagedPath, MetadataName, DefaultGlobalMetadataName);
			UnityPlayerPath = null;
			Version = GetUnityVersionFromDataDirectory(GameDataPath);

			if (HasIl2CppFiles())
			{
				Backend = Assembly.ScriptingBackend.IL2Cpp;
			}
			else if (IsMono(ManagedPath))
			{
				Backend = Assembly.ScriptingBackend.Mono;
			}
			else
			{
				Backend = Assembly.ScriptingBackend.Unknown;
			}

			if (obbPath != null)
			{
				m_obbRoot = new DirectoryInfo(obbPath);
				if (!m_obbRoot.Exists)
				{
					throw new Exception($"Obb directory '{obbPath}' doesn't exist");
				}

				string obbDataPath = Path.Combine(obbPath, AssetName, BinName, DataFolderName);
				if (!Directory.Exists(obbDataPath))
				{
					throw new Exception($"Obb data directory '{obbDataPath}' wasn't found");
				}
				dataPaths.Add(obbDataPath);
			}
			DataPaths = dataPaths.ToArray();
		}

		public override void CollectFiles(bool skipStreamingAssets)
		{
			base.CollectFiles(skipStreamingAssets);
			CollectApkAssetBundles(Files);
		}

		public static bool IsAndroidStructure(string path)
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			if (!directory.Exists)
			{
				return false;
			}

			int match = GetRootAndroidDirectoryMatch(directory);
			if (match <= 8)
			{
				return false;
			}

			string dataPath = Path.Combine(path, AssetName, BinName, DataFolderName);

			return Directory.Exists(dataPath);
		}

		public static bool IsAndroidObbStructure(string path)
		{
			DirectoryInfo directory = new DirectoryInfo(path);
			if (!directory.Exists)
			{
				return false;
			}

			int match = GetRootAndroidDirectoryMatch(directory);
			if (match != 8)
			{
				return false;
			}

			string dataPath = Path.Combine(path, AssetName, BinName, DataFolderName);

			return Directory.Exists(dataPath);
		}

		private static int GetRootAndroidDirectoryMatch(DirectoryInfo directory)
		{
			int matches = 0;
			foreach (DirectoryInfo subDirectory in directory.EnumerateDirectories())
			{
				switch (subDirectory.Name)
				{
					case AssetName:
						matches |= 8;
						break;

					case MetaName:
						matches |= 4;
						break;

					case LibName:
						matches |= 2;
						break;
				}
			}
			return matches;
		}

		private void CollectApkAssetBundles(List<KeyValuePair<string, string>> files)
		{
			string assetPath = Path.Combine(m_root.FullName, AssetName);
			DirectoryInfo root = new DirectoryInfo(assetPath);

			CollectAssetBundles(root, files);
			foreach (DirectoryInfo subDirectory in root.EnumerateDirectories())
			{
				if (subDirectory.Name == BinName)
				{
					continue;
				}
				CollectAssetBundlesRecursively(subDirectory, files);
			}
		}

		private static string? GetIl2CppGameAssemblyPath(string libDirectory)
		{
			if (string.IsNullOrEmpty(libDirectory) || !Directory.Exists(libDirectory))
			{
				return null;
			}

			return Directory.GetFiles(libDirectory, Il2CppGameAssemblyName, SearchOption.AllDirectories).FirstOrDefault();
		}

		private static string? GetAndroidUnityAssemblyPath(string libDirectory)
		{
			if (string.IsNullOrEmpty(libDirectory) || !Directory.Exists(libDirectory))
			{
				return null;
			}

			return Directory.GetFiles(libDirectory, AndroidUnityAssemblyName, SearchOption.AllDirectories).FirstOrDefault();
		}

		private static bool IsMono(string managedDirectory)
		{
			if (string.IsNullOrEmpty(managedDirectory) || !Directory.Exists(managedDirectory))
			{
				return false;
			}

			return Directory.GetFiles(managedDirectory, "*.dll").Length > 0;
		}

		public string LibPath { get; private set; }

		private const string AssetName = "assets";
		private const string MetaName = "META-INF";
		private const string BinName = "bin";
		private const string Il2CppGameAssemblyName = "libil2cpp.so";
		private const string AndroidUnityAssemblyName = "libunity.so";

		private readonly DirectoryInfo? m_obbRoot;
	}
}
