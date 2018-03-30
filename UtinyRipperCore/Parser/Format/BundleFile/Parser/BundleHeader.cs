using System;
using System.Collections.Generic;

namespace UtinyRipper.BundleFiles
{
	internal sealed class BundleHeader
	{
		public static BundleType ParseSignature(string signature)
		{
			if (TryParseSignature(signature, out BundleType bundleType))
			{
				return bundleType;
			}
			throw new ArgumentException($"Unsupported signature '{signature}'");
		}

		public static bool TryParseSignature(string signatureString, out BundleType type)
		{
			switch (signatureString)
			{
				case nameof(BundleType.UnityWeb):
					type = BundleType.UnityWeb;
					return true;

				case nameof(BundleType.UnityRaw):
					type = BundleType.UnityRaw;
					return true;

				case nameof(BundleType.UnityFS):
					type = BundleType.UnityFS;
					return true;

				case HexFASignature:
					type = BundleType.HexFA;
					return true;

				default:
					type = default;
					return false;
			}
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadBundleSize(BundleGeneration generation)
		{
			return generation >= BundleGeneration.BF_260_340;
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadMetadataDecompressedSize(BundleGeneration generation)
		{
			return generation >= BundleGeneration.BF_350_4x;
		}

		public void Read(EndianStream stream)
		{
			string signature = stream.ReadStringZeroTerm();
			Type = ParseSignature(signature);

			Generation = (BundleGeneration)stream.ReadInt32();

			PlayerVersion = stream.ReadStringZeroTerm();
			string engineVersion = stream.ReadStringZeroTerm();
			EngineVersion.Parse(engineVersion);

			switch (Type)
			{
				case BundleType.UnityRaw:
				case BundleType.UnityWeb:
				case BundleType.HexFA:
					ReadRawWeb(stream);
					break;

				case BundleType.UnityFS:
					ReadFileStream(stream);
					break;

				default:
					throw new Exception($"Unknown bundle signature '{Type}'");
			}

		}

		private void ReadRawWeb(EndianStream stream)
		{
			if (Generation < BundleGeneration.BF_530_x)
			{
				ReadPre530Generation(stream);
			}
			else
			{
				Read530Generation(stream);
				stream.BaseStream.Position++;
			}
		}

		private void ReadFileStream(EndianStream stream)
		{
			if (Generation < BundleGeneration.BF_530_x)
			{
				throw new NotSupportedException("File stream supports only 530 and greater generations");
			}

			Read530Generation(stream);
		}

		private void ReadPre530Generation(EndianStream stream)
		{
			MinimumStreamedBytes = stream.ReadUInt32();
			HeaderSize = stream.ReadInt32();
			TotalChunkCount = stream.ReadInt32();
			m_chunkInfos = stream.ReadArray<ChunkInfo>();
			if (IsReadBundleSize(Generation))
			{
				BundleSize = stream.ReadUInt32();
			}
			if (IsReadMetadataDecompressedSize(Generation))
			{
				MetadataDecompressedSize = (int)stream.ReadUInt32();
			}
			stream.BaseStream.Position++;
		}

		private void Read530Generation(EndianStream stream)
		{
			BundleSize = stream.ReadInt64();
			MetadataCompressedSize = stream.ReadInt32();
			MetadataDecompressedSize = stream.ReadInt32();
			Flags = (BundleFlag)stream.ReadInt32();
		}

		/// <summary>
		/// Signature
		/// </summary>
		public BundleType Type { get; private set; }
		/// <summary>
		/// Stream version
		/// </summary>
		public BundleGeneration Generation { get; private set; }
		/// <summary>
		/// Engine version
		/// </summary>
		public string PlayerVersion { get; private set; }
		/// <summary>
		/// Minimum revision
		/// </summary>
		public Version EngineVersion { get; } = new Version();

		/// <summary>
		/// Minimum number of bytes to read for streamed bundles, equal to BundleSize for normal bundles
		/// </summary>
		public uint MinimumStreamedBytes { get; private set; }
		/// <summary>
		/// Equal to 1 if it's a streamed bundle, number of LZMAChunkInfos + mainData assets otherwise
		/// </summary>
		public int TotalChunkCount { get; private set; }
		/// <summary>
		/// LZMA chunks info
		/// </summary>
		public IReadOnlyList<ChunkInfo> ChunkInfos => m_chunkInfos;
		/// <summary>
		/// Size of the header
		/// </summary>
		public int HeaderSize { get; private set; }

		/// <summary>
		/// Equal to file size, sometimes equal to uncompressed data size without the header
		/// </summary>
		public long BundleSize { get; private set; }
		/// <summary>
		/// UnityFS length of the possibly-compressed (LZMA, LZ4) bundle data header
		/// </summary>
		public int MetadataCompressedSize { get; private set; }
		/// <summary>
		/// Decompressed size
		/// </summary>
		public int MetadataDecompressedSize { get; private set; }
		/// <summary>
		/// UnityFS flags
		/// </summary>
		public BundleFlag Flags { get; private set; }

		private const string HexFASignature = "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA";

		private ChunkInfo[] m_chunkInfos;
	}
}
