using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using System.IO;
using System.IO.Compression;

namespace AssetRipper.IO.Files.CompressedFiles.GZip
{
	public sealed class GZipFile : CompressedFile
	{
		private const ushort GZipMagic = 0x1F8B;

		public override void Read(SmartStream stream)
		{
			byte[] buffer = ReadGZip(stream);
			UncompressedFile = new ResourceFile(SmartStream.CreateMemory(buffer, 0, buffer.Length, false), FilePath, Name);
		}

		internal static bool IsGZipFile(EndianReader reader)
		{
			long position = reader.BaseStream.Position;
			ushort gzipMagic = ReadGZipMagic(reader);
			reader.BaseStream.Position = position;
			return gzipMagic == GZipMagic;
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

		private static byte[] ReadGZip(Stream stream)
		{
			using MemoryStream memoryStream = new MemoryStream();
			using GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress);
			gzipStream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}

		public override void Write(Stream stream)
		{
			throw new NotImplementedException();
		}
	}
}
