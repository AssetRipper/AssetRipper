using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if (!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			if (!GetDataPCDirectory(m_root, out string dataPath, out string name))
			{
				throw new Exception($"Data directory wasn't found");
			}
			Name = name;
			DataPathes = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(dataDirectory, files);
			CollectStreamingAssets(dataDirectory, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(dataDirectory, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsPCStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!dinfo.Exists)
			{
				return false;
			}
			return IsRootPCDirectory(dinfo);
		}

		private static bool IsRootPCDirectory(DirectoryInfo rootDiectory)
		{
			return GetDataPCDirectory(rootDiectory, out string _, out string _);
		}

		private static bool GetDataPCDirectory(DirectoryInfo rootDiectory, out string dataPath, out string name)
		{
			name = null;
			int exeCount = 0;
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Extension == ExeExtension)
				{
					exeCount++;
					name = Path.GetFileNameWithoutExtension(finfo.Name);
					string dataFolder = $"{name}_{DataFolderName}";
					dataPath = Path.Combine(rootDiectory.FullName, dataFolder);
					if (DirectoryUtils.Exists(dataPath))
					{
						return true;
					}
				}
			}

			if (exeCount > 0)
			{
				name = exeCount == 1 ? name : rootDiectory.Name;
				dataPath = Path.Combine(rootDiectory.FullName, DataFolderName);
				if (DirectoryUtils.Exists(dataPath))
				{
					return true;
				}
			}

			name = null;
			dataPath = null;
			return false;
		}

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string ExeExtension = ".exe";

		private readonly DirectoryInfo m_root;
	}
}
