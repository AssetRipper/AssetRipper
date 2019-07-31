using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;

namespace uTinyRipper.BundleFiles
{
	public sealed class BundleFileScheme : FileSchemeList
	{
		private struct FileEntryOffset
		{
			public FileEntryOffset(SmartStream stream, long baseOffset)
			{
				Stream = stream;
				Offset = baseOffset;
			}

			public override string ToString()
			{
				return Stream == null ? base.ToString() : $"S:{Stream.StreamType} O:{Offset}";
			}

			public SmartStream Stream { get; }
			public long Offset { get; }
		}

		private BundleFileScheme(SmartStream stream, long offset, long size, string filePath, string fileName) :
			base(stream, offset, size, filePath, fileName)
		{
		}

		internal static BundleFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			BundleFileScheme scheme = new BundleFileScheme(stream, offset, size, filePath, fileName);
			scheme.ReadScheme();
			scheme.ProcessEntries();
			return scheme;
		}

		public BundleFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			BundleFile bundle = new BundleFile(collection, this);
			foreach (FileScheme scheme in Schemes)
			{
				bundle.AddFile(scheme, collection, manager);
			}
			return bundle;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			foreach (FileEntryOffset offset in m_entryStreams.Values)
			{
				offset.Stream.Dispose();
			}
		}

		private void ReadScheme()
		{
			using (PartialStream stream = new PartialStream(m_stream, m_offset, m_size))
			{
				using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
				{
					Header.Read(reader);
				}

				long headerSize = stream.Position;
				using (BundleFileReader reader = new BundleFileReader(stream, EndianType.BigEndian, Header.Generation))
				{
					if (reader.Generation < BundleGeneration.BF_530_x)
					{
						using (SmartStream dataStream = ReadPre530Metadata(reader))
						{
							ReadPre530Blocks(dataStream);
						}
					}
					else
					{
						using (SmartStream dataStream = Read530Metadata(reader))
						{
							Read530Blocks(dataStream, headerSize);
						}
					}
				}
			}
		}

		private void ProcessEntries()
		{
			foreach (BundleFileEntry entry in Metadata.Entries.Values)
			{
				FileEntryOffset offset = m_entryStreams[entry];
				FileScheme scheme = FileCollection.ReadScheme(offset.Stream, offset.Offset, entry.Size, FilePath, entry.NameOrigin);
				AddScheme(scheme);
			}
		}

		private SmartStream ReadPre530Metadata(BundleFileReader reader)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
					{
						Metadata.Read(reader);
						return m_stream.CreateReference();
					}

				case BundleType.UnityWeb:
					{
						// read only last chunk. wtf?
						ChunkInfo chunkInfo = Header.ChunkInfos[Header.ChunkInfos.Count - 1];
						using (SmartStream stream = SmartStream.CreateMemory(new byte[chunkInfo.DecompressedSize]))
						{
							SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, chunkInfo.CompressedSize, stream);
							using (BundleFileReader decompressReader = new BundleFileReader(stream, reader.EndianType, reader.Generation))
							{
								Metadata.Read(decompressReader);
							}
							return stream.CreateReference();
						}
					}

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
			}
		}

		private SmartStream Read530Metadata(BundleFileReader reader)
		{
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				reader.BaseStream.Position = Header.BundleSize - Header.MetadataCompressedSize;
			}

			BundleCompressType metaCompression = Header.Flags.GetCompression();
			switch (metaCompression)
			{
				case BundleCompressType.None:
					{
						Metadata.Read(reader);
						long expectedPosition = Header.Flags.IsMetadataAtTheEnd() ? Header.BundleSize : Header.HeaderSize + Header.MetadataDecompressedSize;
						if (reader.BaseStream.Position != expectedPosition)
						{
							throw new Exception($"Read {reader.BaseStream.Position - Header.HeaderSize} but expected {Header.MetadataDecompressedSize}");
						}
					}
					break;

				case BundleCompressType.LZMA:
					{
						using (MemoryStream stream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, Header.MetadataCompressedSize, stream);
							using (BundleFileReader decompressReader = new BundleFileReader(stream, reader.EndianType, reader.Generation))
							{
								Metadata.Read(decompressReader);
							}
							if (stream.Position != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {stream.Position} but expected {Header.MetadataDecompressedSize}");
							}
						}
					}
					break;

				case BundleCompressType.LZ4:
				case BundleCompressType.LZ4HZ:
					{
						using (MemoryStream stream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							using (Lz4DecodeStream decodeStream = new Lz4DecodeStream(reader.BaseStream, Header.MetadataCompressedSize))
							{
								long read = decodeStream.Read(stream, Header.MetadataDecompressedSize);
								if (read != Header.MetadataDecompressedSize)
								{
									throw new Exception($"Read {read} but expected {Header.MetadataDecompressedSize}");
								}
								if (decodeStream.IsDataLeft)
								{
									throw new Exception($"LZ4 stream still has some data");
								}
							}

							stream.Position = 0;
							using (BundleFileReader decompressReader = new BundleFileReader(stream, reader.EndianType, reader.Generation))
							{
								Metadata.Read(decompressReader);
							}
							if (stream.Position != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {stream.Position} but expected {Header.MetadataDecompressedSize}");
							}
						}
					}
					break;

				default:
					throw new NotSupportedException($"Bundle compression '{metaCompression}' isn't supported");
			}
			return m_stream.CreateReference();
		}

		private void ReadPre530Blocks(SmartStream dataStream)
		{
			long baseOffset;
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
					baseOffset = Header.HeaderSize;
					break;
				case BundleType.UnityWeb:
					baseOffset = 0;
					break;

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
			}

			foreach (BundleFileEntry entry in Metadata.Entries.Values)
			{
				FileEntryOffset feOffset = new FileEntryOffset(dataStream.CreateReference(), baseOffset + entry.Offset);
				m_entryStreams.Add(entry, feOffset);
			}
		}

		private void Read530Blocks(SmartStream dataStream, long headerSize)
		{
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				dataStream.Position = headerSize;
			}

			int cachedBlock = -1;
			long dataOffset = dataStream.Position;

			// If MemoryStream has compressed block then we need to create individual streams for each entry and copy its data into it
			bool createIndividualStreams = dataStream.StreamType == SmartStreamType.Memory;
			if (createIndividualStreams)
			{
				// find out if this bundle file has compressed blocks
				foreach (BlockInfo block in Metadata.BlockInfos)
				{
					if (block.Flags.GetCompression() != BundleCompressType.None)
					{
						createIndividualStreams = true;
						break;
					}
				}
			}

			using (SmartStream blockStream = SmartStream.CreateNull())
			{
				foreach (BundleFileEntry entry in Metadata.Entries.Values)
				{
					// find out block offsets
					long blockCompressedOffset = 0;
					long blockDecompressedOffset = 0;
					int blockIndex = 0;
					while (blockDecompressedOffset + Metadata.BlockInfos[blockIndex].DecompressedSize <= entry.Offset)
					{
						blockCompressedOffset += Metadata.BlockInfos[blockIndex].CompressedSize;
						blockDecompressedOffset += Metadata.BlockInfos[blockIndex].DecompressedSize;
						blockIndex++;
					}

					// if at least one block of this entry is compressed or acording to the rule above
					// we should copy the data of current entry to a separate stream
					bool needToCopy = createIndividualStreams;
					if (!needToCopy)
					{
						// check if this entry has compressed blocks
						long entrySize = 0;
						for (int bi = blockIndex; entrySize < entry.Size; bi++)
						{
							if (Metadata.BlockInfos[bi].Flags.GetCompression() != BundleCompressType.None)
							{
								// it does. then we need to create individual stream and decomress its data into it
								needToCopy = true;
								break;
							}
							entrySize += Metadata.BlockInfos[bi].DecompressedSize;
						}
					}

					long entryOffsetInsideBlock = entry.Offset - blockDecompressedOffset;
					if (needToCopy)
					{
						// well, at leat one block is compressed so we should copy data of current entry to a separate stream
						using (SmartStream entryStream = CreateStream(entry.Size))
						{
							long left = entry.Size;
							dataStream.Position = dataOffset + blockCompressedOffset;

							// copy data of all blocks used by current entry to new stream
							for (int bi = blockIndex; left > 0; bi++)
							{
								long blockOffset = 0;
								BlockInfo block = Metadata.BlockInfos[bi];
								if (cachedBlock == bi)
								{
									// some data of previous entry is in the same block as this one
									// so we don't need to unpack it once again. Instead we can use cached stream
									dataStream.Position += block.CompressedSize;
								}
								else
								{
									BundleCompressType compressType = block.Flags.GetCompression();
									switch (compressType)
									{
										case BundleCompressType.None:
											blockOffset = dataOffset + blockCompressedOffset;
											blockStream.Assign(dataStream);
											break;

										case BundleCompressType.LZMA:
											blockStream.Move(CreateStream(block.DecompressedSize));
											SevenZipHelper.DecompressLZMAStream(dataStream, block.CompressedSize, blockStream, block.DecompressedSize);
											break;

										case BundleCompressType.LZ4:
										case BundleCompressType.LZ4HZ:
											blockStream.Move(CreateStream(block.DecompressedSize));
											using (Lz4DecodeStream lzStream = new Lz4DecodeStream(dataStream, block.CompressedSize))
											{
												long read = lzStream.Read(blockStream, block.DecompressedSize);
												if (read != block.DecompressedSize)
												{
													throw new Exception($"Read {read} but expected {block.DecompressedSize}");
												}
												if (lzStream.IsDataLeft)
												{
													throw new Exception($"LZ4 stream still has some data");
												}
											}
											break;

										default:
											throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
									}
									cachedBlock = bi;
								}

								// consider next offsets:
								// 1) block - if it is new stream then offset is 0, otherwise offset of this block in the bundle file
								// 2) entry - if this is first block for current entry then it is offset of this entry related to this block
								//			  otherwise 0
								long fragmentSize = block.DecompressedSize - entryOffsetInsideBlock;
								blockStream.Position = blockOffset + entryOffsetInsideBlock;
								entryOffsetInsideBlock = 0;

								long size = Math.Min(fragmentSize, left);
								blockStream.CopyStream(entryStream, size);

								blockCompressedOffset += block.CompressedSize;
								left -= size;
							}
							if (left < 0)
							{
								throw new Exception($"Read more than expected");
							}

							FileEntryOffset feOffset = new FileEntryOffset(entryStream.CreateReference(), 0);
							m_entryStreams.Add(entry, feOffset);
						}
					}
					else
					{
						// no compressed blocks was found so we can use original bundle stream
						// since FileEntry.Offset contains decompressedOffset we need to preliminarily subtract it
						FileEntryOffset feOffset = new FileEntryOffset(dataStream.CreateReference(), dataOffset + blockCompressedOffset + entryOffsetInsideBlock);
						m_entryStreams.Add(entry, feOffset);
					}
				}
			}
		}

		private SmartStream CreateStream(long decompressedSize)
		{
			return decompressedSize > int.MaxValue ? SmartStream.CreateTemp() : SmartStream.CreateMemory(new byte[decompressedSize]);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; } = new BundleMetadata();

		public override FileEntryType SchemeType => FileEntryType.Bundle;

		private readonly Dictionary<BundleFileEntry, FileEntryOffset> m_entryStreams = new Dictionary<BundleFileEntry, FileEntryOffset>();
	}
}
