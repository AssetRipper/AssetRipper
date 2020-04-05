using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal sealed class WebGLGameStructure : PlatformGameStructure
	{
		public WebGLGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if (!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			Dictionary<string, string> files = new Dictionary<string, string>();
			string buildPath = Path.Combine(m_root.FullName, BuildName);
			if (Directory.Exists(buildPath))
			{
				DirectoryInfo buildDirectory = new DirectoryInfo(buildPath);
				foreach (FileInfo file in buildDirectory.EnumerateFiles())
				{
					if (file.Name.EndsWith(DataWebExtension, StringComparison.Ordinal))
					{
						Name = file.Name.Substring(0, file.Name.Length - DataWebExtension.Length);
						files.Add(Name, file.FullName);
						break;
					}
				}
				DataPathes = new string[] { rootPath, buildPath };
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
							files.Add(Name, file.FullName);
							break;
						}
					}
					DataPathes = new string[] { rootPath, developmentPath };
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
								files.Add(Name, file.FullName);
								break;
							}
						}
						DataPathes = new string[] { rootPath, releasePath };
					}
					else
					{
						throw new Exception("Build directory wasn't found");
					}
				}
			}

			if(files.Count == 0)
			{
				throw new Exception("No files were found");
			}

			CollectStreamingAssets(m_root, files);
			Files = files;

			Assemblies = new Dictionary<string, string>();
		}

		public static bool IsWebGLStructure(string path)
		{
			DirectoryInfo root = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!root.Exists)
			{
				return false;
			}

			foreach (FileInfo fi in root.EnumerateFiles())
			{
				if (fi.Extension == HtmlExtension)
				{
					foreach(DirectoryInfo di in root.EnumerateDirectories())
					{
						switch(di.Name)
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
										if(file.Extension == DataGzExtension)
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
										if(file.Name.EndsWith(DataWebExtension, StringComparison.Ordinal))
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

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string DevelopmentName = "Development";
		private const string ReleaseName = "Release";
		private const string BuildName = "Build";

		private const string HtmlExtension = ".html";
		public const string DataExtension = ".data";
		public const string DataGzExtension = ".datagz";
		public const string UnityWebExtension = ".unityweb";
		public const string DataWebExtension = DataExtension + UnityWebExtension;

		private readonly DirectoryInfo m_root;
	}
}