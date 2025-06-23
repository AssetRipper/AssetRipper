using AssetRipper.IO.Endian;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

/// <summary>
/// The file header is found at the beginning of an asset file. The header is always using big endian byte order.
/// </summary>
public sealed record class SerializedFileHeader
{
	/// <summary>
	/// Size of the metadata parts of the file
	/// </summary>
	public long MetadataSize { get; set; }
	/// <summary>
	/// Size of the whole file
	/// </summary>
	public long FileSize { get; set; }
	/// <summary>
	/// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update
	/// </summary>
	public FormatVersion Version { get; set; }
	/// <summary>
	/// Offset to the serialized object data. It starts at the data for the first object
	/// </summary>
	public long DataOffset { get; set; }
	/// <summary>
	/// Presumably controls the byte order of the data structure. This field is normally set to 0, which may indicate a little endian byte order.
	/// </summary>
	public bool Endianess { get; set; }

	public const int HeaderMinSize = 16;

	public const int MetadataMinSize = 13;


	/// <summary>
	/// 3.5.0 and greater / Format Version 9 +
	/// </summary>
	public static bool HasEndianess(FormatVersion generation) => generation >= FormatVersion.Unknown_9;

	/// <summary>
	/// 2020.1.0 and greater / Format Version 22 +
	/// </summary>
	public static bool HasLargeFilesSupport(FormatVersion generation) => generation >= FormatVersion.LargeFilesSupport;

	public static bool IsSerializedFileHeader(EndianReader reader, long fileSize)
	{
		long initialPosition = reader.BaseStream.Position;

		//Sanity check that there is enough room here first.
		if (reader.BaseStream.Position + HeaderMinSize > reader.BaseStream.Length)
		{
			return false;
		}

		//Pre-22 format: 
		// - Metadata Size
		// - File Size
		// - Generation
		int metadataSize = reader.ReadInt32();
		ulong headerDefinedFileSize = reader.ReadUInt32();

		// Read generation first, the format changed hugely in gen 22 (unity 2020)
		// Generation is always at [base + 0x8]
		int generation = reader.ReadInt32();
		if (!Enum.IsDefined(typeof(FormatVersion), generation))
		{
			reader.BaseStream.Position = initialPosition;
			return false;
		}

		if (generation >= 22)
		{
			//22 Format:
			//First known value is at 0x14, and is metadata size as a 32-bit integer.
			//Then the file size as a 64-bit integer.
			reader.BaseStream.Position = initialPosition + 0x14;
			metadataSize = reader.ReadInt32();
			headerDefinedFileSize = reader.ReadUInt64();
		}

		reader.BaseStream.Position = initialPosition;

		return metadataSize >= MetadataMinSize
			&& headerDefinedFileSize >= HeaderMinSize + MetadataMinSize
			&& fileSize >= 0
			&& headerDefinedFileSize == (ulong)fileSize;
	}

	public void Read(EndianReader reader)
	{
		//For gen 22+ these will be zero
		MetadataSize = reader.ReadInt32();
		FileSize = reader.ReadUInt32();

		//Read generation
		Version = (FormatVersion)reader.ReadInt32();

		//For gen 22+ this will be zero
		DataOffset = reader.ReadUInt32();

		if (HasEndianess(Version))
		{
			Endianess = reader.ReadBoolean();
			reader.AlignStream();
		}
		if (HasLargeFilesSupport(Version))
		{
			MetadataSize = reader.ReadUInt32();
			FileSize = reader.ReadInt64();
			DataOffset = reader.ReadInt64();
			reader.ReadInt64(); // unknown
		}

		if (MetadataSize <= 0)
		{
			throw new Exception($"Invalid metadata size {MetadataSize}");
		}

		if (!Enum.IsDefined(typeof(FormatVersion), Version))
		{
			throw new Exception($"Unsupported file generation {Version}'");
		}
	}

	public void Write(EndianWriter writer)
	{
		//0x00
		if (HasLargeFilesSupport(Version))
		{
			writer.Write(0);
			writer.Write(0u);
		}
		else
		{
			writer.Write((int)MetadataSize);
			writer.Write((uint)FileSize);
		}

		//0x08
		writer.Write((int)Version);

		//0x0c
		if (HasLargeFilesSupport(Version))
		{
			writer.Write(0u);
		}
		else
		{
			writer.Write((uint)DataOffset);
		}

		//0x10
		if (HasEndianess(Version))
		{
			writer.Write(Endianess);
			writer.AlignStream();
		}

		//0x14
		if (HasLargeFilesSupport(Version))
		{
			writer.Write((uint)MetadataSize);
			writer.Write(FileSize);
			writer.Write(DataOffset);
			writer.Write(0L);
		}
	}
}
