using AssetRipper.IO.Endian;

namespace AssetRipper.Core.Parser.Files.ArchiveFiles
{
	public sealed class ArchiveHeader
	{
		internal static bool IsArchiveHeader(EndianReader reader)
		{
			long position = reader.BaseStream.Position;

			ushort gzipMagic = ReadGZipMagic(reader);
			reader.BaseStream.Position = position;
			if (gzipMagic == GZipMagic)
			{
				return true;
			}

			string? brotliSignature = ReadBrotliMetadata(reader);
			reader.BaseStream.Position = position;
			if (brotliSignature == BrotliSignature)
			{
				return true;
			}

			return false;
		}

		private static ushort ReadGZipMagic(EndianReader reader)
		{
			long remaining = reader.BaseStream.Length - reader.BaseStream.Position;
			if (remaining >= sizeof(ushort))
			{
				return reader.ReadUInt16();
			}
			return 0;
		}

		private static string? ReadBrotliMetadata(EndianReader reader)
		{
			long remaining = reader.BaseStream.Length - reader.BaseStream.Position;
			if (remaining < 4)
			{
				return null;
			}

			reader.BaseStream.Position += 1;
			byte bt = reader.ReadByte(); // read 3 bits
			int sizeBytes = bt & 0x3;

			if (reader.BaseStream.Position + sizeBytes > reader.BaseStream.Length)
			{
				return null;
			}

			int length = 0;
			for (int i = 0; i < sizeBytes; i++)
			{
				byte nbt = reader.ReadByte();  // read next 8 bits
				int bits = (bt >> 2) | ((nbt & 0x3) << 6);
				bt = nbt;
				length += bits << (8 * i);
			}

			if (length <= 0)
			{
				return null;
			}
			if (reader.BaseStream.Position + length > reader.BaseStream.Length)
			{
				return null;
			}

			return reader.ReadString(length);
		}

		public void Read(EndianReader reader)
		{
			ushort gzipMagic = reader.ReadUInt16();
			reader.BaseStream.Position -= sizeof(ushort);
			if (gzipMagic == GZipMagic)
			{
				Type = ArchiveType.GZip;
				return;
			}

			long position = reader.BaseStream.Position;
			string? signature = ReadBrotliMetadata(reader);
			reader.BaseStream.Position = position;
			if (signature == BrotliSignature)
			{
				Type = ArchiveType.Brotli;
				return;
			}

			throw new Exception("Unsupported archive type");
		}

		public ArchiveType Type { get; private set; }

		private const ushort GZipMagic = 0x1F8B;
		private const string BrotliSignature = "UnityWeb Compressed Content (brotli)";
	}
}
