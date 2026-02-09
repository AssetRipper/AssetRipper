using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using System.IO.Compression;

namespace AssetRipper.IO.Files.CompressedFiles.GZip;

public sealed class GZipFile : CompressedFile
{
	private const ushort GZipMagic = 0x1F8B;

	public override void Read(SmartStream stream)
	{
		try
		{
			using SmartStream memoryStream = SmartStream.CreateMemory();
			using (GZipStream gzipStream = new GZipStream(stream, CompressionMode.Decompress, true))
			{
				gzipStream.CopyTo(memoryStream);
			}
			memoryStream.Position = 0;
			UncompressedFile = new ResourceFile(memoryStream, FilePath, Name);
		}
		catch (Exception ex)
		{
			UncompressedFile = new FailedFile()
			{
				Name = Name,
				FilePath = FilePath,
				StackTrace = ex.ToString(),
			};
		}
	}

	public override void Write(Stream stream)
	{
		using MemoryStream memoryStream = new();
		UncompressedFile?.Write(memoryStream);
		memoryStream.Position = 0;
		using GZipStream gzipStream = new GZipStream(stream, CompressionMode.Compress, true);
		memoryStream.CopyTo(gzipStream);
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
}
