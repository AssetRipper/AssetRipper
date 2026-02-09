namespace AssetRipper.IO.Files.Tests;

public class VirtualFileSystemTests
{
	[Test]
	public void StartsWithRoot()
	{
		Assert.That(new VirtualFileSystem().Count, Is.EqualTo(1));
	}

	[Test]
	public void RootExists()
	{
		Assert.That(new VirtualFileSystem().Directory.Exists("/"), Is.True);
	}

	[Test]
	public void EmptyRootExists()
	{
		Assert.That(new VirtualFileSystem().Directory.Exists(""), Is.True);
	}

	[Test]
	public void NullRootExists()
	{
		Assert.That(new VirtualFileSystem().Directory.Exists(null), Is.True);
	}

	[Test]
	public void NonexistentDirectoryDoesNotExist()
	{
		Assert.That(new VirtualFileSystem().Directory.Exists("/test"), Is.False);
	}

	[Test]
	public void CreateRootedDirectorySucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		Assert.That(fs.Directory.Exists("/test"), Is.True);
	}

	[Test]
	public void CreateNonrootedDirectorySucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("test");
		Assert.That(fs.Directory.Exists("test"), Is.True);
	}

	[Test]
	public void NonrootDirectoryPathsAreValidInputToExists()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		Assert.That(fs.Directory.Exists("test"), Is.True);
	}

	[Test]
	public void CreatingDirectoryTwiceDoesNotThrow()
	{
		Assert.DoesNotThrow(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.Directory.Create("/test");
		});
	}

	[Test]
	public void CreatingChildDirectoryAfterCreatingParentSucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		fs.Directory.Create("/test/child");
		Assert.That(fs.Directory.Exists("/test/child"), Is.True);
	}

	[Test]
	public void CreatingChildDirectoryWithoutCreatingParentSucceeds()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child");
		Assert.That(fs.Directory.Exists("/test/child"), Is.True);
	}

	[Test]
	public void CreatingChildDirectoryCreatesParent()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child");
		Assert.That(fs.Directory.Exists("/test"), Is.True);
	}

	[Test]
	public void CreatingChildDirectoryCreatesGrandparent()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/child/grandchild");
		Assert.That(fs.Directory.Exists("/test"), Is.True);
	}

	[Test]
	public void TrailingSlashIsIgnoredDuringCreation()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test/");
		using (Assert.EnterMultipleScope())
		{
			Assert.That(fs.Directory.Exists("/test"), Is.True);
			Assert.That(fs.Count, Is.EqualTo(2));// root and test
		}
	}

	[Test]
	public void TrailingSlashIsIgnoredDuringExistenceCheck()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/test");
		Assert.That(fs.Directory.Exists("/test/"), Is.True);
	}

	[Test]
	public void CreatingFileInNonexistentDirectoryThrows()
	{
		Assert.Throws<DirectoryNotFoundException>(() =>
		{
			VirtualFileSystem fs = new();
			fs.File.Create("/test/file");
		});
	}

	[Test]
	public void CreatingFileInExistentDirectorySucceeds()
	{
		Assert.DoesNotThrow(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.File.Create("/test/file");
		});
	}

	[Test]
	public void CreatingFileTwiceSucceeds()
	{
		Assert.DoesNotThrow(() =>
		{
			VirtualFileSystem fs = new();
			fs.Directory.Create("/test");
			fs.File.Create("/test/file");
			fs.File.Create("/test/file");
		});
	}

	[Test]
	public void CreatingRootedFileSucceeds()
	{
		Assert.DoesNotThrow(() =>
		{
			VirtualFileSystem fs = new();
			fs.File.Create("/test");
		});
	}

	[Test]
	public void NewlyCreatedFileHasLengthZero()
	{
		VirtualFileSystem fs = new();
		Stream stream = fs.File.Create("/test");
		Assert.That(stream.Length, Is.Zero);
	}

	[Test]
	public void ReadWriteTextParity()
	{
		VirtualFileSystem fs = new();
		string path = "/test";
		string contents = "Hello, world!";
		fs.File.WriteAllText(path, contents);
		string readContents = fs.File.ReadAllText(path);
		Assert.That(readContents, Is.EqualTo(contents));
	}

	[Test]
	public void ReadWriteBytesParity()
	{
		VirtualFileSystem fs = new();
		string path = "/test";
		byte[] bytes = [0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x20, 0x77, 0x6F, 0x72, 0x6C, 0x64, 0x21];
		fs.File.WriteAllBytes(path, bytes);
		byte[] readBytes = fs.File.ReadAllBytes(path);
		Assert.That(readBytes, Is.EqualTo(bytes));
	}

	[Test]
	public void EnumerateFilesTopLevelDoesNotFindDeepResults()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/dir");
		fs.File.Create("/dir/deep");
		fs.File.Create("/test");
		IEnumerable<string> files = fs.Directory.EnumerateFiles("/", "*", SearchOption.TopDirectoryOnly);
		Assert.That(files, Is.EquivalentTo(["/test"]));
	}

	[Test]
	public void EnumerateFilesRecursiveDoesFindDeepResults()
	{
		VirtualFileSystem fs = new();
		fs.Directory.Create("/dir");
		fs.File.Create("/dir/deep");
		fs.File.Create("/test");
		IEnumerable<string> files = fs.Directory.EnumerateFiles("/", "*", SearchOption.AllDirectories);
		Assert.That(files, Is.EquivalentTo(["/dir/deep", "/test"]));
	}
}
