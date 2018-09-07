using System;
using System.IO;
using System.Linq;

namespace UtinyRipper.BundleFiles
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
						if (Header.ChunkInfos.Count > 1)
						{
							throw new NotSupportedException($"Raw data with several chunks {Header.ChunkInfos.Count} isn't supported");
						}

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
						Metadata.Read530(reader, bundleStream, dataPosition);
					
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
								Metadata.Read530(metaReader, bundleStream, dataPosition);
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
								Metadata.Read530(metaReader, bundleStream, dataPosition);
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
			Read530Blocks(reader, blockInfos);
		}
				
		private void Read530Blocks(EndianReader reader, BlockInfo[] blockInfos)
		{
			// Special case. If bundle has no compressed blocks then pass it as is
			if(blockInfos.All(t => t.Flags.GetCompression() == BundleCompressType.None))
			{
				return;
			}

			SmartStream bundleStream = (SmartStream)reader.BaseStream;
			long dataOffset = bundleStream.Position;
			long decompressedSize = blockInfos.Sum(t => t.DecompressedSize);
			using (SmartStream blockStream = CreateBlockStream(decompressedSize))
			{
				foreach (BlockInfo blockInfo in blockInfos)
				{
					BundleCompressType compressType = blockInfo.Flags.GetCompression();
					switch (compressType)
					{
						case BundleCompressType.None:
							bundleStream.CopyStream(blockStream, blockInfo.DecompressedSize);
							break;

						case BundleCompressType.LZMA:
							SevenZipHelper.DecompressLZMAStream(bundleStream, blockInfo.CompressedSize, blockStream, blockInfo.DecompressedSize);
							break;

						case BundleCompressType.LZ4:
						case BundleCompressType.LZ4HZ:
							using (Lz4Stream lzStream = new Lz4Stream(bundleStream, blockInfo.CompressedSize))
							{
								long read = lzStream.Read(blockStream, blockInfo.DecompressedSize);
								if (read != blockInfo.DecompressedSize)
								{
									throw new Exception($"Read {read} but expected {blockInfo.CompressedSize}");
								}
							}
							break;

						default:
							throw new NotImplementedException($"Bundle compression '{compressType}' isn't supported");
					}
				}

				BundleFileEntry[] entries = new BundleFileEntry[Metadata.Entries.Count];
				for (int i = 0; i < Metadata.Entries.Count; i++)
				{
					BundleFileEntry bundleEntry = Metadata.Entries[i];
					string name = bundleEntry.Name;
					long offset = bundleEntry.Offset - dataOffset;
					long size = bundleEntry.Size;
					BundleFileEntry streamEntry = new BundleFileEntry(blockStream, m_filePath, name, offset, size);
					entries[i] = streamEntry;
				}
				Metadata.Dispose();
				Metadata = new BundleMetadata(m_filePath, entries);
			}
		}

		private SmartStream CreateBlockStream(long decompressedSize)
		{
			return decompressedSize > int.MaxValue ? SmartStream.CreateTemp() : SmartStream.CreateMemory(new byte[decompressedSize]);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; private set; }

		private readonly string m_filePath;
	}
}