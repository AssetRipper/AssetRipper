using AssetRipper.Core.Utils;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class LinuxGameStructure : PlatformGameStructure
	{
		public LinuxGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
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
			RootPath = DirectoryUtils.ToLongPath(rootPath);
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

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			CollectGameFiles(dataDirectory, Files);
			CollectStreamingAssets(dataDirectory, Files);
			CollectResources(dataDirectory, Files);
			CollectMainAssemblies(dataDirectory, Assemblies);
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
				if (finfo.Extension == x86Extension || finfo.Extension == x64Extension || finfo.Extension == x86_64Extension)
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

		public override PlatformType Platform => PlatformType.Linux;

		private const string x86Extension = ".x86";
		private const string x64Extension = ".x64";
		private const string x86_64Extension = ".x86_64";

		private readonly DirectoryInfo m_root;
	}
}
