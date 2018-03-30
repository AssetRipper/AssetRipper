using System;
using System.IO;
using System.Linq;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFile : IDisposable
	{
		public static bool IsBundleFile(string bundlePath)
		{
			if (!File.Exists(bundlePath))
			{
				throw new Exception($"Bundle at path '{bundlePath}' doesn't exist");
			}

			using (FileStream stream = File.OpenRead(bundlePath))
			{
				return IsBundleFile(stream);
			}
		}

		public static bool IsBundleFile(Stream baseStream)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = stream.BaseStream.Position;
				string signature = stream.ReadStringZeroTerm();
				bool isBundle = BundleHeader.TryParseSignature(signature, out BundleType _);
				stream.BaseStream.Position = position;

				return isBundle;
			}
		}

		public void Load(string bundlePath)
		{
			if(!File.Exists(bundlePath))
			{
				throw new Exception($"Bundle at path '{bundlePath}' doesn't exist");
			}

			FileStream stream = File.OpenRead(bundlePath);
			try
			{
				FileData = Read(stream, true);
			}
			catch
			{
				stream.Dispose();
				throw;
			}
		}

		public void Read(Stream baseStream)
		{
			FileData = Read(baseStream, false);
		}

		public void Dispose()
		{
			if(m_isDisposable)
			{
				FileData.Dispose();
				m_isDisposable = false;
			}
		}

		private BundleFileData Read(Stream baseStream, bool isClosable)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = stream.BaseStream.Position;
				Header.Read(stream);
				if (Header.Generation < BundleGeneration.BF_530_x)
				{
					return ReadPre530Metadata(stream, isClosable);
				}
				else
				{
					return Read530Metadata(stream, isClosable, position);
				}
			}
		}

		private BundleFileData ReadPre530Metadata(EndianStream stream, bool isClosable)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
				{
					if (Header.ChunkInfos.Count > 1)
					{
						throw new NotSupportedException($"Raw data with several chunks {Header.ChunkInfos.Count} isn't supported");
					}
					
					BundleMetadata metadata = new BundleMetadata(stream.BaseStream, isClosable);
					metadata.ReadPre530Metadata(stream);
					
					return new BundleFileData(metadata);
				}

				case BundleType.UnityWeb:
				case BundleType.HexFA:
				{
					BundleMetadata[] metadatas = new BundleMetadata[Header.ChunkInfos.Count];
					for(int i = 0; i < Header.ChunkInfos.Count; i++)
					{
						ChunkInfo chunkInfo = Header.ChunkInfos[i];
						MemoryStream memStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
						SevenZipHelper.DecompressLZMASizeStream(stream.BaseStream, chunkInfo.CompressedSize, memStream);
						
						BundleMetadata metadata = new BundleMetadata(memStream, true);
						using (EndianStream decompressStream = new EndianStream(memStream, EndianType.BigEndian))
						{
							metadata.ReadPre530Metadata(decompressStream);
						}
						metadatas[i] = metadata;
					}
					if(isClosable)
					{
						stream.Dispose();
					}
					
					return new BundleFileData(metadatas);
				}

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
			}
		}

		private BundleFileData Read530Metadata(EndianStream stream, bool isClosable, long basePosition)
		{
			long dataPosition = stream.BaseStream.Position;
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				stream.BaseStream.Position = basePosition + Header.BundleSize - Header.MetadataCompressedSize;
			}
			else
			{
				dataPosition += Header.MetadataCompressedSize;
			}

			BlockInfo[] blockInfos;
			BundleMetadata metadata;
			BundleCompressType metaCompress = Header.Flags.GetCompression();
			switch(metaCompress)
			{
				case BundleCompressType.None:
				{
					long metaPosition = stream.BaseStream.Position;
					
					// unknown 0x10
					stream.BaseStream.Position += 0x10;
					blockInfos = stream.ReadArray<BlockInfo>();
					metadata = new BundleMetadata(stream.BaseStream, isClosable);
					metadata.Read530Metadata(stream, dataPosition);
					
					if(stream.BaseStream.Position != metaPosition + Header.MetadataDecompressedSize)
					{
						throw new Exception($"Read {stream.BaseStream.Position - metaPosition} but expected {Header.MetadataDecompressedSize}");
					}
					break;
				}

				case BundleCompressType.LZMA:
				{
					using (MemoryStream memStream = new MemoryStream(Header.MetadataDecompressedSize))
					{
						SevenZipHelper.DecompressLZMASizeStream(stream.BaseStream, Header.MetadataCompressedSize, memStream);
						memStream.Position = 0;

						using (EndianStream metadataStream = new EndianStream(memStream, EndianType.BigEndian))
						{
							// unknown 0x10
							metadataStream.BaseStream.Position += 0x10;
							blockInfos = metadataStream.ReadArray<BlockInfo>();
							metadata = new BundleMetadata(stream.BaseStream, isClosable);
							metadata.Read530Metadata(metadataStream, dataPosition);
								
							if(memStream.Position != memStream.Length)
							{
								throw new Exception($"Read {memStream.Position} but expected {memStream.Length}");
							}
						}
					}
					break;
				}

				case BundleCompressType.LZ4:
				case BundleCompressType.LZ4HZ:
				{
					using (MemoryStream memStream = new MemoryStream(Header.MetadataDecompressedSize))
					{
						using (Lz4Stream lzStream = new Lz4Stream(stream.BaseStream, Header.MetadataCompressedSize))
						{
							long read = lzStream.Read(memStream, Header.MetadataDecompressedSize);
							memStream.Position = 0;
							
							if(read != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {read} but expected {Header.MetadataDecompressedSize}");
							}
						}

						using (EndianStream metadataStream = new EndianStream(memStream, EndianType.BigEndian))
						{
							// unknown 0x10
							metadataStream.BaseStream.Position += 0x10;
							blockInfos = metadataStream.ReadArray<BlockInfo>();
							metadata = new BundleMetadata(stream.BaseStream, isClosable);
							metadata.Read530Metadata(metadataStream, dataPosition);
							
							if (memStream.Position != memStream.Length)
							{
								throw new Exception($"Read {memStream.Position} but expected {memStream.Length}");
							}
						}
					}
					break;
				}

				default:
					throw new NotSupportedException($"Bundle compression '{metaCompress}' isn't supported");
			}

			stream.BaseStream.Position = dataPosition;
			return Read530Blocks(stream, isClosable, blockInfos, metadata);
		}
				
		private BundleFileData Read530Blocks(EndianStream stream, bool isClosable, BlockInfo[] blockInfos, BundleMetadata metadata)
		{
			// Special case. If bundle has no compressed blocks then pass it as a stream
			if(blockInfos.All(t => t.Flags.GetCompression() == BundleCompressType.None))
			{
				return new BundleFileData(metadata);
			}

			long dataPosisition = stream.BaseStream.Position;
			long decompressedSize = blockInfos.Sum(t => t.DecompressedSize);
			if (decompressedSize > int.MaxValue)
			{
				throw new Exception("How to read such big data? Save to file and then read?");
			}

			MemoryStream memStream = new MemoryStream((int)decompressedSize);
			foreach (BlockInfo blockInfo in blockInfos)
			{
				BundleCompressType compressType = blockInfo.Flags.GetCompression();
				switch (compressType)
				{
					case BundleCompressType.None:
						stream.BaseStream.CopyStream(memStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZMA:
						SevenZipHelper.DecompressLZMAStream(stream.BaseStream, blockInfo.CompressedSize, memStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZ4:
					case BundleCompressType.LZ4HZ:
						using (Lz4Stream lzStream = new Lz4Stream(stream.BaseStream, blockInfo.CompressedSize))
						{
							long read = lzStream.Read(memStream, blockInfo.DecompressedSize);
							if(read != blockInfo.DecompressedSize)
							{
								throw new Exception($"Read {read} but expected {blockInfo.CompressedSize}");
							}
						}
						break;

					default:
						throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
				}
			}
			
			if (isClosable)
			{
				stream.Dispose();
			}

			BundleFileEntry[] entries = new BundleFileEntry[metadata.Entries.Count];
			for(int i = 0; i < metadata.Entries.Count; i++)
			{
				BundleFileEntry bundleEntry = metadata.Entries[i];
				string name = bundleEntry.Name;
				long offset = bundleEntry.Offset - dataPosisition;
				long size = bundleEntry.Size;
				BundleFileEntry streamEntry = new BundleFileEntry(memStream, name, offset, size);
				entries[i] = streamEntry;
			}
			BundleMetadata streamMetadata = new BundleMetadata(memStream, true, entries);
			return new BundleFileData(streamMetadata);
		}
		
		public BundleHeader Header { get; } = new BundleHeader();
		public BundleFileData FileData
		{
			get => m_fileData;
			set
			{
				m_fileData = value;
				m_isDisposable = value != null;
			}
		}

		private BundleFileData m_fileData;
		private bool m_isDisposable = false;
	}
}