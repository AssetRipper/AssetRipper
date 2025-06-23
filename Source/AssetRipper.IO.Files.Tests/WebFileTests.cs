using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.WebFiles;

namespace AssetRipper.IO.Files.Tests;

public class WebFileTests
{
	[Test]
	public void ReadIsSymmetricToWriteForFileWithNoEntries()
	{
		WebFileScheme scheme = new();
		WebFile bundle = new();
		using SmartStream stream = SmartStream.CreateMemory();

		bundle.Write(stream);
		long positionAfterWrite = stream.Position;
		stream.Position = 0;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(positionAfterWrite, Is.GreaterThan(0));
			Assert.That(scheme.CanRead(stream));
		}

		WebFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(positionAfterRead, Is.EqualTo(positionAfterWrite));
		}
	}

	[Test]
	public void ReadIsSymmetricToWriteForFileWithOneEntry()
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
		using (Assert.EnterMultipleScope())
		{
			Assert.That(positionAfterWrite, Is.GreaterThan(0));
			Assert.That(scheme.CanRead(stream));
		}

		WebFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(positionAfterRead, Is.EqualTo(positionAfterWrite), () => "Incorrect end position");
			Assert.That(readBundle.ResourceFiles, Has.Count.EqualTo(1), () => "Incorrect count");
		}
		Assert.That(readBundle.ResourceFiles[0].Stream, Has.Length.EqualTo(ResourceFileSize));
	}
}
