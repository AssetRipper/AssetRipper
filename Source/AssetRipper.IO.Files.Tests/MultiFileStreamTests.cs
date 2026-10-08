using AssetRipper.IO.Files.Streams;

namespace AssetRipper.IO.Files.Tests;

public class MultiFileStreamTests
{
	[Test]
	public void OpenReadIgnoresFilesThatAreNotSplitParts()
	{
		VirtualFileSystem fileSystem = new();
		fileSystem.Directory.Create("/data");
		string path = "/data/archive";
		fileSystem.File.WriteAllBytes(path + ".split0", [1]);
		fileSystem.File.WriteAllBytes(path + ".split1", [2]);
		fileSystem.File.WriteAllBytes(path + ".splitbackup", [3]);

		using Stream stream = MultiFileStream.OpenRead(path, fileSystem);
		using MemoryStream result = new();
		stream.CopyTo(result);

		Assert.That(result.ToArray(), Is.EqualTo(new byte[] { 1, 2 }));
	}
}
