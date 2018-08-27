using System;
using System.Collections.Generic;
using System.IO;

namespace UtinyRipper
{
	internal class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(FileCollection collection, string rootPath):
			base(collection)
		{
			if(string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if(!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			string dataPath = GetDataPCDirectory(m_root);
			if (dataPath == null)
			{
				throw new Exception($"Data directory hasn't been found");
			}
			DataPathes = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(dataDirectory, files);
			CollectStreamingAssets(dataDirectory, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(dataDirectory, assemblies);
			Assemblies = assemblies;
			SetScriptingBackend();
		}

		public static bool IsPCStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if(!dinfo.Exists)
			{
				return false;
			}
			return IsRootPCDirectory(dinfo);
		}

		private static bool IsRootPCDirectory(DirectoryInfo rootDiectory)
		{
			return GetDataPCDirectory(rootDiectory) != null;
		}
		
		private static string GetDataPCDirectory(DirectoryInfo rootDiectory)
		{
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Extension == ExeExtension)
				{
					string exeName = Path.GetFileNameWithoutExtension(finfo.Name);
					string dataFolder = $"{exeName}_{DataPostfix}";
					string dataDirectory = Path.Combine(rootDiectory.FullName, dataFolder);
					if (DirectoryUtils.Exists(dataDirectory))
					{
						return dataDirectory;
					}

					dataDirectory = Path.Combine(rootDiectory.FullName, DataPostfix);
					if (DirectoryUtils.Exists(dataDirectory))
					{
						return dataDirectory;
					}
				}
			}
			return null;
		}

		public override string Name => m_root.Name;
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string DataPostfix = "Data";
		private const string ResourcesName = "Resources";

		private const string ExeExtension = ".exe";

		private readonly DirectoryInfo m_root;
	}
}
