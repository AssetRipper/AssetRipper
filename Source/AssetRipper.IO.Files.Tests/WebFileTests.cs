using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.WebFiles;

namespace AssetRipper.IO.Files.Tests;

public class WebFileTests
{
	[Test]
	public async Task ReadIsSymmetricToWriteForFileWithNoEntries()
	{
		WebFileScheme scheme = new();
		WebFile bundle = new();
		using SmartStream stream = SmartStream.CreateMemory();

		bundle.Write(stream);
		long positionAfterWrite = stream.Position;
		stream.Position = 0;

		{
			await Assert.That(positionAfterWrite).IsGreaterThan(0);
			await Assert.That(scheme.CanRead(stream)).IsTrue();
		}

		WebFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;

		{
			await Assert.That(positionAfterRead).IsEqualTo(positionAfterWrite);
		}
	}

	[Test]
	public async Task ReadIsSymmetricToWriteForFileWithOneEntry()
	{
		const int ResourceFileSize = 23;
		const string ResourceFilePath = "Test/Resource.resource";
		const string ResourceName = "Resource.resource";
		WebFileScheme scheme = new();
		WebFile bundle = new();
		bundle.AddResourceFile(new ResourceFile(SmartStream.CreateMemory(new byte[ResourceFileSize]), ResourceFilePath, ResourceName));

		using SmartStream stream = SmartStream.CreateMemory();

		bundle.Write(stream);
		long positionAfterWrite = stream.Position;
		stream.Position = 0;

		{
			await Assert.That(positionAfterWrite).IsGreaterThan(0);
			await Assert.That(scheme.CanRead(stream)).IsTrue();
		}

		WebFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;

		{
			await Assert.That(positionAfterRead).IsEqualTo(positionAfterWrite).Because("Incorrect end position");
			await Assert.That(readBundle.ResourceFiles).Count().IsEqualTo(1).Because("Incorrect count");
		}
		await Assert.That(readBundle.ResourceFiles[0].Stream.Length).IsEqualTo(ResourceFileSize);
	}
}