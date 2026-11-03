using AssetRipper.IO.Files.Streams;

namespace AssetRipper.IO.Files.Tests;

public class MultiFileStreamTests
{
	[Test]
	public async Task OpenReadIgnoresFilesThatAreNotSplitParts()
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

		await Assert.That(result.ToArray().SequenceEqual(new byte[] { 1, 2 })).IsTrue();
	}

	[Test]
	public async Task ReadByteSkipsEmptySplitParts()
	{
		VirtualFileSystem fileSystem = new();
		fileSystem.Directory.Create("/data");
		string path = "/data/archive";
		fileSystem.File.WriteAllBytes(path + ".split0", [1]);
		fileSystem.File.WriteAllBytes(path + ".split1", []);
		fileSystem.File.WriteAllBytes(path + ".split2", [2]);

		using Stream stream = MultiFileStream.OpenRead(path, fileSystem);

		await Assert.That(stream.ReadByte()).IsEqualTo(1);
		await Assert.That(stream.ReadByte()).IsEqualTo(2);
		await Assert.That(stream.ReadByte()).IsEqualTo(-1);
	}
}