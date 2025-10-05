using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.Exceptions;
using AssetRipper.IO.Files.Extensions;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Streams.Smart;
using K4os.Compression.LZ4;

namespace AssetRipper.IO.Files.BundleFiles.FileStream;

public sealed class FileStreamBundleFile : FileContainer
{
	public FileStreamBundleHeader Header { get; } = new();
	public BlocksInfo BlocksInfo { get; private set; } = new();
	public DirectoryInfo<FileStreamNode> DirectoryInfo { get; set; } = new();

	public FileStreamBundleFile()
	{
	}

	public FileStreamBundleFile(string filePath, FileSystem fileSystem)
	{
		SmartStream stream = SmartStream.OpenRead(filePath, fileSystem);
		Read(stream);
	}

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

		DecompressionFailedException.ThrowIfUncompressedSizeIsNegative(NameFixed, Header.UncompressedBlocksInfoSize);

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
					LzmaCompression.DecompressLzmaStream(stream, Header.CompressedBlocksInfoSize, uncompressedStream, Header.UncompressedBlocksInfoSize);

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
					if (bytesWritten < 0)
					{
						DecompressionFailedException.ThrowNoBytesWritten(NameFixed, metaCompression);
					}
					else if (bytesWritten != uncompressedSize)
					{
						DecompressionFailedException.ThrowIncorrectNumberBytesWritten(NameFixed, metaCompression, uncompressedSize, bytesWritten);
					}
					ReadMetadata(new MemoryStream(uncompressedBytes), uncompressedSize);
				}
				break;

			case CompressionType.Lzham:
				UnsupportedBundleDecompression.ThrowLzham(NameFixed);
				break;

			default:
				UnsupportedBundleDecompression.Throw(NameFixed, metaCompression);
				break;
		}
	}

	private void ReadMetadata(Stream stream, int metadataSize)
	{
		long metadataPosition = stream.Position;
		using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
		{
			BlocksInfo = BlocksInfo.Read(reader);
			if (Header.Flags.GetBlocksAndDirectoryInfoCombined())
			{
				DirectoryInfo = DirectoryInfo<FileStreamNode>.Read(reader);
			}
		}
		if (metadataSize > 0)
		{
			long metadataBytesRead = stream.Position - metadataPosition;
			if (metadataBytesRead == metadataSize)
			{
				// Note: metadataSize might not be a multiple of 4
			}
			else if ((metadataPosition % 4 == 0) && (metadataSize % 4 == 0) && (((metadataBytesRead + 3) & ~3) == metadataSize))
			{
				// The stream needed to be aligned to 4 bytes.
				// https://github.com/AssetRipper/AssetRipper/issues/1470
				// In my testing, there didn't seem to be any indicator (eg in Header.Flags) that the stream needed to be aligned to 4 bytes.
				// We just need to check if aligning the stream to 4 bytes would make the bytes read equal to the metadata size.
			}
			else
			{
				throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize} while reading bundle metadata");
			}
		}
	}

	private void ReadFileStreamData(SmartStream stream, long basePosition, long headerSize)
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
			try
			{
				SmartStream entryStream = blockReader.ReadEntry(entry);
				AddResourceFile(new ResourceFile(entryStream, FilePath, entry.Path));
			}
			catch (Exception ex)
			{
				AddFailedFile(new FailedFile()
				{
					Name = entry.Path,
					FilePath = FilePath,
					StackTrace = ex.ToString(),
				});
			}
		}
	}

	private void WriteFileStreamMetadata(Stream stream, long basePosition)
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
					WriteMetadata(stream, Header.UncompressedBlocksInfoSize);
				}
				break;

			case CompressionType.Lzma:
				throw new NotImplementedException(nameof(CompressionType.Lzma));

			//These cases will likely need to be separated.
			case CompressionType.Lz4:
			case CompressionType.Lz4HC:
				{
					//These should be set after doing this calculation instead of before
					int uncompressedSize = Header.UncompressedBlocksInfoSize;
					int compressedSize = Header.CompressedBlocksInfoSize;

					byte[] uncompressedBytes = new byte[uncompressedSize];
					WriteMetadata(new MemoryStream(uncompressedBytes), uncompressedSize);
					byte[] compressedBytes = new byte[compressedSize];
					int bytesWritten = LZ4Codec.Encode(uncompressedBytes, compressedBytes, LZ4Level.L00_FAST);
					if (bytesWritten != compressedSize)
					{
						throw new Exception($"Incorrect number of bytes written. {bytesWritten} instead of {compressedSize} for {compressedBytes.Length} compressed bytes");
					}
					new BinaryWriter(stream).Write(compressedBytes);
				}
				break;

			default:
				throw new NotSupportedException($"Bundle compression '{metaCompression}' isn't supported");
		}
	}

	private void WriteMetadata(Stream stream, int metadataSize)
	{
		long metadataPosition = stream.Position;
		using (EndianWriter writer = new EndianWriter(stream, EndianType.BigEndian))
		{
			BlocksInfo.Write(writer);
			if (Header.Flags.GetBlocksAndDirectoryInfoCombined())
			{
				DirectoryInfo.Write(writer);
			}
		}
		if (metadataSize > 0)
		{
			if (stream.Position - metadataPosition != metadataSize)
			{
				throw new Exception($"Wrote {stream.Position - metadataPosition} but expected {metadataSize} while writing bundle metadata");
			}
		}
	}

	private void WriteFileStreamData(Stream stream, long basePosition, long headerSize)
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

		if (DirectoryInfo.Nodes.Length == 0)
		{
			return;
		}

		throw new NotImplementedException();
	}
}
