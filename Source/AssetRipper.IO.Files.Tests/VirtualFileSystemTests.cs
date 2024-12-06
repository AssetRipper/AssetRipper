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
		Assert.Multiple(() =>
		{
			Assert.That(fs.Directory.Exists("/test"), Is.True);
			Assert.That(fs.Count, Is.EqualTo(2));// root and test
		});
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
	public void CreatingFileTwiceThrows()
	{
		Assert.Throws<IOException>(() =>
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
}
