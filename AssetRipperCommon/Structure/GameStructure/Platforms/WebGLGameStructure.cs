using System;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class WebGLGameStructure : PlatformGameStructure
	{
		public WebGLGameStructure(string rootPath)
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

			string buildPath = Path.Combine(m_root.FullName, BuildName);
			if (Directory.Exists(buildPath))
			{
				DirectoryInfo buildDirectory = new DirectoryInfo(buildPath);
				foreach (FileInfo file in buildDirectory.EnumerateFiles())
				{
					if (file.Name.EndsWith(DataWebExtension, StringComparison.Ordinal))
					{
						Name = file.Name.Substring(0, file.Name.Length - DataWebExtension.Length);
						Files.Add(Name, file.FullName);
						break;
					}
				}
				DataPaths = new string[] { rootPath, buildPath };
			}
			else
			{
				string developmentPath = Path.Combine(m_root.FullName, DevelopmentName);
				if (Directory.Exists(developmentPath))
				{
					DirectoryInfo buildDirectory = new DirectoryInfo(developmentPath);
					foreach (FileInfo file in buildDirectory.EnumerateFiles())
					{
						if (file.Extension == DataExtension)
						{
							Name = file.Name.Substring(0, file.Name.Length - DataExtension.Length);
							Files.Add(Name, file.FullName);
							break;
						}
					}
					DataPaths = new string[] { rootPath, developmentPath };
				}
				else
				{
					string releasePath = Path.Combine(m_root.FullName, ReleaseName);
					if (Directory.Exists(releasePath))
					{
						DirectoryInfo buildDirectory = new DirectoryInfo(releasePath);
						foreach (FileInfo file in buildDirectory.EnumerateFiles())
						{
							if (file.Extension == DataGzExtension)
							{
								Name = file.Name.Substring(0, file.Name.Length - DataGzExtension.Length);
								Files.Add(Name, file.FullName);
								break;
							}
						}
						DataPaths = new string[] { rootPath, releasePath };
					}
					else
					{
						throw new Exception("Build directory wasn't found");
					}
				}
			}

			Name = m_root.Name;
			RootPath = rootPath;
			GameDataPath = rootPath;
			StreamingAssetsPath = rootPath;
			ResourcesPath = null;
			ManagedPath = null;
			UnityPlayerPath = null;
			UnityVersion = null;
			Il2CppGameAssemblyPath = null;
			Il2CppMetaDataPath = null;
			Backend = Assembly.ScriptingBackend.Unknown;

			if (Files.Count == 0)
			{
				throw new Exception("No files were found");
			}
		}

		public static bool IsWebGLStructure(string path)
		{
			DirectoryInfo root = new DirectoryInfo(path);
			if (!root.Exists)
			{
				return false;
			}

			foreach (FileInfo fi in root.EnumerateFiles())
			{
				if (fi.Extension == HtmlExtension)
				{
					foreach (DirectoryInfo di in root.EnumerateDirectories())
					{
						switch (di.Name)
						{
							case DevelopmentName:
								{
									foreach (FileInfo file in di.EnumerateFiles())
									{
										if (file.Extension == DataExtension)
										{
											return true;
										}
									}
								}
								break;

							case ReleaseName:
								{
									foreach (FileInfo file in di.EnumerateFiles())
									{
										if (file.Extension == DataGzExtension)
										{
											return true;
										}
									}
								}
								break;

							case BuildName:
								{
									foreach (FileInfo file in di.EnumerateFiles())
									{
										if (file.Name.EndsWith(DataWebExtension, StringComparison.Ordinal))
										{
											return true;
										}
									}
								}
								break;
						}
					}

					return false;
				}
			}
			return false;
		}

		private const string DevelopmentName = "Development";
		private const string ReleaseName = "Release";
		private const string BuildName = "Build";

		private const string HtmlExtension = ".html";
		public const string DataExtension = ".data";
		public const string DataGzExtension = ".datagz";
		public const string UnityWebExtension = ".unityweb";
		public const string DataWebExtension = DataExtension + UnityWebExtension;
	}
}
