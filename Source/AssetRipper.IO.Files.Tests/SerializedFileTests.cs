using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

internal class SerializedFileTests
{
	[Theory]
	public void WritingSerializedFileDoesNotThrow(FormatVersion generation)
	{
		Assert.DoesNotThrow(() =>
		{
			SerializedFileBuilder builder = new() { Generation = generation };
			SerializedFile file = builder.Build();
			using SmartStream stream = SmartStream.CreateMemory();
			file.Write(stream);
		});
	}

	[Theory]
	public void WrittenSerializedFileCanBeRead(FormatVersion generation)
	{
		SerializedFileBuilder builder = new() { Generation = generation };
		SerializedFile file = builder.Build();
		using SmartStream stream = SmartStream.CreateMemory();
		file.Write(stream);
		stream.Flush();
		stream.Position = 0;
		Assert.That(SerializedFileScheme.Default.CanRead(stream));
	}

	[Theory]
	public void ReadingAndWritingAreConsistent(FormatVersion generation)
	{
		SerializedFileBuilder builder = new() { Generation = generation };
		SerializedFile original = builder.Build();
		AssertReadingAndWritingAreConsistent(original);
	}

	private static void AssertReadingAndWritingAreConsistent(SerializedFile original)
	{
		SerializedFile read;
		{
			using SmartStream stream = SmartStream.CreateMemory();
			original.Write(stream);
			stream.Flush();
			stream.Position = 0;
			read = SerializedFileScheme.Default.Read(stream, original.FilePath, original.Name);
		}

		using (Assert.EnterMultipleScope())
		{
			Assert.That(read.Generation, Is.EqualTo(original.Generation));
			Assert.That(read.Version, Is.EqualTo(original.Version));
			Assert.That(read.Platform, Is.EqualTo(original.Platform));
			Assert.That(read.EndianType, Is.EqualTo(original.EndianType));
			Assert.That(read.Flags, Is.EqualTo(original.Flags));
			Assert.That(read.Dependencies.ToArray(), Is.EqualTo(original.Dependencies.ToArray()));
			Assert.That(read.Objects.ToArray(), Is.EqualTo(original.Objects.ToArray()));
		}
	}

	[Theory]
	public void SerializeFileHeaderReadingMatchesWriting(FormatVersion generation)
	{
		SerializedFileHeader header = new()
		{
			Version = generation,
			MetadataSize = 256,//arbitrary number greater than 0
		};
		using MemoryStream stream = new();
		using (EndianWriter writer = new(stream, EndianType.BigEndian))
		{
			header.Write(writer);
		}
		stream.Flush();
		Assert.That(stream.Position, Is.GreaterThan(0));
		stream.Position = 0;
		SerializedFileHeader readHeader = new();
		using (EndianReader reader = new(stream, EndianType.BigEndian))
		{
			readHeader.Read(reader);
		}

		using (Assert.EnterMultipleScope())
		{
			Assert.That(stream.Position, Is.EqualTo(stream.Length));
			Assert.That(readHeader, Is.EqualTo(header));
		}
	}
}
