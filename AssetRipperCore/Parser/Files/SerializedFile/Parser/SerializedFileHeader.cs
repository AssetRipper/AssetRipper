using AssetRipper.Core.IO.Endian;
using System;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	/// <summary>
	/// The file header is found at the beginning of an asset file. The header is always using big endian byte order.
	/// </summary>
	public sealed class SerializedFileHeader
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


		/// <summary>
		/// 3.5.0 and greater / Format Version 9 +
		/// </summary>
		public static bool HasEndianess(FormatVersion generation) => generation >= FormatVersion.Unknown_9;

		/// <summary>
		/// 2020.1.0 and greater / Format Version 22 +
		/// </summary>
		public static bool HasLargeFilesSupport(FormatVersion generation) => generation >= FormatVersion.LargeFilesSupport;

		public static bool IsSerializedFileHeader(EndianReader reader, uint fileSize)
		{
			long position = reader.BaseStream.Position;
			if (reader.BaseStream.Position + HeaderMinSize > reader.BaseStream.Length)
			{
				return false;
			}
			int metadataSize = reader.ReadInt32();
			if (metadataSize < SerializedFileMetadata.MetadataMinSize)
			{
				reader.BaseStream.Position = position;
				return false;
			}
			uint hFileSize = reader.ReadUInt32();
			if (hFileSize < HeaderMinSize + SerializedFileMetadata.MetadataMinSize)
			{
				reader.BaseStream.Position = position;
				return false;
			}
			if (hFileSize != fileSize)
			{
				reader.BaseStream.Position = position;
				return false;
			}
			int generation = reader.ReadInt32();
			if (!Enum.IsDefined(typeof(FormatVersion), generation))
			{
				reader.BaseStream.Position = position;
				return false;
			}

			reader.BaseStream.Position = position;
			return true;
		}

		public void Read(EndianReader reader)
		{
			MetadataSize = reader.ReadInt32();
			if (MetadataSize <= 0)
			{
				throw new Exception($"Invalid metadata size {MetadataSize}");
			}
			FileSize = reader.ReadUInt32();
			Version = (FormatVersion)reader.ReadInt32();
			if (!Enum.IsDefined(typeof(FormatVersion), Version))
			{
				throw new Exception($"Unsupported file generation {Version}'");
			}
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
		}

#warning TODO: Needs verified, especially the value byte sizes
		public void Write(EndianWriter writer)
		{
			writer.Write(MetadataSize);
			writer.Write(FileSize);
			writer.Write((int)Version);
			writer.Write(DataOffset);
			if (HasEndianess(Version))
			{
				writer.Write(Endianess);
				writer.AlignStream();
			}
		}
	}
}
