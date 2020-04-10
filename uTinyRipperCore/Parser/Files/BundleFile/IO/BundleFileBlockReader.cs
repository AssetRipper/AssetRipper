using System;
using System.IO;
using uTinyRipper.Lz4;

namespace uTinyRipper.BundleFiles
{
	internal sealed class BundleFileBlockReader : IDisposable
	{
		public BundleFileBlockReader(Stream stream, BlocksInfo blocksInfo)
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

		public SmartStream ReadEntry(Node entry)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(nameof(BundleFileBlockReader));
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

			using (SmartStream entryStream = CreateStream(entry.Size))
			{
				long left = entry.Size;
				m_stream.Position = m_dataOffset + blockCompressedOffset;

				// copy data of all blocks used by current entry to new stream
				while (left > 0)
				{
					long blockStreamOffset;
					Stream blockStream;
					StorageBlock block = m_blocksInfo.StorageBlocks[blockIndex];
					if (m_cachedBlockIndex == blockIndex)
					{
						// data of the previous entry is in the same block as this one
						// so we don't need to unpack it once again. Instead we can use cached stream
						blockStreamOffset = 0;
						blockStream = m_cachedBlockStream;
						m_stream.Position += block.CompressedSize;
					}
					else
					{
						CompressionType compressType = block.Flags.GetCompression();
						if (compressType == CompressionType.None)
						{
							blockStreamOffset = m_dataOffset + blockCompressedOffset;
							blockStream = m_stream;
						}
						else
						{
							blockStreamOffset = 0;
							m_cachedBlockIndex = blockIndex;
							m_cachedBlockStream.Move(CreateStream(block.UncompressedSize));
							switch (compressType)
							{
								case CompressionType.Lzma:
									SevenZipHelper.DecompressLZMAStream(m_stream, block.CompressedSize, m_cachedBlockStream, block.UncompressedSize);
									break;

								case CompressionType.Lz4:
								case CompressionType.Lz4HC:
									using (Lz4DecodeStream lzStream = new Lz4DecodeStream(m_stream, block.CompressedSize))
									{
										lzStream.ReadBuffer(m_cachedBlockStream, block.UncompressedSize);
									}
									break;

								default:
									throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
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
					blockStream.CopyStream(entryStream, size);
					blockIndex++;

					blockCompressedOffset += block.CompressedSize;
					left -= size;
				}
				if (left < 0)
				{
					throw new Exception($"Read more than expected");
				}
				entryStream.Position = 0;
				return entryStream.CreateReference();
			}
		}

		private void Dispose(bool disposing)
		{
			m_isDisposed = true;
			m_cachedBlockStream.Dispose();
		}

		private SmartStream CreateStream(long decompressedSize)
		{
			return decompressedSize > int.MaxValue ? SmartStream.CreateTemp() : SmartStream.CreateMemory(new byte[decompressedSize]);
		}

		private readonly Stream m_stream;
		private readonly BlocksInfo m_blocksInfo;
		private readonly long m_dataOffset;

		private readonly SmartStream m_cachedBlockStream = SmartStream.CreateNull();
		private int m_cachedBlockIndex = -1;

		private bool m_isDisposed = false;
	}
}
