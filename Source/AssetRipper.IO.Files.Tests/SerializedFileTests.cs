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

		// Copy FilePath and Name from original to read for comparison since they are not written to the stream
		read.FilePath = original.FilePath;
		read.Name = original.Name;

		using (Assert.EnterMultipleScope())
		{
			Assert.That(read.Generation, Is.EqualTo(original.Generation));
			Assert.That(read.Version, Is.EqualTo(original.Version));
			Assert.That(read.Platform, Is.EqualTo(original.Platform));
			Assert.That(read.EndianType, Is.EqualTo(original.EndianType));
			Assert.That(read.Flags, Is.EqualTo(original.Flags));
			Assert.That(read.Dependencies.ToArray(), Is.EqualTo(original.Dependencies.ToArray()));
			Assert.That(read.Objects.ToArray(), Is.EqualTo(original.Objects.ToArray()));
			Assert.That(read.ScriptTypes.ToArray(), Is.EqualTo(original.ScriptTypes.ToArray()));
			Assert.That(read.Types.ToArray(), Is.EqualTo(original.Types.ToArray()));
			Assert.That(read.RefTypes.ToArray(), Is.EqualTo(original.RefTypes.ToArray()));
			Assert.That(read.UserInformation, Is.EqualTo(original.UserInformation));
		}
	}

	[Test]
	public void ConstructedSerializedFileCanBeWrittenAndReadCorrectly()
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
			ObjectData = RandomData.MakeRandomData(100),
		};
		builder.Types.Add(type);
		builder.Objects.Add(obj);

		SerializedFile original = builder.Build();
		AssertReadingAndWritingAreConsistent(original);
	}

	[Test]
	public void SplitSerializedFileCanBeDetectedAndRead()
	{
		SerializedFileBuilder builder = new()
		{
			Generation = FormatVersion.LargeFilesSupport,
			Version = new(2020, 3, 48),
			Platform = BuildTarget.Android,
		};
		SerializedFile original = builder.Build();
		using MemoryStream stream = new();
		original.Write(stream);
		byte[] data = stream.ToArray();

		VirtualFileSystem fileSystem = new();
		const string directory = "/game";
		const string path = $"{directory}/globalgamemanagers";
		fileSystem.Directory.Create(directory);
		int splitPosition = data.Length / 2;
		fileSystem.File.WriteAllBytes($"{path}.split0", data[..splitPosition]);
		fileSystem.File.WriteAllBytes($"{path}.split1", data[splitPosition..]);

		SerializedFile read = SerializedFile.FromFile(path, fileSystem);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(SerializedFile.IsSerializedFile(path, fileSystem), Is.True);
			Assert.That(SerializedFile.IsSerializedFile($"{path}.split0", fileSystem), Is.True);
			Assert.That(read.Generation, Is.EqualTo(original.Generation));
			Assert.That(read.Version, Is.EqualTo(original.Version));
			Assert.That(read.Platform, Is.EqualTo(original.Platform));
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
