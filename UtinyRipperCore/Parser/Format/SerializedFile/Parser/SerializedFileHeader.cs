using System;

namespace UtinyRipper.SerializedFiles
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
			FileSize = reader.ReadInt32();
			if (FileSize <= 0)
			{
				throw new Exception($"Invalid data size {FileSize} for asset file {m_name}");
			}
			Generation = (FileGeneration)reader.ReadInt32();
			if (!Enum.IsDefined(typeof(FileGeneration), Generation))
			{
				throw new Exception($"Unsuported file generation {Generation} for asset file '{m_name}'");
			}
			DataOffset = reader.ReadUInt32();
			if(IsReadEndian(Generation))
			{
				Endianess = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			else
			{
				Endianess = false;
			}
		}

		/// <summary>
		/// Size of the metadata parts of the file
		/// </summary>
		public int MetadataSize { get; private set; }
		/// <summary>
		/// Size of the whole file
		/// </summary>
		public int FileSize { get; private set; }
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
		public bool Endianess { get; private set; }

		private readonly string m_name;
	}
}
