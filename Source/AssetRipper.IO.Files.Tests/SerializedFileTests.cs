using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

internal class SerializedFileTests
{
	[Test]
	[CombinedDataSources]
	public async Task WritingSerializedFileDoesNotThrow([MethodDataSource(nameof(GetFormatVersions))] FormatVersion generation)
	{
		await Assert.That(() =>
		{
			SerializedFileBuilder builder = new() { Generation = generation };
			SerializedFile file = builder.Build();
			using SmartStream stream = SmartStream.CreateMemory();
			file.Write(stream);
		}).ThrowsNothing();
	}

	[Test]
	[CombinedDataSources]
	public async Task WrittenSerializedFileCanBeRead([MethodDataSource(nameof(GetFormatVersions))] FormatVersion generation)
	{
		SerializedFileBuilder builder = new() { Generation = generation };
		SerializedFile file = builder.Build();
		using SmartStream stream = SmartStream.CreateMemory();
		file.Write(stream);
		stream.Flush();
		stream.Position = 0;
		await Assert.That(SerializedFileScheme.Default.CanRead(stream)).IsTrue();
	}

	[Test]
	[CombinedDataSources]
	public async Task ReadingAndWritingAreConsistent([MethodDataSource(nameof(GetFormatVersions))] FormatVersion generation)
	{
		SerializedFileBuilder builder = new() { Generation = generation };
		SerializedFile original = builder.Build();
		await AssertReadingAndWritingAreConsistent(original);
	}

	private static async Task AssertReadingAndWritingAreConsistent(SerializedFile original)
	{
		SerializedFile read;
		{
			using SmartStream stream = SmartStream.CreateMemory();
			original.Write(stream);
			stream.Flush();
			stream.Position = 0;
			read = SerializedFileScheme.Default.Read(stream, original.FilePath, original.Name);
		}

		// Copy FilePath and Name from original to read for comparison since they are not written to the stream
		read.FilePath = original.FilePath;
		read.Name = original.Name;


		{
			await Assert.That(read.Generation).IsEqualTo(original.Generation);
			await Assert.That(read.Version).IsEqualTo(original.Version);
			await Assert.That(read.Platform).IsEqualTo(original.Platform);
			await Assert.That(read.EndianType).IsEqualTo(original.EndianType);
			await Assert.That(read.Flags).IsEqualTo(original.Flags);
			await Assert.That(read.Dependencies.ToArray().SequenceEqual(original.Dependencies.ToArray())).IsTrue();
			await Assert.That(read.Objects.ToArray().SequenceEqual(original.Objects.ToArray())).IsTrue();
			await Assert.That(read.ScriptTypes.ToArray().SequenceEqual(original.ScriptTypes.ToArray())).IsTrue();
			await Assert.That(read.Types.ToArray().SequenceEqual(original.Types.ToArray())).IsTrue();
			await Assert.That(read.RefTypes.ToArray().SequenceEqual(original.RefTypes.ToArray())).IsTrue();
			await Assert.That(read.UserInformation).IsEqualTo(original.UserInformation);
		}
	}

	[Test]
	public async Task ConstructedSerializedFileCanBeWrittenAndReadCorrectly()
	{
		SerializedFileBuilder builder = new()
		{
			Generation = FormatVersion.LargeFilesSupport,
			Version = new(6000, 1, 0),
			Platform = BuildTarget.StandaloneWin64Player,
			EndianType = EndianType.LittleEndian,
			HasTypeTree = false,
		};
		SerializedType type = new()
		{
			TypeID = 1,
			IsStrippedType = false,
			ScriptTypeIndex = -1,
		};
		ObjectInfo obj = new(type)
		{
			FileID = 1,
			SerializedTypeIndex = 0,
			ObjectData = new Random(42).NextBytes(100),
		};
		builder.Types.Add(type);
		builder.Objects.Add(obj);

		SerializedFile original = builder.Build();
		await AssertReadingAndWritingAreConsistent(original);
	}

	[Test]
	[CombinedDataSources]
	public async Task SerializeFileHeaderReadingMatchesWriting([MethodDataSource(nameof(GetFormatVersions))] FormatVersion generation)
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
		await Assert.That(stream.Position).IsGreaterThan(0);
		stream.Position = 0;
		SerializedFileHeader readHeader = new();
		using (EndianReader reader = new(stream, EndianType.BigEndian))
		{
			readHeader.Read(reader);
		}


		{
			await Assert.That(stream.Position).IsEqualTo(stream.Length);
			await Assert.That(readHeader).IsEqualTo(header);
		}
	}

	public static FormatVersion[] GetFormatVersions() => Enum.GetValues<FormatVersion>();
}
