using AssetRipper.Core.Utils;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class SwitchGameStructure : PlatformGameStructure
	{
		public SwitchGameStructure(string rootPath)
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

			if (!GetDataSwitchDirectory(m_root, out string dataPath))
			{
				throw new Exception($"Data directory wasn't found");
			}

#warning TODO: Switch paths
			Name = m_root.Name;
			RootPath = rootPath;
			GameDataPath = dataPath;
			ManagedPath = null;
			UnityPlayerPath = null;
			UnityVersion = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;
			Backend = Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(dataPath);

			CollectGameFiles(dataDirectory, Files);

			CollectMainAssemblies(dataDirectory, Assemblies);
		}

		public static bool IsSwitchStructure(string path)
		{
			DirectoryInfo rootInfo = new DirectoryInfo(path);
			if (!rootInfo.Exists)
			{
				return false;
			}
			if (!Directory.Exists(Path.Combine(rootInfo.FullName, ExecutableName)))
			{
				return false;
			}


			return GetDataSwitchDirectory(rootInfo, out string _);
		}

		private static bool GetDataSwitchDirectory(DirectoryInfo rootDirectory, out string dataPath)
		{
			dataPath = null;
			string romPath = Path.Combine(rootDirectory.FullName, RomName);
			if (!Directory.Exists(romPath))
			{
				return false;
			}

			string ldataPath = Path.Combine(romPath, DataFolderName);
			if (!Directory.Exists(ldataPath))
			{
				return false;
			}

			dataPath = ldataPath;
			return true;
		}

		public override PlatformType Platform => PlatformType.Switch;

		private const string ExecutableName = "exefs";
		private const string RomName = "romfs";
	}
}
