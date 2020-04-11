using System;
using System.IO;
using uTinyRipper.BundleFiles;
using uTinyRipper.Lz4;

namespace uTinyRipper
{
	public sealed class BundleFileScheme : FileSchemeList
	{
		private BundleFileScheme(string filePath, string fileName) :
			base(filePath, fileName)
		{
		}

		internal static BundleFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			BundleFileScheme scheme = new BundleFileScheme(filePath, fileName);
			using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, false))
			{
				scheme.ReadScheme(stream);
			}
			return scheme;
		}

		internal static BundleFileScheme ReadScheme(Stream stream, string filePath, string fileName)
		{
			BundleFileScheme scheme = new BundleFileScheme(filePath, fileName);
			scheme.ReadScheme(stream);
			return scheme;
		}

		internal BundleFile ReadFile(GameProcessorContext context)
		{
			BundleFile bundle = new BundleFile(this);
			foreach (FileScheme scheme in Schemes)
			{
				bundle.AddFile(context, scheme);
			}
			return bundle;
		}

		private void ReadScheme(Stream stream)
		{
			long basePosition = stream.Position;
			ReadHeader(stream);

			switch (Header.Signature)
			{
				case BundleType.UnityRaw:
				case BundleType.UnityWeb:
					ReadRawWebMetadata(stream, out Stream dataStream, out long metadataOffset);
					ReadRawWebData(dataStream, metadataOffset);
					break;

				case BundleType.UnityFS:
					long headerSize = stream.Position - basePosition;
					ReadFileStreamMetadata(stream, basePosition);
					ReadFileStreamData(stream, basePosition, headerSize);
					break;

				default:
					throw new Exception($"Unknown bundle signature '{Header.Signature}'");
			}
		}

		private void ReadHeader(Stream stream)
		{
			long headerPosition = stream.Position;
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				Header.Read(reader);
				if (Header.Signature.IsRawWeb())
				{
					if (stream.Position - headerPosition != Header.RawWeb.HeaderSize)
					{
						throw new Exception($"Read {stream.Position - headerPosition} but expected {Header.RawWeb.HeaderSize}");
					}
				}
			}
		}

		private void ReadRawWebMetadata(Stream stream, out Stream dataStream, out long metadataOffset)
		{
			BundleRawWebHeader header = Header.RawWeb;
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
			BundleFileStreamHeader header = Header.FileStream;
			if (header.Flags.IsBlocksInfoAtTheEnd())
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
						using (MemoryStream uncompressedStream = new MemoryStream(new byte[header.UncompressedBlocksInfoSize]))
						{
							SevenZipHelper.DecompressLZMAStream(stream, header.CompressedBlocksInfoSize, uncompressedStream, header.UncompressedBlocksInfoSize);

							uncompressedStream.Position = 0;
							ReadMetadata(uncompressedStream, header.UncompressedBlocksInfoSize);
						}
					}
					break;

				case CompressionType.Lz4:
				case CompressionType.Lz4HC:
					{
						using (MemoryStream uncompressedStream = new MemoryStream(new byte[header.UncompressedBlocksInfoSize]))
						{
							using (Lz4DecodeStream decodeStream = new Lz4DecodeStream(stream, header.CompressedBlocksInfoSize))
							{
								decodeStream.ReadBuffer(uncompressedStream, header.UncompressedBlocksInfoSize);
							}

							uncompressedStream.Position = 0;
							ReadMetadata(uncompressedStream, header.UncompressedBlocksInfoSize);
						}
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
					throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize}");
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
				FileScheme scheme = GameCollection.ReadScheme(buffer, FilePath, entry.PathOrigin);
				AddScheme(scheme);
			}
		}

		private void ReadFileStreamData(Stream stream, long basePosition, long headerSize)
		{
			if (Header.FileStream.Flags.IsBlocksInfoAtTheEnd())
			{
				stream.Position = basePosition + headerSize;
			}

			using (BundleFileBlockReader blockReader = new BundleFileBlockReader(stream, Metadata.BlocksInfo))
			{
				foreach (Node entry in Metadata.DirectoryInfo.Nodes)
				{
					SmartStream entryStream = blockReader.ReadEntry(entry);
					FileScheme scheme = GameCollection.ReadScheme(entryStream, FilePath, entry.PathOrigin);
					AddScheme(scheme);
				}
			}
		}

		public override FileEntryType SchemeType => FileEntryType.Bundle;

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; } = new BundleMetadata();
	}
}
