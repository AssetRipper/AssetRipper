namespace AssetRipper.IO.Files.Tests;

public static class GetUniqueNameTests
{
	[Test]
	public static void FilenameTruncationMultibyteCharacter()
	{
		using (Assert.EnterMultipleScope())
		{
			// A        length 3 cont     cont     cont
			// 01000001 11100110 10010110 10000111 00001010
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 4), Is.EqualTo(".ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 5), Is.EqualTo("A.ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 6), Is.EqualTo("A.ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 7), Is.EqualTo("A.ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 8), Is.EqualTo("A文.ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 9), Is.EqualTo("A文.ext"));
		}
	}

	[Test]
	public static void ExtensionLength()
	{
		using (Assert.EnterMultipleScope())
		{
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 7), Is.EqualTo("A.ext"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 8), Is.EqualTo("A文.ext"));

			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.exte", 8), Is.EqualTo("A.exte"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.exte", 9), Is.EqualTo("A文.exte"));
		}
	}

	[Test]
	public static void WithoutExtension()
	{
		using (Assert.EnterMultipleScope())
		{
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文", 3), Is.EqualTo("A"));
			Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文", 4), Is.EqualTo("A文"));
		}
	}
}
