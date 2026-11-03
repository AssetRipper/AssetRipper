namespace AssetRipper.IO.Files.Tests;

public class VirtualFileSystemTests
{
	[Test]
	public async Task StartsWithRoot()
	{
		await Assert.That(new VirtualFileSystem().Count).IsEqualTo(1);
	}

	[Test]
	public async Task RootExists()
	{
		await Assert.That(new VirtualFileSystem().Directory.Exists("/")).IsTrue();
	}

	[Test]
	public async Task EmptyRootExists()
	{
		await Assert.That(new VirtualFileSystem().Directory.Exists("")).IsTrue();
	}

	[Test]
	public async Task NullRootExists()
	{
		await Assert.That(new VirtualFileSystem().Directory.Exists(null)).IsTrue();
	}

	[Test]
	[Arguments("")]
	[Arguments("/")]
	[Arguments(null)]
	public async Task FileDoesNotExistAtRoot(string? path)
	{
		await Assert.That(new VirtualFileSystem().File.Exists(path)).IsFalse();
	}

	[Test]
	public async Task NonexistentDirectoryDoesNotExist()
	{
		await Assert.That(new VirtualFileSystem().Directory.Exists("/test")).IsFalse();
	}

	[Test]
	public async Task CreateRootedDirectorySucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		await Assert.That(fs.Directory.Exists("/test")).IsTrue();
	}

	[Test]
	public async Task CreateNonrootedDirectorySucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("test");
		await Assert.That(fs.Directory.Exists("test")).IsTrue();
	}

	[Test]
	public async Task NonrootDirectoryPathsAreValidInputToExists()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		await Assert.That(fs.Directory.Exists("test")).IsTrue();
	}

	[Test]
	public async Task CreatingDirectoryTwiceDoesNotThrow()
	{
		await Assert.That(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.Directory.Create("/test");
		}).ThrowsNothing();
	}

	[Test]
	public async Task CreatingChildDirectoryAfterCreatingParentSucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		fs.Directory.Create("/test/child");
		await Assert.That(fs.Directory.Exists("/test/child")).IsTrue();
	}

	[Test]
	public async Task CreatingChildDirectoryWithoutCreatingParentSucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child");
		await Assert.That(fs.Directory.Exists("/test/child")).IsTrue();
	}

	[Test]
	public async Task CreatingChildDirectoryCreatesParent()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child");
		await Assert.That(fs.Directory.Exists("/test")).IsTrue();
	}

	[Test]
	public async Task CreatingChildDirectoryCreatesGrandparent()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child/grandchild");
		await Assert.That(fs.Directory.Exists("/test")).IsTrue();
	}

	[Test]
	public async Task TrailingSlashIsIgnoredDuringCreation()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/");

		{
			await Assert.That(fs.Directory.Exists("/test")).IsTrue();
			await Assert.That(fs.Count).IsEqualTo(2);// root and test
		}
	}

	[Test]
	public async Task TrailingSlashIsIgnoredDuringExistenceCheck()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		await Assert.That(fs.Directory.Exists("/test/")).IsTrue();
	}

	[Test]
	public async Task CreatingFileInNonexistentDirectoryThrows()
	{
		await Assert.ThrowsAsync<DirectoryNotFoundException>(() => Task.Run(() =>
		{
			VirtualFileSystem fs = new();
			fs.File.Create("/test/file");
		}));
	}

	[Test]
	public async Task CreatingFileInExistentDirectorySucceeds()
	{
		await Assert.That(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.File.Create("/test/file");
		}).ThrowsNothing();
	}

	[Test]
	public async Task CreatingFileTwiceSucceeds()
	{
		await Assert.That(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.File.Create("/test/file");
			fs.File.Create("/test/file");
		}).ThrowsNothing();
	}

	[Test]
	public async Task CreatingRootedFileSucceeds()
	{
		await Assert.That(() =>
		{
			VirtualFileSystem fs = new();
			fs.File.Create("/test");
		}).ThrowsNothing();
	}

	[Test]
	public async Task NewlyCreatedFileHasLengthZero()
	{
		VirtualFileSystem fs = new();
		Stream stream = fs.File.Create("/test");
		await Assert.That(stream.Length).IsZero();
	}

	[Test]
	public async Task ReadWriteTextParity()
	{
		VirtualFileSystem fs = new();
		string path = "/test";
		string contents = "Hello, world!";
		fs.File.WriteAllText(path, contents);
		string readContents = fs.File.ReadAllText(path);
		await Assert.That(readContents).IsEqualTo(contents);
	}

	[Test]
	public async Task ReadWriteBytesParity()
	{
		VirtualFileSystem fs = new();
		string path = "/test";
		byte[] bytes = [0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x21];
		fs.File.WriteAllBytes(path, bytes);
		byte[] readBytes = fs.File.ReadAllBytes(path);
		await Assert.That(readBytes.SequenceEqual(bytes)).IsTrue();
	}

	[Test]
	public async Task EnumerateFilesTopLevelDoesNotFindDeepResults()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/dir");
		fs.File.Create("/dir/deep");
		fs.File.Create("/test");
		IEnumerable<string> files = fs.Directory.EnumerateFiles("/", "*", SearchOption.TopDirectoryOnly);
		await Assert.That(files.SequenceEqual(["/test"])).IsTrue();
	}

	[Test]
	public async Task EnumerateFilesRecursiveDoesFindDeepResults()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/dir");
		fs.File.Create("/dir/deep");
		fs.File.Create("/test");
		IEnumerable<string> files = fs.Directory.EnumerateFiles("/", "*", SearchOption.AllDirectories);
		await Assert.That(files.SequenceEqual(["/test", "/dir/deep"])).IsTrue();
	}
}
