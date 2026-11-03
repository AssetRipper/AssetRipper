namespace AssetRipper.IO.Files.Tests;

public class GetUniqueNameTests
{
	[Test]
	public async Task FilenameTruncationMultibyteCharacter()
	{

		{
			// A        length 3 cont     cont     cont
			// 01000001 11100110 10010110 10000111 00001010
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 4)).IsEqualTo(".ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 5)).IsEqualTo("A.ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 6)).IsEqualTo("A.ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 7)).IsEqualTo("A.ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 8)).IsEqualTo("A文.ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 9)).IsEqualTo("A文.ext");
		}
	}

	[Test]
	public async Task ExtensionLength()
	{

		{
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 7)).IsEqualTo("A.ext");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.ext", 8)).IsEqualTo("A文.ext");

			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.exte", 8)).IsEqualTo("A.exte");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文.exte", 9)).IsEqualTo("A文.exte");
		}
	}

	[Test]
	public async Task WithoutExtension()
	{

		{
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文", 3)).IsEqualTo("A");
			await Assert.That(new VirtualFileSystem().GetUniqueName("/dir", "A文", 4)).IsEqualTo("A文");
		}
	}
}