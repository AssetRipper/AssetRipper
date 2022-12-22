using AssetRipper.IO.Files.Extensions;
using SharpCompress.Compressors.LZMA;
using System.Buffers;
using System.IO;

namespace AssetRipper.IO.Files.BundleFiles
{
	public static class LzmaCompression
	{
		/// <summary>
		/// Read LZMA properties and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		/// <param name="decompressedSize">Decompressed data length</param>
		public static void DecompressLzmaStream(Stream compressedStream, long compressedSize, Stream decompressedStream, long decompressedSize)
		{
			byte[] properties = new byte[PropertiesSize];
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(properties, 0, PropertiesSize);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			DecompressLzmaStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

			if (compressedStream.Position > basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
			compressedStream.Position = basePosition + compressedSize;
		}

		/// <summary>
		/// Read LZMA properties and decompressed size and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		public static void DecompressLzmaSizeStream(Stream compressedStream, long compressedSize, Stream decompressedStream)
		{
			byte[] properties = new byte[PropertiesSize]; //GetBuffer();
			byte[] sizeBytes = new byte[UncompressedSize]; //GetBuffer();
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(properties, 0, PropertiesSize);
			compressedStream.ReadBuffer(sizeBytes, 0, UncompressedSize);
			long decompressedSize = BitConverter.ToInt64(sizeBytes, 0);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			DecompressLzmaStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

			if (compressedStream.Position > basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
			compressedStream.Position = basePosition + compressedSize;
		}

		private static void DecompressLzmaStream(byte[] properties, Stream compressedStream, long headlessSize, Stream decompressedStream, long decompressedSize)
		{
			LzmaStream lzmaStream = new LzmaStream(properties, compressedStream, headlessSize, -1, null, false);

			byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
			long totalRead = 0;
			while (totalRead < decompressedSize)
			{
				int toRead = (int)Math.Min(buffer.Length, decompressedSize - totalRead);
				int read = lzmaStream.Read(buffer, 0, toRead);
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

		private const int PropertiesSize = 5;
		private const int UncompressedSize = 8;
	}
}
