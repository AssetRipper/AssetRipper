using AssetRipper.IO.Files.Extensions;
using SharpCompress.Compressors.LZMA;
using System.IO;

namespace AssetRipper.IO.Files.BundleFiles
{
	public static class SevenZipHelper
	{
		/// <summary>
		/// Read LZMA properties and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		/// <param name="decompressedSize">Decompressed data length</param>
		public static void DecompressLZMAStream(Stream compressedStream, long compressedSize, Stream decompressedStream, long decompressedSize)
		{
			byte[] properties = new byte[PropertiesSize];
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(properties, 0, PropertiesSize);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			DecompressLZMAStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

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
		public static void DecompressLZMASizeStream(Stream compressedStream, long compressedSize, Stream decompressedStream)
		{
			byte[] properties = new byte[PropertiesSize]; //GetBuffer();
			byte[] sizeBytes = new byte[UncompressedSize]; //GetBuffer();
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(properties, 0, PropertiesSize);
			compressedStream.ReadBuffer(sizeBytes, 0, UncompressedSize);
			long decompressedSize = BitConverter.ToInt64(sizeBytes, 0);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			DecompressLZMAStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

			if (compressedStream.Position > basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
			compressedStream.Position = basePosition + compressedSize;
		}

		private static void DecompressLZMAStream(byte[] properties, Stream compressedStream, long headlessSize, Stream decompressedStream, long decompressedSize)
		{
			LzmaStream lzmaStream = new LzmaStream(properties, compressedStream, headlessSize, -1, null, false);

			byte[] buffer = GetBuffer();
			int read;
			long totalRead = 0;
			while (totalRead < decompressedSize)
			{
				int toRead = (int)Math.Min(buffer.Length, decompressedSize - totalRead);
				read = lzmaStream.Read(buffer, 0, toRead);
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
		}

		private static byte[] GetBuffer()
		{
			if (s_buffer == null)
			{
				s_buffer = new byte[1024];
			}
			return s_buffer;
		}

		private const int PropertiesSize = 5;
		private const int UncompressedSize = 8;

		[ThreadStatic]
		private static byte[]? s_buffer;
	}
}
