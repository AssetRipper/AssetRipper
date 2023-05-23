using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
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
			Version = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;
			//WiiU doesn't support IL2Cpp
			//See https://docs.unity3d.com/2017.4/Documentation/Manual/ScriptingRestrictions.html

			if (HasMonoAssemblies(ManagedPath))
			{
				Backend = ScriptingBackend.Mono;
			}
			else
			{
				Backend = ScriptingBackend.Unknown;
			}

			DataPaths = new string[] { GameDataPath };
		}

		public static bool IsWiiUStructure(string rootPath)
		{
			string gameDataPath = Path.Combine(rootPath, ContentName, DataFolderName);
			return Directory.Exists(gameDataPath);
		}

		private const string ContentName = "content";
	}
}
