using System.Buffers;
using ZstdSharp;

namespace AssetRipper.IO.Files.BundleFiles
{
	public static class ZstdCompression
	{
		private static readonly byte[] Signature = [0x28, 0xB5, 0x2F, 0xFD];
		public static bool IsZstd(Stream Stream)
		{
			Span<byte> buffer = stackalloc byte[4];

			long pos = Stream.Position;
			Stream.Read(buffer);
			Stream.Position = pos;

			return buffer.SequenceEqual(Signature);
		}

		public static void DecompressStream(Stream compressedStream, long compressedSize, Stream decompressedStream, long decompressedSize)
		{
			using DecompressionStream zstdStream = new DecompressionStream(compressedStream, (int)compressedSize, true, true);

			byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
			long totalRead = 0;
			while (totalRead < decompressedSize)
			{
				int toRead = (int)Math.Min(buffer.Length, decompressedSize - totalRead);
				int read = zstdStream.Read(buffer, 0, toRead);
				if (read > 0)
				{
					decompressedStream.Write(buffer, 0, read);
					totalRead += read;
				}
				else
				{
					break;
				}
			}

			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}
