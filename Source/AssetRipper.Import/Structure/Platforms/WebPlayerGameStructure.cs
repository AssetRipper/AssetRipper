using AssetRipper.Import.Structure.Assembly;

namespace AssetRipper.Import.Structure.Platforms
{
	internal sealed class WebPlayerGameStructure : PlatformGameStructure
	{
		public WebPlayerGameStructure(string rootPath)
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

			if (!GetWebPlayerName(m_root, out string? name))
			{
				throw new Exception($"Web player asset bundle data wasn't found");
			}

			Name = name;
			RootPath = rootPath;
			GameDataPath = null;
			StreamingAssetsPath = null;
			ResourcesPath = null;
			ManagedPath = null;
			UnityPlayerPath = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;
			Version = null;
			Backend = ScriptingBackend.Unknown;

			DataPaths = new string[] { rootPath };

			string abPath = Path.Combine(m_root.FullName, Name + AssetBundleExtension);
			Files.Add(Name, abPath);
		}

		public static bool IsWebPlayerStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(path);
			if (!dinfo.Exists)
			{
				return false;
			}

			return GetWebPlayerName(dinfo, out _);
		}

		public static bool GetWebPlayerName(DirectoryInfo root, [NotNullWhen(true)] out string? name)
		{
			foreach (FileInfo fi in root.EnumerateFiles())
			{
				if (fi.Extension == HtmlExtension)
				{
					name = fi.Name[..^HtmlExtension.Length];
					string abPath = Path.Combine(root.FullName, name + AssetBundleExtension);
					if (File.Exists(abPath))
					{
						return true;
					}
				}
			}
			name = null;
			return false;
		}

		private const string HtmlExtension = ".html";
	}
}
