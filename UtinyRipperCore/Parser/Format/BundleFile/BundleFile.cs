using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtinyRipper.BundleFiles
{
	internal class BundleFile : IDisposable
	{
		public BundleFile(FileCollection fileCollection, string filePath, Action<string> requestDependencyCallback)
		{
			if(fileCollection == null)
			{
				throw new ArgumentNullException(nameof(fileCollection));
			}
			if(string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			if (requestDependencyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestDependencyCallback));
			}

			m_fileCollection = fileCollection;
			m_filePath = filePath;
			m_requestDependencyCallback = requestDependencyCallback;
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

		public static bool IsBundleFile(Stream baseStream)
		{
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				bool isBundle = false;
				long position = stream.BaseStream.Position;
				if(stream.BaseStream.Length - position > 0x20)
				{
					if (stream.ReadStringZeroTerm(0x20, out string signature))
					{
						isBundle = BundleHeader.TryParseSignature(signature, out BundleType _);
					}
				}
				stream.BaseStream.Position = position;

				return isBundle;
			}
		}

		public void Load(string bundlePath)
		{
			if(!FileUtils.Exists(bundlePath))
			{
				throw new Exception($"Bundle at path '{bundlePath}' doesn't exist");
			}

			FileStream stream = FileUtils.OpenRead(bundlePath);
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

		public void Read(Stream baseStream)
		{
			Read(baseStream, false);
		}

		public void Dispose()
		{
			Metadata.Dispose();
		}

		private void Read(Stream baseStream, bool isClosable)
		{
			m_loadedFiles.Clear();
			using (EndianStream stream = new EndianStream(baseStream, baseStream.Position, EndianType.BigEndian))
			{
				long position = stream.BaseStream.Position;
				Header.Read(stream);
				if (Header.Generation < BundleGeneration.BF_530_x)
				{
					ReadPre530Metadata(stream, isClosable);
				}
				else
				{
					Read530Metadata(stream, isClosable, position);
				}
			}

			foreach (BundleFileEntry entry in Metadata.ResourceEntries)
			{
				entry.ReadResourcesFile(m_fileCollection);
			}
			foreach (BundleFileEntry entry in Metadata.AssetsEntries)
			{
				if (m_loadedFiles.Add(entry.Name))
				{
					entry.ReadFile(m_fileCollection, OnRequestDependency);
				}
			}
		}

		private void ReadPre530Metadata(EndianStream stream, bool isClosable)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
				{
					if (Header.ChunkInfos.Count > 1)
					{
						throw new NotSupportedException($"Raw data with several chunks {Header.ChunkInfos.Count} isn't supported");
					}

					Metadata = new BundleMetadata(stream.BaseStream, m_filePath, isClosable);
					Metadata.ReadPre530(stream);
				}
				break;

				case BundleType.UnityWeb:
				case BundleType.HexFA:
				{
					// read only last chunk. wtf?
					ChunkInfo chunkInfo = Header.ChunkInfos[Header.ChunkInfos.Count - 1];
					MemoryStream memStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
					SevenZipHelper.DecompressLZMASizeStream(stream.BaseStream, chunkInfo.CompressedSize, memStream);

					Metadata = new BundleMetadata(memStream, m_filePath, true);
					using (EndianStream decompressStream = new EndianStream(memStream, EndianType.BigEndian))
					{
						Metadata.ReadPre530(decompressStream);
					}

					if (isClosable)
					{
						stream.Dispose();
					}					
				}
				break;

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported before 530 generation");
			}
		}

		private void Read530Metadata(EndianStream stream, bool isClosable, long basePosition)
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
					metadata = new BundleMetadata(stream.BaseStream, m_filePath, isClosable);
					metadata.Read530(stream, dataPosition);
					
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
							metadata = new BundleMetadata(stream.BaseStream, m_filePath, isClosable);
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
							metadata = new BundleMetadata(stream.BaseStream, m_filePath, isClosable);
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

			stream.BaseStream.Position = dataPosition;
			Read530Blocks(stream, isClosable, blockInfos, metadata);
		}
				
		private void Read530Blocks(EndianStream stream, bool isClosable, BlockInfo[] blockInfos, BundleMetadata metadata)
		{
			// Special case. If bundle has no compressed blocks then pass it as is
			if(blockInfos.All(t => t.Flags.GetCompression() == BundleCompressType.None))
			{
				Metadata = metadata;
				return;
			}

			long dataPosisition = stream.BaseStream.Position;
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
						stream.BaseStream.CopyStream(bufferStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZMA:
						SevenZipHelper.DecompressLZMAStream(stream.BaseStream, blockInfo.CompressedSize, bufferStream, blockInfo.DecompressedSize);
						break;

					case BundleCompressType.LZ4:
					case BundleCompressType.LZ4HZ:
						using (Lz4Stream lzStream = new Lz4Stream(stream.BaseStream, blockInfo.CompressedSize))
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
				stream.Dispose();
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

		private void OnRequestDependency(string dependency)
		{
			if (FilenameUtils.ContainsDependency(m_loadedFiles, dependency))
			{
				return;
			}
			foreach (BundleFileEntry entry in Metadata.AssetsEntries)
			{
				if (FilenameUtils.IsDependency(entry.Name, dependency))
				{
					entry.ReadFile(m_fileCollection, OnRequestDependency);
					m_loadedFiles.Add(entry.Name);
					return;
				}
			}

			m_requestDependencyCallback.Invoke(dependency);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; private set; }

		private readonly HashSet<string> m_loadedFiles = new HashSet<string>();

		private readonly FileCollection m_fileCollection;
		private readonly string m_filePath;
		private readonly Action<string> m_requestDependencyCallback;
	}
}