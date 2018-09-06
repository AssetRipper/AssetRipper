using System;
using System.IO;
using System.Linq;

namespace UtinyRipper.BundleFiles
{
	public class BundleFile : IDisposable
	{
		private BundleFile(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			m_filePath = filePath;
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

		public static BundleFile Read(Stream stream, string bundlePath)
		{
			BundleFile bundle = new BundleFile(bundlePath);
			bundle.Read(stream);
			return bundle;
		}

		public void Dispose()
		{
			Metadata.Dispose();
		}

		private void Load()
		{
			if (!FileUtils.Exists(m_filePath))
			{
				throw new Exception($"BundleFile at path '{m_filePath}' doesn't exist");
			}

			FileStream stream = FileUtils.OpenRead(m_filePath);
			try
			{
				Read(stream, true);
			}
			catch
			{
				stream.Dispose();
				throw;
			}
		}

		private void Read(Stream stream)
		{
			Read(stream, false);
		}

		private void Read(Stream stream, bool isClosable)
		{
			using (EndianReader reader = new EndianReader(stream, stream.Position, EndianType.BigEndian))
			{
				long position = reader.BaseStream.Position;
				Header.Read(reader);
				if (Header.Generation < BundleGeneration.BF_530_x)
				{
					ReadPre530Metadata(reader, isClosable);
				}
				else
				{
					Read530Metadata(reader, isClosable, position);
				}
			}
		}

		private void ReadPre530Metadata(EndianReader reader, bool isClosable)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
				{
					if (Header.ChunkInfos.Count > 1)
					{
						throw new NotSupportedException($"Raw data with several chunks {Header.ChunkInfos.Count} isn't supported");
					}

					Metadata = new BundleMetadata(reader.BaseStream, m_filePath, isClosable);
					Metadata.ReadPre530(reader);
				}
				break;

				case BundleType.UnityWeb:
				case BundleType.HexFA:
				{
					// read only last chunk. wtf?
					ChunkInfo chunkInfo = Header.ChunkInfos[Header.ChunkInfos.Count - 1];
					MemoryStream memStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
					SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, chunkInfo.CompressedSize, memStream);

					Metadata = new BundleMetadata(memStream, m_filePath, true);
					using (EndianReader decompressStream = new EndianReader(memStream, EndianType.BigEndian))
					{
						Metadata.ReadPre530(decompressStream);
					}

					if (isClosable)
					{
						reader.Dispose();
					}					
				}
				break;

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
			}
		}

		private void Read530Metadata(EndianReader reader, bool isClosable, long basePosition)
		{
			long dataPosition = reader.BaseStream.Position;
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				reader.BaseStream.Position = basePosition + Header.BundleSize - Header.MetadataCompressedSize;
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
					long metaPosition = reader.BaseStream.Position;
					
					// unknown 0x10
					reader.BaseStream.Position += 0x10;
					blockInfos = reader.ReadArray<BlockInfo>();
					metadata = new BundleMetadata(reader.BaseStream, m_filePath, isClosable);
					metadata.Read530(reader, dataPosition);
					
					if(reader.BaseStream.Position != metaPosition + Header.MetadataDecompressedSize)
					{
						throw new Exception($"Read {reader.BaseStream.Position - metaPosition} but expected {Header.MetadataDecompressedSize}");
					}
					break;
				}

				case BundleCompressType.LZMA:
				{
					using (MemoryStream memStream = new MemoryStream(Header.MetadataDecompressedSize))
					{
						SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, Header.MetadataCompressedSize, memStream);
						memStream.Position = 0;

						using (EndianReader metadataStream = new EndianReader(memStream, EndianType.BigEndian))
						{
							// unknown 0x10
							metadataStream.BaseStream.Position += 0x10;
							blockInfos = metadataStream.ReadArray<BlockInfo>();
							metadata = new BundleMetadata(reader.BaseStream, m_filePath, isClosable);
							metadata.Read530(metadataStream, dataPosition);
								
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
						using (Lz4Stream lzStream = new Lz4Stream(reader.BaseStream, Header.MetadataCompressedSize))
						{
							long read = lzStream.Read(memStream, Header.MetadataDecompressedSize);
							memStream.Position = 0;
							
							if(read != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {read} but expected {Header.MetadataDecompressedSize}");
							}
						}

						using (EndianReader metadataStream = new EndianReader(memStream, EndianType.BigEndian))
						{
							// unknown 0x10
							metadataStream.BaseStream.Position += 0x10;
							blockInfos = metadataStream.ReadArray<BlockInfo>();
							metadata = new BundleMetadata(reader.BaseStream, m_filePath, isClosable);
							metadata.Read530(metadataStream, dataPosition);
							
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

			reader.BaseStream.Position = dataPosition;
			Read530Blocks(reader, isClosable, blockInfos, metadata);
		}
				
		private void Read530Blocks(EndianReader reader, bool isClosable, BlockInfo[] blockInfos, BundleMetadata metadata)
		{
			// Special case. If bundle has no compressed blocks then pass it as is
			if(blockInfos.All(t => t.Flags.GetCompression() == BundleCompressType.None))
			{
				Metadata = metadata;
				return;
			}

			long dataPosisition = reader.BaseStream.Position;
			long decompressedSize = blockInfos.Sum(t => t.DecompressedSize);
			Stream bufferStream;
			if (decompressedSize > int.MaxValue)
			{
				string tempFile = Path.GetTempFileName();
				bufferStream = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose); 
			}
			else
			{
				bufferStream = new MemoryStream((int)decompressedSize);
			}
			
			foreach (BlockInfo blockInfo in blockInfos)
			{
				BundleCompressType compressType = blockInfo.Flags.GetCompression();
				switch (compressType)
				{
					case BundleCompressType.None:
						reader.BaseStream.CopyStream(bufferStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZMA:
						SevenZipHelper.DecompressLZMAStream(reader.BaseStream, blockInfo.CompressedSize, bufferStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZ4:
					case BundleCompressType.LZ4HZ:
						using (Lz4Stream lzStream = new Lz4Stream(reader.BaseStream, blockInfo.CompressedSize))
						{
							long read = lzStream.Read(bufferStream, blockInfo.DecompressedSize);
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
				reader.Dispose();
			}

			BundleFileEntry[] entries = new BundleFileEntry[metadata.Entries.Count];
			for(int i = 0; i < metadata.Entries.Count; i++)
			{
				BundleFileEntry bundleEntry = metadata.Entries[i];
				string name = bundleEntry.Name;
				long offset = bundleEntry.Offset - dataPosisition;
				long size = bundleEntry.Size;
				BundleFileEntry streamEntry = new BundleFileEntry(bufferStream, m_filePath, name, offset, size, true);
				entries[i] = streamEntry;
			}
			Metadata = new BundleMetadata(bufferStream, m_filePath, false, entries);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; private set; }

		private readonly string m_filePath;
	}
}