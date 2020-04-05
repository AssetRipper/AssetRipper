using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal sealed class LinuxGameStructure : PlatformGameStructure
	{
		public LinuxGameStructure(string rootPath)
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

			if (!GetDataLinuxDirectory(m_root, out string dataPath, out string name))
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

		public static bool IsLinuxStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!dinfo.Exists)
			{
				return false;
			}
			return IsRootLinuxDirectory(dinfo);
		}

		private static bool IsRootLinuxDirectory(DirectoryInfo rootDiectory)
		{
			return GetDataLinuxDirectory(rootDiectory, out string _, out string _);
		}

		private static bool GetDataLinuxDirectory(DirectoryInfo rootDiectory, out string dataPath, out string name)
		{
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Extension == x86Extension || finfo.Extension == x64Extension)
				{
					name = Path.GetFileNameWithoutExtension(finfo.Name);
					string dataFolder = $"{name}_{DataFolderName}";
					dataPath = Path.Combine(rootDiectory.FullName, dataFolder);
					if (DirectoryUtils.Exists(dataPath))
					{
						return true;
					}
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

		private const string x86Extension = ".x86";
		private const string x64Extension = ".x64";

		private readonly DirectoryInfo m_root;
	}
}
