using System;

namespace uTinyRipper.ArchiveFiles
{
	public sealed class ArchiveHeader
	{
		internal static bool IsArchiveHeader(EndianReader reader)
		{
			if (reader.BaseStream.Length >= sizeof(ushort))
			{
				ushort magic = reader.ReadUInt16();
				if (magic == GZipMagic)
				{
					return true;
				}
			}

			string signature = ReadBrotliMetadata(reader);
			if(signature == BrotliSignature)
			{
				return true;
			}

			return false;
		}

		private static string ReadBrotliMetadata(EndianReader reader)
		{
			if (reader.BaseStream.Length < 2)
			{
				return null;
			}

			reader.BaseStream.Position += 1;
			byte bt = reader.ReadByte();
			int count = bt & 0x3;

			if (reader.BaseStream.Position + count > reader.BaseStream.Length)
			{
				return null;
			}

			int length = 0;
			for (int i = 0; i < count; i++)
			{
				byte nbt = reader.ReadByte();
				int number = (bt >> 2) & ((nbt & 0x3) << 6);
				bt = nbt;
				length += number << (8 * i);
			}

			if (length == 0)
			{
				return null;
			}
			if (reader.BaseStream.Position + length > reader.BaseStream.Length)
			{
				return null;
			}

			return reader.ReadString(length + 1);
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
			string signature = ReadBrotliMetadata(reader);
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
