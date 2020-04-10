using System;
using System.IO;
using SevenZip.Compression.LZMA;

namespace uTinyRipper
{
	public static class SevenZipHelper
	{
		/// <summary>
		/// Read LZMA properties and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedSize">Decompressed data length</param>
		/// <returns>Decompressed output</returns>
		public static byte[] DecompressLZMA(Stream compressedStream, long compressedSize, long decompressedSize)
		{
			byte[] buffer = new byte[decompressedSize];
			using (MemoryStream decompressedStream = new MemoryStream(buffer))
			{
				DecompressLZMAStream(compressedStream, compressedSize, decompressedStream, decompressedSize);
			}
			return buffer;
		}

		/// <summary>
		/// Read LZMA properties and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		/// <param name="decompressedSize">Decompressed data length</param>
		public static void DecompressLZMAStream(Stream compressedStream, long compressedSize, Stream decompressedStream, long decompressedSize)
		{
			long basePosition = compressedStream.Position;

			byte[] properties = GetPropertiesBuffer();
			compressedStream.ReadBuffer(properties, 0, PropertiesBufferSize);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			decoder.Code(compressedStream, decompressedStream, headlessSize, decompressedSize, null);

			if (compressedStream.Position != basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
		}

		/// <summary>
		/// Read LZMA properties and decompressed size and decompress LZMA data
		/// </summary>
		/// <param name="compressedStream">LZMA compressed stream</param>
		/// <param name="compressedSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		public static void DecompressLZMASizeStream(Stream compressedStream, long compressedSize, Stream decompressedStream)
		{
			long basePosition = compressedStream.Position;

			byte[] properties = GetPropertiesBuffer();
			compressedStream.ReadBuffer(properties, 0, PropertiesBufferSize);
			byte[] sizeBuffer = GetSizeBuffer();
			compressedStream.ReadBuffer(sizeBuffer, 0, SizeBufferSize);

			long headSize = compressedStream.Position - basePosition;
			long dataSize = compressedSize - headSize;
			long decompressedSize = BitConverter.ToInt64(sizeBuffer, 0);

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);
			decoder.Code(compressedStream, decompressedStream, dataSize, decompressedSize, null);

			if (compressedStream.Position != basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
		}

		private static byte[] GetPropertiesBuffer()
		{
			if (PropertiesBuffer == null)
			{
				PropertiesBuffer = new byte[PropertiesBufferSize];
			}
			return PropertiesBuffer;
		}

		private static byte[] GetSizeBuffer()
		{
			if (SizeBuffer == null)
			{
				SizeBuffer = new byte[SizeBufferSize];
			}
			return SizeBuffer;
		}

		private const int PropertiesBufferSize = 5;
		private const int SizeBufferSize = 8;

		[ThreadStatic]
		private static byte[] PropertiesBuffer;
		[ThreadStatic]
		private static byte[] SizeBuffer;
	}
}
