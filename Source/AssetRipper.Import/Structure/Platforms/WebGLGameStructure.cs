using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class WebGLGameStructure : PlatformGameStructure
{
	public WebGLGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		string buildPath = FileSystem.Path.Join(rootPath, BuildName);
		if (FileSystem.Directory.Exists(buildPath))
		{
			foreach (string file in FileSystem.Directory.EnumerateFiles(buildPath))
			{
				if (file.EndsWith(DataWebExtension, StringComparison.Ordinal))
				{
					Name = fileSystem.Path.GetFileName(file)[..^DataWebExtension.Length];
					Files.Add(Name, file);
					break;
				}
			}
			DataPaths = [rootPath, buildPath];
		}
		else
		{
			string developmentPath = FileSystem.Path.Join(rootPath, DevelopmentName);
			if (FileSystem.Directory.Exists(developmentPath))
			{
				foreach (string file in FileSystem.Directory.EnumerateFiles(developmentPath))
				{
					if (file.EndsWith(DataExtension, StringComparison.Ordinal))
					{
						Name = fileSystem.Path.GetFileName(file)[..^DataExtension.Length];
						Files.Add(Name, file);
						break;
					}
				}
				DataPaths = [rootPath, developmentPath];
			}
			else
			{
				string releasePath = FileSystem.Path.Join(rootPath, ReleaseName);
				if (FileSystem.Directory.Exists(releasePath))
				{
					foreach (string file in FileSystem.Directory.EnumerateFiles(releasePath))
					{
						if (file.EndsWith(DataGzExtension, StringComparison.Ordinal))
						{
							Name = fileSystem.Path.GetFileName(file)[..^DataGzExtension.Length];
							Files.Add(Name, file);
							break;
						}
					}
					DataPaths = [rootPath, releasePath];
				}
				else
				{
					throw new DirectoryNotFoundException("Build directory wasn't found");
				}
			}
		}

		Name = FileSystem.Path.GetFileName(rootPath);
		GameDataPath = rootPath;
		StreamingAssetsPath = rootPath;
		ResourcesPath = null;
		ManagedPath = null;
		UnityPlayerPath = null;
		Version = null;
		Il2CppGameAssemblyPath = null;
		Il2CppMetaDataPath = null;
		Backend = ScriptingBackend.Unknown;

		if (Files.Count == 0)
		{
			throw new Exception("No files were found");
		}
	}

	public static bool Exists(string root, FileSystem fileSystem)
	{
		if (!fileSystem.Directory.Exists(root))
		{
			return false;
		}

		foreach (string htmlFile in fileSystem.Directory.EnumerateFiles(root))
		{
			if (!htmlFile.EndsWith(HtmlExtension, StringComparison.Ordinal))
			{
				continue;
			}

			foreach (string directory in fileSystem.Directory.EnumerateDirectories(root))
			{
				switch (fileSystem.Path.GetFileName(directory))
				{
					case DevelopmentName:
						{
							foreach (string file in fileSystem.Directory.EnumerateFiles(directory))
							{
								if (file.EndsWith(DataExtension, StringComparison.Ordinal))
								{
									return true;
								}
							}
						}
						break;

					case ReleaseName:
						{
							foreach (string file in fileSystem.Directory.EnumerateFiles(directory))
							{
								if (file.EndsWith(DataGzExtension, StringComparison.Ordinal))
								{
									return true;
								}
							}
						}
						break;

					case BuildName:
						{
							foreach (string file in fileSystem.Directory.EnumerateFiles(directory))
							{
								if (file.EndsWith(DataWebExtension, StringComparison.Ordinal))
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
