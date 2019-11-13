using System;
using System.IO;
using SevenZip.Compression.LZMA;

namespace uTinyRipper
{
	public static class SevenZipHelper
	{
		/// <summary>
		/// Decompress engine's LZMA stream. Write decomressed data to decompressedStream.
		/// BaseStream contains 'decompress size'
		/// </summary>
		/// <param name="baseStream">LZMA compressed stream</param>
		/// <param name="compressSize">Compressed data length</param>
		/// <param name="decompressedStream">Decompressed out stream</param>
		public static void DecompressLZMASizeStream(Stream baseStream, long compressSize, Stream decompressedStream)
		{
			long basePosition = baseStream.Position;
			byte[] properties = new byte[PropertiesSize];
			baseStream.ReadBuffer(properties, 0, PropertiesSize);
			byte[] countBuffer = new byte[CountSize];
			baseStream.ReadBuffer(countBuffer, 0, CountSize);
			
			long decompressedSize = BitConverter.ToInt64(countBuffer, 0);
			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);

			long headSize = baseStream.Position - basePosition;
			long dataSize = compressSize - headSize;
			long startPosition = decompressedStream.Position;
			decoder.Code(baseStream, decompressedStream, dataSize, decompressedSize, null);

			if (baseStream.Position > basePosition + compressSize)
			{
				throw new Exception($"Read {baseStream.Position - basePosition} more than expected {compressSize}");
			}
			baseStream.Position = basePosition + compressSize;
			decompressedStream.Position = startPosition;
		}

		/// <summary>
		/// Decompress engine's LZMA stream. Return new decompressed MemoryStream.
		/// BaseStream contains 'decompress size'
		/// </summary>
		/// <param name="baseStream">LZMA compressed stream</param>
		/// <param name="compressSize">Compressed data length</param>
		/// <returns>Decompressed MemoryStream</returns>
		public static MemoryStream DecompressLZMASizeStream(Stream baseStream, long compressSize)
		{
			MemoryStream decompressStream = new MemoryStream();
			DecompressLZMASizeStream(baseStream, compressSize, decompressStream);
			return decompressStream;
		}


		/// <summary>
		/// Decompress engine's LZMA stream. Return new decompressed MemoryStream
		/// BaseStream don't contains 'decompress size'
		/// </summary>
		/// <param name="baseStream">LZMA compressed stream</param>
		/// <param name="compressSize">Compressed data length</param>
		/// <param name="decompressedStream">Stream for decompressed output</param>
		/// <param name="decompreesSize">Decompressed data length</param>
		/// <returns>Decompressed MemoryStream</returns>
		public static void DecompressLZMAStream(Stream baseStream, long compressSize, Stream decompressedStream, long decompreesSize)
		{
			long basePosition = baseStream.Position;
			byte[] properties = new byte[PropertiesSize];
			int read = baseStream.Read(properties, 0, PropertiesSize);
			if (read != PropertiesSize)
			{
				throw new Exception("Unable to read lzma properties");
			}

			Decoder decoder = new Decoder();
			decoder.SetDecoderProperties(properties);

			long headSize = baseStream.Position - basePosition;
			long headlessSize = compressSize - headSize;
			long startPosition = decompressedStream.Position;
			decoder.Code(baseStream, decompressedStream, headlessSize, decompreesSize, null);

			if(baseStream.Position > basePosition + compressSize)
			{
				throw new Exception($"Read {baseStream.Position - basePosition} more than expected {compressSize}");
			}
			baseStream.Position = basePosition + compressSize;
			decompressedStream.Position = startPosition;
		}

		/// <summary>
		/// Decompress engine's LZMA stream. Return new decompressed MemoryStream
		/// BaseStream don't contains 'decompress size'
		/// </summary>
		/// <param name="baseStream">LZMA compressed stream</param>
		/// <param name="compressLength">Compressed data length</param>
		/// <param name="decompreesLength">Decompressed data length</param>
		/// <returns>Decompressed MemoryStream</returns>
		public static MemoryStream DecompressLZMAStream(Stream baseStream, long compressLength, long decompreesLength)
		{
			MemoryStream decompressStream = new MemoryStream();
			DecompressLZMAStream(baseStream, compressLength, decompressStream, decompreesLength);
			return decompressStream;
		}

		private const int PropertiesSize = 5;
		private const int CountSize = 8;
	}
}
