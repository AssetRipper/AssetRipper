using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal class AndroidGameStructure : PlatformGameStructure
	{
		public AndroidGameStructure(string rootPath) :
			this(rootPath, string.Empty)
		{
		}

		public AndroidGameStructure(string rootPath, string obbPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if (!m_root.Exists)
			{
				throw new Exception($"Root directory '{rootPath}' doesn't exist");
			}

			string apkDataPath = Path.Combine(rootPath, AssetName, BinName, DataFolderName);
			DirectoryInfo apkDataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(apkDataPath));
			if (!apkDataDirectory.Exists)
			{
				throw new Exception($"Data directory hasn't been found");
			}
			List<string> dataPathes = new List<string>() { apkDataPath };

			DirectoryInfo obbDataDirectory = null;
			if (obbPath != null)
			{
				m_obbRoot = new DirectoryInfo(DirectoryUtils.ToLongPath(obbPath));
				if (!m_obbRoot.Exists)
				{
					throw new Exception($"Obb directory '{obbPath}' doesn't exist");
				}

				string obbDataPath = Path.Combine(obbPath, AssetName, BinName, DataFolderName);
				if (!DirectoryUtils.Exists(obbDataPath))
				{
					throw new Exception($"Obb data directory '{obbDataPath}' wasn't found");
				}
				dataPathes.Add(obbDataPath);
			}
			DataPathes = dataPathes.ToArray();

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(apkDataDirectory, files);
			if (obbDataDirectory != null)
			{
				CollectGameFiles(obbDataDirectory, files);
			}
			CollectApkAssetBundles(files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(apkDataDirectory, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsAndroidStructure(string path)
		{
			DirectoryInfo directory = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
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
			if (!DirectoryUtils.Exists(dataPath))
			{
				return false;
			}

			return true;
		}

		public static bool IsAndroidObbStructure(string path)
		{
			DirectoryInfo directory = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
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
			if (!DirectoryUtils.Exists(dataPath))
			{
				return false;
			}

			return true;
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

		private void CollectApkAssetBundles(IDictionary<string, string> files)
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
				CollectAssetBundles(subDirectory, files);
			}
		}

		public override string Name => m_root.Name;
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string AssetName = "assets";
		private const string MetaName = "META-INF";
		private const string BinName = "bin";

		private readonly DirectoryInfo m_root;
		private readonly DirectoryInfo m_obbRoot;
	}
}
