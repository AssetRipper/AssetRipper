using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.FileAnalyzer.IO;
using AssetRipper.IO.Endian;
using K4os.Compression.LZ4;
using System.IO;
using System.Linq;

namespace AssetRipper.FileAnalyzer
{
	public class BundleFile
	{
		public sealed class Header
		{
			public string signature;
			public uint version;
			public string unityVersion;
			public string unityRevision;
			public long size;
			public uint compressedBlocksInfoSize;
			public uint uncompressedBlocksInfoSize;
			public uint flags;
		}

		public sealed class StorageBlock
		{
			public uint compressedSize;
			public uint uncompressedSize;
			public ushort flags;
		}

		public sealed class Node
		{
			public long offset;
			public long size;
			public uint flags;
			public string path;
		}

		public Header m_Header;
		private StorageBlock[] m_BlocksInfo;
		private Node[] m_DirectoryInfo;

		public StreamFile[] fileList;

		public BundleFile(FileReader reader)
		{
			m_Header = new Header();
			m_Header.signature = reader.ReadStringToNull();
			m_Header.version = reader.ReadUInt32();
			m_Header.unityVersion = reader.ReadStringToNull();
			m_Header.unityRevision = reader.ReadStringToNull();
			switch (m_Header.signature)
			{
				case "UnityArchive":
					break; //TODO
				case "UnityWeb":
				case "UnityRaw":
					if (m_Header.version == 6)
					{
						goto case "UnityFS";
					}
					ReadHeaderAndBlocksInfo(reader);
					using (Stream blocksStream = CreateBlocksStream(reader.FullPath))
					{
						ReadBlocksAndDirectory(reader, blocksStream);
						ReadFiles(blocksStream, reader.FullPath);
					}
					break;
				case "UnityFS":
					ReadHeader(reader);
					ReadBlocksInfoAndDirectory(reader);
					using (Stream blocksStream = CreateBlocksStream(reader.FullPath))
					{
						ReadBlocks(reader, blocksStream);
						ReadFiles(blocksStream, reader.FullPath);
					}
					break;
			}
		}

		private void ReadHeaderAndBlocksInfo(EndianReader reader)
		{
			bool isCompressed = m_Header.signature == "UnityWeb";
			if (m_Header.version >= 4)
			{
				byte[] hash = reader.ReadBytes(16);
				uint crc = reader.ReadUInt32();
			}
			uint minimumStreamedBytes = reader.ReadUInt32();
			m_Header.size = reader.ReadUInt32();
			uint numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
			int levelCount = reader.ReadInt32();
			m_BlocksInfo = new StorageBlock[1];
			for (int i = 0; i < levelCount; i++)
			{
				StorageBlock storageBlock = new StorageBlock()
				{
					compressedSize = reader.ReadUInt32(),
					uncompressedSize = reader.ReadUInt32(),
					flags = (ushort)(isCompressed ? 1 : 0)
				};
				if (i == levelCount - 1)
				{
					m_BlocksInfo[0] = storageBlock;
				}
			}
			if (m_Header.version >= 2)
			{
				uint completeFileSize = reader.ReadUInt32();
			}
			if (m_Header.version >= 3)
			{
				uint fileInfoHeaderSize = reader.ReadUInt32();
			}
			reader.BaseStream.Position = m_Header.size;
		}

		private Stream CreateBlocksStream(string path)
		{
			Stream blocksStream;
			long uncompressedSizeSum = m_BlocksInfo.Sum(x => x.uncompressedSize);
			if (uncompressedSizeSum >= int.MaxValue)
			{
				/*var memoryMappedFile = MemoryMappedFile.CreateNew(null, uncompressedSizeSum);
				assetsDataStream = memoryMappedFile.CreateViewStream();*/
				blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
			}
			else
			{
				blocksStream = new MemoryStream((int)uncompressedSizeSum);
			}
			return blocksStream;
		}

		private void ReadBlocksAndDirectory(EndianReader reader, Stream blocksStream)
		{
			foreach (StorageBlock blockInfo in m_BlocksInfo)
			{
				byte[] compressedBytes = reader.ReadBytes((int)blockInfo.compressedSize);
				if (blockInfo.flags == 1)//LZMA
				{
					using MemoryStream memoryStream = new MemoryStream(compressedBytes);
					SevenZipHelper.DecompressLZMASizeStream(memoryStream, compressedBytes.Length, blocksStream);
				}
				else
				{
					blocksStream.Write(compressedBytes, 0, compressedBytes.Length);
				}
			}
			blocksStream.Position = 0;
			EndianReader blocksReader = new EndianReader(blocksStream, EndianType.BigEndian);
			int nodesCount = blocksReader.ReadInt32();
			m_DirectoryInfo = new Node[nodesCount];
			for (int i = 0; i < nodesCount; i++)
			{
				m_DirectoryInfo[i] = new Node
				{
					path = blocksReader.ReadStringToNull(),
					offset = blocksReader.ReadUInt32(),
					size = blocksReader.ReadUInt32()
				};
			}
		}

		public void ReadFiles(Stream blocksStream, string path)
		{
			fileList = new StreamFile[m_DirectoryInfo.Length];
			for (int i = 0; i < m_DirectoryInfo.Length; i++)
			{
				Node node = m_DirectoryInfo[i];
				StreamFile file = new StreamFile();
				fileList[i] = file;
				file.path = node.path;
				file.fileName = Path.GetFileName(node.path);
				if (node.size >= int.MaxValue)
				{
					/*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
					file.stream = memoryMappedFile.CreateViewStream();*/
					string extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
					Directory.CreateDirectory(extractPath);
					file.stream = new FileStream(extractPath + file.fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				}
				else
				{
					file.stream = new MemoryStream((int)node.size);
				}
				blocksStream.Position = node.offset;
				blocksStream.CopyTo(file.stream, node.size);
				file.stream.Position = 0;
			}
		}

		private void ReadHeader(EndianReader reader)
		{
			m_Header.size = reader.ReadInt64();
			m_Header.compressedBlocksInfoSize = reader.ReadUInt32();
			m_Header.uncompressedBlocksInfoSize = reader.ReadUInt32();
			m_Header.flags = reader.ReadUInt32();
			if (m_Header.signature != "UnityFS")
			{
				reader.ReadByte();
			}
		}

		private void ReadBlocksInfoAndDirectory(EndianReader reader)
		{
			byte[] blocksInfoBytes;
			if (m_Header.version >= 7)
			{
				reader.AlignStream(16);
			}
			if ((m_Header.flags & 0x80) != 0) //kArchiveBlocksInfoAtTheEnd
			{
				long position = reader.BaseStream.Position;
				reader.BaseStream.Position = reader.BaseStream.Length - m_Header.compressedBlocksInfoSize;
				blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
				reader.BaseStream.Position = position;
			}
			else //0x40 kArchiveBlocksAndDirectoryInfoCombined
			{
				blocksInfoBytes = reader.ReadBytes((int)m_Header.compressedBlocksInfoSize);
			}
			MemoryStream blocksInfoUncompresseddStream;
			switch (m_Header.flags & 0x3F) //kArchiveCompressionTypeMask
			{
				default: //None
					{
						blocksInfoUncompresseddStream = new MemoryStream(blocksInfoBytes);
						break;
					}
				case 1: //LZMA
					{
						MemoryStream blocksInfoCompressedStream = new MemoryStream(blocksInfoBytes);
						blocksInfoUncompresseddStream = new MemoryStream((int)m_Header.uncompressedBlocksInfoSize);
						SevenZipHelper.DecompressLZMAStream(blocksInfoCompressedStream, m_Header.compressedBlocksInfoSize, blocksInfoUncompresseddStream, m_Header.uncompressedBlocksInfoSize);
						blocksInfoUncompresseddStream.Position = 0;
						blocksInfoCompressedStream.Close();
						break;
					}
				case 2: //LZ4
				case 3: //LZ4HC
					{
						uint uncompressedSize = m_Header.uncompressedBlocksInfoSize;
						byte[] uncompressedBytes = new byte[uncompressedSize];
						int bytesWritten = LZ4Codec.Decode(blocksInfoBytes, uncompressedBytes);
						if (bytesWritten != uncompressedSize)
						{
							throw new System.Exception($"Incorrect number of bytes written. {bytesWritten} instead of {uncompressedSize}");
						}
						blocksInfoUncompresseddStream = new MemoryStream(uncompressedBytes);
						break;
					}
			}
			using EndianReader blocksInfoReader = new EndianReader(blocksInfoUncompresseddStream, EndianType.BigEndian);
			byte[] uncompressedDataHash = blocksInfoReader.ReadBytes(16);
			int blocksInfoCount = blocksInfoReader.ReadInt32();
			m_BlocksInfo = new StorageBlock[blocksInfoCount];
			for (int i = 0; i < blocksInfoCount; i++)
			{
				m_BlocksInfo[i] = new StorageBlock
				{
					uncompressedSize = blocksInfoReader.ReadUInt32(),
					compressedSize = blocksInfoReader.ReadUInt32(),
					flags = blocksInfoReader.ReadUInt16()
				};
			}

			int nodesCount = blocksInfoReader.ReadInt32();
			m_DirectoryInfo = new Node[nodesCount];
			for (int i = 0; i < nodesCount; i++)
			{
				m_DirectoryInfo[i] = new Node
				{
					offset = blocksInfoReader.ReadInt64(),
					size = blocksInfoReader.ReadInt64(),
					flags = blocksInfoReader.ReadUInt32(),
					path = blocksInfoReader.ReadStringToNull(),
				};
			}
		}

		private void ReadBlocks(EndianReader reader, Stream blocksStream)
		{
			foreach (StorageBlock blockInfo in m_BlocksInfo)
			{
				switch (blockInfo.flags & 0x3F) //kStorageBlockCompressionTypeMask
				{
					default: //None
						{
							reader.BaseStream.CopyTo(blocksStream, blockInfo.compressedSize);
							break;
						}
					case 1: //LZMA
						{
							SevenZipHelper.DecompressLZMAStream(reader.BaseStream, blockInfo.compressedSize, blocksStream, blockInfo.uncompressedSize);
							break;
						}
					case 2: //LZ4
					case 3: //LZ4HC
						{
							byte[] compressedBytes = reader.ReadBytes((int)blockInfo.compressedSize);
							uint uncompressedSize = blockInfo.uncompressedSize;
							byte[] uncompressedBytes = new byte[uncompressedSize];
							int bytesWritten = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
							compressedBytes = null;
							if (bytesWritten != uncompressedSize)
							{
								throw new System.Exception($"Incorrect number of bytes written. {bytesWritten} instead of {uncompressedSize}");
							}
							blocksStream.Write(uncompressedBytes);
							break;
						}
				}
			}
			blocksStream.Position = 0;
		}
	}
}
