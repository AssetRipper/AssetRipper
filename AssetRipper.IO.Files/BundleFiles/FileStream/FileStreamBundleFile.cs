using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Extensions;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using K4os.Compression.LZ4;
using System.IO;

namespace AssetRipper.IO.Files.BundleFiles.FileStream
{
	public sealed class FileStreamBundleFile : FileContainer
	{
		public FileStreamBundleHeader Header { get; } = new();
		public BlocksInfo BlocksInfo { get; } = new();
		public DirectoryInfo<FileStreamNode> DirectoryInfo { get; } = new();

		public override void Read(SmartStream stream)
		{
			EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
			long basePosition = stream.Position;
			Header.Read(reader);
			long headerSize = stream.Position - basePosition;
			ReadFileStreamMetadata(stream, basePosition);//ReadBlocksInfoAndDirectory
			ReadFileStreamData(stream, basePosition, headerSize);//ReadBlocks and ReadFiles
		}

		public override void Write(Stream stream)
		{
			EndianWriter writer = new EndianWriter(stream, EndianType.BigEndian);
			long basePosition = stream.Position;
			Header.Write(writer);
			long headerSize = stream.Position - basePosition;
			WriteFileStreamMetadata(stream, basePosition);
			WriteFileStreamData(stream, basePosition, headerSize);
		}

		private void ReadFileStreamMetadata(Stream stream, long basePosition)
		{
			if (Header.Version >= BundleVersion.BF_LargeFilesSupport)
			{
				stream.Align(16);
			}
			if (Header.Flags.GetBlocksInfoAtTheEnd())
			{
				stream.Position = basePosition + (Header.Size - Header.CompressedBlocksInfoSize);
			}

			CompressionType metaCompression = Header.Flags.GetCompression();
			switch (metaCompression)
			{
				case CompressionType.None:
					{
						ReadMetadata(stream, Header.UncompressedBlocksInfoSize);
					}
					break;

				case CompressionType.Lzma:
					{
						using MemoryStream uncompressedStream = new MemoryStream(new byte[Header.UncompressedBlocksInfoSize]);
						SevenZipHelper.DecompressLZMAStream(stream, Header.CompressedBlocksInfoSize, uncompressedStream, Header.UncompressedBlocksInfoSize);

						uncompressedStream.Position = 0;
						ReadMetadata(uncompressedStream, Header.UncompressedBlocksInfoSize);
					}
					break;

				case CompressionType.Lz4:
				case CompressionType.Lz4HC:
					{
						int uncompressedSize = Header.UncompressedBlocksInfoSize;
						byte[] uncompressedBytes = new byte[uncompressedSize];
						byte[] compressedBytes = new BinaryReader(stream).ReadBytes(Header.CompressedBlocksInfoSize);
						int bytesWritten = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
						if (bytesWritten != uncompressedSize)
						{
							throw new Exception($"Incorrect number of bytes written. {bytesWritten} instead of {uncompressedSize} for {compressedBytes.Length} compressed bytes");
						}
						ReadMetadata(new MemoryStream(uncompressedBytes), uncompressedSize);
					}
					break;

				default:
					throw new NotSupportedException($"Bundle compression '{metaCompression}' isn't supported");
			}
		}

		private void ReadMetadata(Stream stream, int metadataSize)
		{
			long metadataPosition = stream.Position;
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				BlocksInfo.Read(reader);
				if (Header.Flags.GetBlocksAndDirectoryInfoCombined())
				{
					DirectoryInfo.Read(reader);
				}
			}
			if (metadataSize > 0)
			{
				if (stream.Position - metadataPosition != metadataSize)
				{
					throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize} while reading bundle metadata");
				}
			}
		}

		private void ReadFileStreamData(Stream stream, long basePosition, long headerSize)
		{
			if (Header.Flags.GetBlocksInfoAtTheEnd())
			{
				stream.Position = basePosition + headerSize;
				if (Header.Version >= BundleVersion.BF_LargeFilesSupport)
				{
					stream.Align(16);
				}
			}
			if (Header.Flags.GetBlockInfoNeedPaddingAtStart())
			{
				stream.Align(16);
			}

			using BundleFileBlockReader blockReader = new BundleFileBlockReader(stream, BlocksInfo);
			foreach (FileStreamNode entry in DirectoryInfo.Nodes)
			{
				SmartStream entryStream = blockReader.ReadEntry(entry);
				AddResourceFile(new ResourceFile(entryStream, FilePath, entry.Path));
			}
		}

		private void WriteFileStreamMetadata(Stream stream, long basePosition)
		{
			throw new NotImplementedException();
		}

		private void WriteFileStreamData(Stream stream, long basePosition, long headerSize)
		{
			throw new NotImplementedException();
		}
	}
}
