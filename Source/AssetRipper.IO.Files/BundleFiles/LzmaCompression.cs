using AssetRipper.IO.Files.Exceptions;
using SharpCompress.Compressors.LZMA;
using System.Buffers;
using System.Buffers.Binary;

namespace AssetRipper.IO.Files.BundleFiles;

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

		compressedStream.ReadExactly(properties, 0, PropertiesSize);

		long headSize = compressedStream.Position - basePosition;
		long headlessSize = compressedSize - headSize;

		DecompressLzmaStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

		if (compressedStream.Position > basePosition + compressedSize)
		{
			DecompressionFailedException.ThrowReadMoreThanExpected(CompressionType.Lzma, compressedSize, compressedStream.Position - basePosition);
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

		compressedStream.ReadExactly(properties, 0, PropertiesSize);
		compressedStream.ReadExactly(sizeBytes, 0, UncompressedSize);
		long decompressedSize = BinaryPrimitives.ReadInt64LittleEndian(sizeBytes);

		long headSize = compressedStream.Position - basePosition;
		long headlessSize = compressedSize - headSize;

		DecompressLzmaStream(properties, compressedStream, headlessSize, decompressedStream, decompressedSize);

		if (compressedStream.Position > basePosition + compressedSize)
		{
			DecompressionFailedException.ThrowReadMoreThanExpected(CompressionType.Lzma, compressedSize, compressedStream.Position - basePosition);
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

	/// <summary>
	/// Compress some data with LZMA.
	/// </summary>
	/// <param name="uncompressedStream">The source stream with uncompressed data.</param>
	/// <param name="uncompressedSize">The number of bytes to read from <paramref name="uncompressedSize"/>.</param>
	/// <param name="compressedStream">The stream in which to write the compressed data.</param>
	/// <returns>The number of compressed bytes written to <paramref name="compressedStream"/> including the 5 property bytes.</returns>
	public static long CompressLzmaStream(Stream uncompressedStream, long uncompressedSize, Stream compressedStream)
	{
		long basePosition = compressedStream.Position;
		LzmaStream lzmaStream = new LzmaStream(new(), false, compressedStream);
		compressedStream.Write(lzmaStream.Properties);
		CopyToLzma(uncompressedStream, lzmaStream, uncompressedSize);
		lzmaStream.Close();
		return compressedStream.Position - basePosition;
	}

	/// <summary>
	/// Compress some data with LZMA.
	/// </summary>
	/// <param name="uncompressedStream">The source stream with uncompressed data.</param>
	/// <param name="uncompressedSize">The number of bytes to read from <paramref name="uncompressedSize"/>.</param>
	/// <param name="compressedStream">The stream in which to write the compressed data.</param>
	/// <returns>
	/// The number of compressed bytes written to <paramref name="compressedStream"/> including the 5 property bytes
	/// and <see langword="long"/> uncompressed size value.
	/// </returns>
	public static long CompressLzmaSizeStream(Stream uncompressedStream, long uncompressedSize, Stream compressedStream)
	{
		long basePosition = compressedStream.Position;
		LzmaStream lzmaStream = new LzmaStream(new(), false, compressedStream);
		compressedStream.Write(lzmaStream.Properties);
		new BinaryWriter(compressedStream).Write(uncompressedSize);
		CopyToLzma(uncompressedStream, lzmaStream, uncompressedSize);
		lzmaStream.Close();
		return compressedStream.Position - basePosition;
	}

	private static void CopyToLzma(Stream inputStream, LzmaStream lzmaStream, long uncompressedSize)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
		long totalCopied = 0;
		while (totalCopied < uncompressedSize)
		{
			int read = inputStream.Read(buffer, 0, (int)Math.Min(buffer.Length, uncompressedSize - totalCopied));
			if (read == 0)
			{
				throw new EndOfStreamException();
			}
			lzmaStream.Write(buffer, 0, read);
			totalCopied += read;
		}
	}

	private const int PropertiesSize = 5;
	private const int UncompressedSize = sizeof(long);
}
