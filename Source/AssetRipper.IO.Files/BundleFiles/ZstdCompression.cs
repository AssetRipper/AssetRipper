using AssetRipper.IO.Files.Streams;
using ZstdSharp;

namespace AssetRipper.IO.Files.BundleFiles;

public static class ZstdCompression
{
	private static readonly byte[] Signature = [0x28, 0xB5, 0x2F, 0xFD];
	public static bool IsZstd(Stream Stream)
	{
		Span<byte> buffer = stackalloc byte[4];

		long pos = Stream.Position;
		Stream.ReadExactly(buffer);
		Stream.Position = pos;

		return buffer.SequenceEqual(Signature);
	}

	public static void DecompressStream(Stream compressedStream, long compressedSize, Stream decompressedStream, long decompressedSize)
	{
		using PartialStream compressedPartialStream = new PartialStream(compressedStream, compressedStream.Position, compressedSize, true);
		using PartialStream decompressedPartialStream = new PartialStream(decompressedStream, decompressedStream.Position, decompressedSize, true);
		using DecompressionStream zstdStream = new DecompressionStream(compressedPartialStream);
		zstdStream.CopyTo(decompressedPartialStream);
	}
}
