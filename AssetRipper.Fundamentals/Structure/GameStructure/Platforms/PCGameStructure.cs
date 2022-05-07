﻿using AssetRipper.Core.Logging;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			if (IsExecutableFile(rootPath))
			{
				Logger.Info(LogCategory.Import, "PC executable found. Setting root to parent directory");
				m_root = (new FileInfo(rootPath)).Directory;
			}
			else if (IsUnityDataDirectory(rootPath))
			{
				Logger.Info(LogCategory.Import, "PC data directory found. Setting root to parent directory");
				m_root = (new DirectoryInfo(rootPath)).Parent;
			}
			else
			{
				m_root = new DirectoryInfo(rootPath);
				if (!m_root.Exists)
				{
					throw new Exception($"Directory '{rootPath}' doesn't exist");
				}
			}

			if (!GetDataPCDirectory(m_root, out string dataPath, out string name))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = name;
			RootPath = m_root.FullName;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Combine(GameDataPath, StreamingName);
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = Path.Combine(RootPath, DefaultUnityPlayerName);
			UnityVersion = null;
			Il2CppGameAssemblyPath = Path.Combine(RootPath, DefaultGameAssemblyName);
			Il2CppMetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", MetadataName, DefaultGlobalMetadataName);

			if (HasIl2CppFiles())
				Backend = Assembly.ScriptingBackend.Il2Cpp;
			else if (HasMonoAssemblies(ManagedPath))
				Backend = Assembly.ScriptingBackend.Mono;
			else
				Backend = Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { dataPath };
		}

		public static bool IsPCStructure(string path)
		{
			DirectoryInfo dinfo;
			if (IsExecutableFile(path))
				dinfo = (new FileInfo(path)).Directory;
			else if (IsUnityDataDirectory(path))
				return true;
			else
				dinfo = new DirectoryInfo(path);

			if (!dinfo.Exists)
				return false;
			else
				return IsRootPCDirectory(dinfo);
		}

		private static bool IsUnityDataDirectory(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath) || !folderPath.EndsWith($"_{DataFolderName}"))
				return false;

			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			if (!directoryInfo.Exists || directoryInfo.Parent == null)
				return false;

			string folderName = directoryInfo.Name;
			string gameName = folderName.Substring(0, folderName.IndexOf($"_{DataFolderName}"));
			string rootPath = directoryInfo.Parent.FullName;
			if (File.Exists(Path.Combine(rootPath, gameName + ExeExtension)))
				return true;
			else
				return false;
		}

		private static bool IsExecutableFile(string filePath)
		{
			return !string.IsNullOrEmpty(filePath) && filePath.EndsWith(ExeExtension) && File.Exists(filePath);
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
					if (Directory.Exists(dataPath))
					{
						return true;
					}
				}
			}

			if (exeCount > 0)
			{
				name = exeCount == 1 ? name : rootDiectory.Name;
				dataPath = Path.Combine(rootDiectory.FullName, DataFolderName);
				if (Directory.Exists(dataPath))
				{
					return true;
				}
			}

			name = null;
			dataPath = null;
			return false;
		}


		private const string ExeExtension = ".exe";
	}
}
