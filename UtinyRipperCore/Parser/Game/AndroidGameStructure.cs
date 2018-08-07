using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper
{
	internal class AndroidGameStructure : PlatformGameStructure
	{
		public AndroidGameStructure(FileCollection collection, string rootPath) :
			this(collection, rootPath, string.Empty)
		{
		}

		public AndroidGameStructure(FileCollection collection, string rootPath, string obbPath) :
			base(collection)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(rootPath);
			if (!m_root.Exists)
			{
				throw new Exception($"Root directory '{rootPath}' doesn't exist");
			}

			m_dataPath = Path.Combine(rootPath, AssetName, BinName, DataName);
			if (!Directory.Exists(m_dataPath))
			{
				throw new Exception($"Data directory hasn't beed found");
			}

			if(obbPath != null)
			{
				m_obbRoot = new DirectoryInfo(obbPath);
				if (!m_obbRoot.Exists)
				{
					throw new Exception($"Obb directory '{obbPath}' doesn't exist");
				}

				m_obbDataPath = Path.Combine(obbPath, AssetName, BinName, DataName);
				if (!Directory.Exists(m_obbDataPath))
				{
					throw new Exception($"Obb data directory hasn't beed found");
				}
			}
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

			string dataPath = Path.Combine(path, AssetName, BinName, DataName);
			if(!Directory.Exists(dataPath))
			{
				return false;
			}

			return true;
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

			string dataPath = Path.Combine(path, AssetName, BinName, DataName);
			if (!Directory.Exists(dataPath))
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
		
		public override IEnumerable<string> FetchFiles()
		{
			foreach(string file in base.FetchFiles())
			{
				yield return file;
			}
			string assetPath = Path.Combine(m_root.FullName, AssetName);
			DirectoryInfo assetDirectory = new DirectoryInfo(assetPath);
			foreach(string assetBundle in FetchGameAssetBundles(assetDirectory))
			{
				yield return assetBundle;
			}
		}

		private IEnumerable<string> FetchGameAssetBundles(DirectoryInfo assetDirectory)
		{
			foreach (FileInfo file in assetDirectory.EnumerateFiles())
			{
				if (file.Extension == AssetBundleExtension)
				{
					yield return file.FullName;
				}
			}

			foreach(DirectoryInfo subDirectory in assetDirectory.EnumerateDirectories())
			{
				if(subDirectory.Name == BinName)
				{
					continue;
				}
				foreach(string assetBundle in FetchAssetBundles(subDirectory))
				{
					yield return assetBundle;
				}
			}
		}

		public override string Name => m_root.Name;
		public override string MainDataPath => m_dataPath;
		public override IEnumerable<string> DataPathes
		{
			get
			{
				if(m_obbDataPath == null)
				{
					return new string[] { m_dataPath };
				}
				else
				{
					return new string[] { m_dataPath, m_obbDataPath };
				}
			}
		}

		private const string AssetName = "assets";
		private const string LibName = "lib";
		private const string MetaName = "META-INF";
		private const string BinName = "bin";
		private const string DataName = "Data";
		
		private readonly DirectoryInfo m_root;
		private readonly DirectoryInfo m_obbRoot;
		private readonly string m_dataPath;
		private readonly string m_obbDataPath;
		private readonly string m_managedPath;
	}
}
