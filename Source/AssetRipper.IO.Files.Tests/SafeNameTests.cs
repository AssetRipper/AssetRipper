namespace AssetRipper.IO.Files.Tests;

public static class SafeNameTests
{
	[Test]
	public static void NormalUnicodeNameIsPreserved()
	{
		Assert.That(FileSystem.GetSafeFileName("丹花伊吹", "Texture2D_1"), Is.EqualTo("丹花伊吹"));
	}

	[Test]
	public static void ObfuscatedUnicodeNameUsesFallback()
	{
		Assert.That(FileSystem.GetSafeFileName("kanna\u202E\uE171", "AnimationClip_42"), Is.EqualTo("AnimationClip_42"));
	}

	[Test]
	public static void RelativePathCannotTraverseParent()
	{
		string path = FileSystem.GetSafeRelativePath("../Assets/\u202E\uE171");
		using (Assert.EnterMultipleScope())
		{
			Assert.That(Path.IsPathRooted(path), Is.False);
			Assert.That(path.Split(Path.DirectorySeparatorChar), Does.Not.Contain(".."));
			Assert.That(path, Does.Contain("Assets"));
		}
	}

	[Test]
	public static void UnsafeUnicodeCanBeLoggedWithoutFormattingEffects()
	{
		Assert.That(FileSystem.EscapeUnsafeUnicode("kanna\u202E\uE171"), Is.EqualTo("kanna\\u202E\\uE171"));
	}
}
