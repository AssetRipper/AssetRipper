using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Parser.Files.BundleFile.Header;
using AssetRipper.Core.Parser.Files.BundleFile.IO;
using AssetRipper.Core.Parser.Files.BundleFile.Parser;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Structure;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipper.IO.Endian;
using K4os.Compression.LZ4;
using System.IO;

namespace AssetRipper.Core.Parser.Files.BundleFile
{
	public sealed class BundleFileScheme : FileSchemeList
	{
		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; } = new BundleMetadata();
		public override FileEntryType SchemeType => FileEntryType.Bundle;

		private BundleFileScheme(string filePath, string fileName) : base(filePath, fileName) { }

		internal BundleFile ReadFile(GameProcessorContext context)
		{
			BundleFile bundle = new BundleFile(this);
			foreach (FileScheme scheme in Schemes)
			{
				bundle.AddFile(context, scheme);
			}
			return bundle;
		}

		internal static BundleFileScheme ReadScheme(byte[] buffer, string filePath, string fileName) => ReadScheme(new MemoryStream(buffer, 0, buffer.Length, false), filePath, fileName);
		internal static BundleFileScheme ReadScheme(Stream stream, string filePath, string fileName)
		{
			BundleFileScheme scheme = new BundleFileScheme(filePath, fileName);
			scheme.ReadScheme(stream);
			return scheme;
		}

		private void ReadScheme(Stream stream)
		{
			long basePosition = stream.Position;
			ReadHeader(stream);

			switch (Header.Signature)
			{
				case BundleType.UnityRaw:
				case BundleType.UnityWeb:
					ReadRawWebMetadata(stream, out Stream dataStream, out long metadataOffset);//ReadBlocksAndDirectory
					ReadRawWebData(dataStream, metadataOffset);//also ReadBlocksAndDirectory
					break;

				case BundleType.UnityFS:
					long headerSize = stream.Position - basePosition;
					ReadFileStreamMetadata(stream, basePosition);//ReadBlocksInfoAndDirectory
					ReadFileStreamData(stream, basePosition, headerSize);//ReadBlocks and ReadFiles
					break;

				default:
					throw new Exception($"Unknown bundle signature '{Header.Signature}'");
			}
		}

		private void ReadHeader(Stream stream)
		{
			long headerPosition = stream.Position;
			using EndianReader reader = new EndianReader(stream, EndianType.BigEndian);
			Header.Read(reader);
			if (Header.Signature.IsRawWeb())
			{
				if (stream.Position - headerPosition != Header.RawWeb!.HeaderSize)
				{
					throw new Exception($"Read {stream.Position - headerPosition} but expected {Header.RawWeb.HeaderSize} bytes while reading the raw/web bundle header.");
				}
			}
		}

		private void ReadRawWebMetadata(Stream stream, out Stream dataStream, out long metadataOffset)
		{
			BundleRawWebHeader header = Header.RawWeb!;
			int metadataSize = BundleRawWebHeader.HasUncompressedBlocksInfoSize(Header.Version) ? header.UncompressedBlocksInfoSize : 0;
			switch (Header.Signature)
			{
				case BundleType.UnityRaw:
					{
						dataStream = stream;
						metadataOffset = stream.Position;

						ReadMetadata(dataStream, metadataSize);
					}
					break;

				case BundleType.UnityWeb:
					{
						// read only last chunk
						BundleScene chunkInfo = header.Scenes[header.Scenes.Length - 1];
						dataStream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
						SevenZipHelper.DecompressLZMASizeStream(stream, chunkInfo.CompressedSize, dataStream);
						metadataOffset = 0;

						dataStream.Position = 0;
						ReadMetadata(dataStream, metadataSize);
					}
					break;

				default:
					throw new Exception($"Unsupported bundle signature '{Header.Signature}'");
			}
		}

		private void ReadFileStreamMetadata(Stream stream, long basePosition)
		{
			BundleFileStreamHeader header = Header.FileStream!;
			if (Header.Version >= BundleVersion.BF_LargeFilesSupport)
			{
				stream.Align(16);
			}
			if (header.Flags.GetBlocksInfoAtTheEnd())
			{
				stream.Position = basePosition + (header.Size - header.CompressedBlocksInfoSize);
			}

			CompressionType metaCompression = header.Flags.GetCompression();
			switch (metaCompression)
			{
				case CompressionType.None:
					{
						ReadMetadata(stream, header.UncompressedBlocksInfoSize);
					}
					break;

				case CompressionType.Lzma:
					{
						using MemoryStream uncompressedStream = new MemoryStream(new byte[header.UncompressedBlocksInfoSize]);
						SevenZipHelper.DecompressLZMAStream(stream, header.CompressedBlocksInfoSize, uncompressedStream, header.UncompressedBlocksInfoSize);

						uncompressedStream.Position = 0;
						ReadMetadata(uncompressedStream, header.UncompressedBlocksInfoSize);
					}
					break;

				case CompressionType.Lz4:
				case CompressionType.Lz4HC:
					{
						int uncompressedSize = header.UncompressedBlocksInfoSize;
						byte[] uncompressedBytes = new byte[uncompressedSize];
						byte[] compressedBytes = new BinaryReader(stream).ReadBytes(header.CompressedBlocksInfoSize);
						int bytesWritten = LZ4Codec.Decode(compressedBytes, uncompressedBytes);
						if (bytesWritten != uncompressedSize)
						{
							throw new System.Exception($"Incorrect number of bytes written. {bytesWritten} instead of {uncompressedSize} for {compressedBytes.Length} compressed bytes");
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
			using (BundleReader reader = new BundleReader(stream, EndianType.BigEndian, Header.Signature, Header.Version, Header.Flags))
			{
				Metadata.Read(reader);
			}
			if (metadataSize > 0)
			{
				if (stream.Position - metadataPosition != metadataSize)
				{
					throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize} while reading bundle metadata");
				}
			}
		}

		private void ReadRawWebData(Stream stream, long metadataOffset)
		{
			foreach (Node entry in Metadata.DirectoryInfo.Nodes)
			{
				byte[] buffer = new byte[entry.Size];
				stream.Position = metadataOffset + entry.Offset;
				stream.ReadBuffer(buffer, 0, buffer.Length);
				FileScheme scheme = SchemeReader.ReadScheme(buffer, FilePath, entry.PathOrigin);
				AddScheme(scheme);
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
			if (Header.Flags.GetAlignAfterBlocksInfo())
			{
				stream.Align(16);
			}

			using BundleFileBlockReader blockReader = new BundleFileBlockReader(stream, Metadata.BlocksInfo);
			foreach (Node entry in Metadata.DirectoryInfo.Nodes)
			{
				SmartStream entryStream = blockReader.ReadEntry(entry);
				FileScheme scheme = SchemeReader.ReadScheme(entryStream, FilePath, entry.PathOrigin);
				AddScheme(scheme);
			}
		}
	}
}
