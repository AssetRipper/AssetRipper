using AssetRipper.Import.Structure.Assembly;
using AssetRipper.IO.Files;
using System.Diagnostics;

namespace AssetRipper.Import.Structure.Platforms;

internal sealed class WebPlayerGameStructure : PlatformGameStructure
{
	public WebPlayerGameStructure(string rootPath, FileSystem fileSystem) : base(rootPath, fileSystem)
	{
		Debug.Assert(RootPath is not null);

		if (!GetWebPlayerName(RootPath, FileSystem, out string? name))
		{
			throw new Exception($"Web player asset bundle data wasn't found");
		}

		Name = name;
		GameDataPath = null;
		StreamingAssetsPath = null;
		ResourcesPath = null;
		ManagedPath = null;
		UnityPlayerPath = null;
		Il2CppGameAssemblyPath = null;
		Il2CppMetaDataPath = null;
		Version = null;
		Backend = ScriptingBackend.Unknown;

		DataPaths = [RootPath];

		string assetBundlePath = fileSystem.Path.Join(RootPath, Name + AssetBundleExtension);
		Files.Add(Name, assetBundlePath);
	}

	public static bool Exists(string path, FileSystem fileSystem)
	{
		return fileSystem.Directory.Exists(path) && GetWebPlayerName(path, fileSystem, out _);
	}

	public static bool GetWebPlayerName(string root, FileSystem fileSystem, [NotNullWhen(true)] out string? name)
	{
		foreach (string file in fileSystem.Directory.EnumerateFiles(root))
		{
			if (fileSystem.Path.GetExtension(file) == HtmlExtension)
			{
				name = fileSystem.Path.GetFileNameWithoutExtension(file);
				string assetBundlePath = fileSystem.Path.Join(root, name + AssetBundleExtension);
				if (fileSystem.File.Exists(assetBundlePath))
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
