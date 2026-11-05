using AssetRipper.Import.Platforms;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.IO.Files;

namespace AssetRipper.Tests;

public class MetroGameStructureTests
{
	private const string Root = "/package";

	/// <summary>
	/// Dumped Windows Store package: managed assemblies in the root, serialized files in "Data".
	/// </summary>
	private static VirtualFileSystem CreateMetroFileSystem(bool includeManifest)
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create(Root);
		fs.Directory.Create(fs.Path.Join(Root, "Data"));
		fs.File.WriteAllBytes(fs.Path.Join(Root, "Template.exe"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(Root, "UnityPlayer.dll"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(Root, "Assembly-CSharp.dll"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(Root, "UnityEngine.dll"), [0]);
		if (includeManifest)
		{
			fs.File.WriteAllBytes(fs.Path.Join(Root, "AppxManifest.xml"), [0]);
		}
		return fs;
	}

	private static PlatformGameStructure? Detect(VirtualFileSystem fs, string inputPath)
	{
		List<string> paths = [inputPath];
		PlatformChecker.CheckPlatform(paths, fs, out PlatformGameStructure? platform, out _);
		return platform;
	}

	[Test]
	public void MetroPackageUsesRootAssembliesInsteadOfDataManaged()
	{
		VirtualFileSystem fs = CreateMetroFileSystem(includeManifest: true);
		PlatformGameStructure? platform = Detect(fs, Root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("MetroGameStructure"));
			Assert.That(platform.ManagedPath, Is.EqualTo(Root));
			Assert.That(platform.GameDataPath, Is.EqualTo(fs.Path.Join(Root, "Data")));
			Assert.That(platform.Backend, Is.EqualTo(ScriptingBackend.Mono));
		}

		platform.CollectFiles(skipStreamingAssets: true);
		Assert.That(platform.RequestAssembly("Assembly-CSharp"), Is.EqualTo(fs.Path.Join(Root, "Assembly-CSharp.dll")));
	}

	[Test]
	public void MetroPackageWithoutManifestIsStillDetected()
	{
		// A dump may have lost AppxManifest.xml. Root assemblies and no Data/Managed are enough.
		VirtualFileSystem fs = CreateMetroFileSystem(includeManifest: false);
		PlatformGameStructure? platform = Detect(fs, Root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("MetroGameStructure"));
			Assert.That(platform.ManagedPath, Is.EqualTo(Root));
		}
	}

	[Test]
	public void ExecutableGivenDirectlyIsDetectedAsMetro()
	{
		VirtualFileSystem fs = CreateMetroFileSystem(includeManifest: true);
		PlatformGameStructure? platform = Detect(fs, fs.Path.Join(Root, "Template.exe"));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("MetroGameStructure"));
			Assert.That(platform.RootPath, Is.EqualTo(Root));
		}
	}

	[Test]
	public void DataDirectoryGivenDirectlyIsDetectedAsMetro()
	{
		VirtualFileSystem fs = CreateMetroFileSystem(includeManifest: true);
		PlatformGameStructure? platform = Detect(fs, fs.Path.Join(Root, "Data"));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("MetroGameStructure"));
			Assert.That(platform.RootPath, Is.EqualTo(Root));
		}
	}

	[Test]
	public void StandardWindowsGameIsNotDetectedAsMetro()
	{
		const string root = "/game";
		VirtualFileSystem fs = new();
		fs.Directory.Create(fs.Path.Join(root, "Some Game_Data", "Managed"));
		fs.File.WriteAllBytes(fs.Path.Join(root, "Some Game.exe"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(root, "UnityPlayer.dll"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(root, "Some Game_Data", "Managed", "Assembly-CSharp.dll"), [0]);

		PlatformGameStructure? platform = Detect(fs, root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("WindowsGameStructure"));
			Assert.That(platform.ManagedPath, Is.EqualTo(fs.Path.Join(root, "Some Game_Data", "Managed")));
		}
	}

	[Test]
	public void WindowsGameWithPlainDataFolderAndManagedStaysWindows()
	{
		// Old standalone layout: "Data/Managed" exists, so the root DLLs must not hijack detection.
		const string root = "/game";
		VirtualFileSystem fs = new();
		fs.Directory.Create(fs.Path.Join(root, "Data", "Managed"));
		fs.File.WriteAllBytes(fs.Path.Join(root, "Game.exe"), [0]);
		fs.File.WriteAllBytes(fs.Path.Join(root, "UnityEngine.dll"), [0]);

		PlatformGameStructure? platform = Detect(fs, root);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(platform, Is.Not.Null);
			Assert.That(platform!.GetType().Name, Is.EqualTo("WindowsGameStructure"));
		}
	}
}
