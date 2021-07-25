using AssetRipper.Logging;
using AssetRipper.Utils;
using System;
using System.IO;

namespace AssetRipper.Structure.GameStructure.Platforms
{
	internal class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			m_root = new DirectoryInfo(rootPath);
			if (!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			if (!GetDataPCDirectory(m_root, out string dataPath, out string name))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = name;
			RootPath = rootPath;
			GameDataPath = dataPath;
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = Path.Combine(RootPath, "UnityPlayer.dll");
			Il2CppGameAssemblyPath = Path.Combine(RootPath, "GameAssembly.dll");
			Il2CppMetaDataPath = Path.Combine(GameDataPath, "il2cpp_data", "Metadata", "global-metadata.dat");

			if (File.Exists(Il2CppGameAssemblyPath) && File.Exists(UnityPlayerPath))
				Backend = Assembly.ScriptingBackend.Il2Cpp;
			else if (IsMono(ManagedPath))
				Backend = Assembly.ScriptingBackend.Mono;
			else
				Backend = Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			CollectGameFiles(dataDirectory, Files);
			CollectStreamingAssets(dataDirectory, Files);
			CollectMainAssemblies(dataDirectory, Assemblies);
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

		private static bool IsMono(string managedDirectory)
		{
			if (string.IsNullOrEmpty(managedDirectory) || !Directory.Exists(managedDirectory)) return false;
			
			return Directory.GetFiles(managedDirectory, "*.dll").Length > 0;
		}

		public override PlatformType Platform => PlatformType.PC;

		private const string ExeExtension = ".exe";

		private readonly DirectoryInfo m_root;
	}
}
