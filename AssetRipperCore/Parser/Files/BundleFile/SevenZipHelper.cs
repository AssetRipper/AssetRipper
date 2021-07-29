using AssetRipper.Extensions;
using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace AssetRipper.Parser.Files.BundleFile
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
			Decoder decoder = new Decoder();
			byte[] buffer = GetBuffer();
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(buffer, 0, PropertiesSize);
			decoder.SetDecoderProperties(buffer);

			long headSize = compressedStream.Position - basePosition;
			long headlessSize = compressedSize - headSize;

			decoder.Code(compressedStream, decompressedStream, headlessSize, decompressedSize, null);
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
			Decoder decoder = new Decoder();
			byte[] buffer = GetBuffer();
			long basePosition = compressedStream.Position;

			compressedStream.ReadBuffer(buffer, 0, PropertiesSize);
			decoder.SetDecoderProperties(buffer);
			compressedStream.ReadBuffer(buffer, 0, UncompressedSize);
			long decompressedSize = BitConverter.ToInt64(buffer, 0);

			long headSize = compressedStream.Position - basePosition;
			long dataSize = compressedSize - headSize;

			decoder.Code(compressedStream, decompressedStream, dataSize, decompressedSize, null);
			if (compressedStream.Position > basePosition + compressedSize)
			{
				throw new Exception($"Read {compressedStream.Position - basePosition} more than expected {compressedSize}");
			}
			compressedStream.Position = basePosition + compressedSize;
		}

		private static byte[] GetBuffer()
		{
			if (s_buffer == null)
			{
				s_buffer = new byte[UncompressedSize];
			}
			return s_buffer;
		}

		private const int PropertiesSize = 5;
		private const int UncompressedSize = 8;

		[ThreadStatic]
		private static byte[] s_buffer;

		public static MemoryStream StreamDecompress(MemoryStream inStream)
		{
			var decoder = new Decoder();

			inStream.Seek(0, SeekOrigin.Begin);
			var newOutStream = new MemoryStream();

			var properties = new byte[5];
			if (inStream.Read(properties, 0, 5) != 5)
				throw new Exception("input .lzma is too short");
			long outSize = 0;
			for (var i = 0; i < 8; i++)
			{
				var v = inStream.ReadByte();
				if (v < 0)
					throw new Exception("Can't Read 1");
				outSize |= (long)(byte)v << 8 * i;
			}
			decoder.SetDecoderProperties(properties);

			var compressedSize = inStream.Length - inStream.Position;
			decoder.Code(inStream, newOutStream, compressedSize, outSize, null);

			newOutStream.Position = 0;
			return newOutStream;
		}

		public static void StreamDecompress(Stream compressedStream, Stream decompressedStream, long compressedSize, long decompressedSize)
		{
			var basePosition = compressedStream.Position;
			var decoder = new Decoder();
			var properties = new byte[5];
			if (compressedStream.Read(properties, 0, 5) != 5)
				throw new Exception("input .lzma is too short");
			decoder.SetDecoderProperties(properties);
			decoder.Code(compressedStream, decompressedStream, compressedSize - 5, decompressedSize, null);
			compressedStream.Position = basePosition + compressedSize;
		}
	}
}
