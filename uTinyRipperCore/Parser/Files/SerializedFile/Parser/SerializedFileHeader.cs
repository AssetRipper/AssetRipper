using System;

namespace uTinyRipper.SerializedFiles
{
	/// <summary>
	/// The file header is found at the beginning of an asset file. The header is always using big endian byte order.
	/// </summary>
	public sealed class SerializedFileHeader
	{
		public SerializedFileHeader(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}
			m_name = name;
		}

		public static bool IsSerializedFileHeader(EndianReader reader)
		{
			if (reader.BaseStream.Position + HeaderMinSize > reader.BaseStream.Length)
			{
				return false;
			}
			int metadataSize = reader.ReadInt32();
			if (metadataSize < SerializedFileMetadata.MetadataMinSize)
			{
				return false;
			}
			uint fileSize = reader.ReadUInt32();
			if (fileSize < HeaderMinSize + SerializedFileMetadata.MetadataMinSize)
			{
				return false;
			}
			int generation = reader.ReadInt32();
			if (!Enum.IsDefined(typeof(FileGeneration), generation))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		private static bool IsReadEndian(FileGeneration generation)
		{
			return generation >= FileGeneration.FG_350_47x;
		}

		public void Read(EndianReader reader)
		{
			MetadataSize = reader.ReadInt32();
			if (MetadataSize <= 0)
			{
				throw new Exception($"Invalid metadata size {MetadataSize} for asset file {m_name}");
			}
			FileSize = reader.ReadUInt32();
			Generation = (FileGeneration)reader.ReadInt32();
			if (!Enum.IsDefined(typeof(FileGeneration), Generation))
			{
				throw new Exception($"Unsupported file generation {Generation} for asset file '{m_name}'");
			}
			DataOffset = reader.ReadUInt32();
			if (IsReadEndian(Generation))
			{
				SwapEndianess = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			else
			{
				SwapEndianess = false;
			}
		}

		/// <summary>
		/// Size of the metadata parts of the file
		/// </summary>
		public int MetadataSize { get; private set; }
		/// <summary>
		/// Size of the whole file
		/// </summary>
		public uint FileSize { get; private set; }
		/// <summary>
		/// File format version. The number is required for backward compatibility and is normally incremented after the file format has been changed in a major update
		/// </summary>
		public FileGeneration Generation { get; private set; }
		/// <summary>
		/// Offset to the serialized object data. It starts at the data for the first object
		/// </summary>
		public uint DataOffset { get; private set; }
		/// <summary>
		/// Presumably controls the byte order of the data structure. This field is normally set to 0, which may indicate a little endian byte order.
		/// </summary>
		public bool SwapEndianess { get; private set; }

		public const int HeaderMinSize = 16;

		private readonly string m_name;
	}
}
