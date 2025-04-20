﻿namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class SwitchGameStructure : PlatformGameStructure
	{
		private const string ExefsName = "exefs";
		private const string RomName = "romfs";
		private const string MainName = "main";

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

			if (!GetDataSwitchDirectory(m_root, out string? dataPath))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = m_root.Name;
			RootPath = rootPath;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Join(dataPath, StreamingName);
			ResourcesPath = Path.Join(dataPath, ResourcesName);
			ManagedPath = Path.Join(dataPath, ManagedName);
			UnityPlayerPath = null;
			Version = GetUnityVersionFromDataDirectory(dataPath);
			Il2CppGameAssemblyPath = Path.Join(rootPath, ExefsName, MainName);
			Il2CppMetaDataPath = Path.Join(ManagedPath, MetadataName, DefaultGlobalMetadataName);
			Backend = HasIl2CppFiles() ? Assembly.ScriptingBackend.IL2Cpp : Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { dataPath };
		}

		public static bool IsSwitchStructure(string path)
		{
			DirectoryInfo rootInfo = new DirectoryInfo(path);
			if (!rootInfo.Exists)
			{
				return false;
			}
			if (!Directory.Exists(Path.Join(rootInfo.FullName, ExefsName)))
			{
				return false;
			}

			return GetDataSwitchDirectory(rootInfo, out _);
		}

		private static bool GetDataSwitchDirectory(DirectoryInfo rootDirectory, [NotNullWhen(true)] out string? dataPath)
		{
			dataPath = null;
			string romPath = Path.Join(rootDirectory.FullName, RomName);
			if (!Directory.Exists(romPath))
			{
				return false;
			}

			string ldataPath = Path.Join(romPath, DataFolderName);
			if (!Directory.Exists(ldataPath))
			{
				return false;
			}

			dataPath = ldataPath;
			return true;
		}
	}
}
