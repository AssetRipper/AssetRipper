using AssetRipper.Core.Utils;
using System;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class WiiUGameStructure : PlatformGameStructure
	{
		public WiiUGameStructure(string rootPath)
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

			Name = m_root.Name;
			RootPath = m_root.FullName;
			GameDataPath = Path.Combine(RootPath, ContentName, DataFolderName);
			if (!Directory.Exists(GameDataPath))
			{
				throw new Exception($"Data directory wasn't found");
			}
			StreamingAssetsPath = Path.Combine(GameDataPath, StreamingName);
			ResourcesPath = Path.Combine(GameDataPath, ResourcesName);
			ManagedPath = Path.Combine(GameDataPath, ManagedName);
			UnityPlayerPath = null;
			UnityVersion = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;

			if (HasIl2CppFiles())
				Backend = Assembly.ScriptingBackend.Il2Cpp;
			else if (HasMonoAssemblies(ManagedPath))
				Backend = Assembly.ScriptingBackend.Mono;
			else
				Backend = Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { GameDataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(GameDataPath);

			CollectGameFiles(dataDirectory, Files);
			CollectStreamingAssets(dataDirectory, Files);
			CollectResources(dataDirectory, Files);
			CollectMainAssemblies(dataDirectory, Assemblies);
		}

		public static bool IsWiiUStructure(string rootPath)
		{
			string gameDataPath = Path.Combine(rootPath, ContentName, DataFolderName);
			return Directory.Exists(gameDataPath);
		}

		public override PlatformType Platform => PlatformType.WiiU;

		private const string ContentName = "content";
	}
}
