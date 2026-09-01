using AssetRipper.Import.Platforms;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;

namespace AssetRipper.Tests;

public class LinuxGameStructureTests
{
	private const string GameName = "Some Game";
	private const string DataFolder = $"{GameName}_Data";

	private static VirtualFileSystem CreateFileSystem(string root, string executableName)
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create(root);
		string dataPath = fs.Path.Join(root, DataFolder);
		fs.Directory.Create(dataPath);
		fs.File.WriteAllBytes(fs.Path.Join(root, "UnityPlayer.so"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(root, executableName), [0]);
		return fs;
	}

	private static PlatformGameStructure? Detect(VirtualFileSystem fs, string inputPath)
	{
		List<string> paths = [inputPath];
		PlatformChecker.CheckPlatform(paths, fs, out PlatformGameStructure? platform, out _);
		return platform;
	}

	[Test]
	public void ExtensionlessExecutableIsDetectedAsLinux()
	{
		const string root = "/game";
		VirtualFileSystem fs = CreateFileSystem(root, GameName); // no extension
		PlatformGameStructure? platform = Detect(fs, root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("LinuxGameStructure"));
			Assert.That(platform.StreamingAssetsPath, Is.EqualTo(fs.Path.Join(root, DataFolder, "StreamingAssets")));
		}
	}

	[Test]
	public void OldX86_64ExecutableIsDetectedAsLinux()
	{
		const string root = "/game";
		VirtualFileSystem fs = CreateFileSystem(root, $"{GameName}.x86_64");
		PlatformGameStructure? platform = Detect(fs, root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("LinuxGameStructure"));
			Assert.That(platform.StreamingAssetsPath, Is.EqualTo(fs.Path.Join(root, DataFolder, "StreamingAssets")));
		}
	}

	[Test]
	public void DataDirectoryGivenDirectlyIsDetectedAsLinux()
	{
		const string root = "/game";
		VirtualFileSystem fs = CreateFileSystem(root, GameName); // no extension
		PlatformGameStructure? platform = Detect(fs, fs.Path.Join(root, DataFolder));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("LinuxGameStructure"));
			Assert.That(platform.StreamingAssetsPath, Is.EqualTo(fs.Path.Join(root, DataFolder, "StreamingAssets")));
		}
	}
}
