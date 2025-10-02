using AssetRipper.IO.Files.Exceptions;
using AssetRipper.IO.Files.Streams;
using AssetRipper.IO.Files.Streams.Smart;
using K4os.Compression.LZ4;
using System.Buffers;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

internal sealed class BundleFileBlockReader : IDisposable
{
	public BundleFileBlockReader(SmartStream stream, BlocksInfo blocksInfo)
	{
		m_stream = stream;
		m_blocksInfo = blocksInfo;
		m_dataOffset = stream.Position;
	}

	~BundleFileBlockReader()
	{
		Dispose(false);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public SmartStream ReadEntry(FileStreamNode entry)
	{
		ObjectDisposedException.ThrowIf(m_isDisposed, typeof(BundleFileBlockReader));

		// Avoid storing entire non-compresed entries in memory by mapping a stream to the block location.
		if (m_blocksInfo.StorageBlocks.Length == 1 && m_blocksInfo.StorageBlocks[0].CompressionType == CompressionType.None)
		{
			return m_stream.CreatePartial(m_dataOffset + entry.Offset, entry.Size);
		}

		// find block offsets
		int blockIndex;
		long blockCompressedOffset = 0;
		long blockDecompressedOffset = 0;
		for (blockIndex = 0; blockDecompressedOffset + m_blocksInfo.StorageBlocks[blockIndex].UncompressedSize <= entry.Offset; blockIndex++)
		{
			blockCompressedOffset += m_blocksInfo.StorageBlocks[blockIndex].CompressedSize;
			blockDecompressedOffset += m_blocksInfo.StorageBlocks[blockIndex].UncompressedSize;
		}
		long entryOffsetInsideBlock = entry.Offset - blockDecompressedOffset;

		using SmartStream entryStream = CreateStream(entry.Size);
		long left = entry.Size;
		m_stream.Position = m_dataOffset + blockCompressedOffset;

		// copy data of all blocks used by current entry to new stream
		while (left > 0)
		{
			byte[]? rentedArray;

			long blockStreamOffset;
			Stream blockStream;
			StorageBlock block = m_blocksInfo.StorageBlocks[blockIndex];
			if (m_cachedBlockIndex == blockIndex)
			{
				// data of the previous entry is in the same block as this one
				// so we don't need to unpack it once again. Instead we can use cached stream
				blockStreamOffset = 0;
				blockStream = m_cachedBlockStream;
				rentedArray = null;
				m_stream.Position += block.CompressedSize;
			}
			else
			{
				CompressionType compressType = block.CompressionType;
				if (compressType is CompressionType.None)
				{
					blockStreamOffset = m_dataOffset + blockCompressedOffset;
					blockStream = m_stream;
					rentedArray = null;
				}
				else
				{
					blockStreamOffset = 0;
					m_cachedBlockIndex = blockIndex;
					m_cachedBlockStream.Move(CreateTemporaryStream(block.UncompressedSize, out rentedArray));
					switch (compressType)
					{
						case CompressionType.Lzma:
							LzmaCompression.DecompressLzmaStream(m_stream, block.CompressedSize, m_cachedBlockStream, block.UncompressedSize);
							break;

						case CompressionType.Lz4:
						case CompressionType.Lz4HC:
							uint uncompressedSize = block.UncompressedSize;
							byte[] uncompressedBytes = new byte[uncompressedSize];
							byte[] compressedBytes = new BinaryReader(m_stream).ReadBytes((int)block.CompressedSize);
							int bytesWritten = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
							if (bytesWritten < 0)
							{
								DecompressionFailedException.ThrowNoBytesWritten(entry.PathFixed, compressType);
							}
							else if (bytesWritten != uncompressedSize)
							{
								DecompressionFailedException.ThrowIncorrectNumberBytesWritten(entry.PathFixed, compressType, uncompressedSize, bytesWritten);
							}
							new MemoryStream(uncompressedBytes).CopyTo(m_cachedBlockStream);
							break;

						case CompressionType.Lzham:
							UnsupportedBundleDecompression.ThrowLzham(entry.PathFixed);
							break;

						default:
							if (ZstdCompression.IsZstd(m_stream))
							{
								ZstdCompression.DecompressStream(m_stream, block.CompressedSize, m_cachedBlockStream, block.UncompressedSize);
							}
							else
							{
								UnsupportedBundleDecompression.Throw(entry.PathFixed, compressType);
							}
							break;
					}
					blockStream = m_cachedBlockStream;
				}
			}

			// consider next offsets:
			// 1) block - if it is new stream then offset is 0, otherwise offset of this block in the bundle file
			// 2) entry - if this is first block for current entry then it is offset of this entry related to this block
			//			  otherwise 0
			long blockSize = block.UncompressedSize - entryOffsetInsideBlock;
			blockStream.Position = blockStreamOffset + entryOffsetInsideBlock;
			entryOffsetInsideBlock = 0;

			long size = Math.Min(blockSize, left);
			using PartialStream partialStream = new(blockStream, blockStream.Position, size);
			partialStream.CopyTo(entryStream);
			blockIndex++;

			blockCompressedOffset += block.CompressedSize;
			left -= size;

			if (rentedArray != null)
			{
				ArrayPool<byte>.Shared.Return(rentedArray);
			}
		}
		if (left < 0)
		{
			DecompressionFailedException.ThrowReadMoreThanExpected(entry.PathFixed, entry.Size, entry.Size - left);
		}
		entryStream.Position = 0;
		return entryStream.CreateReference();
	}

	private void Dispose(bool disposing)
	{
		m_isDisposed = true;
		m_cachedBlockStream.FreeReference();
	}

	private static SmartStream CreateStream(long decompressedSize)
	{
		return decompressedSize switch
		{
			> MaxMemoryStreamLength => SmartStream.CreateTemp(),
			> MaxPreAllocatedMemoryStreamLength => SmartStream.CreateMemory(),
			_ => SmartStream.CreateMemory(new byte[decompressedSize]),
		};
	}

	private static SmartStream CreateTemporaryStream(long decompressedSize, out byte[]? rentedArray)
	{
		if (decompressedSize > MaxMemoryStreamLength)
		{
			rentedArray = null;
			return SmartStream.CreateTemp();
		}
		else
		{
			rentedArray = ArrayPool<byte>.Shared.Rent((int)decompressedSize);
			return SmartStream.CreateMemory(rentedArray, 0, (int)decompressedSize);
		}
	}

	/// <summary>
	/// The arbitrary maximum size of a decompressed stream to be stored in RAM. 2 GB
	/// </summary>
	/// <remarks>
	/// This number can be set to any integer value, including <see cref="int.MaxValue"/>.
	/// </remarks>
	private const int MaxMemoryStreamLength = int.MaxValue;
	/// <summary>
	/// The arbitrary maximum size of a decompressed stream to be pre-allocated. 100 MB
	/// </summary>
	/// <remarks>
	/// This number can be set to any integer value less than <see cref="MaxMemoryStreamLength"/>.
	/// </remarks>
	private const int MaxPreAllocatedMemoryStreamLength = 100 * 1024 * 1024;
	private readonly SmartStream m_stream;
	private readonly BlocksInfo m_blocksInfo = new();
	private readonly long m_dataOffset;

	private readonly SmartStream m_cachedBlockStream = SmartStream.CreateNull();
	private int m_cachedBlockIndex = -1;

	private bool m_isDisposed = false;
}
