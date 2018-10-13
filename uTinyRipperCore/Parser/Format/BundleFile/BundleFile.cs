using System;
using System.IO;
using System.Linq;

namespace uTinyRipper.BundleFiles
{
	public sealed class BundleFile : IDisposable
	{
		private BundleFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			m_filePath = filePath;
		}

		~BundleFile()
		{
			Dispose(false);
		}
		
		public static bool IsBundleFile(string bundlePath)
		{
			if (!FileMultiStream.Exists(bundlePath))
			{
				throw new Exception($"Bundle at path '{bundlePath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(bundlePath))
			{
				return IsBundleFile(stream);
			}
		}

		public static bool IsBundleFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				bool isBundle = false;
				long position = reader.BaseStream.Position;
				if(reader.BaseStream.Length - position > 0x20)
				{
					if (reader.ReadStringZeroTerm(0x20, out string signature))
					{
						isBundle = BundleHeader.TryParseSignature(signature, out BundleType _);
					}
				}
				reader.BaseStream.Position = position;

				return isBundle;
			}
		}

		public static BundleFile Load(string bundlePath)
		{
			BundleFile bundle = new BundleFile(bundlePath);
			bundle.Load();
			return bundle;
		}

		public static BundleFile Read(SmartStream stream, string bundlePath)
		{
			BundleFile bundle = new BundleFile(bundlePath);
			bundle.Read(stream);
			return bundle;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if(Metadata != null)
			{
				Metadata.Dispose();
			}
		}

		private void Load()
		{
			if (!FileUtils.Exists(m_filePath))
			{
				throw new Exception($"BundleFile at path '{m_filePath}' doesn't exist");
			}

			using (SmartStream stream = SmartStream.OpenRead(m_filePath))
			{
				Read(stream);
			}
		}

		private void Read(SmartStream stream)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				long position = reader.BaseStream.Position;
				Header.Read(reader);
				if (Header.Generation < BundleGeneration.BF_530_x)
				{
					ReadPre530Metadata(reader);
				}
				else
				{
					Read530Metadata(reader, position);
				}
			}
		}

		private void ReadPre530Metadata(EndianReader reader)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
					{
						Metadata = new BundleMetadata(m_filePath);
						Metadata.ReadPre530(reader);
					}
					break;

				case BundleType.UnityWeb:
				case BundleType.HexFA:
					{
						// read only last chunk. wtf?
						ChunkInfo chunkInfo = Header.ChunkInfos[Header.ChunkInfos.Count - 1];
						using (SmartStream stream = SmartStream.CreateMemory(new byte[chunkInfo.DecompressedSize]))
						{
							SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, chunkInfo.CompressedSize, stream);
							Metadata = new BundleMetadata(m_filePath);
							using (EndianReader decompressReader = new EndianReader(stream, EndianType.BigEndian))
							{
								Metadata.ReadPre530(decompressReader);
							}
						}
					}
					break;

					default:
						throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
				}
		}

		private void Read530Metadata(EndianReader reader, long basePosition)
		{
			SmartStream bundleStream = (SmartStream)reader.BaseStream;
			long dataPosition = bundleStream.Position;
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				bundleStream.Position = basePosition + Header.BundleSize - Header.MetadataCompressedSize;
			}
			else
			{
				dataPosition += Header.MetadataCompressedSize;
			}

			BlockInfo[] blockInfos;
			BundleCompressType metaCompression = Header.Flags.GetCompression();
			switch(metaCompression)
			{
				case BundleCompressType.None:
					{
						long metaPosition = bundleStream.Position;

						// unknown 0x10
						bundleStream.Position += 0x10;
						blockInfos = reader.ReadArray<BlockInfo>();
						Metadata = new BundleMetadata(m_filePath);
						Metadata.Read530(reader, bundleStream);
					
						if(bundleStream.Position != metaPosition + Header.MetadataDecompressedSize)
						{
							throw new Exception($"Read {bundleStream.Position - metaPosition} but expected {Header.MetadataDecompressedSize}");
						}
						break;
					}

				case BundleCompressType.LZMA:
					{
						using (MemoryStream metaStream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							SevenZipHelper.DecompressLZMASizeStream(bundleStream, Header.MetadataCompressedSize, metaStream);
							using (EndianReader metaReader = new EndianReader(metaStream, EndianType.BigEndian))
							{
								// unknown 0x10
								metaReader.BaseStream.Position += 0x10;
								blockInfos = metaReader.ReadArray<BlockInfo>();
								Metadata = new BundleMetadata(m_filePath);
								Metadata.Read530(metaReader, bundleStream);
							}

							if (metaStream.Position != metaStream.Length)
							{
								throw new Exception($"Read {metaStream.Position} but expected {metaStream.Length}");
							}
						}
						break;
					}

				case BundleCompressType.LZ4:
				case BundleCompressType.LZ4HZ:
					{
						using (MemoryStream metaStream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							using (Lz4Stream lzStream = new Lz4Stream(bundleStream, Header.MetadataCompressedSize))
							{
								long read = lzStream.Read(metaStream, Header.MetadataDecompressedSize);
								metaStream.Position = 0;
							
								if(read != Header.MetadataDecompressedSize)
								{
									throw new Exception($"Read {read} but expected {Header.MetadataDecompressedSize}");
								}
							}

							using (EndianReader metaReader = new EndianReader(metaStream, EndianType.BigEndian))
							{
								// unknown 0x10
								metaReader.BaseStream.Position += 0x10;
								blockInfos = metaReader.ReadArray<BlockInfo>();
								Metadata = new BundleMetadata(m_filePath);
								Metadata.Read530(metaReader, bundleStream);
							}

							if (metaStream.Position != metaStream.Length)
							{
								throw new Exception($"Read {metaStream.Position} but expected {metaStream.Length}");
							}
						}
						break;
					}

				default:
					throw new NotSupportedException($"Bundle compression '{metaCompression}' isn't supported");
			}

			bundleStream.Position = dataPosition;
			Read530Blocks(bundleStream, blockInfos);
		}
				
		private void Read530Blocks(SmartStream bundleStream, BlockInfo[] blockInfos)
		{
			int cachedBlock = -1;
			long dataOffset = bundleStream.Position;
			BundleFileEntry[] newEntries = new BundleFileEntry[Metadata.Entries.Count];
			using (SmartStream blockStream = SmartStream.CreateNull())
			{
				for (int ei = 0; ei < Metadata.Entries.Count; ei++)
				{
					BundleFileEntry entry = Metadata.Entries[ei];

					// find block corresponding to current entry
					int blockIndex = 0;
					long compressedOffset = 0;
					long decompressedOffset = 0;
					while (true)
					{
						BlockInfo block = blockInfos[blockIndex];
						if (decompressedOffset + block.DecompressedSize > entry.Offset)
						{
							break;
						}
						blockIndex++;
						compressedOffset += block.CompressedSize;
						decompressedOffset += block.DecompressedSize;
					}

					// check does this entry use any compressed blocks
					long entrySize = 0;
					bool isCompressed = false;
					for (int bi = blockIndex; entrySize < entry.Size; bi++)
					{
						BlockInfo block = blockInfos[bi];
						entrySize += block.DecompressedSize;
						if(block.Flags.GetCompression() != BundleCompressType.None)
						{
							isCompressed = true;
							break;
						}
					}

					if(isCompressed)
					{
						// well, at leat one block is compressed so we should copy data of current entry to separate stream
						using (SmartStream entryStream = CreateStream(entry.Size))
						{
							long left = entry.Size;
							long entryOffset = entry.Offset - decompressedOffset;
							bundleStream.Position = dataOffset + compressedOffset;

							// copy data of all blocks used by current entry to created stream
							for (int bi = blockIndex; left > 0; bi++)
							{
								long blockOffset = 0;
								BlockInfo block = blockInfos[bi];
								if (cachedBlock == bi)
								{
									// some data of previous entry is in the same block as this one
									// so we don't need to unpack it once again but can use cached stream
									bundleStream.Position += block.CompressedSize;
								}
								else
								{
									BundleCompressType compressType = block.Flags.GetCompression();
									switch (compressType)
									{
										case BundleCompressType.None:
											blockOffset = dataOffset + compressedOffset;
											blockStream.Assign(bundleStream);
											break;

										case BundleCompressType.LZMA:
											blockStream.Move(CreateStream(block.DecompressedSize));
											SevenZipHelper.DecompressLZMAStream(bundleStream, block.CompressedSize, blockStream, block.DecompressedSize);
											break;

										case BundleCompressType.LZ4:
										case BundleCompressType.LZ4HZ:
											blockStream.Move(CreateStream(block.DecompressedSize));
											using (Lz4Stream lzStream = new Lz4Stream(bundleStream, block.CompressedSize))
											{
												long read = lzStream.Read(blockStream, block.DecompressedSize);
												if (read != block.DecompressedSize)
												{
													throw new Exception($"Read {read} but expected {block.CompressedSize}");
												}
											}
											break;

										default:
											throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
									}
									cachedBlock = bi;
								}

								// consider next offsets:
								// 1) block - if it is new stream then offset is 0, otherwise offset of this block in bundle file
								// 2) entry - if this is first block for current entry then it is offset of this entry related to this block
								//			  otherwise 0
								long fragmentSize = block.DecompressedSize - entryOffset;
								blockStream.Position = blockOffset + entryOffset;
								entryOffset = 0;

								long size = Math.Min(fragmentSize, left);
								blockStream.CopyStream(entryStream, size);

								compressedOffset += block.CompressedSize;
								left -= size;
							}
							if (left < 0)
							{
								throw new Exception($"Read more than expected");
							}

							newEntries[ei] = new BundleFileEntry(entryStream, entry.FilePath, entry.Name, 0, entry.Size);
						}
					}
					else
					{
						// no compressed blocks was found so we can use original bundle stream
						newEntries[ei] = new BundleFileEntry(entry, dataOffset + entry.Offset);
					}
				}
			}
			Metadata.Dispose();
			Metadata = new BundleMetadata(m_filePath, newEntries);
		}

		private SmartStream CreateStream(long decompressedSize)
		{
			return decompressedSize > int.MaxValue ? SmartStream.CreateTemp() : SmartStream.CreateMemory(new byte[decompressedSize]);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; private set; }

		private readonly string m_filePath;
	}
}