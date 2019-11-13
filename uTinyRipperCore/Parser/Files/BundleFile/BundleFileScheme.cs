using System;
using System.IO;
using uTinyRipper.BundleFiles;

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
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				Header.Read(reader);
			}

			using (BundleReader reader = new BundleReader(stream, EndianType.BigEndian, Header.Generation))
			{
				if (reader.Generation < BundleGeneration.BF_530_x)
				{
					ReadPre530Metadata(reader, out Stream dataStream, out long metadataOffset);
					ReadPre530Data(dataStream, metadataOffset);
				}
				else
				{
					long headerSize = stream.Position;
					Read530Metadata(reader, headerSize);
					Read530Data(stream, headerSize);
				}
			}
		}

		private void ReadPre530Metadata(BundleReader reader, out Stream dataStream, out long metadataOffset)
		{
			switch (Header.Type)
			{
				case BundleType.UnityRaw:
					{
						Metadata.Read(reader);
						dataStream = reader.BaseStream;
						metadataOffset = Header.HeaderSize;
					}
					break;

				case BundleType.UnityWeb:
					{
						// read only last chunk. wtf?
						ChunkInfo chunkInfo = Header.ChunkInfos[Header.ChunkInfos.Count - 1];
						MemoryStream stream = new MemoryStream(new byte[chunkInfo.DecompressedSize]);
						SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, chunkInfo.CompressedSize, stream);
						using (BundleReader decompressReader = new BundleReader(stream, reader.EndianType, reader.Generation))
						{
							Metadata.Read(decompressReader);
						}
						dataStream = stream;
						metadataOffset = 0;
					}
					break;

				default:
					throw new NotSupportedException($"Bundle type {Header.Type} isn't supported for pre530 generation");
			}
		}

		private void Read530Metadata(BundleReader reader, long headerSize)
		{
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				reader.BaseStream.Position = Header.BundleSize - Header.MetadataCompressedSize;
			}

			BundleCompressType metaCompression = Header.Flags.GetCompression();
			switch (metaCompression)
			{
				case BundleCompressType.None:
					{
						Metadata.Read(reader);
						long expectedPosition = Header.Flags.IsMetadataAtTheEnd() ? Header.BundleSize : headerSize + Header.MetadataDecompressedSize;
						if (reader.BaseStream.Position != expectedPosition)
						{
							throw new Exception($"Read {reader.BaseStream.Position - headerSize} but expected {Header.MetadataDecompressedSize}");
						}
					}
					break;

				case BundleCompressType.LZMA:
					{
						using (MemoryStream stream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							SevenZipHelper.DecompressLZMASizeStream(reader.BaseStream, Header.MetadataCompressedSize, stream);
							using (BundleReader decompressReader = new BundleReader(stream, reader.EndianType, reader.Generation))
							{
								Metadata.Read(decompressReader);
							}
							if (stream.Position != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {stream.Position} but expected {Header.MetadataDecompressedSize}");
							}
						}
					}
					break;

				case BundleCompressType.LZ4:
				case BundleCompressType.LZ4HZ:
					{
						using (MemoryStream stream = new MemoryStream(new byte[Header.MetadataDecompressedSize]))
						{
							using (Lz4DecodeStream decodeStream = new Lz4DecodeStream(reader.BaseStream, Header.MetadataCompressedSize))
							{
								decodeStream.ReadBuffer(stream, Header.MetadataDecompressedSize);
							}

							stream.Position = 0;
							using (BundleReader decompressReader = new BundleReader(stream, reader.EndianType, reader.Generation))
							{
								Metadata.Read(decompressReader);
							}
							if (stream.Position != Header.MetadataDecompressedSize)
							{
								throw new Exception($"Read {stream.Position} but expected {Header.MetadataDecompressedSize}");
							}
						}
					}
					break;

				default:
					throw new NotSupportedException($"Bundle compression '{metaCompression}' isn't supported");
			}
		}

		private void ReadPre530Data(Stream stream, long metadataOffset)
		{
			foreach (BundleFileEntry entry in Metadata.Entries)
			{
				byte[] buffer = new byte[entry.Size];
				stream.Position = metadataOffset + entry.Offset;
				stream.ReadBuffer(buffer, 0, buffer.Length);
				FileScheme scheme = GameCollection.ReadScheme(buffer, FilePath, entry.NameOrigin);
				AddScheme(scheme);
			}
		}

		private void Read530Data(Stream stream, long headerSize)
		{
			if (Header.Flags.IsMetadataAtTheEnd())
			{
				stream.Position = headerSize;
			}

			using (BundleFileBlockReader blockReader = new BundleFileBlockReader(stream, Metadata))
			{
				foreach (BundleFileEntry entry in Metadata.Entries)
				{
					SmartStream entryStream = blockReader.ReadEntry(entry);
					FileScheme scheme = GameCollection.ReadScheme(entryStream, FilePath, entry.NameOrigin);
					AddScheme(scheme);
				}
			}
		}

		public override FileEntryType SchemeType => FileEntryType.Bundle;

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; } = new BundleMetadata();
	}
}
