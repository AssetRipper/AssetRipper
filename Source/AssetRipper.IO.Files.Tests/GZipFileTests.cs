using AssetRipper.IO.Files.CompressedFiles.GZip;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public class GZipFileTests
{
	[Test]
	public async Task GZipFileReadWriteSymmetry()
	{
		const string Name = "Path.Extension";
		const string FilePath = $"C://Some/Absolute/{Name}";
		const int UncompressedSize = 4067;
		byte[] randomData = new Random(42).NextBytes(UncompressedSize);

		GZipFile file = new()
		{
			FilePath = FilePath,
			Name = Name,
		};
		file.UncompressedFile = new ResourceFile(SmartStream.CreateMemory(randomData), file.FilePath, file.Name);

		SmartStream memoryStream = SmartStream.CreateMemory();
		file.Write(memoryStream);

		{
			await Assert.That(memoryStream.Position).IsGreaterThan(0);
			await Assert.That(memoryStream.IsNull).IsFalse();
		}
		memoryStream.Position = 0;

		GZipFileScheme fileScheme = new();
		await Assert.That(fileScheme.CanRead(memoryStream)).IsTrue();
		GZipFile newFile = fileScheme.Read(memoryStream, FilePath, Name);

		{
			await Assert.That(newFile.Name).IsEqualTo(file.Name);
			await Assert.That(newFile.FilePath).IsEqualTo(file.FilePath);
			await Assert.That(newFile.UncompressedFile).IsNotNull();
			await Assert.That(memoryStream.Position).IsGreaterThan(0);
			await Assert.That(memoryStream.IsNull).IsFalse();
		}
		byte[] decompressedData = newFile.UncompressedFile!.ToByteArray();
		await Assert.That(decompressedData.SequenceEqual(randomData)).IsTrue();
	}
}
