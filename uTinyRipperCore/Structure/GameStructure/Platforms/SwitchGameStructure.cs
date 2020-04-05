using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal sealed class SwitchGameStructure : PlatformGameStructure
	{
		public SwitchGameStructure(string rootPath)
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

			if (!GetDataSwitchDirectory(m_root, out string dataPath))
			{
				throw new Exception($"Data directory wasn't found");
			}

			DataPathes = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(dataDirectory, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(dataDirectory, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsSwitchStructure(string path)
		{
			DirectoryInfo rootInfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!rootInfo.Exists)
			{
				return false;
			}
			if (!DirectoryUtils.Exists(Path.Combine(rootInfo.FullName, ExecutableName)))
			{
				return false;
			}


			return GetDataSwitchDirectory(rootInfo, out string _);
		}

		private static bool GetDataSwitchDirectory(DirectoryInfo rootDirectory, out string dataPath)
		{
			dataPath = null;
			string romPath = Path.Combine(rootDirectory.FullName, RomName);
			if (!DirectoryUtils.Exists(romPath))
			{
				return false;
			}

			string ldataPath = Path.Combine(romPath, DataFolderName);
			if (!DirectoryUtils.Exists(ldataPath))
			{
				return false;
			}

			dataPath = ldataPath;
			return true;
		}

		public override string Name => m_root.Name;
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string ExecutableName = "exefs";
		private const string RomName = "romfs";

		private readonly DirectoryInfo m_root;
	}
}
