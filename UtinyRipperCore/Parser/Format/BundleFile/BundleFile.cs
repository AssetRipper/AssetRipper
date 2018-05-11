using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.SerializedFiles;

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

			m_fileCollection = fileCollection;
			m_filePath = filePath;
			m_requestDependencyCallback = requestDependencyCallback;
		}

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
			if(m_isDisposable)
			{
				foreach(BundleMetadata metadata in Metadatas)
				{
					metadata.Dispose();
				}
				m_isDisposable = false;
			}
		}

		private void Read(Stream baseStream, bool isClosable)
		{
			m_files.Clear();
			m_resources.Clear();

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

				foreach (BundleMetadata metadata in Metadatas)
				{
					foreach (BundleFileEntry entry in metadata.AssetsEntries)
					{
						if (!IsSerializedFileLoaded(entry.Name))
						{
							SerializedFile file = entry.ReadSerializedFile(m_fileCollection, OnRequestDependency);
							m_files.Add(file);
						}
					}
					foreach (BundleFileEntry entry in metadata.ResourceEntries)
					{
						ResourcesFile resesFile = entry.ReadResourcesFile(m_filePath);
						m_resources.Add(resesFile);
					}
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
					
					BundleMetadata metadata = new BundleMetadata(stream.BaseStream, m_filePath, isClosable);
					metadata.ReadPre530(stream);
					Metadatas = new BundleMetadata[] { metadata };
				}
				break;

				case BundleType.UnityWeb:
				case BundleType.HexFA:
				{
					BundleMetadata[] metadatas = new BundleMetadata[Header.ChunkInfos.Count];
					for(int i = 0; i < Header.ChunkInfos.Count; i++)
					{
						ChunkInfo chunkInfo = Header.ChunkInfos[i];
						MemoryStream memStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
						SevenZipHelper.DecompressLZMASizeStream(stream.BaseStream, chunkInfo.CompressedSize, memStream);
						
						BundleMetadata metadata = new BundleMetadata(memStream, m_filePath, true);
						using (EndianStream decompressStream = new EndianStream(memStream, EndianType.BigEndian))
						{
							metadata.ReadPre530(decompressStream);
						}
						metadatas[i] = metadata;
					}
					Metadatas = metadatas;
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
			// Special case. If bundle has no compressed blocks then pass it as a stream
			if(blockInfos.All(t => t.Flags.GetCompression() == BundleCompressType.None))
			{
				Metadatas = new BundleMetadata[] { metadata };
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
				BundleFileEntry streamEntry = new BundleFileEntry(memStream, m_filePath, name, offset, size);
				entries[i] = streamEntry;
			}
			BundleMetadata streamMetadata = new BundleMetadata(memStream, m_filePath, true, entries);
			Metadatas = new BundleMetadata[] { streamMetadata };
		}

		private bool IsSerializedFileLoaded(string name)
		{
			return SerializedFiles.Any(t => t.Name == name);
		}

		private void OnRequestDependency(string dependency)
		{
			if(IsSerializedFileLoaded(dependency))
			{
				return;
			}

			foreach (BundleMetadata metadata in Metadatas)
			{
				foreach (BundleFileEntry entry in metadata.AssetsEntries)
				{
					if(entry.Name == dependency)
					{
						SerializedFile file = entry.ReadSerializedFile(m_fileCollection, OnRequestDependency);
						m_files.Add(file);
						return;
					}
				}
			}

			m_requestDependencyCallback?.Invoke(dependency);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public IReadOnlyList<BundleMetadata> Metadatas
		{
			get => m_metadatas;
			private set
			{
				m_metadatas = value;
				m_isDisposable = m_metadatas != null;
			}
		}
		public IReadOnlyList<SerializedFile> SerializedFiles => m_files;
		public IReadOnlyList<ResourcesFile> ResourceFiles => m_resources;

		private readonly List<SerializedFile> m_files = new List<SerializedFile>();
		private readonly List<ResourcesFile> m_resources = new List<ResourcesFile>();

		private readonly FileCollection m_fileCollection;
		private readonly string m_filePath;
		private readonly Action<string> m_requestDependencyCallback;

		private bool m_isDisposable = false;
		private IReadOnlyList<BundleMetadata> m_metadatas;
	}
}