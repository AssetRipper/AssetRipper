using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using System.IO.Compression;

namespace AssetRipper.IO.Files.CompressedFiles.Brotli;

public sealed class BrotliFile : CompressedFile
{
	private static ReadOnlySpan<byte> BrotliSignature => "UnityWeb Compressed Content (brotli)"u8;

	public override void Read(SmartStream stream)
	{
		try
		{
			byte[] buffer = ReadBrotli(stream);
			UncompressedFile = new ResourceFile(buffer, FilePath, Name);
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

	internal static bool IsBrotliFile(Stream stream)
	{
		long remaining = stream.Length - stream.Position;
		if (remaining < 4)
		{
			return false;
		}

		long position = stream.Position;

		stream.Position += 1;
		byte bt = (byte)stream.ReadByte(); // read 3 bits
		int sizeBytes = bt & 0x3;

		if (stream.Position + sizeBytes > stream.Length)
		{
			stream.Position = position;
			return false;
		}

		int length = 0;
		for (int i = 0; i < sizeBytes; i++)
		{
			byte nbt = (byte)stream.ReadByte();  // read next 8 bits
			int bits = (bt >> 2) | ((nbt & 0x3) << 6);
			bt = nbt;
			length += bits << (8 * i);
		}

		if (length != BrotliSignature.Length
			|| stream.Position + length > stream.Length)
		{
			stream.Position = position;
			return false;
		}

		Span<byte> buffer = stackalloc byte[BrotliSignature.Length];
		stream.ReadExactly(buffer);
		stream.Position = position;
		return buffer.SequenceEqual(BrotliSignature);
	}

	private static byte[] ReadBrotli(Stream stream)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
		brotliStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	public override void Write(Stream stream)
	{
		throw new NotImplementedException();
	}
}
