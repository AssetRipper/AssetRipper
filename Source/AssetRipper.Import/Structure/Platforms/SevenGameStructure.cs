using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
{
	public class SevenGameStructure : PlatformGameStructure
	{
		// only decompile Assembly-CSharp.dll

		#region Constant Fields

		public const string GameName = "7DaysToDie.exe"; // the game we want to rip, awesome game <3
		public const string ExeExtension = ".exe";
		public const string GameUnityDataDir = "7DaysToDie_Data";
		public const string ConfigDataName = "Data"; // where all the xml config files are and asset bundles
		public const string ModsDir = "Mods";
		public const string AddressablesDir = "Addressables";
		public const string BundlesDir = "Bundles";
		public const string MainBundleName = "data.unity3d";
		public readonly string PluginsDir = Path.Combine("Plugins", "x86_64");

		#endregion Constant Fields

		#region Properties

		public string? ConfigDataPath { get; protected set; }
		public string? PluginsPath { get; protected set; }
		public Dictionary<string, string> PluginAssemblies { get; } = new Dictionary<string, string>();

		#endregion Properties

		#region Constructors

		/// <summary>
		/// The root path should be where the main executable lies
		/// </summary>
		/// <param name="rootPath"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="Exception"></exception>
		public SevenGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException("Failed to provide a path to the game directory.");
			}

			// check root path for filename or directory

			if (IsGame7Structure(rootPath)) // check the current directory
			{
				Logger.Info("Found proper 7 Days To Die directory.");
				m_root = new DirectoryInfo(rootPath);
			}
			// might be redundant
			else if (IsExecutableFile(rootPath)) // check if the executable file was supplied instead of the entire folder
			{
				Logger.Info(LogCategory.Import, "7 Days game executable found. Setting root to parent directory");
				m_root = new FileInfo(Path.GetDirectoryName(rootPath)).Directory ?? throw new Exception("File has no directory");
			}

			// if not found throw

			if (!GetUnityDataDirectory(m_root, out string? dataPath, out string? name))
			{
				throw new Exception($"7 Days Unity Data directory wasn't found");
			}

			Name = name;
			RootPath = m_root.FullName;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Combine(GameDataPath, StreamingName);
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = Path.Combine(RootPath, DefaultUnityPlayerName);
			Version = GetUnityVersionFromBundleFile(Path.Combine(GameDataPath, MainBundleName)); // currently 2022.3.29f1 for version 1.0 b333
			ConfigDataPath = GetConfigDataPath();
			PluginsPath = Path.Combine(GameDataPath, PluginsDir);

			if (HasMonoAssemblies(ManagedPath))
			{
				Backend = ScriptingBackend.Mono;
			}
			else
			{
				//Backend = ScriptingBackend.Unknown;
				throw new Exception("Failed to locate the Managed directory containing all the assemblies.");
			}

			DataPaths = new string[] { dataPath };
		}

		#endregion Constructors

		#region Methods

		public string GetConfigDataPath()
		{
			string configDataDir = Path.Combine(RootPath, ConfigDataName);

			if (!Directory.Exists(configDataDir))
			{
				throw new Exception("Failed to find the Config Data folder.");
			}

			return configDataDir;
		}

		public override void CollectFiles(bool skipStreamingAssets)
		{
			foreach (string dataPath in DataPaths)
			{
				DirectoryInfo dataDirectory = new DirectoryInfo(dataPath);
				CollectGameFiles(dataDirectory, Files);
			}

			var addressablesDir = new DirectoryInfo(Path.Combine(RootPath, ConfigDataName, AddressablesDir, "Standalone"));
			CollectAddressablesBundles(addressablesDir, Files);

			var bundlesDir = new DirectoryInfo(Path.Combine(RootPath, ConfigDataName, BundlesDir, "Entities"));
			CollectMiscBundles(bundlesDir, Files);

			var pluginsDir = new DirectoryInfo(PluginsPath);
			CollectPlugins(pluginsDir, Files);

			CollectMainAssemblies();

			// we know there is a streaming assets folder
			CollectStreamingAssets(Files);
		}

		protected void CollectAddressablesBundles(DirectoryInfo root, IDictionary<string, string> files)
		{
			// search recursively all sub folders here
			// Should find 15 files currently as of V1.0

			foreach (FileInfo file in root.EnumerateFiles("*.bundle", SearchOption.AllDirectories))
			{
				//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
				if (IsAddressableBundle(file.FullName))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
					AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		protected void CollectMiscBundles(DirectoryInfo root, IDictionary<string, string> files)
		{
			// only search for two misc bundles here: Entities and Trees
			// the only two files without an extension

			foreach (FileInfo file in root.EnumerateFiles())
			{
				//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
				if (IsMiscBundle(file.FullName))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();
					AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		protected void CollectPlugins(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach (FileInfo file in root.EnumerateFiles("*.dll", SearchOption.AllDirectories))
			{
				//if (file.Extension == AssetBundleExtension || file.Extension == AlternateBundleExtension)
				if (IsDLL(file.FullName))
				{
					string name = Path.GetFileNameWithoutExtension(file.Name).ToLowerInvariant();

					// todo finish fix

					// add plugin assembly to dictionary

					//AddAssetBundle(files, name, file.FullName);
				}
			}
		}

		#endregion Methods


		#region Static Methods

		public static bool IsAddressableBundle(string path)
		{
			//Logger.Info($"IsAddressableBundle: {path}");
			if (path.ToLower().Contains("bundle"))
				return true;

			return false;
		}

		public static bool IsMiscBundle(string path)
		{
			Logger.Info($"IsMiscBundle: {path}");

			if (path.ToLower().Contains("manifest"))
				return false;

			return true;
		}

		public static bool IsDLL(string path)
		{
			//Logger.Info($"IsDLL: {path}");
			if (path.ToLower().Contains("dll"))
				return true;

			return false;
		}

		public static bool IsGame7Structure(string path)
		{
			// check if path is folder or file name
			var directoryX = path;

			if (!Directory.Exists(directoryX))
			{
				// directory doesn't exist, check if this is a file
				if (!File.Exists(path))
				{
					Logger.Error($"Path supplied is not a valid directory or File: {path}");
					return false;
				}

				Logger.Warning($"The supplied path is a file: {path}");

				// get directory of this file
				directoryX = Path.GetDirectoryName(path);

				if (!Directory.Exists(directoryX))
				{
					Logger.Error($"Path supplied is not a valid directory: {path}");
					return false;
				}
			}

			// check for unity data dir 7DaysToDie_Data
			var unityDataDir = Path.Combine(directoryX, GameUnityDataDir);
			if (!Directory.Exists(unityDataDir))
			{
				Logger.Error($"Path supplied is not a valid game directory: {path}");
				return false;
			}

			// check for Config data dir
			var configDataDir = Path.Combine(directoryX, ConfigDataName);
			if (!Directory.Exists(configDataDir))
			{
				Logger.Error($"Path supplied is not a valid game directory: {path}");
				return false;
			}

			//     Addressables - 15 bundles, *.bundle
			//     Bundles - 2, no extension - trees, entities is empty??

			return true;
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
			// looking for 7DaysToDie.exe
			return !string.IsNullOrEmpty(filePath) && (File.Exists(filePath) || File.Exists(Path.Combine(filePath, GameName)));
		}

		private static bool IsRootGameDirectory(DirectoryInfo rootDirectory)
		{
			return GetUnityDataDirectory(rootDirectory, out string? _, out string? _);
		}

		private static bool GetUnityDataDirectory(DirectoryInfo rootDirectory, [NotNullWhen(true)] out string? dataPath, [NotNullWhen(true)] out string? name)
		{
			name = "";
			int exeCount = 0;

			Logger.Info($"Found {rootDirectory.EnumerateFiles().Count()} files in this directory.");

			foreach (FileInfo fileInfo in rootDirectory.EnumerateFiles())
			{
				// we're just looking at all the executables and checking if one of them is the Data folder
				// we know what exe's are in the dir already
				// 7DaysToDie.exe
				// 7DaysToDie_EAC.exe
				// 7dLauncher.exe
				// UnityCrashHandler64.exe

				if (fileInfo.Extension == ExeExtension)
				{
					exeCount++;
					name = Path.GetFileNameWithoutExtension(fileInfo.Name);
					string dataFolder = $"{name}_{DataFolderName}"; // this should be 7DaysToDie_Data
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

		#endregion Static Methods

	}

}
