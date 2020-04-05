using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal sealed class MacGameStructure : PlatformGameStructure
	{
		public MacGameStructure(string rootPath)
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

			Name = m_root.Name.Substring(0, m_root.Name.Length - AppExtension.Length);

			string dataPath = Path.Combine(m_root.FullName, ContentsName, DataFolderName);
			if (!Directory.Exists(dataPath))
			{
				throw new Exception("Data directory wasn't found");
			}
			string resourcePath = Path.Combine(m_root.FullName, ContentsName, ResourcesName);
			if (!Directory.Exists(resourcePath))
			{
				throw new Exception("Resources directory wasn't found");
			}
			DataPathes = new string[] { dataPath, resourcePath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(dataDirectory, files);
			CollectStreamingAssets(dataDirectory, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(dataDirectory, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsMacStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!dinfo.Exists)
			{
				return false;
			}
			if (!dinfo.Name.EndsWith(AppExtension, StringComparison.Ordinal))
			{
				return false;
			}

			string dataPath = Path.Combine(dinfo.FullName, ContentsName, DataFolderName);
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

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string ContentsName = "Contents";
		private const string AppExtension = ".app";

		private readonly DirectoryInfo m_root;
	}
}
